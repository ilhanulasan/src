using Dental.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Dental.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
{
    public DbSet<Patient> Patients => Set<Patient>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Patient>(entity =>
        {
            entity.ToTable("patients");
            entity.HasIndex(p => p.SocialSecurityNumber).IsUnique();
            entity.Property(p => p.Education).HasConversion<string>();
        });
    }
}
