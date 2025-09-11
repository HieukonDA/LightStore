namespace TheLightStore.Services.Cart;

public class CartItemService : ICartItemService
{
    private readonly ILogger<CartItemService> _logger;
    private readonly ISavedCartRepo _savedCartRepo;
    private readonly IShoppingCartRepo _shoppingCartRepo;
    private readonly ICartItemRepo _cartItemRepo;
    private readonly IProductVariantRepo _productVariantRepo;
    private readonly IProductRepo _productRepo;
    private readonly ICartService _cartService;

    public CartItemService(ILogger<CartItemService> logger, ISavedCartRepo savedCartRepo, IShoppingCartRepo shoppingCartRepo, ICartItemRepo cartItemRepo, IProductVariantRepo productVariantRepo, IProductRepo productRepo, ICartService cartService)
    {
        _logger = logger;
        _savedCartRepo = savedCartRepo;
        _shoppingCartRepo = shoppingCartRepo;
        _cartItemRepo = cartItemRepo;
        _productVariantRepo = productVariantRepo;
        _productRepo = productRepo;
        _cartService = cartService;
    }





    /// <summary>
    /// add item to cart
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="sessionId"></param>
    /// <param name="productId"></param>
    /// <param name="quantity"></param>
    /// <param name="variantId"></param>
    /// <returns></returns>
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
            var cartResult = await _cartService.GetOrCreateCartAsync(userId, sessionId);
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
    /// <summary>
    /// update item quantity in cart
    /// </summary>
    /// <param name="cartId"></param>
    /// <param name="productId"></param>
    /// <param name="newQuantity"></param>
    /// <param name="variantId"></param>
    /// <returns></returns>
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
    /// <summary>
    /// remove item from cart
    /// </summary>
    /// <param name="cartId"></param>
    /// <param name="productId"></param>
    /// <param name="variantId"></param>
    /// <returns></returns>
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
    /// <summary>
    /// add multiple items to cart
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="sessionId"></param>
    /// <param name="items"></param>
    /// <returns></returns>
    public async Task<ServiceResult<ShoppingCart>> AddMultipleItemsAsync(int? userId, string? sessionId, List<AddToCartRequest> items)
    {
        try
        {
            _logger.LogInformation("Adding {ItemCount} items to cart for UserId: {UserId}, SessionId: {SessionId}",
                items?.Count ?? 0, userId, sessionId);

            // Validate input
            if (items == null || !items.Any())
            {
                return ServiceResult<ShoppingCart>.FailureResult("No items provided.",
                    new List<string> { "Items list is empty." });
            }

            if (userId == null && string.IsNullOrEmpty(sessionId))
            {
                return ServiceResult<ShoppingCart>.FailureResult("Either userId or sessionId must be provided.",
                    new List<string> { "Invalid input parameters." });
            }

            // Validate each item
            var errors = new List<string>();
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item.ProductId <= 0)
                    errors.Add($"Item {i + 1}: Invalid product ID.");
                if (item.Quantity <= 0)
                    errors.Add($"Item {i + 1}: Quantity must be greater than zero.");
            }

            if (errors.Any())
            {
                return ServiceResult<ShoppingCart>.FailureResult("Invalid items in request.", errors);
            }

            // Get or create cart
            var cartResult = await _cartService.GetOrCreateCartAsync(userId, sessionId);
            if (!cartResult.Success || cartResult.Data == null)
            {
                return ServiceResult<ShoppingCart>.FailureResult("Failed to get or create cart.", cartResult.Errors);
            }

            var cart = await _shoppingCartRepo.GetByIdAsync(cartResult.Data.Id);
            if (cart == null)
            {
                return ServiceResult<ShoppingCart>.FailureResult("Cart not found after creation.",
                    new List<string> { "Unexpected error." });
            }

            var totalQuantityAdded = 0;
            var itemsProcessed = 0;

