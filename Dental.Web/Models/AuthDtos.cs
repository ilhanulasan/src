using System.ComponentModel.DataAnnotations;

namespace Dental.Web.Models;

public class RegisterDto
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

    [Required]
    [MaxLength(32)]
    public string PhoneNumber { get; set; } = string.Empty;

    [MaxLength(512)]
    public string? Address { get; set; }

    /// <summary>Optional data URL (image/jpeg, image/png, image/webp) for profile picture.</summary>
    [MaxLength(1_500_000)]
    public string? PictureData { get; set; }
}

public class LoginDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public required string Token { get; set; }
    public required UserProfileDto User { get; set; }
}

public class UserProfileDto
{
    public required string Id { get; set; }
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? PictureUrl { get; set; }
    public required IReadOnlyList<string> Roles { get; set; }
}
