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
    Task UpdateCartTotalsAsync(int cartId);
    Task TransferCartOwnershipAsync(string sessionId, int userId);
    Task MergeCartsAsync(int targetCartId, int sourceCartId);
    Task ClearExpiredCartsAsync(DateTime expirationDate);
}