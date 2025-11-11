using System.Net;
using System.Text.Json;
using Contracts.Common;
using MassTransit;

namespace ApiGateway.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, errorCode, message) = exception switch
        {
            RequestTimeoutException => (
                (int)HttpStatusCode.GatewayTimeout,
                "TIMEOUT_ERROR",
                "The request to the service timed out. Please try again later."
            ),
            RequestFaultException faultEx => ParseFaultException(faultEx),
            UnauthorizedAccessException => (
                (int)HttpStatusCode.Unauthorized,
                "UNAUTHORIZED",
                "You are not authorized to access this resource."
            ),
            ArgumentException argEx => (
                (int)HttpStatusCode.BadRequest,
                ErrorCodes.VALIDATION_ERROR,
                argEx.Message
            ),
            KeyNotFoundException => (
                (int)HttpStatusCode.NotFound,
                ErrorCodes.RESOURCE_NOT_FOUND,
                "The requested resource was not found."
            ),
            InvalidOperationException invalidEx => (
                (int)HttpStatusCode.BadRequest,
                "INVALID_OPERATION",
                invalidEx.Message
            ),
            _ => (
                (int)HttpStatusCode.InternalServerError,
                ErrorCodes.INTERNAL_ERROR,
                "An internal server error occurred. Please try again later."
            )
        };

        context.Response.StatusCode = statusCode;

        var errorResponse = new ErrorResponse(
            StatusCode: statusCode,
            Message: message,
            Details: exception.GetType().Name
        );

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(errorResponse, options);
        await context.Response.WriteAsync(json);
    }

    private static (int statusCode, string errorCode, string message) ParseFaultException(RequestFaultException faultEx)
    {
        // Try to extract meaningful error from fault messages
        var faultMessage = faultEx.Message;

        if (faultMessage.Contains("already exists", StringComparison.OrdinalIgnoreCase))
        {
            return (
                (int)HttpStatusCode.Conflict,
                ErrorCodes.DUPLICATE_RESOURCE,
                "The resource already exists."
            );
        }

        if (faultMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return (
                (int)HttpStatusCode.NotFound,
                ErrorCodes.RESOURCE_NOT_FOUND,
                "The requested resource was not found."
            );
        }

        if (faultMessage.Contains("invalid", StringComparison.OrdinalIgnoreCase) ||
            faultMessage.Contains("validation", StringComparison.OrdinalIgnoreCase))
        {
            return (
                (int)HttpStatusCode.BadRequest,
                ErrorCodes.VALIDATION_ERROR,
                faultMessage
            );
        }

        if (faultMessage.Contains("unauthorized", StringComparison.OrdinalIgnoreCase) ||
            faultMessage.Contains("credentials", StringComparison.OrdinalIgnoreCase))
        {
            return (
                (int)HttpStatusCode.Unauthorized,
                ErrorCodes.INVALID_CREDENTIALS,
                "Invalid credentials provided."
            );
        }

        // Default to 500 for unhandled faults
        return (
            (int)HttpStatusCode.InternalServerError,
            ErrorCodes.INTERNAL_ERROR,
            "An error occurred while processing your request."
        );
    }
}
