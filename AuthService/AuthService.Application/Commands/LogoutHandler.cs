using System.Security.Cryptography;
using System.Text;
using AuthService.Domain.Repositories;
using Contracts.Auth;
using FluentValidation;


namespace AuthService.Application.Commands;

public class LogoutHandler
{
    private readonly IAuthUserRepository _repo;
    private readonly IValidator<LogoutRequest> _validator;

    public LogoutHandler(IAuthUserRepository repo, IValidator<LogoutRequest> validator)
    {
        _repo = repo;
        _validator = validator;
    }

    public async Task<LogoutResponse> Handle(LogoutRequest req)
    {
        await _validator.ValidateAndThrowAsync(req);

        using var sha = SHA256.Create();
        var hash = Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(req.RefreshToken)));

        var user = await _repo.FindByRefreshHashAsync(hash);
        if (user is null) return new LogoutResponse(true);

        user.RevokeRefreshToken();
        await _repo.SaveChangesAsync();

        return new LogoutResponse(true);
    }
}
