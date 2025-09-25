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

            if (dto.UserId.HasValue)
            {
                await SendToUserAsync(dto.UserId.Value, realtimeDto, ct);
            }
            else
            {
                await BroadcastToAdminsAsync(realtimeDto, ct);
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

    #endregion

    #region Database Notifications

    public async Task<ServiceResult<PagedResult<NotificationDto>>> GetUserNotificationsAsync(int userId, PagedRequest request, CancellationToken ct = default)
    {
        try
        {
            var query = _context.Notifications
                .Where(n => n.UserId == userId || n.UserId == null) // User-specific hoặc broadcast
                .Where(n => n.ExpiresAt == null || n.ExpiresAt > DateTime.UtcNow) // Chưa hết hạn
                .OrderByDescending(n => n.CreatedAt);

            var totalCount = await query.CountAsync(ct);
            
            var notifications = await query
                .Skip((request.Page - 1) * request.Size)
                .Take(request.Size)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
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

    public async Task<ServiceResult<NotificationStatsDto>> GetUserNotificationStatsAsync(int userId, CancellationToken ct = default)
    {
        try
        {
            var today = DateTime.UtcNow.Date;
            var query = _context.Notifications
                .Where(n => n.UserId == userId || n.UserId == null)
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
}
