namespace TheLightStore.Interfaces.Repository;

public class ShoppingCartRepo : IShoppingCartRepo
{
    readonly private DBContext _context;
    readonly private ICartItemRepo _cartItemRepo;

    public ShoppingCartRepo(DBContext context, ICartItemRepo cartItemRepo)
    {
        _context = context;
        _cartItemRepo = cartItemRepo;
    }


    // Basic CRUD
    public async Task<ShoppingCart?> GetByIdAsync(int cartId)
    {
        try
        {
            return await _context.ShoppingCarts.FindAsync(cartId);
        }
        catch (System.Exception ex)
        {
            throw new Exception($"Error retrieving shopping cart by ID: {ex.Message}");
        }
    }
    public async Task<ShoppingCart?> GetByUserIdAsync(int userId)
    {
        try
        {
            return await _context.ShoppingCarts.FirstOrDefaultAsync(cart => cart.UserId == userId);
        }
        catch (System.Exception ex)
        {
            throw new Exception($"Error retrieving shopping cart by user ID: {ex.Message}");
        }
    }

    public async Task<ShoppingCart?> GetBySessionIdAsync(string sessionId)
    {
        try
        {
            return await _context.ShoppingCarts.FirstOrDefaultAsync(cart => cart.SessionId == sessionId);
        }
        catch (System.Exception ex)
        {
            throw new Exception($"Error retrieving shopping cart by session ID: {ex.Message}");
        }
    }

    public async Task<ShoppingCart> CreateAsync(ShoppingCart cart)
    {
        try
        {
            _context.ShoppingCarts.Add(cart);
            await _context.SaveChangesAsync();
            return cart;
        }
        catch (System.Exception ex)
        {
            throw new Exception($"Error creating shopping cart: {ex.Message}");
        }
    }

    public async Task<ShoppingCart> UpdateAsync(ShoppingCart cart)
    {
        try
        {
            _context.ShoppingCarts.Update(cart);
            await _context.SaveChangesAsync();
            return cart;
        }
        catch (System.Exception ex)
        {
            throw new Exception($"Error updating shopping cart: {ex.Message}");
        }
    }

    public async Task<bool> DeleteAsync(int cartId)
    {
        try
        {
            var cart = await _context.ShoppingCarts.FindAsync(cartId);
            if (cart == null) return false;

            // First, delete associated cart items
            await _cartItemRepo.DeleteByCartIdAsync(cartId);

            _context.ShoppingCarts.Remove(cart);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (System.Exception ex)
        {
            throw new Exception($"Error deleting shopping cart: {ex.Message}");
        }
    }

    // Cart with Items
    public async Task<ShoppingCart?> GetCartWithItemsAsync(int cartId)
    {
        try
        {
            return await _context.ShoppingCarts
                .Include(cart => cart.CartItems)
                .FirstOrDefaultAsync(cart => cart.Id == cartId);
        }
        catch (System.Exception ex)
        {
            throw new Exception($"Error retrieving shopping cart with items: {ex.Message}");
        }
    }
    public async Task<ShoppingCart?> GetCartWithItemsByUserIdAsync(int userId)
    {
        try
        {
            return await _context.ShoppingCarts
                .Include(cart => cart.CartItems)
                    .ThenInclude(ci => ci.Product)
                        .ThenInclude(p => p.ProductImages)
                .Include(cart => cart.CartItems)
                    .ThenInclude(ci => ci.Variant)
                .FirstOrDefaultAsync(cart => cart.UserId == userId);
        }
        catch (System.Exception ex)
        {
            throw new Exception($"Error retrieving shopping cart with items by user ID: {ex.Message}");
        }
    }
    public async Task<ShoppingCart?> GetCartWithItemsBySessionIdAsync(string sessionId)
    {
        try
        {
            return await _context.ShoppingCarts
                .Include(cart => cart.CartItems)
                    .ThenInclude(ci => ci.Product)
                        .ThenInclude(p => p.ProductImages)
                .Include(cart => cart.CartItems)
                    .ThenInclude(ci => ci.Variant)
                .FirstOrDefaultAsync(cart => cart.SessionId == sessionId);
        }
        catch (System.Exception ex)
        {
            throw new Exception($"Error retrieving shopping cart with items by session ID: {ex.Message}");
        }
    }

    // Cart Operations
    public async Task<bool> UpdateCartTotalsAsync(int cartId)
    {
        try
        {
            var cart = await GetCartWithItemsAsync(cartId);
            if (cart == null) throw new Exception("Cart not found");

            decimal total = 0;
            foreach (var item in cart.CartItems)
            {
                // Assuming each CartItem has a Product with a Price property
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    total += product.BasePrice * item.Quantity;
                }
            }

            cart.Subtotal = total;
            await UpdateAsync(cart);
            return true;
        }
        catch (System.Exception ex)
        {
            return false;
            throw new Exception($"Error updating cart totals: {ex.Message}");
        }
    }
    public async Task<bool> TransferCartOwnershipAsync(string sessionId, int userId)
    {
        try
        {
            var cart = await GetBySessionIdAsync(sessionId);
            if (cart == null) throw new Exception("Cart not found");

            cart.UserId = userId;
            cart.SessionId = null; // Clear session ID after transfer
            await UpdateAsync(cart);
            return true;
        }
        catch (System.Exception ex)
        {
            return false;
            throw new Exception($"Error transferring cart ownership: {ex.Message}");
        }
    }
    public async Task<bool> MergeCartsAsync(int targetCartId, int sourceCartId)
    {
        try
        {
            var targetCart = await GetCartWithItemsAsync(targetCartId);
            var sourceCart = await GetCartWithItemsAsync(sourceCartId);
            if (targetCart == null || sourceCart == null) throw new Exception("One or both carts not found");

            foreach (var item in sourceCart.CartItems)
            {
                var existingItem = targetCart.CartItems
                    .FirstOrDefault(ci => ci.ProductId == item.ProductId && ci.VariantId == item.VariantId);

                if (existingItem != null)
                {
                    // If item exists in target cart, update quantity
                    existingItem.Quantity += item.Quantity;
                    await _cartItemRepo.UpdateAsync(existingItem);
                }
                else
                {
                    // If item does not exist, transfer it to target cart
                    item.CartId = targetCart.Id;
                    await _cartItemRepo.CreateAsync(item);
                }
            }

            // After merging, delete the source cart
            await DeleteAsync(sourceCartId);

            // Update totals for the target cart
            await UpdateCartTotalsAsync(targetCartId);
            return true;
        }
        catch (System.Exception ex)
        {
            return false;
            throw new Exception($"Error merging carts: {ex.Message}");
        }
    }

