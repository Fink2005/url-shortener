using Microsoft.AspNetCore.Mvc;
using MassTransit;
using Contracts.Saga.Auth;

namespace SagaGateway.Controllers;

[ApiController]
[Route("saga")]
[Tags("Auth Service Saga")]
public class RegisterGatewayController : ControllerBase
{
    private readonly IRequestClient<RegisterRequestedEvent> _client;

    public RegisterGatewayController(IRequestClient<RegisterRequestedEvent> client)
    {
        _client = client;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequestedEvent request)
    {
        var response = await _client.GetResponse<RegisterRequestedEvent>(request);
        return Ok(response.Message);
    }
}
