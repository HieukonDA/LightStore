using TheLightStore.Domain.Entities.Orders;

namespace TheLightStore.Application.Interfaces.Repositories;

public interface IOrderStatusHistoryRepo
{
    Task AddAsync(OrderStatusHistory history, CancellationToken ct = default);
    Task<IEnumerable<OrderStatusHistory>> GetByOrderIdAsync(int orderId, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
