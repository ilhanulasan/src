using Dental.Web.Data;
using Dental.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dental.Web.Controllers.Api;

[ApiController]
[Route("api/financial-accounts")]
public class FinancialAccountsController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FinancialAccount>>> List(CancellationToken ct) =>
        Ok(await db.FinancialAccounts.AsNoTracking().Where(a => a.IsActive).OrderBy(a => a.Name).ToListAsync(ct));

    [HttpPost]
    public async Task<ActionResult<FinancialAccount>> Create([FromBody] FinancialAccount input, CancellationToken ct)
    {
        input.Id = Guid.NewGuid();
        db.FinancialAccounts.Add(input);
        await db.SaveChangesAsync(ct);
        return Ok(input);
    }
}

[ApiController]
[Route("api/invoices")]
public class InvoicesController(ApplicationDbContext db, ILogger<InvoicesController> log) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Invoice>>> List([FromQuery] Guid? patientId, CancellationToken ct)
    {
        var q = db.Invoices.AsNoTracking().Include(i => i.Lines).AsQueryable();
        if (patientId.HasValue) q = q.Where(i => i.PatientId == patientId);
        return Ok(await q.OrderByDescending(i => i.IssueDate).ToListAsync(ct));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Invoice>> Get(Guid id, CancellationToken ct)
    {
        var inv = await db.Invoices.AsNoTracking().Include(i => i.Lines).FirstOrDefaultAsync(i => i.Id == id, ct);
        return inv is null ? NotFound() : Ok(inv);
    }

    [HttpPost]
    public async Task<ActionResult<Invoice>> Create([FromBody] Invoice input, CancellationToken ct)
    {
        input.Id = Guid.NewGuid();
        if (string.IsNullOrWhiteSpace(input.InvoiceNumber))
        {
            input.InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
        }

        foreach (var line in input.Lines)
        {
            line.Id = Guid.NewGuid();
            line.InvoiceId = input.Id;
            line.LineTotal = line.Quantity * line.UnitPrice * (1 + line.TaxRate / 100m);
        }

        input.Subtotal = input.Lines.Sum(l => l.Quantity * l.UnitPrice);
        input.TaxAmount = input.Lines.Sum(l => l.LineTotal - l.Quantity * l.UnitPrice);
        input.TotalAmount = input.Lines.Sum(l => l.LineTotal);
        input.PaidAmount = 0;
        input.Status = InvoiceStatus.Issued;

        db.Invoices.Add(input);
        db.PatientLedgerEntries.Add(new PatientLedgerEntry
        {
            Id = Guid.NewGuid(),
            PatientId = input.PatientId,
            EntryType = LedgerEntryType.Charge,
            Amount = input.TotalAmount,
            Description = $"Fatura {input.InvoiceNumber}",
            InvoiceId = input.Id,
            EntryDate = DateTimeOffset.UtcNow,
        });

        await db.SaveChangesAsync(ct);
        log.LogInformation("Issued invoice {Number}", input.InvoiceNumber);
        return CreatedAtAction(nameof(Get), new { id = input.Id }, input);
    }
}

[ApiController]
[Route("api/payments")]
public class PaymentsController(ApplicationDbContext db, ILogger<PaymentsController> log) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Payment>>> List([FromQuery] Guid? patientId, CancellationToken ct)
    {
        var q = db.Payments.AsNoTracking().Include(p => p.FinancialAccount).AsQueryable();
        if (patientId.HasValue) q = q.Where(p => p.PatientId == patientId);
        return Ok(await q.OrderByDescending(p => p.PaidAt).ToListAsync(ct));
    }

    [HttpPost]
    public async Task<ActionResult<Payment>> Create([FromBody] Payment input, CancellationToken ct)
    {
        input.Id = Guid.NewGuid();
        input.PaidAt = input.PaidAt == default ? DateTimeOffset.UtcNow : input.PaidAt;

        var account = await db.FinancialAccounts.FindAsync([input.FinancialAccountId], ct);
        if (account is null) return BadRequest("Financial account not found.");

        account.Balance += input.Amount;
        db.Payments.Add(input);
        db.PatientLedgerEntries.Add(new PatientLedgerEntry
        {
            Id = Guid.NewGuid(),
            PatientId = input.PatientId,
            EntryType = LedgerEntryType.Payment,
            Amount = input.Amount,
            Description = input.Notes ?? "Tahsilat",
            PaymentId = input.Id,
            InvoiceId = input.InvoiceId,
            EntryDate = input.PaidAt,
        });

        if (input.InvoiceId.HasValue)
        {
            var invoice = await db.Invoices.FindAsync([input.InvoiceId.Value], ct);
            if (invoice is not null)
            {
                invoice.PaidAmount += input.Amount;
                invoice.Status = invoice.PaidAmount >= invoice.TotalAmount
                    ? InvoiceStatus.Paid
                    : InvoiceStatus.PartiallyPaid;
            }
        }

        await db.SaveChangesAsync(ct);
        log.LogInformation("Recorded payment {Id} amount {Amount}", input.Id, input.Amount);
        return Ok(input);
    }
}

