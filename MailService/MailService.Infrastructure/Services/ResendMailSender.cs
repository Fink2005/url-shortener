using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using MailService.Application.Abstractions;
using MailService.Domain.Entities;

namespace MailService.Infrastructure.Services;

public class ResendMailSender : IMailSender
{
    private readonly HttpClient _httpClient;
    private readonly string _fromEmail;

    public ResendMailSender(HttpClient httpClient, string fromEmail)
    {
        _httpClient = httpClient;
        _fromEmail = fromEmail;
    }

    public async Task SendMailAsync(MailRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.To))
            throw new ArgumentException("Recipient email is required.");
        if (string.IsNullOrWhiteSpace(request.Subject))
            throw new ArgumentException("Subject is required.");

        var payload = new
        {
            from = _fromEmail,
            to = request.To,
            subject = request.Subject,
            html = request.Body
        };

        var response = await _httpClient.PostAsJsonAsync("https://api.resend.com/emails", payload);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[✗] Failed to send email → {request.To}");
            Console.WriteLine($"Status: {response.StatusCode}");
            Console.WriteLine($"Response: {error}");
            Console.ResetColor();
            throw new HttpRequestException($"Resend API error: {response.StatusCode}");
        }

        var successJson = await response.Content.ReadFromJsonAsync<JsonElement>();
        var id = successJson.GetProperty("id").GetString();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[✓] Email sent successfully → {request.To} (ID: {id})");
        Console.ResetColor();
    }
}
