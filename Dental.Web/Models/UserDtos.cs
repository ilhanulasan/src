using System.ComponentModel.DataAnnotations;

namespace Dental.Web.Models;

public class CreateUserDto
{
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [MaxLength(128)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(128)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(32)]
    public string? PhoneNumber { get; set; }

    [MaxLength(512)]
    public string? Address { get; set; }

    [Required]
    [MinLength(1)]
    public IList<string> Roles { get; set; } = [];
}

public class UpdateUserDto
{
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(128)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(128)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(32)]
    public string? PhoneNumber { get; set; }

    [MaxLength(512)]
    public string? Address { get; set; }

    [Required]
    [MinLength(1)]
    public IList<string> Roles { get; set; } = [];
}

public class ResetUserPasswordDto
{
    [Required]
    [MinLength(8)]
    public string NewPassword { get; set; } = string.Empty;
}
