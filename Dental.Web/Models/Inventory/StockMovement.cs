using System.ComponentModel.DataAnnotations;
using Dental.Web.Models.Common;

namespace Dental.Web.Models;

public class StockMovement : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid StockLotId { get; set; }
    public StockLot StockLot { get; set; } = null!;

    public StockMovementType MovementType { get; set; }

    public decimal Quantity { get; set; }

    public DateTimeOffset MovedAt { get; set; } = DateTimeOffset.UtcNow;

    [MaxLength(512)]
    public string? Reference { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }
}
