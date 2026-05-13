using Application.Interfaces.Repositories;
using Domain.Enums;
using Domain.Models;
using MediatR;

namespace Application.Features.Verification.Commands;

public record UpdateVerificationRequestCommand(
    Guid AccountId,
    string FirstName,
    string LastName,
    string MiddleName,
    DateOnly BirthDate,
    string PassportSeriesNumber,
    string PersonalNumber,
    Guid OfficeId,
    DateTime ScheduledAt
) : IRequest<SubmitVerificationResult>;

public class UpdateVerificationRequestHandler : IRequestHandler<UpdateVerificationRequestCommand, SubmitVerificationResult>
{
    private readonly IGenericRepository<VerificationRequest> _requestRepo;

    public UpdateVerificationRequestHandler(IGenericRepository<VerificationRequest> requestRepo) => _requestRepo = requestRepo;

    public async Task<SubmitVerificationResult> Handle(UpdateVerificationRequestCommand request, CancellationToken cancellationToken)
    {
        var existing = await _requestRepo.FirstOrDefaultAsync(r => r.AccountId == request.AccountId && r.Status == VerificationStatuses.wait);
        if (existing == null) return new SubmitVerificationResult(false, "Заявка не найдена или уже обработана.");

        existing.FirstName = request.FirstName;
        existing.LastName = request.LastName;
        existing.MiddleName = request.MiddleName;
        existing.BirthDate = request.BirthDate;
        existing.PassportSeriesNumber = request.PassportSeriesNumber;
        existing.PersonalNumber = request.PersonalNumber;
        existing.OfficeId = request.OfficeId;
        existing.ScheduledAt = request.ScheduledAt;

        _requestRepo.Update(existing);
        await _requestRepo.SaveChangesAsync();
        return new SubmitVerificationResult(true, null);
    }
}
