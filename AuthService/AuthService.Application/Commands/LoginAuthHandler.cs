using Microsoft.AspNetCore.Identity;
using AuthService.Domain.Repositories;
using AuthService.Application.Abstractions.Security;
using FluentValidation;
using Contracts.Auth;

namespace AuthService.Application.Commands;

public class LoginAuthHandler
{
    private readonly IAuthUserRepository _repo;
    private readonly PasswordHasher<string> _hasher = new();
    private readonly IJwtTokenService _jwt;
    private readonly IValidator<LoginAuthRequest> _validator;

    public LoginAuthHandler(IAuthUserRepository repo, IJwtTokenService jwt, IValidator<LoginAuthRequest> validator)
    {
        _repo = repo;
        _jwt = jwt;
        _validator = validator;

    }

    public async Task<LoginAuthResponse> Handle(LoginAuthRequest req)
    {
        await _validator.ValidateAndThrowAsync(req);

        var user = await _repo.FindByUsernameAsync(req.Username)
                   ?? throw new UnauthorizedAccessException("Invalid username or password");

        var result = _hasher.VerifyHashedPassword(req.Username, user.PasswordHash, req.Password);
        if (result == PasswordVerificationResult.Failed)
            throw new UnauthorizedAccessException("Invalid username or password");

        var access = _jwt.GenerateAccessToken(user);
        var (plainRefresh, hashRefresh, expireAt) = _jwt.GenerateRefreshToken();

        user.SetRefreshToken(hashRefresh, expireAt);
        await _repo.SaveChangesAsync();

        return new LoginAuthResponse(access, plainRefresh);
    }
}
