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

    public static async Task SeedOfficesAndDoctorsAsync(ClinicDbContext context, IPasswordHasher hasher)
    {
        var office = await context.Offices.FirstOrDefaultAsync();
        if (office == null)
        {
            office = new Office
            {
                Id = Guid.NewGuid(),
                Name = "Главный корпус",
                Address = "ул. Медицинская, д. 1",
                Phone = "+375 (29) 111-22-33",
                IsActive = true,
                PhotoUrl = "default_office.png"
            };
            await context.Offices.AddAsync(office);
            await context.SaveChangesAsync();
        }

        if (!await context.Accounts.AnyAsync(a => a.Role == RoleType.doctor))
        {
            var therapistSpec = await context.Specializations.FirstOrDefaultAsync(s => s.Name == "Терапевт");
            var surgeonSpec = await context.Specializations.FirstOrDefaultAsync(s => s.Name == "Хирург");

            if (therapistSpec == null || surgeonSpec == null) return; // Защита от ошибок

            var doc1Account = new Account
            {
                Email = "smirnova@clinic.com",
                PasswordHash = hasher.Hash("doc_pass"),
                Role = RoleType.doctor,
                Phone = "+375290000001",
                EmailVerified = true,
                PhoneVerified = true,
                IdentityVerified = true
            };
            await context.Accounts.AddAsync(doc1Account);

            var doc2Account = new Account
            {
                Email = "sokolov@clinic.com",
                PasswordHash = hasher.Hash("doc_pass"),
                Role = RoleType.doctor,
                Phone = "+375290000002",
                EmailVerified = true,
                PhoneVerified = true,
                IdentityVerified = true
            };
            await context.Accounts.AddAsync(doc2Account);

            await context.SaveChangesAsync();

            var doc1 = new Doctor
            {
                AccountId = doc1Account.Id,
                OfficeId = office.Id,
                FirstName = "Анна",
                LastName = "Смирнова",
                MiddleName = "Ивановна",
                Bio =
                    "Врач-терапевт высшей категории с заботливым подходом к каждому пациенту. Специализируется на сложных диагностических случаях.",
                HiredAt = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-3)),
                AvatarUrl = "default_doctor.png",
                RatingAvg = 4.9m,
                IsActive = true
            };

            var doc2 = new Doctor
            {
                AccountId = doc2Account.Id,
                OfficeId = office.Id,
                FirstName = "Дмитрий",
                LastName = "Соколов",
                MiddleName = "Петрович",
                Bio =
                    "Ведущий хирург клиники. Выполняет малоинвазивные операции. Стажировался в ведущих клиниках Европы.",
                HiredAt = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-5)),
                AvatarUrl = "default_doctor.png",
                RatingAvg = 5.0m,
                IsActive = true
            };

            await context.Doctors.AddRangeAsync(doc1, doc2);
            await context.SaveChangesAsync();

            await context.DoctorSpecializations.AddRangeAsync(
                new DoctorSpecialization
                {
                    DoctorId = doc1.Id, SpecializationId = therapistSpec.Id, IsPrimary = true,
                    CareerStartDate = new DateOnly(2012, 8, 1)
                },
                new DoctorSpecialization
                {
                    DoctorId = doc2.Id, SpecializationId = surgeonSpec.Id, IsPrimary = true,
                    CareerStartDate = new DateOnly(2008, 9, 1)
                }
            );

            await context.SaveChangesAsync();
        }
    }

    public static async Task SeedRegistrarsAsync(ClinicDbContext context, IPasswordHasher hasher)
    {
        if (!await context.Accounts.AnyAsync(a => a.Role == RoleType.registrar))
        {
            var office = await context.Offices.FirstOrDefaultAsync();
            if (office == null) return;

            var regAccount = new Account
            {
                Email = "registrar@clinic.com",
                PasswordHash = hasher.Hash("reg_pass"),
                Role = RoleType.registrar,
                Phone = "+375291234567",
                EmailVerified = true,
                PhoneVerified = true,
                IdentityVerified = true
            };

            await context.Accounts.AddAsync(regAccount);
            await context.SaveChangesAsync();

            var registrar = new Registrar
            {
                AccountId = regAccount.Id,
                OfficeId = office.Id,
                FirstName = "Елена",
                LastName = "Иванова",
                MiddleName = "Сергеевна",
                HiredAt = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
                AvatarUrl = "default_staff.png",
                IsActive = true
            };

            await context.Registrars.AddAsync(registrar);
            await context.SaveChangesAsync();
        }
    }

    public static async Task SeedLabTestsAsync(ClinicDbContext context)
    {
        if (!await context.LabTestsDictionaries.AnyAsync())
        {
            await context.LabTestsDictionaries.AddRangeAsync(
                new LabTestsDictionary
                {
                    Id = Guid.NewGuid(), Name = "Общий анализ крови (ОАК)", Description = "Базовое исследование крови"
                },
                new LabTestsDictionary
                {
                    Id = Guid.NewGuid(), Name = "Общий анализ мочи (ОАМ)", Description = "Базовое исследование мочи"
                },
                new LabTestsDictionary
                {
                    Id = Guid.NewGuid(), Name = "Биохимический анализ крови",
                    Description = "Развернутое исследование крови"
                },
                new LabTestsDictionary
                    { Id = Guid.NewGuid(), Name = "Рентген грудной клетки", Description = "Снимок легких" },
                new LabTestsDictionary
                {
                    Id = Guid.NewGuid(), Name = "МРТ головного мозга", Description = "Магнитно-резонансная томография"
                }
            );
            await context.SaveChangesAsync();
        }
    }
}
