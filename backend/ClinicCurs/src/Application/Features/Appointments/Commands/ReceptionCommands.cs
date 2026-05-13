using Application.Interfaces.Repositories;
using Domain.Enums;
using Domain.Models;
using MediatR;

namespace Application.Features.Appointments.Commands;

// 1. ЗАПРОС РАСПИСАНИЯ НА СЕГОДНЯ
public record OfficeAppointmentDto(Guid Id, string PatientName, string DoctorName, DateTime ScheduledStart, string Status, string TypeName);

public record GetOfficeAppointmentsQuery(Guid AccountId) : IRequest<List<OfficeAppointmentDto>>;

public class GetOfficeAppointmentsHandler : IRequestHandler<GetOfficeAppointmentsQuery, List<OfficeAppointmentDto>>
{
    private readonly IAppointmentRepository _appRepo;
    private readonly IGenericRepository<Registrar> _regRepo;

    public GetOfficeAppointmentsHandler(IAppointmentRepository appRepo, IGenericRepository<Registrar> regRepo)
    {
        _appRepo = appRepo;
        _regRepo = regRepo;
    }

    public async Task<List<OfficeAppointmentDto>> Handle(GetOfficeAppointmentsQuery request, CancellationToken cancellationToken)
    {
        // Находим, в каком офисе работает этот регистратор
        var reg = await _regRepo.FirstOrDefaultAsync(r => r.AccountId == request.AccountId);
        if (reg == null) return new List<OfficeAppointmentDto>();

        var apps = await _appRepo.GetOfficeAppointmentsDetailedAsync(reg.OfficeId, DateTime.UtcNow, cancellationToken);

        return apps.Select(a => new OfficeAppointmentDto(
            a.Id,
            $"{a.Account.Patient?.LastName} {a.Account.Patient?.FirstName}".Trim(),
            $"{a.Doctor?.LastName} {a.Doctor?.FirstName}".Trim(),
            a.ScheduledStart.ToLocalTime(),
            a.Status.ToString(),
            a.Type.Category.ToString()
        )).ToList();
    }
}

// 2. КОМАНДА: ПАЦИЕНТ ПРИШЕЛ
public record ConfirmArrivalCommand(Guid AppointmentId) : IRequest<bool>;

public class ConfirmArrivalHandler : IRequestHandler<ConfirmArrivalCommand, bool>
{
    private readonly IGenericRepository<Appointment> _appRepo;
    public ConfirmArrivalHandler(IGenericRepository<Appointment> appRepo) => _appRepo = appRepo;

    public async Task<bool> Handle(ConfirmArrivalCommand request, CancellationToken cancellationToken)
    {
        var app = await _appRepo.GetByIdAsync(request.AppointmentId);
        // Регистратор может подтвердить прибытие, только если статус = "Запланирован"
        if (app != null && app.Status == AppointmentStatuses.planned)
        {
            app.Status = AppointmentStatuses.confirmed;
            _appRepo.Update(app);
            await _appRepo.SaveChangesAsync();
            return true;
        }
        return false;
    }
}
