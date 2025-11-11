using Microsoft.AspNetCore.Identity;
using AuthService.Domain.Entities;
using AuthService.Domain.Repositories;
using AuthService.Domain.Enums;
using Contracts.Auth;
using AuthService.Application.Abstractions.Security;
using FluentValidation;
using MassTransit;

namespace AuthService.Application.Commands;

public class RegisterAuthHandler
{
    private readonly IAuthUserRepository _repo;
    private readonly PasswordHasher<string> _hasher = new();
    private readonly IJwtTokenService _jwt;
    private readonly IValidator<RegisterAuthRequest> _validator;
    private readonly IPublishEndpoint _publishEndpoint;

    public RegisterAuthHandler(
        IAuthUserRepository repo,
        IJwtTokenService jwt,
        IValidator<RegisterAuthRequest> validator,
        IPublishEndpoint publishEndpoint)
    {
        _repo = repo;
        _jwt = jwt;
        _validator = validator;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<RegisterAuthResponse> Handle(RegisterAuthRequest req)
    {
        await _validator.ValidateAndThrowAsync(req);

        if (await _repo.FindByUsernameAsync(req.Username) is not null)
            throw new InvalidOperationException("Username already exists");

        if (await _repo.FindByEmailAsync(req.Email) is not null)
            throw new InvalidOperationException("Email already registered");

        var hash = _hasher.HashPassword(req.Username, req.Password);
        var user = new AuthUser(req.Username, req.Email, hash, Role.User);

        var (_, hashRefresh, expireAt) = _jwt.GenerateRefreshToken();
        user.SetRefreshToken(hashRefresh, expireAt);

        await _repo.AddAsync(user);
        await _repo.SaveChangesAsync();

        // 🔥 Publish event để trigger saga
        await _publishEndpoint.Publish(req);

        return new RegisterAuthResponse(true);
    }
}
