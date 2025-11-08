using UserService.Domain.Entities;
using UserService.Infrastructure.Data;
using Contracts.Users;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace UserService.Application.Users.Queries;

public class GetUserHandler
{
    private readonly AppDbContext _db;
    private readonly IValidator<GetUserRequest> _validator;

    public GetUserHandler(AppDbContext db, IValidator<GetUserRequest> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<GetUserResponse> Handle(GetUserRequest request)
    {
        // ✅ validate input
        await _validator.ValidateAndThrowAsync(request);

        // ✅ domain logic
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.Id);
        if (user == null) throw new Exception("User not found");

        return new GetUserResponse(user.AuthId, user.Id, user.Username, user.Email);
    }
}
