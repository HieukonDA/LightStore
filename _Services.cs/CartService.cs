using System.Text.Json;

namespace TheLightStore.Interfaces.Cart;

public class CartService : ICartService
{
    private readonly ILogger<CartService> _logger;
    private readonly ISavedCartRepo _savedCartRepo;
    private readonly IShoppingCartRepo _shoppingCartRepo;
    private readonly ICartItemRepo _cartItemRepo;
    private readonly IProductVariantRepo _productVariantRepo;
    private readonly IProductRepo _productRepo;

    public CartService(ILogger<CartService> logger, ISavedCartRepo savedCartRepo, IShoppingCartRepo shoppingCartRepo, ICartItemRepo cartItemRepo, IProductVariantRepo productVariantRepo, IProductRepo productRepo)
    {
        _logger = logger;
        _savedCartRepo = savedCartRepo;
        _shoppingCartRepo = shoppingCartRepo;
        _cartItemRepo = cartItemRepo;
        _productVariantRepo = productVariantRepo;
        _productRepo = productRepo;
    }


    #region Cart Management


    public async Task<ServiceResult<CartDto?>> GetCartAsync(int? userId, string? sessionId)
    {
        try
        {
            _logger.LogInformation("Fetching cart for UserId: {UserId}, SessionId: {SessionId}", userId, sessionId);
            //validate input
            if (userId == null && string.IsNullOrEmpty(sessionId))
            {
                return ServiceResult<CartDto?>.FailureResult("Either userId or sessionId must be provided.", new List<string> { "Invalid input parameters." });
            }

            ShoppingCart? cart = await GetCartEntityAsync(userId, sessionId);

            if (cart == null)
            {
                return ServiceResult<CartDto?>.FailureResult("cart not found.", new List<string> { "No cart found for the given userId or sessionId." });
            }

            //map to dto
            var cartDto = MapToCartDto(cart);
            _logger.LogInformation("Cart retrieved successfully for CartId: {CartId}", cartDto.Id);
            return ServiceResult<CartDto?>.SuccessResult(cartDto, "Cart retrieved successfully.");

        }
        catch (System.Exception ex)
        {
            return ServiceResult<CartDto?>.FailureResult("An error occurred while fetching the cart.", new List<string> { ex.Message });
        }

    }



    public async Task<ServiceResult<CartDto>> GetOrCreateCartAsync(int? userId, string? sessionId)
    {
        try
        {
            _logger.LogInformation("Getting or creating cart for UserId: {UserId}, SessionId: {SessionId}", userId, sessionId);

            // Validate input parameters
            if (userId == null && string.IsNullOrWhiteSpace(sessionId))
            {
                _logger.LogError("Both UserId and SessionId are null/empty - cannot create cart");
                return ServiceResult<CartDto>.FailureResult("Either UserId or SessionId must be provided", new List<string> { "Invalid input parameters." });
            }

            // Try to get existing cart first
            var existingCart = await GetCartEntityAsync(userId, sessionId);

            if (existingCart != null)
            {
                _logger.LogInformation("Found existing cart {CartId}", existingCart.Id);
                var existingCartDto = MapToCartDto(existingCart);
                return ServiceResult<CartDto>.SuccessResult(existingCartDto);
            }

            // Create new cart
            var newCart = new ShoppingCart
            {
                UserId = userId,
                SessionId = sessionId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Subtotal = 0,
                ItemsCount = 0, // Assuming this property exists
            };

            var createdCart = await _shoppingCartRepo.CreateAsync(newCart);
            
            var cartDto = MapToCartDto(createdCart);
            _logger.LogInformation("Successfully created new cart {CartId}", createdCart.Id);
            
            return ServiceResult<CartDto>.SuccessResult(cartDto, "New cart created successfully.");
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting or creating cart for UserId: {UserId}, SessionId: {SessionId}", userId, sessionId);
            throw;
        }

    }
    public async Task<ServiceResult<bool>> ClearCartAsync(int cartId)
    {
        try
        {
            _logger.LogInformation("Clearing cart {CartId}", cartId);

            // Check if cart exists
            var cart = await _shoppingCartRepo.GetByIdAsync(cartId);
            if (cart == null)
            {
                _logger.LogWarning("Cart {CartId} not found", cartId);
                return ServiceResult<bool>.FailureResult($"Cart {cartId} not found", new List<string>{ "Invalid cart ID." });
            }

            // Delete all cart items
            await _cartItemRepo.DeleteByCartIdAsync(cartId);

            // Reset cart totals
            cart.ItemsCount = 0;
            cart.UpdatedAt = DateTime.UtcNow;

            await _shoppingCartRepo.UpdateAsync(cart);

            _logger.LogInformation("Successfully cleared cart {CartId}", cartId);
            return ServiceResult<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cart {CartId}", cartId);
            return ServiceResult<bool>.FailureResult($"Failed to clear cart {cartId}", new List<string> { ex.Message });
        }

    }
    public async Task<ServiceResult<bool>> DeleteCartAsync(int cartId)
    {
        try
            {
                _logger.LogInformation("Deleting cart {CartId}", cartId);

                // Check if cart exists
                var cart = await _shoppingCartRepo.GetByIdAsync(cartId);
                if (cart == null)
                {
                    _logger.LogWarning("Cart {CartId} not found", cartId);
                    return ServiceResult<bool>.FailureResult($"Cart {cartId} not found", new List<string> { "Invalid cart ID." });
                }

                // Delete cart (should cascade delete items if properly configured)
                await _shoppingCartRepo.DeleteAsync(cartId);

                _logger.LogInformation("Successfully deleted cart {CartId}", cartId);
                return ServiceResult<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting cart {CartId}", cartId);
                return ServiceResult<bool>.FailureResult($"Failed to delete cart {cartId}", new List<string> { ex.Message });
            }
    }

