using Dental.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Dental.Web.Data;

internal static class DbContextModelConfiguration
{
    public static void ConfigureDentalModules(ModelBuilder modelBuilder)
    {
        ConfigurePatient(modelBuilder);
        ConfigurePatientSubEntities(modelBuilder);
        ConfigureClinical(modelBuilder);
        ConfigureAppointments(modelBuilder);
        ConfigureInventory(modelBuilder);
        ConfigureFinance(modelBuilder);
    }

    private static void ConfigurePatient(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Patient>(entity =>
        {
            entity.ToTable("patients");
            entity.HasIndex(p => p.SocialSecurityNumber).IsUnique();
            entity.Property(p => p.Education).HasConversion<string>();
            entity.HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<PatientOdontogram>(entity =>
        {
            entity.ToTable("patient_odontograms");
            entity.HasIndex(o => o.PatientId).IsUnique();
            entity.HasOne(o => o.Patient)
                .WithMany()
                .HasForeignKey(o => o.PatientId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigurePatientSubEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PatientMedicalHistory>(e =>
        {
            e.ToTable("patient_medical_histories");
            e.HasOne(x => x.Patient).WithMany(p => p.MedicalHistories).HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PatientClinicalNote>(e =>
        {
            e.ToTable("patient_clinical_notes");
            e.HasOne(x => x.Patient).WithMany(p => p.ClinicalNotes).HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Examination).WithMany(ex => ex.ClinicalNotes).HasForeignKey(x => x.ExaminationId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<PatientAllergy>(e =>
        {
            e.ToTable("patient_allergies");
            e.HasOne(x => x.Patient).WithMany(p => p.Allergies).HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PatientChronicCondition>(e =>
        {
            e.ToTable("patient_chronic_conditions");
            e.HasOne(x => x.Patient).WithMany(p => p.ChronicConditions).HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PatientDocument>(e =>
        {
            e.ToTable("patient_documents");
            e.Property(x => x.Category).HasConversion<string>();
            e.HasOne(x => x.Patient).WithMany(p => p.Documents).HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PatientKvkkConsent>(e =>
        {
            e.ToTable("patient_kvkk_consents");
            e.Property(x => x.ConsentType).HasConversion<string>();
            e.HasIndex(x => new { x.PatientId, x.ConsentType });
            e.HasOne(x => x.Patient).WithMany(p => p.KvkkConsents).HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureClinical(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Icd10Code>(e =>
        {
            e.ToTable("icd10_codes");
            e.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<Examination>(e =>
        {
            e.ToTable("examinations");
            e.Property(x => x.Status).HasConversion<string>();
            e.HasOne(x => x.Patient).WithMany().HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Doctor).WithMany().HasForeignKey(x => x.DoctorUserId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ExaminationDiagnosis>(e =>
        {
            e.ToTable("examination_diagnoses");
            e.HasOne(x => x.Examination).WithMany(ex => ex.Diagnoses).HasForeignKey(x => x.ExaminationId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Icd10Code).WithMany().HasForeignKey(x => x.Icd10CodeId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MedicalIntervention>(e =>
        {
            e.ToTable("medical_interventions");
            e.HasOne(x => x.Examination).WithMany(ex => ex.Interventions).HasForeignKey(x => x.ExaminationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TreatmentPlan>(e =>
        {
            e.ToTable("treatment_plans");
            e.Property(x => x.Status).HasConversion<string>();
            e.HasOne(x => x.Patient).WithMany().HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Examination).WithMany().HasForeignKey(x => x.ExaminationId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<TreatmentPlanItem>(e =>
        {
            e.ToTable("treatment_plan_items");
            e.Property(x => x.Status).HasConversion<string>();
            e.HasOne(x => x.TreatmentPlan).WithMany(p => p.Items).HasForeignKey(x => x.TreatmentPlanId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureAppointments(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppointmentResource>(e =>
        {
            e.ToTable("appointment_resources");
            e.Property(x => x.ResourceType).HasConversion<string>();
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Appointment>(e =>
        {
            e.ToTable("appointments");
            e.Property(x => x.Status).HasConversion<string>();
            e.HasIndex(x => new { x.PrimaryResourceId, x.StartAt });
            e.HasOne(x => x.Patient).WithMany().HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.PrimaryResource).WithMany(r => r.Appointments).HasForeignKey(x => x.PrimaryResourceId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.RecurringTemplate).WithMany(t => t.GeneratedAppointments)
                .HasForeignKey(x => x.RecurringTemplateId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<AppointmentResourceLink>(e =>
        {
            e.ToTable("appointment_resource_links");
            e.HasKey(x => new { x.AppointmentId, x.ResourceId });
            e.HasOne(x => x.Appointment).WithMany(a => a.AdditionalResources).HasForeignKey(x => x.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Resource).WithMany().HasForeignKey(x => x.ResourceId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<WaitlistEntry>(e =>
        {
            e.ToTable("waitlist_entries");
            e.Property(x => x.Status).HasConversion<string>();
            e.HasOne(x => x.Patient).WithMany().HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.PreferredResource).WithMany().HasForeignKey(x => x.PreferredResourceId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<RecurringAppointmentTemplate>(e =>
        {
            e.ToTable("recurring_appointment_templates");
            e.Property(x => x.Frequency).HasConversion<string>();
            e.HasOne(x => x.Patient).WithMany().HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.PrimaryResource).WithMany().HasForeignKey(x => x.PrimaryResourceId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SmsReminderLog>(e =>
        {
            e.ToTable("sms_reminder_logs");
            e.Property(x => x.Status).HasConversion<string>();
            e.HasOne(x => x.Appointment).WithMany(a => a.SmsReminders).HasForeignKey(x => x.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureInventory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Supplier>(e => e.ToTable("suppliers"));

        modelBuilder.Entity<Product>(e =>
        {
            e.ToTable("products");
            e.HasIndex(x => x.Barcode);
            e.HasIndex(x => x.Sku);
            e.HasOne(x => x.Supplier).WithMany(s => s.Products).HasForeignKey(x => x.SupplierId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<StockLot>(e =>
        {
            e.ToTable("stock_lots");
            e.HasIndex(x => x.Barcode);
            e.HasIndex(x => x.ExpiryDate);
            e.HasOne(x => x.Product).WithMany(p => p.StockLots).HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<StockMovement>(e =>
        {
            e.ToTable("stock_movements");
            e.Property(x => x.MovementType).HasConversion<string>();
            e.HasOne(x => x.StockLot).WithMany(l => l.Movements).HasForeignKey(x => x.StockLotId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureFinance(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FinancialAccount>(e =>
        {
            e.ToTable("financial_accounts");
            e.Property(x => x.AccountType).HasConversion<string>();
        });

        modelBuilder.Entity<PatientLedgerEntry>(e =>
        {
            e.ToTable("patient_ledger_entries");
            e.Property(x => x.EntryType).HasConversion<string>();
            e.HasOne(x => x.Patient).WithMany().HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Invoice).WithMany(i => i.LedgerEntries).HasForeignKey(x => x.InvoiceId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Payment).WithMany().HasForeignKey(x => x.PaymentId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Invoice>(e =>
        {
            e.ToTable("invoices");
            e.Property(x => x.Status).HasConversion<string>();
            e.HasIndex(x => x.InvoiceNumber).IsUnique();
            e.HasOne(x => x.Patient).WithMany().HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Examination).WithMany().HasForeignKey(x => x.ExaminationId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<InvoiceLine>(e =>
        {
            e.ToTable("invoice_lines");
            e.HasOne(x => x.Invoice).WithMany(i => i.Lines).HasForeignKey(x => x.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Payment>(e =>
        {
            e.ToTable("payments");
            e.Property(x => x.Method).HasConversion<string>();
            e.HasOne(x => x.Patient).WithMany().HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Invoice).WithMany(i => i.Payments).HasForeignKey(x => x.InvoiceId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.FinancialAccount).WithMany(a => a.Payments).HasForeignKey(x => x.FinancialAccountId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.InstallmentPlan).WithMany(p => p.Payments).HasForeignKey(x => x.InstallmentPlanId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<PaymentInstallmentPlan>(e =>
        {
            e.ToTable("payment_installment_plans");
            e.HasOne(x => x.Patient).WithMany().HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Invoice).WithMany().HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<PaymentInstallment>(e =>
        {
            e.ToTable("payment_installments");
            e.HasOne(x => x.Plan).WithMany(p => p.Installments).HasForeignKey(x => x.PlanId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Payment).WithMany().HasForeignKey(x => x.PaymentId).OnDelete(DeleteBehavior.SetNull);
        });
    }
}
