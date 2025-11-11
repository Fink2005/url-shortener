# 🚀 Hướng dẫn khởi chạy toàn bộ Microservices

## 📋 Yêu cầu

- Docker & Docker Compose
- dotnet CLI (để test locally)
- PostgreSQL (hoặc dùng container)
- RabbitMQ (hoặc dùng container)

---

## 🐳 **Option 1: Khởi chạy với Docker Compose (RECOMMENDED)**

### **Bước 1: Khởi chạy toàn bộ hệ thống**

```bash
cd /Users/fink/Desktop/Workspace/url-shortener

# Khởi chạy tất cả services
docker-compose up -d

# Hoặc xem logs realtime
docker-compose up

# Dừng tất cả
docker-compose down
```

### **Bước 2: Kiểm tra status các services**

```bash
docker-compose ps
```

**Output dự kiến:**

```
CONTAINER ID   IMAGE                    PORTS                    STATUS
xxxxx          url-shortener_rabbitmq   0.0.0.0:5672->5672      Up (healthy)
xxxxx          postgres:16-alpine       0.0.0.0:5432->5432      Up (healthy)
xxxxx          url-shortener_authservice         0.0.0.0:5002->8080  Up
xxxxx          url-shortener_userservice        0.0.0.0:5001->8080  Up
xxxxx          url-shortener_urlservice         0.0.0.0:5003->8080  Up
xxxxx          url-shortener_mailservice        0.0.0.0:5004->8080  Up
xxxxx          url-shortener_sagaservice        0.0.0.0:5005->8080  Up
xxxxx          url-shortener_apigateway        0.0.0.0:5000->8080  Up
```

### **Bước 3: Kiểm tra logs**

```bash
# Xem logs tất cả services
docker-compose logs -f

# Xem logs 1 service cụ thể
docker-compose logs -f authservice
docker-compose logs -f sagaservice
```

---

## 🏃 **Option 2: Khởi chạy Local (CLI)**

### **Bước 1: Khởi chạy Dependencies**

```bash
# Terminal 1: RabbitMQ
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 \
  -e RABBITMQ_DEFAULT_USER=guest \
  -e RABBITMQ_DEFAULT_PASS=guest \
  rabbitmq:3-management

# Terminal 2: PostgreSQL
docker run -d --name postgres -p 5432:5432 \
  -e POSTGRES_PASSWORD=postgres \
  -v postgres_data:/var/lib/postgresql/data \
  postgres:16-alpine
```

### **Bước 2: Chạy Migration**

```bash
# AuthService migration
cd AuthService && dotnet ef database update --project AuthService.Infrastructure --startup-project AuthService.Api

# UserService migration
cd UserService && dotnet ef database update --project UserService.Infrastructure --startup-project UserService.Api

# UrlService migration
cd UrlService && dotnet ef database update --project UrlService.Infrastructure --startup-project UrlService.Api

# SagaService migration
cd SagaService && dotnet ef database update --project SagaService.Infrastructure --startup-project SagaService.Api
```

### **Bước 3: Chạy từng Service**

```bash
# Terminal 3: AuthService (Port 5002)
cd AuthService/AuthService.Api && dotnet run

# Terminal 4: UserService (Port 5001)
cd UserService/UserService.Api && dotnet run

# Terminal 5: UrlService (Port 5003)
cd UrlService/UrlService.Api && dotnet run

# Terminal 6: MailService (Port 5004)
cd MailService/MailService.Api && dotnet run

# Terminal 7: SagaService (Port 5005)
cd SagaService/SagaService.Api && dotnet run

# Terminal 8: ApiGateway (Port 5000)
cd ApiGateway && dotnet run
```

---

## 📊 **Service Ports**

| Service         | Port  | URL                                  |
| --------------- | ----- | ------------------------------------ |
| **ApiGateway**  | 5000  | http://localhost:5000                |
| **UserService** | 5001  | http://localhost:5001                |
| **AuthService** | 5002  | http://localhost:5002                |
| **UrlService**  | 5003  | http://localhost:5003                |
| **MailService** | 5004  | http://localhost:5004                |
| **SagaService** | 5005  | http://localhost:5005                |
| **RabbitMQ UI** | 15672 | http://localhost:15672 (guest/guest) |
| **PostgreSQL**  | 5432  | localhost:5432                       |

---

## ✅ **Kiểm tra Health Check**

```bash
# Kiểm tra từng service
curl http://localhost:5000/health
curl http://localhost:5001/health
curl http://localhost:5002/health
curl http://localhost:5003/health
curl http://localhost:5004/health
curl http://localhost:5005/health

# RabbitMQ Management UI
open http://localhost:15672
# Username: guest
# Password: guest
```

---

## 🧪 **Test Saga Flow**

### **Step 1: Gửi request đăng ký user qua ApiGateway**

```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "email": "test@example.com",
    "password": "Test@123"
  }'
```

### **Step 2: Kiểm tra RabbitMQ messages**

```bash
# Xem queue messages
docker-compose exec rabbitmq rabbitmqctl list_queues
```

### **Step 3: Kiểm tra SagaState trong database**

```bash
# Kết nối vào database
psql -h localhost -U postgres -d saga_db

# Query saga states
SELECT * FROM "UserOnboardingStates";
```

### **Step 4: Kiểm tra logs**

```bash
docker-compose logs sagaservice
docker-compose logs authservice
docker-compose logs userservice
docker-compose logs mailservice
```

---

## 🔧 **Troubleshooting**

### **Port already in use**

```bash
# Tìm process dùng port
lsof -i :5000
kill -9 <PID>

# Hoặc thay đổi port trong docker-compose.yml
```

### **Database connection failed**

```bash
# Kiểm tra PostgreSQL running
docker-compose ps postgres

# Kiểm tra connection string
echo $ConnectionStrings__DefaultConnection
```

### **RabbitMQ not responding**

```bash
# Restart RabbitMQ
docker-compose restart rabbitmq

# Hoặc xem logs
docker-compose logs rabbitmq
```

### **Xóa toàn bộ containers & volumes**

```bash
docker-compose down -v

# Rebuild từ đầu
docker-compose build --no-cache
docker-compose up -d
```

---

## 📝 **Environment Variables**

### **MailService**

```
Resend__ApiKey=re_your_api_key_here
Resend__FromEmail=noreply@example.com
```

### **Database**

```
ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=auth_db;Username=postgres;Password=postgres
```

### **RabbitMQ**

```
RabbitMQ__Host=rabbitmq
RabbitMQ__Port=5672
RabbitMQ__Username=guest
RabbitMQ__Password=guest
```

---

## 🎯 **Monitoring & Debugging**

### **RabbitMQ Monitoring**

- URL: http://localhost:15672
- Xem queues, exchanges, connections
- Monitor message throughput

### **PostgreSQL Monitoring**

```bash
# Connect to database
docker-compose exec postgres psql -U postgres

# List databases
\l

# Connect to saga_db
\c saga_db

# Check tables
\dt
```

### **View all logs**

```bash
docker-compose logs -f --tail=100
```

---

## ✨ **Tips**

1. **Luôn khởi chạy RabbitMQ & PostgreSQL trước**
2. **Chạy migrations trước khi chạy services**
3. **Kiểm tra logs khi có lỗi**
4. **Dùng `docker-compose down -v` để reset state**

Hoàn tất! 🎉 Toàn bộ hệ thống microservices của bạn đã sẵn sàng khởi chạy!
