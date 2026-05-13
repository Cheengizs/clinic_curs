using Application.Interfaces.Repositories;
using Domain.Enums;
using Domain.Models;
using MediatR;

namespace Application.Features.Appointments.Commands;

public record BookAppointmentResult(bool IsSuccess, string? ErrorMessage);

public record BookAppointmentCommand(Guid AccountId, Guid DoctorId, Guid TypeId, DateTime ScheduledStart) : IRequest<BookAppointmentResult>;

public class BookAppointmentHandler : IRequestHandler<BookAppointmentCommand, BookAppointmentResult>
{
    private readonly IGenericRepository<Appointment> _appointmentRepo;
    private readonly IGenericRepository<Doctor> _doctorRepo;
    private readonly IGenericRepository<AppointmentType> _typeRepo;
    private readonly IGenericRepository<Account> _accountRepo;

    public BookAppointmentHandler(
        IGenericRepository<Appointment> appointmentRepo,
        IGenericRepository<Doctor> doctorRepo,
        IGenericRepository<AppointmentType> typeRepo,
        IGenericRepository<Account> accountRepo)
    {
        _appointmentRepo = appointmentRepo;
        _doctorRepo = doctorRepo;
        _typeRepo = typeRepo;
        _accountRepo = accountRepo;
    }

    public async Task<BookAppointmentResult> Handle(BookAppointmentCommand request, CancellationToken cancellationToken)
    {
        var account = await _accountRepo.GetByIdAsync(request.AccountId);
        if (account == null || !account.IdentityVerified) 
            return new BookAppointmentResult(false, "Для записи к врачу необходимо пройти верификацию.");

        var doctor = await _doctorRepo.GetByIdAsync(request.DoctorId);
        if (doctor == null) return new BookAppointmentResult(false, "Врач не найден.");

        var type = await _typeRepo.GetByIdAsync(request.TypeId);
        if (type == null) return new BookAppointmentResult(false, "Вид записи не найден.");

        // Вычисляем границы новой записи в UTC
        var newStart = request.ScheduledStart.ToUniversalTime();
        var newEnd = newStart.AddMinutes(type.DefaultDurationMinutes);

        // ЖЕСТКАЯ ПРОВЕРКА ПЕРЕСЕЧЕНИЙ В БД
        var hasOverlap = await _appointmentRepo.AnyAsync(a => 
            a.DoctorId == request.DoctorId && 
            a.Status != AppointmentStatuses.cancelled &&
            newStart < a.ScheduledEnd && 
            newEnd > a.ScheduledStart);

        if (hasOverlap) return new BookAppointmentResult(false, "К сожалению, это время или его часть уже заняты.");

        var appointment = new Appointment
        {
            AccountId = request.AccountId,
            DoctorId = request.DoctorId,
            OfficeId = doctor.OfficeId,
            TypeId = type.Id,
            ScheduledStart = newStart,
            ScheduledEnd = newEnd,
            Status = AppointmentStatuses.planned
        };

        await _appointmentRepo.AddAsync(appointment);
        await _appointmentRepo.SaveChangesAsync();

        return new BookAppointmentResult(true, null);
    }
}
