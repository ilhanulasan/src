using System.Security.Claims;
using Dental.Web.Data;
using Dental.Web.Models;
using Dental.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dental.Web.Controllers.Api;

[Authorize(Policy = "PatientPortal")]
[ApiController]
[Route("api/portal")]
public class PatientPortalController(ApplicationDbContext db, ILogger<PatientPortalController> log) : ControllerBase
{
    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    [HttpGet("patient")]
    public async Task<ActionResult<Patient>> GetLinkedPatient(CancellationToken ct)
    {
        var userId = CurrentUserId;
        if (userId is null) return Unauthorized();

        var patient = await db.Patients.AsNoTracking().FirstOrDefaultAsync(p => p.UserId == userId, ct);
        return patient is null ? NotFound() : Ok(patient);
    }

    [HttpPost("link")]
    public async Task<ActionResult<Patient>> LinkPatient([FromBody] LinkPatientRequest request, CancellationToken ct)
    {
        var userId = CurrentUserId;
        if (userId is null) return Unauthorized();

        var existing = await db.Patients.FirstOrDefaultAsync(p => p.UserId == userId, ct);
        if (existing is not null) return Ok(existing);

        Patient? patient = null;
        if (!string.IsNullOrWhiteSpace(request.SocialSecurityNumber))
        {
            patient = await db.Patients.FirstOrDefaultAsync(
                p => p.SocialSecurityNumber == request.SocialSecurityNumber.Trim(), ct);
        }

        if (patient is null && request.CreateFromProfile)
        {
            patient = new Patient
            {
                Id = Guid.NewGuid(),
                Name = request.FirstName?.Trim() ?? "Hasta",
                Surname = request.LastName?.Trim() ?? "Kayıt",
                SocialSecurityNumber = request.SocialSecurityNumber?.Trim()
                    ?? $"USR-{userId[..Math.Min(8, userId.Length)]}",
                Phone = request.Phone,
                Email = request.Email,
                DateOfBirth = request.DateOfBirth ?? DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Gender = request.Gender ?? "Other",
                Education = EducationLevel.Graduate,
                UserId = userId,
                IsActive = true,
            };
            db.Patients.Add(patient);
        }
        else if (patient is not null)
        {
            if (patient.UserId is not null && patient.UserId != userId)
            {
                return Conflict("This patient record is already linked to another account.");
            }

            patient.UserId = userId;
            if (!string.IsNullOrWhiteSpace(request.Phone)) patient.Phone = request.Phone;
            if (!string.IsNullOrWhiteSpace(request.Email)) patient.Email = request.Email;
        }
        else
        {
            return NotFound("Patient record not found. Provide SSN or enable profile creation.");
        }

        await db.SaveChangesAsync(ct);
        log.LogInformation("Linked patient {PatientId} to user {UserId}", patient!.Id, userId);
        return Ok(patient);
    }

    [HttpGet("resources")]
    public async Task<ActionResult<IEnumerable<AppointmentResource>>> Resources(CancellationToken ct) =>
        Ok(await db.AppointmentResources.AsNoTracking()
            .Where(r => r.IsActive && r.ResourceType == AppointmentResourceType.Doctor)
            .OrderBy(r => r.Name)
            .ToListAsync(ct));

    [HttpGet("availability")]
    public async Task<ActionResult<IEnumerable<TimeSlotDto>>> Availability(
        [FromQuery] Guid resourceId,
        [FromQuery] DateOnly date,
        CancellationToken ct)
    {
        var slots = await AppointmentScheduling.GetAvailableSlotsAsync(db, resourceId, date, ct);
        return Ok(slots.Select(s => new TimeSlotDto(s.StartAt, s.EndAt)));
    }