            // Process each item
            foreach (var item in items)
            {
                try
                {
                    // Check if item already exists in cart
                    var existingItem = await _cartItemRepo.GetCartItemAsync(cart.Id, item.ProductId, item.VariantId);
                    if (existingItem != null)
                    {
                        // Update quantity
                        existingItem.Quantity += item.Quantity;
                        await _cartItemRepo.UpdateAsync(existingItem);
                    }
                    else
                    {
                        // Add new item
                        var newItem = new CartItem
                        {
                            CartId = cart.Id,
                            ProductId = item.ProductId,
                            VariantId = item.VariantId,
                            Quantity = item.Quantity,
                            AddedAt = DateTime.UtcNow
                        };
                        await _cartItemRepo.CreateAsync(newItem);
                    }

                    totalQuantityAdded += item.Quantity;
                    itemsProcessed++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to add item {ProductId} (Variant: {VariantId}) to cart {CartId}",
                        item.ProductId, item.VariantId, cart.Id);
                    errors.Add($"Failed to add product {item.ProductId}: {ex.Message}");
                }
            }

            // Update cart totals
            if (totalQuantityAdded > 0)
            {
                cart.ItemsCount += totalQuantityAdded;
                cart.UpdatedAt = DateTime.UtcNow;
                await _shoppingCartRepo.UpdateAsync(cart);
            }

            var message = errors.Any()
                ? $"Added {itemsProcessed} of {items.Count} items to cart. Some items failed."
                : $"Successfully added all {itemsProcessed} items to cart.";

            _logger.LogInformation("Completed adding multiple items to cart {CartId}. Items processed: {ItemsProcessed}/{TotalItems}, Total quantity added: {TotalQuantity}",
                cart.Id, itemsProcessed, items.Count, totalQuantityAdded);

            if (errors.Any() && itemsProcessed == 0)
            {
                return ServiceResult<ShoppingCart>.FailureResult("Failed to add any items to cart.", errors);
            }

            return errors.Any()
                ? ServiceResult<ShoppingCart>.FailureResult(message, errors)
                : ServiceResult<ShoppingCart>.SuccessResult(cart, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding multiple items to cart for UserId: {UserId}, SessionId: {SessionId}",
                userId, sessionId);

            return ServiceResult<ShoppingCart>.FailureResult("An error occurred while adding items to cart.",
                new List<string> { ex.Message });
        }
    }

    // Cart Info

    /// <summary>
    /// get cart summary
    /// </summary>
    /// <param name="cartId"></param>
    /// <returns></returns>
    public async Task<ServiceResult<CartSummaryDto>> GetCartSummaryAsync(int cartId)
    {
        try
        {
            _logger.LogInformation("Fetching summary for cart {CartId}", cartId);

            // Validate input
            if (cartId <= 0)
            {
                return ServiceResult<CartSummaryDto>.FailureResult("Invalid cart ID.", new List<string> { "Cart ID must be greater than zero." });
            }

            // Get cart
            var cart = await _shoppingCartRepo.GetByIdAsync(cartId);
            if (cart == null)
            {
                return ServiceResult<CartSummaryDto>.FailureResult("Cart not found.", new List<string> { "No cart exists with the given ID." });
            }

            // Get cart items
            var cartItems = await _cartItemRepo.GetByCartIdAsync(cartId);

            // Calculate summary
            var itemsCount = cartItems.Sum(item => item.Quantity);
            var subtotal = cartItems.Sum(item => item.Quantity * item.Product.BasePrice); // Assuming Product navigation property is loaded

            var summary = new CartSummaryDto
            {
                CartId = cart.Id,
                ItemsCount = itemsCount,
                Subtotal = subtotal,
                LastUpdated = cart.UpdatedAt
            };

            _logger.LogInformation("Successfully fetched summary for cart {CartId}", cartId);
            return ServiceResult<CartSummaryDto>.SuccessResult(summary, "Cart summary retrieved successfully.");
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error fetching summary for cart {CartId}", cartId);
            return ServiceResult<CartSummaryDto>.FailureResult("An error occurred while fetching cart summary.", new List<string> { ex.Message });
        }

    }

    /// <summary>
    /// get cart items
    /// </summary>
    /// <param name="cartId"></param>
    /// <returns></returns>
    public async Task<ServiceResult<List<CartItemDto>>> GetCartItemsAsync(int cartId)
    {
        try
        {
            _logger.LogInformation("Fetching items for cart {CartId}", cartId);

            // Validate input
            if (cartId <= 0)
            {
                return ServiceResult<List<CartItemDto>>.FailureResult("Invalid cart ID.", new List<string> { "Cart ID must be greater than zero." });
            }

            // Get cart
            var cart = await _shoppingCartRepo.GetByIdAsync(cartId);
            if (cart == null)
            {
                return ServiceResult<List<CartItemDto>>.FailureResult("Cart not found.", new List<string> { "No cart exists with the given ID." });
            }

            // Get cart items with details
            var cartItems = await _cartItemRepo.GetCartItemsWithDetailsAsync(cartId);

            // Map to DTOs
            var cartItemDtos = cartItems.Select(item => new CartItemDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductName = item.Product.Name,
                ProductSku = item.Product.Sku,
                VariantId = item.VariantId,
                VariantName = item.Variant != null ? item.Variant.Name : null,
                Quantity = item.Quantity,
                UnitPrice = item.Product.BasePrice, // Assuming Product navigation property is loaded
                TotalPrice = item.Quantity * item.Product.BasePrice,
                IsAvailable = item.Product.IsActive, // Simplified availability check
                AddedAt = item.AddedAt
            }).ToList();

            _logger.LogInformation("Successfully fetched {ItemCount} items for cart {CartId}", cartItemDtos.Count, cartId);
            return ServiceResult<List<CartItemDto>>.SuccessResult(cartItemDtos, "Cart items retrieved successfully.");
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error fetching items for cart {CartId}", cartId);
            return ServiceResult<List<CartItemDto>>.FailureResult("An error occurred while fetching cart items.", new List<string> { ex.Message });
        }

    }
    /// <summary>
    /// get cart items count
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public async Task<ServiceResult<int>> GetCartItemsCountAsync(int? userId, string? sessionId)
    {
        try
        {
            _logger.LogInformation("Counting items in cart for UserId: {UserId}, SessionId: {SessionId}", userId, sessionId);

            // Validate input
            if (userId == null && string.IsNullOrEmpty(sessionId))
            {
                return ServiceResult<int>.FailureResult("Either userId or sessionId must be provided.", new List<string> { "Invalid input parameters." });
            }

            // Get cart
            var cart = await _cartService.GetCartEntityAsync(userId, sessionId);
            if (cart == null)
            {
                return ServiceResult<int>.SuccessResult(0, "No cart found; item count is 0.");
            }

            // Get item count
            var itemCount = await _cartItemRepo.GetByCartIdAsync(cart.Id);
            var totalItems = itemCount.Sum(i => i.Quantity);

            _logger.LogInformation("Cart {CartId} has {ItemCount} items", cart.Id, totalItems);
            return ServiceResult<int>.SuccessResult(totalItems, "Cart item count retrieved successfully.");
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error counting items in cart for UserId: {UserId}, SessionId: {SessionId}", userId, sessionId);
            return ServiceResult<int>.FailureResult("An error occurred while counting cart items.", new List<string> { ex.Message });
        }

    }
}
