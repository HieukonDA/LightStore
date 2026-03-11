using Microsoft.EntityFrameworkCore;
using TheLightStore.Application.Interfaces.Repositories;
using TheLightStore.Domain.Entities.Orders;
using TheLightStore.Infrastructure.Persistence;

namespace TheLightStore.Infrastructure.Repositories.Orders;

public class OrderStatusHistoryRepo : IOrderStatusHistoryRepo
{
    private readonly DBContext _context;

    public OrderStatusHistoryRepo(DBContext context)
    {
        _context = context;
    }

    public async Task AddAsync(OrderStatusHistory history, CancellationToken ct = default)
    {
        await _context.OrderStatusHistories.AddAsync(history, ct);
    }

    public async Task<IEnumerable<OrderStatusHistory>> GetByOrderIdAsync(int orderId, CancellationToken ct = default)
    {
        return await _context.OrderStatusHistories
            .Where(h => h.OrderId == orderId)
            .OrderBy(h => h.ChangedAt)
            .ToListAsync(ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
}
