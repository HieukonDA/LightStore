using TheLightStore.Dtos.Product;

namespace TheLightStore.Services.Inventory;

public class InventoryService : IInventoryService
{
    private readonly IProductRepo _productRepo;
    private readonly IProductVariantRepo _productVariantRepo;
    private readonly IInventoryReservationRepo _reservationRepo;
    private readonly IInventoryLogRepo _inventoryLogRepo;
    private readonly ILogger<InventoryService> _logger;
    private readonly int _reservationTimeoutMinutes;

    public InventoryService(
        IProductRepo productRepo,
        IProductVariantRepo productVariantRepo,
        IInventoryReservationRepo reservationRepo,
        IInventoryLogRepo inventoryLogRepo,
        ILogger<InventoryService> logger,
        IConfiguration configuration)
    {
        _productRepo = productRepo;
        _productVariantRepo = productVariantRepo;
        _reservationRepo = reservationRepo;
        _inventoryLogRepo = inventoryLogRepo;
        _logger = logger;
        _reservationTimeoutMinutes = configuration.GetValue<int>("InventorySettings:ReservationTimeoutMinutes", 30);
    }


    public async Task<bool> IsStockAvailableAsync(int productId, int quantity)
    {
        try
        {
            if (productId <= 0) throw new ArgumentException("Invalid productId");
            if (quantity <= 0) throw new ArgumentException("Quantity must be greater than zero");


            var product = await _productRepo.GetByIdAsync(productId);
            if (product == null || !product.IsActive)
                return false;

            if (!product.ManageStock)
                return true;

            var availableStock = await GetAvailableStockAsync(productId, null);
            return availableStock >= quantity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking stock availability for product {ProductId}", productId);
            return false;
        }
    }

    public async Task<List<StockCheckResult>> CheckBulkAvailabilityAsync(List<StockCheckRequest> requests)
    {
        var results = new List<StockCheckResult>();

        foreach (var request in requests)
        {
            try
            {
                var result = new StockCheckResult
                {
                    ProductId = request.ProductId,
                    VariantId = request.VariantId
                };

                // Check variant first if specified
                if (request.VariantId.HasValue)
                {
                    var variant = await _productVariantRepo.GetByIdAsync(request.VariantId.Value);
                    if (variant == null || !variant.IsActive)
                    {
                        result.IsAvailable = false;
                        result.AvailableQuantity = 0;
                        result.ErrorMessage = "Variant not found or inactive";
                        results.Add(result);
                        continue;
                    }

                    var availableStock = await GetAvailableStockAsync(request.ProductId, request.VariantId);
                    result.AvailableQuantity = availableStock;
                    result.IsAvailable = availableStock >= request.Quantity;
                }
                else
                {
                    var product = await _productRepo.GetByIdAsync(request.ProductId);
                    if (product == null || !product.IsActive)
                    {
                        result.IsAvailable = false;
                        result.AvailableQuantity = 0;
                        result.ErrorMessage = "Product not found or inactive";
                        results.Add(result);
                        continue;
                    }

                    if (!product.ManageStock)
                    {
                        result.IsAvailable = true;
                        result.AvailableQuantity = int.MaxValue;
                    }
                    else
                    {
                        var availableStock = await GetAvailableStockAsync(request.ProductId, null);
                        result.AvailableQuantity = availableStock;
                        result.IsAvailable = availableStock >= request.Quantity;
                    }
                }

                results.Add(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking stock for product {ProductId}, variant {VariantId}",
                    request.ProductId, request.VariantId);

                results.Add(new StockCheckResult
                {
                    ProductId = request.ProductId,
                    VariantId = request.VariantId,
                    IsAvailable = false,
                    AvailableQuantity = 0,
                    ErrorMessage = "Error checking stock availability"
                });
            }
        }

        return results;
    }

