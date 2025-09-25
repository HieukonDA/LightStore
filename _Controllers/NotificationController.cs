using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TheLightStore.Dtos.Notifications;
using TheLightStore.Interfaces.Notifications;

namespace TheLightStore.Controllers.Notifications;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize] // Yêu cầu authentication
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationController> _logger;

    public NotificationController(INotificationService notificationService, ILogger<NotificationController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Lấy danh sách thông báo của user hiện tại
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetNotifications([FromQuery] PagedRequest request)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var result = await _notificationService.GetUserNotificationsAsync(userId.Value, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Lấy thống kê thông báo của user hiện tại
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetNotificationStats()
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var result = await _notificationService.GetUserNotificationStatsAsync(userId.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Đánh dấu thông báo đã đọc
    /// </summary>
    [HttpPut("mark-as-read")]
    public async Task<IActionResult> MarkAsRead([FromBody] MarkAsReadDto dto)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var result = await _notificationService.MarkAsReadAsync(userId.Value, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Xóa thông báo
    /// </summary>
    [HttpDelete("{notificationId}")]
    public async Task<IActionResult> DeleteNotification(int notificationId)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var result = await _notificationService.DeleteNotificationAsync(userId.Value, notificationId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Tạo thông báo mới (chỉ Admin)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationDto dto)
    {
        await _notificationService.CreateAndBroadcastNotificationAsync(dto);
        return Ok(new { message = "Thông báo đã được tạo và gửi thành công" });
    }

    /// <summary>
    /// Test gửi thông báo (chỉ Admin)
    /// </summary>
    [HttpPost("test")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> TestNotification([FromBody] RealTimeNotificationDto dto)
    {
        await _notificationService.BroadcastToAdminsAsync(dto);
        return Ok(new { message = "Test notification sent successfully" });
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}