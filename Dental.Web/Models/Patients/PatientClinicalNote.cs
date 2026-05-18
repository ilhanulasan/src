using System.ComponentModel.DataAnnotations;
using Dental.Web.Models.Common;

namespace Dental.Web.Models;

public class PatientClinicalNote : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    [Required]
    [MaxLength(128)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(8000)]
    public string Content { get; set; } = string.Empty;

    public bool IsConfidential { get; set; }

    public Guid? ExaminationId { get; set; }
    public Examination? Examination { get; set; }
}
