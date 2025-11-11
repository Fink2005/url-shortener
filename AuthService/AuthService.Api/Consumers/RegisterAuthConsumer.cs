using MassTransit;
using Contracts.Auth;
using FluentValidation;
using AuthService.Application.Commands;

namespace AuthService.Api.Consumers;

public class RegisterAuthConsumer : IConsumer<RegisterAuthRequest>
{
    private readonly RegisterAuthHandler _handler;

    public RegisterAuthConsumer(RegisterAuthHandler handler)
    {
        _handler = handler;
    }

    public async Task Consume(ConsumeContext<RegisterAuthRequest> context)
    {
        try
        {
            var result = await _handler.Handle(context.Message);
            await context.RespondAsync(result);
        }
        catch (ValidationException valEx)
        {
            Console.WriteLine($"❌ [AuthService] Validation failed for registration: {valEx.Message}");
            throw new ArgumentException($"Validation failed: {string.Join(", ", valEx.Errors.Select(e => e.ErrorMessage))}");
        }
        catch (InvalidOperationException invalidEx)
        {
            Console.WriteLine($"❌ [AuthService] Invalid operation during registration: {invalidEx.Message}");
            throw;
        }
        catch (ArgumentException argEx)
        {
            Console.WriteLine($"❌ [AuthService] Argument error during registration: {argEx.Message}");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [AuthService] Error in RegisterAuthConsumer: {ex.Message}");
            throw new InvalidOperationException("An error occurred during registration", ex);
        }
    }
}
