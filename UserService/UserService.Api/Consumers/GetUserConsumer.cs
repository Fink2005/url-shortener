// UserService.Api/Consumers/GetUserConsumer.cs
using MassTransit;
using Contracts.Users;
using UserService.Application.Users.Queries;

namespace UserService.Api.Consumers;

public class GetUserConsumer : IConsumer<GetUserRequest>
{
    private readonly GetUserHandler _handler;

    public GetUserConsumer(GetUserHandler handler) => _handler = handler;

    public async Task Consume(ConsumeContext<GetUserRequest> context)
    {
        var response = await _handler.Handle(context.Message);
        await context.RespondAsync(response);
    }
}
