using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Contracts.Users;

namespace ApiGateway.Controllers;

[ApiController]
[Route("users")]
[Tags("User Gateway")]
public class UserGatewayController : ControllerBase
{
    private readonly IRequestClient<GetUserRequest> _getUserClient;
    private readonly IRequestClient<CreateUserRequest> _createUserClient;
    private readonly IRequestClient<DeleteUserRequest> _deleteUserClient;

    public UserGatewayController(
        IRequestClient<GetUserRequest> getUserClient,
        IRequestClient<CreateUserRequest> createUserClient,
        IRequestClient<DeleteUserRequest> deleteUserClient
        )
    {
        _getUserClient = getUserClient;
        _createUserClient = createUserClient;
        _deleteUserClient = deleteUserClient;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var response = await _getUserClient.GetResponse<GetUserResponse>(new GetUserRequest(id));
        return Ok(response.Message);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        try
        {
            var response = await _createUserClient.GetResponse<CreateUserResponse>(request);
            return Ok(response.Message);
        }
        catch (RequestFaultException ex)
        {
            // MassTransit trả lỗi ValidationFault → 400 cho client
            return BadRequest(new
            {
                status = 400,
                error = "Validation failed",
                details = ex.Message
            });
        }
        catch (Exception ex)
        {
            // Những lỗi khác (timeout, network...) vẫn là 500
            return StatusCode(500, new
            {
                error = "Internal server error",
                details = ex.Message
            });
        }
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var response = await _deleteUserClient.GetResponse<DeleteUserResponse>(new DeleteUserRequest(id));
        return Ok(response.Message);
    }
}
