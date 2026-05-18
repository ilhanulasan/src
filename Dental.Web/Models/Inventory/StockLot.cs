using System.ComponentModel.DataAnnotations;
using Dental.Web.Models.Common;

namespace Dental.Web.Models;

public class StockLot : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    [MaxLength(64)]
    public string? Barcode { get; set; }

    [MaxLength(64)]
    public string? BatchNumber { get; set; }

    public decimal QuantityOnHand { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    public DateOnly? ReceivedDate { get; set; }

    public ICollection<StockMovement> Movements { get; set; } = [];
}
