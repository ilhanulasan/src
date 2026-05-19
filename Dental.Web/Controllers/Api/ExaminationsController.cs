using System.Security.Claims;
using Dental.Web.Data;
using Dental.Web.Models;
using Dental.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dental.Web.Controllers.Api;

[Authorize(Policy = "Staff")]
[ApiController]
[Route("api/examinations")]
public class ExaminationsController(ApplicationDbContext db, ILogger<ExaminationsController> log) : ControllerBase
{
    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);
    private bool IsDoctorOnly => User.IsInRole(AppRoles.Doctor) && !User.IsInRole(AppRoles.Admin);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Examination>>> List(
        [FromQuery] Guid? patientId,
        [FromQuery] ExaminationStatus? status,
        CancellationToken ct)
    {
        var q = db.Examinations.AsNoTracking().Include(e => e.Diagnoses).ThenInclude(d => d.Icd10Code).AsQueryable();
        if (patientId.HasValue) q = q.Where(e => e.PatientId == patientId);
        if (status.HasValue) q = q.Where(e => e.Status == status);
        if (IsDoctorOnly && CurrentUserId is not null)
        {
            q = await DoctorScope.ApplyExaminationFilterAsync(q, db, CurrentUserId, ct);
        }

        return Ok(await q.OrderByDescending(e => e.ExaminedAt).ToListAsync(ct));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Examination>> Get(Guid id, CancellationToken ct)
    {
        var exam = await db.Examinations.AsNoTracking()
            .Include(e => e.Diagnoses).ThenInclude(d => d.Icd10Code)
            .Include(e => e.Interventions)
            .FirstOrDefaultAsync(e => e.Id == id, ct);
        return exam is null ? NotFound() : Ok(exam);
    }

    [HttpPost]
    public async Task<ActionResult<Examination>> Create([FromBody] CreateExaminationRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (!await db.Patients.AnyAsync(p => p.Id == request.PatientId, ct))
        {
            return BadRequest("Patient not found.");
        }

        var entity = new Examination
        {
            Id = Guid.NewGuid(),
            PatientId = request.PatientId,
            DoctorUserId = request.DoctorUserId,
            ExaminedAt = request.ExaminedAt ?? DateTimeOffset.UtcNow,
            Status = request.Status,
            ChiefComplaint = request.ChiefComplaint,
            ClinicalFindings = request.ClinicalFindings,
            Notes = request.Notes,
        };

        db.Examinations.Add(entity);
        await db.SaveChangesAsync(ct);
        log.LogInformation("Created examination {Id}", entity.Id);
        return CreatedAtAction(nameof(Get), new { id = entity.Id }, entity);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Examination input, CancellationToken ct)
    {
        var entity = await db.Examinations.FirstOrDefaultAsync(e => e.Id == id, ct);
        if (entity is null) return NotFound();
        entity.Status = input.Status;
        entity.ChiefComplaint = input.ChiefComplaint;
        entity.ClinicalFindings = input.ClinicalFindings;
        entity.Notes = input.Notes;
        entity.DoctorUserId = input.DoctorUserId;
        entity.ExaminedAt = input.ExaminedAt;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/diagnoses")]
    public async Task<ActionResult<ExaminationDiagnosis>> AddDiagnosis(
        Guid id, [FromBody] AddExaminationDiagnosisRequest request, CancellationToken ct)
    {
        if (!await db.Examinations.AnyAsync(e => e.Id == id, ct)) return NotFound();
        if (request.Icd10CodeId == Guid.Empty) return BadRequest("ICD-10 code id is required.");
        if (!await db.Icd10Codes.AnyAsync(c => c.Id == request.Icd10CodeId, ct)) return BadRequest("ICD-10 code not found.");

        var diagnosis = new ExaminationDiagnosis
        {
            Id = Guid.NewGuid(),
            ExaminationId = id,
            Icd10CodeId = request.Icd10CodeId,
            IsPrimary = request.IsPrimary,
            Notes = request.Notes,
        };

        db.ExaminationDiagnoses.Add(diagnosis);
        await db.SaveChangesAsync(ct);
        return Ok(diagnosis);
    }

    [HttpDelete("{examId:guid}/diagnoses/{diagId:guid}")]
    public async Task<IActionResult> RemoveDiagnosis(Guid examId, Guid diagId, CancellationToken ct)
    {
        var entity = await db.ExaminationDiagnoses.FirstOrDefaultAsync(d => d.Id == diagId && d.ExaminationId == examId, ct);
        if (entity is null) return NotFound();
        db.ExaminationDiagnoses.Remove(entity);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/interventions")]
    public async Task<ActionResult<MedicalIntervention>> AddIntervention(
        Guid id, [FromBody] MedicalIntervention input, CancellationToken ct)
    {
        if (!await db.Examinations.AnyAsync(e => e.Id == id, ct)) return NotFound();
        input.Id = Guid.NewGuid();
        input.ExaminationId = id;
        db.MedicalInterventions.Add(input);
        await db.SaveChangesAsync(ct);
        return Ok(input);
    }
}
