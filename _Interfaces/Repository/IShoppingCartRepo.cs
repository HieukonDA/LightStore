namespace TheLightStore.Interfaces.Repository;

public interface IShoppingCartRepo
{
    // Basic CRUD
    Task<ShoppingCart?> GetByIdAsync(int cartId);
    Task<ShoppingCart?> GetByUserIdAsync(int userId);
    Task<ShoppingCart?> GetBySessionIdAsync(string sessionId);
    Task<ShoppingCart> CreateAsync(ShoppingCart cart);
    Task<ShoppingCart> UpdateAsync(ShoppingCart cart);
    Task<bool> DeleteAsync(int cartId);

    // Cart with Items
    Task<ShoppingCart?> GetCartWithItemsAsync(int cartId);
    Task<ShoppingCart?> GetCartWithItemsByUserIdAsync(int userId);
    Task<ShoppingCart?> GetCartWithItemsBySessionIdAsync(string sessionId);

    // Cart Operations
    Task<bool> UpdateCartTotalsAsync(int cartId);
    Task<bool> TransferCartOwnershipAsync(string sessionId, int userId);
    Task<bool> MergeCartsAsync(int targetCartId, int sourceCartId);
    Task<bool> ClearExpiredCartsAsync(DateTime expirationDate);

    // Statistics Methods - NEW
    Task<int> CountActiveCartsAsync();
    Task<int> CountAbandonedCartsAsync(TimeSpan abandonedPeriod);
    Task<decimal> GetTotalCartsValueAsync();
    Task<decimal> GetAverageCartValueAsync();
    Task<double> GetAverageItemsPerCartAsync();
    Task<int> GetTotalCartItemsAsync();

    // Additional methods can be added as needed
    Task<List<ShoppingCart>> GetExpiredEmptyCartsAsync(DateTime cutoffDate);
}