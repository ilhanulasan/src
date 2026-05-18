using System.ComponentModel.DataAnnotations;
using Dental.Web.Models.Common;

namespace Dental.Web.Models;

public class SmsReminderLog : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid AppointmentId { get; set; }
    public Appointment Appointment { get; set; } = null!;

    [Required]
    [MaxLength(32)]
    public string PhoneNumber { get; set; } = string.Empty;

    [MaxLength(512)]
    public string Message { get; set; } = string.Empty;

    public DateTimeOffset ScheduledFor { get; set; }
    public DateTimeOffset? SentAt { get; set; }

    public SmsReminderStatus Status { get; set; } = SmsReminderStatus.Pending;

    [MaxLength(512)]
    public string? ProviderResponse { get; set; }
}
