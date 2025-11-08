using AuthService.Domain.Enums;

namespace AuthService.Domain.Entities;

public class AuthUser
{
    public Guid Id { get; private set; }
    public string Username { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = string.Empty;
    public Role Role { get; private set; } = Role.User;

    // Refresh token (lưu HASH + hạn)
    public string? RefreshTokenHash { get; private set; }
    public DateTime? RefreshTokenExpireAt { get; private set; }

    private AuthUser() { }

    public AuthUser(string username, string email, string passwordHash, Role role = Role.User)
    {
        if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("Username is required");
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required");
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("PasswordHash is required");

        Id = Guid.NewGuid();
        Username = username;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
    }


    public void SetRefreshToken(string refreshTokenHash, DateTime expireAt)
    {
        RefreshTokenHash = refreshTokenHash;
        RefreshTokenExpireAt = expireAt;
    }

    public void RevokeRefreshToken()
    {
        RefreshTokenHash = null;
        RefreshTokenExpireAt = null;
    }

    public void PromoteToAdmin() => Role = Role.Admin;
}
