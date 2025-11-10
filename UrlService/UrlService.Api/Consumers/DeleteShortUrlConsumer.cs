using MassTransit;
using Contracts.Url;
using UrlService.Application.Commands;

namespace UrlService.Api.Consumers;


public class DeleteShortUrlConsumer : IConsumer<DeleteShortUrlRequest>
{
    private readonly DeleteShortUrlHandler _handler;

    public DeleteShortUrlConsumer(DeleteShortUrlHandler handler)
    {
        _handler = handler;
    }

    public async Task Consume(ConsumeContext<DeleteShortUrlRequest> context)
    {
        var result = await _handler.Handle(context.Message);
        await context.RespondAsync(result);
    }
}
