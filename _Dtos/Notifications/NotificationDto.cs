namespace TheLightStore.Dtos.Notifications;

public class NotificationDto
{
    public int Id { get; set; }
    public string TargetRole { get; set; } = "admin";
    public string Type { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public int? ReferenceId { get; set; }
    public string? RedirectUrl { get; set; }
    public bool IsRead { get; set; }
    public string Priority { get; set; } = "normal";
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public string? Metadata { get; set; }
}

/// <summary>
/// DTO cho thông báo real-time qua SignalR
/// </summary>
public class RealTimeNotificationDto
{
    public string Type { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public int? ReferenceId { get; set; }
    public string? RedirectUrl { get; set; }
    public string Priority { get; set; } = "normal";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public object? Data { get; set; } // Dữ liệu bổ sung (order info, payment info...)
}

/// <summary>
/// DTO để tạo thông báo mới
/// </summary>
public class CreateNotificationDto
{
    public int? UserId { get; set; } // null = broadcast to all users of TargetRole
    
    /// <summary>
    /// Vai trò người nhận: "admin", "customer", "all"
    /// </summary>
    public string TargetRole { get; set; } = "admin"; // admin, customer, all
    
    public string Type { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public int? ReferenceId { get; set; }
    public string? RedirectUrl { get; set; }
    public string Priority { get; set; } = "normal";
    public DateTime? ExpiresAt { get; set; }
    public object? Metadata { get; set; }
}

/// <summary>
/// DTO để đánh dấu đã đọc thông báo
/// </summary>
public class MarkAsReadDto
{
    public List<int> NotificationIds { get; set; } = new();
}

/// <summary>
/// DTO thống kê thông báo
/// </summary>
public class NotificationStatsDto
{
    public int TotalCount { get; set; }
    public int UnreadCount { get; set; }
    public int TodayCount { get; set; }
    public int HighPriorityCount { get; set; }
}