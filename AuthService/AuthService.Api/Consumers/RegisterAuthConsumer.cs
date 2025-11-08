using MassTransit;
using Contracts.Auth;
using AuthService.Application.Commands;

namespace AuthService.Api.Consumers;

public class RegisterAuthConsumer : IConsumer<RegisterAuthRequest>
{
    private readonly RegisterAuthHandler _handler;

    public RegisterAuthConsumer(RegisterAuthHandler handler)
    {
        _handler = handler;
    }

    public async Task Consume(ConsumeContext<RegisterAuthRequest> context)
    {
        var result = await _handler.Handle(context.Message);
        await context.RespondAsync(result);
    }
}
