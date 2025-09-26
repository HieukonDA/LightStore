# Tóm tắt Migration: Thêm TargetRole cho hệ thống Notifications

## 📋 Những gì đã thay đổi:

### 1. Database Schema
- ✅ **Thêm cột `TargetRole`** vào bảng `Notifications`
  - Type: `NVARCHAR(20) NOT NULL DEFAULT 'admin'`
  - Values: `'admin'`, `'customer'`, `'all'`
  - Constraint: `CK_Notifications_TargetRole`

- ✅ **Indexes mới** cho performance:
  - `IX_Notifications_TargetRole`
  - `IX_Notifications_UserId_TargetRole_IsRead`

- ✅ **Stored procedures** mới:
  - `sp_GetNotificationsByRole`
  - `sp_GetNotificationStatsByRole`

### 2. Models & DTOs
- ✅ **Notification.cs**: Thêm property `TargetRole`
- ✅ **NotificationDto.cs**: Thêm `TargetRole` field
- ✅ **CreateNotificationDto.cs**: Đổi `TargetType` → `TargetRole`

### 3. NotificationService
- ✅ **CreateAndBroadcastNotificationAsync**: 
  - Lưu `TargetRole` vào database
  - Logic routing dựa trên `TargetRole` thay vì URL guessing

- ✅ **GetUserNotificationsAsync**: 
  - Thêm parameter `userRole`
  - Filter theo `TargetRole` và `userRole`

- ✅ **GetUserNotificationStatsAsync**: 
  - Thêm parameter `userRole`
  - Stats theo role-specific notifications

### 4. Controllers
- ✅ **NotificationController** (Admin):
  - Calls với `userRole = "admin"`
  - Chỉ nhận thông báo `TargetRole = "admin"` hoặc `"all"`

- ✅ **CustomerNotificationController** (Customer):
  - Calls với `userRole = "customer"`
  - Chỉ nhận thông báo `TargetRole = "customer"` hoặc `"all"`

### 5. Business Logic Updates
- ✅ **Admin Notifications**:
  ```csharp
  TargetRole = "admin"
  // NotifyNewOrderAsync, NotifyPaymentSuccessAsync, NotifyLowStockAsync
  ```

- ✅ **Customer Notifications**:
  ```csharp
  TargetRole = "customer" 
  // NotifyCustomerOrderStatusAsync, NotifyCustomerPaymentAsync, NotifyCustomerPromotionAsync
  ```

## 🔧 Files Modified:

### Core Files
1. `_Models/Notifications/Notification.cs`
2. `_Dtos/Notifications/NotificationDto.cs`
3. `_Services.cs/Notification/NotificationService.cs`
4. `_Interfaces/Notification/INotificationService.cs`

### Controllers
5. `_Controllers/NotificationController.cs`
6. `_Controllers/CustomerNotificationController.cs`

### Database
7. `Migration_AddTargetRole.sql`

### Debug & Testing
8. `_Controllers/SignalRDebugController.cs`
9. `RunMigration.ps1`

## 🎯 Kết quả:

### ❌ Trước (VẤN ĐỀ):
- Customer nhận được thông báo admin: "Khách hàng X đặt đơn hàng"
- Logic phân biệt dựa vào URL (không reliable)
- 404 Error cho SignalR negotiate endpoint

### ✅ Sau (GIẢI PHÁP):
- **Admin** chỉ nhận: `TargetRole = "admin"` hoặc `"all"`
- **Customer** chỉ nhận: `TargetRole = "customer"` hoặc `"all"`
- Logic phân biệt rõ ràng dựa trên database column
- SignalR Hub có debug endpoints

## 📊 Data Migration:

Existing records được update dựa trên pattern:
```sql
UPDATE [Notifications] 
SET [TargetRole] = CASE 
    WHEN [RedirectUrl] LIKE '%/admin/%' THEN 'admin'
    WHEN [RedirectUrl] LIKE '%/orders/%' AND [RedirectUrl] NOT LIKE '%/admin/%' THEN 'customer'
    WHEN [Type] = 'promotion' THEN 'customer'
    ELSE 'admin'
END
```

## 🧪 Testing Endpoints:

### SignalR Debug
- `GET /api/v1/SignalRDebug/hub-info`
- `GET /api/v1/SignalRDebug/test-hub`
- `POST /api/v1/SignalRDebug/test-admin` (Requires Admin)
- `POST /api/v1/SignalRDebug/test-customer/{id}` (Requires Admin)

### Admin Notifications  
- `GET /api/v1/notification` (Role: Admin)
- `GET /api/v1/notification/stats` (Role: Admin)

### Customer Notifications
- `GET /api/v1/customer-notifications` (Role: Customer)  
- `GET /api/v1/customer-notifications/stats` (Role: Customer)

## 🚀 Deployment Steps:

1. **Build & Test**:
   ```bash
   dotnet build
   dotnet test
   ```

2. **Run Migration**:
   ```bash
   powershell -ExecutionPolicy Bypass .\RunMigration.ps1
   ```

3. **Manual SQL** (if PowerShell fails):
   - Open SSMS
   - Execute `Migration_AddTargetRole.sql`

4. **Deploy Application**:
   ```bash
   dotnet run
   ```

5. **Verify**:
   - Check SignalR: `https://localhost:5264/api/v1/SignalRDebug/hub-info`
   - Test admin notifications
   - Test customer notifications

## 💡 Frontend Update Required:

SignalR Service URL should be:
```javascript
const hubUrl = `${baseUrl}/notificationHub`; // ✅ CORRECT
// NOT: `/hubs/notification` ❌ WRONG
```