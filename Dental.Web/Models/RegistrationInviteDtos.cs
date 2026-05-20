using System.ComponentModel.DataAnnotations;

namespace Dental.Web.Models;

public class RegistrationInviteInfoDto
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public string? Phone { get; set; }
}

public class CompleteRegistrationInviteDto
{
    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [MaxLength(256)]
    [EmailAddress]
    public string? Email { get; set; }

    [MaxLength(32)]
    public string? Phone { get; set; }
}
