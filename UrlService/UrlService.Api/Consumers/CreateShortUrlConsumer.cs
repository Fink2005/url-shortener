using MassTransit;
using Contracts.Url;
using UrlService.Application.Commands;

namespace UrlService.Api.Consumers;


public class CreateShortUrlConsumer : IConsumer<CreateShortUrlRequest>
{
    private readonly CreateShortUrlHandler _handler;

    public CreateShortUrlConsumer(CreateShortUrlHandler handler)
    {
        _handler = handler;
    }

    public async Task Consume(ConsumeContext<CreateShortUrlRequest> context)
    {
        var result = await _handler.Handle(context.Message);
        await context.RespondAsync(result);
    }
}
