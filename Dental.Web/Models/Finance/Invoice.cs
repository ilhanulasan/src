using System.ComponentModel.DataAnnotations;
using Dental.Web.Models.Common;

namespace Dental.Web.Models;

public class Invoice : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    [Required]
    [MaxLength(32)]
    public string InvoiceNumber { get; set; } = string.Empty;

    public DateOnly IssueDate { get; set; }
    public DateOnly? DueDate { get; set; }

    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public Guid? ExaminationId { get; set; }
    public Examination? Examination { get; set; }

    public ICollection<InvoiceLine> Lines { get; set; } = [];
    public ICollection<PatientLedgerEntry> LedgerEntries { get; set; } = [];
    public ICollection<Payment> Payments { get; set; } = [];
}
