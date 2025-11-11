using System;

namespace MailService.Domain;

public class MailRequest
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string Subject { get; set; } = null!;
    public string HtmlBody { get; set; } = null!;
    public string? TextBody { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool Sent { get; set; }
    public string? Error { get; set; }
    public DateTime? SentAt { get; set; }
}
