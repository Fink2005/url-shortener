# 🚀 Hướng dẫn chạy Migration cho SagaService

## ✅ Các bước đã hoàn thành:

### 1. **Cập nhật Database Connection**

- File: `SagaService.Api/appsettings.json`
- Database: `saga_db` (đã tạo trên PostgreSQL)
- Connection String được cấu hình cho AWS RDS PostgreSQL

### 2. **Tạo Initial Migration**

```bash
cd SagaService
dotnet ef migrations add InitSaga --project SagaService.Infrastructure --startup-project SagaService.Api
```

Kết quả:

- ✓ Migration file: `20251110212759_InitSaga.cs`
- ✓ Tạo bảng: `UserOnboardingStates` với các cột:
  - `CorrelationId` (UUID, Primary Key)
  - `CurrentState` (text) - Trạng thái saga
  - `AuthId` (UUID nullable) - ID auth user
  - `UserId` (UUID nullable) - ID user profile
  - `Username` (text) - Username
  - `Email` (text) - Email
  - `ConfirmationToken` (text) - Token xác nhận
  - `EmailConfirmed` (boolean) - Đã xác nhận email?
  - `AssignedRole` (text) - Role được gán
  - `CreatedAt` (timestamp) - Thời gian tạo
  - `CompletedAt` (timestamp nullable) - Thời gian hoàn thành
  - `FailureReason` (text nullable) - Lý do thất bại

### 3. **Áp dụng Migration vào Database**

```bash
dotnet ef database update --project SagaService.Infrastructure --startup-project SagaService.Api
```

Kết quả:

- ✓ Database `saga_db` đã sẵn sàng
- ✓ Bảng `UserOnboardingStates` được tạo
- ✓ Migration history được ghi lại

---

## 🔄 Các lệnh Migration hữu ích:

### **Kiểm tra trạng thái Migration:**

```bash
dotnet ef migrations list --project SagaService.Infrastructure --startup-project SagaService.Api
```

### **Xem SQL sẽ thực thi:**

```bash
dotnet ef migrations script --project SagaService.Infrastructure --startup-project SagaService.Api
```

### **Rollback migration (Undo):**

```bash
dotnet ef migrations remove --project SagaService.Infrastructure --startup-project SagaService.Api
```

### **Xóa database và tạo lại:**

```bash
dotnet ef database drop --project SagaService.Infrastructure --startup-project SagaService.Api
dotnet ef database update --project SagaService.Infrastructure --startup-project SagaService.Api
```

---

## 📝 Tạo Migration mới (khi thay đổi DbModel):

1. **Sửa `UserOnboardingState.cs` hoặc `SagaStateDbContext.cs`**

2. **Tạo migration mới:**

   ```bash
   dotnet ef migrations add DescriptionOfChange --project SagaService.Infrastructure --startup-project SagaService.Api
   ```

3. **Áp dụng vào database:**
   ```bash
   dotnet ef database update --project SagaService.Infrastructure --startup-project SagaService.Api
   ```

---

## 🔐 Notes:

- **MigrationsAssembly**: Migrations được lưu trong `SagaService.Infrastructure`
- **DbContext**: `SagaStateDbContext` (nằm trong Infrastructure)
- **Database**: PostgreSQL trên AWS RDS
- **Timeout**: Default 30s (có thể tăng nếu cần)

---

## ✅ Kiểm tra Database đã tạo thành công:

Kết nối vào PostgreSQL database `saga_db` và chạy:

```sql
SELECT * FROM "UserOnboardingStates";
```

Bảng sẽ trống (chưa có saga instance) nhưng schema đã được tạo sẵn.
