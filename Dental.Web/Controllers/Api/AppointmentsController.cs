using Dental.Web.Data;
using Dental.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dental.Web.Controllers.Api;

[ApiController]
[Route("api/appointments")]
public class AppointmentsController(ApplicationDbContext db, ILogger<AppointmentsController> log) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Appointment>>> List(
        [FromQuery] Guid? patientId,
        [FromQuery] Guid? resourceId,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] AppointmentStatus? status,
        CancellationToken ct)
    {
        var q = db.Appointments.AsNoTracking()
            .Include(a => a.PrimaryResource)
            .Include(a => a.AdditionalResources).ThenInclude(l => l.Resource)
            .AsQueryable();

        if (patientId.HasValue) q = q.Where(a => a.PatientId == patientId);
        if (resourceId.HasValue) q = q.Where(a =>
            a.PrimaryResourceId == resourceId ||
            a.AdditionalResources.Any(r => r.ResourceId == resourceId));
        if (from.HasValue) q = q.Where(a => a.EndAt >= from);
        if (to.HasValue) q = q.Where(a => a.StartAt <= to);
        if (status.HasValue) q = q.Where(a => a.Status == status);

        return Ok(await q.OrderBy(a => a.StartAt).ToListAsync(ct));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Appointment>> Get(Guid id, CancellationToken ct)
    {
        var appt = await db.Appointments.AsNoTracking()
            .Include(a => a.PrimaryResource)
            .Include(a => a.AdditionalResources).ThenInclude(l => l.Resource)
            .FirstOrDefaultAsync(a => a.Id == id, ct);
        return appt is null ? NotFound() : Ok(appt);
    }

    [HttpPost]
    public async Task<ActionResult<Appointment>> Create([FromBody] CreateAppointmentRequest request, CancellationToken ct)
    {
        if (await HasConflict(request.PrimaryResourceId, request.AdditionalResourceIds, request.StartAt, request.EndAt, null, ct))
        {
            return Conflict("Resource is not available for the selected time slot.");
        }

        var appt = new Appointment
        {
            Id = Guid.NewGuid(),
            PatientId = request.PatientId,
            PrimaryResourceId = request.PrimaryResourceId,
            StartAt = request.StartAt,
            EndAt = request.EndAt,
            Notes = request.Notes,
            Status = AppointmentStatus.Scheduled,
            IsOnlineBooking = request.IsOnlineBooking,
            RecurringTemplateId = request.RecurringTemplateId,
        };

        db.Appointments.Add(appt);
        foreach (var rid in request.AdditionalResourceIds ?? [])
        {
            db.AppointmentResourceLinks.Add(new AppointmentResourceLink { AppointmentId = appt.Id, ResourceId = rid });
        }

        if (request.ScheduleSmsReminder)
        {
            var patient = await db.Patients.AsNoTracking().FirstAsync(p => p.Id == request.PatientId, ct);
            if (!string.IsNullOrWhiteSpace(patient.Phone))
            {
                db.SmsReminderLogs.Add(new SmsReminderLog
                {
                    Id = Guid.NewGuid(),
                    AppointmentId = appt.Id,
                    PhoneNumber = patient.Phone,
                    Message = $"Randevu hatırlatması: {request.StartAt.LocalDateTime:dd.MM.yyyy HH:mm}",
                    ScheduledFor = request.StartAt.AddHours(-24),
                    Status = SmsReminderStatus.Pending,
                });
            }
        }

        await db.SaveChangesAsync(ct);
        log.LogInformation("Created appointment {Id}", appt.Id);
        return CreatedAtAction(nameof(Get), new { id = appt.Id }, appt);
    }

    [HttpPost("{id:guid}/confirm")]
    public async Task<IActionResult> Confirm(Guid id, CancellationToken ct)
    {
        var appt = await db.Appointments.FindAsync([id], ct);
        if (appt is null) return NotFound();
        appt.Status = AppointmentStatus.Confirmed;
        appt.ConfirmedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelAppointmentRequest? body, CancellationToken ct)
    {
        var appt = await db.Appointments.FindAsync([id], ct);
        if (appt is null) return NotFound();
        appt.Status = AppointmentStatus.Cancelled;
        appt.CancelledAt = DateTimeOffset.UtcNow;
        appt.CancellationReason = body?.Reason;

        var reminders = await db.SmsReminderLogs.Where(s => s.AppointmentId == id && s.Status == SmsReminderStatus.Pending).ToListAsync(ct);
        foreach (var r in reminders) r.Status = SmsReminderStatus.Cancelled;

        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/reschedule")]
    public async Task<IActionResult> Reschedule(Guid id, [FromBody] RescheduleAppointmentRequest request, CancellationToken ct)
    {
        var appt = await db.Appointments.FindAsync([id], ct);
        if (appt is null) return NotFound();

        if (await HasConflict(appt.PrimaryResourceId, null, request.StartAt, request.EndAt, id, ct))
        {
            return Conflict("Resource is not available for the selected time slot.");
        }

        appt.StartAt = request.StartAt;
        appt.EndAt = request.EndAt;
        appt.Status = AppointmentStatus.Rescheduled;
        appt.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpGet("analytics/density")]
    public async Task<ActionResult<IEnumerable<AppointmentDensityDto>>> Density(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        [FromQuery] Guid? resourceId,
        CancellationToken ct)
    {
        var start = new DateTimeOffset(from.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var end = new DateTimeOffset(to.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);

        var q = db.Appointments.AsNoTracking()
            .Where(a => a.StartAt >= start && a.StartAt <= end &&
                        a.Status != AppointmentStatus.Cancelled);

        if (resourceId.HasValue) q = q.Where(a => a.PrimaryResourceId == resourceId);

        var grouped = await q
            .GroupBy(a => DateOnly.FromDateTime(a.StartAt.Date))
            .Select(g => new AppointmentDensityDto(g.Key, g.Count()))
            .OrderBy(x => x.Date)
            .ToListAsync(ct);

        return Ok(grouped);
    }

    private async Task<bool> HasConflict(
        Guid primaryResourceId,
        IEnumerable<Guid>? additionalIds,
        DateTimeOffset start,
        DateTimeOffset end,
        Guid? excludeAppointmentId,
        CancellationToken ct)
    {
        var resourceIds = new List<Guid> { primaryResourceId };
        if (additionalIds != null) resourceIds.AddRange(additionalIds);

        var q = db.Appointments.Where(a =>
            a.Status != AppointmentStatus.Cancelled &&
            a.StartAt < end && a.EndAt > start &&
            (resourceIds.Contains(a.PrimaryResourceId) ||
             a.AdditionalResources.Any(l => resourceIds.Contains(l.ResourceId))));

        if (excludeAppointmentId.HasValue) q = q.Where(a => a.Id != excludeAppointmentId);

        return await q.AnyAsync(ct);
    }
}

[ApiController]
[Route("api/appointment-resources")]
public class AppointmentResourcesController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AppointmentResource>>> List(
        CancellationToken ct,
        [FromQuery] AppointmentResourceType? type,
        [FromQuery] bool activeOnly = true)
    {
        var q = db.AppointmentResources.AsNoTracking().AsQueryable();
        if (type.HasValue) q = q.Where(r => r.ResourceType == type);
        if (activeOnly) q = q.Where(r => r.IsActive);
        return Ok(await q.OrderBy(r => r.Name).ToListAsync(ct));
    }

    [HttpPost]
    public async Task<ActionResult<AppointmentResource>> Create([FromBody] AppointmentResource input, CancellationToken ct)
    {
        input.Id = Guid.NewGuid();
        db.AppointmentResources.Add(input);
        await db.SaveChangesAsync(ct);
        return Ok(input);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] AppointmentResource input, CancellationToken ct)
    {
        var entity = await db.AppointmentResources.FindAsync([id], ct);
        if (entity is null) return NotFound();
        entity.Name = input.Name;
        entity.ResourceType = input.ResourceType;
        entity.Description = input.Description;
        entity.DefaultDurationMinutes = input.DefaultDurationMinutes;
        entity.Color = input.Color;
        entity.IsActive = input.IsActive;
        entity.UserId = input.UserId;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }
}

