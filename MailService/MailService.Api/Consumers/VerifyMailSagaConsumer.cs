using MassTransit;
using Contracts.Saga;
using MailService.Application.Abstractions;
using System;
using System.Threading.Tasks;

namespace MailService.Api.Consumers;

public class VerifyMailSagaConsumer : IConsumer<VerifyEmailRequestedEvent>
{
    private readonly ITokenService _tokenService;
    private readonly IPublishEndpoint _publishEndpoint;

    public VerifyMailSagaConsumer(ITokenService tokenService, IPublishEndpoint publishEndpoint)
    {
        _tokenService = tokenService;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Consume(ConsumeContext<VerifyEmailRequestedEvent> context)
    {
        try
        {
            Console.WriteLine($"📬 [MailService] Received VerifyEmailRequestedEvent for {context.Message.Email}");

            var email = context.Message.Email;
            var token = context.Message.Token;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
            {
                Console.WriteLine($"❌ [MailService] Email or token is empty");

                // Publish failure event
                await _publishEndpoint.Publish(new EmailVerificationFailedEvent(
                    Guid.NewGuid(),
                    email,
                    "Email and token are required"
                ));

                await context.RespondAsync(new VerifyEmailRequestedEvent(email, token));
                return;
            }

            // Verify token from Redis
            Console.WriteLine($"🔍 [MailService] Verifying token for {email}...");
            var isValid = await _tokenService.VerifyTokenAsync(email, token);

            if (isValid)
            {
                Console.WriteLine($"✅ [MailService] Token verified successfully for {email}");

                // Publish event to AuthService to update IsEmailVerified
                await _publishEndpoint.Publish(new EmailVerifiedEvent(
                    Guid.NewGuid(),
                    email
                ));

                Console.WriteLine($"📨 [MailService] Published EmailVerifiedEvent to AuthService");
            }
            else
            {
                Console.WriteLine($"❌ [MailService] Invalid or expired token for {email}");

                // Publish failure event
                await _publishEndpoint.Publish(new EmailVerificationFailedEvent(
                    Guid.NewGuid(),
                    email,
                    "Invalid or expired token"
                ));
            }

            // Respond back to Gateway
            await context.RespondAsync(new VerifyEmailRequestedEvent(email, token));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [MailService] Error in VerifyMailSagaConsumer: {ex.Message}");

            // Publish failure event
            await _publishEndpoint.Publish(new EmailVerificationFailedEvent(
                Guid.NewGuid(),
                context.Message.Email,
                $"System error: {ex.Message}"
            ));

            throw;
        }
    }
}
