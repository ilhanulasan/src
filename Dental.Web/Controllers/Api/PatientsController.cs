using Dental.Web.Data;
using Dental.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dental.Web.Controllers.Api;

[ApiController]
[Route("api/patients")]
public class PatientsController(ApplicationDbContext db, ILogger<PatientsController> log) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Patient>>> GetAll(CancellationToken ct)
    {
        var list = await db.Patients.AsNoTracking().OrderBy(p => p.Surname).ThenBy(p => p.Name).ToListAsync(ct);
        return Ok(list);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Patient>> GetById(Guid id, CancellationToken ct)
    {
        var patient = await db.Patients.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
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
        existing.DateOfBirth = input.DateOfBirth;
        existing.Gender = input.Gender;
        existing.Education = input.Education;

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
}
