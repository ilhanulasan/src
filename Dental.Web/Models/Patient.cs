using System.ComponentModel.DataAnnotations;

namespace Dental.Web.Models;

public class Patient
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(128)]
    public string Surname { get; set; } = string.Empty;

    [Required]
    [MaxLength(32)]
    public string SocialSecurityNumber { get; set; } = string.Empty;

    [MaxLength(512)]
    public string? Address { get; set; }

    [MaxLength(64)]
    public string? Phone { get; set; }

    public DateOnly DateOfBirth { get; set; }

    [Required]
    [MaxLength(32)]
    public string Gender { get; set; } = string.Empty;

    public EducationLevel Education { get; set; }
}
