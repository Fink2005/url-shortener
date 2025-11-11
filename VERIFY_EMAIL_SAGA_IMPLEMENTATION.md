# Verify Email Saga Implementation

## Overview

Đã triển khai flow verify email qua Saga pattern. Khi người dùng verify email thành công bằng email và token, hệ thống sẽ cập nhật `IsEmailVerified = true` trong AuthService.

## Flow

```
User → Gateway → MailService (verify token) → AuthService (update IsEmailVerified)
```

### Chi tiết flow:

1. **User gọi API**: `POST /saga/verify-email` với body `{email, token}`
2. **Gateway** (`VerifyEmailGatewayController`) gửi `VerifyEmailRequestedEvent`
3. **MailService** (`VerifyMailSagaConsumer`) nhận event:
   - Verify token từ Redis
   - Nếu token hợp lệ → Publish `EmailVerifiedEvent`
   - Nếu token không hợp lệ → Publish `EmailVerificationFailedEvent`
4. **AuthService** (`VerifyAuthMailSagaConsumer`) nhận `EmailVerifiedEvent`:
   - Gọi `VerifyEmailAuthHandler` để update `IsEmailVerified = true`
5. **Response** về Gateway

## Files Created/Modified

### 1. Contracts (Libs/Contracts/SagaContract.cs)

```csharp
public record VerifyEmailRequestedEvent(string Email, string Token);
public record EmailVerifiedEvent(Guid CorrelationId, string Email);
public record EmailVerificationFailedEvent(Guid CorrelationId, string Email, string Reason);
```

### 2. AuthUser Entity (AuthService.Domain/Entities/AuthUser.cs)

Added method:

```csharp
public void VerifyEmail()
{
    if (IsEmailVerified)
        throw new InvalidOperationException("Email is already verified");

    IsEmailVerified = true;
}
```

### 3. Repository Interface (AuthService.Domain/Repositories/IAuthUserRepository.cs)

Added methods:

```csharp
Task UpdateAsync(AuthUser user);
Task<AuthUser?> GetByEmailAsync(string email);
```

### 4. Repository Implementation (AuthService.Infrastructure/Repositories/AuthUserRepository.cs)

Implemented:

```csharp
public async Task UpdateAsync(AuthUser user)
{
    _db.AuthUsers.Update(user);
    await _db.SaveChangesAsync();
}

public Task<AuthUser?> GetByEmailAsync(string email)
    => _db.AuthUsers.FirstOrDefaultAsync(x => x.Email == email);
```

### 5. Handler (AuthService.Application/Commands/VerifyEmailAuthHandler.cs)

New handler để xử lý logic verify email:

- Tìm user theo email
- Check nếu đã verified → throw exception
- Gọi `user.VerifyEmail()` để update
- Save changes

### 6. MailService Consumer (MailService.Api/Consumers/VerifyMailSagaConsumer.cs)

New Saga consumer trong MailService:

- Nhận `VerifyEmailRequestedEvent`
- Verify token từ Redis
- Nếu valid → Publish `EmailVerifiedEvent`
- Nếu invalid → Publish `EmailVerificationFailedEvent`

### 7. AuthService Consumer (AuthService.Api/Consumers/AuthSagaConsumers/VerifyAuthMailSagaConsumer.cs)

Saga consumer trong AuthService:

- Nhận `EmailVerifiedEvent` từ MailService
- Gọi `VerifyEmailAuthHandler` để update `IsEmailVerified = true`

### 8. Gateway Controller (ApiGateway/Controllers/sagaGatewayController/VerifyEmailGatewayController.cs)

New endpoint: `POST /saga/verify-email`

### 9. Program.cs Updates

- **AuthService.Api/Program.cs**: Đăng ký `VerifyEmailAuthHandler` và `VerifyAuthMailSagaConsumer`
- **MailService.Api/Program.cs**: Đăng ký `VerifyMailSagaConsumer`

## API Endpoint

### Verify Email

```http
POST /saga/verify-email
Content-Type: application/json

{
  "email": "user@example.com",
  "token": "verification-token-here"
}
```

### Success Response

```json
{
  "success": true,
  "message": "Email verified successfully"
}
```

### Error Response

```json
{
  "success": false,
  "message": "Invalid or expired token"
}
```

## Error Handling

1. **User not found**: Throw `InvalidOperationException`
2. **Email already verified**: Throw `InvalidOperationException`
3. **Invalid token**: Publish `EmailVerificationFailedEvent`
4. **System error**: Catch and publish `EmailVerificationFailedEvent`

## Events Published

### Success

```csharp
EmailVerifiedEvent(Guid.NewGuid(), email)
```

### Failure

```csharp
EmailVerificationFailedEvent(Guid.NewGuid(), email, reason)
```

## Testing

1. Đăng ký user mới (nhận email verification)
2. Copy token từ email
3. Gọi API:

```bash
curl -X POST http://localhost:5000/saga/verify-email \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","token":"your-token-here"}'
```

4. Check database: `IsEmailVerified` phải là `true`

## Notes

- Migration cho `IsEmailVerified` đã tồn tại: `20251111044155_AddIsEmailVerified`
- Consumer lắng nghe trên endpoint `auth-service`
- Token được verify bởi MailService sử dụng Redis
- Flow này hoàn toàn độc lập với MailService verify endpoint thông thường

## Future Improvements

1. Thêm SagaService consumer để log audit trail
2. Thêm retry logic cho failed verifications
3. Thêm expiry time cho verification tokens
4. Gửi notification sau khi verify thành công
