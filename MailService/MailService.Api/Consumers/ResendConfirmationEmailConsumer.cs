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

            // Check if token exists in Redis (user registered but not verified yet)
            var existingToken = await _tokenService.GetTokenAsync(email);

            if (string.IsNullOrEmpty(existingToken))
            {
                Console.WriteLine($"⚠️ [MailService] No pending verification found for {email}");
                await context.RespondAsync(new ResendConfirmationEmailResponse(
                    false,
                    "Email not found or already verified"
                ));
                return;
            }

            // Token exists, resend the email with the same token
            Console.WriteLine($"✅ [MailService] Found existing token for {email}, resending email...");

            var verificationLink = $"https://api.url-shortener.site/auth/verify-email?email={Uri.EscapeDataString(email)}&token={existingToken}";

            var emailBody = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #333;'>Verify Your Email - URL Shortener</h2>
                        <p>Hello!</p>
                        <p>You requested to resend your email verification link. Click the button below to verify your email address:</p>
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{verificationLink}' 
                               style='background-color: #4CAF50; 
                                      color: white; 
                                      padding: 12px 30px; 
                                      text-decoration: none; 
                                      border-radius: 4px;
                                      display: inline-block;'>
                                Verify Email
                            </a>
                        </div>
                        <p>Or copy and paste this link into your browser:</p>
                        <p style='word-break: break-all; color: #0066cc;'>{verificationLink}</p>
                        <p style='color: #666; font-size: 12px; margin-top: 30px;'>
                            This link will expire in 24 hours. If you didn't request this, please ignore this email.
                        </p>
                    </div>
                </body>
                </html>";

            await _mailSender.SendMailAsync(new Domain.Entities.MailRequest
            {
                To = email,
                Subject = "Verify Your Email - URL Shortener (Resent)",
                Body = emailBody
            });

            Console.WriteLine($"✅ [MailService] Verification email resent successfully to: {email}");

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
