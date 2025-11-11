using System;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace MailService.Application.Abstractions;

/// <summary>
/// Service untuk menyimpan dan memverifikasi email confirmation tokens di Redis
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Simpan token ke Redis dengan expiry 5 menit
    /// </summary>
    Task<bool> SaveTokenAsync(string email, string token, int expiryMinutes = 5);

    /// <summary>
    /// Verifikasi token - ambil dari Redis dan hapus (one-time use)
    /// </summary>
    Task<bool> VerifyTokenAsync(string email, string token);

    /// <summary>
    /// Ambil token tanpa hapus (untuk debugging)
    /// </summary>
    Task<string?> GetTokenAsync(string email);

    /// <summary>
    /// Hapus token
    /// </summary>
    Task<bool> DeleteTokenAsync(string email);
}

public class RedisTokenService : ITokenService
{
    private readonly IConnectionMultiplexer _redis;
    private const string TokenKeyPrefix = "email-token:";

    public RedisTokenService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task<bool> SaveTokenAsync(string email, string token, int expiryMinutes = 5)
    {
        try
        {
            var db = _redis.GetDatabase();
            var key = $"{TokenKeyPrefix}{email}";

            // Lưu token với TTL
            var result = await db.StringSetAsync(
                key,
                token,
                expiry: TimeSpan.FromMinutes(expiryMinutes)
            );

            if (result)
            {
                Console.WriteLine($"[Redis] Token saved for {email}, expires in {expiryMinutes} minutes");
            }

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Redis] Error saving token: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> VerifyTokenAsync(string email, string providedToken)
    {
        try
        {
            var db = _redis.GetDatabase();
            var key = $"{TokenKeyPrefix}{email}";

            // Ambil token từ Redis
            var storedToken = await db.StringGetAsync(key);

            if (!storedToken.HasValue)
            {
                Console.WriteLine($"[Redis] Token not found for {email}");
                return false;
            }

            // Kiểm tra token cocok
            bool isValid = storedToken.ToString() == providedToken;

            if (isValid)
            {
                // Token valid - hapus (one-time use)
                await db.KeyDeleteAsync(key);
                Console.WriteLine($"[Redis] Token verified and deleted for {email}");
            }
            else
            {
                Console.WriteLine($"[Redis] Token mismatch for {email}");
            }

            return isValid;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Redis] Error verifying token: {ex.Message}");
            return false;
        }
    }

    public async Task<string?> GetTokenAsync(string email)
    {
        try
        {
            var db = _redis.GetDatabase();
            var key = $"{TokenKeyPrefix}{email}";

            var token = await db.StringGetAsync(key);
            return token.HasValue ? token.ToString() : null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Redis] Error getting token: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> DeleteTokenAsync(string email)
    {
        try
        {
            var db = _redis.GetDatabase();
            var key = $"{TokenKeyPrefix}{email}";

            var result = await db.KeyDeleteAsync(key);
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Redis] Error deleting token: {ex.Message}");
            return false;
        }
    }
}
