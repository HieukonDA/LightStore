using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TheLightStore.Dtos.Notifications;
using TheLightStore.Dtos.Paging;
using TheLightStore.Interfaces.Notifications;

namespace TheLightStore.Controllers.Customers;

/// <summary>
/// API Controller cho thông báo của customers
/// </summary>
[ApiController]
[Route("api/v1/customer/notifications")]
[Authorize(Roles = "Customer")] // Chỉ customer mới được truy cập
public class CustomerNotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<CustomerNotificationController> _logger;

    public CustomerNotificationController(
        INotificationService notificationService,
        ILogger<CustomerNotificationController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Lấy danh sách thông báo của customer hiện tại
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMyNotifications([FromQuery] PagedRequest request)
    {
        var customerId = GetCurrentUserId();
        if (!customerId.HasValue)
        {
            return Unauthorized(new { message = "Customer not found" });
        }

        // ✅ Customer chỉ nhận thông báo customer
        var result = await _notificationService.GetUserNotificationsAsync(customerId.Value, request, "customer");
        
        if (result.Success)
        {
            return Ok(new
            {
                success = true,
                data = result.Data,
                message = "Notifications retrieved successfully"
            });
        }

        return BadRequest(new
        {
            success = false,
            message = result.Message,
            errors = result.Errors
        });
    }

    /// <summary>
    /// Lấy thống kê thông báo
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetMyStats()
    {
        var customerId = GetCurrentUserId();
        if (!customerId.HasValue)
        {
            return Unauthorized(new { message = "Customer not found" });
        }

        // ✅ Customer chỉ nhận thống kê thông báo customer
        var result = await _notificationService.GetUserNotificationStatsAsync(customerId.Value, "customer");
        
        if (result.Success)
        {
            return Ok(new
            {
                success = true,
                data = result.Data,
                message = "Stats retrieved successfully"
            });
        }

        return BadRequest(new
        {
            success = false,
            message = result.Message,
            errors = result.Errors
        });
    }

    /// <summary>
    /// Đánh dấu thông báo đã đọc
    /// </summary>
    [HttpPut("{notificationId}/read")]
    public async Task<IActionResult> MarkAsRead(int notificationId)
    {
        var customerId = GetCurrentUserId();
        if (!customerId.HasValue)
        {
            return Unauthorized(new { message = "Customer not found" });
        }

        var dto = new MarkAsReadDto
        {
            NotificationIds = new List<int> { notificationId }
        };

        var result = await _notificationService.MarkAsReadAsync(customerId.Value, dto);
        
        if (result.Success)
        {
            return Ok(new
            {
                success = true,
                message = "Notification marked as read successfully"
            });
        }

        return BadRequest(new
        {
            success = false,
            message = result.Message,
            errors = result.Errors
        });
    }

    /// <summary>
    /// Đánh dấu tất cả thông báo đã đọc
    /// </summary>
    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var customerId = GetCurrentUserId();
        if (!customerId.HasValue)
        {
            return Unauthorized(new { message = "Customer not found" });
        }

        // Get all unread notification IDs
        var notificationsResult = await _notificationService.GetUserNotificationsAsync(
            customerId.Value, 
            new PagedRequest { Page = 1, Size = 1000 }, 
            "customer");

        if (!notificationsResult.Success || notificationsResult.Data?.Items
 == null)
        {
            return BadRequest(new { message = "Failed to retrieve notifications" });
        }

        var unreadIds = notificationsResult.Data.Items

            .Where(n => !n.IsRead)
            .Select(n => n.Id)
            .ToList();

        if (!unreadIds.Any())
        {
            return Ok(new
            {
                success = true,
                message = "No unread notifications to mark"
            });
        }

        var dto = new MarkAsReadDto { NotificationIds = unreadIds };
        var result = await _notificationService.MarkAsReadAsync(customerId.Value, dto);
        
        if (result.Success)
        {
            return Ok(new
            {
                success = true,
                message = $"Marked {unreadIds.Count} notifications as read"
            });
        }

        return BadRequest(new
        {
            success = false,
            message = result.Message,
            errors = result.Errors
        });
    }

    /// <summary>
    /// Xóa thông báo
    /// </summary>
    [HttpDelete("{notificationId}")]
    public async Task<IActionResult> DeleteNotification(int notificationId)
    {
        var customerId = GetCurrentUserId();
        if (!customerId.HasValue)
        {
            return Unauthorized(new { message = "Customer not found" });
        }

        var result = await _notificationService.DeleteNotificationAsync(customerId.Value, notificationId);
        
        if (result.Success)
        {
            return Ok(new
            {
                success = true,
                message = "Notification deleted successfully"
            });
        }

        return BadRequest(new
        {
            success = false,
            message = result.Message,
            errors = result.Errors
        });
    }

    /// <summary>
    /// Lấy thống kê tổng hợp cho customer
    /// </summary>
    [HttpGet("dashboard-stats")]
    public async Task<IActionResult> GetDashboardStats()
    {
        var customerId = GetCurrentUserId();
        if (!customerId.HasValue)
        {
            return Unauthorized(new { message = "Customer not found" });
        }

        var result = await _notificationService.GetUserNotificationStatsAsync(customerId.Value, "customer");
        
        if (result.Success)
        {
            return Ok(new
            {
                success = true,
                data = new
                {
                    notifications = result.Data,
                    customerId = customerId.Value,
                    timestamp = DateTime.UtcNow
                },
                message = "Dashboard stats retrieved successfully"
            });
        }

        return BadRequest(new
        {
            success = false,
            message = result.Message,
            errors = result.Errors
        });
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                         ?? User.FindFirst("sub")?.Value
                         ?? User.FindFirst("id")?.Value;
        
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}