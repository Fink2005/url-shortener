using MassTransit;
using Contracts.Saga.Auth;

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

        // RegisterRequestedEvent will be consumed by UserOnboardingStateMachine
        // The Saga will handle sending confirmation email
        Console.WriteLine($"✅ [SagaService] Event processed. Saga will handle email confirmation.");
        Console.WriteLine($"========================================");
        
        await Task.CompletedTask;
    }
}
