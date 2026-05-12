using Application.Interfaces;
using Application.Interfaces.Repositories;
using ClinicCurs.Infrastructure.Data;
using Domain.Enums;
using Infrastructure.Auth;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtProvider, JwtProvider>();
        services.AddScoped<IEmailService, MailKitEmailService>();
        
        var connectionString = configuration.GetConnectionString("Postgres");
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);

        dataSourceBuilder.MapEnum<RoleType>();
        dataSourceBuilder.MapEnum<Gender>();
        dataSourceBuilder.MapEnum<VerificationStatuses>();
        dataSourceBuilder.MapEnum<AppointmentStatuses>();
        dataSourceBuilder.MapEnum<AppointmentCategory>();
        dataSourceBuilder.MapEnum<LabStatus>();
        dataSourceBuilder.MapEnum<BloodTypeEnum>();
        dataSourceBuilder.MapEnum<RhesusFactorEnum>();
        dataSourceBuilder.MapEnum<DiagnosisType>();
        dataSourceBuilder.MapEnum<RecommendationType>();

        var dataSource = dataSourceBuilder.Build();

        services.AddDbContext<ClinicDbContext>(options =>
        {
            options.UseNpgsql(dataSource);
        });
        
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        return services;
    }
}
