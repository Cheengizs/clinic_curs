using System.Security.Cryptography;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Domain.Models;
using MediatR;

namespace Application.Features.Auth.Commands;

public record ForgotPasswordCommand(string Email) : IRequest<bool>;

public class ForgotPasswordHandler : IRequestHandler<ForgotPasswordCommand, bool>
{
    private readonly IGenericRepository<Account> _accountRepo;
    private readonly IGenericRepository<PasswordReset> _resetRepo;
    private readonly IEmailService _emailService;

    public ForgotPasswordHandler(IGenericRepository<Account> accountRepo, IGenericRepository<PasswordReset> resetRepo, IEmailService emailService)
    {
        _accountRepo = accountRepo;
        _resetRepo = resetRepo;
        _emailService = emailService;
    }

    public async Task<bool> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var account = await _accountRepo.FirstOrDefaultAsync(a => a.Email == request.Email);
        if (account == null) return true; // Не выдаем, что email не найден (безопасность)

        // Генерируем случайный токен
        var rawToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        
        var resetEntry = new PasswordReset
        {
            AccountId = account.Id,
            TokenHash = rawToken, // В реальном проекте лучше еще раз хэшнуть через SHA256
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            IsUsed = false
        };

        await _resetRepo.AddAsync(resetEntry);
        await _resetRepo.SaveChangesAsync();

        var resetLink = $"http://localhost:5133/api/auth/reset-password?token={rawToken}";
        await _emailService.SendPasswordResetEmailAsync(account.Email, resetLink);

        return true;
    }
}
