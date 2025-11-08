using MassTransit;
using Contracts.Auth;
using AuthService.Application.Commands;

namespace AuthService.Api.Consumers;

public class LoginAuthConsumer : IConsumer<LoginAuthRequest>
{
    private readonly LoginAuthHandler _handler;

    public LoginAuthConsumer(LoginAuthHandler handler)
    {
        _handler = handler;
    }

    public async Task Consume(ConsumeContext<LoginAuthRequest> context)
    {
        var result = await _handler.Handle(context.Message);
        await context.RespondAsync(result);
    }
}
