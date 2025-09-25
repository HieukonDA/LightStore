using System.ComponentModel.DataAnnotations;

namespace TheLightStore.Models.Notifications;

public class Notification
{
    [Key]
    public int Id { get; set; }
    
    /// <summary>
    /// ID người nhận thông báo (Admin/Staff)
    /// </summary>
    public int? UserId { get; set; }
    
    /// <summary>
    /// Loại thông báo: 'order', 'payment', 'system'
    /// </summary>
    public string Type { get; set; } = null!;
    
    /// <summary>
    /// Tiêu đề thông báo
    /// </summary>
    public string Title { get; set; } = null!;
    
    /// <summary>
    /// Nội dung thông báo
    /// </summary>
    public string Content { get; set; } = null!;
    
    /// <summary>
    /// ID tham chiếu (OrderId, PaymentId,...)
    /// </summary>
    public int? ReferenceId { get; set; }
    
    /// <summary>
    /// URL chuyển hướng khi click thông báo
    /// </summary>
    public string? RedirectUrl { get; set; }
    
    /// <summary>
    /// Đã đọc hay chưa
    /// </summary>
    public bool IsRead { get; set; } = false;
    
    /// <summary>
    /// Mức độ ưu tiên: 'low', 'normal', 'high', 'urgent'
    /// </summary>
    public string Priority { get; set; } = "normal";
    
    /// <summary>
    /// Thời gian tạo thông báo
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Thời gian đọc thông báo
    /// </summary>
    public DateTime? ReadAt { get; set; }
    
    /// <summary>
    /// Thời gian hết hạn (nếu có)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
    
    /// <summary>
    /// Metadata bổ sung (JSON)
    /// </summary>
    public string? Metadata { get; set; }
}