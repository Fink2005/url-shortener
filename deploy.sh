#!/bin/bash

echo "🚀 Deploying URL Shortener to Ubuntu Production Server"
echo "======================================================="

# Check if running as root
if [ "$EUID" -eq 0 ]; then 
    echo "⚠️  Please don't run as root. Run as normal user."
    exit 1
fi

# Stop existing services
echo "🛑 Stopping existing services..."
docker-compose down

# Pull latest code (if using git)
echo "📥 Pulling latest code..."
git pull origin main || echo "⚠️  Not a git repo, skipping..."

# Build and start services
echo "🔨 Building and starting services..."
docker-compose up -d --build

# Wait for services to be healthy
echo "⏳ Waiting for services to start..."
sleep 30

# Check service status
echo ""
echo "📊 Service Status:"
docker-compose ps

# Show logs
echo ""
echo "📝 Recent Logs:"
docker-compose logs --tail=20

echo ""
echo "✅ Deployment complete!"
echo ""
echo "🔗 Access your services at:"
echo "   - API Gateway: http://$(hostname -I | awk '{print $1}'):5050"
echo "   - RabbitMQ Management: http://$(hostname -I | awk '{print $1}'):15672"
echo ""
echo "📊 Useful commands:"
echo "   - View logs: docker-compose logs -f [service_name]"
echo "   - Restart: docker-compose restart [service_name]"
echo "   - Stop all: docker-compose down"
echo "   - Check status: docker-compose ps"
