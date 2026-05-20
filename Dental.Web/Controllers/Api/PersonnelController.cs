using System.Security.Claims;
using Dental.Web.Data;
using Dental.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dental.Web.Controllers.Api;

[Authorize(Policy = "Staff")]
[ApiController]
[Route("api/personnel")]
public class PersonnelController(ApplicationDbContext db, ILogger<PersonnelController> log) : ControllerBase
{
    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PersonnelDto>>> List(
        [FromQuery] PersonnelType? type,
        [FromQuery] bool activeOnly = false,
        CancellationToken ct = default)
    {
        var q = db.ClinicPersonnel.AsNoTracking().AsQueryable();
        if (type.HasValue)
        {
            q = q.Where(p => p.PersonnelType == type);
        }

        if (activeOnly)
        {
            q = q.Where(p => p.IsActive);
        }

        var list = await q
            .OrderBy(p => p.PersonnelType)
            .ThenBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .ToListAsync(ct);

        return Ok(list.Select(Map).ToList());
    }

    [HttpGet("doctors-for-appointments")]
    public async Task<ActionResult<IReadOnlyList<DoctorAppointmentOptionDto>>> DoctorsForAppointments(CancellationToken ct)
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

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PersonnelDto>> GetById(Guid id, CancellationToken ct)
    {
        var entity = await db.ClinicPersonnel.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        return Ok(Map(entity));
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<PersonnelDto>> Create([FromBody] CreatePersonnelDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (!ValidateSpecialties(dto.PersonnelType, dto.Specialties, out var specialtyError))
        {
            return BadRequest(specialtyError);
        }

        var entity = new ClinicPersonnel
        {
            Id = Guid.NewGuid(),
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Email = NormalizeOptional(dto.Email),
            Phone = NormalizeOptional(dto.Phone),
            Notes = NormalizeOptional(dto.Notes),
            PersonnelType = dto.PersonnelType,
            Specialties = dto.Specialties.Distinct().ToList(),
            IsActive = dto.IsActive,
            CreatedByUserId = CurrentUserId,
        };

        db.ClinicPersonnel.Add(entity);
        await SyncDoctorResourceAsync(entity, ct);
        await db.SaveChangesAsync(ct);

        log.LogInformation("Created personnel {PersonnelId}", entity.Id);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, Map(entity));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<PersonnelDto>> Update(Guid id, [FromBody] UpdatePersonnelDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (id != dto.Id)
        {
            return BadRequest("Route id must match payload id.");
        }

        if (!ValidateSpecialties(dto.PersonnelType, dto.Specialties, out var specialtyError))
        {
            return BadRequest(specialtyError);
        }

        var entity = await db.ClinicPersonnel.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        entity.FirstName = dto.FirstName.Trim();
        entity.LastName = dto.LastName.Trim();
        entity.Email = NormalizeOptional(dto.Email);
        entity.Phone = NormalizeOptional(dto.Phone);
        entity.Notes = NormalizeOptional(dto.Notes);
        entity.PersonnelType = dto.PersonnelType;
        entity.Specialties = dto.Specialties.Distinct().ToList();
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        entity.UpdatedByUserId = CurrentUserId;

        if (entity.PersonnelType != PersonnelType.Doctor)
        {
            entity.Specialties = [];
            await DeactivateDoctorResourceAsync(entity, ct);
        }
        else
        {
            await SyncDoctorResourceAsync(entity, ct);
        }

        await db.SaveChangesAsync(ct);
        log.LogInformation("Updated personnel {PersonnelId}", id);
        return Ok(Map(entity));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var entity = await db.ClinicPersonnel.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        await DeactivateDoctorResourceAsync(entity, ct);
        db.ClinicPersonnel.Remove(entity);
        await db.SaveChangesAsync(ct);
        log.LogInformation("Deleted personnel {PersonnelId}", id);
        return NoContent();
    }

    private static bool ValidateSpecialties(PersonnelType type, IList<DentalSpecialty> specialties, out string? error)
    {
        if (type == PersonnelType.Doctor)
        {
            error = null;
            return true;
        }

        if (specialties.Count > 0)
        {
            error = "Specialties apply only to doctors.";
            return false;
        }

        error = null;
        return true;
    }

    private async Task SyncDoctorResourceAsync(ClinicPersonnel entity, CancellationToken ct)
    {
        if (entity.PersonnelType != PersonnelType.Doctor)
        {
            return;
        }

        var displayName = $"Dr. {entity.FirstName} {entity.LastName}".Trim();

        if (entity.AppointmentResourceId is Guid resourceId)
        {
            var resource = await db.AppointmentResources.FirstOrDefaultAsync(r => r.Id == resourceId, ct);
            if (resource is not null)
            {
                resource.Name = displayName;
                resource.ResourceType = AppointmentResourceType.Doctor;
                resource.IsActive = entity.IsActive;
                resource.UserId = entity.UserId;
                resource.UpdatedAt = DateTimeOffset.UtcNow;
                return;
            }
        }

        var created = new AppointmentResource
        {
            Id = Guid.NewGuid(),
            Name = displayName,
            ResourceType = AppointmentResourceType.Doctor,
            DefaultDurationMinutes = 30,
            Color = "#0f766e",
            IsActive = entity.IsActive,
            UserId = entity.UserId,
            CreatedByUserId = CurrentUserId,
        };
        db.AppointmentResources.Add(created);
        entity.AppointmentResourceId = created.Id;
    }

    private async Task DeactivateDoctorResourceAsync(ClinicPersonnel entity, CancellationToken ct)
    {
        if (entity.AppointmentResourceId is not Guid resourceId)
        {
            return;
        }

        var resource = await db.AppointmentResources.FirstOrDefaultAsync(r => r.Id == resourceId, ct);
        if (resource is not null)
        {
            resource.IsActive = false;
            resource.UpdatedAt = DateTimeOffset.UtcNow;
        }

        entity.AppointmentResourceId = null;
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static PersonnelDto Map(ClinicPersonnel p) =>
        new()
        {
            Id = p.Id,
            FirstName = p.FirstName,
            LastName = p.LastName,
            Email = p.Email,
            Phone = p.Phone,
            Notes = p.Notes,
            PersonnelType = p.PersonnelType,
            Specialties = p.Specialties,
            UserId = p.UserId,
            AppointmentResourceId = p.AppointmentResourceId,
            IsActive = p.IsActive,
        };
}
