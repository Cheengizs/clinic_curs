using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Features.Appointments.Queries;

public record PatientAppointmentDto(
    Guid Id,
    string DoctorName,
    string DoctorSpecialization,
    string OfficeName,
    string OfficeAddress,
    string AvatarUrl,
    DateTime ScheduledStart,
    DateTime ScheduledEnd,
    string Status,
    string TypeName,
    bool HasReview // <--- НОВОЕ ПОЛЕ
);

public record GetPatientAppointmentsQuery(Guid AccountId) : IRequest<List<PatientAppointmentDto>>;

public class GetPatientAppointmentsHandler : IRequestHandler<GetPatientAppointmentsQuery, List<PatientAppointmentDto>>
{
    private readonly IAppointmentRepository _appointmentRepo;

    public GetPatientAppointmentsHandler(IAppointmentRepository appointmentRepo) => _appointmentRepo = appointmentRepo;

    public async Task<List<PatientAppointmentDto>> Handle(GetPatientAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var appointments = await _appointmentRepo.GetPatientAppointmentsDetailedAsync(request.AccountId, cancellationToken);
            
        return appointments.Select(a => new PatientAppointmentDto(
            a.Id,
            $"{a.Doctor?.LastName} {a.Doctor?.FirstName} {a.Doctor?.MiddleName}".Trim(),
            a.Doctor?.DoctorSpecializations.FirstOrDefault(ds => ds.IsPrimary)?.Specialization.Name ?? "Специалист",
            a.Office.Name,
            a.Office.Address,
            a.Doctor?.AvatarUrl ?? "default_doctor.png",
            a.ScheduledStart.ToLocalTime(),
            a.ScheduledEnd.ToLocalTime(),
            a.Status.ToString(),
            TranslateCategory(a.Type.Category.ToString()),
            a.Review != null // <--- ПРОВЕРЯЕМ, ЕСТЬ ЛИ ОТЗЫВ В БД
        )).ToList();
    }

    private static string TranslateCategory(string category) => category switch {
        "initial_consultation" => "Первичный прием",
        "follow_up" => "Повторный прием",
        "diagnostic" => "Диагностика",
        "procedure" => "Процедура",
        "vaccination" => "Вакцинация",
        _ => "Прием"
    };
}
