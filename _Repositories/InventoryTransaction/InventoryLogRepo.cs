namespace TheLightStore.Interfaces.Notification;

public class InventoryLogRepo : IInventoryLogRepo
{
    private readonly DBContext _context;

    public InventoryLogRepo(DBContext context)
    {
        _context = context;
    }

    public async Task AddAsync(InventoryLog log)
    {
        await _context.InventoryLogs.AddAsync(log);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    // 👇 Có thể bổ sung thêm method lấy log để tiện debug
    public async Task<List<InventoryLog>> GetByProductIdAsync(int productId)
    {
        return await _context.InventoryLogs
            .Where(l => l.ProductId == productId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
    }

    public Task<List<InventoryLog>> GetByVariantIdAsync(int variantId)
    {
        throw new NotImplementedException();
    }
}