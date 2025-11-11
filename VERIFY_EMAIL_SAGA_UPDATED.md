# Verify Email Saga Implementation - UPDATED

## 🎯 Overview

Flow verify email qua Saga pattern với kiến trúc microservices:

- **MailService**: Chịu trách nhiệm verify token
- **AuthService**: Chịu trách nhiệm cập nhật `IsEmailVerified = true`
- **Gateway**: Điều phối request từ client

## 🔄 Flow Diagram

```
User (Client)
   ↓
   POST /saga/verify-email {email, token}
   ↓
Gateway (VerifyEmailGatewayController)
   ↓
   Publish: VerifyEmailRequestedEvent
   ↓
MailService (VerifyMailSagaConsumer)
   ├─→ Verify token in Redis
   ├─→ ✅ Valid Token
   │     └─→ Publish: EmailVerifiedEvent
   │           ↓
   │         AuthService (VerifyAuthMailSagaConsumer)
   │           └─→ Update IsEmailVerified = true
   │
   └─→ ❌ Invalid Token
         └─→ Publish: EmailVerificationFailedEvent
```

## 📝 Chi tiết từng bước

### 1. User gọi API

```http
POST http://localhost:5000/saga/verify-email
Content-Type: application/json

{
  "email": "user@example.com",
  "token": "verification-token-from-email"
}
```

### 2. Gateway xử lý

- Controller: `VerifyEmailGatewayController`
- Publish event: `VerifyEmailRequestedEvent(email, token)`
- Chờ response từ MailService

### 3. MailService verify token

- Consumer: `VerifyMailSagaConsumer`
- Verify token từ Redis
- **Nếu valid**:
  - Publish: `EmailVerifiedEvent(correlationId, email)`
  - Log: ✅ Token verified successfully
- **Nếu invalid**:
  - Publish: `EmailVerificationFailedEvent(correlationId, email, reason)`
  - Log: ❌ Invalid or expired token

### 4. AuthService update IsEmailVerified

- Consumer: `VerifyAuthMailSagaConsumer`
- Lắng nghe: `EmailVerifiedEvent`
- Handler: `VerifyEmailAuthHandler`
- Update: `user.VerifyEmail()` → `IsEmailVerified = true`
- Save to database

### 5. Response về Gateway

```json
{
  "success": true,
  "message": "Email verified successfully"
}
```

## 📁 Files Structure

### Contracts (Libs/Contracts/)

```
SagaContract.cs
├── VerifyEmailRequestedEvent(Email, Token)
├── EmailVerifiedEvent(CorrelationId, Email)
└── EmailVerificationFailedEvent(CorrelationId, Email, Reason)
```

### MailService

```
MailService.Api/
├── Consumers/
│   └── VerifyMailSagaConsumer.cs       ← NEW
└── Program.cs                           ← UPDATED
```

**VerifyMailSagaConsumer.cs**:

- Consumes: `VerifyEmailRequestedEvent`
- Verifies token with Redis
- Publishes: `EmailVerifiedEvent` or `EmailVerificationFailedEvent`

### AuthService

```
AuthService.Api/
├── Consumers/AuthSagaConsumers/
│   └── VerifyAuthMailSagaConsumer.cs   ← UPDATED
└── Program.cs                           ← UPDATED

AuthService.Application/
└── Commands/
    └── VerifyEmailAuthHandler.cs       ← NEW

AuthService.Domain/
├── Entities/
│   └── AuthUser.cs                     ← UPDATED (added VerifyEmail())
└── Repositories/
    └── IAuthUserRepository.cs          ← UPDATED (added UpdateAsync, GetByEmailAsync)

AuthService.Infrastructure/
└── Repositories/
    └── AuthUserRepository.cs           ← UPDATED
```

**VerifyAuthMailSagaConsumer.cs**:

- Consumes: `EmailVerifiedEvent`
- Calls: `VerifyEmailAuthHandler`
- Updates: `IsEmailVerified = true`

### ApiGateway

```
ApiGateway/
└── Controllers/sagaGatewayController/
    └── VerifyEmailGatewayController.cs  ← NEW
```

## 🔧 Code Highlights

### 1. MailService - VerifyMailSagaConsumer

