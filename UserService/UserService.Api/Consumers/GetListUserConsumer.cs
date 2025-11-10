// UserService.Api/Consumers/GetUserConsumer.cs
using MassTransit;
using Contracts.Users;
using UserService.Application.Queries;

namespace UserService.Api.Consumers;

public class GetListUserConsumer : IConsumer<GetListUsersRequest>
{
    private readonly GetListUserHandler _handler;

    public GetListUserConsumer(GetListUserHandler handler) => _handler = handler;

    public async Task Consume(ConsumeContext<GetListUsersRequest> context)
    {
        var response = await _handler.Handle(context.Message);
        await context.RespondAsync(response);
    }
}
