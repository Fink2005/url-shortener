.PHONY: help up down logs ps reset clean build migrate test

help: ## Show this help
	@echo "URL Shortener Microservices - Available Commands"
	@echo "================================================"
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "  \033[36m%-20s\033[0m %s\n", $$1, $$2}'

up: ## Start all services with Docker Compose
	@echo "🚀 Starting all services..."
	docker-compose up -d
	@sleep 3
	@echo "✅ Services started!"
	@make ps

down: ## Stop all services
	@echo "⛔ Stopping services..."
	docker-compose stop
	@echo "✅ Services stopped!"

ps: ## Show running containers
	@docker-compose ps

logs: ## View logs from all services
	docker-compose logs -f --tail=50

logs-saga: ## View SagaService logs
	docker-compose logs -f sagaservice --tail=50

logs-mail: ## View MailService logs
	docker-compose logs -f mailservice --tail=50

logs-auth: ## View AuthService logs
	docker-compose logs -f authservice --tail=50

logs-user: ## View UserService logs
	docker-compose logs -f userservice --tail=50

logs-url: ## View UrlService logs
	docker-compose logs -f urlservice --tail=50

logs-gateway: ## View ApiGateway logs
	docker-compose logs -f apigateway --tail=50

build: ## Build all Docker images
	@echo "🔨 Building Docker images..."
	docker-compose build

rebuild: ## Rebuild all Docker images (no cache)
	@echo "🔨 Rebuilding Docker images..."
	docker-compose build --no-cache

reset: ## Reset everything (remove containers and volumes)
	@read -p "⚠️  This will delete all data. Continue? [y/N] " -n 1 -r; \
	echo ""; \
	if [[ $$REPLY =~ ^[Yy]$$ ]]; then \
		echo "🔥 Resetting..."; \
		docker-compose down -v; \
		echo "✅ Reset complete!"; \
	else \
		echo "Cancelled"; \
	fi

clean: ## Clean build artifacts
	@echo "🧹 Cleaning build artifacts..."
	find . -type d -name "bin" -exec rm -rf {} + 2>/dev/null || true
	find . -type d -name "obj" -exec rm -rf {} + 2>/dev/null || true
	echo "✅ Clean complete!"

migrate: ## Run database migrations
	@echo "📦 Running migrations..."
	@echo "SagaService migration..."
	cd SagaService/SagaService.Api && dotnet ef database update --project ../SagaService.Infrastructure/SagaService.Infrastructure.csproj

status: ## Check service health
	@echo "🏥 Checking service health..."
	@docker-compose ps --format "table {{.Names}}\t{{.Status}}"

test-saga: ## Test SagaService endpoint
	@echo "🧪 Testing SagaService..."
	curl -X GET http://localhost:5005/health || echo "SagaService not responding"

test-auth: ## Test AuthService endpoint
	@echo "🧪 Testing AuthService..."
	curl -X GET http://localhost:5002/health || echo "AuthService not responding"

test-mail: ## Test MailService endpoint
	@echo "🧪 Testing MailService..."
	curl -X GET http://localhost:5004/health || echo "MailService not responding"

test-all: ## Test all service endpoints
	@echo "🧪 Testing all services..."
	@echo "AuthService..."
	@curl -s -X GET http://localhost:5002/health > /dev/null && echo "✅ AuthService OK" || echo "❌ AuthService DOWN"
	@echo "UserService..."
	@curl -s -X GET http://localhost:5001/health > /dev/null && echo "✅ UserService OK" || echo "❌ UserService DOWN"
	@echo "UrlService..."
	@curl -s -X GET http://localhost:5003/health > /dev/null && echo "✅ UrlService OK" || echo "❌ UrlService DOWN"
	@echo "MailService..."
	@curl -s -X GET http://localhost:5004/health > /dev/null && echo "✅ MailService OK" || echo "❌ MailService DOWN"
	@echo "SagaService..."
	@curl -s -X GET http://localhost:5005/health > /dev/null && echo "✅ SagaService OK" || echo "❌ SagaService DOWN"
	@echo "ApiGateway..."
	@curl -s -X GET http://localhost:5000/health > /dev/null && echo "✅ ApiGateway OK" || echo "❌ ApiGateway DOWN"

shell-postgres: ## Open PostgreSQL shell
	@docker-compose exec postgres psql -U postgres -d postgres

shell-rabbitmq: ## Open RabbitMQ management UI
	@echo "🐰 Opening RabbitMQ Management UI..."
	@open "http://localhost:15672" || echo "Visit http://localhost:15672 (guest/guest)"

db-saga: ## Connect to SagaService database
	docker-compose exec postgres psql -U postgres -d saga_db

db-auth: ## Connect to AuthService database
	docker-compose exec postgres psql -U postgres -d auth_db

db-user: ## Connect to UserService database
	docker-compose exec postgres psql -U postgres -d user_db

db-url: ## Connect to UrlService database
	docker-compose exec postgres psql -U postgres -d url_db

dev-local: ## Run services locally (requires RabbitMQ & PostgreSQL)
	@echo "📝 Starting local development..."
	@echo "Make sure RabbitMQ and PostgreSQL are running!"
	@echo ""
	@echo "Run these commands in separate terminals:"
	@echo ""
	@echo "Terminal 1 (AuthService):"
	@echo "  cd AuthService/AuthService.Api && dotnet run"
	@echo ""
	@echo "Terminal 2 (UserService):"
	@echo "  cd UserService/UserService.Api && dotnet run"
	@echo ""
	@echo "Terminal 3 (UrlService):"
	@echo "  cd UrlService/UrlService.Api && dotnet run"
	@echo ""
	@echo "Terminal 4 (MailService):"
	@echo "  cd MailService/MailService.Api && dotnet run"
	@echo ""
	@echo "Terminal 5 (SagaService):"
	@echo "  cd SagaService/SagaService.Api && dotnet run"
	@echo ""
	@echo "Terminal 6 (ApiGateway):"
	@echo "  cd ApiGateway && dotnet run"
