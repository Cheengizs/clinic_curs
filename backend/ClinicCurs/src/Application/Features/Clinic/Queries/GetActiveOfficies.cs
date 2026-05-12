using Application.Interfaces.Repositories;
using Domain.Models;
using MediatR;

namespace Application.Features.Clinic.Queries;

public record OfficeDto(
    Guid Id, 
    string Name, 
    string Address, 
    string Phone, 
    string PhotoUrl);

public record GetActiveOfficesQuery() : IRequest<List<OfficeDto>>;

public class GetActiveOfficesHandler : IRequestHandler<GetActiveOfficesQuery, List<OfficeDto>>
{
    private readonly IGenericRepository<Office> _officeRepo;

    public GetActiveOfficesHandler(IGenericRepository<Office> officeRepo)
    {
        _officeRepo = officeRepo;
    }

    public async Task<List<OfficeDto>> Handle(GetActiveOfficesQuery request, CancellationToken cancellationToken)
    {
        // Ищем в базе только активные офисы
        var offices = await _officeRepo.FindAsync(o => o.IsActive);
        
        return offices.Select(o => new OfficeDto(
            o.Id,
            o.Name,
            o.Address,
            o.Phone ?? "Нет номера",
            o.PhotoUrl ?? "default_office.png"
        )).ToList();
    }
}
