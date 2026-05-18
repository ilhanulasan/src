using System.ComponentModel.DataAnnotations;
using Dental.Web.Models.Common;

namespace Dental.Web.Models;

public class PatientChronicCondition : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    [Required]
    [MaxLength(256)]
    public string ConditionName { get; set; } = string.Empty;

    [MaxLength(16)]
    public string? Icd10Code { get; set; }

    public DateOnly? DiagnosedOn { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;
}
