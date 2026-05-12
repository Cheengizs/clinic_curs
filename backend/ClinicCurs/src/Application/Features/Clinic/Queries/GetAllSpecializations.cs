using Application.Interfaces.Repositories;
using Domain.Models;
using MediatR;

namespace Application.Features.Clinic.Queries;

public record SpecializationDto(Guid Id, string Name, string Description);

public record GetAllSpecializationsQuery() : IRequest<List<SpecializationDto>>;

public class GetAllSpecializationsHandler : IRequestHandler<GetAllSpecializationsQuery, List<SpecializationDto>>
{
    private readonly IGenericRepository<Specialization> _specRepo;

    public GetAllSpecializationsHandler(IGenericRepository<Specialization> specRepo)
    {
        _specRepo = specRepo;
    }

    public async Task<List<SpecializationDto>> Handle(GetAllSpecializationsQuery request, CancellationToken cancellationToken)
    {
        // Получаем все специализации из БД
        var specializations = await _specRepo.GetAllAsync();
        
        return specializations.Select(s => new SpecializationDto(
            s.Id,
            s.Name,
            s.Description ?? ""
        )).OrderBy(s => s.Name).ToList(); // Сортируем по алфавиту для удобства
    }
}
