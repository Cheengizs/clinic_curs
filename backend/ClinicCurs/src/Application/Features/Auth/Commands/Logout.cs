using Application.Interfaces.Repositories;
using Domain.Models;
using MediatR;

namespace Application.Features.Auth.Commands;

public record LogoutResult(bool IsSuccess, string? ErrorMessage);

public record LogoutCommand(string RefreshToken) : IRequest<LogoutResult>;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, LogoutResult>
{
    private readonly IGenericRepository<RefreshToken> _refreshTokenRepo;

    public LogoutCommandHandler(IGenericRepository<RefreshToken> refreshTokenRepo)
    {
        _refreshTokenRepo = refreshTokenRepo;
    }

    public async Task<LogoutResult> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return new LogoutResult(false, "Refresh токен не предоставлен.");
        }

        // Ищем токен в базе данных
        var storedToken = await _refreshTokenRepo.FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

        if (storedToken == null)
        {
            return new LogoutResult(true, null);
        }

        if (storedToken.IsRevoked)
        {
            return new LogoutResult(true, null);
        }

        // Отзываем токен (вместо физического удаления сохраняем историю)
        storedToken.IsRevoked = true;
        
        _refreshTokenRepo.Update(storedToken);
        await _refreshTokenRepo.SaveChangesAsync();

        return new LogoutResult(true, null);
    }
}
