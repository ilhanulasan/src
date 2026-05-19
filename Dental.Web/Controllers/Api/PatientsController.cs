using System.Security.Claims;
using System.Text.Json;
using Dental.Web.Data;
using Dental.Web.Models;
using Dental.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dental.Web.Controllers.Api;

[Authorize(Policy = "Staff")]
[ApiController]
[Route("api/patients")]
public class PatientsController(ApplicationDbContext db, ILogger<PatientsController> log) : ControllerBase
{
    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);
    private bool IsDoctorOnly => User.IsInRole(AppRoles.Doctor) && !User.IsInRole(AppRoles.Admin);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Patient>>> GetAll(CancellationToken ct)
    {
        var q = db.Patients.AsNoTracking().AsQueryable();
        if (IsDoctorOnly && CurrentUserId is not null)
        {
            q = await DoctorScope.ApplyPatientFilterAsync(q, db, CurrentUserId, ct);
        }

        var list = await q.OrderBy(p => p.Surname).ThenBy(p => p.Name).ToListAsync(ct);
        return Ok(list);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Patient>> GetById(Guid id, CancellationToken ct)
    {
        var q = db.Patients.AsNoTracking().Where(p => p.Id == id);
        if (IsDoctorOnly && CurrentUserId is not null)
        {
            q = await DoctorScope.ApplyPatientFilterAsync(q, db, CurrentUserId, ct);
        }

        var patient = await q.FirstOrDefaultAsync(ct);
        if (patient is null)
        {
            return NotFound();
        }

        return Ok(patient);
    }

    [HttpPost]
    public async Task<ActionResult<Patient>> Create([FromBody] Patient input, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        input.Id = Guid.NewGuid();

        db.Patients.Add(input);

        try
        {
            await db.SaveChangesAsync(ct);
            log.LogInformation("Created patient {PatientId}", input.Id);
            return CreatedAtAction(nameof(GetById), new { id = input.Id }, input);
        }
        catch (DbUpdateException ex)
        {
            log.LogWarning(ex, "Duplicate SSN when creating patient");
            return Conflict("A patient with this social security number already exists.");
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Patient input, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (id != input.Id)
        {
            return BadRequest("Route id must match payload id.");
        }

        var existing = await db.Patients.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (existing is null)
        {
            return NotFound();
        }

        existing.Name = input.Name;
        existing.Surname = input.Surname;
        existing.SocialSecurityNumber = input.SocialSecurityNumber;
        existing.Address = input.Address;
        existing.Phone = input.Phone;
        existing.Email = input.Email;
        existing.DateOfBirth = input.DateOfBirth;
        existing.Gender = input.Gender;
        existing.Education = input.Education;
        existing.BloodType = input.BloodType;
        existing.EmergencyContactName = input.EmergencyContactName;
        existing.EmergencyContactPhone = input.EmergencyContactPhone;
        existing.ClinicalSummary = input.ClinicalSummary;
        existing.IsActive = input.IsActive;
        existing.UpdatedAt = DateTimeOffset.UtcNow;

        try
        {
            await db.SaveChangesAsync(ct);
            log.LogInformation("Updated patient {PatientId}", id);
            return NoContent();
        }
        catch (DbUpdateException ex)
        {
            log.LogWarning(ex, "Duplicate SSN when updating patient");
            return Conflict("A patient with this social security number already exists.");
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var entity = await db.Patients.FindAsync([id], ct);
        if (entity is null)
        {
            return NotFound();
        }

        db.Patients.Remove(entity);
        await db.SaveChangesAsync(ct);
        log.LogInformation("Deleted patient {PatientId}", id);
        return NoContent();
    }

    [HttpGet("{id:guid}/odontogram")]
    public async Task<ActionResult<OdontogramDocumentDto>> GetOdontogram(Guid id, CancellationToken ct)
    {
        var exists = await db.Patients.AsNoTracking().AnyAsync(p => p.Id == id, ct);
        if (!exists)
        {
            return NotFound();
        }

        var entity = await db.PatientOdontograms.AsNoTracking().FirstOrDefaultAsync(o => o.PatientId == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        return Ok(OdontogramDocumentDto.FromEntity(entity));
    }

    [HttpPut("{id:guid}/odontogram")]
    public async Task<ActionResult<OdontogramDocumentDto>> UpsertOdontogram(
        Guid id,
        [FromBody] OdontogramDocumentDto body,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var patientExists = await db.Patients.AnyAsync(p => p.Id == id, ct);
        if (!patientExists)
        {
            return NotFound();
        }

        var snapshot = body.ToSnapshot();
        var normalizedType = OdontogramDocumentDto.NormalizedChartType(body.Type);
        var json = JsonSerializer.Serialize(snapshot, SerializationOptions.Json);

        var entity = await db.PatientOdontograms.FirstOrDefaultAsync(o => o.PatientId == id, ct);
        if (entity is null)
        {
            entity = new PatientOdontogram
            {
                Id = Guid.NewGuid(),
                PatientId = id,
                Type = normalizedType,
                PayloadJson = json,
            };

            db.PatientOdontograms.Add(entity);
        }
        else
        {
            entity.Type = normalizedType;
            entity.PayloadJson = json;
        }

        await db.SaveChangesAsync(ct);
        log.LogInformation("Upserted odontogram for patient {PatientId}", id);
        return Ok(OdontogramDocumentDto.FromEntity(entity));
    }
}
