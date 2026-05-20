using System.ComponentModel.DataAnnotations;
using Dental.Web.Models.Common;

namespace Dental.Web.Models;

public class ClinicPersonnel : AuditableEntity
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(128)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(128)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(256)]
    public string? Email { get; set; }

    [MaxLength(32)]
    public string? Phone { get; set; }

    [MaxLength(512)]
    public string? Notes { get; set; }

    public PersonnelType PersonnelType { get; set; }

    public List<DentalSpecialty> Specialties { get; set; } = [];

    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public Guid? AppointmentResourceId { get; set; }
    public AppointmentResource? AppointmentResource { get; set; }

    public bool IsActive { get; set; } = true;
}
