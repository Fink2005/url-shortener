using Contracts.Url;
using UrlService.Domain.Repositories;
using UrlService.Domain.ValueObjects;

namespace UrlService.Application.Commands;

public class DisableShortUrlHandler
{
    private readonly IShortUrlRepository _repo;
    public DisableShortUrlHandler(IShortUrlRepository repo) => _repo = repo;

    public async Task<DisableShortUrlResponse> Handle(DisableShortUrlRequest req, CancellationToken ct = default)
    {
        var code = new ShortCode(req.Code);
        var entity = await _repo.FindByCodeAsync(code, ct);
        if (entity is null) return new DisableShortUrlResponse(false);

        entity.Disable();
        await _repo.SaveChangesAsync(ct);
        return new DisableShortUrlResponse(true);
    }
}
