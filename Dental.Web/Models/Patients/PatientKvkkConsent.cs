using System.ComponentModel.DataAnnotations;
using Dental.Web.Models.Common;

namespace Dental.Web.Models;

public class PatientKvkkConsent : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    public KvkkConsentType ConsentType { get; set; }

    public bool IsGranted { get; set; }

    public DateTimeOffset ConsentedAt { get; set; } = DateTimeOffset.UtcNow;

    [MaxLength(45)]
    public string? IpAddress { get; set; }

    [MaxLength(512)]
    public string? UserAgent { get; set; }

    [MaxLength(32)]
    public string? ConsentVersion { get; set; }

    public DateTimeOffset? RevokedAt { get; set; }
}
