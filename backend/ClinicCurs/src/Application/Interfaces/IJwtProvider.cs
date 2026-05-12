using System.Security.Claims;
using Domain.Models;

namespace Application.Interfaces;

public interface IJwtProvider
{
    string GenerateToken(Account account);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    string GenerateEmailVerificationToken(Account account);
    Guid? ValidateEmailVerificationToken(string token);
}
