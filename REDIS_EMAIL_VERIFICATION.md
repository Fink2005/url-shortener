# Email Token Verification with Redis - Complete Guide

## Overview

Email confirmation tokens now stored in **Redis** with **5-minute expiry**. Tokens are one-time use and automatically deleted after verification.

## Architecture

```
┌──────────────────────────────────────────────────────────────┐
│                 Registration Flow                             │
└──────────────────────────────────────────────────────────────┘

1. POST /auth/register
   ↓
2. AuthService.RegisterAuthHandler
   ├─ Create AuthUser
   └─ Publish RegisterAuthRequest
   ↓
3. SagaService receives event
   ├─ Trigger state machine
   └─ Send SendConfirmationEmailCommand
   ↓
4. MailService.SendMailConsumer
   ├─ Generate ConfirmationToken (UUID or random)
   ├─ Save to Redis: email-token:<email> → <token> (5 min TTL)
   ├─ Send email with token
   └─ Publish EmailConfirmationSent
   ↓
5. ✅ Email arrives with token

┌──────────────────────────────────────────────────────────────┐
│             Email Verification Flow                           │
└──────────────────────────────────────────────────────────────┘

1. User clicks email or enters token
   ↓
2. POST /api/verification/verify
   {
     "email": "user@example.com",
     "token": "abc123xyz"
   }
   ↓
3. MailService.VerificationController
   ├─ Retrieve token from Redis
   ├─ Compare with provided token
   ├─ If match: DELETE token (one-time use)
   └─ Return success/failure
   ↓
4. AuthService updates user status to "EmailVerified"
```

## Redis Setup

### Service Started
```yaml
redis:
  image: redis:7-alpine
  container_name: redis
  ports:
    - "6379:6379"
  volumes:
    - redis_data:/data
  healthcheck:
    test: ["CMD", "redis-cli", "ping"]
```

### Connection String
```
redis:6379  (Docker network)
localhost:6379  (Local development)
```

## Token Storage Format

**Key:** `email-token:<email>`
**Value:** `<confirmation_token>`
**TTL:** 5 minutes (300 seconds)

### Example
```
Key: email-token:user@example.com
Value: 550e8400-e29b-41d4-a716-446655440000
Expiry: 5 minutes
```

## API Endpoints

### 1. Verify Email Token
```
POST /api/verification/verify
Content-Type: application/json

{
  "email": "user@example.com",
  "token": "550e8400-e29b-41d4-a716-446655440000"
}
```

**Successful Response (200):**
```json
{
  "success": true,
  "message": "Email verified successfully"
}
```

**Failed Response (401):**
```json
{
  "success": false,
  "message": "Invalid or expired token"
}
```

### 2. Check Token Exists (Debug Only)
```
GET /api/verification/check/{email}
```

**Example:**
```
GET /api/verification/check/user@example.com
```

**Response (if exists):**
```json
{
  "success": true,
  "message": "Token exists",
  "token": "550e8400-e29b-41d4-a716-446655440000"
}
```

**Response (if not exists):**
```json
{
  "success": false,
  "message": "No active token for this email"
}
```

## Email Template

Token is displayed in the email:

```html
<div class='token-box'>
  <div class='token-code'>550e8400-e29b-41d4-a716-446655440000</div>
</div>
<p class='expiry'>⏰ Mã xác nhận này sẽ hết hạn trong <strong>5 phút</strong>.</p>
```

## Code Changes

### 1. SendMailConsumer Updates
```csharp
public class SendMailConsumer : IConsumer<SendConfirmationEmailCommand>
{
    private readonly IMailSender _mailSender;
    private readonly ITokenService _tokenService;  // ← NEW

    public SendMailConsumer(IMailSender mailSender, ITokenService tokenService)
    {
        _mailSender = mailSender;
        _tokenService = tokenService;
    }

    public async Task Consume(ConsumeContext<SendConfirmationEmailCommand> context)
    {
        var message = context.Message;

        // Save token to Redis (5 min expiry)
        await _tokenService.SaveTokenAsync(message.Email, message.ConfirmationToken, 5);

        // Send email
        await _mailSender.SendMailAsync(mailRequest);
    }
}
```

