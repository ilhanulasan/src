using System.ComponentModel.DataAnnotations;
using Dental.Web.Models.Common;

namespace Dental.Web.Models;

public class PaymentInstallmentPlan : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    public Guid? InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }

    public decimal TotalAmount { get; set; }
    public int InstallmentCount { get; set; }

    [MaxLength(512)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<PaymentInstallment> Installments { get; set; } = [];
    public ICollection<Payment> Payments { get; set; } = [];
}
