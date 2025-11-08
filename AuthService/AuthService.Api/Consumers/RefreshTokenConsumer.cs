using MassTransit;
using Contracts.Auth;
using AuthService.Application.Commands;

namespace AuthService.Api.Consumers;

public class RefreshTokenConsumer : IConsumer<RefreshTokenRequest>
{
    private readonly RefreshTokenHandler _handler;

    public RefreshTokenConsumer(RefreshTokenHandler handler)
    {
        _handler = handler;
    }

    public async Task Consume(ConsumeContext<RefreshTokenRequest> context)
    {
        var result = await _handler.Handle(context.Message);
        await context.RespondAsync(result);
    }
}
