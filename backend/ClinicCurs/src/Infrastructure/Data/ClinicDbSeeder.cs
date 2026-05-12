using Application.Interfaces;
using Domain.Enums;
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
    
    public static async Task SeedSpecializationsAsync(ClinicDbContext context)
    {
        if (!await context.Specializations.AnyAsync())
        {
            var baseSpecs = new List<Specialization>
            {
                new() { Id = Guid.NewGuid(), Name = "Терапевт", Description = "Общая практика" },
                new() { Id = Guid.NewGuid(), Name = "Хирург", Description = "Оперативное лечение" },
                new() { Id = Guid.NewGuid(), Name = "Кардиолог", Description = "Сердечно-сосудистая система" },
                new() { Id = Guid.NewGuid(), Name = "Невролог", Description = "Нервная система" },
                new() { Id = Guid.NewGuid(), Name = "Офтальмолог", Description = "Зрение" },
                new() { Id = Guid.NewGuid(), Name = "Оториноларинголог", Description = "ЛОР" },
                new() { Id = Guid.NewGuid(), Name = "Дерматолог", Description = "Кожные заболевания" }
            };

            await context.Specializations.AddRangeAsync(baseSpecs);
            await context.SaveChangesAsync();
        }
    }
    
    public static async Task SeedAppointmentTypesAsync(ClinicDbContext context)
    {
        if (!await context.AppointmentTypes.AnyAsync())
        {
            var types = new List<AppointmentType>
            {
                new() { Category = AppointmentCategory.initial_consultation, DefaultDurationMinutes = 30 },
                new() { Category = AppointmentCategory.follow_up, DefaultDurationMinutes = 20 },
                new() { Category = AppointmentCategory.diagnostic, DefaultDurationMinutes = 60 },
                new() { Category = AppointmentCategory.procedure, DefaultDurationMinutes = 45 },
                new() { Category = AppointmentCategory.vaccination, DefaultDurationMinutes = 15 }
            };

            await context.AppointmentTypes.AddRangeAsync(types);
            await context.SaveChangesAsync();
        }
    }
}
