using FluentValidation;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;
using UserService.Infrastructure.Repositories;
using Contracts.Users;

namespace UserService.Application.Queries;

public class GetListUserHandler
{
    private readonly IUserRepository _userRepo;

    public GetListUserHandler(IUserRepository userRepo)
    {
        _userRepo = userRepo;
    }

    public async Task<GetListUsersResponse> Handle(GetListUsersRequest request)
    {
        var users = await _userRepo.GetAllAsync();
        var userDtos = users.Select(u => new UserDto(u.Id, u.AuthId, u.Username, u.Email)).ToList();
        return new GetListUsersResponse(userDtos);
    }
}
