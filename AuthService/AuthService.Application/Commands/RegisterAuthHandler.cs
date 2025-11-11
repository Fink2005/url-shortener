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

    public RegisterAuthHandler(
        IAuthUserRepository repo,
        IJwtTokenService jwt,
        IValidator<RegisterAuthRequest> validator)
    {
        _repo = repo;
        _jwt = jwt;
        _validator = validator;
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

        await _repo.AddAsync(user);
        await _repo.SaveChangesAsync();

        // Note: Event publishing is now handled by RegisterAuthConsumer
        // to ensure it's published before the response is sent to the gateway

        return new RegisterAuthResponse(true);
    }
}
