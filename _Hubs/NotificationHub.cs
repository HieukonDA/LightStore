using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace TheLightStore.Hubs;

[Authorize] // Yêu cầu authentication
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userRoles = Context.User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        
        _logger.LogInformation($"Client connected: {Context.ConnectionId}, User: {userId}");
        
        // Tự động join admin group nếu là admin/staff
        if (userRoles != null && (userRoles.Contains("Admin") || userRoles.Contains("Staff")))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "AdminGroup");
            _logger.LogInformation($"User {userId} joined AdminGroup automatically");
        }
        
        // Join group theo userId để có thể gửi notification riêng
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
        }
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation($"Client disconnected: {Context.ConnectionId}, User: {userId}");
        
        if (exception != null)
        {
            _logger.LogError(exception, "Connection disconnected with error");
        }
        
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Client chủ động join admin group (backup method)
    /// </summary>
    [Authorize(Roles = "Admin,Staff")]
    public async Task JoinAdminGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "AdminGroup");
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation($"Client {Context.ConnectionId} (User: {userId}) joined AdminGroup manually");
    }

    /// <summary>
    /// Leave admin group
    /// </summary>
    public async Task LeaveAdminGroup()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "AdminGroup");
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation($"Client {Context.ConnectionId} (User: {userId}) left AdminGroup");
    }

    /// <summary>
    /// Method để client đánh dấu đã nhận notification
    /// </summary>
    public async Task AcknowledgeNotification(int notificationId)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation($"User {userId} acknowledged notification {notificationId}");
        
        // Có thể lưu vào database hoặc cache
        await Clients.Caller.SendAsync("NotificationAcknowledged", notificationId);
    }

    /// <summary>
    /// Ping để kiểm tra connection
    /// </summary>
    public async Task Ping()
    {
        await Clients.Caller.SendAsync("Pong", DateTime.UtcNow);
    }
}