using Microsoft.EntityFrameworkCore.Storage;

namespace TheLightStore.Interfaces.Repository;

public interface IInventoryReservationRepo
{
    Task AddAsync(InventoryReservation reservation);
    Task<InventoryReservation?> GetByIdAsync(int id);
    Task<List<InventoryReservation>> GetByOrderIdAsync(int orderId); // Quan trọng!
    Task<List<InventoryReservation>> GetExpiredAsync();
    Task UpdateAsync(InventoryReservation reservation);
    Task DeleteAsync(int id);
    Task SaveChangesAsync();
    Task<IDbContextTransaction> BeginTransactionAsync();
    Task<int> GetReservedQuantityAsync(int productId, int? variantId);
    Task<int> GetActiveReservedQuantityAsync(int productId, int? variantId);
}