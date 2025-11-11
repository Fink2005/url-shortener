#!/bin/bash

# Startup script để chạy production environment với Docker

echo "🚀 Starting production environment with Docker..."

# Build tất cả images
echo "🔨 Building Docker images..."
docker-compose build

# Start all services
echo "📦 Starting all services..."
docker-compose up -d

sleep 5

echo ""
echo "✅ Production environment running!"
echo ""
echo "🐳 Docker Compose Services:"
docker-compose ps

echo ""
echo "📍 Services available on:"
echo "   - ApiGateway: http://localhost:5050"
echo "   - AuthService: http://localhost:5002"
echo "   - UserService: http://localhost:5001"
echo "   - UrlService: http://localhost:5003"
echo "   - MailService: http://localhost:5004"
echo "   - SagaService: http://localhost:5005"
echo "   - RabbitMQ Dashboard: http://localhost:15672 (guest/guest)"
echo "   - Redis Commander: (optional, install separately)"
echo ""
echo "View logs: docker-compose logs -f [service_name]"
echo "Stop: docker-compose down"
