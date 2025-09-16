namespace TheLightStore.Interfaces.Repository;
public interface IInventoryLogRepo
{
    Task AddAsync(InventoryLog log);
    Task<List<InventoryLog>> GetByProductIdAsync(int productId);
    Task<List<InventoryLog>> GetByVariantIdAsync(int variantId);
    Task SaveChangesAsync();
}