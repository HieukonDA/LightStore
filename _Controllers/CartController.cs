namespace TheLightStore.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class CartController : ControllerBase
{
    private readonly ILogger<CartController> _logger;
    private readonly ICartService _cartService;
    private readonly ICartItemService _cartItemService;
    private readonly ICartSaveService _cartSaveService;
    private readonly ICartValidationService _cartValidationService;

    public CartController(
        ILogger<CartController> logger,
        ICartService cartService,
        ICartItemService cartItemService,
        ICartSaveService cartSaveService,
        ICartValidationService cartValidationService)
    {
        _logger = logger;
        _cartService = cartService;
        _cartItemService = cartItemService;
        _cartSaveService = cartSaveService;
        _cartValidationService = cartValidationService;
    }

    #region Cart Management

    /// <summary>
    /// Get current cart for user or session
    /// </summary>
    /// <returns>Cart details</returns>
    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        try
        {
            // var (userId, sessionId) = GetUserAndSessionInfo();

            int? userId = GetUserId();
            string? sessionId = Request.Cookies["SessionId"];

            // Debug logs
            _logger.LogInformation("=== DEBUG GetCart ===");
            _logger.LogInformation("UserId: {UserId}", userId);
            _logger.LogInformation("SessionId: {SessionId}", sessionId);
            _logger.LogInformation("Session.Id: {SessionContextId}", HttpContext.Session.Id);
            _logger.LogInformation("User.Identity.IsAuthenticated: {IsAuth}", User.Identity.IsAuthenticated);

            if (string.IsNullOrEmpty(sessionId))
            {
                sessionId = Guid.NewGuid().ToString();
                Response.Cookies.Append("SessionId", sessionId, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });
            }

            var result = await _cartService.GetCartAsync(userId, sessionId);

            if (!result.Success)
            {
                return BadRequest(new { success = false, message = result.Message, errors = result.Errors });
            }

            return Ok(new { success = true, data = result.Data, message = result.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cart");
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get or create cart for user or session
    /// </summary>
    /// <returns>Cart details</returns>
    [HttpPost("get-or-create")]
    public async Task<IActionResult> GetOrCreateCart()
    {
        try
        {
            var (userId, sessionId) = GetUserAndSessionInfo();
            var result = await _cartService.GetOrCreateCartAsync(userId, sessionId);

            if (!result.Success)
            {
                return BadRequest(new { success = false, message = result.Message, errors = result.Errors });
            }

            return Ok(new { success = true, data = result.Data, message = result.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting or creating cart");
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }

    /// <summary>
    /// Clear all items from cart
    /// </summary>
    /// <param name="cartId">Cart ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{cartId}/clear")]
    public async Task<IActionResult> ClearCart(int cartId)
    {
        try
        {
            var result = await _cartService.ClearCartAsync(cartId);

            if (!result.Success)
            {
                return BadRequest(new { success = false, message = result.Message, errors = result.Errors });
            }

            return Ok(new { success = true, message = result.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cart {CartId}", cartId);
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }

    /// <summary>
    /// Delete entire cart
    /// </summary>
    /// <param name="cartId">Cart ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{cartId}")]
    public async Task<IActionResult> DeleteCart(int cartId)
    {
        try
        {
            var result = await _cartService.DeleteCartAsync(cartId);

            if (!result.Success)
            {
                return BadRequest(new { success = false, message = result.Message, errors = result.Errors });
            }

            return Ok(new { success = true, message = result.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting cart {CartId}", cartId);
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }

    #endregion

    #region Cart Items Management

    /// <summary>
    /// Add item to cart
    /// </summary>
    /// <param name="request">Add to cart request</param>
    /// <returns>Updated cart</returns>
    [HttpPost("items")]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid request", errors = ModelState });
            }

            var (userId, sessionId) = GetUserAndSessionInfo();
            _logger.LogInformation("Adding to cart - UserId: {UserId}, SessionId: {SessionId}", userId, sessionId);
            var result = await _cartItemService.AddToCartAsync(userId, sessionId, request.ProductId, request.Quantity, request.VariantId);

            if (!result.Success)
            {
                return BadRequest(new { success = false, message = result.Message, errors = result.Errors });
            }

            return Ok(new { success = true, data = result.Data, message = result.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding item to cart");
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }

    /// <summary>
    /// Add multiple items to cart at once
    /// </summary>
    /// <param name="request">Multiple items request</param>
    /// <returns>Updated cart</returns>
    [HttpPost("items/bulk")]
    public async Task<IActionResult> AddMultipleItems([FromBody] AddMultipleItemsRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid request", errors = ModelState });
            }

            var (userId, sessionId) = GetUserAndSessionInfo();
            var result = await _cartItemService.AddMultipleItemsAsync(userId, sessionId, request.Items);

            if (!result.Success)
            {
                return BadRequest(new { success = false, message = result.Message, errors = result.Errors });
            }

            return Ok(new { success = true, data = result.Data, message = result.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding multiple items to cart");
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }

    /// <summary>
    /// Update item quantity in cart
    /// </summary>
    /// <param name="cartId">Cart ID</param>
    /// <param name="request">Update quantity request</param>
    /// <returns>Updated cart</returns>
    [HttpPut("{cartId}/items")]
    public async Task<IActionResult> UpdateQuantity(int cartId, [FromBody] UpdateQuantityRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid request", errors = ModelState });
            }

            var result = await _cartItemService.UpdateQuantityAsync(cartId, request.ProductId, request.NewQuantity, request.VariantId);

            if (!result.Success)
            {
                return BadRequest(new { success = false, message = result.Message, errors = result.Errors });
            }

            return Ok(new { success = true, data = result.Data, message = result.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating item quantity in cart {CartId}", cartId);
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }

    /// <summary>
    /// Remove item from cart
    /// </summary>
    /// <param name="cartId">Cart ID</param>
    /// <param name="productId">Product ID</param>
    /// <param name="variantId">Optional variant ID</param>
    /// <returns>Updated cart</returns>
    [HttpDelete("{cartId}/items/{productId}")]
    public async Task<IActionResult> RemoveItem(int cartId, int productId, [FromQuery] int? variantId = null)
    {
        try
        {
            var result = await _cartItemService.RemoveItemAsync(cartId, productId, variantId);

            if (!result.Success)
            {
                return BadRequest(new { success = false, message = result.Message, errors = result.Errors });
            }

            return Ok(new { success = true, data = result.Data, message = result.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing item from cart {CartId}", cartId);
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get cart items
    /// </summary>
    /// <param name="cartId">Cart ID</param>
    /// <returns>List of cart items</returns>
    [HttpGet("{cartId}/items")]
    public async Task<IActionResult> GetCartItems(int cartId)
    {
        try
        {
            var result = await _cartItemService.GetCartItemsAsync(cartId);

            if (!result.Success)
            {
                return BadRequest(new { success = false, message = result.Message, errors = result.Errors });
            }

            return Ok(new { success = true, data = result.Data, message = result.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cart items for cart {CartId}", cartId);
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get cart items count
    /// </summary>
    /// <returns>Number of items in cart</returns>
    [HttpGet("items/count")]
    public async Task<IActionResult> GetCartItemsCount()
    {
        try
        {
            var (userId, sessionId) = GetUserAndSessionInfo();
            var result = await _cartItemService.GetCartItemsCountAsync(userId, sessionId);

            if (!result.Success)
            {
                return BadRequest(new { success = false, message = result.Message, errors = result.Errors });
            }

            return Ok(new { success = true, data = result.Data, message = result.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cart items count");
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get cart summary
    /// </summary>
    /// <param name="cartId">Cart ID</param>
    /// <returns>Cart summary</returns>
    [HttpGet("{cartId}/summary")]
    public async Task<IActionResult> GetCartSummary(int cartId)
    {
        try
        {
            var result = await _cartItemService.GetCartSummaryAsync(cartId);

            if (!result.Success)
            {
                return BadRequest(new { success = false, message = result.Message, errors = result.Errors });
            }

            return Ok(new { success = true, data = result.Data, message = result.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cart summary for cart {CartId}", cartId);
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }

    #endregion

    #region Saved Carts

    /// <summary>
    /// Save cart for later
    /// </summary>
    /// <param name="cartId">Cart ID</param>
    /// <param name="request">Save cart request</param>
    /// <returns>Success status</returns>
    [HttpPost("{cartId}/save")]
    [Authorize]
    public async Task<IActionResult> SaveCartForLater(int cartId, [FromBody] SaveCartRequest request)
    {
        try
        {
            var userId = GetUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(new { success = false, message = "User not authenticated" });
            }

            var result = await _cartSaveService.SaveCartForLaterAsync(cartId, userId.Value, request?.CartName);

            if (!result.Success)
            {
                return BadRequest(new { success = false, message = result.Message, errors = result.Errors });
            }

            return Ok(new { success = true, message = result.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving cart {CartId} for later", cartId);
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get saved carts for user
    /// </summary>
    /// <returns>List of saved carts</returns>
    [HttpGet("saved")]
    [Authorize]
    public async Task<IActionResult> GetSavedCarts()
    {
        try
        {
            var userId = GetUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(new { success = false, message = "User not authenticated" });
            }

            var result = await _cartSaveService.GetSavedCartsAsync(userId.Value);

            if (!result.Success)
            {
                return BadRequest(new { success = false, message = result.Message, errors = result.Errors });
            }

            return Ok(new { success = true, data = result.Data, message = result.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting saved carts");
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }

    /// <summary>
    /// Restore saved cart
    /// </summary>
    /// <param name="savedCartId">Saved cart ID</param>
    /// <returns>Success status</returns>
    [HttpPost("saved/{savedCartId}/restore")]
    [Authorize]
    public async Task<IActionResult> RestoreSavedCart(int savedCartId)
    {
        try
        {
            var userId = GetUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(new { success = false, message = "User not authenticated" });
            }

            var result = await _cartSaveService.RestoreSavedCartAsync(savedCartId, userId.Value);

            if (!result.Success)
            {
                return BadRequest(new { success = false, message = result.Message, errors = result.Errors });
            }

            return Ok(new { success = true, message = result.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring saved cart {SavedCartId}", savedCartId);
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }

    /// <summary>
    /// Delete saved cart
    /// </summary>
    /// <param name="savedCartId">Saved cart ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("saved/{savedCartId}")]
    [Authorize]
    public async Task<IActionResult> DeleteSavedCart(int savedCartId)
    {
        try
        {
            var userId = GetUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(new { success = false, message = "User not authenticated" });
            }

            var result = await _cartSaveService.DeleteSavedCartAsync(savedCartId, userId.Value);

            if (!result.Success)
            {
                return BadRequest(new { success = false, message = result.Message, errors = result.Errors });
            }

            return Ok(new { success = true, message = result.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting saved cart {SavedCartId}", savedCartId);
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }

    #endregion

    #region Cart Transfer & Merge

    /// <summary>
    /// Transfer guest cart to authenticated user
    /// </summary>
    /// <param name="request">Transfer cart request</param>
    /// <returns>Success status</returns>
    [HttpPost("transfer")]
    [Authorize]
    public async Task<IActionResult> TransferGuestCart([FromBody] TransferCartRequest request)
    {
        try
        {
            var userId = GetUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(new { success = false, message = "User not authenticated" });
            }

            var result = await _cartSaveService.TransferGuestCartToUserAsync(request.SessionId, userId.Value);

            if (!result.Success)
            {
                return BadRequest(new { success = false, message = result.Message, errors = result.Errors });
            }

            return Ok(new { success = true, message = result.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transferring guest cart");
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }

    /// <summary>
    /// Merge two carts
    /// </summary>
    /// <param name="request">Merge carts request</param>
    /// <returns>Success status</returns>
    [HttpPost("merge")]
    public async Task<IActionResult> MergeCarts([FromBody] MergeCartsRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid request", errors = ModelState });
            }

            var result = await _cartSaveService.MergeCartsAsync(request.TargetCartId, request.SourceCartId);

            if (!result.Success)
            {
                return BadRequest(new { success = false, message = result.Message, errors = result.Errors });
            }

            return Ok(new { success = true, message = result.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error merging carts");
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }

    #endregion

    #region Cart Validation

    /// <summary>
    /// Validate cart for checkout
    /// </summary>
    /// <param name="cartId">Cart ID</param>
    /// <returns>Validation result</returns>
    [HttpGet("{cartId}/validate")]
    public async Task<IActionResult> ValidateCartForCheckout(int cartId)
    {
        try
        {
            var result = await _cartValidationService.ValidateCartForCheckoutAsync(cartId);

            if (!result.Success)
            {
                return BadRequest(new { success = false, message = result.Message, errors = result.Errors });
            }

            return Ok(new { success = true, data = result.Data, message = result.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating cart {CartId}", cartId);
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get cart validation issues
    /// </summary>
    /// <param name="cartId">Cart ID</param>
    /// <returns>List of validation issues</returns>
    [HttpGet("{cartId}/issues")]
    public async Task<IActionResult> GetCartIssues(int cartId)
    {
        try
        {
            var result = await _cartValidationService.GetCartIssuesAsync(cartId);

            if (!result.Success)
            {
                return BadRequest(new { success = false, message = result.Message, errors = result.Errors });
            }

            return Ok(new { success = true, data = result.Data, message = result.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cart issues for cart {CartId}", cartId);
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }

    #endregion

    #region Admin/Statistics

    /// <summary>
    /// Get cart statistics (Admin only)
    /// </summary>
    /// <returns>Cart statistics</returns>
    [HttpGet("statistics")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetCartStatistics()
    {
        try
        {
            var result = await _cartValidationService.GetCartStatisticsAsync();

            if (!result.Success)
            {
                return BadRequest(new { success = false, message = result.Message, errors = result.Errors });
            }

            return Ok(new { success = true, data = result.Data, message = result.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cart statistics");
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }

    /// <summary>
    /// Cleanup expired carts (Admin only)
    /// </summary>
    /// <param name="daysOld">Days old threshold</param>
    /// <returns>Number of deleted carts</returns>
    [HttpPost("cleanup")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CleanupExpiredCarts([FromQuery] int daysOld = 30)
    {
        try
        {
            var result = await _cartValidationService.CleanupExpiredCartsAsync(daysOld);

            if (!result.Success)
            {
                return BadRequest(new { success = false, message = result.Message, errors = result.Errors });
            }

            return Ok(new { success = true, data = result.Data, message = result.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up expired carts");
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }

    #endregion

    #region Helper Methods

    private (int?, string?) GetUserAndSessionInfo()
    {
        var userId = GetUserId();
        var sessionId = GetSessionId();
        return (userId, sessionId);
    }

    private int? GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    private string? GetSessionId()
    {
        // 1. Thử header trước (nếu FE gửi)
        var headerSessionId = Request.Headers["X-Session-ID"].FirstOrDefault();
        if (!string.IsNullOrEmpty(headerSessionId))
        {
            return headerSessionId;
        }
        
        // 2. Đọc từ cookie SessionId
        if (Request.Cookies.TryGetValue("SessionId", out var cookieSessionId))
        {
            _logger.LogInformation("Using cookie session ID: {SessionId}", cookieSessionId);
            return cookieSessionId;
        }
        
        // 3. Fallback: ASP.NET Session
        return HttpContext.Session.Id;
    }


    #endregion
}

#region Request DTOs

public class AddMultipleItemsRequest
{
    public List<AddToCartRequest> Items { get; set; } = new();
}


public class SaveCartRequest
{
    public string? CartName { get; set; }
}

public class TransferCartRequest
{
    public string SessionId { get; set; } = string.Empty;
}

public class MergeCartsRequest
{
    public int TargetCartId { get; set; }
    public int SourceCartId { get; set; }
}

#endregion