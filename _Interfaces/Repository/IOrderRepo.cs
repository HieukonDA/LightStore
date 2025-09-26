namespace TheLightStore.Interfaces.Orders;

public interface IOrderRepo
{
    Task<PagedResult<Order>> GetAllAsync(PagedRequest request, CancellationToken ct = default);
    Task<Order?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken ct = default);
    Task<IEnumerable<Order>> GetByUserIdAsync(int userId, CancellationToken ct = default);
    Task AddAsync(Order order, CancellationToken ct = default);
    Task UpdateAsync(Order order, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);


    // CẦN THÊM cho Dashboard Stats & Charts:
    Task<decimal> GetTotalRevenueAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken ct = default);
    Task<int> GetTotalOrdersCountAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken ct = default);
    Task<decimal> GetAverageOrderValueAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken ct = default);
    Task<List<SalesDataPoint>> GetSalesByMonthAsync(int months = 6, CancellationToken ct = default);
    Task<IEnumerable<Order>> GetRecentOrdersAsync(int limit = 10, CancellationToken ct = default);
}


public interface IOrderItemRepo
{
    Task<IEnumerable<OrderItem>> GetByOrderIdAsync(int orderId, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<OrderItem> items, CancellationToken ct = default);
}