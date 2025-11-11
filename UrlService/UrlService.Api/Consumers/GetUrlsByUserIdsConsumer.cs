using MassTransit;
using Contracts.Url;
using UrlService.Domain.Repositories;

namespace UrlService.Api.Consumers;

public class GetUrlsByUserIdsConsumer : IConsumer<GetUrlsByUserIdsRequest>
{
    private readonly IShortUrlRepository _repository;
    private readonly IConfiguration _configuration;

    public GetUrlsByUserIdsConsumer(IShortUrlRepository repository, IConfiguration configuration)
    {
        _repository = repository;
        _configuration = configuration;
    }

    public async Task Consume(ConsumeContext<GetUrlsByUserIdsRequest> context)
    {
        try
        {
            Console.WriteLine($"📥 [UrlService] Batch GetUrlsByUserIds for {context.Message.UserIds.Count} users");
            
            var shortUrls = await _repository.GetByUserIdsAsync(context.Message.UserIds);
            var baseUrl = _configuration["BaseUrl"] ?? "http://localhost:5003";

            // Group URLs by UserId
            var urlsByUserId = shortUrls
                .GroupBy(url => url.UserId)
                .Where(g => g.Key.HasValue)
                .ToDictionary(
                    g => g.Key!.Value,
                    g => g.Select(url => new UrlDto
                    {
                        Id = url.Id,
                        ShortCode = url.ShortCode,
                        ShortUrl = $"{baseUrl}/{url.ShortCode}",
                        OriginalUrl = url.OriginalUrl,
                        CreatedAt = url.CreatedAt,
                        ExpireAt = url.ExpireAt,
                        IsActive = url.IsActive
                    }).ToList()
                );

            Console.WriteLine($"✅ [UrlService] Found URLs for {urlsByUserId.Count} users, total {shortUrls.Count} URLs");
            
            await context.RespondAsync(new GetUrlsByUserIdsResponse(urlsByUserId));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [UrlService] Error in GetUrlsByUserIds: {ex.Message}");
            throw;
        }
    }
}