    public async Task<List<ReserveStockResult>> ReserveStockForOrderAsync(string orderId, List<ReserveStockRequest> items)
    {
        var results = new List<ReserveStockResult>();
        if (items == null || !items.Any()) return results;

        using var transaction = await _reservationRepo.BeginTransactionAsync();
        try
        {
            // STEP 1: Sort IDs để giảm deadlock
            var productIds = items.Select(x => x.ProductId).Distinct().OrderBy(x => x).ToList();
            var variantIds = items.Where(x => x.VariantId.HasValue).Select(x => x.VariantId.Value).Distinct().OrderBy(x => x).ToList();

            // STEP 2: Lấy stock với FOR UPDATE (row lock trong transaction)
            var productAvailability = await _productRepo.GetProductsAvailabilityWithLockAsync(productIds, transaction);
            var variantAvailability = variantIds.Any()
                ? await _productVariantRepo.GetVariantsAvailabilityWithLockAsync(variantIds, transaction)
                : new Dictionary<int, ProductAvailabilityInfo>();

            // STEP 3: Kiểm tra đủ stock ngay trong transaction
            foreach (var item in items)
            {
                var available = item.VariantId.HasValue
                    ? variantAvailability[item.VariantId.Value].AvailableQuantity
                    : productAvailability[item.ProductId].AvailableQuantity;

                if (available < item.Quantity)
                {
                    // Thiếu stock -> rollback luôn
                    await transaction.RollbackAsync();

                    return items.Select(x => new ReserveStockResult
                    {
                        Success = false,
                        ErrorMessage = "Insufficient stock",
                        ReservationId = ""
                    }).ToList();
                }
            }

            // STEP 4: Tạo reservations
            foreach (var item in items)
            {
                var reservation = new InventoryReservation
                {
                    ProductId = item.ProductId,
                    VariantId = item.VariantId,
                    Quantity = item.Quantity,
                    ReservedUntil = DateTime.UtcNow.AddMinutes(_reservationTimeoutMinutes),
                    Status = InventoryStatus.Reserved,
                    OrderId = int.TryParse(orderId, out var orderIdInt) ? orderIdInt : null,
                    CreatedAt = DateTime.UtcNow
                };

                await _reservationRepo.AddAsync(reservation);

                results.Add(new ReserveStockResult
                {
                    Success = true,
                    ReservationId = reservation.Id.ToString()
                });

                await LogInventoryChangeAsync(item.ProductId, item.VariantId,
                    InventoryChangeType.Reserved, -item.Quantity,
                    $"Reserved for order {orderId}", reservation.Id.ToString());
            }

            // STEP 5: Commit transaction
            await _reservationRepo.SaveChangesAsync();
            await transaction.CommitAsync();

            return results;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error in ReserveStockForOrderAsync for order {OrderId}", orderId);
            throw;
        }
    }


