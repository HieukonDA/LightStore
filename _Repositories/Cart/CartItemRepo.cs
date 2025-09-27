
namespace TheLightStore.Interfaces.Repository;

public class CartItemRepo : ICartItemRepo
{
    readonly private DBContext _context;
    
    public CartItemRepo(DBContext context)
    {
        _context = context;
    }



    // Basic CRUD
    public async Task<CartItem?> GetByIdAsync(int itemId)
    {
        try
        {
            return await _context.CartItems.FindAsync(itemId);
        }
        catch (System.Exception ex)
        {
            throw new Exception($"Error retrieving cart item by ID: {ex.Message}");
        }
    }

    public async Task<List<CartItem>> GetByCartIdAsync(int cartId)
    {
        try
        {
            return await _context.CartItems.Where(item => item.CartId == cartId).ToListAsync();
        }
        catch (System.Exception ex)
        {
            throw new Exception($"Error retrieving cart items by cart ID: {ex.Message}");
        }
    }

    public async Task<CartItem?> GetCartItemAsync(int cartId, int productId, int? variantId)
    {
        try
        {
            return await _context.CartItems
                .FirstOrDefaultAsync(item => item.CartId == cartId && item.ProductId == productId && item.VariantId == variantId);
        }
        catch (System.Exception ex)
        {
            throw new Exception($"Error retrieving cart item: {ex.Message}");
        }
    }

    public async Task<CartItem> CreateAsync(CartItem cartItem)
    {
        try
        {
            _context.CartItems.Add(cartItem);
            await _context.SaveChangesAsync();
            return cartItem;
        }
        catch (System.Exception ex)
        {
            throw new Exception($"Error creating cart item: {ex.Message}");
        }
    }

    public async Task<CartItem> UpdateAsync(CartItem cartItem)
    {
        try
        {
            _context.CartItems.Update(cartItem);
            await _context.SaveChangesAsync();
            return cartItem;
        }
        catch (System.Exception ex)
        {
            throw new Exception($"Error updating cart item: {ex.Message}");
        }
    }

    public async Task<bool> DeleteAsync(int itemId)
    {
        try
        {
            var cartItem = await _context.CartItems.FindAsync(itemId);
            if (cartItem == null) return false;

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (System.Exception ex)
        {
            throw new Exception($"Error deleting cart item: {ex.Message}");
        }
    }

    public async Task<bool> DeleteByCartIdAsync(int cartId)
    {
        try
        {
            var cartItems = await _context.CartItems.Where(item => item.CartId == cartId).ToListAsync();
            if (cartItems.Count == 0) return false;

            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (System.Exception ex)
        {
            throw new Exception($"Error deleting cart items by cart ID: {ex.Message}");
        }
    }


    // Item Operations
    public async Task<bool> ExistsInCartAsync(int cartId, int productId, int? variantId)
    {
        try
        {
            return await _context.CartItems.AnyAsync(item => item.CartId == cartId && item.ProductId == productId && item.VariantId == variantId);
        }
        catch (System.Exception ex)
        {
            throw new Exception($"Error checking existence of cart item: {ex.Message}");
        }

    }

    public async Task UpdateQuantityAsync(int itemId, int newQuantity)
    {
        try
        {
            var cartItem = await _context.CartItems.FindAsync(itemId);
            if (cartItem == null) return;

            cartItem.Quantity = newQuantity;
            _context.CartItems.Update(cartItem);
            await _context.SaveChangesAsync();
        }
        catch (System.Exception ex)
        {
            throw new Exception($"Error updating cart item quantity: {ex.Message}");
        }
    }

    public async Task<List<CartItem>> GetCartItemsWithDetailsAsync(int cartId)
    {
        try
        {
            return await _context.CartItems
                .Include(item => item.Product)
                .Include(item => item.Variant)
                .Where(item => item.CartId == cartId)
                .ToListAsync();
        }
        catch (System.Exception ex)
        {
            throw new Exception($"Error retrieving cart items with details: {ex.Message}");
        }
    }

    public async Task<List<CartItem>> GetCartItemsWithStockAsync(int cartId)
    {
        try
        {
            return await _context.CartItems
                .Include(item => item.Product)
                    .ThenInclude(p => p.ProductImages)
                .Include(item => item.Variant)
                .Where(item => item.CartId == cartId)
                .OrderBy(item => item.AddedAt)
                .ToListAsync();
        }
        catch (System.Exception ex)
        {
            throw new Exception($"Error retrieving cart items with stock information: {ex.Message}");
        }
    }
}