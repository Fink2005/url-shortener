using MassTransit;
using Contracts.Url;
using UrlService.Domain.Repositories;

namespace UrlService.Api.Consumers;

public class GetUrlsByUserConsumer : IConsumer<GetUrlsByUserRequest>
{
    private readonly IShortUrlRepository _repository;
    private readonly IConfiguration _configuration;

    public GetUrlsByUserConsumer(IShortUrlRepository repository, IConfiguration configuration)
    {
        _repository = repository;
        _configuration = configuration;
    }

    public async Task Consume(ConsumeContext<GetUrlsByUserRequest> context)
    {
        try
        {
            Console.WriteLine($"📥 [UrlService] GetUrlsByUser request for UserId: {context.Message.UserId}");

            var shortUrls = await _repository.GetAllByUserIdAsync(context.Message.UserId);

            var baseUrl = _configuration["BaseUrl"] ?? "http://localhost:5003";

            var urlDtos = shortUrls.Select(url => new UrlDto
            {
                Id = url.Id,
                ShortCode = url.ShortCode,
                ShortUrl = $"{baseUrl}/{url.ShortCode}",
                OriginalUrl = url.OriginalUrl,
                CreatedAt = url.CreatedAt,
                ExpireAt = url.ExpireAt,
                IsActive = url.IsActive
            }).ToList();

            Console.WriteLine($"✅ [UrlService] Found {urlDtos.Count} URLs for user {context.Message.UserId}");

            await context.RespondAsync(new GetUrlsByUserResponse(context.Message.UserId, urlDtos));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [UrlService] Error getting URLs by user: {ex.Message}");
            throw;
        }
    }
}