    public async Task<bool> ClearExpiredCartsAsync(DateTime expirationDate)
    {
        try
        {
            var expiredCarts = await _context.ShoppingCarts
                .Where(cart => cart.CreatedAt < expirationDate)
                .ToListAsync();

            _context.ShoppingCarts.RemoveRange(expiredCarts);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (System.Exception ex)
        {
            return false;
            throw new Exception($"Error clearing expired carts: {ex.Message}");
        }
    }

    #region statistics methods

    public async Task<int> CountActiveCartsAsync()
    {
        return await _context.ShoppingCarts
            .Where(c => c.ItemsCount > 0)
            .CountAsync();
    }

    public async Task<int> CountAbandonedCartsAsync(TimeSpan abandonedPeriod)
    {
        var cutoffDate = DateTime.UtcNow - abandonedPeriod;

        return await _context.ShoppingCarts
            .Where(c => c.ItemsCount > 0 &&
                    c.UpdatedAt.HasValue &&
                    c.UpdatedAt.Value < cutoffDate)
            .CountAsync();
    }

    public async Task<decimal> GetTotalCartsValueAsync()
    {
        return await _context.ShoppingCarts
            .Where(c => c.ItemsCount > 0)
            .SumAsync(c => c.Subtotal);
    }

    public async Task<decimal> GetAverageCartValueAsync()
    {
        var activeCartsWithValue = await _context.ShoppingCarts
            .Where(c => c.ItemsCount > 0)
            .Select(c => c.Subtotal)
            .ToListAsync();

        return activeCartsWithValue.Any() ? activeCartsWithValue.Average() : 0;
    }

    public async Task<double> GetAverageItemsPerCartAsync()
    {
        var activeCartsWithItems = await _context.ShoppingCarts
            .Where(c => c.ItemsCount > 0)
            .Select(c => c.ItemsCount)
            .ToListAsync();

        return activeCartsWithItems.Any() ? activeCartsWithItems.Average() : 0;
    }

    public async Task<int> GetTotalCartItemsAsync()
    {
        return await _context.ShoppingCarts
            .Where(c => c.ItemsCount > 0)
            .SumAsync(c => c.ItemsCount);
    }



    #endregion

    // Additional methods can be added as needed
    public async Task<List<ShoppingCart>> GetExpiredEmptyCartsAsync(DateTime cutoffDate)
    {
        try
        {
            return await _context.ShoppingCarts
                .Where(cart => cart.ItemsCount == 0 && cart.CreatedAt < cutoffDate)
                .ToListAsync();
        }
        catch (System.Exception ex)
        {
            throw new Exception($"Error retrieving expired empty carts: {ex.Message}");
        }
    }
}