namespace TheLightStore.Interfaces.Notifications;

public interface INotificationService
{
    Task NotifyOrderCreatedAsync(Order order, CancellationToken ct = default);
    Task NotifyOrderStatusChangedAsync(Order order, string oldStatus, string newStatus, CancellationToken ct = default);
    
}