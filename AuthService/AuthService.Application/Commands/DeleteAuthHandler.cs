using System.Security.Cryptography;
using System.Text;
using AuthService.Domain.Repositories;
using Contracts.Auth;
using FluentValidation;


namespace AuthService.Application.Commands;

public class DeleteAuthHandler
{
    private readonly IAuthUserRepository _repo;
    private readonly IValidator<DeleteAuthRequest> _validator;

    public DeleteAuthHandler(IAuthUserRepository repo, IValidator<DeleteAuthRequest> validator)
    {
        _repo = repo;
        _validator = validator;
    }

    public async Task<DeleteAuthResponse> Handle(DeleteAuthRequest req)
    {
        await _validator.ValidateAndThrowAsync(req);

        var user = await _repo.DeleteByIdAsync(req.Id);
        if (user is null) return new DeleteAuthResponse(false);

        return new DeleteAuthResponse(true);
    }
}


