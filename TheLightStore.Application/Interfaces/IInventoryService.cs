using TheLightStore.Application.DTOs.Inventory;

namespace TheLightStore.Application.Interfaces;

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
