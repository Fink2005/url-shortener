using UserService.Domain.Entities;

namespace UserService.Domain.Repositories;

public interface IUserRepository
{
    Task<User?> FindByAuthIdAsync(Guid authId);
    Task<User?> FindByUserIdAsync(Guid userId);
    Task AddAsync(User user);
    Task<bool> DeleteAsync(Guid userId);
    Task SaveChangesAsync();
}
