using Application.Interfaces;
using Application.Interfaces.Repositories;
using Domain.Models;
using MediatR;

namespace Application.Features.Auth.Commands;

public record RegisterAccountResult(bool IsSuccess, string? ErrorMessage, Guid? AccountId);

public record RegisterAccountCommand(string Email, string Password, string Phone) 
    : IRequest<RegisterAccountResult>;

public class RegisterAccountCommandHandler : IRequestHandler<RegisterAccountCommand, RegisterAccountResult>
{
    private readonly IGenericRepository<Account> _accountRepo;
    private readonly IPasswordHasher _hasher;

    public RegisterAccountCommandHandler(IGenericRepository<Account> accountRepo, IPasswordHasher hasher)
    {
        _accountRepo = accountRepo;
        _hasher = hasher;
    }

    public async Task<RegisterAccountResult> Handle(RegisterAccountCommand request, CancellationToken cancellationToken)
    {
        if (await _accountRepo.AnyAsync(a => a.Email == request.Email))
        {
            return new RegisterAccountResult(false, "Пользователь с таким Email уже существует.", null);
        }

        var newAccount = new Account
        {
            Email = request.Email,
            PasswordHash = _hasher.Hash(request.Password),
            Role = RoleType.patient,
            Phone = request.Phone
        };

        await _accountRepo.AddAsync(newAccount);
        await _accountRepo.SaveChangesAsync();

        return new RegisterAccountResult(true, null, newAccount.Id);
    }
}
