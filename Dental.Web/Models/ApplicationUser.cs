using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Dental.Web.Models;

public class ApplicationUser : IdentityUser
{
    [Required]
    [MaxLength(128)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(128)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(512)]
    public string? Address { get; set; }

    [MaxLength(2048)]
    public string? PictureUrl { get; set; }
}
