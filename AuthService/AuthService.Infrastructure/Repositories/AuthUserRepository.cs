using Microsoft.EntityFrameworkCore;
using AuthService.Domain.Entities;
using AuthService.Domain.Repositories;
using AuthService.Infrastructure.Data;

namespace AuthService.Infrastructure.Repositories;

public class AuthUserRepository : IAuthUserRepository
{
    private readonly AuthDbContext _db;
    public AuthUserRepository(AuthDbContext db) => _db = db;

    public Task<AuthUser?> FindByUsernameAsync(string username)
        => _db.AuthUsers.FirstOrDefaultAsync(x => x.Username == username);

    public Task<AuthUser?> FindByEmailAsync(string email)
        => _db.AuthUsers.FirstOrDefaultAsync(x => x.Email == email);

    public Task<AuthUser?> FindByRefreshHashAsync(string refreshTokenHash)
        => _db.AuthUsers.FirstOrDefaultAsync(x => x.RefreshTokenHash == refreshTokenHash);

    public Task<AuthUser?> FindByIdAsync(Guid id)
        => _db.AuthUsers.FirstOrDefaultAsync(x => x.Id == id);

    public Task<List<AuthUser>> GetByIdsAsync(List<Guid> ids)
        => _db.AuthUsers.Where(x => ids.Contains(x.Id)).ToListAsync();

    public async Task<AuthUser?> DeleteByIdAsync(Guid id)
    {
        var user = await _db.AuthUsers.FirstOrDefaultAsync(x => x.Id == id);
        if (user == null)
            return null;

        _db.AuthUsers.Remove(user);
        await _db.SaveChangesAsync();
        return user;
    }


    public async Task AddAsync(AuthUser user) => await _db.AuthUsers.AddAsync(user);

    public async Task UpdateAsync(AuthUser user)
    {
        _db.AuthUsers.Update(user);
        await _db.SaveChangesAsync();
    }

    public Task<AuthUser?> GetByEmailAsync(string email)
        => _db.AuthUsers.FirstOrDefaultAsync(x => x.Email == email);

    public Task<List<AuthUser>> GetAllAsync()
        => _db.AuthUsers.ToListAsync();

    public Task SaveChangesAsync() => _db.SaveChangesAsync();
}
