using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Contracts.Saga;

namespace ApiGateway.Controllers;

[ApiController]
[Route("admin/dashboard")]
[Tags("Admin Dashboard")]
[Authorize(Policy = "AdminOnly")]
public class AdminDashboardController : ControllerBase
{
    private readonly IRequestClient<GetUserWithUrlsRequest> _getUserWithUrlsClient;
    private readonly IRequestClient<GetAllUsersWithUrlsRequest> _getAllUsersWithUrlsClient;
    private readonly IRequestClient<DeleteUserSagaRequest> _deleteUserSagaClient;

    public AdminDashboardController(
        IRequestClient<GetUserWithUrlsRequest> getUserWithUrlsClient,
        IRequestClient<GetAllUsersWithUrlsRequest> getAllUsersWithUrlsClient,
        IRequestClient<DeleteUserSagaRequest> deleteUserSagaClient)
    {
        _getUserWithUrlsClient = getUserWithUrlsClient;
        _getAllUsersWithUrlsClient = getAllUsersWithUrlsClient;
        _deleteUserSagaClient = deleteUserSagaClient;
    }

    /// <summary>
    /// Get user details with all their created URLs (Admin only)
    /// </summary>
    [HttpGet("users/{userId}")]
    public async Task<IActionResult> GetUserWithUrls(Guid userId)
    {
        try
        {
            Console.WriteLine($"📥 [Gateway] Admin requesting user with URLs: {userId}");

            var response = await _getUserWithUrlsClient.GetResponse<GetUserWithUrlsResponse>(
                new GetUserWithUrlsRequest(userId),
                timeout: RequestTimeout.After(s: 30)
            );

            Console.WriteLine($"✅ [Gateway] Successfully retrieved user data for: {userId}");
            return Ok(response.Message.UserWithUrls);
        }
        catch (RequestFaultException faultEx)
        {
            var firstException = faultEx.Fault.Exceptions?.First();
            var message = firstException?.Message ?? "Failed to retrieve user data";

            Console.WriteLine($"❌ [Gateway] Request fault: {message}");

            if (message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { message = "User not found" });
            }

            return StatusCode(500, new { message });
        }
        catch (RequestTimeoutException)
        {
            Console.WriteLine($"⏰ [Gateway] Request timeout for userId: {userId}");
            return StatusCode(504, new { message = "Request timeout while retrieving user data" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [Gateway] Error: {ex.Message}");
            return StatusCode(500, new { message = "An error occurred while retrieving user data" });
        }
    }

    /// <summary>
    /// Get all users with their created URLs (Admin only)
    /// </summary>
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsersWithUrls()
    {
        try
        {
            Console.WriteLine($"📥 [Gateway] Admin requesting all users with URLs");

            var response = await _getAllUsersWithUrlsClient.GetResponse<GetAllUsersWithUrlsResponse>(
                new GetAllUsersWithUrlsRequest(),
                timeout: RequestTimeout.After(m: 2)
            );

            Console.WriteLine($"✅ [Gateway] Successfully retrieved {response.Message.Users.Count} users");
            return Ok(response.Message.Users);
        }
        catch (RequestTimeoutException)
        {
            Console.WriteLine($"⏰ [Gateway] Request timeout for get all users");
            return StatusCode(504, new { message = "Request timeout while retrieving users data" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [Gateway] Error: {ex.Message}");
            return StatusCode(500, new { message = "An error occurred while retrieving users data" });
        }
    }

    /// <summary>
    /// Delete user from both UserService and AuthService (Admin only)
    /// </summary>
    [HttpDelete("users/{userId}")]
    public async Task<IActionResult> DeleteUser(Guid userId)
    {
        try
        {
            Console.WriteLine($"📥 [Gateway] Admin deleting user: {userId}");

            var response = await _deleteUserSagaClient.GetResponse<DeleteUserSagaResponse>(
                new DeleteUserSagaRequest(userId),
                timeout: RequestTimeout.After(s: 30)
            );

            if (response.Message.Success)
            {
                Console.WriteLine($"✅ [Gateway] Successfully deleted user: {userId}");
                return Ok(new { message = response.Message.Message });
            }
            else
            {
                Console.WriteLine($"❌ [Gateway] Failed to delete user: {response.Message.Message}");
                return BadRequest(new { message = response.Message.Message });
            }
        }
        catch (RequestFaultException faultEx)
        {
            var firstException = faultEx.Fault.Exceptions?.First();
            var message = firstException?.Message ?? "Failed to delete user";

            Console.WriteLine($"❌ [Gateway] Request fault: {message}");

            if (message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { message = "User not found" });
            }

            return StatusCode(500, new { message });
        }
        catch (RequestTimeoutException)
        {
            Console.WriteLine($"⏰ [Gateway] Request timeout for delete user: {userId}");
            return StatusCode(504, new { message = "Request timeout while deleting user" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [Gateway] Error: {ex.Message}");
            return StatusCode(500, new { message = "An error occurred while deleting user" });
        }
    }
}
