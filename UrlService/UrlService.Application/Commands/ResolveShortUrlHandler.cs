using Contracts.Url;
using UrlService.Domain.Repositories;
using UrlService.Domain.ValueObjects;

namespace UrlService.Application.Commands;


public class ResolveShortUrlHandler
{
    private readonly IShortUrlRepository _repo;
    public ResolveShortUrlHandler(IShortUrlRepository repo) => _repo = repo;

    public async Task<ResolveShortUrlResponse> Handle(ResolveShortUrlRequest req, CancellationToken ct = default)
    {
        var code = new ShortCode(req.Code);
        var entity = await _repo.FindByCodeAsync(code, ct);
        if (entity is null) throw new KeyNotFoundException("Short code not found");

        var active = entity.IsActive && !entity.IsExpired();
        return new ResolveShortUrlResponse(entity.OriginalUrl, active);
    }
}
