using Application.Interfaces;
using Application.Interfaces.Repositories;
using Domain.Models;
using MediatR;

namespace Application.Features.Auth.Commands;

public record AuthResult(bool IsSuccess, string? ErrorMessage, string? AccessToken, string? RefreshToken);

public record LoginAccountCommand(string Email, string Password) : IRequest<AuthResult>;

public class LoginAccountCommandHandler : IRequestHandler<LoginAccountCommand, AuthResult>
{
    private readonly IGenericRepository<Account> _accountRepo;
    private readonly IGenericRepository<RefreshToken> _refreshTokenRepo;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtProvider _jwtProvider;

    public LoginAccountCommandHandler(
        IGenericRepository<Account> accountRepo,
        IGenericRepository<RefreshToken> refreshTokenRepo,
        IPasswordHasher hasher, 
        IJwtProvider jwtProvider)
    {
        _accountRepo = accountRepo;
        _refreshTokenRepo = refreshTokenRepo;
        _hasher = hasher;
        _jwtProvider = jwtProvider;
    }

    public async Task<AuthResult> Handle(LoginAccountCommand request, CancellationToken cancellationToken)
    {
        var user = await _accountRepo.FirstOrDefaultAsync(a => a.Email == request.Email);

        if (user == null || !_hasher.Verify(request.Password, user.PasswordHash))
        {
            return new AuthResult(false, "Неверный Email или пароль.", null, null);
        }

        var accessToken = _jwtProvider.GenerateToken(user);
        var refreshToken = _jwtProvider.GenerateRefreshToken();

        var rtEntity = new RefreshToken
        {
            AccountId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        await _refreshTokenRepo.AddAsync(rtEntity);
        await _refreshTokenRepo.SaveChangesAsync();

        return new AuthResult(true, null, accessToken, refreshToken);
    }
}
