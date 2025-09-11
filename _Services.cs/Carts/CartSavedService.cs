namespace TheLightStore.Services.Cart;

public class CartSavedService : ICartSaveService
{

    private readonly ILogger<CartItemService> _logger;
    private readonly ISavedCartRepo _savedCartRepo;
    private readonly IShoppingCartRepo _shoppingCartRepo;
    private readonly ICartItemRepo _cartItemRepo;
    private readonly IProductVariantRepo _productVariantRepo;
    private readonly IProductRepo _productRepo;
    private readonly ICartService _cartService;

    public CartSavedService(
        ILogger<CartItemService> logger,
        ISavedCartRepo savedCartRepo,
        IShoppingCartRepo shoppingCartRepo,
        ICartItemRepo cartItemRepo,
        IProductVariantRepo productVariantRepo,
        IProductRepo productRepo,
        ICartService cartService
    )
    {
        _logger = logger;
        _savedCartRepo = savedCartRepo;
        _shoppingCartRepo = shoppingCartRepo;
        _cartItemRepo = cartItemRepo;
        _productVariantRepo = productVariantRepo;
        _productRepo = productRepo;
        _cartService = cartService;
    }


    // Save for Later
    public async Task<ServiceResult<bool>> SaveCartForLaterAsync(int cartId, int userId, string? cartName = null)
    {
        try
        {
            _logger.LogInformation("Saving cart {CartId} for user {UserId} with name '{CartName}'", cartId, userId, cartName);

            // Validate input
            if (cartId <= 0 || userId <= 0)
            {
                return ServiceResult<bool>.FailureResult("Invalid cart ID or user ID.", new List<string> { "Cart ID and User ID must be greater than zero." });
            }

            // Get cart
            var cart = await _shoppingCartRepo.GetByIdAsync(cartId);
            if (cart == null || cart.UserId != userId)
            {
                return ServiceResult<bool>.FailureResult("Cart not found or does not belong to the user.", new List<string> { "Invalid cart ID or user mismatch." });
            }

            // Create saved cart entry
            var savedCart = new SavedCart
            {
                UserId = userId,
                CartData = System.Text.Json.JsonSerializer.Serialize(cart), // Simplified; consider deep copy or DTO
                CartName = string.IsNullOrWhiteSpace(cartName) ? $"Saved Cart {DateTime.UtcNow:yyyy-MM-dd HH:mm}" : cartName,
                CreatedAt = DateTime.UtcNow
            };

            await _savedCartRepo.CreateAsync(savedCart);

            _logger.LogInformation("Successfully saved cart {CartId} for user {UserId}", cartId, userId);
            return ServiceResult<bool>.SuccessResult(true, "Cart saved for later successfully.");
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error saving cart {CartId} for user {UserId}", cartId, userId);
            return ServiceResult<bool>.FailureResult("An error occurred while saving the cart.", new List<string> { ex.Message });
        }
    }
    public async Task<ServiceResult<List<SavedCartDto>>> GetSavedCartsAsync(int userId)
    {
        try
        {
            _logger.LogInformation("Fetching saved carts for user {UserId}", userId);

            // Validate input
            if (userId <= 0)
            {
                return ServiceResult<List<SavedCartDto>>.FailureResult("Invalid user ID.", new List<string> { "User ID must be greater than zero." });
            }

            // Get saved carts
            var savedCarts = await _savedCartRepo.GetByUserIdAsync(userId);
            var savedCartDtos = savedCarts.Select(sc => new SavedCartDto
            {
                Id = sc.Id,
                CartName = sc.CartName,
                CreatedAt = sc.CreatedAt,
                ItemsCount = 0, // Placeholder; deserialize CartData to count items if needed
                PreviewItems = JsonSerializer.Deserialize<List<SavedCartItemDto>>(sc.CartData)


            }).ToList();

            _logger.LogInformation("Successfully fetched {SavedCartCount} saved carts for user {UserId}", savedCartDtos.Count, userId);
            return ServiceResult<List<SavedCartDto>>.SuccessResult(savedCartDtos, "Saved carts retrieved successfully.");
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error fetching saved carts for user {UserId}", userId);
            return ServiceResult<List<SavedCartDto>>.FailureResult("An error occurred while fetching saved carts.", new List<string> { ex.Message });
        }
    }
    public async Task<ServiceResult<bool>> RestoreSavedCartAsync(int savedCartId, int userId)
    {
        try
        {
            _logger.LogInformation("Restoring saved cart {SavedCartId} for user {UserId}", savedCartId, userId);

            // Validate input
            if (savedCartId <= 0 || userId <= 0)
            {
                return ServiceResult<bool>.FailureResult("Invalid saved cart ID or user ID.", new List<string> { "IDs must be greater than zero." });
            }

            // Get saved cart
            var savedCart = await _savedCartRepo.GetByIdAsync(savedCartId);

            if (savedCart == null || savedCart.UserId != userId)
            {
                return ServiceResult<bool>.FailureResult("Saved cart not found or does not belong to the user.", new List<string> { "Invalid saved cart ID or user mismatch." });
            }

            // Deserialize cart data
            var cart = JsonSerializer.Deserialize<List<SavedCartItemDto>>(savedCart.CartData);

            if (cart == null)
            {
                return ServiceResult<bool>.FailureResult("Failed to restore cart.", new List<string> { "Invalid cart data." });
            }

            // Get or create user's current shopping cart
            var currentCart = await _shoppingCartRepo.GetByUserIdAsync(userId);
            if (currentCart == null)
            {
                currentCart = new ShoppingCart
                {
                    UserId = userId,
                    SessionId = null,
                    ItemsCount = 0,
                    Subtotal = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                currentCart = await _shoppingCartRepo.CreateAsync(currentCart);
                _logger.LogInformation("Đã tạo cart mới {CartId} cho user {UserId}", currentCart.Id, userId);
            }

            await _cartItemRepo.DeleteByCartIdAsync(currentCart.Id);
            _logger.LogInformation("Đã xóa tất cả items cũ trong cart {CartId}", currentCart.Id);

            // Restore items to the current cart
            int restoredItemsCount = 0;
            var skippedItems = new List<string>();

            foreach (var savedItem in cart)
            {
                try
                {
                    // Kiểm tra product còn tồn tại không
                    var product = await _productRepo.GetByIdAsync(savedItem.ProductId);
                    if (product == null)
                    {
                        skippedItems.Add($"Product ID {savedItem.ProductId} không còn tồn tại");
                        continue;
                    }

                    // Xác định giá sử dụng
                    decimal unitPrice = product.BasePrice;
                    if (savedItem.VariantId.HasValue)
                    {
                        var variant = await _productVariantRepo.GetByIdAsync(savedItem.VariantId.Value);
                        if (variant != null)
                        {
                            if (variant.Price > 0) unitPrice = variant.Price;

                        }
                        else
                        {
                            skippedItems.Add($"Variant ID {savedItem.VariantId} không còn tồn tại, sử dụng giá gốc của sản phẩm");
                        }
                    }

                    // Tạo CartItem mới
                    var cartItem = new CartItem
                    {
                        CartId = currentCart.Id,
                        ProductId = savedItem.ProductId,
                        VariantId = savedItem.VariantId,
                        Quantity = savedItem.Quantity,
                        UnitPrice = unitPrice,
                        AddedAt = DateTime.UtcNow
                    };

                    await _cartItemRepo.CreateAsync(cartItem);
                    restoredItemsCount++;

                    _logger.LogDebug("Đã restore item: ProductId={ProductId}, VariantId={VariantId}, Quantity={Quantity}",
                        savedItem.ProductId, savedItem.VariantId, savedItem.Quantity);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi restore item ProductId={ProductId}, VariantId={VariantId}",
                        savedItem.ProductId, savedItem.VariantId);
                    skippedItems.Add($"Product ID {savedItem.ProductId}: {ex.Message}");
                }

                currentCart.ItemsCount = restoredItemsCount;
                currentCart.UpdatedAt = DateTime.UtcNow;
                await _shoppingCartRepo.UpdateAsync(currentCart);

            }

            var resultMessage = $"Restored {restoredItemsCount} items.";
            if (skippedItems.Any())
            {
                resultMessage += $" Skipped: {string.Join("; ", skippedItems)}";
            }

            return ServiceResult<bool>.SuccessResult(true, resultMessage);
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error restoring saved cart {SavedCartId} for user {UserId}", savedCartId, userId);
            return ServiceResult<bool>.FailureResult("An error occurred while restoring the cart.", new List<string> { ex.Message });
        }

    }
    public async Task<ServiceResult<bool>> DeleteSavedCartAsync(int savedCartId, int userId)
    {
        try
        {
            _logger.LogInformation("Deleting saved cart {SavedCartId} for user {UserId}", savedCartId, userId);

            // Validate input
            if (savedCartId <= 0 || userId <= 0)
            {
                return ServiceResult<bool>.FailureResult("Invalid saved cart ID or user ID.", new List<string> { "IDs must be greater than zero." });
            }

            // Get saved cart
            var savedCart = await _savedCartRepo.GetByIdAsync(savedCartId);
            if (savedCart == null || savedCart.UserId != userId)
            {
                return ServiceResult<bool>.FailureResult("Saved cart not found or does not belong to the user.", new List<string> { "Invalid saved cart ID or user mismatch." });
            }

            // Delete saved cart
            await _savedCartRepo.DeleteAsync(savedCartId);

            _logger.LogInformation("Successfully deleted saved cart {SavedCartId} for user {UserId}", savedCartId, userId);
            return ServiceResult<bool>.SuccessResult(true, "Saved cart deleted successfully.");
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error deleting saved cart {SavedCartId} for user {UserId}", savedCartId, userId);
            return ServiceResult<bool>.FailureResult("An error occurred while deleting the saved cart.", new List<string> { ex.Message });
        }
    }

    
    
