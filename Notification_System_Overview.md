# Hệ thống thông báo Real-time (SignalR) - LightStore

## Tổng quan

Hệ thống thông báo real-time của LightStore sử dụng SignalR để cung cấp thông báo trực tiếp cho cả Admin và Customer. Hệ thống hỗ trợ:

- **Admin Notifications**: Thông báo về đơn hàng mới, thanh toán, tồn kho
- **Customer Notifications**: Thông báo về trạng thái đơn hàng, thanh toán, khuyến mãi
- **Database Storage**: Lưu trữ thông báo để xem lại sau
- **Real-time Delivery**: Gửi thông báo ngay lập tức qua WebSocket

## Kiến trúc hệ thống

```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   Frontend      │    │   SignalR Hub    │    │   Backend API   │
│                 │◄──►│ NotificationHub  │◄──►│ Services        │
│ - Admin Panel   │    │                  │    │ - OrderService  │
│ - Customer App  │    │ Groups:          │    │ - PaymentService│
│ - Notifications │    │ • AdminGroup     │    │ - NotifyService │
└─────────────────┘    │ • CustomersGroup │    └─────────────────┘
                       │ • User_{userId}  │              │
                       │ • Customer_{id}  │              ▼
                       └──────────────────┘    ┌─────────────────┐
                                              │   Database      │
                                              │ - Notifications │
                                              │ - Orders        │
                                              │ - Users         │
                                              └─────────────────┘
```

## Các thành phần chính

### 1. NotificationHub (SignalR Hub)
- **File**: `_Hubs/NotificationHub.cs`
- **Endpoint**: `/notificationHub`
- **Authentication**: JWT Required
- **Groups**: 
  - `AdminGroup`: Tất cả Admin/Staff
  - `CustomersGroup`: Tất cả Customers
  - `User_{userId}`: Specific user
  - `Customer_{customerId}`: Specific customer

### 2. NotificationService
- **File**: `_Services.cs/Notification/NotificationService.cs`
- **Chức năng**:
  - Email notifications
  - Real-time SignalR notifications
  - Database storage
  - Business logic integration

### 3. Controllers
- **Admin**: `_Controllers/NotificationController.cs`
- **Customer**: `_Controllers/CustomerNotificationController.cs`

### 4. Database
- **Table**: `Notifications`
- **Purpose**: Persistent storage cho thông báo

## Workflow thông báo

### Admin Notifications
```
Order Created → NotificationService.NotifyNewOrderAsync() 
             → Save to DB + SignalR to AdminGroup
             
Payment Success → NotificationService.NotifyPaymentSuccessAsync()
                → Save to DB + SignalR to AdminGroup
                
Low Stock → NotificationService.NotifyLowStockAsync()
          → Save to DB + SignalR to AdminGroup
```

### Customer Notifications  
```
Order Status Change → NotificationService.NotifyCustomerOrderStatusAsync()
                   → Save to DB + SignalR to Customer_{id}
                   
Payment Result → NotificationService.NotifyCustomerPaymentAsync()
              → Save to DB + SignalR to Customer_{id}
              
Promotion → NotificationService.BroadcastPromotionAsync()
         → Save to DB + SignalR to CustomersGroup
```

## API Endpoints

### Admin Endpoints (`/api/v1/notification`)
- `GET /` - Lấy danh sách thông báo
- `GET /stats` - Thống kê thông báo
- `PUT /mark-as-read` - Đánh dấu đã đọc
- `DELETE /{id}` - Xóa thông báo
- `POST /` - Tạo thông báo mới (Admin only)
- `POST /test` - Test broadcast (Admin only)

### Customer Endpoints (`/api/v1/customer/notifications`)
- `GET /` - Lấy thông báo của customer
- `GET /stats` - Thống kê thông báo
- `GET /unread-count` - Số lượng chưa đọc
- `PUT /mark-as-read` - Đánh dấu đã đọc
- `PUT /mark-all-as-read` - Đánh dấu tất cả đã đọc
- `DELETE /{id}` - Xóa thông báo

## SignalR Events

### Hub Methods (Client → Server)
```javascript
// Authentication & Connection
connection.start()

// Group Management
connection.invoke("JoinAdminGroup")
connection.invoke("JoinCustomerGroup") 
connection.invoke("LeaveAdminGroup")
connection.invoke("LeaveCustomerGroup")

// Utilities
connection.invoke("Ping")
connection.invoke("AcknowledgeNotification", notificationId)
```

