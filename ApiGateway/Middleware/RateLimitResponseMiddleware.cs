using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace ApiGateway.Middleware;

/// <summary>
/// Middleware to customize rate limit exceeded response
/// </summary>
public class RateLimitResponseMiddleware
{
    private readonly RequestDelegate _next;

    public RateLimitResponseMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        // If rate limit exceeded (429 status)
        if (context.Response.StatusCode == 429)
        {
            context.Response.ContentType = "application/json";

            var retryAfter = context.Response.Headers["Retry-After"].ToString();

            var response = new
            {
                success = false,
                message = "Rate limit exceeded. Too many requests.",
                statusCode = 429,
                retryAfter = retryAfter,
                hint = "Please wait before making more requests."
            };

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
