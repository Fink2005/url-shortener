using MassTransit;
using Contracts.Auth;
using AuthService.Application.Commands;

namespace AuthService.Api.Consumers;

public class LoginUserConsumer : IConsumer<LoginUserRequest>
{
    private readonly LoginUserHandler _handler;

    public LoginUserConsumer(LoginUserHandler handler)
    {
        _handler = handler;
    }

    public async Task Consume(ConsumeContext<LoginUserRequest> context)
    {
        var result = await _handler.Handle(context.Message);
        await context.RespondAsync(result);
    }
}
