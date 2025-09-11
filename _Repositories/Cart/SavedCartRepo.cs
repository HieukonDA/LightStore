using System.Text.Json;

namespace TheLightStore.Interfaces.Repository;

public class SavedCartRepo : ISavedCartRepo
{
    readonly private DBContext _context;
    readonly private ICartItemRepo _cartItemRepo;

    public SavedCartRepo(DBContext context, ICartItemRepo cartItemRepo)
    {
        _context = context;
        _cartItemRepo = cartItemRepo;
    }

    // Basic CRUD
    public async Task<SavedCart?> GetByIdAsync(int savedCartId)
    {
        try
        {
            return await _context.SavedCarts.FindAsync(savedCartId);
        }
        catch (System.Exception ex)
        {
            throw new Exception($"Error retrieving saved cart by ID: {ex.Message}");
        }
    }
    public async Task<List<SavedCart>> GetByUserIdAsync(int userId)
    {
        try
        {
            return await _context.SavedCarts.Where(cart => cart.UserId == userId).ToListAsync();
        }
        catch (System.Exception ex)
        {
            throw new Exception($"Error retrieving saved carts by user ID: {ex.Message}");
        }
    }

    public async Task<SavedCart> CreateAsync(SavedCart savedCart)
    {
        try
        {
            _context.SavedCarts.Add(savedCart);
            await _context.SaveChangesAsync();
            return savedCart;
        }
        catch (System.Exception ex)
        {
            throw new Exception($"Error creating saved cart: {ex.Message}");
        }
    }
    public async Task<SavedCart> UpdateAsync(SavedCart savedCart)
    {
        try
        {
            _context.SavedCarts.Update(savedCart);
            await _context.SaveChangesAsync();
            return savedCart;
        }
        catch (System.Exception ex)
        {
            throw new Exception($"Error updating saved cart: {ex.Message}");
        }
    }
    public async Task<bool> DeleteAsync(int savedCartId)
    {
        try
        {
            var savedCart = await _context.SavedCarts.FindAsync(savedCartId);
            if (savedCart == null) return false;

            _context.SavedCarts.Remove(savedCart);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (System.Exception ex)
        {
            throw new Exception($"Error deleting saved cart: {ex.Message}");
        }
    }

    // Operations
    public async Task<bool> SaveCurrentCartAsync(int cartId, int userId, string? cartName)
    {
        try
        {
            var existingCart = await _context.ShoppingCarts.FindAsync(cartId);
            if (existingCart == null || existingCart.UserId != userId)
                throw new Exception("Cart not found or does not belong to the user.");

            var cartItems = await _cartItemRepo.GetByCartIdAsync(cartId);
            if (cartItems.Count == 0)
                throw new Exception("Cannot save an empty cart.");

            var savedCart = new SavedCart
            {
                UserId = userId,
                CartName = cartName,
                CreatedAt = DateTime.UtcNow,
                CartData = JsonSerializer.Serialize(cartItems.Select(item => new 
                {
                    ProductId = item.ProductId,
                    VariantId = item.VariantId,
                    Quantity = item.Quantity,
                    PriceAtSave = item.UnitPrice
                })),
            ItemsCount = cartItems.Count
            };

            _context.SavedCarts.Add(savedCart);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (System.Exception ex)
        {
            throw new Exception($"Error saving current cart: {ex.Message}");
        }
    }
    public async Task<ShoppingCart?> RestoreSavedCartAsync(int savedCartId, int userId)
    {
        try
        {
            var savedCart = await _context.SavedCarts.FindAsync(savedCartId);
            if (savedCart == null || savedCart.UserId != userId)
                throw new Exception("Saved cart not found or does not belong to the user.");

            var cartItemsData = JsonSerializer.Deserialize<List<CartItem>>(savedCart.CartData);
        
            var shoppingCart = new ShoppingCart
            {
                UserId = userId,
                SessionId = null, // Hoặc generate session ID mới nếu cần
                ItemsCount = cartItemsData.Count,
                Subtotal = cartItemsData.Sum(item => item.UnitPrice * item.Quantity), // Tính subtotal
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CartItems = cartItemsData
            };

            _context.ShoppingCarts.Add(shoppingCart);
            await _context.SaveChangesAsync();
            return shoppingCart;
        }
        catch (System.Exception ex)
        {
            throw new Exception($"Error restoring saved cart: {ex.Message}");
        }
    }

}