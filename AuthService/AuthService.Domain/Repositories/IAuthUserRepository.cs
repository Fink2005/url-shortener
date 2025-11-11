using AuthService.Domain.Entities;

namespace AuthService.Domain.Repositories;

public interface IAuthUserRepository
{
    Task<AuthUser?> FindByUsernameAsync(string username);
    Task<AuthUser?> FindByEmailAsync(string email);
    Task<AuthUser?> FindByRefreshHashAsync(string refreshTokenHash);
    Task<AuthUser?> FindByIdAsync(Guid id);
    Task<List<AuthUser>> GetByIdsAsync(List<Guid> ids);
    Task<AuthUser?> DeleteByIdAsync(Guid id);
    Task AddAsync(AuthUser user);
    Task UpdateAsync(AuthUser user);
    Task<AuthUser?> GetByEmailAsync(string email);
    Task<List<AuthUser>> GetAllAsync();
    Task SaveChangesAsync();
}
