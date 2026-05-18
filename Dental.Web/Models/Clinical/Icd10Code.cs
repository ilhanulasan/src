using System.ComponentModel.DataAnnotations;

namespace Dental.Web.Models;

public class Icd10Code
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(16)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(512)]
    public string DescriptionTr { get; set; } = string.Empty;

    [MaxLength(512)]
    public string? DescriptionEn { get; set; }

    [MaxLength(128)]
    public string? Category { get; set; }

    public bool IsActive { get; set; } = true;
}
