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
        try
        {
            var result = await _handler.Handle(context.Message);
            // Respond to gateway
            await context.RespondAsync(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error in RegisterAuthConsumer: {ex.Message}");
            throw;
        }
    }
}
