using System.ComponentModel.DataAnnotations;
using Dental.Web.Models.Common;

namespace Dental.Web.Models;

public class Appointment : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    public Guid PrimaryResourceId { get; set; }
    public AppointmentResource PrimaryResource { get; set; } = null!;

    public DateTimeOffset StartAt { get; set; }
    public DateTimeOffset EndAt { get; set; }

    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public bool IsOnlineBooking { get; set; }

    public Guid? RecurringTemplateId { get; set; }
    public RecurringAppointmentTemplate? RecurringTemplate { get; set; }

    public DateTimeOffset? ConfirmedAt { get; set; }
    public DateTimeOffset? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }

    [MaxLength(64)]
    public string? GuestConfirmationToken { get; set; }

    public ICollection<AppointmentResourceLink> AdditionalResources { get; set; } = [];
    public ICollection<SmsReminderLog> SmsReminders { get; set; } = [];
}
