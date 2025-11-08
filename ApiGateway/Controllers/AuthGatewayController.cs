using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Contracts.Auth;

namespace ApiGateway.Controllers;

[ApiController]
[Route("auth")]
[Tags("Auth Service")]
public class AuthGatewayController : ControllerBase
{
    private readonly IRequestClient<RegisterAuthRequest> _registerClient;
    private readonly IRequestClient<LoginAuthRequest> _loginClient;
    private readonly IRequestClient<RefreshTokenRequest> _refreshClient;
    private readonly IRequestClient<LogoutRequest> _logoutClient;
    private readonly IRequestClient<DeleteAuthRequest> _deleteClient;

    public AuthGatewayController(
        IRequestClient<RegisterAuthRequest> registerClient,
        IRequestClient<LoginAuthRequest> loginClient,
        IRequestClient<RefreshTokenRequest> refreshClient,
        IRequestClient<LogoutRequest> logoutClient,
        IRequestClient<DeleteAuthRequest> deleteClient)
    {
        _registerClient = registerClient;
        _loginClient = loginClient;
        _refreshClient = refreshClient;
        _logoutClient = logoutClient;
        _deleteClient = deleteClient;
    }

    // =========================
    // POST /auth/register
    // =========================
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterAuthRequest request)
    {
        var response = await _registerClient.GetResponse<RegisterAuthResponse>(request);
        return Ok(response.Message);
    }

    // =========================
    // POST /auth/login
    // =========================
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginAuthRequest request)
    {
        var response = await _loginClient.GetResponse<LoginAuthResponse>(request);
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

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var response = await _deleteClient.GetResponse<DeleteAuthResponse>(new DeleteAuthRequest(id));
        return Ok(response.Message);
    }

}
