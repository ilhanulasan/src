using System.ComponentModel.DataAnnotations;

namespace Dental.Web.Models;

public class PersonnelDto
{
    public Guid Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Notes { get; set; }
    public PersonnelType PersonnelType { get; set; }
    public IReadOnlyList<DentalSpecialty> Specialties { get; set; } = [];
    public string? UserId { get; set; }
    public Guid? AppointmentResourceId { get; set; }
    public bool IsActive { get; set; }
}

public class CreatePersonnelDto
{
    [Required]
    [MaxLength(128)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(128)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(256)]
    [EmailAddress]
    public string? Email { get; set; }

    [MaxLength(32)]
    public string? Phone { get; set; }

    [MaxLength(512)]
    public string? Notes { get; set; }

    [Required]
    public PersonnelType PersonnelType { get; set; }

    public IList<DentalSpecialty> Specialties { get; set; } = [];

    public bool IsActive { get; set; } = true;
}

public class UpdatePersonnelDto : CreatePersonnelDto
{
    [Required]
    public Guid Id { get; set; }
}

public class DoctorAppointmentOptionDto
{
    public Guid PersonnelId { get; set; }
    public Guid ResourceId { get; set; }
    public required string DisplayName { get; set; }
    public IReadOnlyList<DentalSpecialty> Specialties { get; set; } = [];
}
