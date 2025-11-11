using System;
using System.Threading.Tasks;
using MassTransit;
using MailService.Application.Abstractions;
using Contracts.Mail;

namespace MailService.Api.Consumers;

public class VerifyEmailConsumer : IConsumer<VerifyEmailRequest>
{
    private readonly ITokenService _tokenService;

    public VerifyEmailConsumer(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    public async Task Consume(ConsumeContext<VerifyEmailRequest> context)
    {
        var message = context.Message;

        if (string.IsNullOrWhiteSpace(message.Email) || string.IsNullOrWhiteSpace(message.Token))
        {
            await context.RespondAsync(new VerifyEmailResponse(false, "Email and token are required"));
            return;
        }

        var isValid = await _tokenService.VerifyTokenAsync(message.Email, message.Token);

        if (isValid)
        {
            await context.RespondAsync(new VerifyEmailResponse(true, "Email verified successfully"));
            Console.WriteLine($"✓ Email verified for {message.Email}");
        }
        else
        {
            await context.RespondAsync(new VerifyEmailResponse(false, "Invalid or expired token"));
            Console.WriteLine($"✗ Email verification failed for {message.Email}");
        }
    }
}

public class CheckEmailTokenConsumer : IConsumer<CheckEmailTokenRequest>
{
    private readonly ITokenService _tokenService;

    public CheckEmailTokenConsumer(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    public async Task Consume(ConsumeContext<CheckEmailTokenRequest> context)
    {
        var message = context.Message;
        var token = await _tokenService.GetTokenAsync(message.Email);

        if (token == null)
        {
            await context.RespondAsync(new CheckEmailTokenResponse(false, "No active token for this email", null));
        }
        else
        {
            await context.RespondAsync(new CheckEmailTokenResponse(true, "Token exists", token));
        }
    }
}
