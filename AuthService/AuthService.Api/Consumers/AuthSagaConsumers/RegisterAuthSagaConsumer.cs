using MassTransit;
using Contracts.Saga.Auth;
using AuthService.Application.Commands;
using Contracts.Auth;

namespace AuthService.Api.Saga.Consumers;

public class RegisterAuthSagaConsumer : IConsumer<RegisterRequestedEvent>
{
    private readonly RegisterAuthHandler _handler;

    public RegisterAuthSagaConsumer(RegisterAuthHandler handler)
    {
        _handler = handler;
    }

    public async Task Consume(ConsumeContext<RegisterRequestedEvent> context)
    {
        try
        {
            // Convert Saga event to handler request
            var request = new RegisterAuthRequest(
                context.Message.Username,
                context.Message.Email,
                context.Message.Password
            );
            
            var result = await _handler.Handle(request);

            // Respond back to Gateway
            await context.RespondAsync(result);
            Console.WriteLine($"✅ RegisterAuthSagaConsumer completed for {context.Message.Email}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in RegisterAuthSagaConsumer: {ex.Message}");
            throw;
        }
    }
}
