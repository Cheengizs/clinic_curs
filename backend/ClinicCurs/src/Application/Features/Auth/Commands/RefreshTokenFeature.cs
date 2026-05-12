using System.Security.Claims;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Domain.Models;
using MediatR;

namespace Application.Features.Auth.Commands;

public record RefreshTokenCommand(string AccessToken, string RefreshToken) : IRequest<AuthResult>;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResult>
{
    private readonly IGenericRepository<Account> _accountRepo;
    private readonly IGenericRepository<RefreshToken> _refreshTokenRepo;
    private readonly IJwtProvider _jwtProvider;

    public RefreshTokenCommandHandler(
        IGenericRepository<Account> accountRepo, 
        IGenericRepository<RefreshToken> refreshTokenRepo, 
        IJwtProvider jwtProvider)
    {
        _accountRepo = accountRepo;
        _refreshTokenRepo = refreshTokenRepo;
        _jwtProvider = jwtProvider;
    }

    public async Task<AuthResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var principal = _jwtProvider.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal == null)
            return new AuthResult(false, "Недействительный Access токен.", null, null);

        var accountIdStr = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(accountIdStr) || !Guid.TryParse(accountIdStr, out var accountId))
        {
            return new AuthResult(false, "Недействительный токен.", null, null);
        }

        // 2. Ищем Refresh Token в БД
        var storedToken = await _refreshTokenRepo.FirstOrDefaultAsync(
            rt => rt.Token == request.RefreshToken && rt.AccountId == accountId);

        // 3. Проверки на валидность и протухание
        if (storedToken == null || storedToken.IsRevoked || storedToken.ExpiresAt < DateTime.UtcNow)
        {
            return new AuthResult(false, "Refresh токен недействителен или истек. Авторизуйтесь заново.", null, null);
        }

        // 4. Отзываем старый рефреш токен (чтобы его нельзя было использовать повторно)
        storedToken.IsRevoked = true;
        _refreshTokenRepo.Update(storedToken);

        // 5. Генерируем новую пару
        var account = await _accountRepo.GetByIdAsync(accountId);
        if (account == null)
            return new AuthResult(false, "Аккаунт не найден.", null, null);

        var newAccessToken = _jwtProvider.GenerateToken(account);
        var newRefreshToken = _jwtProvider.GenerateRefreshToken();

        var newRtEntity = new RefreshToken
        {
            AccountId = account.Id,
            Token = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        await _refreshTokenRepo.AddAsync(newRtEntity);
        await _refreshTokenRepo.SaveChangesAsync();

        return new AuthResult(true, null, newAccessToken, newRefreshToken);
    }
}
