using Dental.Web.Data;
using Dental.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dental.Web.Controllers.Api;

[ApiController]
[Route("api/examinations")]
public class ExaminationsController(ApplicationDbContext db, ILogger<ExaminationsController> log) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Examination>>> List(
        [FromQuery] Guid? patientId,
        [FromQuery] ExaminationStatus? status,
        CancellationToken ct)
    {
        var q = db.Examinations.AsNoTracking().Include(e => e.Diagnoses).ThenInclude(d => d.Icd10Code).AsQueryable();
        if (patientId.HasValue) q = q.Where(e => e.PatientId == patientId);
        if (status.HasValue) q = q.Where(e => e.Status == status);
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
    public async Task<ActionResult<Examination>> Create([FromBody] Examination input, CancellationToken ct)
    {
        if (!await db.Patients.AnyAsync(p => p.Id == input.PatientId, ct)) return BadRequest("Patient not found.");
        input.Id = Guid.NewGuid();
        input.ExaminedAt = input.ExaminedAt == default ? DateTimeOffset.UtcNow : input.ExaminedAt;
        db.Examinations.Add(input);
        await db.SaveChangesAsync(ct);
        log.LogInformation("Created examination {Id}", input.Id);
        return CreatedAtAction(nameof(Get), new { id = input.Id }, input);
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
        Guid id, [FromBody] ExaminationDiagnosis input, CancellationToken ct)
    {
        if (!await db.Examinations.AnyAsync(e => e.Id == id, ct)) return NotFound();
        if (!await db.Icd10Codes.AnyAsync(c => c.Id == input.Icd10CodeId, ct)) return BadRequest("ICD-10 code not found.");
        input.Id = Guid.NewGuid();
        input.ExaminationId = id;
        db.ExaminationDiagnoses.Add(input);
        await db.SaveChangesAsync(ct);
        return Ok(input);
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
