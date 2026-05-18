using Dental.Web.Data;
using Dental.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dental.Web.Controllers.Api;

[ApiController]
[Route("api/icd10")]
public class Icd10Controller(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Icd10Code>>> Search([FromQuery] string? q, CancellationToken ct)
    {
        var query = db.Icd10Codes.AsNoTracking().Where(c => c.IsActive);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLowerInvariant();
            query = query.Where(c =>
                c.Code.ToLower().Contains(term) ||
                c.DescriptionTr.ToLower().Contains(term) ||
                (c.DescriptionEn != null && c.DescriptionEn.ToLower().Contains(term)));
        }

        return Ok(await query.OrderBy(c => c.Code).Take(100).ToListAsync(ct));
    }
}
