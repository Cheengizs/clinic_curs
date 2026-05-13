using Application.Interfaces.Repositories;
using Domain.Models;
using MediatR;

namespace Application.Features.Appointments.Queries;

public record DoctorAppointmentDto(
    Guid Id,
    string PatientName,
    string PatientBirthDate,
    string PatientGender,
    DateTime ScheduledStart,
    DateTime ScheduledEnd,
    string Status,
    string TypeName
);

public record GetDoctorAppointmentsQuery(Guid AccountId, DateTime Date) : IRequest<List<DoctorAppointmentDto>>;

public class GetDoctorAppointmentsHandler : IRequestHandler<GetDoctorAppointmentsQuery, List<DoctorAppointmentDto>>
{
    private readonly IAppointmentRepository _appointmentRepo;
    private readonly IGenericRepository<Doctor> _doctorRepo;

    public GetDoctorAppointmentsHandler(IAppointmentRepository appointmentRepo, IGenericRepository<Doctor> doctorRepo)
    {
        _appointmentRepo = appointmentRepo;
        _doctorRepo = doctorRepo;
    }

    public async Task<List<DoctorAppointmentDto>> Handle(GetDoctorAppointmentsQuery request, CancellationToken cancellationToken)
    {
        // Находим профиль врача по его AccountId
        var doctor = await _doctorRepo.FirstOrDefaultAsync(d => d.AccountId == request.AccountId);
        if (doctor == null) return new List<DoctorAppointmentDto>();

        var appointments = await _appointmentRepo.GetDoctorAppointmentsDetailedAsync(doctor.Id, request.Date, cancellationToken);
            
        return appointments.Select(a => new DoctorAppointmentDto(
            a.Id,
            $"{a.Account.Patient?.LastName} {a.Account.Patient?.FirstName} {a.Account.Patient?.MiddleName}".Trim(),
            a.Account.Patient?.BirthDate.ToString("dd.MM.yyyy") ?? "Неизвестно",
            a.Account.Patient?.Gender.ToString() == "male" ? "Мужской" : "Женский",
            a.ScheduledStart.ToLocalTime(),
            a.ScheduledEnd.ToLocalTime(),
            a.Status.ToString(),
            TranslateCategory(a.Type.Category.ToString())
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
