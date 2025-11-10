using Contracts.Url;
using FluentValidation;
using UrlService.Domain.Entities;
using UrlService.Domain.ValueObjects;
using UrlService.Domain.Repositories;
namespace UrlService.Application.Commands;

public class DeleteShortUrlHandler
{
    private readonly IValidator<DeleteShortUrlRequest> _validator;
    private readonly IShortUrlRepository _repo;

    public DeleteShortUrlHandler(IValidator<DeleteShortUrlRequest> validator, IShortUrlRepository repo)
    {
        _validator = validator;
        _repo = repo;

    }

    public async Task<DeleteShortUrlResponse> Handle(DeleteShortUrlRequest request)
    {

        await _validator.ValidateAndThrowAsync(request);
        var entity = await _repo.FindByIdAsync(request.Id);
        if (entity is null)
        {
            throw new Exception("Short URL not found.");
        }

        await _repo.DeleteByIdAsync(request.Id);
        await _repo.SaveChangesAsync();

        return new DeleteShortUrlResponse(true);
    }
}
