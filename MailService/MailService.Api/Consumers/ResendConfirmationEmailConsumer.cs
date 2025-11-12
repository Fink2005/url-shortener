using System;
using System.Threading.Tasks;
using MassTransit;
using Contracts.Mail;
using MailService.Application.Abstractions;

namespace MailService.Api.Consumers;

public class ResendConfirmationEmailConsumer : IConsumer<ResendConfirmationEmailRequest>
{
    private readonly ITokenService _tokenService;
    private readonly IMailSender _mailSender;
    private readonly IPublishEndpoint _publishEndpoint;

    public ResendConfirmationEmailConsumer(
        ITokenService tokenService,
        IMailSender mailSender,
        IPublishEndpoint publishEndpoint)
    {
        _tokenService = tokenService;
        _mailSender = mailSender;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Consume(ConsumeContext<ResendConfirmationEmailRequest> context)
    {
        try
        {
            var email = context.Message.Email;
            Console.WriteLine($"📬 [MailService] Received resend confirmation email request for: {email}");

            // Generate new 6-digit verification code
            var random = new Random();
            var verificationCode = random.Next(100000, 999999).ToString();

            Console.WriteLine($"🔄 [MailService] Generating new 6-digit code for {email}...");

            var emailBody = $@"
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
                            .resend-notice {{ background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 12px; margin: 15px 0; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h1>Email Verification</h1>
                            </div>
                            <div class='content'>
                                <div class='resend-notice'>
                                    <strong>📨 Resent Verification Code</strong>
                                    <p style='margin: 5px 0 0 0; font-size: 14px;'>You requested a new verification code.</p>
                                </div>
                                <p>Hello!</p>
                                <p>Please verify your email address using the 6-digit code below:</p>
                                <div class='token-box'>
                                    <div class='token-code'>{verificationCode}</div>
                                </div>
                                <p class='expiry'>⏰ This verification code will expire in <strong>5 minutes</strong>.</p>
                                <p>If you did not request this code, please ignore this email.</p>
                                <div class='footer'>
                                    <p>&copy; {DateTime.UtcNow.Year} URL Shortener. All rights reserved.</p>
                                </div>
                            </div>
                        </div>
                    </body>
                </html>
            ";

            // Save new 6-digit code to Redis with 5-minute expiry (overwrite old code)
            await _tokenService.SaveTokenAsync(email, verificationCode, expiryMinutes: 5);

            await _mailSender.SendMailAsync(new Domain.Entities.MailRequest
            {
                To = email,
                Subject = "Email Verification - URL Shortener (Resent)",
                Body = emailBody
            });

            Console.WriteLine($"✅ [MailService] Verification email resent successfully to {email} (6-digit code: {verificationCode}, expires in 5 minutes)");

            await context.RespondAsync(new ResendConfirmationEmailResponse(
                true,
                "Verification email resent successfully"
            ));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [MailService] Error in ResendConfirmationEmailConsumer: {ex.Message}");

            await context.RespondAsync(new ResendConfirmationEmailResponse(
                false,
                $"Failed to resend email: {ex.Message}"
            ));
        }
    }
}