    public async Task CommitReservationsAsync(string orderId)
    {
        using var transaction = await _reservationRepo.BeginTransactionAsync();
        try
        {
            if (!int.TryParse(orderId, out var orderIdInt))
            {
                _logger.LogWarning("Invalid orderId format: {OrderId}", orderId);
                return;
            }

            var reservations = await _reservationRepo.GetByOrderIdAsync(orderIdInt);
            var reservedItems = reservations.Where(x => x.Status == InventoryStatus.Reserved).ToList();

            foreach (var reservation in reservedItems)
            {
                // Update reservation status
                reservation.Status = InventoryStatus.Committed;
                await _reservationRepo.UpdateAsync(reservation);

                // Update actual stock
                await UpdateStockQuantityAsync(reservation.ProductId, reservation.VariantId, -reservation.Quantity);

                // Log the commit
                await LogInventoryChangeAsync(reservation.ProductId, reservation.VariantId,
                    InventoryChangeType.Committed, -reservation.Quantity,
                    $"Committed for order {orderId}", reservation.Id.ToString());
            }

            await _reservationRepo.SaveChangesAsync();
            _logger.LogInformation("Committed {Count} reservations for order {OrderId}", reservedItems.Count, orderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error committing reservations for order {OrderId}", orderId);
            throw;
        }
    }

    public async Task ReleaseReservationsAsync(string orderId)
    {
        using var transaction = await _reservationRepo.BeginTransactionAsync();
        try
        {
            if (!int.TryParse(orderId, out var orderIdInt))
            {
                _logger.LogWarning("Invalid orderId format: {OrderId}", orderId);
                return;
            }

            var reservations = await _reservationRepo.GetByOrderIdAsync(orderIdInt);
            var activeReservations = reservations.Where(x =>
                x.Status == InventoryStatus.Reserved || x.Status == InventoryStatus.Committed).ToList();

            foreach (var reservation in activeReservations)
            {
                reservation.Status = InventoryStatus.Released;
                await _reservationRepo.UpdateAsync(reservation);

                if (reservation.Status == InventoryStatus.Committed)
                {
                    await UpdateStockQuantityAsync(reservation.ProductId, reservation.VariantId, +reservation.Quantity);
                }


                // Log the release
                await LogInventoryChangeAsync(reservation.ProductId, reservation.VariantId,
                    InventoryChangeType.Released, reservation.Quantity,
                    $"Released for cancelled order {orderId}", reservation.Id.ToString());
            }

            await _reservationRepo.SaveChangesAsync();
            _logger.LogInformation("Released {Count} reservations for order {OrderId}", activeReservations.Count, orderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error releasing reservations for order {OrderId}", orderId);
            throw;
        }
    }

    public async Task CleanupExpiredReservationsAsync()
    {
        try
        {
            var expiredReservations = await _reservationRepo.GetExpiredAsync();
            var count = 0;

            foreach (var reservation in expiredReservations)
            {
                if (reservation.Status == InventoryStatus.Reserved)
                {
                    reservation.Status = InventoryStatus.Expired;
                    await _reservationRepo.UpdateAsync(reservation);

                    // Log the expiration
                    await LogInventoryChangeAsync(reservation.ProductId, reservation.VariantId,
                        InventoryChangeType.Expired, reservation.Quantity,
                        "Reservation expired", reservation.Id.ToString());

                    count++;
                }
            }

            if (count > 0)
            {
                await _reservationRepo.SaveChangesAsync();
                _logger.LogInformation("Cleaned up {Count} expired reservations", count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up expired reservations");
        }
    }





    #region  helper methods


    private async Task<int> GetAvailableStockAsync(int productId, int? variantId)
    {
        // Defensive checks
        if (productId <= 0) return 0;

        // Variant-specific stock
        if (variantId.HasValue)
        {
            var variant = await _productVariantRepo.GetByIdAsync(variantId.Value);
            if (variant == null) return 0;
            if (!variant.StockQuantity.HasValue) return 0;

            // Lấy tổng lượng đang được reserve (chỉ COUNT status = Reserved và chưa expired)
            // Yêu cầu repository cung cấp aggregation method để efficient query
            var reservedQty = await _reservationRepo.GetActiveReservedQuantityAsync(productId, variantId.Value);

            var available = variant.StockQuantity.Value - reservedQty;
            return Math.Max(0, available);
        }
        else // Product-level stock
        {
            var product = await _productRepo.GetByIdAsync(productId);
            if (product == null) return 0;

            if (!product.ManageStock)
            {
                // Không quản lý tồn kho -> coi như infinite
                return int.MaxValue;
            }

            var reservedQty = await _reservationRepo.GetActiveReservedQuantityAsync(productId, null);

            var available = product.StockQuantity - reservedQty;
            return Math.Max(0, available);
        }
    }

    private async Task UpdateStockQuantityAsync(int productId, int? variantId, int quantityChange)
    {
        if (variantId.HasValue)
        {
            var variant = await _productVariantRepo.GetByIdAsync(variantId.Value);
            if (variant?.StockQuantity.HasValue == true)
            {
                variant.StockQuantity += quantityChange;
                variant.UpdatedAt = DateTime.UtcNow;
                await _productVariantRepo.UpdateAsync(variant);
            }
        }
        else
        {
            var product = await _productRepo.GetByIdAsync(productId);
            if (product != null)
            {
                product.StockQuantity += quantityChange;
                product.UpdatedAt = DateTime.UtcNow;
                await _productRepo.UpdateAsync(product);
            }
        }
    }
    
    private async Task LogInventoryChangeAsync(int productId, int? variantId, string changeType, 
        int quantityChange, string reason, string? referenceId = null)
    {
        try
        {
            var currentStock = await GetAvailableStockAsync(productId, variantId);
            
            var log = new InventoryLog
            {
                ProductId = productId,
                VariantId = variantId,
                ChangeType = changeType,
                QuantityBefore = currentStock - quantityChange,
                QuantityChange = quantityChange,
                QuantityAfter = currentStock,
                Reason = reason,
                ReferenceId = int.TryParse(referenceId, out var refId) ? refId : null,
                ReferenceType = "InventoryReservation",
                CreatedAt = DateTime.UtcNow
            };

            await _inventoryLogRepo.AddAsync(log);
            await _inventoryLogRepo.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging inventory change for product {ProductId}", productId);
            // Don't throw here as logging shouldn't break the main flow
        }
    }
    
    #endregion  helper methods


}