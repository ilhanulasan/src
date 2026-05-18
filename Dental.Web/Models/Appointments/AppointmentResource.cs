using System.ComponentModel.DataAnnotations;
using Dental.Web.Models.Common;

namespace Dental.Web.Models;

public class AppointmentResource : AuditableEntity
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    public AppointmentResourceType ResourceType { get; set; }

    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }

    [MaxLength(512)]
    public string? Description { get; set; }

    public int DefaultDurationMinutes { get; set; } = 30;

    public string? Color { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Appointment> Appointments { get; set; } = [];
}
