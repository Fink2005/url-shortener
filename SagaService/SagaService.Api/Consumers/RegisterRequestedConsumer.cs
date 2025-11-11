using MassTransit;
using Contracts.Saga.Auth;
using Contracts.Mail;

namespace SagaService.Api.Consumers;

public class RegisterRequestedConsumer : IConsumer<RegisterRequestedEvent>
{
    private readonly IRequestClient<SendConfirmationEmailRequest> _mailClient;

    public RegisterRequestedConsumer(IRequestClient<SendConfirmationEmailRequest> mailClient)
    {
        _mailClient = mailClient;
    }

    public async Task Consume(ConsumeContext<RegisterRequestedEvent> context)
    {
        var msg = context.Message;

        Console.WriteLine($"📬 ========================================");
        Console.WriteLine($"📬 [SagaService] Received RegisterRequestedEvent!");
        Console.WriteLine($"📬 [SagaService] Email: {msg.Email}");
        Console.WriteLine($"📬 [SagaService] Username: {msg.Username}");
        Console.WriteLine($"📬 ========================================");

        Console.WriteLine($"📤 [SagaService] Requesting MailService to send confirmation email...");

        // Gửi mail xác nhận
        await _mailClient.GetResponse<SendConfirmationEmailResponse>(
            new SendConfirmationEmailRequest(msg.Email)
        );

        Console.WriteLine($"✅ [SagaService] Confirmation email sent successfully for {msg.Email}");
        Console.WriteLine($"========================================");
    }
}
