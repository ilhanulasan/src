using Dental.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Dental.Web.Data;

internal static class ClinicSeedData
{
    public static async Task SeedAsync(ApplicationDbContext db, CancellationToken ct = default)
    {
        if (!await db.AppointmentResources.AnyAsync(ct))
        {
            db.AppointmentResources.AddRange(
                new AppointmentResource
                {
                    Id = Guid.Parse("b2000001-0000-4000-8000-000000000001"),
                    Name = "Dr. Klinik",
                    ResourceType = AppointmentResourceType.Doctor,
                    DefaultDurationMinutes = 30,
                    Color = "#0f766e",
                    IsActive = true,
                },
                new AppointmentResource
                {
                    Id = Guid.Parse("b2000001-0000-4000-8000-000000000002"),
                    Name = "Muayene Odası 1",
                    ResourceType = AppointmentResourceType.Room,
                    DefaultDurationMinutes = 30,
                    Color = "#2563eb",
                    IsActive = true,
                });

            await db.SaveChangesAsync(ct);
        }

        if (!await db.FinancialAccounts.AnyAsync(ct))
        {
            db.FinancialAccounts.Add(
                new FinancialAccount
                {
                    Id = Guid.Parse("b2000002-0000-4000-8000-000000000001"),
                    Name = "Kasa",
                    AccountType = FinancialAccountType.Cash,
                    Currency = "TRY",
                    Balance = 0,
                    IsActive = true,
                });

            await db.SaveChangesAsync(ct);
        }
    }
}
