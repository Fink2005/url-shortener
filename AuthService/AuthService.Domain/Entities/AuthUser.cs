using AuthService.Domain.Enums;

namespace AuthService.Domain.Entities;

public class AuthUser
{
    public Guid Id { get; private set; }
    public string WalletAddress { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = string.Empty;
    public Role Role { get; private set; } = Role.User;
    public bool IsKycVerified { get; private set; }

    // Lưu refresh token theo cách đơn giản: hash + hạn
    public string? RefreshTokenHash { get; private set; }
    public DateTime? RefreshTokenExpireAt { get; private set; }

    private AuthUser() { }

    public AuthUser(string walletAddress, string email, string passwordHash, Role role = Role.User)
    {
        if (string.IsNullOrWhiteSpace(walletAddress)) throw new ArgumentException("Wallet is required");
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required");
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("PasswordHash is required");

        Id = Guid.NewGuid();
        WalletAddress = walletAddress;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        IsKycVerified = false;
    }

    public void VerifyKyc() => IsKycVerified = true;

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
