using Dental.Web.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Dental.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<PatientOdontogram> PatientOdontograms => Set<PatientOdontogram>();
    public DbSet<PatientMedicalHistory> PatientMedicalHistories => Set<PatientMedicalHistory>();
    public DbSet<PatientClinicalNote> PatientClinicalNotes => Set<PatientClinicalNote>();
    public DbSet<PatientAllergy> PatientAllergies => Set<PatientAllergy>();
    public DbSet<PatientChronicCondition> PatientChronicConditions => Set<PatientChronicCondition>();
    public DbSet<PatientDocument> PatientDocuments => Set<PatientDocument>();
    public DbSet<PatientKvkkConsent> PatientKvkkConsents => Set<PatientKvkkConsent>();

    public DbSet<Icd10Code> Icd10Codes => Set<Icd10Code>();
    public DbSet<Examination> Examinations => Set<Examination>();
    public DbSet<ExaminationDiagnosis> ExaminationDiagnoses => Set<ExaminationDiagnosis>();
    public DbSet<MedicalIntervention> MedicalInterventions => Set<MedicalIntervention>();
    public DbSet<TreatmentPlan> TreatmentPlans => Set<TreatmentPlan>();
    public DbSet<TreatmentPlanItem> TreatmentPlanItems => Set<TreatmentPlanItem>();

    public DbSet<AppointmentResource> AppointmentResources => Set<AppointmentResource>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<AppointmentResourceLink> AppointmentResourceLinks => Set<AppointmentResourceLink>();
    public DbSet<WaitlistEntry> WaitlistEntries => Set<WaitlistEntry>();
    public DbSet<RecurringAppointmentTemplate> RecurringAppointmentTemplates => Set<RecurringAppointmentTemplate>();
    public DbSet<SmsReminderLog> SmsReminderLogs => Set<SmsReminderLog>();

    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<StockLot> StockLots => Set<StockLot>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

    public DbSet<FinancialAccount> FinancialAccounts => Set<FinancialAccount>();
    public DbSet<PatientLedgerEntry> PatientLedgerEntries => Set<PatientLedgerEntry>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentInstallmentPlan> PaymentInstallmentPlans => Set<PaymentInstallmentPlan>();
    public DbSet<PaymentInstallment> PaymentInstallments => Set<PaymentInstallment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        DbContextModelConfiguration.ConfigureDentalModules(modelBuilder);
        Icd10SeedData.Seed(modelBuilder);
    }
}
