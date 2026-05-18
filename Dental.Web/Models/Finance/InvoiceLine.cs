using System.ComponentModel.DataAnnotations;

namespace Dental.Web.Models;

public class InvoiceLine
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;

    [Required]
    [MaxLength(256)]
    public string Description { get; set; } = string.Empty;

    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; }
    public decimal LineTotal { get; set; }
}
