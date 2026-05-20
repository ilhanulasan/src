using Dental.Web.Data;
using Dental.Web.Models;
using Dental.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dental.Web.Controllers.Api;

[AllowAnonymous]
[ApiController]
[Route("api/public/booking")]
public class PublicBookingController(
    ApplicationDbContext db,
    GuestBookingService guestBooking,
    ILogger<PublicBookingController> log) : ControllerBase
{
    [HttpGet("doctors")]
    public async Task<ActionResult<IReadOnlyList<DoctorAppointmentOptionDto>>> Doctors(CancellationToken ct)
    {
        var doctors = await db.ClinicPersonnel.AsNoTracking()
            .Where(p => p.PersonnelType == PersonnelType.Doctor && p.IsActive && p.AppointmentResourceId != null)
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .ToListAsync(ct);

        return Ok(doctors.Select(p => new DoctorAppointmentOptionDto
        {
            PersonnelId = p.Id,
            ResourceId = p.AppointmentResourceId!.Value,
            DisplayName = $"{p.FirstName} {p.LastName}",
            Specialties = p.Specialties,
        }).ToList());
    }

    [HttpGet("availability")]
    public async Task<ActionResult<IEnumerable<TimeSlotDto>>> Availability(
        [FromQuery] Guid resourceId,
        [FromQuery] DateOnly date,
        CancellationToken ct)
    {
        var slots = await AppointmentScheduling.GetAvailableSlotsAsync(db, resourceId, date, ct);
        return Ok(slots.Select(s => new TimeSlotDto(s.StartAt, s.EndAt)));
    }

    [HttpPost]
    public async Task<ActionResult<GuestBookResponseDto>> Book([FromBody] GuestBookRequestDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (string.IsNullOrWhiteSpace(dto.Email))
        {
            return BadRequest("Email is required.");
        }

        try
        {
            var result = await guestBooking.BookAsync(
                new GuestBookRequest(
                    dto.FirstName,
                    dto.LastName,
                    dto.Email,
                    dto.Phone,
                    dto.ResourceId,
                    dto.StartAt,
                    dto.EndAt,
                    dto.Notes,
                    dto.DateOfBirth,
                    dto.Gender,
                    dto.PreferTurkish),
                ct);

            return Ok(new GuestBookResponseDto
            {
                AppointmentId = result.AppointmentId,
                PatientId = result.PatientId,
                IsNewPatient = result.IsNewPatient,
                NeedsRegistration = result.NeedsRegistration,
                MessageKey = "booking.submitted",
            });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("slot", StringComparison.OrdinalIgnoreCase))
        {
            return Conflict("Selected slot is no longer available.");
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Guest booking failed");
            return Problem("Could not complete booking.");
        }
    }

    [HttpPost("confirm")]
    public async Task<IActionResult> Confirm([FromBody] ConfirmGuestAppointmentDto dto, CancellationToken ct)
    {
        var ok = await guestBooking.ConfirmAsync(dto.AppointmentId, dto.Token, ct);
        return ok ? NoContent() : NotFound();
    }
}

public class GuestBookRequestDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public Guid ResourceId { get; set; }
    public DateTimeOffset StartAt { get; set; }
    public DateTimeOffset EndAt { get; set; }
    public string? Notes { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public bool PreferTurkish { get; set; } = true;
}

public class GuestBookResponseDto
{
    public Guid AppointmentId { get; set; }
    public Guid PatientId { get; set; }
    public bool IsNewPatient { get; set; }
    public bool NeedsRegistration { get; set; }
    public string MessageKey { get; set; } = string.Empty;
}

public class ConfirmGuestAppointmentDto
{
    public Guid AppointmentId { get; set; }
    public string Token { get; set; } = string.Empty;
}
