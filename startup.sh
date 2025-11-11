#!/bin/bash

# 🚀 Startup Script cho URL Shortener Microservices

set -e  # Exit on error

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

echo "🚀 URL Shortener Microservices Startup"
echo "======================================"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Menu
echo ""
echo "Chọn option khởi chạy:"
echo "1. Docker Compose (toàn bộ hệ thống)"
echo "2. Local CLI (riêng lẻ từng service)"
echo "3. Dừng tất cả containers"
echo "4. View logs"
echo "5. Reset (down -v)"
echo ""
read -p "Nhập lựa chọn (1-5): " choice

case $choice in
    1)
        echo -e "${YELLOW}📦 Khởi chạy Docker Compose...${NC}"
        docker-compose up -d
        sleep 5
        echo -e "${GREEN}✅ Tất cả services đang chạy!${NC}"
        docker-compose ps
        ;;
    2)
        echo -e "${YELLOW}🏃 Khởi chạy Local CLI...${NC}"
        echo "Cần khởi chạy dependencies trước:"
        echo "1. RabbitMQ"
        echo "2. PostgreSQL"
        echo ""
        read -p "Bạn đã khởi chạy RabbitMQ & PostgreSQL chưa? (y/n): " deps
        
        if [ "$deps" != "y" ]; then
            echo -e "${YELLOW}Khởi chạy dependencies...${NC}"
            docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 \
              -e RABBITMQ_DEFAULT_USER=guest \
              -e RABBITMQ_DEFAULT_PASS=guest \
              rabbitmq:3-management
            
            docker run -d --name postgres -p 5432:5432 \
              -e POSTGRES_PASSWORD=postgres \
              -v postgres_data:/var/lib/postgresql/data \
              postgres:16-alpine
            
            sleep 10
            echo -e "${GREEN}✅ Dependencies started!${NC}"
        fi
        
        echo ""
        echo -e "${YELLOW}Khởi chạy services từng cái...${NC}"
        echo "Mở terminal mới và chạy:"
        echo ""
        echo "Terminal 1 - AuthService:"
        echo "cd AuthService/AuthService.Api && dotnet run"
        echo ""
        echo "Terminal 2 - UserService:"
        echo "cd UserService/UserService.Api && dotnet run"
        echo ""
        echo "Terminal 3 - UrlService:"
        echo "cd UrlService/UrlService.Api && dotnet run"
        echo ""
        echo "Terminal 4 - MailService:"
        echo "cd MailService/MailService.Api && dotnet run"
        echo ""
        echo "Terminal 5 - SagaService:"
        echo "cd SagaService/SagaService.Api && dotnet run"
        echo ""
        echo "Terminal 6 - ApiGateway:"
        echo "cd ApiGateway && dotnet run"
        ;;
    3)
        echo -e "${YELLOW}⛔ Dừng tất cả containers...${NC}"
        docker-compose stop
        echo -e "${GREEN}✅ Tất cả containers đã dừng!${NC}"
        ;;
    4)
        echo -e "${YELLOW}📋 Logs...${NC}"
        docker-compose logs -f --tail=50
        ;;
    5)
        echo -e "${RED}🔥 Reset toàn bộ (xóa containers & volumes)...${NC}"
        read -p "Bạn chắc chắn muốn reset? (y/n): " confirm
        if [ "$confirm" = "y" ]; then
            docker-compose down -v
            echo -e "${GREEN}✅ Reset thành công!${NC}"
        fi
        ;;
    *)
        echo -e "${RED}❌ Lựa chọn không hợp lệ!${NC}"
        exit 1
        ;;
esac

echo ""
echo "🎉 Done!"
