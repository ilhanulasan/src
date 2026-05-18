using System.ComponentModel.DataAnnotations;
using Dental.Web.Models.Common;

namespace Dental.Web.Models;

public class Payment : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    public Guid? InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }

    public Guid FinancialAccountId { get; set; }
    public FinancialAccount FinancialAccount { get; set; } = null!;

    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }

    public DateTimeOffset PaidAt { get; set; } = DateTimeOffset.UtcNow;

    [MaxLength(512)]
    public string? Reference { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public Guid? InstallmentPlanId { get; set; }
    public PaymentInstallmentPlan? InstallmentPlan { get; set; }
}
