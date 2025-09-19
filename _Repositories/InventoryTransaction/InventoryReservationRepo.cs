using Microsoft.EntityFrameworkCore.Storage;

namespace TheLightStore.Repositories.InventoryTransaction;

public class InventoryReservationRepo : IInventoryReservationRepo
{
    private readonly DBContext _context;

    public InventoryReservationRepo(DBContext context)
    {
        _context = context;
    }

    public async Task AddAsync(InventoryReservation reservation)
    {
        reservation.CreatedAt = DateTime.UtcNow;
        await _context.InventoryReservations.AddAsync(reservation);
    }

    public async Task<InventoryReservation?> GetByIdAsync(int id)
    {
        return await _context.InventoryReservations
            .Include(ir => ir.Product)
            .Include(ir => ir.Variant)
            .FirstOrDefaultAsync(ir => ir.Id == id);
    }

    public async Task<List<InventoryReservation>> GetByOrderIdAsync(int orderId)
    {
        return await _context.InventoryReservations
            .Include(r => r.Product)
            .Include(r => r.Variant)
            .Where(r => r.OrderId == orderId)
            .ToListAsync();
    }

    public async Task<List<InventoryReservation>> GetExpiredAsync()
    {
        var now = DateTime.UtcNow;
        return await _context.InventoryReservations
            .Where(ir => ir.ReservedUntil < now && ir.Status == InventoryStatus.Active)
            .Include(ir => ir.Product)
            .Include(ir => ir.Variant)
            .ToListAsync();
    }

    public async Task UpdateAsync(InventoryReservation reservation)
    {
        _context.InventoryReservations.Update(reservation);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        var reservation = await _context.InventoryReservations.FindAsync(id);
        if (reservation != null)
        {
            _context.InventoryReservations.Remove(reservation);
        }
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await _context.Database.BeginTransactionAsync();
    }

    public async Task<int> GetReservedQuantityAsync(int productId, int? variantId)
    {
        return await _context.InventoryReservations
            .Where(r => r.ProductId == productId &&
                       r.VariantId == variantId &&
                       r.Status == "Reserved" &&
                       r.ReservedUntil > DateTime.UtcNow)
            .SumAsync(r => r.Quantity);
    }



    // Additional helper methods for inventory management
    public async Task<List<InventoryReservation>> GetByCartIdAsync(int cartId)
    {
        return await _context.InventoryReservations
            .Include(r => r.Product)
            .Include(r => r.Variant)
            .Where(r => r.CartId == cartId && r.Status == "Active")
            .ToListAsync();
    }

    public async Task<List<InventoryReservation>> GetBySessionIdAsync(string sessionId)
    {
        return await _context.InventoryReservations
            .Include(r => r.Product)
            .Include(r => r.Variant)
            .Where(r => r.SessionId == sessionId && r.Status == "Active")
            .ToListAsync();
    }

    public async Task<List<InventoryReservation>> GetByUserIdAsync(int userId)
    {
        return await _context.InventoryReservations
            .Include(r => r.Product)
            .Include(r => r.Variant)
            .Where(r => r.UserId == userId && r.Status == "Active")
            .ToListAsync();
    }

    public async Task<int> GetTotalReservedQuantityAsync(int productId, int? variantId = null)
    {
        var query = _context.InventoryReservations
            .Where(r => r.ProductId == productId && r.Status == "Active");

        if (variantId.HasValue)
        {
            query = query.Where(r => r.VariantId == variantId.Value);
        }

        return await query.SumAsync(r => r.Quantity);
    }
    
    // inside InventoryReservationRepository : IInventoryReservationRepo
    public async Task<int> GetActiveReservedQuantityAsync(int productId, int? variantId)
    {
        var now = DateTime.UtcNow;

        var query = _context.InventoryReservations
            .Where(r => r.ProductId == productId
                        && r.Status == InventoryStatus.Active
                        && r.ReservedUntil > now);

        if (variantId.HasValue)
            query = query.Where(r => r.VariantId == variantId.Value);
        else
            query = query.Where(r => r.VariantId == null);

        var sum = await query.SumAsync(r => (int?)r.Quantity) ?? 0;
        return sum;
    }



}