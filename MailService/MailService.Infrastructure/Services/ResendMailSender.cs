using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MailService.Application.Abstractions;
using MailService.Domain.Entities;

namespace MailService.Infrastructure.Services;

public class ResendMailSender : IMailSender
{
    private readonly string _apiKey;
    private readonly HttpClient _httpClient;
    private readonly string _fromEmail;

    public ResendMailSender(string apiKey, string fromEmail = "noreply@example.com")
    {
        _apiKey = apiKey;
        _fromEmail = fromEmail;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }

    public async Task SendMailAsync(MailRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.To) || string.IsNullOrWhiteSpace(request.Subject))
        {
            throw new ArgumentException("Email and Subject are required.");
        }

        var payload = new
        {
            from = _fromEmail,
            to = request.To,
            subject = request.Subject,
            html = request.Body
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync("https://api.resend.com/emails", payload);
            response.EnsureSuccessStatusCode();
            Console.WriteLine($"✓ Email sent to {request.To}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Failed to send email to {request.To}: {ex.Message}");
            throw;
        }
    }
}
