using System.ComponentModel.DataAnnotations;
using Dental.Web.Models.Common;

namespace Dental.Web.Models;

public class RecurringAppointmentTemplate : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    public Guid PrimaryResourceId { get; set; }
    public AppointmentResource PrimaryResource { get; set; } = null!;

    public RecurrenceFrequency Frequency { get; set; }

    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }

    public TimeOnly StartTime { get; set; }
    public int DurationMinutes { get; set; } = 30;

    public bool IsActive { get; set; } = true;

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public ICollection<Appointment> GeneratedAppointments { get; set; } = [];
}
