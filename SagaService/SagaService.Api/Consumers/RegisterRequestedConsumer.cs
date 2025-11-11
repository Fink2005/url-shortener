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

        Console.WriteLine($"📬 Saga: Received RegisterRequestedEvent for {msg.Email}");

        // Gửi mail xác nhận
        await _mailClient.GetResponse<SendConfirmationEmailResponse>(
            new SendConfirmationEmailRequest(msg.Email)
        );

        Console.WriteLine($"📨 Saga: Sent confirmation email for {msg.Email}");
    }
}
