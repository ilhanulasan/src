using System.ComponentModel.DataAnnotations;
using Dental.Web.Models.Common;

namespace Dental.Web.Models;

public class WaitlistEntry : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    public Guid? PreferredResourceId { get; set; }
    public AppointmentResource? PreferredResource { get; set; }

    public WaitlistStatus Status { get; set; } = WaitlistStatus.Active;

    public int Priority { get; set; }

    public DateTimeOffset? PreferredFrom { get; set; }
    public DateTimeOffset? PreferredTo { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }
}
