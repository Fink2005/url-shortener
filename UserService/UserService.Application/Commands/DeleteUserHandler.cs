using FluentValidation;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;
using UserService.Infrastructure.Repositories;
using Contracts.Users;

namespace UserService.Application.Commands;

public class DeleteUserHandler
{
    private readonly IUserRepository _userRepo;
    private readonly IValidator<DeleteUserRequest> _validator;

    public DeleteUserHandler(IUserRepository userRepo, IValidator<DeleteUserRequest> validator)
    {
        _userRepo = userRepo;
        _validator = validator;
    }

    public async Task<DeleteUserResponse> Handle(DeleteUserRequest request)
    {
        await _validator.ValidateAndThrowAsync(request);

        var existing = await _userRepo.FindByUserIdAsync(request.Id);
        if (existing == null)
            throw new InvalidOperationException("User does not exist.");

        await _userRepo.DeleteAsync(request.Id);

        return new DeleteUserResponse(true);
    }
}
