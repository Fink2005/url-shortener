using Contracts.Url;
using FluentValidation;
using UrlService.Domain.Entities;
using UrlService.Domain.ValueObjects;
using UrlService.Domain.Repositories;
namespace UrlService.Application.Commands;

public class CreateShortUrlHandler
{
    private readonly IValidator<CreateShortUrlRequest> _validator;
    private readonly IShortUrlRepository _repo;

    public CreateShortUrlHandler(IValidator<CreateShortUrlRequest> validator, IShortUrlRepository repo)
    {
        _validator = validator;
        _repo = repo;

    }

    public async Task<CreateShortUrlResponse> Handle(CreateShortUrlRequest request)
    {
        await _validator.ValidateAndThrowAsync(request);

        var shortCode = new ShortCode();
        var entity = new ShortUrl(request.OriginalUrl, shortCode.Value, request.UserId);

        await _repo.AddAsync(entity);
        await _repo.SaveChangesAsync();

        return new CreateShortUrlResponse(
            entity.Id,
            $"https://url-shortener.site/{entity.ShortCode}",
            entity.CreatedAt,
            entity.ExpireAt
        );
    }
}