[ApiController]
[Route("api/installment-plans")]
public class InstallmentPlansController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PaymentInstallmentPlan>>> List([FromQuery] Guid? patientId, CancellationToken ct)
    {
        var q = db.PaymentInstallmentPlans.AsNoTracking().Include(p => p.Installments).AsQueryable();
        if (patientId.HasValue) q = q.Where(p => p.PatientId == patientId);
        return Ok(await q.OrderByDescending(p => p.CreatedAt).ToListAsync(ct));
    }

    [HttpPost]
    public async Task<ActionResult<PaymentInstallmentPlan>> Create([FromBody] CreateInstallmentPlanRequest request, CancellationToken ct)
    {
        var plan = new PaymentInstallmentPlan
        {
            Id = Guid.NewGuid(),
            PatientId = request.PatientId,
            InvoiceId = request.InvoiceId,
            TotalAmount = request.TotalAmount,
            InstallmentCount = request.InstallmentCount,
            Description = request.Description,
            IsActive = true,
        };

        var perInstallment = Math.Round(request.TotalAmount / request.InstallmentCount, 2);
        var remainder = request.TotalAmount - perInstallment * request.InstallmentCount;

        for (var i = 0; i < request.InstallmentCount; i++)
        {
            var amount = perInstallment + (i == 0 ? remainder : 0);
            plan.Installments.Add(new PaymentInstallment
            {
                Id = Guid.NewGuid(),
                PlanId = plan.Id,
                InstallmentNumber = i + 1,
                Amount = amount,
                DueDate = request.FirstDueDate.AddMonths(i),
            });
        }

        db.PaymentInstallmentPlans.Add(plan);
        await db.SaveChangesAsync(ct);
        return Ok(plan);
    }

    [HttpPost("{planId:guid}/installments/{installmentId:guid}/pay")]
    public async Task<ActionResult<Payment>> PayInstallment(
        Guid planId,
        Guid installmentId,
        [FromBody] PayInstallmentRequest request,
        CancellationToken ct)
    {
        var installment = await db.PaymentInstallments
            .Include(i => i.Plan)
            .FirstOrDefaultAsync(i => i.Id == installmentId && i.PlanId == planId, ct);
        if (installment is null || installment.IsPaid) return BadRequest();

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            PatientId = installment.Plan.PatientId,
            InvoiceId = installment.Plan.InvoiceId,
            FinancialAccountId = request.FinancialAccountId,
            Amount = installment.Amount,
            Method = request.Method,
            InstallmentPlanId = planId,
            PaidAt = DateTimeOffset.UtcNow,
            Notes = $"Taksit #{installment.InstallmentNumber}",
        };

        var account = await db.FinancialAccounts.FindAsync([request.FinancialAccountId], ct);
        if (account is null) return BadRequest("Account not found.");
        account.Balance += payment.Amount;

        installment.IsPaid = true;
        installment.PaidAt = payment.PaidAt;
        installment.PaymentId = payment.Id;

        db.Payments.Add(payment);
        db.PatientLedgerEntries.Add(new PatientLedgerEntry
        {
            Id = Guid.NewGuid(),
            PatientId = payment.PatientId,
            EntryType = LedgerEntryType.Payment,
            Amount = payment.Amount,
            Description = payment.Notes ?? "Taksit ödemesi",
            PaymentId = payment.Id,
            InvoiceId = payment.InvoiceId,
            EntryDate = payment.PaidAt,
        });

        await db.SaveChangesAsync(ct);
        return Ok(payment);
    }
}

public record CreateInstallmentPlanRequest(
    Guid PatientId,
    Guid? InvoiceId,
    decimal TotalAmount,
    int InstallmentCount,
    DateOnly FirstDueDate,
    string? Description);

public record PayInstallmentRequest(Guid FinancialAccountId, PaymentMethod Method);
