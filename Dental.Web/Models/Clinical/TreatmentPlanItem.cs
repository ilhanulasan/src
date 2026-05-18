using System.ComponentModel.DataAnnotations;
using Dental.Web.Models.Common;

namespace Dental.Web.Models;

public class TreatmentPlanItem : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid TreatmentPlanId { get; set; }
    public TreatmentPlan TreatmentPlan { get; set; } = null!;

    [Required]
    [MaxLength(256)]
    public string ProcedureName { get; set; } = string.Empty;

    [MaxLength(64)]
    public string? ToothNumbers { get; set; }

    public int SortOrder { get; set; }

    public TreatmentPlanItemStatus Status { get; set; } = TreatmentPlanItemStatus.Planned;

    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; } = 1;

    [MaxLength(2000)]
    public string? Notes { get; set; }
}
