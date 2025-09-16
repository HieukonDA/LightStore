using TheLightStore.DTOs.Orders;
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
    // private readonly INotificationService _notificationService;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepo orderRepo,
        IOrderItemRepo orderItemRepo,
        IOrderStatusHistoryRepo statusHistoryRepo,
        IInventoryService inventoryService,
        IPaymentService paymentService,
        // INotificationService notificationService,
        ILogger<OrderService> logger)
    {
        _orderRepo = orderRepo;
        _orderItemRepo = orderItemRepo;
        _statusHistoryRepo = statusHistoryRepo;
        _inventoryService = inventoryService;
        _paymentService = paymentService;
        // _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<Order> CreateOrderAsync(OrderCreateDto dto, CancellationToken ct = default)
    {
        // 1. Tạo Order (Pending)
        var order = new Order
        {
            UserId = dto.UserId ?? 0,
            OrderNumber = GenerateOrderNumber(),
            CustomerName = dto.CustomerName,
            CustomerEmail = dto.CustomerEmail,
            CustomerPhone = dto.CustomerPhone,
            Subtotal = dto.Items.Sum(i => i.UnitPrice * i.Quantity),
            TotalAmount = CalculateOrderTotal(dto),
            TaxAmount = dto.TaxAmount,
            ShippingCost = dto.ShippingCost,
            DiscountAmount = dto.DiscountAmount,
            OrderStatus = "PENDING",
            CustomerNotes = dto.CustomerNotes,
            OrderDate = DateTime.UtcNow,
            OrderItems = dto.Items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                VariantId = i.VariantId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,                           // ✅ thay Price -> UnitPrice
                TotalPrice = i.UnitPrice * i.Quantity,
                ProductName = i.ProductName,                       // ✅ cần có trong DTO
                ProductSku = i.ProductSku,                         // ✅ cần có trong DTO
                VariantName = i.VariantName,                       // ✅ nếu có
                ProductAttributes = i.ProductAttributes
            }).ToList()
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
            throw new InvalidOperationException("One or more items are out of stock.");
        }

        // 3. Create Pending Payment
        var payment = await _paymentService.CreatePaymentAsync(order.Id, order.TotalAmount, dto.PaymentMethod);

        // 4. Update Order status -> WAITING_FOR_PAYMENT
        order.OrderStatus = "WAITING_FOR_PAYMENT";
        await _orderRepo.UpdateAsync(order, ct);
        await _orderRepo.SaveChangesAsync(ct);

        // 5. Add status history
        await _statusHistoryRepo.AddAsync(new OrderStatusHistory
        {
            OrderId = order.Id,
            OldStatus = null, // lúc tạo đơn hàng thì chưa có trạng thái cũ
            NewStatus = order.OrderStatus, // dùng Order.OrderStatus hiện tại
            Comment = "Waiting for payment", // thay cho Notes
            ChangedAt = DateTime.UtcNow
        }, ct);
        await _orderRepo.SaveChangesAsync(ct);

        return order;
    }

    public Task<Order?> GetOrderByIdAsync(int orderId, CancellationToken ct = default)
        => _orderRepo.GetByIdAsync(orderId);

    public Task<IEnumerable<Order>> GetOrdersByUserAsync(int userId, CancellationToken ct = default)
        => _orderRepo.GetByUserIdAsync(userId);

    public async Task ConfirmOrderAsync(int orderId, string? adminNotes = null, CancellationToken ct = default)
    {
        var order = await _orderRepo.GetByIdAsync(orderId) ?? throw new KeyNotFoundException();
        order.OrderStatus = "CONFIRMED";
        await _orderRepo.UpdateAsync(order, ct);
        await _statusHistoryRepo.AddAsync(new OrderStatusHistory
        {
            OrderId = order.Id,
            OldStatus = null, // lúc tạo đơn hàng thì chưa có trạng thái cũ
            NewStatus = order.OrderStatus, // dùng Order.OrderStatus hiện tại
            Comment = "Waiting for payment", // thay cho Notes
            ChangedAt = DateTime.UtcNow
        }, ct);
        await _orderRepo.SaveChangesAsync(ct);
    }

    public async Task ShipOrderAsync(int orderId, string? trackingNumber = null, CancellationToken ct = default)
    {
        var order = await _orderRepo.GetByIdAsync(orderId) ?? throw new KeyNotFoundException();
        order.OrderStatus = "SHIPPED";
        order.OrderNumber = trackingNumber;
        await _orderRepo.UpdateAsync(order, ct);
        await _statusHistoryRepo.AddAsync(new OrderStatusHistory
        {
            OrderId = order.Id,
            OldStatus = null, // lúc tạo đơn hàng thì chưa có trạng thái cũ
            NewStatus = order.OrderStatus, // dùng Order.OrderStatus hiện tại
            Comment = "Waiting for payment", // thay cho Notes
            ChangedAt = DateTime.UtcNow
        }, ct);
        await _orderRepo.SaveChangesAsync(ct);
    }

    public async Task DeliverOrderAsync(int orderId, CancellationToken ct = default)
    {
        var order = await _orderRepo.GetByIdAsync(orderId) ?? throw new KeyNotFoundException();
        order.OrderStatus = "DELIVERED";
        await _orderRepo.UpdateAsync(order, ct);
        await _statusHistoryRepo.AddAsync(new OrderStatusHistory
        {
            OrderId = order.Id,
            OldStatus = null, // lúc tạo đơn hàng thì chưa có trạng thái cũ
            NewStatus = order.OrderStatus, // dùng Order.OrderStatus hiện tại
            Comment = "Waiting for payment", // thay cho Notes
            ChangedAt = DateTime.UtcNow
        }, ct);
        await _orderRepo.SaveChangesAsync(ct);
    }

    public async Task CancelOrderAsync(int orderId, string? reason = null, CancellationToken ct = default)
    {
        var order = await _orderRepo.GetByIdAsync(orderId) ?? throw new KeyNotFoundException();
        order.OrderStatus = "CANCELLED";
        await _orderRepo.UpdateAsync(order, ct);

        // Release stock if reserved
        await _inventoryService.ReleaseReservationsAsync(order.Id.ToString());

        await _statusHistoryRepo.AddAsync(new OrderStatusHistory
        {
            OrderId = order.Id,
            OldStatus = null, // lúc tạo đơn hàng thì chưa có trạng thái cũ
            NewStatus = order.OrderStatus, // dùng Order.OrderStatus hiện tại
            Comment = "Waiting for payment", // thay cho Notes
            ChangedAt = DateTime.UtcNow
        }, ct);

        await _orderRepo.SaveChangesAsync(ct);
    }

    public Task<IEnumerable<OrderStatusHistory>> GetOrderHistoryAsync(int orderId, CancellationToken ct = default)
        => _statusHistoryRepo.GetByOrderIdAsync(orderId, ct);

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
}
