using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Contracts.Mail;

namespace ApiGateway.Controllers;

[ApiController]
[Route("mail")]
[Tags("Mail Service")]
public class MailGatewayController : ControllerBase
{
    private readonly IRequestClient<VerifyEmailRequest> _verifyClient;
    private readonly IRequestClient<CheckEmailTokenRequest> _checkClient;
    private readonly IRequestClient<SendConfirmationEmailRequest> _sendConfirmationClient;

    public MailGatewayController(
        IRequestClient<VerifyEmailRequest> verifyClient,
        IRequestClient<CheckEmailTokenRequest> checkClient,
        IRequestClient<SendConfirmationEmailRequest> sendConfirmationClient)
    {
        _verifyClient = verifyClient;
        _checkClient = checkClient;
        _sendConfirmationClient = sendConfirmationClient;
    }

    [HttpPost("verify")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Token))
        {
            return BadRequest(new { success = false, message = "Email and token are required" });
        }

        try
        {
            var response = await _verifyClient.GetResponse<VerifyEmailResponse>(request);

            if (response.Message.Success)
            {
                return Ok(response.Message);
            }
            else
            {
                return Unauthorized(response.Message);
            }
        }
        catch (RequestTimeoutException)
        {
            return StatusCode(500, new { success = false, message = "Mail service timeout" });
        }
    }
    [HttpPost("send-confirmation")]
    public async Task<IActionResult> SendConfirmationEmail([FromBody] SendConfirmationEmailRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest(new { success = false, message = "Email is required" });
        }

        try
        {
            var response = await _sendConfirmationClient.GetResponse<SendConfirmationEmailResponse>(request);
            return Ok(response.Message);
        }
        catch (RequestTimeoutException)
        {
            return StatusCode(500, new { success = false, message = "Mail service timeout" });
        }
    }

    [HttpGet("check/{email}")]
    public async Task<IActionResult> CheckEmailToken(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest(new { success = false, message = "Email is required" });
        }

        try
        {
            var request = new CheckEmailTokenRequest(email);
            var response = await _checkClient.GetResponse<CheckEmailTokenResponse>(request);

            if (response.Message.Success)
            {
                return Ok(response.Message);
            }
            else
            {
                return NotFound(response.Message);
            }
        }
        catch (RequestTimeoutException)
        {
            return StatusCode(500, new { success = false, message = "Mail service timeout" });
        }
    }
}
