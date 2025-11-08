namespace UserService.Api.Consumers;

using MassTransit;
using Contracts.Users;
using UserService.Application.Users.Commands;

public class CreateUserConsumer : IConsumer<CreateUserRequest>
{
    private readonly CreateUserHandler _handler;

    public CreateUserConsumer(CreateUserHandler handler)
    {
        _handler = handler;
    }

    public async Task Consume(ConsumeContext<CreateUserRequest> context)
    {
        var response = await _handler.Handle(context.Message);
        await context.RespondAsync(response);
    }
}