### 2. ITokenService Interface
```csharp
public interface ITokenService
{
    Task<bool> SaveTokenAsync(string email, string token, int expiryMinutes = 5);
    Task<bool> VerifyTokenAsync(string email, string token);
    Task<string?> GetTokenAsync(string email);
    Task<bool> DeleteTokenAsync(string email);
}
```

### 3. Redis Registration (Program.cs)
```csharp
// Redis connection
var redisConnection = builder.Configuration["Redis:Connection"] ?? "redis:6379";
var redis = ConnectionMultiplexer.Connect(redisConnection);
builder.Services.AddSingleton<IConnectionMultiplexer>(redis);
builder.Services.AddSingleton<ITokenService, RedisTokenService>();
```

## Configuration

### appsettings.json
```json
{
  "Redis": {
    "Connection": "redis:6379"
  },
  "Resend": {
    "ApiKey": "re_...",
    "FromEmail": "..."
  }
}
```

### docker-compose.yml (MailService)
```yaml
mailservice:
  environment:
    - Redis__Connection=redis:6379
    - Resend__ApiKey=re_...
  depends_on:
    redis:
      condition: service_healthy
```

## Testing

### Full Flow Test

**1. Register User:**
```bash
curl -X POST http://localhost:5050/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "email": "test@example.com",
    "password": "SecurePass123!"
  }'
```

**Expected:** Email sent with token (check inbox)

**2. Extract Token from Email or Redis:**
```bash
# Check if token exists
curl http://localhost:5004/api/verification/check/test@example.com

# Response:
# {
#   "success": true,
#   "token": "550e8400-e29b-41d4-a716-446655440000"
# }
```

**3. Verify Token (First Time - Success):**
```bash
curl -X POST http://localhost:5004/api/verification/verify \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "token": "550e8400-e29b-41d4-a716-446655440000"
  }'

# Response: 200 OK
# {
#   "success": true,
#   "message": "Email verified successfully"
# }
```

**4. Verify Again (Should Fail - Token Deleted):**
```bash
curl -X POST http://localhost:5004/api/verification/verify \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "token": "550e8400-e29b-41d4-a716-446655440000"
  }'

# Response: 401 Unauthorized
# {
#   "success": false,
#   "message": "Invalid or expired token"
# }
```

### Redis CLI Debugging

```bash
# Connect to Redis
docker exec -it redis redis-cli

# List all keys
KEYS *
# Output: (integer) 1, email-token:test@example.com

# Get token
GET email-token:test@example.com
# Output: "550e8400-e29b-41d4-a716-446655440000"

# Check TTL (seconds remaining)
TTL email-token:test@example.com
# Output: (integer) 245  (5 min = 300 sec)

# Delete manually
DEL email-token:test@example.com
# Output: (integer) 1
```

## Troubleshooting

| Issue | Cause | Solution |
|-------|-------|----------|
| Redis connection refused | Redis not running or wrong host | Check `docker-compose ps redis`, verify `Redis__Connection` |
| Token not saved | ITokenService not registered | Check `Program.cs` has `AddSingleton<ITokenService, RedisTokenService>()` |
| Token disappears before 5 min | TTL issue or expired | Check Redis `TTL email-token:<email>`, verify no manual delete |
| Verification endpoint 404 | VerificationController not mapped | Ensure `app.MapControllers()` in Program.cs |
| Token not in email | Saga not publishing command | Check SagaService logs for SendConfirmationEmailCommand |

## Logs to Watch

### MailService Logs
```
[Redis] Token saved for user@example.com, expires in 5 minutes
✓ Confirmation email sent to user@example.com (token expires in 5 minutes)
[Redis] Token verified and deleted for user@example.com
```

### Redis Logs
```
* Ready to accept connections
```

## What's Next?

1. ✅ Token saved to Redis with 5-min expiry
2. ✅ Email sent with token
3. ✅ Verification endpoint created
4. ⏳ **AuthService endpoint** to mark email as verified
5. ⏳ **Auto-verify** email when user clicks link in email
6. ⏳ **Resend token** if expired

## Security Notes

- ✅ Tokens are one-time use (deleted after verification)
- ✅ 5-minute expiry prevents brute force
- ✅ Redis in-memory (fast)
- ⚠️ TODO: Rate limiting on verification attempts
- ⚠️ TODO: Add HTTPS-only cookie for token
- ⚠️ TODO: Log verification attempts for audit trail

---
Created: 2025-11-11
Updated: 2025-11-11
