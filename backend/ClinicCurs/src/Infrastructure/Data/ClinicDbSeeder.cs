using Application.Interfaces;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public static class ClinicDbSeeder
{
    public static async Task SeedAdminAsync(ClinicDbContext context, IPasswordHasher passwordHasher)
    {
        var adminEmail = "admin@clinic.com";

        if (!await context.Accounts.AnyAsync(a => a.Email == adminEmail))
        {
            var adminAccount = new Account
            {
                Email = adminEmail,
                PasswordHash = passwordHasher.Hash("admin_admin"), 
                Phone = "375447778899",
                EmailVerified = true,
                PhoneVerified = true,
                IdentityVerified = true,
                Role = RoleType.admin,
                IsDeleted = false
            };

            await context.Accounts.AddAsync(adminAccount);
            await context.SaveChangesAsync(); 

            var adminProfile = new Admin
            {
                AccountId = adminAccount.Id,
                FirstName = "Артем",
                LastName = "Валинский",
                MiddleName = "Станиславович",
                CreatedAt = DateTime.UtcNow
            };

            await context.Admins.AddAsync(adminProfile);
            await context.SaveChangesAsync();
        }
    }
}
