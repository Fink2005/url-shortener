using MassTransit;
using Contracts.Url;
using UrlService.Application.Commands;

namespace UrlService.Api.Consumers;


public class GetListShortUrlConsumer : IConsumer<GetListShortUrlsRequest>
{
    private readonly GetListShortUrlsHandler _handler;

    public GetListShortUrlConsumer(GetListShortUrlsHandler handler)
    {
        _handler = handler;
    }

    public async Task Consume(ConsumeContext<GetListShortUrlsRequest> context)
    {
        var result = await _handler.Handle(context.Message);
        await context.RespondAsync(result);
    }
}
