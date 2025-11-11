# Development & Production Setup Guide

## 🚀 Quick Start

### Development (Local with `dotnet run`)

```bash
# Terminal 1: Start dependencies (RabbitMQ + Redis) in Docker
docker-compose -f docker-compose.dev.yml up

# Terminal 2-7: Start each service in separate terminals
cd AuthService/AuthService.Api && dotnet run
cd MailService/MailService.Api && dotnet run
cd UserService/UserService.Api && dotnet run
cd UrlService/UrlService.Api && dotnet run
cd SagaService/SagaService.Api && dotnet run
cd ApiGateway && dotnet run
```

**Or use the startup script:**

```bash
./start-dev.sh
```

**Services will run on:**

- AuthService: http://localhost:5002
- MailService: http://localhost:5004
- UserService: http://localhost:5001
- UrlService: http://localhost:5003
- SagaService: http://localhost:5005
- ApiGateway: http://localhost:5050

### Production (Full Docker)

```bash
./start-prod.sh
```

Or manually:

```bash
docker-compose build
docker-compose up -d
```

## 📝 Configuration

### Local Development (appsettings.Development.json)

Each service has `appsettings.Development.json` configured for local development:

- **RabbitMQ Host**: `localhost`
- **Redis Connection**: `localhost:6379`
- **Database**: `localhost:5432` (PostgreSQL)
- **User**: `postgres` / **Password**: `postgres`

### Docker Production (docker-compose.yml)

Services use Docker network hostnames:

- **RabbitMQ Host**: `rabbitmq` (internal Docker network)
- **Redis Connection**: `redis:6379`
- **Database**: `url-shortener-instance.cpicai0qavde.ap-southeast-1.rds.amazonaws.com` (AWS RDS)

## 🐳 Docker Compose Files

- **docker-compose.dev.yml**: Only RabbitMQ + Redis (minimal for local dev)
- **docker-compose.yml**: Full production setup with all services

## 📊 Accessing Services

### Local Development

```bash
# Test AuthService
curl http://localhost:5002/swagger

# Test API Gateway
curl http://localhost:5050/swagger

# RabbitMQ Management
http://localhost:15672 (guest/guest)
```

### Production (Docker)

```bash
# Same endpoints but services are in Docker
docker-compose logs -f authservice
docker-compose exec authservice bash
```

## 🔧 Environment Variables

Services automatically read from:

1. **appsettings.Development.json** (local dev - defaults to localhost)
2. **appsettings.json** (fallback)
3. **docker-compose.yml** environment variables (Docker production)

## 📦 Dependencies

- .NET 9.0 SDK (local dev)
- Docker & Docker Compose (both dev and prod)
- PostgreSQL 15 (local or RDS)
- RabbitMQ 3
- Redis 7

## 🛑 Stopping Services

### Local Development

```bash
# Ctrl+C to stop all services from start-dev.sh
# Or manually stop each terminal

# Stop Docker dependencies
docker-compose -f docker-compose.dev.yml down
```

### Production

```bash
docker-compose down
```

## 🐛 Troubleshooting

### "Connection refused" on localhost:5672

- Check if RabbitMQ is running: `docker-compose -f docker-compose.dev.yml ps`
- Start it: `docker-compose -f docker-compose.dev.yml up -d`

### "Cannot connect to Redis"

- Verify Redis is running: `docker-compose -f docker-compose.dev.yml ps redis`
- Test connection: `redis-cli ping`

### Service can't connect to database

- Check PostgreSQL is running
- Verify connection string in appsettings.Development.json
- Local default: `Host=localhost;Port=5432;Database=auth_db;Username=postgres;Password=postgres`
