using FluentValidation;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;
using Contracts.Users;

namespace UserService.Application.Commands;

public class CreateUserHandler
{
    private readonly IUserRepository _userRepo;
    private readonly IValidator<CreateUserRequest> _validator;

    public CreateUserHandler(IUserRepository userRepo, IValidator<CreateUserRequest> validator)
    {
        _userRepo = userRepo;
        _validator = validator;
    }

    public async Task<CreateUserResponse> Handle(CreateUserRequest request)
    {
        // ✅ Validate input
        await _validator.ValidateAndThrowAsync(request);

        // ✅ Application logic: check business constraint
        var existing = await _userRepo.FindByAuthIdAsync(request.AuthId);
        if (existing != null)
            throw new InvalidOperationException("User already exists.");

        // ✅ Domain logic: delegate to domain entity constructor
        var user = new User(request.AuthId, request.Username, request.Email);

        await _userRepo.AddAsync(user);
        await _userRepo.SaveChangesAsync();

        // ✅ Map to contract DTO
        return new CreateUserResponse(user.Id, user.Username, user.Email);
    }
}
