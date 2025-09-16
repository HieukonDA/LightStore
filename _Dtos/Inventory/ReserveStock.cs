namespace TheLightStore.Dtos.Inventory;
public class ReserveStockRequest
{
    public int ProductId { get; set; }
    public int? VariantId { get; set; }
    public int Quantity { get; set; }
}

public class ReserveStockResult
{
    public string ReservationId { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}