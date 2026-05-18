using System.ComponentModel.DataAnnotations;
using Dental.Web.Models.Common;

namespace Dental.Web.Models;

public class FinancialAccount : AuditableEntity
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    public FinancialAccountType AccountType { get; set; }

    [MaxLength(64)]
    public string? AccountNumber { get; set; }

    [MaxLength(8)]
    public string Currency { get; set; } = "TRY";

    public decimal Balance { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Payment> Payments { get; set; } = [];
}
