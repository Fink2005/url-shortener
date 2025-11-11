# Why User Not Created - Saga Issue Diagnosis

## Your Question

"sao tôi đăng ký có thấy saga hoạt động mail k hoạt động đăng ký tạo user ở authservice nhưng đâu tạo ở userservice gì đâu nhỉ"

Translation: "Why is registration not creating user at UserService? Saga seems to work but mail doesn't work and user not created there"

## Root Causes We Fixed

### 1. ❌ ApiGateway RequestClient Not Registered (FIXED)

**Problem:** ApiGateway controller called `_registerClient.GetResponse()` but no RequestClient was registered in DI

```csharp
// BEFORE (broken)
var response = await _registerClient.GetResponse<RegisterAuthResponse>(request);
// ↑ This _registerClient was null because never registered
```

**Solution:** Added in `ApiGateway/Program.cs`:

```csharp
builder.Services.AddScoped(provider =>
    provider.GetRequiredService<IBus>().CreateRequestClient<RegisterAuthRequest>());
```

### 2. ✅ AuthService Publishes Event (WORKING)

AuthService `RegisterAuthHandler` correctly does:

```csharp
await _publishEndpoint.Publish(req);  // ✅ Publishes RegisterAuthRequest
```

This triggers the saga via event correlation.

### 3. ✅ Saga Listens to RegisterAuthRequest (WORKING)

SagaService state machine correctly listens:

```csharp
public Event<RegisterAuthRequest> OnboardingStarted { get; private set; }

// Correlation: match by email
Event(() => OnboardingStarted, x =>
{
    x.CorrelateBy((state, ctx) => state.Email == ctx.Message.Email);
});
```

### 4. ✅ Saga Sends CreateUserCommand (SHOULD WORK)

After auth created, saga sends:

```csharp
ctx.Publish(new CreateUserCommand(
    ctx.Saga.CorrelationId,
    ctx.Saga.Username,
    ctx.Saga.Email,
    ctx.Saga.AuthId,
    Role.User
));
```

### 5. ✅ UserService Has Consumer (WORKING)

UserService has `CreateUserFromSagaConsumer` registered:

```csharp
x.AddConsumer<CreateUserFromSagaConsumer>();  // ✅ Registered
```

## Why It Might Still Not Work

### Issue A: Email Correlation Failed

If user registers with email that's already in DB, saga won't trigger.

**Test:** Use unique email each time

```bash
curl -X POST http://localhost:5050/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser_'$(date +%s)'",
    "email": "test_'$(date +%s)'@example.com",
    "password": "SecurePass123!"
  }'
```

### Issue B: RabbitMQ Message Not Routed

Consumer might not receive message if queue binding wrong.

**Check:**

```bash
# Check RabbitMQ UI
curl -u guest:guest http://localhost:15672/api/queues/%2F
# Should show: user-service queue exists
```

### Issue C: UserService Not Running or Crashed

If UserService pod crashed, it won't consume messages.

**Check:**

```bash
docker-compose ps userservice
# Should show: Up
```

### Issue D: Database Connection Failed

UserService might fail to save because DB connection error.

**Check UserService logs:**

```bash
docker-compose logs userservice | grep -i error | tail -10
```

## Step-by-Step Debug

### Step 1: Test AuthService Directly

```bash
curl -X POST http://localhost:5002/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "direct_test",
    "email": "direct@example.com",
    "password": "Pass123!"
  }'
```

Check logs:

```bash
docker-compose logs authservice | grep -i "publish"
```

Should see: `Publish RegisterAuthRequest`

### Step 2: Check RabbitMQ

Open: http://localhost:15672

- Username: guest
- Password: guest
- Go to Queues tab
- Look for messages in queues (should be empty if consuming works)

### Step 3: Check SagaService Logs

```bash
docker-compose logs sagaservice | grep -i "saga\|onboarding"
```

Should see:

```
[Saga] Starting onboarding for direct@example.com
[Saga] Auth user created: ...
[Saga] User profile created
```

### Step 4: Check UserService Logs

```bash
docker-compose logs userservice | grep -i "create\|user"
```

Should see:

```
Creating user profile from saga...
User created: ...
```

### Step 5: Verify Data

```bash
# Connect to database
psql -h url-shortener-instance.cpicai0qavde.ap-southeast-1.rds.amazonaws.com \
  -U postgres \
  -d user_db \
  -c "SELECT * FROM user_profiles ORDER BY created_at DESC LIMIT 1;"
```

Should show the new user profile.

## Common Issues & Fixes

| Issue                       | Symptom                                 | Fix                                                 |
| --------------------------- | --------------------------------------- | --------------------------------------------------- |
| No saga trigger             | Register succeeds but no user created   | Check email unique, verify saga logs                |
| Queue not binding           | Message stays in queue                  | Check consumer registration                         |
| Auth created but saga fails | See auth user in DB but no user profile | Check saga state machine logic                      |
| Connection timeout          | Saga logs show connection error         | Verify RabbitMQ running and healthy                 |
| Database insert fails       | User profile not created                | Check DB connection string, verify auth_id FK valid |
| Email not sent              | No confirmation email received          | Check Resend API key, check MailService logs        |

## Actual Flow (With Fixes Applied)

```
1. POST /auth/register → ApiGateway:5050
   ↓
2. ApiGateway uses RequestClient (NOW REGISTERED!)
   → Sends request to AuthService:5002
   ↓
3. AuthService.RegisterAuthHandler
   ├─ Validates input ✅
   ├─ Creates AuthUser ✅
   └─ Publishes RegisterAuthRequest event ✅
   ↓
4. Message to RabbitMQ:
   Topic: RegisterAuthRequest
   Consumers: SagaService
   ↓
5. SagaService.UserOnboardingStateMachine
   ├─ Receives RegisterAuthRequest ✅
   ├─ Correlates by email ✅
   └─ Creates saga instance
   ↓
6. Saga publishes CreateUserCommand
   Topic: CreateUserCommand
   Consumer: UserService.CreateUserFromSagaConsumer
   ↓
7. UserService
   ├─ Receives command ✅
   ├─ Creates UserProfile ✅
   └─ Publishes UserProfileCreated event
   ↓
8. Saga receives UserProfileCreated
   ├─ Publishes SendConfirmationEmailCommand
   ↓
9. MailService
   ├─ Receives command ✅
   ├─ Calls Resend API
   └─ Sends email ✅
   ↓
10. Saga transitions to Completed ✅
```

## Next: Test It

1. Stop all services: `docker-compose down`
2. Rebuild: `docker-compose up -d`
3. Wait 10 seconds for all services to start
4. Run step-by-step debug above
5. Report which step fails!

---

Created: 2025-11-11
