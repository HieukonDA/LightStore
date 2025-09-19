using TheLightStore.Dtos.Orders;
using TheLightStore.DTOs.Orders;

namespace TheLightStore.Interfaces.Orders;

public interface IOrderService
{
    // Core
    Task<ServiceResult<OrderDto>> CreateOrderAsync(OrderCreateDto dto, CancellationToken ct = default);

    Task<ServiceResult<Order?>> GetOrderByIdAsync(int orderId, CancellationToken ct = default);
    Task<ServiceResult<IEnumerable<Order>>> GetOrdersByUserAsync(int userId, CancellationToken ct = default);

    // State transitions
    Task<ServiceResult<bool>> ConfirmOrderAsync(int orderId, string? adminNotes = null, CancellationToken ct = default);
    Task<ServiceResult<bool>> ShipOrderAsync(int orderId, string? trackingNumber = null, CancellationToken ct = default);
    Task<ServiceResult<bool>> DeliverOrderAsync(int orderId, CancellationToken ct = default);
    Task<ServiceResult<bool>> CancelOrderAsync(int orderId, string? reason = null, CancellationToken ct = default);

    // Tracking
    Task<ServiceResult<IEnumerable<OrderStatusHistory>>> GetOrderHistoryAsync(int orderId, CancellationToken ct = default);
}