using System.Collections.Generic;

namespace Contracts.Common;

public record ErrorResponse(
    int StatusCode,
    string Message,
    string? Details = null,
    Dictionary<string, string[]>? Errors = null
);

public record ValidationErrorResponse(
    int StatusCode,
    string Message,
    Dictionary<string, string[]> Errors
) : ErrorResponse(StatusCode, Message, null, Errors);

public class ErrorCodes
{
    // Auth errors (400-403)
    public const string INVALID_CREDENTIALS = "INVALID_CREDENTIALS";
    public const string USER_ALREADY_EXISTS = "USER_ALREADY_EXISTS";
    public const string USER_NOT_FOUND = "USER_NOT_FOUND";
    public const string EMAIL_NOT_VERIFIED = "EMAIL_NOT_VERIFIED";
    public const string INVALID_TOKEN = "INVALID_TOKEN";
    public const string TOKEN_EXPIRED = "TOKEN_EXPIRED";

    // Validation errors (400)
    public const string VALIDATION_ERROR = "VALIDATION_ERROR";
    public const string INVALID_EMAIL = "INVALID_EMAIL";
    public const string WEAK_PASSWORD = "WEAK_PASSWORD";

    // Resource errors (404)
    public const string RESOURCE_NOT_FOUND = "RESOURCE_NOT_FOUND";
    public const string URL_NOT_FOUND = "URL_NOT_FOUND";

    // Business logic errors (409)
    public const string DUPLICATE_RESOURCE = "DUPLICATE_RESOURCE";
    public const string EMAIL_ALREADY_VERIFIED = "EMAIL_ALREADY_VERIFIED";

    // Server errors (500)
    public const string INTERNAL_ERROR = "INTERNAL_ERROR";
    public const string DATABASE_ERROR = "DATABASE_ERROR";
    public const string EXTERNAL_SERVICE_ERROR = "EXTERNAL_SERVICE_ERROR";
}