    [HttpPost("book")]
    public async Task<ActionResult<Appointment>> Book([FromBody] PortalBookRequest request, CancellationToken ct)
    {
        var userId = CurrentUserId;
        if (userId is null) return Unauthorized();

        var patient = await db.Patients.FirstOrDefaultAsync(p => p.UserId == userId, ct);
        if (patient is null) return BadRequest("Link a patient profile before booking.");

        var startAt = AppointmentScheduling.ToUtc(request.StartAt);
        var endAt = AppointmentScheduling.ToUtc(request.EndAt);

        if (endAt <= startAt)
        {
            return BadRequest("Invalid time range.");
        }

        if (await AppointmentScheduling.HasResourceConflictAsync(
                db, request.ResourceId, null, startAt, endAt, null, ct))
        {
            return Conflict("Selected slot is no longer available.");
        }

        var appt = new Appointment
        {
            Id = Guid.NewGuid(),
            PatientId = patient.Id,
            PrimaryResourceId = request.ResourceId,
            StartAt = startAt,
            EndAt = endAt,
            Notes = request.Notes,
            Status = AppointmentStatus.Scheduled,
            IsOnlineBooking = true,
        };

        db.Appointments.Add(appt);

        if (!string.IsNullOrWhiteSpace(patient.Phone))
        {
            db.SmsReminderLogs.Add(new SmsReminderLog
            {
                Id = Guid.NewGuid(),
                AppointmentId = appt.Id,
                PhoneNumber = patient.Phone,
                Message = $"Online randevu: {startAt.LocalDateTime:dd.MM.yyyy HH:mm}",
                ScheduledFor = startAt.AddHours(-24),
                Status = SmsReminderStatus.Pending,
            });
        }

        await db.SaveChangesAsync(ct);
        log.LogInformation("Portal booking {AppointmentId} for patient {PatientId}", appt.Id, patient.Id);
        return Ok(appt);
    }

    [HttpGet("appointments")]
    public async Task<ActionResult<IEnumerable<Appointment>>> MyAppointments(CancellationToken ct)
    {
        var userId = CurrentUserId;
        if (userId is null) return Unauthorized();

        var patientId = await db.Patients.AsNoTracking()
            .Where(p => p.UserId == userId)
            .Select(p => p.Id)
            .FirstOrDefaultAsync(ct);

        if (patientId == Guid.Empty) return Ok(Array.Empty<Appointment>());

        var list = await db.Appointments.AsNoTracking()
            .Include(a => a.PrimaryResource)
            .Where(a => a.PatientId == patientId)
            .OrderByDescending(a => a.StartAt)
            .ToListAsync(ct);

        return Ok(list);
    }

    [HttpPost("appointments/{id:guid}/cancel")]
    public async Task<IActionResult> CancelMyAppointment(Guid id, CancellationToken ct)
    {
        var userId = CurrentUserId;
        if (userId is null) return Unauthorized();

        var patientId = await db.Patients.Where(p => p.UserId == userId).Select(p => p.Id).FirstOrDefaultAsync(ct);
        if (patientId == Guid.Empty) return NotFound();

        var appt = await db.Appointments.FirstOrDefaultAsync(a => a.Id == id && a.PatientId == patientId, ct);
        if (appt is null) return NotFound();

        if (appt.StartAt <= DateTimeOffset.UtcNow.AddHours(2))
        {
            return BadRequest("Appointments within 2 hours cannot be cancelled online.");
        }

        appt.Status = AppointmentStatus.Cancelled;
        appt.CancelledAt = DateTimeOffset.UtcNow;
        appt.CancellationReason = "Cancelled by patient (portal)";
        await db.SaveChangesAsync(ct);
        return NoContent();
    }
}

public record LinkPatientRequest(
    string? SocialSecurityNumber,
    bool CreateFromProfile,
    string? FirstName,
    string? LastName,
    string? Phone,
    string? Email,
    DateOnly? DateOfBirth,
    string? Gender);

public record PortalBookRequest(
    Guid ResourceId,
    DateTimeOffset StartAt,
    DateTimeOffset EndAt,
    string? Notes);

public record TimeSlotDto(DateTimeOffset StartAt, DateTimeOffset EndAt);