        // Transfer & Merge
    public async Task<ServiceResult<bool>> TransferGuestCartToUserAsync(string sessionId, int userId)
    {
        try
        {
            _logger.LogInformation("Transferring cart from SessionId: {SessionId} to UserId: {UserId}", sessionId, userId);

            // Validate input
            if (string.IsNullOrEmpty(sessionId) || userId <= 0)
            {
                return ServiceResult<bool>.FailureResult("Invalid session ID or user ID.", new List<string> { "Invalid input parameters." });
            }

            // Transfer cart ownership
            var transferResult = await _shoppingCartRepo.TransferCartOwnershipAsync(sessionId, userId);
            if (!transferResult)
            {
                return ServiceResult<bool>.FailureResult("No cart found for the given session ID.", new List<string> { "Transfer failed." });
            }

            _logger.LogInformation("Successfully transferred cart from SessionId: {SessionId} to UserId: {UserId}", sessionId, userId);
            return ServiceResult<bool>.SuccessResult(true, "Cart transferred successfully.");
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error transferring cart from SessionId: {SessionId} to UserId: {UserId}", sessionId, userId);
            return ServiceResult<bool>.FailureResult("An error occurred while transferring the cart.", new List<string> { ex.Message });
        }

    }

    public async Task<ServiceResult<bool>> MergeCartsAsync(int targetCartId, int sourceCartId)
    {
        try
        {
            _logger.LogInformation("Merging cart {SourceCartId} into cart {TargetCartId}", sourceCartId, targetCartId);
            // Validate input
            if (targetCartId <= 0 || sourceCartId <= 0 || targetCartId == sourceCartId)
            {
                return ServiceResult<bool>.FailureResult("Invalid cart IDs provided.", new List<string> { "Cart IDs must be greater than zero and not the same." });
            }

            // Get both carts
            var targetCart = await _shoppingCartRepo.GetByIdAsync(targetCartId);

            var sourceCart = await _shoppingCartRepo.GetByIdAsync(sourceCartId);
            if (targetCart == null || sourceCart == null)
            {
                return ServiceResult<bool>.FailureResult("One or both carts not found.", new List<string> { "Invalid cart IDs." });
            }

            // Get items from source cart
            var sourceItems = await _cartItemRepo.GetByCartIdAsync(sourceCartId);

            // Merge items into target cart
            foreach (var item in sourceItems)
            {
                var existingItem = await _cartItemRepo.GetCartItemAsync(targetCartId, item.ProductId, item.VariantId);
                if (existingItem != null)
                {
                    // Update quantity if item exists
                    existingItem.Quantity += item.Quantity;
                    await _cartItemRepo.UpdateAsync(existingItem);
                }
                else
                {
                    // Reassign item to target cart
                    item.CartId = targetCartId;
                    await _cartItemRepo.UpdateAsync(item);
                }
            }

            _logger.LogInformation("Successfully merged cart {SourceCartId} into cart {TargetCartId}", sourceCartId, targetCartId);
            return ServiceResult<bool>.SuccessResult(true, "Carts merged successfully.");
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error merging cart {SourceCartId} into cart {TargetCartId}", sourceCartId, targetCartId);
            return ServiceResult<bool>.FailureResult("An error occurred while merging carts.", new List<string> { ex.Message });
        }
    }

}