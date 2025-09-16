using TheLightStore.DTOs.Orders;

namespace TheLightStore.Interfaces.Orders;

public interface IOrderService
{
    // Core
    Task<Order> CreateOrderAsync(OrderCreateDto dto, CancellationToken ct = default);

    Task<Order?> GetOrderByIdAsync(int orderId, CancellationToken ct = default);
    Task<IEnumerable<Order>> GetOrdersByUserAsync(int userId, CancellationToken ct = default);

    // State transitions
    Task ConfirmOrderAsync(int orderId, string? adminNotes = null, CancellationToken ct = default);
    Task ShipOrderAsync(int orderId, string? trackingNumber = null, CancellationToken ct = default);
    Task DeliverOrderAsync(int orderId, CancellationToken ct = default);
    Task CancelOrderAsync(int orderId, string? reason = null, CancellationToken ct = default);

    // Tracking
    Task<IEnumerable<OrderStatusHistory>> GetOrderHistoryAsync(int orderId, CancellationToken ct = default);
}