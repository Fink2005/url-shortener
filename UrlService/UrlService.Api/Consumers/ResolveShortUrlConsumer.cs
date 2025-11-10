using MassTransit;
using Contracts.Url;
using UrlService.Application.Commands;

namespace UrlService.Api.Consumers;

public class ResolveShortUrlConsumer : IConsumer<ResolveShortUrlRequest>
{
    private readonly ResolveShortUrlHandler _handler;

    public ResolveShortUrlConsumer(ResolveShortUrlHandler handler)
    {
        _handler = handler;
    }

    public async Task Consume(ConsumeContext<ResolveShortUrlRequest> context)
    {
        var result = await _handler.Handle(context.Message);
        await context.RespondAsync(result);
    }
}
