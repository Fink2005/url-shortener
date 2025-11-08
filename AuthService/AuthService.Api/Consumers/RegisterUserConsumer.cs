using MassTransit;
using Contracts.Auth;
using AuthService.Application.Commands;

namespace AuthService.Api.Consumers;

public class RegisterUserConsumer : IConsumer<RegisterUserRequest>
{
    private readonly RegisterUserHandler _handler;

    public RegisterUserConsumer(RegisterUserHandler handler)
    {
        _handler = handler;
    }

    public async Task Consume(ConsumeContext<RegisterUserRequest> context)
    {
        var result = await _handler.Handle(context.Message);
        await context.RespondAsync(result);
    }
}
