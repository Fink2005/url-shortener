using MassTransit;
using Contracts.Saga.Auth;
using Contracts.Auth;

namespace SagaService.Api.Consumers;

public class RegisterRequestedConsumer : IConsumer<RegisterRequestedEvent>
{
    public RegisterRequestedConsumer()
    {
    }

    public async Task Consume(ConsumeContext<RegisterRequestedEvent> context)
    {
        var msg = context.Message;

        Console.WriteLine($"📬 ========================================");
        Console.WriteLine($"📬 [SagaService] Received RegisterRequestedEvent!");
        Console.WriteLine($"📬 [SagaService] Email: {msg.Email}");
        Console.WriteLine($"📬 [SagaService] Username: {msg.Username}");
        Console.WriteLine($"📬 ========================================");

        // Publish RegisterAuthRequest to start UserOnboardingStateMachine
        await context.Publish(new RegisterAuthRequest(
            msg.Username,
            msg.Email,
            msg.Password
        ));

        Console.WriteLine($"✅ [SagaService] Published RegisterAuthRequest to start Saga");
        Console.WriteLine($"========================================");
    }
}
