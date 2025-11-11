using Microsoft.AspNetCore.Mvc;
using MailService.Application.Abstractions;

namespace MailService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Email Verification")]
public class VerificationController : ControllerBase
{
    private readonly ITokenService _tokenService;

    public VerificationController(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    /// <summary>
    /// Verify email confirmation token
    /// </summary>
    /// <remarks>
    /// Token expires in 5 minutes
    /// 
    /// Example:
    /// POST /api/verification/verify
    /// {
    ///   "email": "user@example.com",
    ///   "token": "abc123xyz"
    /// }
    /// </remarks>
    [HttpPost("verify")]
    public async Task<IActionResult> VerifyToken([FromBody] VerifyEmailRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Token))
        {
            return BadRequest(new { success = false, message = "Email and token are required" });
        }

        var isValid = await _tokenService.VerifyTokenAsync(request.Email, request.Token);

        if (isValid)
        {
            return Ok(new { success = true, message = "Email verified successfully" });
        }

        return Unauthorized(new { success = false, message = "Invalid or expired token" });
    }

    /// <summary>
    /// Check if a token exists for an email (debugging only)
    /// </summary>
    [HttpGet("check/{email}")]
    public async Task<IActionResult> CheckToken(string email)
    {
        var token = await _tokenService.GetTokenAsync(email);

        if (token == null)
        {
            return NotFound(new { success = false, message = "No active token for this email" });
        }

        return Ok(new { success = true, message = "Token exists", token });
    }
}

public class VerifyEmailRequest
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}
