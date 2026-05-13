using Application.Interfaces.Repositories;
using Domain.Enums;
using Domain.Models;
using MediatR;

namespace Application.Features.Verification.Commands;

public record EditVerificationRequestByStaffCommand(
    Guid RequestId,
    string FirstName,
    string LastName,
    string MiddleName,
    DateOnly BirthDate,
    Gender Gender, // <--
    string PassportSeriesNumber,
    string PersonalNumber,
    string ResidentialAddress, // <--
    Guid OfficeId,
    DateTime ScheduledAt
) : IRequest<SubmitVerificationResult>;

public class EditVerificationRequestByStaffHandler : IRequestHandler<EditVerificationRequestByStaffCommand, SubmitVerificationResult>
{
    private readonly IGenericRepository<VerificationRequest> _requestRepo;

    public EditVerificationRequestByStaffHandler(IGenericRepository<VerificationRequest> requestRepo) => _requestRepo = requestRepo;

    public async Task<SubmitVerificationResult> Handle(EditVerificationRequestByStaffCommand request, CancellationToken cancellationToken)
    {
        var existing = await _requestRepo.GetByIdAsync(request.RequestId);
        if (existing == null || existing.Status != VerificationStatuses.wait)
            return new SubmitVerificationResult(false, "Заявка не найдена или уже была обработана.");

        existing.FirstName = request.FirstName;
        existing.LastName = request.LastName;
        existing.MiddleName = request.MiddleName;
        existing.BirthDate = request.BirthDate;
        existing.Gender = request.Gender;
        existing.PassportSeriesNumber = request.PassportSeriesNumber;
        existing.PersonalNumber = request.PersonalNumber;
        existing.ResidentialAddress = request.ResidentialAddress;
        existing.OfficeId = request.OfficeId;
        existing.ScheduledAt = request.ScheduledAt;

        _requestRepo.Update(existing);
        await _requestRepo.SaveChangesAsync();
        
        return new SubmitVerificationResult(true, null);
    }
}
