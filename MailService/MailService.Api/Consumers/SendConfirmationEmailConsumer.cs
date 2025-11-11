using System;
using System.Threading.Tasks;
using MassTransit;
using Contracts.Mail;
using MailService.Application.Abstractions;

namespace MailService.Api.Consumers;

public class SendConfirmationEmailConsumer : IConsumer<SendConfirmationEmailRequest>
{
    private readonly IMailSender _mailSender;
    private readonly ITokenService _tokenService;

    public SendConfirmationEmailConsumer(IMailSender mailSender, ITokenService tokenService)
    {
        _mailSender = mailSender;
        _tokenService = tokenService;
    }

    public async Task Consume(ConsumeContext<SendConfirmationEmailRequest> context)
    {
        var message = context.Message;
        Console.WriteLine($"[SendConfirmationEmailConsumer] Processing email for: {message.Email}");

        try
        {
            // Generate token (6 digits)
            var token = new Random().Next(100000, 999999).ToString();
            
            // Save token to Redis (5 minutes)
            await _tokenService.SaveTokenAsync(message.Email, token, 5);
            Console.WriteLine($"[SendConfirmationEmailConsumer] Token saved to Redis: {message.Email}");

            // Prepare email
            var emailBody = $@"
            <html>
                <body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333;"">
                    <h2 style=""color: #0066cc;"">Email Verification</h2>
                    <p>Your verification code:</p>
                    <h1 style=""color: #0066cc; letter-spacing: 5px;"">{token}</h1>
                    <p>This code will expire in <strong>5 minutes</strong>.</p>
                    <p>If you didn't request this, please ignore this email.</p>
                </body>
            </html>";

            var mailRequest = new Domain.Entities.MailRequest
            {
                To = message.Email,
                Subject = "Email Verification Code",
                Body = emailBody
            };

            // Send email
            await _mailSender.SendMailAsync(mailRequest);
            Console.WriteLine($"[SendConfirmationEmailConsumer] Email sent to: {message.Email}");

            // Respond to client
            await context.RespondAsync(new SendConfirmationEmailResponse(true));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SendConfirmationEmailConsumer] Error: {ex.Message}");
            await context.RespondAsync(new SendConfirmationEmailResponse(false));
        }
    }
}
