using Microsoft.AspNetCore.Mvc;
using MassTransit;

namespace ApiGateway.Extensions;

public static class ErrorHandlingExtensions
{
    public static IActionResult HandleRequestFault(this ControllerBase controller, RequestFaultException faultEx, string defaultMessage = "An error occurred while processing your request")
    {
        var fault = faultEx.Fault;
        if (fault.Exceptions == null || !fault.Exceptions.Any())
        {
            return controller.StatusCode(500, new { message = defaultMessage });
        }

        var firstException = fault.Exceptions.First();
        var exceptionType = firstException.ExceptionType ?? "";
        var message = firstException.Message;

        // Unauthorized - 401
        if (exceptionType.Contains("UnauthorizedAccessException"))
        {
            return controller.Unauthorized(new { message });
        }

        // Forbidden - 403
        if (exceptionType.Contains("ForbiddenException") || message.Contains("forbidden", StringComparison.OrdinalIgnoreCase))
        {
            return controller.StatusCode(403, new { message });
        }

        // Not Found - 404
        if (exceptionType.Contains("NotFoundException") ||
            exceptionType.Contains("KeyNotFoundException") ||
            message.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return controller.NotFound(new { message });
        }

        // Conflict - 409
        if (exceptionType.Contains("InvalidOperationException") &&
            (message.Contains("already exists", StringComparison.OrdinalIgnoreCase) ||
             message.Contains("duplicate", StringComparison.OrdinalIgnoreCase)))
        {
            return controller.Conflict(new { message });
        }

        // Validation / Bad Request - 400
        if (exceptionType.Contains("ValidationException") ||
            exceptionType.Contains("ArgumentException") ||
            exceptionType.Contains("FormatException"))
        {
            return controller.BadRequest(new { message });
        }

        // Default to 500
        return controller.StatusCode(500, new { message = defaultMessage, details = message });
    }

    public static async Task<IActionResult> ExecuteWithErrorHandling<TResponse>(
        this ControllerBase controller,
        IRequestClient<TResponse> client,
        TResponse request,
        string errorMessage = "An error occurred while processing your request") where TResponse : class
    {
        try
        {
            var response = await client.GetResponse<TResponse>(request);
            return controller.Ok(response.Message);
        }
        catch (RequestTimeoutException)
        {
            return controller.StatusCode(504, new { message = "The request timed out. Please try again." });
        }
        catch (RequestFaultException faultEx)
        {
            return controller.HandleRequestFault(faultEx, errorMessage);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [Gateway] Unexpected error: {ex.Message}");
            return controller.StatusCode(500, new { message = errorMessage });
        }
    }
}
