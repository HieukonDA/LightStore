using TheLightStore.Interfaces.Orders;

namespace TheLightStore.Repositories.Orders;

public class OrderRepo : IOrderRepo
{
    private readonly DBContext _context;

    public OrderRepo(DBContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
            .Include(o => o.OrderPayments)
            .FirstOrDefaultAsync(o => o.Id == id, ct);
    }

    public async Task<IEnumerable<Order>> GetByUserIdAsync(int userId, CancellationToken ct = default)
    {
        return await _context.Orders
            .Where(o => o.UserId == userId)
            .Include(o => o.OrderItems)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Order order, CancellationToken ct = default)
    {
        await _context.Orders.AddAsync(order, ct);
    }

    public async Task UpdateAsync(Order order, CancellationToken ct = default)
    {
        _context.Orders.Update(order);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
}

public class OrderItemRepo : IOrderItemRepo
{
    private readonly DBContext _context;

    public OrderItemRepo(DBContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<OrderItem>> GetByOrderIdAsync(int orderId, CancellationToken ct = default)
    {
        return await _context.OrderItems
            .Where(oi => oi.OrderId == orderId)
            .ToListAsync(ct);
    }

    public async Task AddRangeAsync(IEnumerable<OrderItem> items, CancellationToken ct = default)
    {
        await _context.OrderItems.AddRangeAsync(items, ct);
    }
}