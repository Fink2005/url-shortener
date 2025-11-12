using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Contracts.Auth;
using Contracts.Saga;
using Contracts.Saga.Auth;

namespace ApiGateway.Controllers;

[ApiController]
[Route("auth")]
[Tags("Auth Service")]
public class AuthGatewayController : ControllerBase
{
    private readonly IRequestClient<LoginAuthRequest> _loginClient;
    private readonly IRequestClient<RefreshTokenRequest> _refreshClient;
    private readonly IRequestClient<LogoutRequest> _logoutClient;
    private readonly IRequestClient<RegisterRequestedEvent> _registerClient;
    private readonly IRequestClient<VerifyEmailRequestedEvent> _verifyEmailClient;

    public AuthGatewayController(
        IRequestClient<LoginAuthRequest> loginClient,
        IRequestClient<RefreshTokenRequest> refreshClient,
        IRequestClient<LogoutRequest> logoutClient,
        IRequestClient<RegisterRequestedEvent> registerClient,
        IRequestClient<VerifyEmailRequestedEvent> verifyEmailClient)
    {
        _loginClient = loginClient;
        _refreshClient = refreshClient;
        _logoutClient = logoutClient;
        _registerClient = registerClient;
        _verifyEmailClient = verifyEmailClient;
    }

    // =========================
    // POST /auth/register
    // =========================
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestedEvent request)
    {
        try
        {
            Console.WriteLine($"📬 [Gateway] Received register request for {request.Email}");

            var response = await _registerClient.GetResponse<RegisterRequestedEvent>(request);

            Console.WriteLine($"✅ [Gateway] Registration completed for {request.Email}");

            return Ok(response.Message);
        }
        catch (RequestTimeoutException)
        {
            Console.WriteLine($"⏱️ [Gateway] Register request timeout");
            return StatusCode(408, new
            {
                success = false,
                message = "Request timeout"
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [Gateway] Register error: {ex.Message}");
            return StatusCode(500, new
            {
                success = false,
                message = ex.Message
            });
        }
    }

    // =========================
    // POST /auth/verify-email
    // =========================
    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequestDto request)
    {
        try
        {
            // ✅ Validate input
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Token))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Email and token are required"
                });
            }

            Console.WriteLine($"📬 [Gateway] Received verify email request for {request.Email}");

            // Send request to MailService for token validation
            var response = await _verifyEmailClient.GetResponse<VerifyEmailResponse>(
                new VerifyEmailRequestedEvent(request.Email, request.Token),
                timeout: TimeSpan.FromSeconds(10)
            );

            var result = response.Message;

            if (result.Success)
            {
                Console.WriteLine($"✅ [Gateway] Email verified successfully for {request.Email}");
                return Ok(new
                {
                    success = true,
                    message = "Email verified successfully. User profile is being created."
                });
            }
            else
            {
                Console.WriteLine($"❌ [Gateway] Email verification failed: {result.Message}");
                return BadRequest(new
                {
                    success = false,
                    message = result.Message
                });
            }
        }
        catch (RequestTimeoutException)
        {
            Console.WriteLine($"⏱️ [Gateway] Email verification timeout");
            return StatusCode(408, new
            {
                success = false,
                message = "Request timeout. Please try again."
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [Gateway] Email verification error: {ex.Message}");
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred during email verification"
            });
        }
    }

    // =========================
    // POST /auth/resend-verification
    // =========================
    [HttpPost("resend-verification")]
    public async Task<IActionResult> ResendVerificationEmail([FromBody] ResendVerificationEmailDto request)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Email is required"
                });
            }

            Console.WriteLine($"📬 [Gateway] Resend verification email request for {request.Email}");

            // Send request to MailService to resend confirmation email
            var busControl = _registerClient as IBus 
                ?? throw new InvalidOperationException("Cannot access message bus");

            // Publish event to resend email (MailService will handle it)
            await busControl.Publish(new Contracts.Mail.ResendConfirmationEmailRequest(request.Email));

            Console.WriteLine($"✅ [Gateway] Resend verification request published for {request.Email}");

            return Ok(new
            {
                success = true,
                message = "If your email is registered and not verified, a new verification link will be sent. Please check your inbox."
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [Gateway] Resend verification error: {ex.Message}");
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while resending verification email"
            });
        }
    }

    // =========================
    // POST /auth/login
    // =========================
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginAuthRequest request)
    {
        try
        {
            var response = await _loginClient.GetResponse<LoginAuthResponse>(request);
            return Ok(response.Message);
        }
        catch (RequestFaultException faultEx)
        {
            // MassTransit wraps service errors in RequestFaultException
            var fault = faultEx.Fault;
            if (fault.Exceptions != null && fault.Exceptions.Any())
            {
                var firstException = fault.Exceptions.First();
                var message = firstException.Message;

                // Check if it's an UnauthorizedAccessException
                if (firstException.ExceptionType?.Contains("UnauthorizedAccessException") == true)
                {
                    return Unauthorized(new { message });
                }

                // Check if it's a ValidationException
                if (firstException.ExceptionType?.Contains("ValidationException") == true)
                {
                    return BadRequest(new { message });
                }
            }

            return StatusCode(500, new { message = "An error occurred while processing your request" });
        }
    }

    // =========================
    // POST /auth/refresh
    // =========================
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var response = await _refreshClient.GetResponse<RefreshTokenResponse>(request);
        return Ok(response.Message);
    }

    // =========================
    // POST /auth/logout
    // =========================
    [HttpPost("logout")]
    [Authorize(Policy = "Authenticated")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        var response = await _logoutClient.GetResponse<LogoutResponse>(request);
        return Ok(response.Message);
    }

}

// DTOs for API Gateway requests
public record VerifyEmailRequestDto(string Email, string Token);
public record ResendVerificationEmailDto(string Email, string? Username = null);
