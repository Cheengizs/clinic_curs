using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Domain.Models;

namespace ClinicCurs.Infrastructure.Data;

public class ClinicDbContext : DbContext
{
    public ClinicDbContext(DbContextOptions<ClinicDbContext> options) : base(options)
    {
    }

    #region DbSets

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<PasswordReset> PasswordResets => Set<PasswordReset>();
    public DbSet<Office> Offices => Set<Office>();
    public DbSet<Registrar> Registrars => Set<Registrar>();
    public DbSet<VerificationRequest> VerificationRequests => Set<VerificationRequest>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Specialization> Specializations => Set<Specialization>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<DoctorSpecialization> DoctorSpecializations => Set<DoctorSpecialization>();
    public DbSet<Schedule> Schedules => Set<Schedule>();
    public DbSet<AppointmentType> AppointmentTypes => Set<AppointmentType>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<MedicalCard> MedicalCards => Set<MedicalCard>();
    public DbSet<MedicalRecord> MedicalRecords => Set<MedicalRecord>();
    public DbSet<Icd10Dictionary> Icd10Dictionaries => Set<Icd10Dictionary>();
    public DbSet<RecordDiagnosis> RecordDiagnoses => Set<RecordDiagnosis>();
    public DbSet<Recommendation> Recommendations => Set<Recommendation>();
    public DbSet<LabTestsDictionary> LabTestsDictionaries => Set<LabTestsDictionary>();
    public DbSet<LabResult> LabResults => Set<LabResult>();

    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresEnum<RoleType>();
        modelBuilder.HasPostgresEnum<Gender>();
        modelBuilder.HasPostgresEnum<VerificationStatuses>();
        modelBuilder.HasPostgresEnum<AppointmentStatuses>();
        modelBuilder.HasPostgresEnum<AppointmentCategory>();
        modelBuilder.HasPostgresEnum<LabStatus>();
        modelBuilder.HasPostgresEnum<BloodTypeEnum>();
        modelBuilder.HasPostgresEnum<RhesusFactorEnum>();
        modelBuilder.HasPostgresEnum<DiagnosisType>();
        modelBuilder.HasPostgresEnum<RecommendationType>();

        modelBuilder.Entity<Registrar>().HasOne(e => e.Account).WithOne(e => e.Registrar)
            .HasForeignKey<Registrar>(e => e.AccountId);
        modelBuilder.Entity<Doctor>().HasOne(e => e.Account).WithOne(e => e.Doctor)
            .HasForeignKey<Doctor>(e => e.AccountId);
        modelBuilder.Entity<Patient>().HasOne(e => e.Account).WithOne(e => e.Patient)
            .HasForeignKey<Patient>(e => e.AccountId);
        modelBuilder.Entity<Review>().HasOne(e => e.Appointment).WithOne(e => e.Review)
            .HasForeignKey<Review>(e => e.AppointmentId);
        modelBuilder.Entity<MedicalCard>().HasOne(e => e.Patient).WithOne(e => e.MedicalCard)
            .HasForeignKey<MedicalCard>(e => e.PatientId);
        modelBuilder.Entity<MedicalRecord>().HasOne(e => e.Appointment).WithOne(e => e.MedicalRecord)
            .HasForeignKey<MedicalRecord>(e => e.AppointmentId);

        modelBuilder.Entity<DoctorSpecialization>().HasKey(e => new { e.DoctorId, e.SpecializationId });
        modelBuilder.Entity<RecordDiagnosis>().HasKey(e => new { e.RecordId, e.DiagnosisId });
        modelBuilder.Entity<RefreshToken>()
            .HasOne(rt => rt.Account)
            .WithMany(a => a.RefreshTokens)
            .HasForeignKey(rt => rt.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entity.ClrType;

            // Определяем префикс таблицы (tbl_ или m2m_)
            string prefix = clrType.Name.Contains("Specialization") && clrType.Name.Contains("Doctor") ||
                            clrType.Name.Contains("Record") && clrType.Name.Contains("Diagnosis")
                ? "m2m_"
                : "tbl_";

            entity.SetTableName(prefix + clrType.Name.ToSnakeCase());

            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(property.Name.ToSnakeCase());

                if (property.Name == "Id") property.SetDefaultValueSql("gen_random_uuid()");
                if (property.Name == "CreatedAt") property.SetDefaultValueSql("now()");
            }
        }

        modelBuilder.Entity<Icd10Dictionary>().ToTable("tbl_icd10_dictionary");
        modelBuilder.Entity<DoctorSpecialization>().ToTable("m2m_doctor_specialization");
        modelBuilder.Entity<RecordDiagnosis>().ToTable("m2m_record_diagnosis");
    }
}

public static class StringExtensions
{
    public static string ToSnakeCase(this string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        var startUnderscore = Regex.Replace(input, @"([a-z0-9])([A-Z])", "$1_$2").ToLower();
        return startUnderscore.Replace("tbl_", "").Replace("m2m_", "");
    }
}
