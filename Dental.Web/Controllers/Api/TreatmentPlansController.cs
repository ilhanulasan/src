using Dental.Web.Data;
using Dental.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dental.Web.Controllers.Api;

[Authorize(Policy = "Staff")]
[ApiController]
[Route("api/treatment-plans")]
public class TreatmentPlansController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TreatmentPlan>>> List([FromQuery] Guid? patientId, CancellationToken ct)
    {
        var q = db.TreatmentPlans.AsNoTracking().Include(p => p.Items).AsQueryable();
        if (patientId.HasValue) q = q.Where(p => p.PatientId == patientId);
        return Ok(await q.OrderByDescending(p => p.CreatedAt).ToListAsync(ct));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TreatmentPlan>> Get(Guid id, CancellationToken ct)
    {
        var plan = await db.TreatmentPlans.AsNoTracking().Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
        return plan is null ? NotFound() : Ok(plan);
    }

    [HttpPost]
    public async Task<ActionResult<TreatmentPlan>> Create([FromBody] TreatmentPlan input, CancellationToken ct)
    {
        input.Id = Guid.NewGuid();
        foreach (var item in input.Items)
        {
            item.Id = Guid.NewGuid();
            item.TreatmentPlanId = input.Id;
        }

        input.EstimatedTotal = input.Items.Sum(i => i.UnitPrice * i.Quantity);
        db.TreatmentPlans.Add(input);
        await db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(Get), new { id = input.Id }, input);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] TreatmentPlan input, CancellationToken ct)
    {
        var entity = await db.TreatmentPlans.Include(p => p.Items).FirstOrDefaultAsync(p => p.Id == id, ct);
        if (entity is null) return NotFound();
        entity.Title = input.Title;
        entity.Description = input.Description;
        entity.Status = input.Status;
        entity.PlannedStartDate = input.PlannedStartDate;
        entity.PlannedEndDate = input.PlannedEndDate;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPut("{planId:guid}/items/{itemId:guid}")]
    public async Task<IActionResult> UpdateItem(Guid planId, Guid itemId, [FromBody] TreatmentPlanItem input, CancellationToken ct)
    {
        var item = await db.TreatmentPlanItems.FirstOrDefaultAsync(i => i.Id == itemId && i.TreatmentPlanId == planId, ct);
        if (item is null) return NotFound();
        item.ProcedureName = input.ProcedureName;
        item.ToothNumbers = input.ToothNumbers;
        item.Status = input.Status;
        item.UnitPrice = input.UnitPrice;
        item.Quantity = input.Quantity;
        item.Notes = input.Notes;
        item.UpdatedAt = DateTimeOffset.UtcNow;

        var plan = await db.TreatmentPlans.Include(p => p.Items).FirstAsync(p => p.Id == planId, ct);
        plan.EstimatedTotal = plan.Items.Sum(i => i.UnitPrice * i.Quantity);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }
}
