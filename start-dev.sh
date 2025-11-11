#!/bin/bash

# Startup script để chạy local development environment

echo "🚀 Starting local development environment..."

# Start RabbitMQ + Redis in Docker
echo "📦 Starting RabbitMQ and Redis..."
docker-compose -f docker-compose.dev.yml up -d

# Wait for services to be ready
sleep 5

echo ""
echo "✅ Dependencies running:"
echo "   - RabbitMQ: http://localhost:15672 (guest/guest)"
echo "   - Redis: localhost:6379"
echo ""
echo "🔧 Services available. Run in separate terminals:"
echo ""
echo "Terminal 1 (AuthService):"
echo "  cd AuthService/AuthService.Api && dotnet run"
echo ""
echo "Terminal 2 (MailService):"
echo "  cd MailService/MailService.Api && dotnet run"
echo ""
echo "Terminal 3 (UserService):"
echo "  cd UserService/UserService.Api && dotnet run"
echo ""
echo "Terminal 4 (UrlService):"
echo "  cd UrlService/UrlService.Api && dotnet run"
echo ""
echo "Terminal 5 (SagaService):"
echo "  cd SagaService/SagaService.Api && dotnet run"
echo ""
echo "Terminal 6 (ApiGateway):"
echo "  cd ApiGateway && dotnet run"
echo ""
echo "📍 Services will run on:"
echo "   - AuthService: http://localhost:5002"
echo "   - MailService: http://localhost:5004"
echo "   - UserService: http://localhost:5001"
echo "   - UrlService: http://localhost:5003"
echo "   - SagaService: http://localhost:5005"
echo "   - ApiGateway: http://localhost:5050"
echo ""
echo "Stop dependencies with:"
echo "  docker-compose -f docker-compose.dev.yml down"
echo ""
