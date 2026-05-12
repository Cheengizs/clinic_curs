using Application.Interfaces.Repositories;
using Domain.Models;
using MediatR;
using AppointmentCategory = Domain.Enums.AppointmentCategory;

namespace Application.Features.Admin.Commands;

public record CreateAppointmentTypeCommand(
    AppointmentCategory Category, 
    int DefaultDurationMinutes) : IRequest<Guid>;

public class CreateAppointmentTypeHandler : IRequestHandler<CreateAppointmentTypeCommand, Guid>
{
    private readonly IGenericRepository<AppointmentType> _typeRepo;

    public CreateAppointmentTypeHandler(IGenericRepository<AppointmentType> typeRepo)
    {
        _typeRepo = typeRepo;
    }

    public async Task<Guid> Handle(CreateAppointmentTypeCommand request, CancellationToken cancellationToken)
    {
        var appointmentType = new AppointmentType
        {
            Category = request.Category,
            DefaultDurationMinutes = request.DefaultDurationMinutes
        };

        await _typeRepo.AddAsync(appointmentType);
        await _typeRepo.SaveChangesAsync();

        return appointmentType.Id;
    }
}
