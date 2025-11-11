using System;
using System.Collections.Generic;
namespace Contracts.Url;

// === CREATE SHORT URL ===
public record CreateShortUrlRequest
{
    public string OriginalUrl { get; init; } = string.Empty;
    public Guid? UserId { get; init; } // User who created this URL

    public CreateShortUrlRequest() { }

    public CreateShortUrlRequest(string originalUrl, Guid? userId = null)
    {
        OriginalUrl = originalUrl;
        UserId = userId;
    }
}

public record CreateShortUrlResponse
{
    public Guid Id { get; init; }
    // public string OriginalUrl { get; init; } = string.Empty;
    // public string ShortCode { get; init; } = string.Empty;
    public string ShortUrl { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? ExpireAt { get; init; }

    public CreateShortUrlResponse() { }

    public CreateShortUrlResponse(Guid id, string shortUrl, DateTime createdAt, DateTime? expireAt)
    {
        Id = id;
        ShortUrl = shortUrl;
        CreatedAt = createdAt;
        ExpireAt = expireAt;
    }
}





public record GetListShortUrlsRequest();

public record GetListShortUrlsResponse(List<ShortUrlDto> ShortUrls);

public record ShortUrlDto(Guid Id, string OriginalUrl, string ShortUrl, DateTime CreatedAt, DateTime? ExpireAt);


// ===== RESOLVE SHORT URL =====
public record ResolveShortUrlRequest(string Code);
public record ResolveShortUrlResponse(string OriginalUrl, bool IsActive);

// ===== DISABLE SHORT URL =====
public record DisableShortUrlRequest(string Code);
public record DisableShortUrlResponse(bool Success);


// ===== DELETE SHORT URL =====
public record DeleteShortUrlRequest(Guid Id);
public record DeleteShortUrlResponse(bool Success);


// ===== GET URLS BY USER =====
public record GetUrlsByUserRequest(Guid UserId);
public record GetUrlsByUserResponse(Guid UserId, List<UrlDto> Urls);

// ===== BATCH GET URLS BY MULTIPLE USERS =====
public record GetUrlsByUserIdsRequest(List<Guid> UserIds);
public record GetUrlsByUserIdsResponse(Dictionary<Guid, List<UrlDto>> UrlsByUserId);

public class UrlDto
{
    public Guid Id { get; set; }
    public string ShortCode { get; set; } = string.Empty;
    public string ShortUrl { get; set; } = string.Empty;
    public string OriginalUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpireAt { get; set; }
    public bool IsActive { get; set; }
}