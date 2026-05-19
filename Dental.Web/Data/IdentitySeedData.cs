using Dental.Web.Models;
using Microsoft.AspNetCore.Identity;

namespace Dental.Web.Data;

public static class IdentitySeedData
{
    public const string DefaultAdminEmail = "admin@gmail.com";
    public const string DefaultAdminPassword = "Password123";

    public static async Task SeedDefaultAdminAsync(
        UserManager<ApplicationUser> userManager,
        bool resetCredentialsInDevelopment)
    {
        var existing = await userManager.FindByEmailAsync(DefaultAdminEmail);
        if (existing is null)
        {
            var user = new ApplicationUser
            {
                UserName = DefaultAdminEmail,
                Email = DefaultAdminEmail,
                FirstName = "Admin",
                LastName = "User",
                PhoneNumber = "0000000000",
                EmailConfirmed = true,
            };

            var create = await userManager.CreateAsync(user, DefaultAdminPassword);
            if (!create.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Could not seed default admin user: {string.Join("; ", create.Errors.Select(e => e.Description))}");
            }

            await EnsureAdminRoleAsync(userManager, user);
            return;
        }

        await EnsureAdminRoleAsync(userManager, existing);

        if (!resetCredentialsInDevelopment)
        {
            return;
        }

        await UnlockAsync(userManager, existing);

        if (await userManager.CheckPasswordAsync(existing, DefaultAdminPassword))
        {
            return;
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(existing);
        var reset = await userManager.ResetPasswordAsync(existing, token, DefaultAdminPassword);
        if (!reset.Succeeded)
        {
            throw new InvalidOperationException(
                $"Could not reset default admin password: {string.Join("; ", reset.Errors.Select(e => e.Description))}");
        }
    }

    private static async Task EnsureAdminRoleAsync(UserManager<ApplicationUser> userManager, ApplicationUser user)
    {
        if (!await userManager.IsInRoleAsync(user, AppRoles.Admin))
        {
            var roleAdd = await userManager.AddToRoleAsync(user, AppRoles.Admin);
            if (!roleAdd.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Could not assign Admin role to default user: {string.Join("; ", roleAdd.Errors.Select(e => e.Description))}");
            }
        }
    }

    private static async Task UnlockAsync(UserManager<ApplicationUser> userManager, ApplicationUser user)
    {
        await userManager.SetLockoutEndDateAsync(user, null);
        await userManager.ResetAccessFailedCountAsync(user);
    }
}
