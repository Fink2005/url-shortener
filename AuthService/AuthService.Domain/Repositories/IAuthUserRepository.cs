using AuthService.Domain.Entities;

namespace AuthService.Domain.Repositories;

public interface IAuthUserRepository
{
    Task<AuthUser?> FindByUsernameAsync(string username);
    Task<AuthUser?> FindByEmailAsync(string email);
    Task<AuthUser?> FindByRefreshHashAsync(string refreshTokenHash);
    Task<AuthUser?> FindByIdAsync(Guid id);
    Task AddAsync(AuthUser user);
    Task SaveChangesAsync();
}
