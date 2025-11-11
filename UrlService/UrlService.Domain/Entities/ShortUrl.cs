namespace UrlService.Domain.Entities;

public class ShortUrl
{
    public Guid Id { get; private set; }
    public string OriginalUrl { get; private set; } = string.Empty;
    public string ShortCode { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime? ExpireAt { get; private set; }
    public bool IsActive { get; private set; }
    public Guid? UserId { get; private set; } // User who created this URL

    // ✅ Constructor chính: nhận OriginalUrl và ShortCode
    public ShortUrl(string originalUrl, string shortCode, Guid? userId = null)
    {
        Id = Guid.NewGuid();
        OriginalUrl = originalUrl;
        ShortCode = string.IsNullOrWhiteSpace(shortCode)
            ? GenerateShortCode()
            : shortCode;
        CreatedAt = DateTime.UtcNow;
        ExpireAt = DateTime.UtcNow.AddDays(7); // Ví dụ: mặc định 7 ngày
        IsActive = true;
        UserId = userId;
    }

    // ✅ Dành cho EF Core
    private ShortUrl() { }

    // Tạo short code ngẫu nhiên nếu không truyền
    private string GenerateShortCode()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("=", "")
            .Replace("+", "")
            .Replace("/", "")
            .Substring(0, 8);
    }

    public void Disable() => IsActive = false;

    public bool IsExpired() => ExpireAt.HasValue && ExpireAt.Value < DateTime.UtcNow;
}
