namespace TheLightStore.Interfaces.Orders;

public interface IOrderRepo
{
    Task<Order?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<Order>> GetByUserIdAsync(int userId, CancellationToken ct = default);
    Task AddAsync(Order order, CancellationToken ct = default);
    Task UpdateAsync(Order order, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}


public interface IOrderItemRepo
{
    Task<IEnumerable<OrderItem>> GetByOrderIdAsync(int orderId, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<OrderItem> items, CancellationToken ct = default);
}