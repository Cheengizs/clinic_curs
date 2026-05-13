using Application.Interfaces.Repositories;
using Domain.Models;
using MediatR;

namespace Application.Features.Admin.Commands;

public record CreateScheduleCommand(
    Guid DoctorId,
    Guid OfficeId,
    DateOnly WorkDate,
    TimeOnly StartTime,
    TimeOnly EndTime
) : IRequest<bool>;

public class CreateScheduleHandler : IRequestHandler<CreateScheduleCommand, bool>
{
    private readonly IGenericRepository<Schedule> _scheduleRepo;

    public CreateScheduleHandler(IGenericRepository<Schedule> scheduleRepo)
    {
        _scheduleRepo = scheduleRepo;
    }

    public async Task<bool> Handle(CreateScheduleCommand request, CancellationToken cancellationToken)
    {
        // Проверяем, нет ли уже расписания у этого врача на этот день
        var existing = await _scheduleRepo.FirstOrDefaultAsync(s => 
            s.DoctorId == request.DoctorId && s.WorkDate == request.WorkDate);
        
        if (existing != null) return false;

        var schedule = new Schedule
        {
            DoctorId = request.DoctorId,
            OfficeId = request.OfficeId,
            WorkDate = request.WorkDate,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            IsActive = true
        };

        await _scheduleRepo.AddAsync(schedule);
        await _scheduleRepo.SaveChangesAsync();
        return true;
    }
}
