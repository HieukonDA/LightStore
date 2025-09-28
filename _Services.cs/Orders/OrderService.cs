using TheLightStore.Dtos.Orders;
using TheLightStore.DTOs.Orders;
using TheLightStore.Interfaces.Notifications;
using TheLightStore.Interfaces.Orders;
using TheLightStore.Interfaces.Payment;

namespace TheLightStore.Services.Orders;

public class OrderService : IOrderService
{
    private readonly IOrderRepo _orderRepo;
    private readonly IOrderItemRepo _orderItemRepo;
    private readonly IOrderStatusHistoryRepo _statusHistoryRepo;
    private readonly IInventoryService _inventoryService;
    private readonly IPaymentService _paymentService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepo orderRepo,
        IOrderItemRepo orderItemRepo,
        IOrderStatusHistoryRepo statusHistoryRepo,
        IInventoryService inventoryService,
        IPaymentService paymentService,
        INotificationService notificationService,
        ILogger<OrderService> logger)
    {
        _orderRepo = orderRepo;
        _orderItemRepo = orderItemRepo;
        _statusHistoryRepo = statusHistoryRepo;
        _inventoryService = inventoryService;
        _paymentService = paymentService;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<ServiceResult<PagedResult<OrderDto>>> GetAllAsync(PagedRequest request, CancellationToken ct = default)
    {
        try
        {
            var result = await _orderRepo.GetAllAsync(request, ct);

            var orderDtos = result.Items.Select(result => CartMapper.MapOrderToDto(result)).ToList();
            var pagedResult = new PagedResult<OrderDto>
            {
                Items = orderDtos,
                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize
            };

            return ServiceResult<PagedResult<OrderDto>>.SuccessResult(pagedResult, "Orders retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders");
            return ServiceResult<PagedResult<OrderDto>>.FailureResult("An error occurred while retrieving orders", new List<string> { ex.Message });
        }
    }

    public async Task<ServiceResult<OrderDto>> CreateOrderAsync(OrderCreateDto dto, CancellationToken ct = default)
    {
        try
        {
            Console.WriteLine($"[INFO] Starting order creation for user {dto.UserId} with {dto.Items?.Count ?? 0} items");
            _logger.LogInformation("Starting order creation for user {UserId} with {ItemCount} items",
                    dto.UserId, dto.Items?.Count ?? 0);

            // 1. Tạo Order (Pending)
            var order = new Order
            {
                UserId = dto.UserId,
                OrderNumber = GenerateOrderNumber(),
                CustomerName = dto.CustomerName,
                CustomerEmail = dto.CustomerEmail,
                CustomerPhone = dto.CustomerPhone,
                Subtotal = dto.Items.Sum(i => i.UnitPrice * i.Quantity),
                TotalAmount = CalculateOrderTotal(dto),
                TaxAmount = dto.TaxAmount,
                ShippingCost = dto.ShippingCost,
                DiscountAmount = dto.DiscountAmount,
                OrderStatus = OrderStatus.Pending,
                CustomerNotes = dto.CustomerNotes,
                OrderDate = DateTime.UtcNow,
                OrderItems = dto.Items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    VariantId = i.VariantId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.UnitPrice * i.Quantity,
                    ProductName = i.ProductName,
                    ProductSku = i.ProductSku,
                    VariantName = i.VariantName,
                    ProductAttributes = i.ProductAttributes
                }).ToList(),
                OrderPayments = new List<OrderPayment>()
            };

            await _orderRepo.AddAsync(order, ct);
            await _orderRepo.SaveChangesAsync(ct);

            // 2. Reserve Stock
            var reserveResults = await _inventoryService.ReserveStockForOrderAsync(
                order.Id.ToString(),
                dto.Items.Select(i => new ReserveStockRequest
                {
                    ProductId = i.ProductId,
                    VariantId = i.VariantId,
                    Quantity = i.Quantity
                }).ToList()
            );

            if (reserveResults.Any(r => !r.Success))
            {
                Console.WriteLine("[ERROR] One or more items are out of stock.");
                return ServiceResult<OrderDto>.FailureResult("One or more items are out of stock.",
                    new List<string> { "OutOfStock" });
            }

            // 3. Create Pending Payment
            var payment = await _paymentService.CreatePaymentAsync(order.Id, order.TotalAmount, dto.PaymentMethod);

            var orderDto = CartMapper.MapOrderToDto(order);

            _logger.LogInformation("Payment created with QrCodeUrl: {QrCodeUrl}, CheckoutUrl: {CheckoutUrl}", payment.QrCodeUrl, payment.CheckoutUrl);

            orderDto.Payment.QrCodeUrl = payment.QrCodeUrl;
            orderDto.Payment.CheckoutUrl = payment.CheckoutUrl;
            await _orderRepo.SaveChangesAsync(ct);

            // // 4. Update Order status -> WAITING_FOR_PAYMENT
            // order.OrderStatus = OrderStatus.Processing;
            // await _orderRepo.UpdateAsync(order, ct);
            // await _orderRepo.SaveChangesAsync(ct);

            // // 5. Add status history
            // await _statusHistoryRepo.AddAsync(new OrderStatusHistory
            // {
            //     OrderId = order.Id,
            //     OldStatus = null,
            //     NewStatus = order.OrderStatus,
            //     Comment = "Waiting for payment",
            //     ChangedAt = DateTime.UtcNow
            // }, ct);
            // await _orderRepo.SaveChangesAsync(ct);

            // 4. Gửi thông báo đơn hàng mới
            await _notificationService.NotifyNewOrderAsync(order, ct);
            
            // 5. Gửi thông báo cho customer về đơn hàng mới
            if (order.UserId.HasValue)
            {
                await _notificationService.NotifyCustomerOrderStatusAsync(order.UserId.Value, order, "pending", ct);
            }

            Console.WriteLine("[SUCCESS] Order created successfully.");
            return ServiceResult<OrderDto>.SuccessResult(orderDto, "Order created successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EXCEPTION] {ex.Message}");
            _logger.LogError(ex, "Error creating order for user {UserId}", dto.UserId);

            return ServiceResult<OrderDto>.FailureResult("Failed to create order",
                new List<string> { ex.Message });
        }
    }


    public async Task<ServiceResult<OrderDto?>> GetOrderByIdAsync(int orderId, CancellationToken ct = default)
    {
        try
        {
            if (orderId <= 0)
            {
                Console.WriteLine("[ERROR] Invalid orderId");
                return ServiceResult<OrderDto?>.FailureResult("Invalid orderId", new List<string> { "orderId must be greater than 0" });
            }

            var order = await _orderRepo.GetByIdAsync(orderId, ct);
            if (order == null)
            {
                Console.WriteLine("[ERROR] Order not found");
                return ServiceResult<OrderDto?>.FailureResult("Order not found", new List<string> { $"Order {orderId} does not exist" });
            }

            // Map Order entity to OrderDto with OrderItems
            var orderDto = MapOrderToDto(order);

            Console.WriteLine("[SUCCESS] Order retrieved successfully");
            return ServiceResult<OrderDto?>.SuccessResult(orderDto, "Order retrieved successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EXCEPTION] {ex.Message}");
            _logger.LogError(ex, "Error getting order {OrderId}", orderId);
            return ServiceResult<OrderDto?>.FailureResult("Failed to get order", new List<string> { ex.Message });
        }
    }

    public async Task<ServiceResult<OrderDto?>> GetOrderByOrderNumberAsync(string orderNumber, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(orderNumber))
            {
                Console.WriteLine("[ERROR] Invalid orderNumber");
                return ServiceResult<OrderDto?>.FailureResult("Invalid orderNumber", new List<string> { "orderNumber must be provided" });
            }

            var order = await _orderRepo.GetByOrderNumberAsync(orderNumber, ct);
            if (order == null)
            {
                Console.WriteLine("[ERROR] Order not found");
                return ServiceResult<OrderDto?>.FailureResult("Order not found", new List<string> { $"Order {orderNumber} does not exist" });
            }

            // Map Order entity to OrderDto with OrderItems  
            var orderDto = MapOrderToDto(order);

            Console.WriteLine("[SUCCESS] Order retrieved successfully");
            return ServiceResult<OrderDto?>.SuccessResult(orderDto, "Order retrieved successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EXCEPTION] {ex.Message}");
            _logger.LogError(ex, "Error getting order {OrderNumber}", orderNumber);
            return ServiceResult<OrderDto?>.FailureResult("Failed to get order", new List<string> { ex.Message });
        }
    }

    public async Task<ServiceResult<IEnumerable<Order>>> GetOrdersByUserAsync(int userId, CancellationToken ct = default)
    {
        try
        {
            if (userId <= 0)
            {
                Console.WriteLine("[ERROR] Invalid userId");
                return ServiceResult<IEnumerable<Order>>.FailureResult("Invalid userId", new List<string> { "userId must be greater than 0" });
            }

            var orders = await _orderRepo.GetByUserIdAsync(userId, ct);
            if (orders == null || !orders.Any())
            {
                Console.WriteLine("[ERROR] No orders found for user");
                return ServiceResult<IEnumerable<Order>>.FailureResult("No orders found for user", new List<string> { $"User {userId} has no orders" });
            }

            Console.WriteLine("[SUCCESS] Orders retrieved successfully");
            return ServiceResult<IEnumerable<Order>>.SuccessResult(orders, "Orders retrieved successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EXCEPTION] {ex.Message}");
            _logger.LogError(ex, "Error getting orders for user {UserId}", userId);
            return ServiceResult<IEnumerable<Order>>.FailureResult("Failed to get orders", new List<string> { ex.Message });
        }
    }

    #region state transitions
        

    public async Task<ServiceResult<bool>> ConfirmOrderAsync(int orderId, string? adminNotes = null, CancellationToken ct = default)
    {
        try
        {
            if (orderId <= 0)
            {
                Console.WriteLine("[ERROR] Invalid orderId");
                return ServiceResult<bool>.FailureResult("Invalid orderId", new List<string> { "orderId must be greater than 0" });
            }

            var order = await _orderRepo.GetByIdAsync(orderId, ct);
            if (order == null)
            {
                Console.WriteLine("[ERROR] Order not found");
                return ServiceResult<bool>.FailureResult("Order not found", new List<string> { $"Order {orderId} does not exist" });
            }

            var oldStatus = order.OrderStatus.ToString();
            order.OrderStatus = OrderStatus.Confirmed;
            await _orderRepo.UpdateAsync(order, ct);
            await _statusHistoryRepo.AddAsync(new OrderStatusHistory
            {
                OrderId = order.Id,
                OldStatus = null,
                NewStatus = order.OrderStatus,
                Comment = adminNotes ?? "Order confirmed",
                ChangedAt = DateTime.UtcNow
            }, ct);
            await _orderRepo.SaveChangesAsync(ct);

            // Gửi thông báo cập nhật trạng thái cho admin
            await _notificationService.NotifyOrderUpdateAsync(order, oldStatus, order.OrderStatus.ToString(), ct);
            
            // Gửi thông báo cho customer
            if (order.UserId.HasValue)
            {
                await _notificationService.NotifyCustomerOrderStatusAsync(order.UserId.Value, order, "confirmed", ct);
            }

            Console.WriteLine("[SUCCESS] Order confirmed successfully");
            return ServiceResult<bool>.SuccessResult(true, "Order confirmed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EXCEPTION] {ex.Message}");
            _logger.LogError(ex, "Error confirming order {OrderId}", orderId);
            return ServiceResult<bool>.FailureResult("Failed to confirm order", new List<string> { ex.Message });
        }
    }


    public async Task<ServiceResult<bool>> ProcessOrderAsync(int orderId, string? processingNotes = null, CancellationToken ct = default)
    {
        try
        {
            if (orderId <= 0)
            {
                Console.WriteLine("[ERROR] Invalid orderId");
                return ServiceResult<bool>.FailureResult("Invalid orderId", new List<string> { "orderId must be greater than 0" });
            }

            var order = await _orderRepo.GetByIdAsync(orderId, ct);
            if (order == null)
            {
                Console.WriteLine("[ERROR] Order not found");
                return ServiceResult<bool>.FailureResult("Order not found", new List<string> { $"Order {orderId} does not exist" });
            }

            // Kiểm tra order status - chỉ cho phép process order đã confirmed
            if (order.OrderStatus != OrderStatus.Confirmed)
            {
                Console.WriteLine($"[ERROR] Invalid order status. Current: {order.OrderStatus}, Expected: {OrderStatus.Confirmed}");
                return ServiceResult<bool>.FailureResult("Invalid order status", 
                    new List<string> { $"Order must be in Confirmed status to process. Current status: {order.OrderStatus}" });
            }

            var oldStatus = order.OrderStatus;
            order.OrderStatus = OrderStatus.Processing;
            
            await _orderRepo.UpdateAsync(order, ct);
            
            // Thêm status history
            await _statusHistoryRepo.AddAsync(new OrderStatusHistory
            {
                OrderId = order.Id,
                OldStatus = oldStatus,
                NewStatus = order.OrderStatus,
                Comment = processingNotes ?? "Order moved to processing",
                ChangedAt = DateTime.UtcNow
            }, ct);
            
            await _orderRepo.SaveChangesAsync(ct);

            // Gửi thông báo cập nhật trạng thái cho admin
            await _notificationService.NotifyOrderUpdateAsync(order, oldStatus.ToString(), order.OrderStatus.ToString(), ct);
            
            // Gửi thông báo cho customer
            if (order.UserId.HasValue)
            {
                await _notificationService.NotifyCustomerOrderStatusAsync(order.UserId.Value, order, "processing", ct);
            }
            
            // Log
            _logger.LogInformation("Order {OrderId} moved to processing status", orderId);
            //     _logger.LogWarning(notifEx, "Failed to send notification for order {OrderId}", orderId);
            //     // Không fail toàn bộ process vì notification
            // }

            Console.WriteLine("[SUCCESS] Order processed successfully");
            return ServiceResult<bool>.SuccessResult(true, "Order processed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EXCEPTION] {ex.Message}");
            _logger.LogError(ex, "Error processing order {OrderId}", orderId);
            return ServiceResult<bool>.FailureResult("Failed to process order", new List<string> { ex.Message });
        }
    }


    public async Task<ServiceResult<bool>> ShipOrderAsync(int orderId, string? trackingNumber = null, CancellationToken ct = default)
    {
        try
        {
            if (orderId <= 0)
            {
                Console.WriteLine("[ERROR] Invalid orderId");
                return ServiceResult<bool>.FailureResult("Invalid orderId", new List<string> { "orderId must be greater than 0" });
            }

            var order = await _orderRepo.GetByIdAsync(orderId, ct);
            if (order == null)
            {
                Console.WriteLine("[ERROR] Order not found");
                return ServiceResult<bool>.FailureResult("Order not found", new List<string> { $"Order {orderId} does not exist" });
            }

            var oldStatus = order.OrderStatus;
            order.OrderStatus = OrderStatus.Shipping;
            order.OrderNumber = trackingNumber ?? order.OrderNumber;
            await _orderRepo.UpdateAsync(order, ct);
            await _statusHistoryRepo.AddAsync(new OrderStatusHistory
            {
                OrderId = order.Id,
                OldStatus = oldStatus,
                NewStatus = order.OrderStatus,
                Comment = $"Shipped with tracking {trackingNumber ?? "N/A"}",
                ChangedAt = DateTime.UtcNow
            }, ct);
            await _orderRepo.SaveChangesAsync(ct);

            // Gửi thông báo cho admin
            await _notificationService.NotifyOrderUpdateAsync(order, oldStatus.ToString(), order.OrderStatus.ToString(), ct);
            
            // Gửi thông báo cho customer
            if (order.UserId.HasValue)
            {
                await _notificationService.NotifyCustomerOrderStatusAsync(order.UserId.Value, order, "shipped", ct);
            }

            Console.WriteLine("[SUCCESS] Order shipped successfully");
            return ServiceResult<bool>.SuccessResult(true, "Order shipped successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EXCEPTION] {ex.Message}");
            _logger.LogError(ex, "Error shipping order {OrderId}", orderId);
            return ServiceResult<bool>.FailureResult("Failed to ship order", new List<string> { ex.Message });
        }
    }

    public async Task<ServiceResult<bool>> DeliverOrderAsync(int orderId, CancellationToken ct = default)
    {
        try
        {
            if (orderId <= 0)
            {
                Console.WriteLine("[ERROR] Invalid orderId");
                return ServiceResult<bool>.FailureResult("Invalid orderId", new List<string> { "orderId must be greater than 0" });
            }

            var order = await _orderRepo.GetByIdAsync(orderId, ct);
            if (order == null)
            {
                Console.WriteLine("[ERROR] Order not found");
                return ServiceResult<bool>.FailureResult("Order not found", new List<string> { $"Order {orderId} does not exist" });
            }

            var oldStatus = order.OrderStatus;
            order.OrderStatus = OrderStatus.Delivered;
            await _orderRepo.UpdateAsync(order, ct);
            await _statusHistoryRepo.AddAsync(new OrderStatusHistory
            {
                OrderId = order.Id,
                OldStatus = oldStatus,
                NewStatus = order.OrderStatus,
                Comment = "Order delivered",
                ChangedAt = DateTime.UtcNow
            }, ct);
            await _orderRepo.SaveChangesAsync(ct);

            // Gửi thông báo cho admin
            await _notificationService.NotifyOrderUpdateAsync(order, oldStatus.ToString(), order.OrderStatus.ToString(), ct);
            
            // Gửi thông báo cho customer
            if (order.UserId.HasValue)
            {
                await _notificationService.NotifyCustomerOrderStatusAsync(order.UserId.Value, order, "delivered", ct);
            }

            Console.WriteLine("[SUCCESS] Order delivered successfully");
            return ServiceResult<bool>.SuccessResult(true, "Order delivered successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EXCEPTION] {ex.Message}");
            _logger.LogError(ex, "Error delivering order {OrderId}", orderId);
            return ServiceResult<bool>.FailureResult("Failed to deliver order", new List<string> { ex.Message });
        }
    }

    public async Task<ServiceResult<bool>> CancelOrderAsync(int orderId, string? reason = null, CancellationToken ct = default)
    {
        // using var transaction = await _orderRepo.BeginTransactionAsync(ct);
        try
        {
            if (orderId <= 0)
            {
                Console.WriteLine("[ERROR] Invalid orderId");
                return ServiceResult<bool>.FailureResult("Invalid orderId", new List<string> { "orderId must be greater than 0" });
            }

            var order = await _orderRepo.GetByIdAsync(orderId, ct);
            if (order == null)
            {
                Console.WriteLine("[ERROR] Order not found");
                return ServiceResult<bool>.FailureResult("Order not found", new List<string> { $"Order {orderId} does not exist" });
            }

            var oldStatus = order.OrderStatus;

            order.OrderStatus = OrderStatus.Cancelled;
            order.CancelledAt = DateTime.UtcNow;
            await _orderRepo.UpdateAsync(order, ct);
            await _orderRepo.SaveChangesAsync(ct);


            await _statusHistoryRepo.AddAsync(new OrderStatusHistory
            {
                OrderId = order.Id,
                OldStatus = oldStatus,
                NewStatus = order.OrderStatus,
                Comment = reason ?? "Order cancelled",
                ChangedAt = DateTime.UtcNow
            }, ct);

            await _orderRepo.SaveChangesAsync(ct);

            // Release stock if reserved
            await _inventoryService.ReleaseReservationsAsync(order.Id.ToString());

            // Gửi thông báo cho admin
            await _notificationService.NotifyOrderUpdateAsync(order, oldStatus.ToString(), order.OrderStatus.ToString(), ct);

            // Gửi thông báo cho customer
            if (order.UserId.HasValue)
            {
                await _notificationService.NotifyCustomerOrderStatusAsync(order.UserId.Value, order, "cancelled", ct);
            }

            Console.WriteLine("[SUCCESS] Order cancelled successfully");
            return ServiceResult<bool>.SuccessResult(true, "Order cancelled successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EXCEPTION] {ex.Message}");
            _logger.LogError(ex, "Error cancelling order {OrderId}", orderId);
            return ServiceResult<bool>.FailureResult("Failed to cancel order", new List<string> { ex.Message });
        }
    }




    #endregion


    public async Task<ServiceResult<IEnumerable<OrderStatusHistory>>> GetOrderHistoryAsync(int orderId, CancellationToken ct = default)
    {
        try
        {
            if (orderId <= 0)
            {
                Console.WriteLine("[ERROR] Invalid orderId");
                return ServiceResult<IEnumerable<OrderStatusHistory>>.FailureResult(
                    "Invalid orderId",
                    new List<string> { "orderId must be greater than 0" }
                );
            }

            var histories = await _statusHistoryRepo.GetByOrderIdAsync(orderId, ct);

            if (histories == null || !histories.Any())
            {
                Console.WriteLine("[ERROR] No order history found");
                return ServiceResult<IEnumerable<OrderStatusHistory>>.FailureResult(
                    "No order history found",
                    new List<string> { $"Order {orderId} has no history" }
                );
            }

            Console.WriteLine("[SUCCESS] Order history retrieved successfully");
            return ServiceResult<IEnumerable<OrderStatusHistory>>.SuccessResult(histories, "Order history retrieved successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EXCEPTION] {ex.Message}");
            _logger.LogError(ex, "Error getting order history for order {OrderId}", orderId);
            return ServiceResult<IEnumerable<OrderStatusHistory>>.FailureResult("Failed to get order history", new List<string> { ex.Message });
        }
    }

    //admin 
    #region Dashboard Stats & Analytics

    public async Task<ServiceResult<decimal>> GetTotalRevenueAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken ct = default)
    {
        try
        {
            Console.WriteLine($"[INFO] Getting total revenue from {fromDate?.ToString("yyyy-MM-dd") ?? "beginning"} to {toDate?.ToString("yyyy-MM-dd") ?? "now"}");
            
            var revenue = await _orderRepo.GetTotalRevenueAsync(fromDate, toDate, ct);
            
            Console.WriteLine($"[SUCCESS] Total revenue retrieved: {revenue:C}");
            return ServiceResult<decimal>.SuccessResult(revenue, "Total revenue retrieved successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EXCEPTION] {ex.Message}");
            _logger.LogError(ex, "Error getting total revenue from {FromDate} to {ToDate}", fromDate, toDate);
            return ServiceResult<decimal>.FailureResult("Failed to get total revenue", new List<string> { ex.Message });
        }
    }

    public async Task<ServiceResult<int>> GetTotalOrdersCountAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken ct = default)
    {
        try
        {
            Console.WriteLine($"[INFO] Getting total orders count from {fromDate?.ToString("yyyy-MM-dd") ?? "beginning"} to {toDate?.ToString("yyyy-MM-dd") ?? "now"}");
            
            var count = await _orderRepo.GetTotalOrdersCountAsync(fromDate, toDate, ct);
            
            Console.WriteLine($"[SUCCESS] Total orders count retrieved: {count}");
            return ServiceResult<int>.SuccessResult(count, "Total orders count retrieved successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EXCEPTION] {ex.Message}");
            _logger.LogError(ex, "Error getting total orders count from {FromDate} to {ToDate}", fromDate, toDate);
            return ServiceResult<int>.FailureResult("Failed to get total orders count", new List<string> { ex.Message });
        }
    }

    public async Task<ServiceResult<decimal>> GetAverageOrderValueAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken ct = default)
    {
        try
        {
            Console.WriteLine($"[INFO] Getting average order value from {fromDate?.ToString("yyyy-MM-dd") ?? "beginning"} to {toDate?.ToString("yyyy-MM-dd") ?? "now"}");
            
            var averageValue = await _orderRepo.GetAverageOrderValueAsync(fromDate, toDate, ct);
            
            Console.WriteLine($"[SUCCESS] Average order value retrieved: {averageValue:C}");
            return ServiceResult<decimal>.SuccessResult(averageValue, "Average order value retrieved successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EXCEPTION] {ex.Message}");
            _logger.LogError(ex, "Error getting average order value from {FromDate} to {ToDate}", fromDate, toDate);
            return ServiceResult<decimal>.FailureResult("Failed to get average order value", new List<string> { ex.Message });
        }
    }

    public async Task<ServiceResult<List<SalesDataPoint>>> GetSalesByMonthAsync(int months = 6, CancellationToken ct = default)
    {
        try
        {
            if (months <= 0)
            {
                Console.WriteLine("[ERROR] Invalid months parameter");
                return ServiceResult<List<SalesDataPoint>>.FailureResult("Invalid months parameter", 
                    new List<string> { "Months must be greater than 0" });
            }

            Console.WriteLine($"[INFO] Getting sales data for last {months} months");
            
            var salesData = await _orderRepo.GetSalesByMonthAsync(months, ct);
            
            Console.WriteLine($"[SUCCESS] Sales data retrieved for {salesData?.Count ?? 0} months");
            return ServiceResult<List<SalesDataPoint>>.SuccessResult(salesData, "Sales data retrieved successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EXCEPTION] {ex.Message}");
            _logger.LogError(ex, "Error getting sales data for {Months} months", months);
            return ServiceResult<List<SalesDataPoint>>.FailureResult("Failed to get sales data", new List<string> { ex.Message });
        }
    }

    public async Task<ServiceResult<IEnumerable<OrderDto>>> GetRecentOrdersAsync(int limit = 10, CancellationToken ct = default)
    {
        try
        {
            if (limit <= 0)
            {
                Console.WriteLine("[ERROR] Invalid limit parameter");
                return ServiceResult<IEnumerable<OrderDto>>.FailureResult("Invalid limit parameter", 
                    new List<string> { "Limit must be greater than 0" });
            }

            Console.WriteLine($"[INFO] Getting recent {limit} orders");
            
            var recentOrders = await _orderRepo.GetRecentOrdersAsync(limit, ct);
            
            // Map to OrderDto
            var orderDtos = recentOrders.Select(order => new OrderDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                OrderStatus = order.OrderStatus,
                OrderDate = order.OrderDate ?? DateTime.UtcNow,
                Subtotal = order.Subtotal,
                TaxAmount = order.TaxAmount,
                ShippingCost = order.ShippingCost,
                DiscountAmount = order.DiscountAmount,
                TotalAmount = order.TotalAmount,
                CustomerName = order.CustomerName,
                CustomerEmail = order.CustomerEmail,
                CustomerPhone = order.CustomerPhone,
                // Note: ShippingAddress, Items, Payment would need additional queries if needed for recent orders
                ShippingAddress = new OrderAddressDto(), // Empty for recent orders list
                Items = new List<OrderItemDto>(), // Empty for recent orders list  
                Payment = new OrderPaymentDto() // Empty for recent orders list
            }).ToList();
            
            Console.WriteLine($"[SUCCESS] {orderDtos.Count} recent orders retrieved and mapped to DTOs");
            return ServiceResult<IEnumerable<OrderDto>>.SuccessResult(orderDtos, "Recent orders retrieved successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EXCEPTION] {ex.Message}");
            _logger.LogError(ex, "Error getting recent orders with limit {Limit}", limit);
            return ServiceResult<IEnumerable<OrderDto>>.FailureResult("Failed to get recent orders", new List<string> { ex.Message });
        }
    }

    #endregion

    private decimal CalculateOrderTotal(OrderCreateDto dto)
    {
        var itemsTotal = dto.Items.Sum(i => i.UnitPrice * i.Quantity);
        return itemsTotal + dto.ShippingCost - (dto.DiscountAmount ?? 0) + (dto.TaxAmount ?? 0);
    }
    
    private string GenerateOrderNumber()
    {
        // Ví dụ: ORD-YYYYMMDD-random4digit
        return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
    }

    private OrderDto MapOrderToDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            OrderStatus = order.OrderStatus,
            OrderDate = order.OrderDate ?? DateTime.MinValue,
            Subtotal = order.Subtotal,
            TaxAmount = order.TaxAmount,
            ShippingCost = order.ShippingCost,
            DiscountAmount = order.DiscountAmount,
            TotalAmount = order.TotalAmount,
            CustomerName = order.CustomerName,
            CustomerEmail = order.CustomerEmail,
            CustomerPhone = order.CustomerPhone,
            
            // Map OrderItems
            Items = order.OrderItems.Select(item => new OrderItemDto
            {
                ProductId = item.ProductId,
                VariantId = item.VariantId,
                ProductName = item.ProductName,
                ProductSku = item.ProductSku,
                VariantName = item.VariantName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TotalPrice = item.TotalPrice,
                ProductAttributes = item.ProductAttributes
            }).ToList(),
            
            // Map ShippingAddress (if exists)
            ShippingAddress = order.OrderAddresses.FirstOrDefault(a => a.AddressType == "shipping") is var shippingAddr && shippingAddr != null
                ? new OrderAddressDto
                {
                    AddressType = shippingAddr.AddressType,
                    RecipientName = shippingAddr.RecipientName,
                    Phone = shippingAddr.Phone,
                    AddressLine1 = shippingAddr.AddressLine1,
                    AddressLine2 = shippingAddr.AddressLine2,
                    Ward = shippingAddr.Ward,
                    District = shippingAddr.District,
                    City = shippingAddr.City,
                    Province = shippingAddr.Province,
                    PostalCode = shippingAddr.PostalCode
                } : new OrderAddressDto(),
            
            // Map Payment (if exists)
            Payment = order.OrderPayments.FirstOrDefault() is var payment && payment != null
                ? new OrderPaymentDto
                {
                    Id = payment.Id,
                    OrderId = payment.OrderId,
                    Amount = payment.Amount,
                    PaymentMethod = payment.PaymentMethod,
                    PaymentStatus = payment.PaymentStatus,
                    PaymentRequestId = payment.PaymentRequestId ?? "",
                    Currency = payment.Currency,
                    TransactionId = payment.TransactionId,
                    PaidAt = payment.PaidAt,
                    FailedAt = payment.FailedAt,
                    CreatedAt = payment.CreatedAt,
                    UpdatedAt = payment.UpdatedAt
                } : new OrderPaymentDto()
        };
    }
}
