using Application.Interfaces;
using Application.Interfaces.Repositories;
using Domain.Models;
using MediatR;

namespace Application.Features.Auth.Commands;
public record RegisterAccountResult(bool IsSuccess, string? ErrorMessage, Guid? AccountId);

// 1. Убрали параметр Phone
public record RegisterAccountCommand(string Email, string Password) 
    : IRequest<RegisterAccountResult>;

public class RegisterAccountCommandHandler : IRequestHandler<RegisterAccountCommand, RegisterAccountResult>
{
    private readonly IGenericRepository<Account> _accountRepo;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtProvider _jwtProvider;
    private readonly IEmailService _emailService;

    public RegisterAccountCommandHandler(
        IGenericRepository<Account> accountRepo, 
        IPasswordHasher hasher,
        IJwtProvider jwtProvider,
        IEmailService emailService)
    {
        _accountRepo = accountRepo;
        _hasher = hasher;
        _jwtProvider = jwtProvider;
        _emailService = emailService;
    }

    public async Task<RegisterAccountResult> Handle(RegisterAccountCommand request, CancellationToken cancellationToken)
    {
        if (await _accountRepo.AnyAsync(a => a.Email == request.Email))
            return new RegisterAccountResult(false, "Пользователь с таким Email уже существует.", null);

        var newAccount = new Account
        {
            Email = request.Email,
            PasswordHash = _hasher.Hash(request.Password),
            Role = RoleType.patient,
            Phone = null, // 2. Явно указываем null (или можно вообще удалить эту строку)
            EmailVerified = false,
            PhoneVerified = false 
        };

        await _accountRepo.AddAsync(newAccount);
        await _accountRepo.SaveChangesAsync();

        var emailToken = _jwtProvider.GenerateEmailVerificationToken(newAccount);
        var verifyLink = $"http://localhost:5133/api/auth/verify-email?token={emailToken}";

        await _emailService.SendVerificationEmailAsync(newAccount.Email, verifyLink);

        return new RegisterAccountResult(true, null, newAccount.Id);
    }
}
