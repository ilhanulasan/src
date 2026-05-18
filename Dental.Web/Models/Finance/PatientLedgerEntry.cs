using System.ComponentModel.DataAnnotations;
using Dental.Web.Models.Common;

namespace Dental.Web.Models;

public class PatientLedgerEntry : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    public LedgerEntryType EntryType { get; set; }

    public decimal Amount { get; set; }

    public DateTimeOffset EntryDate { get; set; } = DateTimeOffset.UtcNow;

    [MaxLength(512)]
    public string Description { get; set; } = string.Empty;

    public Guid? InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }

    public Guid? PaymentId { get; set; }
    public Payment? Payment { get; set; }
}
