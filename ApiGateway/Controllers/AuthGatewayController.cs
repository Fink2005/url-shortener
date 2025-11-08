using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Contracts.Auth;

namespace ApiGateway.Controllers;

[ApiController]
[Route("auth")]
[Tags("Auth Service")]
public class AuthGatewayController : ControllerBase
{
    private readonly IRequestClient<RegisterUserRequest> _registerClient;
    private readonly IRequestClient<LoginUserRequest> _loginClient;
    private readonly IRequestClient<RefreshTokenRequest> _refreshClient;
    private readonly IRequestClient<LogoutRequest> _logoutClient;

    public AuthGatewayController(
        IRequestClient<RegisterUserRequest> registerClient,
        IRequestClient<LoginUserRequest> loginClient,
        IRequestClient<RefreshTokenRequest> refreshClient,
        IRequestClient<LogoutRequest> logoutClient)
    {
        _registerClient = registerClient;
        _loginClient = loginClient;
        _refreshClient = refreshClient;
        _logoutClient = logoutClient;
    }

    // =========================
    // POST /auth/register
    // =========================
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
    {
        var response = await _registerClient.GetResponse<RegisterUserResponse>(request);
        return Ok(response.Message);
    }

    // =========================
    // POST /auth/login
    // =========================
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserRequest request)
    {
        var response = await _loginClient.GetResponse<LoginUserResponse>(request);
        return Ok(response.Message);
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
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        var response = await _logoutClient.GetResponse<LogoutResponse>(request);
        return Ok(response.Message);
    }
}
