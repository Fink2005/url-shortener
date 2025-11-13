using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MassTransit;
using Contracts.Url;
using Contracts.Users;

namespace ApiGateway.Controllers;

[ApiController]
[Route("url")]
[Tags("Url Service")]
[Authorize(Policy = "Authenticated")]
public class UrlGatewayController : ControllerBase
{
    private readonly IRequestClient<CreateShortUrlRequest> _createClient;
    private readonly IRequestClient<ResolveShortUrlRequest> _resolveClient;
    private readonly IRequestClient<GetListShortUrlsRequest> _listClient;
    private readonly IRequestClient<DeleteShortUrlRequest> _deleteClient;
    private readonly IRequestClient<GetUserByAuthIdRequest> _getUserByAuthIdClient;

    public UrlGatewayController(
        IRequestClient<CreateShortUrlRequest> createClient,
        IRequestClient<ResolveShortUrlRequest> resolveClient,
        IRequestClient<GetListShortUrlsRequest> listClient,
        IRequestClient<DeleteShortUrlRequest> deleteClient,
        IRequestClient<GetUserByAuthIdRequest> getUserByAuthIdClient)
    {
        _createClient = createClient;
        _resolveClient = resolveClient;
        _listClient = listClient;
        _deleteClient = deleteClient;
        _getUserByAuthIdClient = getUserByAuthIdClient;
    }

    // --- Tạo link rút gọn ---
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateShortUrlRequest body)
    {
        // Extract authId from JWT token
        // .NET maps "sub" claim to ClaimTypes.NameIdentifier in long form
        var authIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
                       ?? User.FindFirst("sub")?.Value;
        
        Guid? userId = null;
        if (!string.IsNullOrEmpty(authIdClaim) && Guid.TryParse(authIdClaim, out var authId))
        {
            try
            {
                // Get userId from UserService using authId
                var userResponse = await _getUserByAuthIdClient.GetResponse<GetUserByAuthIdResponse>(
                    new GetUserByAuthIdRequest(authId)
                );
                userId = userResponse.Message.UserId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Failed to get userId from authId {authId}: {ex.Message}");
            }
        }

        // Create request with userId from user_db (not authId!)
        var request = new CreateShortUrlRequest(body.OriginalUrl, userId);
        var response = await _createClient.GetResponse<CreateShortUrlResponse>(request);
        return Ok(response.Message);
    }

    // --- Resolve (mở link gốc) ---
    [AllowAnonymous]  // ✅ Anyone can resolve short URLs (no login required)
    [HttpGet("{code}")]
    public async Task<IActionResult> Resolve([FromRoute] string code)
    {
        var request = new ResolveShortUrlRequest(code);
        var response = await _resolveClient.GetResponse<ResolveShortUrlResponse>(request);
        return Ok(response.Message);
    }

    [HttpGet()]
    public async Task<IActionResult> List()
    {
        var request = new GetListShortUrlsRequest();
        var response = await _listClient.GetResponse<GetListShortUrlsResponse>(request);
        return Ok(response.Message);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var request = new DeleteShortUrlRequest(id);
        var response = await _deleteClient.GetResponse<DeleteShortUrlResponse>(request);
        return Ok(response.Message);
    }
}
