namespace TheLightStore.Application.DTOs.Inventory;
public class StockCheckRequest
{
    public int ProductId { get; set; }
    public int? VariantId { get; set; }  // Quan trọng!
    public int Quantity { get; set; }
}

public class StockCheckResult
{
    public int ProductId { get; set; }
    public int? VariantId { get; set; }
    public bool IsAvailable { get; set; }
    public int AvailableQuantity { get; set; }
    public string? ErrorMessage { get; set; }
}