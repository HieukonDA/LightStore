namespace TheLightStore.Dtos.Cart;

public class CartValidationResult
{
    public bool IsValidForCheckout { get; set; }
    public List<CartValidationIssue> Issues { get; set; } = new();
    public bool HasInactiveProducts { get; set; }
    public bool HasPriceChanges { get; set; }
}

public class CartValidationIssue
{
    public int CartItemId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string IssueType { get; set; } = string.Empty; // "inactive", "price_changed", "discontinued"
    public string Description { get; set; } = string.Empty;
    public decimal? OldPrice { get; set; }
    public decimal? NewPrice { get; set; }
}
