using MassTransit;
using Contracts.Url;
using UrlService.Application.Commands;

namespace UrlService.Api.Consumers;

public class DisableShortUrlConsumer : IConsumer<DisableShortUrlRequest>
{
    private readonly DisableShortUrlHandler _handler;

    public DisableShortUrlConsumer(DisableShortUrlHandler handler)
    {
        _handler = handler;
    }

    public async Task Consume(ConsumeContext<DisableShortUrlRequest> context)
    {
        var result = await _handler.Handle(context.Message);
        await context.RespondAsync(result);
    }
}
