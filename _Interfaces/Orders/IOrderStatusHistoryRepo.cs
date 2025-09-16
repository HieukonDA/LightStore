namespace TheLightStore.Interfaces.Orders;

public interface IOrderStatusHistoryRepo
{
    Task AddAsync(OrderStatusHistory history, CancellationToken ct = default);
    Task<IEnumerable<OrderStatusHistory>> GetByOrderIdAsync(int orderId, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}