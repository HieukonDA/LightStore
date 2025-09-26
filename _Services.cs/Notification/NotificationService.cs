using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TheLightStore.Dtos.Notifications;
using TheLightStore.Dtos.Paging;
using TheLightStore.Hubs;
using TheLightStore.Interfaces.Email;
using TheLightStore.Interfaces.Notifications;
using TheLightStore.Models.Notifications;
using TheLightStore.Models.Orders_Carts;
using TheLightStore.Services;

namespace TheLightStore.Services.Notifications;

public class NotificationService : INotificationService
{
    private readonly IEmailService _emailService;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly DBContext _context;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IEmailService emailService,
        IHubContext<NotificationHub> hubContext,
        DBContext context,
        ILogger<NotificationService> logger)
    {
        _emailService = emailService;
        _hubContext = hubContext;
        _context = context;
        _logger = logger;
    }

    public async Task NotifyOrderCreatedAsync(Order order, CancellationToken ct = default)
    {
        var subject = $"[TheLightStore] Xác nhận đơn hàng {order.OrderNumber}";
        var body = $@"
Xin chào {order.CustomerName},

Cảm ơn bạn đã đặt hàng tại TheLightStore!
Mã đơn hàng của bạn là: {order.OrderNumber}.
Tổng thanh toán: {order.TotalAmount:C}.

Chúng tôi sẽ xử lý đơn hàng và thông báo khi có cập nhật.

Trân trọng,
TheLightStore
";

        if (!string.IsNullOrEmpty(order.CustomerEmail))
        {
            await _emailService.SendEmailAsync(order.CustomerEmail, subject, body);
        }
    }

    public async Task NotifyOrderStatusChangedAsync(Order order, string oldStatus, string newStatus, CancellationToken ct = default)
    {
        var subject = $"[TheLightStore] Cập nhật đơn hàng {order.OrderNumber}";
        var body = $@"
Xin chào {order.CustomerName},

Trạng thái đơn hàng {order.OrderNumber} đã thay đổi:
- Từ: {oldStatus}
- Sang: {newStatus}

Trân trọng,
TheLightStore
";

        if (!string.IsNullOrEmpty(order.CustomerEmail))
        {
            await _emailService.SendEmailAsync(order.CustomerEmail, subject, body);
        }
    }

    #region Real-time Notifications

    public async Task CreateAndBroadcastNotificationAsync(CreateNotificationDto dto, CancellationToken ct = default)
    {
        try
        {
            // Lưu vào database
            var notification = new Notification
            {
                UserId = dto.UserId,
                TargetRole = dto.TargetRole, // ✅ SỬA: Thêm TargetRole
                Type = dto.Type,
                Title = dto.Title,
                Content = dto.Content,
                ReferenceId = dto.ReferenceId,
                RedirectUrl = dto.RedirectUrl,
                Priority = dto.Priority,
                ExpiresAt = dto.ExpiresAt,
                Metadata = dto.Metadata != null ? JsonSerializer.Serialize(dto.Metadata) : null
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync(ct);

            // Gửi real-time
            var realtimeDto = new RealTimeNotificationDto
            {
                Type = dto.Type,
                Title = dto.Title,
                Content = dto.Content,
                ReferenceId = dto.ReferenceId,
                RedirectUrl = dto.RedirectUrl,
                Priority = dto.Priority,
                Data = new { NotificationId = notification.Id }
            };

            // ✅ SỬA: Sử dụng TargetRole để phân biệt rõ ràng
            if (dto.UserId.HasValue)
            {
                if (dto.TargetRole == "customer")
                {
                    // Thông báo cho customer
                    await SendToCustomerAsync(dto.UserId.Value, realtimeDto, ct);
                }
                else if (dto.TargetRole == "admin")
                {
                    // Thông báo cho admin/staff cụ thể
                    await SendToUserAsync(dto.UserId.Value, realtimeDto, ct);
                }
                else
                {
                    // Default: tự động phán đoán dựa vào URL
                    if (dto.RedirectUrl?.Contains("/admin/") == true)
                    {
                        await SendToUserAsync(dto.UserId.Value, realtimeDto, ct);
                    }
                    else
                    {
                        await SendToCustomerAsync(dto.UserId.Value, realtimeDto, ct);
                    }
                }
            }
            else
            {
                switch (dto.TargetRole?.ToLower())
                {
                    case "customer":
                        await BroadcastToCustomersAsync(realtimeDto, ct);
                        break;
                    case "admin":
                        await BroadcastToAdminsAsync(realtimeDto, ct);
                        break;
                    case "all":
                        await BroadcastToCustomersAsync(realtimeDto, ct);
                        await BroadcastToAdminsAsync(realtimeDto, ct);
                        break;
                    default:
                        // ✅ Default chỉ gửi cho admin (không gửi cho customer)
                        await BroadcastToAdminsAsync(realtimeDto, ct);
                        break;
                }
            }

            _logger.LogInformation($"Notification created and broadcast: {dto.Title}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error creating notification: {dto.Title}");
            throw;
        }
    }

    public async Task BroadcastToAdminsAsync(RealTimeNotificationDto notification, CancellationToken ct = default)
    {
        try
        {
            await _hubContext.Clients.Group("AdminGroup")
                .SendAsync("ReceiveNotification", notification, ct);
            
            _logger.LogInformation($"Notification broadcast to admins: {notification.Title}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error broadcasting to admins: {notification.Title}");
        }
    }

    public async Task SendToUserAsync(int userId, RealTimeNotificationDto notification, CancellationToken ct = default)
    {
        try
        {
            await _hubContext.Clients.Group($"User_{userId}")
                .SendAsync("ReceiveNotification", notification, ct);
            
            _logger.LogInformation($"Notification sent to user {userId}: {notification.Title}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending to user {userId}: {notification.Title}");
        }
    }

    public async Task SendToCustomerAsync(int customerId, RealTimeNotificationDto notification, CancellationToken ct = default)
    {
        try
        {
            await _hubContext.Clients.Group($"Customer_{customerId}")
                .SendAsync("ReceiveNotification", notification, ct);
            
            _logger.LogInformation($"Notification sent to customer {customerId}: {notification.Title}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending to customer {customerId}: {notification.Title}");
        }
    }

    public async Task BroadcastToCustomersAsync(RealTimeNotificationDto notification, CancellationToken ct = default)
    {
        try
        {
            await _hubContext.Clients.Group("CustomersGroup")
                .SendAsync("ReceiveNotification", notification, ct);
            
            _logger.LogInformation($"Notification broadcast to all customers: {notification.Title}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error broadcasting to customers: {notification.Title}");
        }
    }

    #endregion 

    #region Database Notifications

    public async Task<ServiceResult<PagedResult<NotificationDto>>> GetUserNotificationsAsync(int userId, PagedRequest request, string userRole = "admin", CancellationToken ct = default)
    {
        try
        {
            var query = _context.Notifications
                .Where(n => (n.UserId == userId || n.UserId == null)) // User-specific hoặc broadcast
                .Where(n => n.TargetRole == userRole || n.TargetRole == "all") // ✅ Filter theo TargetRole
                .Where(n => n.ExpiresAt == null || n.ExpiresAt > DateTime.UtcNow) // Chưa hết hạn
                .OrderByDescending(n => n.Priority == "urgent" ? 4 : n.Priority == "high" ? 3 : n.Priority == "normal" ? 2 : 1)
                .ThenByDescending(n => n.CreatedAt);

            var totalCount = await query.CountAsync(ct);
            
            var notifications = await query
                .Skip((request.Page - 1) * request.Size)
                .Take(request.Size)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    TargetRole = n.TargetRole, // ✅ Thêm TargetRole
                    Type = n.Type,
                    Title = n.Title,
                    Content = n.Content,
                    ReferenceId = n.ReferenceId,
                    RedirectUrl = n.RedirectUrl,
                    IsRead = n.IsRead,
                    Priority = n.Priority,
                    CreatedAt = n.CreatedAt,
                    ReadAt = n.ReadAt,
                    Metadata = n.Metadata
                })
                .ToListAsync(ct);

            var pagedResult = new PagedResult<NotificationDto>
            {
                Items = notifications,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.Size
            };

            return ServiceResult<PagedResult<NotificationDto>>.SuccessResult(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting notifications for user {userId}");
            return ServiceResult<PagedResult<NotificationDto>>.FailureResult("Lỗi khi lấy danh sách thông báo", new List<string> { ex.Message });
        }
    }

    public async Task<ServiceResult<NotificationStatsDto>> GetUserNotificationStatsAsync(int userId, string userRole = "admin", CancellationToken ct = default)
    {
        try
        {
            var today = DateTime.UtcNow.Date;
            var query = _context.Notifications
                .Where(n => (n.UserId == userId || n.UserId == null))
                .Where(n => n.TargetRole == userRole || n.TargetRole == "all") // ✅ Filter theo TargetRole
                .Where(n => n.ExpiresAt == null || n.ExpiresAt > DateTime.UtcNow);

            var stats = new NotificationStatsDto
            {
                TotalCount = await query.CountAsync(ct),
                UnreadCount = await query.Where(n => !n.IsRead).CountAsync(ct),
                TodayCount = await query.Where(n => n.CreatedAt >= today).CountAsync(ct),
                HighPriorityCount = await query.Where(n => n.Priority == "high" || n.Priority == "urgent").CountAsync(ct)
            };

            return ServiceResult<NotificationStatsDto>.SuccessResult(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting notification stats for user {userId}");
            return ServiceResult<NotificationStatsDto>.FailureResult("Lỗi khi lấy thống kê thông báo", new List<string> { ex.Message });
        }
    }

    public async Task<ServiceResult<bool>> MarkAsReadAsync(int userId, MarkAsReadDto dto, CancellationToken ct = default)
    {
        try
        {
            var notifications = await _context.Notifications
                .Where(n => dto.NotificationIds.Contains(n.Id))
                .Where(n => n.UserId == userId || n.UserId == null)
                .ToListAsync(ct);

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync(ct);
            
            _logger.LogInformation($"Marked {notifications.Count} notifications as read for user {userId}");
            return ServiceResult<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error marking notifications as read for user {userId}");
            return ServiceResult<bool>.FailureResult("Lỗi khi đánh dấu đã đọc thông báo", new List<string> { ex.Message });
        }
    }

    public async Task<ServiceResult<bool>> DeleteNotificationAsync(int userId, int notificationId, CancellationToken ct = default)
    {
        try
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && (n.UserId == userId || n.UserId == null), ct);

            if (notification == null)
            {
                return ServiceResult<bool>.FailureResult("Không tìm thấy thông báo", new List<string>());
            }

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync(ct);
            
            return ServiceResult<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting notification {notificationId} for user {userId}");
            return ServiceResult<bool>.FailureResult("Lỗi khi xóa thông báo", new List<string> { ex.Message });
        }
    }

    #endregion

    #region Business Logic Notifications

    public async Task NotifyNewOrderAsync(Order order, CancellationToken ct = default)
    {
        // Gửi email cho khách hàng
        await NotifyOrderCreatedAsync(order, ct);

        // Gửi thông báo real-time cho admin
        var notification = new CreateNotificationDto
        {
            UserId = null,
            TargetRole = "admin", // ✅ Rõ ràng đây là thông báo cho admin
            Type = "order",
            Title = "Đơn hàng mới",
            Content = $"Khách hàng {order.CustomerName} vừa đặt đơn hàng #{order.OrderNumber} với giá trị {order.TotalAmount:C}",
            ReferenceId = order.Id,
            RedirectUrl = $"/admin/orders/{order.Id}",
            Priority = "normal"
        };

        await CreateAndBroadcastNotificationAsync(notification, ct);
    }

    public async Task NotifyOrderUpdateAsync(Order order, string oldStatus, string newStatus, CancellationToken ct = default)
    {
        // Gửi email cho khách hàng
        await NotifyOrderStatusChangedAsync(order, oldStatus, newStatus, ct);

        // Gửi thông báo real-time cho admin
        var notification = new CreateNotificationDto
        {
            TargetRole = "admin", // ✅ Rõ ràng đây là thông báo cho admin
            Type = "order",
            Title = "Cập nhật đơn hàng",
            Content = $"Đơn hàng #{order.OrderNumber} đã chuyển từ '{oldStatus}' sang '{newStatus}'",
            ReferenceId = order.Id,
            RedirectUrl = $"/admin/orders/{order.Id}",
            Priority = newStatus.ToLower() == "cancelled" ? "high" : "normal"
        };

        await CreateAndBroadcastNotificationAsync(notification, ct);
    }

    public async Task NotifyPaymentSuccessAsync(Order order, CancellationToken ct = default)
    {
        var notification = new CreateNotificationDto
        {
            TargetRole = "admin", // ✅ Rõ ràng đây là thông báo cho admin
            Type = "payment",
            Title = "Thanh toán thành công",
            Content = $"Đơn hàng #{order.OrderNumber} đã được thanh toán thành công số tiền {order.TotalAmount:C}",
            ReferenceId = order.Id,
            RedirectUrl = $"/admin/orders/{order.Id}",
            Priority = "high"
        };

        await CreateAndBroadcastNotificationAsync(notification, ct);
    }

    public async Task NotifyLowStockAsync(int productId, string productName, int currentStock, CancellationToken ct = default)
    {
        var notification = new CreateNotificationDto
        {
            TargetRole = "admin", // ✅ Rõ ràng đây là thông báo cho admin
            Type = "inventory",
            Title = "Cảnh báo hết hàng",
            Content = $"Sản phẩm '{productName}' chỉ còn {currentStock} sản phẩm trong kho",
            ReferenceId = productId,
            RedirectUrl = $"/admin/products/{productId}",
            Priority = currentStock == 0 ? "urgent" : "high"
        };

        await CreateAndBroadcastNotificationAsync(notification, ct);
    }

    #endregion

    #region Customer Notifications

    public async Task NotifyCustomerOrderStatusAsync(int customerId, Order order, string newStatus, CancellationToken ct = default)
    {
        try
        {
            // Tạo thông báo trong database cho customer
            var dbNotification = new CreateNotificationDto
            {
                UserId = customerId,
                TargetRole = "customer", // ✅ Rõ ràng đây là thông báo cho customer
                Type = "order",
                Title = GetOrderStatusTitle(newStatus),
                Content = GetOrderStatusContent(order, newStatus),
                ReferenceId = order.Id,
                RedirectUrl = $"/orders/{order.Id}",
                Priority = GetOrderStatusPriority(newStatus)
            };
            
            await CreateAndBroadcastNotificationAsync(dbNotification, ct);

            // Gửi real-time notification cho customer
            var realtimeNotification = new RealTimeNotificationDto
            {
                Type = "order",
                Title = GetOrderStatusTitle(newStatus),
                Content = GetOrderStatusContent(order, newStatus),
                ReferenceId = order.Id,
                RedirectUrl = $"/orders/{order.Id}",
                Priority = GetOrderStatusPriority(newStatus),
                Data = new { 
                    OrderId = order.Id,
                    OrderNumber = order.OrderNumber,
                    NewStatus = newStatus,
                    TotalAmount = order.TotalAmount
                }
            };

            await SendToCustomerAsync(customerId, realtimeNotification, ct);
            
            _logger.LogInformation($"Customer {customerId} notified about order {order.OrderNumber} status change to {newStatus}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error notifying customer {customerId} about order status change");
        }
    }

    public async Task NotifyCustomerPaymentAsync(int customerId, Order order, bool success, string? paymentMethod = null, CancellationToken ct = default)
    {
        try
        {
            var title = success ? "Thanh toán thành công" : "Thanh toán thất bại";
            var content = success 
                ? $"Thanh toán cho đơn hàng #{order.OrderNumber} đã thành công. Số tiền: {order.TotalAmount:C}"
                : $"Thanh toán cho đơn hàng #{order.OrderNumber} thất bại. Vui lòng thử lại.";
            
            if (!string.IsNullOrEmpty(paymentMethod))
            {
                content += $" Phương thức: {paymentMethod}";
            }

            // Tạo thông báo trong database
            var dbNotification = new CreateNotificationDto
            {
                UserId = customerId,
                TargetRole = "customer", // ✅ Rõ ràng đây là thông báo cho customer
                Type = "payment",
                Title = title,
                Content = content,
                ReferenceId = order.Id,
                RedirectUrl = $"/orders/{order.Id}",
                Priority = success ? "normal" : "high"
            };
            
            await CreateAndBroadcastNotificationAsync(dbNotification, ct);

            // Gửi real-time notification
            var realtimeNotification = new RealTimeNotificationDto
            {
                Type = "payment",
                Title = title,
                Content = content,
                ReferenceId = order.Id,
                RedirectUrl = $"/orders/{order.Id}",
                Priority = success ? "normal" : "high",
                Data = new { 
                    OrderId = order.Id,
                    OrderNumber = order.OrderNumber,
                    Success = success,
                    PaymentMethod = paymentMethod,
                    Amount = order.TotalAmount
                }
            };

            await SendToCustomerAsync(customerId, realtimeNotification, ct);
            
            _logger.LogInformation($"Customer {customerId} notified about payment {(success ? "success" : "failure")} for order {order.OrderNumber}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error notifying customer {customerId} about payment status");
        }
    }

    public async Task NotifyCustomerPromotionAsync(int customerId, string title, string content, string? redirectUrl = null, CancellationToken ct = default)
    {
        try
        {
            // Tạo thông báo trong database
            var dbNotification = new CreateNotificationDto
            {
                UserId = customerId,
                TargetRole = "customer", // ✅ Rõ ràng đây là thông báo cho customer
                Type = "promotion",
                Title = title,
                Content = content,
                RedirectUrl = redirectUrl ?? "/promotions",
                Priority = "normal"
            };
            
            await CreateAndBroadcastNotificationAsync(dbNotification, ct);

            // Gửi real-time notification
            var realtimeNotification = new RealTimeNotificationDto
            {
                Type = "promotion",
                Title = title,
                Content = content,
                RedirectUrl = redirectUrl ?? "/promotions",
                Priority = "normal"
            };

            await SendToCustomerAsync(customerId, realtimeNotification, ct);
            
            _logger.LogInformation($"Customer {customerId} notified about promotion: {title}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error notifying customer {customerId} about promotion");
        }
    }

    public async Task BroadcastPromotionAsync(string title, string content, string? redirectUrl = null, CancellationToken ct = default)
    {
        try
        {
            // Tạo thông báo broadcast trong database (UserId = null)
            var dbNotification = new CreateNotificationDto
            {
                UserId = null, // Broadcast to all
                TargetRole = "customer", // ✅ Broadcast cho tất cả customers
                Type = "promotion",
                Title = title,
                Content = content,
                RedirectUrl = redirectUrl ?? "/promotions",
                Priority = "normal"
            };
            
            await CreateAndBroadcastNotificationAsync(dbNotification, ct);

            // Gửi real-time notification cho tất cả customers
            var realtimeNotification = new RealTimeNotificationDto
            {
                Type = "promotion",
                Title = title,
                Content = content,
                RedirectUrl = redirectUrl ?? "/promotions",
                Priority = "normal"
            };

            await BroadcastToCustomersAsync(realtimeNotification, ct);
            
            _logger.LogInformation($"Promotion broadcast to all customers: {title}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting promotion to customers");
        }
    }

    #endregion

    #region Helper Methods

    private string GetOrderStatusTitle(string status)
    {
        return status.ToLower() switch
        {
            "confirmed" => "Đơn hàng đã được xác nhận",
            "processing" => "Đơn hàng đang được xử lý",
            "shipped" => "Đơn hàng đã được gửi đi",
            "delivered" => "Đơn hàng đã được giao",
            "cancelled" => "Đơn hàng đã bị hủy",
            _ => "Cập nhật đơn hàng"
        };
    }

    private string GetOrderStatusContent(Order order, string status)
    {
        return status.ToLower() switch
        {
            "confirmed" => $"Đơn hàng #{order.OrderNumber} đã được xác nhận và đang được chuẩn bị.",
            "processing" => $"Đơn hàng #{order.OrderNumber} đang được xử lý. Chúng tôi sẽ sớm gửi hàng cho bạn.",
            "shipped" => $"Đơn hàng #{order.OrderNumber} đã được gửi đi và đang trên đường đến bạn.",
            "delivered" => $"Đơn hàng #{order.OrderNumber} đã được giao thành công. Cảm ơn bạn đã mua sắm!",
            "cancelled" => $"Đơn hàng #{order.OrderNumber} đã bị hủy. Nếu có thắc mắc, vui lòng liên hệ hỗ trợ.",
            _ => $"Đơn hàng #{order.OrderNumber} đã được cập nhật trạng thái: {status}"
        };
    }

    private string GetOrderStatusPriority(string status)
    {
        return status.ToLower() switch
        {
            "cancelled" => "high",
            "delivered" => "high",
            "shipped" => "normal",
            _ => "normal"
        };
    }

    #endregion
}
