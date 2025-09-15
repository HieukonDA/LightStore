namespace TheLightStore.Interfaces.Cart;

public interface ICartService
{
    Task<ServiceResult<CartDto?>> GetCartAsync(int? userId, string? sessionId);
    Task<ServiceResult<CartDto>> GetOrCreateCartAsync(int? userId, string? sessionId);
    Task<ServiceResult<bool>> ClearCartAsync(int cartId);
    Task<ServiceResult<bool>> DeleteCartAsync(int cartId);
    Task<ShoppingCart?> GetCartEntityAsync(int? userId, string? sessionId);
}

public interface ICartItemService
{
    Task<ServiceResult<CartDto>> AddToCartAsync(int? userId, string? sessionId, int productId, int quantity, int? variantId = null);
    Task<ServiceResult<CartDto>> UpdateQuantityAsync(int cartId, int productId, int newQuantity, int? variantId = null);
    Task<ServiceResult<CartDto>> RemoveItemAsync(int cartId, int productId, int? variantId = null);
    Task<ServiceResult<CartDto>> AddMultipleItemsAsync(int? userId, string? sessionId, List<AddToCartRequest> items);

    Task<ServiceResult<CartSummaryDto>> GetCartSummaryAsync(int cartId);
    Task<ServiceResult<List<CartItemDto>>> GetCartItemsAsync(int cartId);
    Task<ServiceResult<int>> GetCartItemsCountAsync(int? userId, string? sessionId);
}

public interface ICartSaveService
{
    Task<ServiceResult<bool>> SaveCartForLaterAsync(int cartId, int userId, string? cartName = null);
    Task<ServiceResult<List<SavedCartDto>>> GetSavedCartsAsync(int userId);
    Task<ServiceResult<bool>> RestoreSavedCartAsync(int savedCartId, int userId);
    Task<ServiceResult<bool>> DeleteSavedCartAsync(int savedCartId, int userId);
    Task<ServiceResult<bool>> MergeCartsAsync(int targetCartId, int sourceCartId);
    Task<ServiceResult<bool>> TransferGuestCartToUserAsync(string sessionId, int userId);
}

public interface ICartValidationService
{
    Task<ServiceResult<CartValidationResult>> ValidateCartForCheckoutAsync(int cartId);
    Task<ServiceResult<List<CartValidationIssue>>> GetCartIssuesAsync(int cartId);
    Task<ServiceResult<CartStatisticsDto>> GetCartStatisticsAsync();
    Task<ServiceResult<int>> CleanupExpiredCartsAsync(int daysOld = 30);
}



 