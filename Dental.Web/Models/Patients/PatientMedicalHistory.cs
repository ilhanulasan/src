using System.ComponentModel.DataAnnotations;
using Dental.Web.Models.Common;

namespace Dental.Web.Models;

public class PatientMedicalHistory : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    [Required]
    [MaxLength(256)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? Description { get; set; }

    public DateOnly? RecordedOn { get; set; }

    [MaxLength(128)]
    public string? RecordedBy { get; set; }
}
