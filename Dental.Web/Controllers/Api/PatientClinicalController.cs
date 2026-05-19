using Dental.Web.Data;
using Dental.Web.Models;
using Dental.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dental.Web.Controllers.Api;

[Authorize(Policy = "Staff")]
[ApiController]
[Route("api/patients/{patientId:guid}")]
public class PatientClinicalController(
    ApplicationDbContext db,
    IPatientDocumentStorageService documents,
    ILogger<PatientClinicalController> log) : ControllerBase
{
    private async Task<bool> PatientExists(Guid patientId, CancellationToken ct) =>
        await db.Patients.AnyAsync(p => p.Id == patientId, ct);

    [HttpGet("medical-histories")]
    public async Task<ActionResult<IEnumerable<PatientMedicalHistory>>> GetMedicalHistories(Guid patientId, CancellationToken ct)
    {
        if (!await PatientExists(patientId, ct)) return NotFound();
        var list = await db.PatientMedicalHistories.AsNoTracking()
            .Where(x => x.PatientId == patientId).OrderByDescending(x => x.RecordedOn).ToListAsync(ct);
        return Ok(list);
    }

    [HttpPost("medical-histories")]
    public async Task<ActionResult<PatientMedicalHistory>> CreateMedicalHistory(
        Guid patientId, [FromBody] PatientMedicalHistory input, CancellationToken ct)
    {
        if (!await PatientExists(patientId, ct)) return NotFound();
        input.Id = Guid.NewGuid();
        input.PatientId = patientId;
        db.PatientMedicalHistories.Add(input);
        await db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetMedicalHistories), new { patientId }, input);
    }

    [HttpDelete("medical-histories/{id:guid}")]
    public async Task<IActionResult> DeleteMedicalHistory(Guid patientId, Guid id, CancellationToken ct)
    {
        var entity = await db.PatientMedicalHistories.FirstOrDefaultAsync(x => x.Id == id && x.PatientId == patientId, ct);
        if (entity is null) return NotFound();
        db.PatientMedicalHistories.Remove(entity);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpGet("clinical-notes")]
    public async Task<ActionResult<IEnumerable<PatientClinicalNote>>> GetClinicalNotes(Guid patientId, CancellationToken ct)
    {
        if (!await PatientExists(patientId, ct)) return NotFound();
        var list = await db.PatientClinicalNotes.AsNoTracking()
            .Where(x => x.PatientId == patientId).OrderByDescending(x => x.CreatedAt).ToListAsync(ct);
        return Ok(list);
    }

    [HttpPost("clinical-notes")]
    public async Task<ActionResult<PatientClinicalNote>> CreateClinicalNote(
        Guid patientId, [FromBody] PatientClinicalNote input, CancellationToken ct)
    {
        if (!await PatientExists(patientId, ct)) return NotFound();
        input.Id = Guid.NewGuid();
        input.PatientId = patientId;
        db.PatientClinicalNotes.Add(input);
        await db.SaveChangesAsync(ct);
        return Ok(input);
    }

    [HttpGet("allergies")]
    public async Task<ActionResult<IEnumerable<PatientAllergy>>> GetAllergies(Guid patientId, CancellationToken ct)
    {
        if (!await PatientExists(patientId, ct)) return NotFound();
        return Ok(await db.PatientAllergies.AsNoTracking().Where(x => x.PatientId == patientId).ToListAsync(ct));
    }

    [HttpPost("allergies")]
    public async Task<ActionResult<PatientAllergy>> CreateAllergy(Guid patientId, [FromBody] PatientAllergy input, CancellationToken ct)
    {
        if (!await PatientExists(patientId, ct)) return NotFound();
        input.Id = Guid.NewGuid();
        input.PatientId = patientId;
        db.PatientAllergies.Add(input);
        await db.SaveChangesAsync(ct);
        return Ok(input);
    }

    [HttpPut("allergies/{id:guid}")]
    public async Task<IActionResult> UpdateAllergy(Guid patientId, Guid id, [FromBody] PatientAllergy input, CancellationToken ct)
    {
        var entity = await db.PatientAllergies.FirstOrDefaultAsync(x => x.Id == id && x.PatientId == patientId, ct);
        if (entity is null) return NotFound();
        entity.Substance = input.Substance;
        entity.Severity = input.Severity;
        entity.Reaction = input.Reaction;
        entity.IsActive = input.IsActive;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpGet("chronic-conditions")]
    public async Task<ActionResult<IEnumerable<PatientChronicCondition>>> GetChronicConditions(Guid patientId, CancellationToken ct)
    {
        if (!await PatientExists(patientId, ct)) return NotFound();
        return Ok(await db.PatientChronicConditions.AsNoTracking().Where(x => x.PatientId == patientId).ToListAsync(ct));
    }

    [HttpPost("chronic-conditions")]
    public async Task<ActionResult<PatientChronicCondition>> CreateChronicCondition(
        Guid patientId, [FromBody] PatientChronicCondition input, CancellationToken ct)
    {
        if (!await PatientExists(patientId, ct)) return NotFound();
        input.Id = Guid.NewGuid();
        input.PatientId = patientId;
        db.PatientChronicConditions.Add(input);
        await db.SaveChangesAsync(ct);
        return Ok(input);
    }

    [HttpGet("kvkk-consents")]
    public async Task<ActionResult<IEnumerable<PatientKvkkConsent>>> GetKvkkConsents(Guid patientId, CancellationToken ct)
    {
        if (!await PatientExists(patientId, ct)) return NotFound();
        return Ok(await db.PatientKvkkConsents.AsNoTracking().Where(x => x.PatientId == patientId).ToListAsync(ct));
    }

    [HttpPost("kvkk-consents")]
    public async Task<ActionResult<PatientKvkkConsent>> RecordKvkkConsent(
        Guid patientId, [FromBody] PatientKvkkConsent input, CancellationToken ct)
    {
        if (!await PatientExists(patientId, ct)) return NotFound();
        input.Id = Guid.NewGuid();
        input.PatientId = patientId;
        input.ConsentedAt = DateTimeOffset.UtcNow;
        input.IpAddress ??= HttpContext.Connection.RemoteIpAddress?.ToString();
        input.UserAgent ??= Request.Headers.UserAgent.ToString();
        db.PatientKvkkConsents.Add(input);
        await db.SaveChangesAsync(ct);
        log.LogInformation("KVKK consent recorded for patient {PatientId} type {Type}", patientId, input.ConsentType);
        return Ok(input);
    }

    [HttpGet("documents")]
    public async Task<ActionResult<IEnumerable<PatientDocument>>> GetDocuments(Guid patientId, CancellationToken ct)
    {
        if (!await PatientExists(patientId, ct)) return NotFound();
        return Ok(await db.PatientDocuments.AsNoTracking().Where(x => x.PatientId == patientId).ToListAsync(ct));
    }

    [HttpPost("documents")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<ActionResult<PatientDocument>> UploadDocument(
        Guid patientId,
        IFormFile file,
        [FromForm] PatientDocumentCategory category,
        [FromForm] string? description,
        CancellationToken ct)
    {
        if (!await PatientExists(patientId, ct)) return NotFound();
        if (file.Length == 0) return BadRequest("File is empty.");

        await using var stream = file.OpenReadStream();
        var path = await documents.SaveAsync(patientId, file.FileName, stream, ct);

        var doc = new PatientDocument
        {
            Id = Guid.NewGuid(),
            PatientId = patientId,
            FileName = file.FileName,
            StoragePath = path,
            ContentType = file.ContentType,
            FileSizeBytes = file.Length,
            Category = category,
            Description = description,
            IsEncrypted = true,
        };

        db.PatientDocuments.Add(doc);
        await db.SaveChangesAsync(ct);
        return Ok(doc);
    }

    [HttpGet("documents/{id:guid}/download")]
    public async Task<IActionResult> DownloadDocument(Guid patientId, Guid id, CancellationToken ct)
    {
        var doc = await db.PatientDocuments.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.PatientId == patientId, ct);
        if (doc is null) return NotFound();

        var stream = await documents.OpenReadAsync(doc.StoragePath, ct);
        if (stream is null) return NotFound();

        return File(stream, doc.ContentType ?? "application/octet-stream", doc.FileName);
    }

    [HttpDelete("documents/{id:guid}")]
    public async Task<IActionResult> DeleteDocument(Guid patientId, Guid id, CancellationToken ct)
    {
        var doc = await db.PatientDocuments.FirstOrDefaultAsync(x => x.Id == id && x.PatientId == patientId, ct);
        if (doc is null) return NotFound();
        await documents.DeleteAsync(doc.StoragePath, ct);
        db.PatientDocuments.Remove(doc);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpGet("balance")]
    public async Task<ActionResult<PatientBalanceDto>> GetBalance(Guid patientId, CancellationToken ct)
    {
        if (!await PatientExists(patientId, ct)) return NotFound();
        var entries = await db.PatientLedgerEntries.AsNoTracking()
            .Where(x => x.PatientId == patientId).ToListAsync(ct);

        var charges = entries.Where(e => e.EntryType is LedgerEntryType.Charge).Sum(e => e.Amount);
        var payments = entries.Where(e => e.EntryType is LedgerEntryType.Payment or LedgerEntryType.Refund).Sum(e => e.Amount);
        return Ok(new PatientBalanceDto(patientId, charges, payments, charges - payments));
    }
}

public record PatientBalanceDto(Guid PatientId, decimal TotalCharges, decimal TotalPayments, decimal Balance);
