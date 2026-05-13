using Application.Interfaces.Repositories;
using Domain.Enums;
using Domain.Models;
using MediatR;

namespace Application.Features.Appointments.Queries;

// ДОБАВЛЕН TypeId
public record GetAvailableSlotsQuery(Guid DoctorId, DateOnly Date, Guid TypeId) : IRequest<List<string>>;

public class GetAvailableSlotsHandler : IRequestHandler<GetAvailableSlotsQuery, List<string>>
{
    private readonly IGenericRepository<Appointment> _appointmentRepo;
    private readonly IGenericRepository<Schedule> _scheduleRepo;
    private readonly IGenericRepository<AppointmentType> _typeRepo;

    public GetAvailableSlotsHandler(
        IGenericRepository<Appointment> appointmentRepo,
        IGenericRepository<Schedule> scheduleRepo,
        IGenericRepository<AppointmentType> typeRepo)
    {
        _appointmentRepo = appointmentRepo;
        _scheduleRepo = scheduleRepo;
        _typeRepo = typeRepo;
    }

    public async Task<List<string>> Handle(GetAvailableSlotsQuery request, CancellationToken cancellationToken)
    {
        var schedule = await _scheduleRepo.FirstOrDefaultAsync(s => 
            s.DoctorId == request.DoctorId && s.WorkDate == request.Date && s.IsActive);

        if (schedule == null) return new List<string>();

        // Узнаем длительность выбранной услуги
        var type = await _typeRepo.GetByIdAsync(request.TypeId);
        if (type == null) return new List<string>();
        int durationMinutes = type.DefaultDurationMinutes;

        var startOfDay = request.Date.ToDateTime(TimeOnly.MinValue).ToUniversalTime();
        var endOfDay = request.Date.ToDateTime(TimeOnly.MinValue).AddDays(1).ToUniversalTime();

        var booked = await _appointmentRepo.FindAsync(a =>
            a.DoctorId == request.DoctorId &&
            a.ScheduledStart >= startOfDay &&
            a.ScheduledStart < endOfDay &&
            a.Status != AppointmentStatuses.cancelled);

        var availableSlots = new List<string>();

        var current = schedule.StartTime;
        var end = schedule.EndTime;
        var step = TimeSpan.FromMinutes(30);

        while (current < end)
        {
            var proposedStartLocal = request.Date.ToDateTime(current);
            var proposedEndLocal = proposedStartLocal.AddMinutes(durationMinutes);

            // Если прием заканчивается позже, чем конец смены врача — этот слот недоступен
            var currentEndTime = current.AddMinutes(durationMinutes);
            if (currentEndTime > end)
            {
                current = current.Add(step);
                continue;
            }

            // ПРОВЕРКА ПЕРЕСЕЧЕНИЯ ИНТЕРВАЛОВ (Overlap)
            bool hasOverlap = booked.Any(a => 
            {
                var bookedStartLocal = a.ScheduledStart.ToLocalTime();
                var bookedEndLocal = a.ScheduledEnd.ToLocalTime();
                
                // Пересечение есть, если старт новой записи раньше конца старой, 
                // А конец новой записи позже старта старой
                return proposedStartLocal < bookedEndLocal && proposedEndLocal > bookedStartLocal;
            });

            if (!hasOverlap)
            {
                availableSlots.Add(current.ToString("HH:mm"));
            }

            current = current.Add(step);
        }

        return availableSlots;
    }
}
