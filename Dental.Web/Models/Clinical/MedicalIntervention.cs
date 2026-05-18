using System.ComponentModel.DataAnnotations;
using Dental.Web.Models.Common;

namespace Dental.Web.Models;

public class MedicalIntervention : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid ExaminationId { get; set; }
    public Examination Examination { get; set; } = null!;

    [Required]
    [MaxLength(256)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? Description { get; set; }

    [MaxLength(128)]
    public string? ToothNumbers { get; set; }

    public DateTimeOffset PerformedAt { get; set; } = DateTimeOffset.UtcNow;

    public string? PerformedByUserId { get; set; }
}
