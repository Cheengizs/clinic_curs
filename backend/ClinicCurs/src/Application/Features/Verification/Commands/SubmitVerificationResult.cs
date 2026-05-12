using Application.Interfaces.Repositories;
using Domain.Enums;
using Domain.Models;
using MediatR;

namespace Application.Features.Verification.Commands;
public record SubmitVerificationResult(bool IsSuccess, string? ErrorMessage);

public record SubmitVerificationRequestCommand(
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

public class SubmitVerificationRequestCommandHandler : IRequestHandler<SubmitVerificationRequestCommand, SubmitVerificationResult>
{
    private readonly IGenericRepository<VerificationRequest> _requestRepo;
    private readonly IGenericRepository<Account> _accountRepo;
    private readonly IGenericRepository<Office> _officeRepo;

    public SubmitVerificationRequestCommandHandler(
        IGenericRepository<VerificationRequest> requestRepo,
        IGenericRepository<Account> accountRepo,
        IGenericRepository<Office> officeRepo) // Инжектим репозиторий офисов
    {
        _requestRepo = requestRepo;
        _accountRepo = accountRepo;
        _officeRepo = officeRepo;
    }

    public async Task<SubmitVerificationResult> Handle(SubmitVerificationRequestCommand request, CancellationToken cancellationToken)
    {
        var account = await _accountRepo.GetByIdAsync(request.AccountId);
        if (account == null)
            return new SubmitVerificationResult(false, "Аккаунт не найден.");

        // ЖЕСТКАЯ ПРОВЕРКА: Подтверждены ли контакты?
        if (!account.EmailVerified || !account.PhoneVerified)
        {
            return new SubmitVerificationResult(false, "Для записи на верификацию необходимо подтвердить Email и номер телефона.");
        }

        // ПРОВЕРКА: Существует ли офис?
        var office = await _officeRepo.GetByIdAsync(request.OfficeId);
        if (office == null || !office.IsActive)
        {
            return new SubmitVerificationResult(false, "Выбранный офис не найден или неактивен.");
        }

        // ПРОВЕРКА: Нет ли уже активной заявки?
        var existingRequest = await _requestRepo.FirstOrDefaultAsync(
            r => r.AccountId == request.AccountId && 
                 (r.Status == VerificationStatuses.wait || r.Status == VerificationStatuses.verified));

        if (existingRequest != null)
        {
            return new SubmitVerificationResult(false, "У вас уже есть ожидающая или одобренная заявка на верификацию.");
        }

        var verificationReq = new VerificationRequest
        {
            AccountId = request.AccountId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            MiddleName = request.MiddleName,
            BirthDate = request.BirthDate,
            PassportSeriesNumber = request.PassportSeriesNumber,
            PersonalNumber = request.PersonalNumber,
            OfficeId = request.OfficeId,       // <-- Сохраняем офис
            ScheduledAt = request.ScheduledAt, // <-- Сохраняем время
            Status = VerificationStatuses.wait
        };

        await _requestRepo.AddAsync(verificationReq);
        await _requestRepo.SaveChangesAsync();

        return new SubmitVerificationResult(true, null);
    }
}
