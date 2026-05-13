using Application.Interfaces.Repositories;
using Domain.Enums;
using Domain.Models;
using MediatR;

namespace Application.Features.Appointments.Commands;

public record CancelAppointmentCommand(Guid AccountId, Guid AppointmentId) : IRequest<bool>;

public class CancelAppointmentHandler : IRequestHandler<CancelAppointmentCommand, bool>
{
    private readonly IGenericRepository<Appointment> _appointmentRepo;
    
    public CancelAppointmentHandler(IGenericRepository<Appointment> appointmentRepo) => _appointmentRepo = appointmentRepo;

    public async Task<bool> Handle(CancelAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentRepo.GetByIdAsync(request.AppointmentId);
        
        // Пациент может отменить только СВОЮ запись
        if (appointment == null || appointment.AccountId != request.AccountId) return false;
            
        // Отменить можно только запланированные или подтвержденные записи
        if (appointment.Status == AppointmentStatuses.planned || appointment.Status == AppointmentStatuses.confirmed)
        {
            appointment.Status = AppointmentStatuses.cancelled;
            _appointmentRepo.Update(appointment);
            await _appointmentRepo.SaveChangesAsync();
            return true;
        }
        return false;
    }
}
