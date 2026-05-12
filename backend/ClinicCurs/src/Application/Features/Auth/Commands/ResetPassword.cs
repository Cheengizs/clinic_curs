using Application.Interfaces;
using Application.Interfaces.Repositories;
using Domain.Models;
using MediatR;

namespace Application.Features.Auth.Commands;

public record ResetPasswordCommand(string Token, string NewPassword) : IRequest<bool>;

public class ResetPasswordHandler : IRequestHandler<ResetPasswordCommand, bool>
{
    private readonly IGenericRepository<Account> _accountRepo;
    private readonly IGenericRepository<PasswordReset> _resetRepo;
    private readonly IPasswordHasher _hasher;

    public ResetPasswordHandler(IGenericRepository<Account> accountRepo, IGenericRepository<PasswordReset> resetRepo, IPasswordHasher hasher)
    {
        _accountRepo = accountRepo;
        _resetRepo = resetRepo;
        _hasher = hasher;
    }

    public async Task<bool> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        // Ищем активный токен
        var resetEntry = await _resetRepo.FirstOrDefaultAsync(r => 
            r.TokenHash == request.Token && !r.IsUsed && r.ExpiresAt > DateTime.UtcNow);

        if (resetEntry == null) return false;

        var account = await _accountRepo.GetByIdAsync(resetEntry.AccountId);
        if (account == null) return false;

        // Обновляем пароль
        account.PasswordHash = _hasher.Hash(request.NewPassword);
        _accountRepo.Update(account);

        resetEntry.IsUsed = true;
        _resetRepo.Update(resetEntry);

        await _accountRepo.SaveChangesAsync();
        return true;
    }
}
