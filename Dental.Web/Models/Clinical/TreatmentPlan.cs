using System.ComponentModel.DataAnnotations;
using Dental.Web.Models.Common;

namespace Dental.Web.Models;

public class TreatmentPlan : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    public Guid? ExaminationId { get; set; }
    public Examination? Examination { get; set; }

    [Required]
    [MaxLength(256)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    public TreatmentPlanStatus Status { get; set; } = TreatmentPlanStatus.Draft;

    public DateOnly? PlannedStartDate { get; set; }
    public DateOnly? PlannedEndDate { get; set; }

    public decimal EstimatedTotal { get; set; }

    public ICollection<TreatmentPlanItem> Items { get; set; } = [];
}
