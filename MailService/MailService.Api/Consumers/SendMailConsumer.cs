using System;
using System.Threading.Tasks;
using MassTransit;
using MailService.Application.Abstractions;
using Contracts.Saga;
using MailService.Domain.Entities;

namespace MailService.Api.Consumers;

public class SendMailConsumer : IConsumer<SendConfirmationEmailCommand>
{
    private readonly IMailSender _mailSender;
    private readonly ITokenService _tokenService;

    public SendMailConsumer(IMailSender mailSender, ITokenService tokenService)
    {
        _mailSender = mailSender;
        _tokenService = tokenService;
    }

    public async Task Consume(ConsumeContext<SendConfirmationEmailCommand> context)
    {
        var message = context.Message;

        try
        {
            // Generate 6-digit verification code
            var random = new Random();
            var verificationCode = random.Next(100000, 999999).ToString();

            var htmlBody = $@"
                <html>
                    <head>
                        <style>
                            body {{ font-family: Arial, sans-serif; }}
                            .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                            .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; }}
                            .content {{ padding: 20px; background-color: #f9f9f9; }}
                            .token-box {{ background-color: #fff; border: 2px solid #007bff; padding: 15px; text-align: center; margin: 20px 0; }}
                            .token-code {{ font-size: 32px; font-weight: bold; color: #007bff; letter-spacing: 4px; }}
                            .footer {{ font-size: 12px; color: #999; text-align: center; padding-top: 20px; }}
                            .expiry {{ color: #ff6b6b; font-weight: bold; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h1>Email Verification</h1>
                            </div>
                            <div class='content'>
                                <p>Hello!</p>
                                <p>Thank you for registering. Please verify your email address using the 6-digit code below:</p>
                                <div class='token-box'>
                                    <div class='token-code'>{verificationCode}</div>
                                </div>
                                <p class='expiry'>⏰ This verification code will expire in <strong>5 minutes</strong>.</p>
                                <p>If you did not create this account, please ignore this email.</p>
                                <div class='footer'>
                                    <p>&copy; {DateTime.UtcNow.Year} URL Shortener. All rights reserved.</p>
                                </div>
                            </div>
                        </div>
                    </body>
                </html>
            ";

            var mailRequest = new MailRequest
            {
                To = message.Email,
                Subject = "Email Verification - URL Shortener",
                Body = htmlBody
            };

            // Save 6-digit code to Redis with 5-minute expiry
            await _tokenService.SaveTokenAsync(message.Email, verificationCode, expiryMinutes: 5);

            // Send email
            await _mailSender.SendMailAsync(mailRequest);

            // Publish event that email was sent successfully
            await context.Publish(new EmailConfirmationSent(message.CorrelationId));

            Console.WriteLine($"✓ Verification email sent to {message.Email} (6-digit code: {verificationCode}, expires in 5 minutes)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Failed to send verification email to {message.Email}: {ex.Message}");
            throw;
        }
    }
}
