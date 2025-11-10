using Microsoft.AspNetCore.Mvc;
using MassTransit;
using Contracts.Url;

namespace ApiGateway.Controllers;

[ApiController]
[Route("url")]
[Tags("Url Service")]
public class UrlGatewayController : ControllerBase
{
    private readonly IRequestClient<CreateShortUrlRequest> _createClient;
    private readonly IRequestClient<ResolveShortUrlRequest> _resolveClient;
    private readonly IRequestClient<DisableShortUrlRequest> _disableClient;
    private readonly IRequestClient<GetListShortUrlsRequest> _listClient;
    private readonly IRequestClient<DeleteShortUrlRequest> _deleteClient;

    public UrlGatewayController(
        IRequestClient<CreateShortUrlRequest> createClient,
        IRequestClient<ResolveShortUrlRequest> resolveClient,
        IRequestClient<DisableShortUrlRequest> disableClient,
        IRequestClient<GetListShortUrlsRequest> listClient,
        IRequestClient<DeleteShortUrlRequest> deleteClient

        )
    {
        _createClient = createClient;
        _resolveClient = resolveClient;
        _listClient = listClient;
        _disableClient = disableClient;
        _deleteClient = deleteClient;
    }

    // --- Tạo link rút gọn ---
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateShortUrlRequest request)
    {
        var response = await _createClient.GetResponse<CreateShortUrlResponse>(request);
        return Ok(response.Message);
    }

    // --- Resolve (mở link gốc) ---
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

    // --- Disable link ---
    [HttpPost("disable")]
    public async Task<IActionResult> Disable([FromBody] DisableShortUrlRequest request)
    {
        var response = await _disableClient.GetResponse<DisableShortUrlResponse>(request);
        return Ok(response.Message);
    }
}
