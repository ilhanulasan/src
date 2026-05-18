namespace Dental.Web.Models;

public class AppointmentResourceLink
{
    public Guid AppointmentId { get; set; }
    public Appointment Appointment { get; set; } = null!;

    public Guid ResourceId { get; set; }
    public AppointmentResource Resource { get; set; } = null!;
}
