namespace ApiGateway.Middleware;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Security Headers for Cloudflare/Browsers
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

        // Only add HSTS if behind Cloudflare/HTTPS proxy
        if (context.Request.Headers.ContainsKey("CF-RAY") ||
            context.Request.Headers.ContainsKey("X-Forwarded-Proto"))
        {
            var proto = context.Request.Headers["X-Forwarded-Proto"].ToString();
            if (proto == "https")
            {
                context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
            }
        }

        await _next(context);
    }
}
