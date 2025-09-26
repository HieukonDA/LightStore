# Mở rộng hệ thống thông báo cho Customers

## Hiện tại đã có:
✅ Email notifications cho customers
✅ Real-time notifications cho Admin/Staff
✅ Database lưu trữ notifications

## Cần mở rộng thêm: 

### 1. Customer Real-time Notifications (SignalR)
```csharp
// Trong NotificationHub.cs
public override async Task OnConnectedAsync()
{
    var userId = GetCurrentUserId();
    var userRoles = GetCurrentUserRoles();
    
    // Join admin group nếu là admin/staff
    if (userRoles.Contains("Admin") || userRoles.Contains("Staff"))
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "AdminGroup");
    }
    
    // Join customer group nếu là customer
    if (userRoles.Contains("Customer") && userId.HasValue)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Customer_{userId.Value}");
    }
}

// Method gửi thông báo cho customer cụ thể
public async Task SendToCustomerAsync(int customerId, RealTimeNotificationDto notification)
{
    await _hubContext.Clients.Group($"Customer_{customerId}")
        .SendAsync("ReceiveNotification", notification);
}
```

### 2. Customer Notification Types
```csharp
public enum CustomerNotificationType
{
    OrderConfirmed,      // Đơn hàng đã được xác nhận
    OrderShipped,        // Đơn hàng đang giao
    OrderDelivered,      // Đơn hàng đã giao
    OrderCancelled,      // Đơn hàng bị hủy
    PaymentSuccess,      // Thanh toán thành công
    PaymentFailed,       // Thanh toán thất bại
    ProductBackInStock,  // Sản phẩm đã có hàng trở lại
    Promotion           // Khuyến mãi mới
}
```

### 3. Customer Notification Service Methods
```csharp
// Thêm vào INotificationService
Task NotifyCustomerOrderStatusAsync(int customerId, Order order, string newStatus);
Task NotifyCustomerPaymentAsync(int customerId, Order order, bool success);
Task NotifyCustomerPromotionAsync(int customerId, string title, string content);
Task BroadcastPromotionAsync(string title, string content); // Gửi cho tất cả customers
```

### 4. Customer Notification API Endpoints
```csharp
[Route("api/v1/customer/notifications")]
public class CustomerNotificationController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetMyNotifications([FromQuery] PagedRequest request);
    
    [HttpPut("mark-as-read")]
    public async Task<IActionResult> MarkAsRead([FromBody] MarkAsReadDto dto);
    
    [HttpGet("stats")]
    public async Task<IActionResult> GetNotificationStats();
}
```

### 5. Database Schema Update
```sql
-- Thêm customer notifications vào bảng Notifications
ALTER TABLE [Notifications] 
ADD CONSTRAINT [CK_Notifications_Type_Extended] 
CHECK ([Type] IN ('order', 'payment', 'inventory', 'system', 'user', 'customer', 'promotion'));

-- Thêm index cho customer notifications
CREATE INDEX [IX_Notifications_Customer] 
ON [Notifications] ([UserId], [Type], [CreatedAt] DESC)
WHERE [Type] IN ('customer', 'promotion');
```

### 6. Frontend Customer Notification
```javascript
// Customer notification connection
const customerConnection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub", {
        accessTokenFactory: () => localStorage.getItem("customerToken")
    })
    .build();

// Listen for customer notifications
customerConnection.on("ReceiveNotification", function(notification) {
    if (notification.type === "order") {
        showOrderNotification(notification);
    } else if (notification.type === "promotion") {
        showPromotionNotification(notification);
    }
});
```

## Workflow thông báo Customer:

### Đơn hàng mới:
1. Customer đặt hàng → Email xác nhận (✅ Đã có)
2. Admin xác nhận → Email + Real-time notification cho customer (🆕 Cần thêm)

### Trạng thái đơn hàng:
1. Admin cập nhật trạng thái → Email + Real-time notification (🆕 Cần thêm)
2. Đơn hàng giao thành công → Email + Real-time notification (🆕 Cần thêm)

### Thanh toán:
1. Thanh toán thành công → Email + Real-time notification (🆕 Cần thêm)
2. Thanh toán thất bại → Email + Real-time notification (🆕 Cần thêm)

### Khuyến mãi:
1. Admin tạo khuyến mãi → Broadcast cho tất cả customers (🆕 Cần thêm)
2. Sản phẩm yêu thích có khuyến mãi → Notification cá nhân (🆕 Cần thêm)