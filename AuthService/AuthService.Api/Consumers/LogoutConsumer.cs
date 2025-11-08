using MassTransit;
using Contracts.Auth;
using AuthService.Application.Commands;

namespace AuthService.Api.Consumers;

public class LogoutConsumer : IConsumer<LogoutRequest>
{
    private readonly LogoutHandler _handler;

    public LogoutConsumer(LogoutHandler handler)
    {
        _handler = handler;
    }

    public async Task Consume(ConsumeContext<LogoutRequest> context)
    {
        var result = await _handler.Handle(context.Message);
        await context.RespondAsync(result);
    }
}
