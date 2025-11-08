using System.Security.Cryptography;
using System.Text;
using AuthService.Domain.Repositories;
using AuthService.Application.Abstractions.Security;
using Contracts.Auth;
using FluentValidation;


namespace AuthService.Application.Commands;

public class RefreshTokenHandler
{
    private readonly IAuthUserRepository _repo;
    private readonly IJwtTokenService _jwt;
    private readonly IValidator<RefreshTokenRequest> _validator;


    public RefreshTokenHandler(IAuthUserRepository repo, IJwtTokenService jwt, IValidator<RefreshTokenRequest> validator)
    {
        _repo = repo;
        _jwt = jwt;
        _validator = validator;
    }

    public async Task<RefreshTokenResponse> Handle(RefreshTokenRequest req)
    {
        await _validator.ValidateAndThrowAsync(req);

        using var sha = SHA256.Create();
        var hash = Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(req.RefreshToken)));

        var user = await _repo.FindByRefreshHashAsync(hash)
                   ?? throw new UnauthorizedAccessException("Invalid refresh token");

        if (user.RefreshTokenExpireAt is null || user.RefreshTokenExpireAt <= DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token expired");

        var access = _jwt.GenerateAccessToken(user);
        var (plainRefresh, hashRefresh, expireAt) = _jwt.GenerateRefreshToken();

        user.SetRefreshToken(hashRefresh, expireAt);
        await _repo.SaveChangesAsync();

        return new RefreshTokenResponse(access, plainRefresh);
    }
}
