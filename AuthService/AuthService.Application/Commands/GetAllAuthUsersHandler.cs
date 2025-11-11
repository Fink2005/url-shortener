using AuthService.Domain.Repositories;
using AuthService.Domain.Entities;

namespace AuthService.Application.Commands;

public class GetAllAuthUsersHandler
{
    private readonly IAuthUserRepository _authUserRepository;

    public GetAllAuthUsersHandler(IAuthUserRepository authUserRepository)
    {
        _authUserRepository = authUserRepository;
    }

    public async Task<GetAllAuthUsersResponse> Handle()
    {
        var users = await _authUserRepository.GetAllAsync();

        var userDtos = users.Select(u => new AuthUserDto
        {
            Id = u.Id,
            Username = u.Username,
            Email = u.Email,
            Role = u.Role.ToString(),
            IsEmailVerified = u.IsEmailVerified
        }).ToList();

        return new GetAllAuthUsersResponse(userDtos);
    }
}

public record GetAllAuthUsersResponse(List<AuthUserDto> Users);

public class AuthUserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsEmailVerified { get; set; }
}
