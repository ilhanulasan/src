using System.ComponentModel.DataAnnotations;
using Dental.Web.Models.Common;

namespace Dental.Web.Models;

public class Supplier : AuditableEntity
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(256)]
    public string CompanyName { get; set; } = string.Empty;

    [MaxLength(128)]
    public string? ContactPerson { get; set; }

    [MaxLength(64)]
    public string? Phone { get; set; }

    [MaxLength(256)]
    public string? Email { get; set; }

    [MaxLength(512)]
    public string? Address { get; set; }

    [MaxLength(64)]
    public string? TaxNumber { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Product> Products { get; set; } = [];
}
