using Application.Interfaces.Repositories;
using Domain.Models;
using MediatR;

namespace Application.Features.Admin.Commands;

public record UpdateOfficePhotoCommand(Guid OfficeId, string PhotoUrl) : IRequest<bool>;

public class UpdateOfficePhotoHandler : IRequestHandler<UpdateOfficePhotoCommand, bool>
{
    private readonly IGenericRepository<Office> _officeRepo;

    public UpdateOfficePhotoHandler(IGenericRepository<Office> officeRepo) => _officeRepo = officeRepo;

    public async Task<bool> Handle(UpdateOfficePhotoCommand request, CancellationToken cancellationToken)
    {
        var office = await _officeRepo.GetByIdAsync(request.OfficeId);
        if (office == null) return false;

        office.PhotoUrl = request.PhotoUrl;
        _officeRepo.Update(office);
        await _officeRepo.SaveChangesAsync();
        return true;
    }
}
