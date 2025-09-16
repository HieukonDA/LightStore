namespace TheLightStore.Interfaces.Inventory;

public interface IInventoryService
{
    // Check stock trước khi tạo order
    Task<bool> IsStockAvailableAsync(int productId, int quantity);
    Task<List<StockCheckResult>> CheckBulkAvailabilityAsync(List<StockCheckRequest> requests);

    // Reserve stock khi customer checkout
    Task<List<ReserveStockResult>> ReserveStockForOrderAsync(string orderId, List<ReserveStockRequest> items);

    // Commit reservation khi payment thành công
    Task CommitReservationsAsync(string orderId);

    // Release reservation khi cancel order
    Task ReleaseReservationsAsync(string orderId);

    // Background job cleanup
    Task CleanupExpiredReservationsAsync();

}

// public interface IStockService
// {
//     Task<int> GetAvailableStockAsync(int productId);
//     Task UpdateStockAsync(int productId, int newQuantity);
//     Task AdjustStockAsync(int productId, int adjustment);
// }

