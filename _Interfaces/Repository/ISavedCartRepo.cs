namespace TheLightStore.Interfaces.Repository;

public interface ISavedCartRepo
{
    // Basic CRUD
    Task<SavedCart?> GetByIdAsync(int savedCartId);
    Task<List<SavedCart>> GetByUserIdAsync(int userId);
    Task<SavedCart> CreateAsync(SavedCart savedCart);
    Task<SavedCart> UpdateAsync(SavedCart savedCart);
    Task<bool> DeleteAsync(int savedCartId);

    // Operations
    Task<bool> SaveCurrentCartAsync(int cartId, int userId, string? cartName);
    Task<ShoppingCart?> RestoreSavedCartAsync(int savedCartId, int userId);
}