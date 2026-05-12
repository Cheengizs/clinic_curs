namespace Domain.Models;

#region 1. Enums (Domain Types)

public enum RoleType
{
    patient,
    doctor,
    registrar,
    admin
}

public enum Gender
{
    male,
    female
}

public enum VerificationStatuses
{
    wait,
    verified,
    declined
}

public enum AppointmentStatuses
{
    planned,
    confirmed,
    completed,
    cancelled,
    no_show
}

public enum AppointmentCategory
{
    initial_consultation,
    follow_up,
    diagnostic,
    procedure,
    vaccination
}

public enum LabStatus
{
    pending,
    ready,
    cancelled
}

public enum BloodTypeEnum
{
    O_first,
    A_second,
    B_third,
    AB_fourth
}

public enum RhesusFactorEnum
{
    positive,
    negative,
    neutral
}

public enum DiagnosisType
{
    main,
    concomitant,
    complication
}

public enum RecommendationType
{
    medication,
    procedure,
    lifestyle,
    analysis
}

#endregion

#region 2. Entities (Tables)

public class Account
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string Phone { get; set; }
    public bool EmailVerified { get; set; }
    public bool PhoneVerified { get; set; }
    public bool IdentityVerified { get; set; }
    public RoleType Role { get; set; }
    public bool IsDeleted { get; set; }

    // Navigation
    public virtual Registrar Registrar { get; set; }
    public virtual Doctor Doctor { get; set; }
    public virtual Patient Patient { get; set; }
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public virtual ICollection<PasswordReset> PasswordResets { get; set; }
    public virtual ICollection<VerificationRequest> VerificationRequests { get; set; }
    public virtual ICollection<Appointment> Appointments { get; set; }
}

public class PasswordReset
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public string TokenHash { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime CreatedAt { get; set; }
    public virtual Account Account { get; set; }
}

public class Office
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public string Phone { get; set; }
    public bool IsActive { get; set; }
    public string PhotoUrl { get; set; }

    public virtual ICollection<Doctor> Doctors { get; set; }
    public virtual ICollection<Registrar> Registrars { get; set; }
    public virtual ICollection<Schedule> Schedules { get; set; }
    public virtual ICollection<Appointment> Appointments { get; set; }
    public virtual ICollection<LabResult> LabResults { get; set; }
}

public class Registrar
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public Guid OfficeId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string MiddleName { get; set; }
    public DateOnly? HiredAt { get; set; }
    public string AvatarUrl { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }

    public virtual Account Account { get; set; }
    public virtual Office Office { get; set; }
    public virtual ICollection<VerificationRequest> VerificationRequests { get; set; }
    public virtual ICollection<Appointment> Appointments { get; set; }
}

public class VerificationRequest
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public string LastName { get; set; }
    public string FirstName { get; set; }
    public string MiddleName { get; set; }
    public DateOnly BirthDate { get; set; }
    public string PassportSeriesNumber { get; set; }
    public string PersonalNumber { get; set; }
    public string DocumentScanUrl { get; set; }
    public VerificationStatuses Status { get; set; }
    public Guid? RegistrarId { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual Account Account { get; set; }
    public virtual Registrar Registrar { get; set; }
}

public class Patient
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string MiddleName { get; set; }
    public DateOnly BirthDate { get; set; }
    public Gender Gender { get; set; }
    public string PassportSeriesNumber { get; set; }
    public string PersonalNumber { get; set; }
    public string ResidentialAddress { get; set; }
    public string AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual Account Account { get; set; }
    public virtual MedicalCard MedicalCard { get; set; }
    public virtual ICollection<Review> Reviews { get; set; }
}

public class Specialization
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public virtual ICollection<DoctorSpecialization> DoctorSpecializations { get; set; }
}

public class Doctor
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public Guid OfficeId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string MiddleName { get; set; }
    public string Bio { get; set; }
    public DateOnly? HiredAt { get; set; }
    public string AvatarUrl { get; set; }
    public decimal RatingAvg { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }

    public virtual Account Account { get; set; }
    public virtual Office Office { get; set; }
    public virtual ICollection<DoctorSpecialization> DoctorSpecializations { get; set; }
    public virtual ICollection<Schedule> Schedules { get; set; }
    public virtual ICollection<Appointment> Appointments { get; set; }
    public virtual ICollection<Review> Reviews { get; set; }
    public virtual ICollection<MedicalRecord> MedicalRecords { get; set; }
}

