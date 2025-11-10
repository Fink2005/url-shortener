using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;
using UserService.Infrastructure.Data;
namespace UserService.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<User?> FindByAuthIdAsync(Guid authId)
    {
        return await _db.Users.FirstOrDefaultAsync(u => u.AuthId == authId);
    }
    public async Task<User?> FindByUserIdAsync(Guid userId)
    {
        return await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task AddAsync(User user)
    {
        await _db.Users.AddAsync(user);
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _db.Users.ToListAsync();
    }

    public async Task<bool> DeleteAsync(Guid userId)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            return false;

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }
}
