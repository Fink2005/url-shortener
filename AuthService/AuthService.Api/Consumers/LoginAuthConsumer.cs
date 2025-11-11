using MassTransit;
using Contracts.Auth;
using Contracts.Common;
using FluentValidation;
using AuthService.Application.Commands;

namespace AuthService.Api.Consumers;

public class LoginAuthConsumer : IConsumer<LoginAuthRequest>
{
    private readonly LoginAuthHandler _handler;

    public LoginAuthConsumer(LoginAuthHandler handler)
    {
        _handler = handler;
    }

    public async Task Consume(ConsumeContext<LoginAuthRequest> context)
    {
        try
        {
            var result = await _handler.Handle(context.Message);
            await context.RespondAsync(result);
        }
        catch (ValidationException valEx)
        {
            var errors = valEx.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            Console.WriteLine($"❌ [AuthService] Validation failed for login: {string.Join(", ", errors.Keys)}");

            await context.RespondAsync(new ErrorResponse(
                StatusCode: 400,
                Message: "Validation failed",
                Details: ErrorCodes.VALIDATION_ERROR,
                Errors: errors
            ));
        }
        catch (UnauthorizedAccessException unauthEx)
        {
            Console.WriteLine($"❌ [AuthService] Unauthorized: {unauthEx.Message}");
            throw; // Re-throw để MassTransit tạo Fault
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [AuthService] Error in LoginAuthConsumer: {ex.Message}");
            throw; // Re-throw để MassTransit tạo Fault
        }
    }
}