// M2M Junction with extra data
public class DoctorSpecialization
{
    public Guid DoctorId { get; set; }
    public Guid SpecializationId { get; set; }
    public bool IsPrimary { get; set; }
    public DateOnly? CareerStartDate { get; set; }

    public virtual Doctor Doctor { get; set; }
    public virtual Specialization Specialization { get; set; }
}

public class Schedule
{
    public Guid Id { get; set; }
    public Guid DoctorId { get; set; }
    public Guid OfficeId { get; set; }
    public DateOnly WorkDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool IsActive { get; set; }

    public virtual Doctor Doctor { get; set; }
    public virtual Office Office { get; set; }
}

public class AppointmentType
{
    public Guid Id { get; set; }
    public AppointmentCategory Category { get; set; }
    public int DefaultDurationMinutes { get; set; }
    public virtual ICollection<Appointment> Appointments { get; set; }
}

public class Appointment
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public Guid OfficeId { get; set; }
    public Guid TypeId { get; set; }
    public Guid? DoctorId { get; set; }
    public Guid? RegistrarId { get; set; }
    public DateTime ScheduledStart { get; set; }
    public DateTime ScheduledEnd { get; set; }
    public AppointmentStatuses Status { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual Account Account { get; set; }
    public virtual Office Office { get; set; }
    public virtual AppointmentType Type { get; set; }
    public virtual Doctor Doctor { get; set; }
    public virtual Registrar Registrar { get; set; }
    public virtual Review Review { get; set; }
    public virtual MedicalRecord MedicalRecord { get; set; }
}

public class Review
{
    public Guid Id { get; set; }
    public Guid DoctorId { get; set; }
    public Guid PatientId { get; set; }
    public Guid AppointmentId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual Doctor Doctor { get; set; }
    public virtual Patient Patient { get; set; }
    public virtual Appointment Appointment { get; set; }
}

public class MedicalCard
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string CardNumber { get; set; }
    public BloodTypeEnum BloodType { get; set; }
    public RhesusFactorEnum RhesusFactor { get; set; }
    public string ChronicDiseases { get; set; }
    public string Allergies { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public virtual Patient Patient { get; set; }
    public virtual ICollection<MedicalRecord> MedicalRecords { get; set; }
    public virtual ICollection<LabResult> LabResults { get; set; }
}

public class MedicalRecord
{
    public Guid Id { get; set; }
    public Guid CardId { get; set; }
    public Guid AppointmentId { get; set; }
    public Guid DoctorId { get; set; }
    public string Complaints { get; set; }
    public string ObjectiveData { get; set; }
    public string Assessment { get; set; }
    public string Plan { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual MedicalCard MedicalCard { get; set; }
    public virtual Appointment Appointment { get; set; }
    public virtual Doctor Doctor { get; set; }
    public virtual ICollection<RecordDiagnosis> RecordDiagnoses { get; set; }
    public virtual ICollection<Recommendation> Recommendations { get; set; }
}

public class Icd10Dictionary
{
    public Guid Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public virtual ICollection<RecordDiagnosis> RecordDiagnoses { get; set; }
}

// M2M Junction with extra data
public class RecordDiagnosis
{
    public Guid RecordId { get; set; }
    public Guid DiagnosisId { get; set; }
    public DiagnosisType Type { get; set; }

    public virtual MedicalRecord Record { get; set; }
    public virtual Icd10Dictionary Diagnosis { get; set; }
}

public class Recommendation
{
    public Guid Id { get; set; }
    public Guid RecordId { get; set; }
    public RecommendationType Type { get; set; }
    public string Title { get; set; }
    public string Details { get; set; }
    public int DurationDays { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual MedicalRecord Record { get; set; }
    public virtual ICollection<LabResult> LabResults { get; set; }
}

public class LabTestsDictionary
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public virtual ICollection<LabResult> LabResults { get; set; }
}

public class LabResult
{
    public Guid Id { get; set; }
    public Guid CardId { get; set; }
    public Guid TestId { get; set; }
    public Guid? RecommendationId { get; set; }
    public Guid OfficeId { get; set; }
    public string ResultFileUrl { get; set; }
    public LabStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual MedicalCard MedicalCard { get; set; }
    public virtual LabTestsDictionary Test { get; set; }
    public virtual Recommendation Recommendation { get; set; }
    public virtual Office Office { get; set; }
}

#endregion
