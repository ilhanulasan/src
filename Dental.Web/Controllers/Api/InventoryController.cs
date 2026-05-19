using Dental.Web.Data;
using Dental.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dental.Web.Controllers.Api;

[Authorize(Policy = "AdminOnly")]
[ApiController]
[Route("api/suppliers")]
public class SuppliersController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Supplier>>> List([FromQuery] bool activeOnly = true, CancellationToken ct = default)
    {
        var q = db.Suppliers.AsNoTracking().AsQueryable();
        if (activeOnly) q = q.Where(s => s.IsActive);
        return Ok(await q.OrderBy(s => s.CompanyName).ToListAsync(ct));
    }

    [HttpPost]
    public async Task<ActionResult<Supplier>> Create([FromBody] Supplier input, CancellationToken ct)
    {
        input.Id = Guid.NewGuid();
        db.Suppliers.Add(input);
        await db.SaveChangesAsync(ct);
        return Ok(input);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Supplier input, CancellationToken ct)
    {
        var entity = await db.Suppliers.FindAsync([id], ct);
        if (entity is null) return NotFound();
        entity.CompanyName = input.CompanyName;
        entity.ContactPerson = input.ContactPerson;
        entity.Phone = input.Phone;
        entity.Email = input.Email;
        entity.Address = input.Address;
        entity.TaxNumber = input.TaxNumber;
        entity.IsActive = input.IsActive;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }
}

[Authorize(Policy = "AdminOnly")]
[ApiController]
[Route("api/products")]
public class ProductsController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> List([FromQuery] string? barcode, CancellationToken ct)
    {
        var q = db.Products.AsNoTracking().Include(p => p.Supplier).AsQueryable();
        if (!string.IsNullOrWhiteSpace(barcode)) q = q.Where(p => p.Barcode == barcode);
        return Ok(await q.OrderBy(p => p.Name).ToListAsync(ct));
    }

    [HttpPost]
    public async Task<ActionResult<Product>> Create([FromBody] Product input, CancellationToken ct)
    {
        input.Id = Guid.NewGuid();
        db.Products.Add(input);
        await db.SaveChangesAsync(ct);
        return Ok(input);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Product input, CancellationToken ct)
    {
        var entity = await db.Products.FindAsync([id], ct);
        if (entity is null) return NotFound();
        entity.Name = input.Name;
        entity.Sku = input.Sku;
        entity.Barcode = input.Barcode;
        entity.Unit = input.Unit;
        entity.SupplierId = input.SupplierId;
        entity.UnitCost = input.UnitCost;
        entity.UnitPrice = input.UnitPrice;
        entity.MinimumStockLevel = input.MinimumStockLevel;
        entity.IsActive = input.IsActive;
        entity.Description = input.Description;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }
}

