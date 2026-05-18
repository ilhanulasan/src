using System.ComponentModel.DataAnnotations;
using Dental.Web.Models.Common;

namespace Dental.Web.Models;

public class PatientAllergy : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    [Required]
    [MaxLength(256)]
    public string Substance { get; set; } = string.Empty;

    [MaxLength(32)]
    public string? Severity { get; set; }

    [MaxLength(2000)]
    public string? Reaction { get; set; }

    public bool IsActive { get; set; } = true;
}
