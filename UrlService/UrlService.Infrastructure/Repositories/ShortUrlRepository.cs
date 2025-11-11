using Microsoft.EntityFrameworkCore;
using UrlService.Domain.Entities;
using UrlService.Domain.Repositories;
using UrlService.Domain.ValueObjects;
using UrlService.Infrastructure.Data;

namespace UrlService.Infrastructure.Repositories;

public class ShortUrlRepository : IShortUrlRepository
{
    private readonly UrlDbContext _db;
    public ShortUrlRepository(UrlDbContext db) => _db = db;

    public async Task AddAsync(ShortUrl entity, CancellationToken ct = default)
    {
        await _db.ShortUrls.AddAsync(entity, ct);
    }

    public async Task<List<ShortUrl>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.ShortUrls.ToListAsync(ct);
    }

    public async Task<List<ShortUrl>> GetAllByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _db.ShortUrls
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<ShortUrl?> FindByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.ShortUrls.FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<ShortUrl?> FindExpiredDateAsync(DateTime expireAt, CancellationToken ct = default)
    {
        return await _db.ShortUrls.FirstOrDefaultAsync(x => x.ExpireAt == expireAt, ct);
    }

    public async Task<ShortUrl?> DeleteByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.ShortUrls.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is not null)
        {
            _db.ShortUrls.Remove(entity);
        }
        return entity;
    }

    public async Task<ShortUrl?> FindByCodeAsync(ShortCode code, CancellationToken ct = default)
    {
        return await _db.ShortUrls.FirstOrDefaultAsync(x => x.ShortCode == code.Value, ct);
    }

    public async Task<bool> CodeExistsAsync(ShortCode code, CancellationToken ct = default)
    {
        return await _db.ShortUrls.AnyAsync(x => x.ShortCode == code.Value, ct);
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
