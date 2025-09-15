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
                _logger.LogWarning("No cart found for UserId: {UserId}, SessionId: {SessionId}", userId, sessionId);
                return ServiceResult<CartDto?>.FailureResult("cart not found.", new List<string> { "No cart found for the given userId or sessionId." });
            }

            //map to dto
            var cartDto = CartMapper.MapToDto(cart);
            _logger.LogInformation("Cart retrieved successfully for CartId: {CartId}", cartDto.Id);
            return ServiceResult<CartDto?>.SuccessResult(cartDto, "Cart retrieved successfully.");

        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching the cart: {Message}", ex.Message);
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
                var existingCartDto = CartMapper.MapToDto(existingCart);
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

            var cartDto = CartMapper.MapToDto(createdCart);
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
                return ServiceResult<bool>.FailureResult($"Cart {cartId} not found", new List<string> { "Invalid cart ID." });
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

    #region helper method 
  

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