```csharp
public async Task Consume(ConsumeContext<VerifyEmailRequestedEvent> context)
{
    var isValid = await _tokenService.VerifyTokenAsync(email, token);

    if (isValid)
    {
        await _publishEndpoint.Publish(new EmailVerifiedEvent(
            Guid.NewGuid(), email
        ));
    }
    else
    {
        await _publishEndpoint.Publish(new EmailVerificationFailedEvent(
            Guid.NewGuid(), email, "Invalid or expired token"
        ));
    }
}
```

### 2. AuthService - VerifyAuthMailSagaConsumer

```csharp
public async Task Consume(ConsumeContext<EmailVerifiedEvent> context)
{
    var authRequest = new VerifyEmailAuthRequest(context.Message.Email, string.Empty);
    var authResponse = await _authHandler.Handle(authRequest);
    // IsEmailVerified updated to true
}
```

### 3. AuthUser Entity - VerifyEmail Method

```csharp
public void VerifyEmail()
{
    if (IsEmailVerified)
        throw new InvalidOperationException("Email is already verified");

    IsEmailVerified = true;
}
```

### 4. Gateway Controller

```csharp
[HttpPost]
public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequestDto request)
{
    var response = await _requestClient.GetResponse<VerifyEmailRequestedEvent>(
        new VerifyEmailRequestedEvent(request.Email, request.Token)
    );

    return Ok(new { success = true, message = "Email verified successfully" });
}
```

## 🧪 Testing Steps

### 1. Register new user

```bash
POST http://localhost:5000/saga/register
{
  "username": "testuser",
  "email": "test@example.com",
  "password": "Test@123"
}
```

### 2. Check email for verification token

Email sẽ chứa link dạng:

```
http://your-frontend.com/verify?email=test@example.com&token=abc123xyz
```

### 3. Verify email with token

```bash
POST http://localhost:5000/saga/verify-email
{
  "email": "test@example.com",
  "token": "abc123xyz"
}
```

### 4. Check database

```sql
SELECT Email, IsEmailVerified FROM AuthUsers WHERE Email = 'test@example.com';
```

Expected result:

```
Email                 | IsEmailVerified
----------------------|----------------
test@example.com      | true
```

### 5. Check logs

**MailService log:**

```
📬 [MailService] Received VerifyEmailRequestedEvent for test@example.com
🔍 [MailService] Verifying token for test@example.com...
✅ [MailService] Token verified successfully for test@example.com
📨 [MailService] Published EmailVerifiedEvent to AuthService
```

**AuthService log:**

```
📬 [AuthService] Received EmailVerifiedEvent for test@example.com
🔍 [AuthService] Updating IsEmailVerified for test@example.com...
✅ [AuthService] IsEmailVerified updated successfully for test@example.com
```

## ⚠️ Error Scenarios

### 1. Invalid Token

```
Request → MailService → EmailVerificationFailedEvent
Response: {"success": false, "message": "Invalid or expired token"}
```

### 2. User Not Found

```
EmailVerifiedEvent → AuthService → InvalidOperationException
Log: ❌ User not found
```

### 3. Email Already Verified

```
EmailVerifiedEvent → AuthService → InvalidOperationException
Log: ❌ Email is already verified
```

## 📊 Comparison: Old vs New

### Old (Incorrect)

```
Gateway → AuthService → MailService → AuthService
         └─ Verify + Update in same consumer
         └─ Tight coupling
```

### New (Correct)

```
Gateway → MailService → AuthService
         └─ Verify      └─ Update
         └─ Separation of concerns
         └─ Loosely coupled
```

## ✅ Benefits

1. **Separation of Concerns**:

   - MailService: Email & token management
   - AuthService: User authentication data

2. **Event-Driven**:

   - Loosely coupled services
   - Easy to add new consumers (audit, notification, etc.)

3. **Scalability**:

   - Each service can scale independently

4. **Maintainability**:
   - Clear responsibilities
   - Easy to debug and test

## 🚀 Future Enhancements

1. **Add SagaService orchestration**

   - Track entire verification saga
   - Handle compensation on failures

2. **Add notification service**

   - Send "Email verified" notification
   - SMS/Push notification support

3. **Add retry mechanism**

   - Auto-resend verification email
   - Exponential backoff

4. **Add analytics**
   - Track verification rates
   - Monitor token expiry

## 📚 Related Documentation

- [SAGA_SETUP.md](./SAGA_SETUP.md) - Saga pattern setup
- [REDIS_EMAIL_VERIFICATION.md](./REDIS_EMAIL_VERIFICATION.md) - Redis token management
- [STARTUP_GUIDE.md](./STARTUP_GUIDE.md) - How to run the system
