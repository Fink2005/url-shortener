
using MassTransit;
using Contracts.Auth;
using AuthService.Application.Commands;

namespace AuthService.Api.Consumers;

public class DeleteAuthConsumer : IConsumer<DeleteAuthRequest>
{
    private readonly DeleteAuthHandler _handler;

    public DeleteAuthConsumer(DeleteAuthHandler handler)
    {
        _handler = handler;
    }

    public async Task Consume(ConsumeContext<DeleteAuthRequest> context)
    {
        var result = await _handler.Handle(context.Message);
        await context.RespondAsync(result);
    }
}
