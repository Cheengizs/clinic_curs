namespace Domain.Enums;

public enum RoleType { patient, doctor, registrar, admin }
public enum Gender { male, female }
public enum VerificationStatuses { wait, verified, declined }
public enum AppointmentStatuses { planned, confirmed, completed, cancelled, no_show }
public enum AppointmentCategory { initial_consultation, follow_up, diagnostic, procedure, vaccination }
public enum LabStatus { pending, ready, cancelled }
public enum BloodTypeEnum { O_first, A_second, B_third, AB_fourth }
public enum RhesusFactorEnum { positive, negative, neutral }
public enum DiagnosisType { main, concomitant, complication }
public enum RecommendationType { medication, procedure, lifestyle, analysis }
public enum TicketStatus { open, in_progress, closed }
