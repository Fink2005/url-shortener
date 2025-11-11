# 🚀 Quick Start - Email Token Verification

## What You Asked

```
"tôi muốn khi đăng ký gởi mail sẽ tạo mã lưu vào redis trong 5 phút quá 5 phút sẽ hết hạn"
```

## ✅ What's Done

- ✅ Redis added to docker-compose
- ✅ Token service created (ITokenService + RedisTokenService)
- ✅ SendMailConsumer saves token to Redis (5 min TTL)
- ✅ Verification endpoint created
- ✅ Email template updated with 5-minute expiry message

## 🎯 Complete Flow

```
1. User Register
   ↓
2. Email sent with token (saved to Redis for 5 min)
   ↓
3. User verifies token: POST /api/verification/verify
   ↓
4. ✅ Success (token deleted) or ❌ Fail (invalid/expired)
```

## 🧪 Test It (3 Steps)

### Step 1: Start Services

```bash
docker-compose up -d
sleep 10
```

### Step 2: Register User

```bash
curl -X POST http://localhost:5050/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "email": "test@example.com",
    "password": "Pass123!"
  }'
```

**⏰ Email arrives with token**

### Step 3: Verify Token

```bash
# Get token (for testing)
TOKEN=$(curl -s http://localhost:5004/api/verification/check/test@example.com | jq -r '.token')

# Verify it
curl -X POST http://localhost:5004/api/verification/verify \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"test@example.com\",\"token\":\"$TOKEN\"}"
```

**✅ Response: {"success": true}**

## 🔑 Key Endpoints

| Endpoint                          | Method | Purpose                              |
| --------------------------------- | ------ | ------------------------------------ |
| `/api/verification/verify`        | POST   | Verify token (deletes after success) |
| `/api/verification/check/{email}` | GET    | Check if token exists (debug)        |

## 📊 Redis Info

```
Host: redis:6379
Key: email-token:<email>
Value: <token>
TTL: 5 minutes (auto-expires)
Use: One-time only (deleted after verify)
```

## 📝 What Changed

### SendMailConsumer (MailService)

```csharp
// Now saves token to Redis
await _tokenService.SaveTokenAsync(message.Email, message.ConfirmationToken, 5);
```

### VerificationController (NEW)

```csharp
// Endpoint to verify token
POST /api/verification/verify
{
  "email": "user@example.com",
  "token": "550e8400-e29b-41d4-a716-446655440000"
}
```

## 💾 Config

### docker-compose.yml (Added)

```yaml
redis:
  image: redis:7-alpine
  ports:
    - "6379:6379"
  volumes:
    - redis_data:/data
```

### appsettings.json (Added)

```json
{
  "Redis": {
    "Connection": "redis:6379"
  }
}
```

## 🐛 Debug Commands

```bash
# Check if Redis is running
docker-compose ps redis

# Check all tokens in Redis
docker exec -it redis redis-cli KEYS "*"

# Get specific token
docker exec -it redis redis-cli GET email-token:test@example.com

# Check TTL (seconds remaining)
docker exec -it redis redis-cli TTL email-token:test@example.com

# View logs
docker-compose logs mailservice | grep -i token
docker-compose logs mailservice | grep -i redis
```

## ✨ Token Lifetime

```
t=0min    : Token created, saved to Redis
t=1-4min  : Token valid
t=5min    : Token auto-expires in Redis
t>5min    : Verification fails "Invalid or expired token"

⚡ After successful verify: Token immediately deleted (one-time use)
```

## 📦 What's New

| File                        | Purpose                        |
| --------------------------- | ------------------------------ |
| `ITokenService.cs`          | Interface for token management |
| `RedisTokenService.cs`      | Redis implementation           |
| `VerificationController.cs` | Verify token endpoint          |
| `docker-compose.yml`        | Added Redis service            |

## 🔒 Security

- ✅ 5-minute auto-expiry
- ✅ One-time use (deleted after verify)
- ✅ In-memory Redis (fast & secure)
- ✅ UUID tokens (cryptographically secure)

## 🚨 Common Issues

| Issue                     | Fix                                          |
| ------------------------- | -------------------------------------------- |
| Redis connection refused  | Check `docker-compose ps redis` is running   |
| Token not in email        | Check MailService logs for errors            |
| Verification always fails | Verify correct token format, check Redis TTL |
| Token lasts > 5 min       | Something refreshed TTL manually             |

## 📚 Full Documentation

See detailed guides:

- `REDIS_EMAIL_VERIFICATION.md` - Complete guide
- `REDIS_IMPLEMENTATION_COMPLETE.md` - Full implementation summary

---

**Ready to test!** 🎉

```bash
docker-compose up -d && sleep 10 && echo "Go register a user!"
```
