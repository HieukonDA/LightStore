using TheLightStore.Dtos.Notifications;
using TheLightStore.Models.Orders_Carts;

namespace TheLightStore.Interfaces.Notifications;

public interface INotificationService
{
    // Email notifications (existing)
    Task NotifyOrderCreatedAsync(Order order, CancellationToken ct = default);
    Task NotifyOrderStatusChangedAsync(Order order, string oldStatus, string newStatus, CancellationToken ct = default);
    
    // Real-time notifications (new)
    Task CreateAndBroadcastNotificationAsync(CreateNotificationDto dto, CancellationToken ct = default);
    Task BroadcastToAdminsAsync(RealTimeNotificationDto notification, CancellationToken ct = default);
    Task SendToUserAsync(int userId, RealTimeNotificationDto notification, CancellationToken ct = default);
    
    // Database notifications
    Task<ServiceResult<PagedResult<NotificationDto>>> GetUserNotificationsAsync(int userId, PagedRequest request, string userRole = "admin", CancellationToken ct = default);
    Task<ServiceResult<NotificationStatsDto>> GetUserNotificationStatsAsync(int userId, string userRole = "admin", CancellationToken ct = default);
    Task<ServiceResult<bool>> MarkAsReadAsync(int userId, MarkAsReadDto dto, CancellationToken ct = default);
    Task<ServiceResult<bool>> DeleteNotificationAsync(int userId, int notificationId, CancellationToken ct = default);
    
    // Business logic notifications
    Task NotifyNewOrderAsync(Order order, CancellationToken ct = default);
    Task NotifyOrderUpdateAsync(Order order, string oldStatus, string newStatus, CancellationToken ct = default);
    Task NotifyPaymentSuccessAsync(Order order, CancellationToken ct = default);
    Task NotifyLowStockAsync(int productId, string productName, int currentStock, CancellationToken ct = default);
    
    // Customer notifications
    Task SendToCustomerAsync(int customerId, RealTimeNotificationDto notification, CancellationToken ct = default);
    Task BroadcastToCustomersAsync(RealTimeNotificationDto notification, CancellationToken ct = default);
    Task NotifyCustomerOrderStatusAsync(int customerId, Order order, string newStatus, CancellationToken ct = default);
    Task NotifyCustomerPaymentAsync(int customerId, Order order, bool success, string? paymentMethod = null, CancellationToken ct = default);
    Task NotifyCustomerPromotionAsync(int customerId, string title, string content, string? redirectUrl = null, CancellationToken ct = default);
    Task BroadcastPromotionAsync(string title, string content, string? redirectUrl = null, CancellationToken ct = default);
}