    #endregion

    #region Add/Remove Items (No stock check here!)
    public async Task<ServiceResult<ShoppingCart>> AddToCartAsync(int? userId, string? sessionId, int productId, int quantity, int? variantId = null)
    {
        try
        {
            _logger.LogInformation("Adding product {ProductId} (Variant: {VariantId}) x{Quantity} to cart for UserId: {UserId}, SessionId: {SessionId}", productId, variantId, quantity, userId, sessionId);

            // Validate input
            if (quantity <= 0)
            {
                return ServiceResult<ShoppingCart>.FailureResult("Quantity must be greater than zero.", new List<string> { "Invalid quantity." });
            }
            if (userId == null && string.IsNullOrEmpty(sessionId))
            {
                return ServiceResult<ShoppingCart>.FailureResult("Either userId or sessionId must be provided.", new List<string> { "Invalid input parameters." });
            }

            // Get or create cart
            var cartResult = await GetOrCreateCartAsync(userId, sessionId);
            if (!cartResult.Success || cartResult.Data == null)
            {
                return ServiceResult<ShoppingCart>.FailureResult("Failed to get or create cart.", cartResult.Errors);
            }

            var cart = await _shoppingCartRepo.GetByIdAsync(cartResult.Data.Id);
            if (cart == null)
            {
                return ServiceResult<ShoppingCart>.FailureResult("Cart not found after creation.", new List<string> { "Unexpected error." });
            }

            // Check if item already exists in cart
            var existingItem = await _cartItemRepo.GetCartItemAsync(cart.Id, productId, variantId);
            if (existingItem != null)
            {
                // Update quantity
                existingItem.Quantity += quantity;
                await _cartItemRepo.UpdateAsync(existingItem);
                _logger.LogInformation("Updated quantity of existing item in cart {CartId}", cart.Id);
            }
            else
            {
                // Add new item
                var newItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = productId,
                    VariantId = variantId,
                    Quantity = quantity,
                    AddedAt = DateTime.UtcNow
                };
                await _cartItemRepo.CreateAsync(newItem);
                _logger.LogInformation("Added new item to cart {CartId}", cart.Id);
            }

            // Update cart totals
            cart.ItemsCount += quantity;
            cart.UpdatedAt = DateTime.UtcNow;
            await _shoppingCartRepo.UpdateAsync(cart);

            _logger.LogInformation("Successfully added item to cart {CartId}", cart);
            return ServiceResult<ShoppingCart>.SuccessResult(cart, "Item added to cart successfully.");
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error adding item to cart for UserId: {UserId}, SessionId: {SessionId}", userId, sessionId);
            return ServiceResult<ShoppingCart>.FailureResult("Failed to add item to cart.", new List<string> { ex.Message });
        }

    }
    public async Task<ServiceResult<ShoppingCart>> UpdateQuantityAsync(int cartId, int productId, int newQuantity, int? variantId = null)
    {
        try
        {
            _logger.LogInformation("Updating quantity of product {ProductId} (Variant: {VariantId}) to {NewQuantity} in cart {CartId}", 
                productId, variantId, newQuantity, cartId);

            // Validate input
            if (newQuantity < 0)
            {
                return ServiceResult<ShoppingCart>.FailureResult("Quantity cannot be negative.", 
                    new List<string> { "Invalid quantity." });
            }

            if (cartId <= 0 || productId <= 0)
            {
                return ServiceResult<ShoppingCart>.FailureResult("Invalid cart or product ID.", 
                    new List<string> { "Invalid input parameters." });
            }

            // Get cart
            var cart = await _shoppingCartRepo.GetByIdAsync(cartId);
            if (cart == null)
            {
                return ServiceResult<ShoppingCart>.FailureResult("Cart not found.", 
                    new List<string> { "Cart does not exist." });
            }

            // Find cart item
            var cartItem = await _cartItemRepo.GetCartItemAsync(cartId, productId, variantId);
            if (cartItem == null)
            {
                return ServiceResult<ShoppingCart>.FailureResult("Item not found in cart.", 
                    new List<string> { "Cart item does not exist." });
            }

            var oldQuantity = cartItem.Quantity;

            if (newQuantity == 0)
            {
                // Remove item if quantity is 0
                await _cartItemRepo.DeleteAsync(cartItem.Id);
                cart.ItemsCount -= oldQuantity;
                _logger.LogInformation("Removed item from cart {CartId} (quantity was set to 0)", cartId);
            }
            else
            {
                // Update quantity
                cartItem.Quantity = newQuantity;
                await _cartItemRepo.UpdateAsync(cartItem);
                cart.ItemsCount = cart.ItemsCount - oldQuantity + newQuantity;
                _logger.LogInformation("Updated item quantity in cart {CartId} from {OldQuantity} to {NewQuantity}", 
                    cartId, oldQuantity, newQuantity);
            }

            // Update cart
            cart.UpdatedAt = DateTime.UtcNow;
            await _shoppingCartRepo.UpdateAsync(cart);

            _logger.LogInformation("Successfully updated cart {CartId}", cartId);
            return ServiceResult<ShoppingCart>.SuccessResult(cart, "Cart updated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating quantity in cart {CartId} for product {ProductId}", 
                cartId, productId);
            
            return ServiceResult<ShoppingCart>.FailureResult("An error occurred while updating cart.", 
                new List<string> { ex.Message });
        }
    }
    public async Task<ServiceResult<ShoppingCart>> RemoveItemAsync(int cartId, int productId, int? variantId = null)
    {
        try
        {
            _logger.LogInformation("Removing product {ProductId} (Variant: {VariantId}) from cart {CartId}", 
                productId, variantId, cartId);

            // Validate input
            if (cartId <= 0 || productId <= 0)
            {
                return ServiceResult<ShoppingCart>.FailureResult("Invalid cart or product ID.", 
                    new List<string> { "Invalid input parameters." });
            }

            // Get cart
            var cart = await _shoppingCartRepo.GetByIdAsync(cartId);
            if (cart == null)
            {
                return ServiceResult<ShoppingCart>.FailureResult("Cart not found.", 
                    new List<string> { "Cart does not exist." });
            }

            // Find and remove cart item
            var cartItem = await _cartItemRepo.GetCartItemAsync(cartId, productId, variantId);
            if (cartItem == null)
            {
                return ServiceResult<ShoppingCart>.FailureResult("Item not found in cart.", 
                    new List<string> { "Cart item does not exist." });
            }

            var removedQuantity = cartItem.Quantity;
            await _cartItemRepo.DeleteAsync(cartItem.Id);

            // Update cart totals
            cart.ItemsCount -= removedQuantity;
            cart.UpdatedAt = DateTime.UtcNow;
            await _shoppingCartRepo.UpdateAsync(cart);

            _logger.LogInformation("Successfully removed item from cart {CartId}, quantity: {RemovedQuantity}", 
                cartId, removedQuantity);
            
            return ServiceResult<ShoppingCart>.SuccessResult(cart, "Item removed from cart successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while removing item from cart {CartId}, product {ProductId}", 
                cartId, productId);
            
            return ServiceResult<ShoppingCart>.FailureResult("An error occurred while removing item from cart.", 
                new List<string> { ex.Message });
        }
    }


    

    

    
    #endregion

    #region helper method 

    private CartDto MapToCartDto(ShoppingCart cart)
    {
        return new CartDto
        {
            Id = cart.Id,
            UserId = cart.UserId,
            SessionId = cart.SessionId,
            ItemsCount = cart.ItemsCount, // Assuming this property exists in ShoppingCart
            Subtotal = cart.Subtotal,
            CreatedAt = cart.CreatedAt,
            UpdatedAt = cart.UpdatedAt ?? cart.CreatedAt
        };
    }   
    
    public async Task<ShoppingCart?> GetCartEntityAsync(int? userId, string? sessionId)
    {
        ShoppingCart? cart = null;

        // Priority: User cart first, then session cart
        if (userId.HasValue)
        {
            cart = await _shoppingCartRepo.GetCartWithItemsByUserIdAsync(userId.Value);

            if (cart == null && !string.IsNullOrEmpty(sessionId))
            {
                var sessionCart = await _shoppingCartRepo.GetCartWithItemsBySessionIdAsync(sessionId);
                if (sessionCart != null)
                {
                    _logger.LogInformation("Transferring cart ownership from session {SessionId} to user {UserId}", sessionId, userId);
                    await _shoppingCartRepo.TransferCartOwnershipAsync(sessionId, userId.Value);
                    cart = await _shoppingCartRepo.GetCartWithItemsByUserIdAsync(userId.Value);
                }
            }
        }
        else if (!string.IsNullOrEmpty(sessionId))
        {
            cart = await _shoppingCartRepo.GetCartWithItemsBySessionIdAsync(sessionId);
        }

        return cart;
    }
        
    #endregion

} 