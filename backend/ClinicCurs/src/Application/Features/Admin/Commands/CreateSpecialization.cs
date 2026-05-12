using Application.Interfaces.Repositories;
using Domain.Models;
using MediatR;

namespace Application.Features.Admin.Commands;

public record CreateSpecializationCommand(string Name, string? Description) : IRequest<Guid>;

public class CreateSpecializationHandler : IRequestHandler<CreateSpecializationCommand, Guid>
{
    private readonly IGenericRepository<Specialization> _specRepo;

    public CreateSpecializationHandler(IGenericRepository<Specialization> specRepo)
    {
        _specRepo = specRepo;
    }

    public async Task<Guid> Handle(CreateSpecializationCommand request, CancellationToken cancellationToken)
    {
        // Проверяем, нет ли уже специализации с таким именем
        var existing = await _specRepo.FirstOrDefaultAsync(s => s.Name.ToLower() == request.Name.ToLower());
        if (existing != null) return existing.Id;

        var specialization = new Specialization
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            IsActive = true
        };

        await _specRepo.AddAsync(specialization);
        await _specRepo.SaveChangesAsync();

        return specialization.Id;
    }
}
