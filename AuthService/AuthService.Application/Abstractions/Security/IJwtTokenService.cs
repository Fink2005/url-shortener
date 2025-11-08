using AuthService.Domain.Entities;

namespace AuthService.Application.Abstractions.Security;

public interface IJwtTokenService
{
    string GenerateAccessToken(AuthUser user);
    (string tokenPlain, string tokenHash, DateTime expireAt) GenerateRefreshToken();
}
