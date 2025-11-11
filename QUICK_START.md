# 🚀 URL Shortener - Quick Start Guide

## 📋 Tóm tắt hệ thống

- **6 Microservices**: AuthService, UserService, UrlService, MailService, SagaService, ApiGateway
- **Message Broker**: RabbitMQ
- **Database**: PostgreSQL (4 databases)
- **Container**: Docker Compose
- **Pattern**: Saga Orchestration + DDD

---

## ⚡ Quick Start (Recommend)

### Với Docker Compose (Simplest)

```bash
# 1. Start all services
docker-compose up -d

# 2. Check status
docker-compose ps

# 3. View logs
docker-compose logs -f sagaservice
```

**Services sẽ chạy trên:**

- API Gateway: http://localhost:5000
- AuthService: http://localhost:5002
- UserService: http://localhost:5001
- UrlService: http://localhost:5003
- MailService: http://localhost:5004
- SagaService: http://localhost:5005
- RabbitMQ Management: http://localhost:15672 (guest/guest)
- PostgreSQL: localhost:5432 (postgres/postgres)

---

## 🛠️ Sử dụng Makefile (Recommended)

### Khởi chạy

```bash
# Start all services
make up

# Check status
make ps

# View logs
make logs
make logs-saga
make logs-mail
```

### Testing

```bash
# Test all services
make test-all

# Test individual service
make test-saga
make test-auth
```

### Database

```bash
# Connect to SagaService database
make db-saga

# Connect to PostgreSQL shell
make shell-postgres

# Open RabbitMQ UI
make shell-rabbitmq
```

### Management

```bash
# Stop all services
make down

# Reset everything (delete containers + volumes)
make reset

# View all available commands
make help
```

---

## 🖥️ Sử dụng Script

### macOS / Linux

```bash
chmod +x startup.sh
./startup.sh
```

### Windows

```cmd
startup.bat
```

---

## 💻 Local Development (Manual)

Nếu muốn chạy từng service riêng lẻ:

### 1. Start Dependencies (optional - Docker Compose)

```bash
docker-compose up -d rabbitmq postgres
```

Hoặc chạy local:

- RabbitMQ: http://localhost:5672
- PostgreSQL: postgres://postgres:postgres@localhost:5432

### 2. Run Each Service

**Terminal 1 - AuthService:**

```bash
cd AuthService/AuthService.Api
dotnet run
```

**Terminal 2 - UserService:**

```bash
cd UserService/UserService.Api
dotnet run
```

**Terminal 3 - UrlService:**

```bash
cd UrlService/UrlService.Api
dotnet run
```

**Terminal 4 - MailService:**

```bash
cd MailService/MailService.Api
dotnet run
```

**Terminal 5 - SagaService:**

```bash
cd SagaService/SagaService.Api
dotnet run
```

**Terminal 6 - ApiGateway:**

```bash
cd ApiGateway
dotnet run
```

---

## 📊 Kiểm tra Workflow

### 1. Register User (Trigger Saga)

```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "Password123!"
  }'
```

### 2. Check Saga State

```sql
SELECT * FROM "UserOnboardingStates"
ORDER BY "CreatedAt" DESC
LIMIT 1;
```

```bash
# Via Docker
docker-compose exec postgres psql -U postgres -d saga_db -c \
  'SELECT "CorrelationId", "CurrentState", "Email", "ConfirmationToken" FROM "UserOnboardingStates" ORDER BY "CreatedAt" DESC LIMIT 1;'
```

### 3. View Logs

```bash
# SagaService
docker-compose logs -f sagaservice

# MailService
docker-compose logs -f mailservice

# AuthService
docker-compose logs -f authservice

# All
docker-compose logs -f
```

---

## 🔧 Troubleshooting

### Services không khởi chạy

```bash
# Check logs
docker-compose logs

# Check container status
docker-compose ps

# Check ports
lsof -i :5000-5005
```

### Database errors

```bash
# Reset databases
docker-compose down -v

# Recreate
docker-compose up -d
```

### RabbitMQ issues

```bash
# Check RabbitMQ
docker-compose logs rabbitmq

# Access RabbitMQ UI: http://localhost:15672 (guest/guest)
```

---

## 📚 Configuration

### Environment Variables

Edit `docker-compose.yml` để thay đổi:

- Database credentials
- RabbitMQ settings
- API ports
- Resend API key

### Database Strings

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=postgres;Port=5432;Database=saga_db;Username=postgres;Password=postgres"
  }
}
```

---

## 🧪 Testing

### Health Check

```bash
curl http://localhost:5000/health
curl http://localhost:5002/health
curl http://localhost:5001/health
curl http://localhost:5003/health
curl http://localhost:5004/health
curl http://localhost:5005/health
```

### Database Check

```bash
# List all databases
docker-compose exec postgres psql -U postgres -l

# Check saga table
docker-compose exec postgres psql -U postgres -d saga_db -c \
  '\dt "UserOnboardingStates"'
```

---

## 🚦 Workflow Flow

```
1. User calls /api/auth/register
   ↓
2. SagaService creates UserOnboarding saga
   ↓
3. AuthService creates auth user
   ↓
4. MailService sends confirmation email
   ↓
5. User confirms email (token validation)
   ↓
6. AuthService assigns default role
   ↓
7. UserService creates user profile
   ↓
8. Saga completes ✅
```

---

## 📝 More Commands

```bash
# View all available commands
make help

# Show Docker compose services
docker-compose config --services

# Rebuild images
docker-compose build --no-cache

# Clean local build artifacts
make clean
```

---

## 🔗 Useful Links

- Docker Compose: https://docs.docker.com/compose/
- RabbitMQ: http://localhost:15672
- PostgreSQL: postgresql://postgres:postgres@localhost:5432
- MassTransit: https://masstransit.io/
- Entity Framework Core: https://docs.microsoft.com/ef/core/

---

## 💡 Tips

- **One command start**: `docker-compose up -d`
- **One command stop**: `docker-compose down`
- **View all logs**: `docker-compose logs -f`
- **Database access**: `make db-saga`
- **All help**: `make help`

---

Made with ❤️ for Microservices
