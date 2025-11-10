using MassTransit;
using Contracts.Users;
using UserService.Application.Commands;

namespace UserService.Api.Consumers;

public class DeleteUserConsumer : IConsumer<DeleteUserRequest>
{
    private readonly DeleteUserHandler _handler;

    public DeleteUserConsumer(DeleteUserHandler handler) => _handler = handler;

    public async Task Consume(ConsumeContext<DeleteUserRequest> context)
    {
        var response = await _handler.Handle(context.Message);
        await context.RespondAsync(response);
    }
}
