using Application.Interfaces.Repositories;
using Domain.Models;
using MediatR;

namespace Application.Features.Admin.Commands;

public record CreateOfficeCommand(
    string Name, 
    string Address, 
    string Phone
    ) : IRequest<Guid>;

public class CreateOfficeHandler : IRequestHandler<CreateOfficeCommand, Guid>
{
    private readonly IGenericRepository<Office> _officeRepo;

    public CreateOfficeHandler(IGenericRepository<Office> officeRepo) => _officeRepo = officeRepo;

    public async Task<Guid> Handle(CreateOfficeCommand request, CancellationToken cancellationToken)
    {
        var office = new Office
        {
            Name = request.Name,
            Address = request.Address,
            Phone = request.Phone,
            IsActive = true
        };

        await _officeRepo.AddAsync(office);
        await _officeRepo.SaveChangesAsync();
        return office.Id;
    }
}
