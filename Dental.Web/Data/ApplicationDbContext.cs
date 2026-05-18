using Dental.Web.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Dental.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<PatientOdontogram> PatientOdontograms => Set<PatientOdontogram>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.ToTable("patients");
            entity.HasIndex(p => p.SocialSecurityNumber).IsUnique();
            entity.Property(p => p.Education).HasConversion<string>();
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
}
