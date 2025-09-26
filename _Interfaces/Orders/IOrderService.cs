using TheLightStore.Dtos.Orders;
using TheLightStore.DTOs.Orders;

namespace TheLightStore.Interfaces.Orders;

public interface IOrderService
{
    // Core
    Task<ServiceResult<PagedResult<OrderDto>>> GetAllAsync(PagedRequest request, CancellationToken ct = default);
    Task<ServiceResult<OrderDto>> CreateOrderAsync(OrderCreateDto dto, CancellationToken ct = default);

    Task<ServiceResult<Order?>> GetOrderByIdAsync(int orderId, CancellationToken ct = default);
    Task<ServiceResult<Order?>> GetOrderByOrderNumberAsync(string orderNumber, CancellationToken ct = default);
    Task<ServiceResult<IEnumerable<Order>>> GetOrdersByUserAsync(int userId, CancellationToken ct = default);

    // State transitions
    Task<ServiceResult<bool>> ConfirmOrderAsync(int orderId, string? adminNotes = null, CancellationToken ct = default);
    Task<ServiceResult<bool>> ProcessOrderAsync(int orderId, string? processingNotes = null, CancellationToken ct = default);
    Task<ServiceResult<bool>> ShipOrderAsync(int orderId, string? trackingNumber = null, CancellationToken ct = default);
    Task<ServiceResult<bool>> DeliverOrderAsync(int orderId, CancellationToken ct = default);
    Task<ServiceResult<bool>> CancelOrderAsync(int orderId, string? reason = null, CancellationToken ct = default);

    // Tracking
    Task<ServiceResult<IEnumerable<OrderStatusHistory>>> GetOrderHistoryAsync(int orderId, CancellationToken ct = default);

    //admin stats
    // Dashboard Stats & Analytics
    Task<ServiceResult<decimal>> GetTotalRevenueAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken ct = default);
    Task<ServiceResult<int>> GetTotalOrdersCountAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken ct = default);
    Task<ServiceResult<decimal>> GetAverageOrderValueAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken ct = default);
    Task<ServiceResult<List<SalesDataPoint>>> GetSalesByMonthAsync(int months = 6, CancellationToken ct = default);
    Task<ServiceResult<IEnumerable<OrderDto>>> GetRecentOrdersAsync(int limit = 10, CancellationToken ct = default);
}