[Authorize(Policy = "AdminOnly")]
[ApiController]
[Route("api/stock")]
public class StockController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet("lots")]
    public async Task<ActionResult<IEnumerable<StockLot>>> ListLots(
        [FromQuery] Guid? productId,
        [FromQuery] string? barcode,
        CancellationToken ct)
    {
        var q = db.StockLots.AsNoTracking().Include(l => l.Product).AsQueryable();
        if (productId.HasValue) q = q.Where(l => l.ProductId == productId);
        if (!string.IsNullOrWhiteSpace(barcode)) q = q.Where(l => l.Barcode == barcode);
        return Ok(await q.OrderBy(l => l.ExpiryDate).ToListAsync(ct));
    }

    [HttpGet("expiry-alerts")]
    public async Task<ActionResult<IEnumerable<StockExpiryAlertDto>>> ExpiryAlerts(
        [FromQuery] int withinDays = 30,
        CancellationToken ct = default)
    {
        var threshold = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(withinDays));
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var lots = await db.StockLots.AsNoTracking()
            .Include(l => l.Product)
            .Where(l => l.ExpiryDate != null && l.ExpiryDate <= threshold && l.QuantityOnHand > 0)
            .OrderBy(l => l.ExpiryDate)
            .ToListAsync(ct);

        return Ok(lots.Select(l => new StockExpiryAlertDto(
            l.Id,
            l.ProductId,
            l.Product.Name,
            l.BatchNumber,
            l.ExpiryDate!.Value,
            l.QuantityOnHand,
            l.ExpiryDate < today)));
    }

    [HttpGet("low-stock")]
    public async Task<ActionResult<IEnumerable<LowStockAlertDto>>> LowStock(CancellationToken ct)
    {
        var products = await db.Products.AsNoTracking().Where(p => p.IsActive).ToListAsync(ct);
        var lots = await db.StockLots.AsNoTracking().GroupBy(l => l.ProductId)
            .Select(g => new { ProductId = g.Key, Qty = g.Sum(x => x.QuantityOnHand) })
            .ToListAsync(ct);

        var alerts = products
            .Select(p =>
            {
                var qty = lots.FirstOrDefault(l => l.ProductId == p.Id)?.Qty ?? 0;
                return new { p, qty };
            })
            .Where(x => x.qty <= x.p.MinimumStockLevel)
            .Select(x => new LowStockAlertDto(x.p.Id, x.p.Name, x.qty, x.p.MinimumStockLevel))
            .ToList();

        return Ok(alerts);
    }

    [HttpPost("lots")]
    public async Task<ActionResult<StockLot>> ReceiveLot([FromBody] ReceiveStockRequest request, CancellationToken ct)
    {
        var lot = new StockLot
        {
            Id = Guid.NewGuid(),
            ProductId = request.ProductId,
            Barcode = request.Barcode,
            BatchNumber = request.BatchNumber,
            QuantityOnHand = request.Quantity,
            ExpiryDate = request.ExpiryDate,
            ReceivedDate = request.ReceivedDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
        };

        db.StockLots.Add(lot);
        db.StockMovements.Add(new StockMovement
        {
            Id = Guid.NewGuid(),
            StockLotId = lot.Id,
            MovementType = StockMovementType.Inbound,
            Quantity = request.Quantity,
            Reference = request.Reference,
            Notes = request.Notes,
        });

        await db.SaveChangesAsync(ct);
        return Ok(lot);
    }

    [HttpPost("lots/{lotId:guid}/movements")]
    public async Task<ActionResult<StockMovement>> AddMovement(
        Guid lotId, [FromBody] StockMovement input, CancellationToken ct)
    {
        var lot = await db.StockLots.FindAsync([lotId], ct);
        if (lot is null) return NotFound();

        input.Id = Guid.NewGuid();
        input.StockLotId = lotId;
        input.MovedAt = DateTimeOffset.UtcNow;

        lot.QuantityOnHand += input.MovementType switch
        {
            StockMovementType.Inbound or StockMovementType.Return => input.Quantity,
            StockMovementType.Outbound or StockMovementType.Expired => -input.Quantity,
            StockMovementType.Adjustment => input.Quantity,
            _ => 0,
        };

        if (lot.QuantityOnHand < 0) return BadRequest("Insufficient stock.");

        db.StockMovements.Add(input);
        await db.SaveChangesAsync(ct);
        return Ok(input);
    }
}

public record ReceiveStockRequest(
    Guid ProductId,
    decimal Quantity,
    string? Barcode,
    string? BatchNumber,
    DateOnly? ExpiryDate,
    DateOnly? ReceivedDate,
    string? Reference,
    string? Notes);

public record StockExpiryAlertDto(
    Guid LotId,
    Guid ProductId,
    string ProductName,
    string? BatchNumber,
    DateOnly ExpiryDate,
    decimal QuantityOnHand,
    bool IsExpired);

public record LowStockAlertDto(Guid ProductId, string ProductName, decimal CurrentQuantity, int MinimumLevel);
