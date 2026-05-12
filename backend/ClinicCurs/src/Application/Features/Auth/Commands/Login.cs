using Application.Interfaces;
using Application.Interfaces.Repositories;
using Domain.Models;
using MediatR;

namespace Application.Features.Auth.Commands;

public record LoginAccountResult(bool IsSuccess, string? ErrorMessage, string? Token);

public record LoginAccountCommand(string Email, string Password) 
    : IRequest<LoginAccountResult>;

public class LoginAccountCommandHandler : IRequestHandler<LoginAccountCommand, LoginAccountResult>
{
    private readonly IGenericRepository<Account> _accountRepo;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtProvider _jwtProvider;

    public LoginAccountCommandHandler(
        IGenericRepository<Account> accountRepo, 
        IPasswordHasher hasher, 
        IJwtProvider jwtProvider)
    {
        _accountRepo = accountRepo;
        _hasher = hasher;
        _jwtProvider = jwtProvider;
    }

    public async Task<LoginAccountResult> Handle(LoginAccountCommand request, CancellationToken cancellationToken)
    {
        var user = await _accountRepo.FirstOrDefaultAsync(a => a.Email == request.Email);

        if (user == null || !_hasher.Verify(request.Password, user.PasswordHash))
        {
            return new LoginAccountResult(false, "Неверный Email или пароль.", null);
        }

        var token = _jwtProvider.GenerateToken(user);

        return new LoginAccountResult(true, null, token);
    }
}
