using Application.Interfaces;
using Application.Interfaces.Repositories;
using Domain.Models;
using MediatR;

namespace Application.Features.Auth.Commands;

public record VerifyEmailResult(bool IsSuccess, string? ErrorMessage);

public record VerifyEmailCommand(string Token) : IRequest<VerifyEmailResult>;

public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, VerifyEmailResult>
{
    private readonly IGenericRepository<Account> _accountRepo;
    private readonly IJwtProvider _jwtProvider;

    public VerifyEmailCommandHandler(IGenericRepository<Account> accountRepo, IJwtProvider jwtProvider)
    {
        _accountRepo = accountRepo;
        _jwtProvider = jwtProvider;
    }

    public async Task<VerifyEmailResult> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        // 1. Расшифровываем токен и достаем AccountId
        var accountId = _jwtProvider.ValidateEmailVerificationToken(request.Token);
        if (accountId == null)
        {
            return new VerifyEmailResult(false, "Недействительная или устаревшая ссылка.");
        }

        // 2. Ищем аккаунт
        var account = await _accountRepo.GetByIdAsync(accountId.Value);
        if (account == null)
        {
            return new VerifyEmailResult(false, "Аккаунт не найден.");
        }

        if (account.EmailVerified)
        {
            return new VerifyEmailResult(false, "Email уже подтвержден.");
        }

        // 3. Обновляем статус
        account.EmailVerified = true;
        _accountRepo.Update(account);
        await _accountRepo.SaveChangesAsync();

        return new VerifyEmailResult(true, null);
    }
}
