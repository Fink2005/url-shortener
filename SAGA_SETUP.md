# Saga Orchestration Setup - Complete Guide

## Current Status

✅ **Completed:**
- All 7 services running (AuthService, UserService, UrlService, SagaService, MailService, ApiGateway, RabbitMQ)
- Swagger UI accessible on all services
- MassTransit configured for all services
- RequestClient registration in ApiGateway for auth requests
- Saga state machine defined with 6 states
- Database connections configured for AWS RDS

❌ **Testing Needed:**
- End-to-end saga workflow
- User profile creation via saga
- Email confirmation flow

## Architecture Flow

```
┌─────────────────────────────────────────────────────────────┐
│                    API Gateway (5050)                        │
│  POST /auth/register → AuthGatewayController                │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│                 AuthService (5002)                           │
│ RegisterAuthHandler:                                         │
│  1. Validate credentials                                     │
│  2. Create AuthUser                                          │
│  3. Publish RegisterAuthRequest event                        │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼ RabbitMQ
┌─────────────────────────────────────────────────────────────┐
│              SagaService (5005)                              │
│ UserOnboardingStateMachine:                                  │
│  State 1: AwaitingAuthCreation                              │
│    ├─ Receive: RegisterAuthRequest (triggers via correlation)
│    └─ Send: CreateAuthUserCommand to AuthService            │
│  State 2: AwaitingEmailConfirmation                         │
│    └─ Send: SendConfirmationEmailCommand to MailService     │
│  State 3: AwaitingUserCreation                              │
│    └─ Send: CreateUserCommand to UserService                │
│  State 4: Completed                                          │
└────────────────────┬────────────────────────────────────────┘
                     │
        ┌────────────┼────────────┬────────────┐
        ▼            ▼            ▼            ▼
   AuthService   UserService  MailService  UrlService
   (5002)        (5001)       (5004)       (5003)
   ├─Create      ├─Create     ├─Send       │
   │ Auth User   │ User       │ Email      │
   └─Return      │ Profile    └─Confirm    │
     Event       └─Return        Token     │
                   Event                   │
```

## Events & Commands Flow

### Event: RegisterAuthRequest (triggers saga)
```json
{
  "username": "string",
  "email": "string",
  "password": "string"
}
```

**Where it flows:**
1. AuthService publishes this event
2. SagaService state machine listens via correlation `state.Email == message.Email`
3. Saga instance created with new CorrelationId

### Command: CreateAuthUserCommand
```json
{
  "correlationId": "guid",
  "username": "string",
  "email": "string",
  "password": "string"
}
```

**Who sends:** Saga
**Who receives:** AuthService Consumer

### Event: AuthUserCreated (response)
```json
{
  "correlationId": "guid",
  "authUserId": "guid",
  "username": "string",
  "email": "string"
}
```

### Command: SendConfirmationEmailCommand
```json
{
  "correlationId": "guid",
  "email": "string",
  "confirmationToken": "string",
  "username": "string"
}
```

**Who sends:** Saga
**Who receives:** MailService

### Command: CreateUserCommand
```json
{
  "correlationId": "guid",
  "username": "string",
  "email": "string",
  "authUserId": "guid",
  "role": "User"
}
```

**Who sends:** Saga
**Who receives:** UserService

### Event: UserProfileCreated (response)
```json
{
  "correlationId": "guid",
  "userId": "guid",
  "username": "string",
  "email": "string",
  "authUserId": "guid"
}
```

## Testing the Saga

### 1. Register User (Trigger Saga)
```bash
curl -X POST http://localhost:5050/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "email": "test@example.com",
    "password": "SecurePass123!"
  }'
```

**Expected Response:**
```json
{
  "success": true
}
```

### 2. Check AuthService Logs
```bash
docker-compose logs authservice | tail -20
```
Should see: `"Publish RegisterAuthRequest"`

### 3. Check SagaService Logs
```bash
docker-compose logs sagaservice | tail -20
```
Should see:
```
[Saga] Starting onboarding for test@example.com
[Saga] Auth user created: <auth-id>
[Saga] Email sent
[Saga] User profile created
```

### 4. Check UserService Logs
```bash
docker-compose logs userservice | tail -20
```
Should see: User profile created

### 5. Verify Data in Databases
```bash
# Connect to RDS and check each database
psql -h url-shortener-instance.cpicai0qavde.ap-southeast-1.rds.amazonaws.com \
  -U postgres \
  -d auth_db \
  -c "SELECT * FROM auth_users WHERE email='test@example.com';"

psql -h ... -d user_db \
  -c "SELECT * FROM user_profiles WHERE email='test@example.com';"

psql -h ... -d saga_db \
  -c "SELECT * FROM user_onboarding_states WHERE email='test@example.com';"
```

## Debugging Commands

### Check RabbitMQ Messages
- UI: http://localhost:15672 (guest/guest)
- Check queues: `auth-service`, `user-service`, `saga-service`
- Check exchanges: Events routing

### Check Service Health
```bash
curl http://localhost:5001/health  # UserService
curl http://localhost:5002/health  # AuthService
curl http://localhost:5003/health  # UrlService
curl http://localhost:5004/health  # MailService
curl http://localhost:5005/health  # SagaService
```

### Check Saga State
```bash
docker-compose exec saga_db psql -U postgres -d saga_db \
  -c "SELECT * FROM user_onboarding_states;"
```

## Key Configuration Points

### ApiGateway/Program.cs
- RequestClients registered for: RegisterAuthRequest, LoginAuthRequest, RefreshTokenRequest, LogoutRequest, DeleteAuthRequest
- RabbitMQ host: `rabbitmq` (Docker network DNS)

### AuthService/Program.cs
- RabbitMQ host: `rabbitmq`
- RegisterAuthHandler injects IPublishEndpoint
- Publishes RegisterAuthRequest after creating auth user

### SagaService/Program.cs
- Saga state machine registered
- EF repository configured for Postgres
- Pessimistic locking enabled

### docker-compose.yml
- All services connected to `url-shortener-network`
- AWS RDS connection strings configured
- Resend API key configured in MailService env

## Troubleshooting

### Saga not triggering
1. Check AuthService logs for "Publish" message
2. Check RabbitMQ UI for message in queues
3. Verify `state.Email == message.Email` correlation matches

### User not created
1. Check SagaService logs for CreateUserCommand publish
2. Check UserService logs for command receive
3. Verify UserService consumer registered

### Email not sent
1. Check MailService logs
2. Verify Resend API key is valid
3. Check `Resend__ApiKey` env variable set

## Next Steps

1. ✅ All services should be running
2. ⏳ Test registration endpoint
3. ⏳ Verify saga state transitions
4. ⏳ Confirm user created in UserService DB
5. ⏳ Test confirmation email receipt

---
Last Updated: 2025-11-11
