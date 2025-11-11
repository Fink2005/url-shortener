@echo off
REM 🚀 Startup Script for URL Shortener Microservices (Windows)

setlocal enabledelayedexpansion

echo.
echo ============================================
echo URL Shortener Microservices Startup
echo ============================================
echo.
echo Choose startup option:
echo 1. Docker Compose (all services)
echo 2. Local CLI (individual services)
echo 3. Stop all containers
echo 4. View logs
echo 5. Reset (down -v)
echo.

set /p choice="Enter choice (1-5): "

if "%choice%"=="1" (
    echo.
    echo Starting Docker Compose...
    docker-compose up -d
    timeout /t 5
    echo.
    echo All services are starting!
    docker-compose ps
) else if "%choice%"=="2" (
    echo.
    echo Starting Local CLI...
    echo Please ensure RabbitMQ and PostgreSQL are running.
    echo.
    echo Open new terminals and run:
    echo.
    echo Terminal 1 - AuthService:
    echo cd AuthService\AuthService.Api ^& dotnet run
    echo.
    echo Terminal 2 - UserService:
    echo cd UserService\UserService.Api ^& dotnet run
    echo.
    echo Terminal 3 - UrlService:
    echo cd UrlService\UrlService.Api ^& dotnet run
    echo.
    echo Terminal 4 - MailService:
    echo cd MailService\MailService.Api ^& dotnet run
    echo.
    echo Terminal 5 - SagaService:
    echo cd SagaService\SagaService.Api ^& dotnet run
    echo.
    echo Terminal 6 - ApiGateway:
    echo cd ApiGateway ^& dotnet run
) else if "%choice%"=="3" (
    echo.
    echo Stopping all containers...
    docker-compose stop
    echo All containers stopped!
) else if "%choice%"=="4" (
    echo.
    echo Showing logs...
    docker-compose logs -f --tail=50
) else if "%choice%"=="5" (
    set /p confirm="Are you sure? (y/n): "
    if "!confirm!"=="y" (
        docker-compose down -v
        echo Reset complete!
    )
) else (
    echo Invalid choice!
    exit /b 1
)

echo.
echo Done!