[ApiController]
[Route("api/waitlist")]
public class WaitlistController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<WaitlistEntry>>> List([FromQuery] WaitlistStatus? status, CancellationToken ct)
    {
        var q = db.WaitlistEntries.AsNoTracking().Include(w => w.Patient).AsQueryable();
        if (status.HasValue) q = q.Where(w => w.Status == status);
        return Ok(await q.OrderBy(w => w.Priority).ThenBy(w => w.CreatedAt).ToListAsync(ct));
    }

    [HttpPost]
    public async Task<ActionResult<WaitlistEntry>> Create([FromBody] WaitlistEntry input, CancellationToken ct)
    {
        input.Id = Guid.NewGuid();
        db.WaitlistEntries.Add(input);
        await db.SaveChangesAsync(ct);
        return Ok(input);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] WaitlistEntry input, CancellationToken ct)
    {
        var entity = await db.WaitlistEntries.FindAsync([id], ct);
        if (entity is null) return NotFound();
        entity.Status = input.Status;
        entity.Priority = input.Priority;
        entity.Notes = input.Notes;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }
}

[ApiController]
[Route("api/recurring-appointments")]
public class RecurringAppointmentsController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RecurringAppointmentTemplate>>> List(
        [FromQuery] Guid? patientId,
        CancellationToken ct)
    {
        var q = db.RecurringAppointmentTemplates.AsNoTracking().AsQueryable();
        if (patientId.HasValue) q = q.Where(t => t.PatientId == patientId);
        return Ok(await q.Where(t => t.IsActive).ToListAsync(ct));
    }

    [HttpPost]
    public async Task<ActionResult<RecurringAppointmentTemplate>> Create(
        [FromBody] RecurringAppointmentTemplate input,
        CancellationToken ct)
    {
        input.Id = Guid.NewGuid();
        db.RecurringAppointmentTemplates.Add(input);
        await db.SaveChangesAsync(ct);
        return Ok(input);
    }

    [HttpPost("{id:guid}/generate")]
    public async Task<ActionResult<int>> GenerateOccurrences(Guid id, [FromQuery] int count = 4, CancellationToken ct = default)
    {
        var template = await db.RecurringAppointmentTemplates.FindAsync([id], ct);
        if (template is null || !template.IsActive) return NotFound();

        var created = 0;
        var date = template.StartDate;
        for (var i = 0; i < count; i++)
        {
            if (template.EndDate.HasValue && date > template.EndDate.Value) break;

            var start = new DateTimeOffset(date.ToDateTime(template.StartTime), TimeSpan.Zero);
            var end = start.AddMinutes(template.DurationMinutes);

            var appt = new Appointment
            {
                Id = Guid.NewGuid(),
                PatientId = template.PatientId,
                PrimaryResourceId = template.PrimaryResourceId,
                StartAt = start,
                EndAt = end,
                Status = AppointmentStatus.Scheduled,
                RecurringTemplateId = template.Id,
                Notes = template.Notes,
            };
            db.Appointments.Add(appt);
            created++;
            date = template.Frequency switch
            {
                RecurrenceFrequency.Daily => date.AddDays(1),
                RecurrenceFrequency.Weekly => date.AddDays(7),
                RecurrenceFrequency.BiWeekly => date.AddDays(14),
                RecurrenceFrequency.Monthly => date.AddMonths(1),
                _ => date.AddDays(7),
            };
        }

        await db.SaveChangesAsync(ct);
        return Ok(created);
    }
}

public record CreateAppointmentRequest(
    Guid PatientId,
    Guid PrimaryResourceId,
    DateTimeOffset StartAt,
    DateTimeOffset EndAt,
    string? Notes,
    bool IsOnlineBooking,
    Guid? RecurringTemplateId,
    IEnumerable<Guid>? AdditionalResourceIds,
    bool ScheduleSmsReminder);

public record CancelAppointmentRequest(string? Reason);
public record RescheduleAppointmentRequest(DateTimeOffset StartAt, DateTimeOffset EndAt);
public record AppointmentDensityDto(DateOnly Date, int Count);
