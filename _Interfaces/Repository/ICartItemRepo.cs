namespace TheLightStore.Interfaces.Repository;

public interface ICartItemRepo
{
    // Basic CRUD
    Task<CartItem?> GetByIdAsync(int itemId);
    Task<List<CartItem>> GetByCartIdAsync(int cartId);
    Task<CartItem?> GetCartItemAsync(int cartId, int productId, int? variantId);
    Task<CartItem> CreateAsync(CartItem cartItem);
    Task<CartItem> UpdateAsync(CartItem cartItem);
    Task<bool> DeleteAsync(int itemId);
    Task<bool> DeleteByCartIdAsync(int cartId);

    // Item Operations
    Task<bool> ExistsInCartAsync(int cartId, int productId, int? variantId);
    Task UpdateQuantityAsync(int itemId, int newQuantity);
    Task<List<CartItem>> GetCartItemsWithDetailsAsync(int cartId);
}