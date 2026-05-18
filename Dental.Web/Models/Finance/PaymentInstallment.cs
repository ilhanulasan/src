using Dental.Web.Models.Common;

namespace Dental.Web.Models;

public class PaymentInstallment : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid PlanId { get; set; }
    public PaymentInstallmentPlan Plan { get; set; } = null!;

    public int InstallmentNumber { get; set; }
    public decimal Amount { get; set; }
    public DateOnly DueDate { get; set; }
    public bool IsPaid { get; set; }
    public DateTimeOffset? PaidAt { get; set; }

    public Guid? PaymentId { get; set; }
    public Payment? Payment { get; set; }
}
