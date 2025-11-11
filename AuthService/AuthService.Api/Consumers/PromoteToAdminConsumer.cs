using MassTransit;
using Contracts.Auth;
using AuthService.Application.Commands;

namespace AuthService.Api.Consumers;

public class PromoteToAdminConsumer : IConsumer<PromoteToAdminRequest>
{
    private readonly PromoteToAdminHandler _handler;

    public PromoteToAdminConsumer(PromoteToAdminHandler handler)
    {
        _handler = handler;
    }

    public async Task Consume(ConsumeContext<PromoteToAdminRequest> context)
    {
        try
        {
            Console.WriteLine($"📬 [AuthService] Received PromoteToAdminRequest for user {context.Message.UserId}");

            var response = await _handler.Handle(context.Message);

            await context.RespondAsync(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [AuthService] Error in PromoteToAdminConsumer: {ex.Message}");
            await context.RespondAsync(new PromoteToAdminResponse(false, ex.Message));
        }
    }
}
