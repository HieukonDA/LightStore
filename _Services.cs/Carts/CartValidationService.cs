namespace TheLightStore.Services.Cart;

public class CartValidationService : ICartValidationService
{
    private readonly ILogger<CartValidationService> _logger;
    private readonly ISavedCartRepo _savedCartRepo;
    private readonly IShoppingCartRepo _shoppingCartRepo;
    private readonly ICartItemRepo _cartItemRepo;
    private readonly IProductVariantRepo _productVariantRepo;
    private readonly IProductRepo _productRepo;
    private readonly ICartService _cartService;

    public CartValidationService(ILogger<CartValidationService> logger, ISavedCartRepo savedCartRepo, IShoppingCartRepo shoppingCartRepo, ICartItemRepo cartItemRepo, IProductVariantRepo productVariantRepo, IProductRepo productRepo, ICartService cartService)
    {
        _logger = logger;
        _savedCartRepo = savedCartRepo;
        _shoppingCartRepo = shoppingCartRepo;
        _cartItemRepo = cartItemRepo;
        _productVariantRepo = productVariantRepo;
        _productRepo = productRepo;
        _cartService = cartService;
    }

    public async Task<ServiceResult<CartValidationResult>> ValidateCartForCheckoutAsync(int cartId)
    {
        try
        {
            _logger.LogInformation("Validating cart {CartId} for checkout", cartId);

            // Validate input
            if (cartId <= 0)
            {
                return ServiceResult<CartValidationResult>.FailureResult("Invalid cart ID.", new List<string> { "Cart ID must be greater than zero." });
            }

            // Get cart
            var cart = await _shoppingCartRepo.GetByIdAsync(cartId);
            if (cart == null)
            {
                return ServiceResult<CartValidationResult>.FailureResult("Cart not found.", new List<string> { "No cart exists with the given ID." });
            }

            // Get cart items with details
            var cartItems = await _cartItemRepo.GetCartItemsWithDetailsAsync(cartId);

            var issues = new List<CartValidationIssue>();

            // Validate each item
            foreach (var item in cartItems)
            {
                if (!item.Product.IsActive)
                {
                    issues.Add(new CartValidationIssue
                    {
                        CartItemId = item.Id,
                        ProductName = item.Product.Name,
                        IssueType = "inactive",
                        Description = $"Product '{item.Product.Name}' is no longer available."
                    });
                }
                else if (item.VariantId != 0 && (!item.Variant.IsActive))
                {
                    issues.Add(new CartValidationIssue
                    {
                        CartItemId = item.Id,
                        ProductName = item.Product.Name,
                        IssueType = "variant_inactive",
                        Description = $"Variant '{item.Variant?.Name}' of product '{item.Product.Name}' is no longer available."
                    });
                }
                else if (item.Quantity > item.Product.StockQuantity)
                {
                    issues.Add(new CartValidationIssue
                    {
                        CartItemId = item.Id,
                        ProductName = item.Product.Name,
                        IssueType = "insufficient_stock",
                        Description = $"Insufficient stock for product '{item.Product.Name}'. Available: {item.Product.StockQuantity}, Requested: {item.Quantity}."
                    });
                }
            }

            var validationResult = new CartValidationResult
            {
                IsValidForCheckout = !issues.Any(),
                Issues = issues
            };

            if (validationResult.IsValidForCheckout)
            {
                _logger.LogInformation("Cart {CartId} is valid for checkout", cartId);
                return ServiceResult<CartValidationResult>.SuccessResult(validationResult);
            }

            _logger.LogWarning("Cart {CartId} has validation issues for checkout", cartId);
            return ServiceResult<CartValidationResult>.FailureResult("Cart has issues preventing checkout.", new List<string> { validationResult.ToString() });
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error validating cart {CartId} for checkout", cartId);
            throw;
        }
    }
    public async Task<ServiceResult<List<CartValidationIssue>>> GetCartIssuesAsync(int cartId)
    {
        try
        {
            var validationResult = await ValidateCartForCheckoutAsync(cartId);
            if (!validationResult.Success)
            {
                return ServiceResult<List<CartValidationIssue>>.FailureResult("Failed to validate cart.", validationResult.Errors);
            }

            return ServiceResult<List<CartValidationIssue>>.SuccessResult(validationResult.Data?.Issues ?? new List<CartValidationIssue>());
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error getting cart issues for cart {CartId}", cartId);
            return ServiceResult<List<CartValidationIssue>>.FailureResult("An error occurred while retrieving cart issues.", new List<string> { ex.Message });
        }

    }

    public async Task<ServiceResult<CartStatisticsDto>> GetCartStatisticsAsync()
    {
        try
        {
            var stats = new CartStatisticsDto
            {
                TotalActiveCarts = await _shoppingCartRepo.CountActiveCartsAsync(),
                TotalAbandonedCarts = await _shoppingCartRepo.CountAbandonedCartsAsync(TimeSpan.FromDays(30)),
                TotalCartsValue = (decimal)await _shoppingCartRepo.GetAverageItemsPerCartAsync(),
                AverageCartValue = (decimal)await _shoppingCartRepo.GetAverageCartValueAsync(),
                TotalCartItems = await _shoppingCartRepo.GetTotalCartItemsAsync(),
                GeneratedAt = DateTime.UtcNow
            };

            _logger.LogInformation("Successfully calculated cart statistics");
            return ServiceResult<CartStatisticsDto>.SuccessResult(stats, "Cart statistics retrieved successfully.");
        }
        catch (System.Exception)
        {
            
            throw;
        }
    }
    public async Task<ServiceResult<int>> CleanupExpiredCartsAsync(int daysOld = 30)
    { 
        try
        {
            _logger.LogInformation("Starting cleanup of expired carts older than {DaysOld} days", daysOld);

            var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);

            // Sử dụng repository method
            var expiredCarts = await _shoppingCartRepo.GetExpiredEmptyCartsAsync(cutoffDate);

            if (!expiredCarts.Any())
            {
                _logger.LogInformation("No expired carts found for cleanup");
                return ServiceResult<int>.SuccessResult(0, "No expired carts found.");
            }

            // Xóa các cart expired
            foreach (var cart in expiredCarts)
            {
                await _shoppingCartRepo.DeleteAsync(cart.Id);
            }

            int deletedCount = expiredCarts.Count;
            _logger.LogInformation("Successfully deleted {DeletedCount} expired carts", deletedCount);

            return ServiceResult<int>.SuccessResult(deletedCount, $"Successfully deleted {deletedCount} expired carts.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during expired carts cleanup");
            return ServiceResult<int>.FailureResult("Failed to cleanup expired carts.", new List<string> { ex.Message });
        }

    }
}