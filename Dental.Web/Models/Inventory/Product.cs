using System.ComponentModel.DataAnnotations;
using Dental.Web.Models.Common;

namespace Dental.Web.Models;

public class Product : AuditableEntity
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(256)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(64)]
    public string? Sku { get; set; }

    [MaxLength(64)]
    public string? Barcode { get; set; }

    [MaxLength(32)]
    public string Unit { get; set; } = "adet";

    public Guid? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    public decimal UnitCost { get; set; }
    public decimal UnitPrice { get; set; }

    public int MinimumStockLevel { get; set; }

    public bool IsActive { get; set; } = true;

    [MaxLength(2000)]
    public string? Description { get; set; }

    public ICollection<StockLot> StockLots { get; set; } = [];
}