### Server Events (Server → Client)
```javascript
// Nhận thông báo
connection.on("ReceiveNotification", function(notification) {
    // notification: RealTimeNotificationDto
})

// Acknowledgment
connection.on("NotificationAcknowledged", function(notificationId) {
    // Xác nhận đã nhận
})

// Utilities  
connection.on("Pong", function(timestamp) {
    // Response cho Ping
})
```

## Notification Types

### Admin Notification Types
- `"order"` - Đơn hàng mới/cập nhật
- `"payment"` - Thanh toán thành công
- `"inventory"` - Cảnh báo tồn kho
- `"system"` - Thông báo hệ thống

### Customer Notification Types
- `"order"` - Cập nhật đơn hàng
- `"payment"` - Kết quả thanh toán
- `"promotion"` - Khuyến mãi
- `"system"` - Thông báo hệ thống

## Priority Levels
- `"low"` - Thấp
- `"normal"` - Bình thường (mặc định)
- `"high"` - Cao
- `"urgent"` - Khẩn cấp

## Data Models

### RealTimeNotificationDto
```csharp
public class RealTimeNotificationDto
{
    public string Type { get; set; }           // "order", "payment", etc.
    public string Title { get; set; }          // Tiêu đề
    public string Content { get; set; }        // Nội dung
    public int? ReferenceId { get; set; }      // ID tham chiếu
    public string? RedirectUrl { get; set; }   // URL chuyển hướng
    public string Priority { get; set; }       // "normal", "high", etc.
    public DateTime Timestamp { get; set; }    // Thời gian
    public object? Data { get; set; }          // Dữ liệu bổ sung
}
```

### NotificationDto (Database)
```csharp
public class NotificationDto
{
    public int Id { get; set; }
    public string Type { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public int? ReferenceId { get; set; }
    public string? RedirectUrl { get; set; }
    public bool IsRead { get; set; }
    public string Priority { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public string? Metadata { get; set; }
}
```

## Database Schema

```sql
CREATE TABLE [Notifications] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [UserId] INT NULL,                    -- NULL = broadcast
    [Type] NVARCHAR(50) NOT NULL,         
    [Title] NVARCHAR(255) NOT NULL,
    [Content] NVARCHAR(MAX) NOT NULL,
    [ReferenceId] INT NULL,
    [RedirectUrl] NVARCHAR(500) NULL,
    [IsRead] BIT DEFAULT 0,
    [Priority] NVARCHAR(20) DEFAULT 'normal',
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [ReadAt] DATETIME2 NULL,
    [ExpiresAt] DATETIME2 NULL,
    [Metadata] NVARCHAR(MAX) NULL,
    
    CONSTRAINT [FK_Notifications_Users] 
        FOREIGN KEY ([UserId]) REFERENCES [Users]([Id])
);
```

## Configuration

### appsettings.json
```json
{
  "Jwt": {
    "Key": "your-secret-key",
    "Issuer": "LightStore",
    "Audience": "LightStore-Users"
  },
  "ConnectionStrings": {
    "DefaultConnection": "your-sql-connection"
  }
}
```

### CORS Settings
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Required for SignalR
    });
});
```

## Troubleshooting

### Common Issues

1. **Connection Failed**
   - Kiểm tra JWT token
   - Verify CORS settings
   - Check endpoint URL

2. **Not Receiving Notifications**
   - Verify user roles
   - Check group membership
   - Confirm SignalR connection

3. **Database Errors**
   - Run migration script
   - Check foreign key constraints
   - Verify connection string

### Debug Tips

1. **Enable SignalR Logging**
   ```csharp
   builder.Services.AddSignalR(options =>
   {
       options.EnableDetailedErrors = true;
   });
   ```

2. **Frontend Debug**
   ```javascript
   connection.configureLogging(signalR.LogLevel.Debug);
   ```

3. **Check Connection State**
   ```javascript
   console.log("Connection state:", connection.state);
   ```

## Security Considerations

1. **Authentication**: JWT required for all connections
2. **Authorization**: Role-based access control
3. **Data Validation**: All inputs validated
4. **Rate Limiting**: Prevent spam/abuse
5. **HTTPS**: Required in production

## Performance Optimization

1. **Connection Pooling**: SignalR handles automatically
2. **Message Compression**: Enable in production
3. **Heartbeat**: Configure keep-alive
4. **Scaling**: Use Redis backplane for multiple servers

## Monitoring & Logging

1. **Connection Metrics**: Track active connections
2. **Message Delivery**: Log successful/failed deliveries
3. **Performance**: Monitor response times
4. **Errors**: Comprehensive error logging

## Next Steps

1. Implement frontend components
2. Add push notifications (mobile)
3. Email fallback for offline users
4. Advanced notification preferences
5. Notification analytics dashboard