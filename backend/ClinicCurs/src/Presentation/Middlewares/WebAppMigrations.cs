using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Presentation.Middlewares;

public static class WebAppMigrations
{
    public static async Task Migrations(this WebApplication app)
    {
        // return;

        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<ClinicDbContext>();
                var passwordHasher = services.GetRequiredService<IPasswordHasher>();

                await context.Database.MigrateAsync();

                await ClinicDbSeeder.SeedSpecializationsAsync(context);
                await ClinicDbSeeder.SeedAppointmentTypesAsync(context);
                await ClinicDbSeeder.SeedOfficesAndDoctorsAsync(context, passwordHasher);
                await ClinicDbSeeder.SeedAdminAsync(context, passwordHasher);
                await ClinicDbSeeder.SeedRegistrarsAsync(context, passwordHasher);
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "Ошибка при инициализации базы данных.");
            }
        }
    }
}
