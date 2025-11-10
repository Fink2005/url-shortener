using Contracts.Url;
using FluentValidation;
using UrlService.Domain.Entities;
using UrlService.Domain.ValueObjects;
using UrlService.Domain.Repositories;
namespace UrlService.Application.Commands;

public class GetListShortUrlsHandler
{
    private readonly IShortUrlRepository _repo;

    public GetListShortUrlsHandler(IShortUrlRepository repo)
    {
        _repo = repo;

    }

    public async Task<GetListShortUrlsResponse> Handle(GetListShortUrlsRequest request)
    {
        var entities = await _repo.GetAllAsync();

        var dtos = entities.Select(x => new ShortUrlDto(
            x.Id,
            x.OriginalUrl,
            $"https://url-shortener.site/{x.ShortCode}",
            x.CreatedAt,
            x.ExpireAt
        )).ToList();

        return new GetListShortUrlsResponse(dtos);
    }

}
