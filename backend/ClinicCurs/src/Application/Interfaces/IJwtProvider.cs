using Domain.Models;

namespace Application.Interfaces;

public interface IJwtProvider
{
    string GenerateToken(Account account);
}
