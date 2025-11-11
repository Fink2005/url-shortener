using UrlService.Domain.Entities;
using UrlService.Domain.ValueObjects;

namespace UrlService.Domain.Repositories;

public interface IShortUrlRepository
{
    Task AddAsync(ShortUrl entity, CancellationToken ct = default);
    Task<ShortUrl?> FindByCodeAsync(ShortCode code, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(ShortCode code, CancellationToken ct = default);
    Task<List<ShortUrl>> GetAllAsync(CancellationToken ct = default);
    Task<List<ShortUrl>> GetAllByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<ShortUrl?> FindByIdAsync(Guid id, CancellationToken ct = default);
    Task<ShortUrl?> FindExpiredDateAsync(DateTime expireAt, CancellationToken ct = default);
    Task<ShortUrl?> DeleteByIdAsync(Guid id, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
