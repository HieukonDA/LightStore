using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using TheLightStore.Application.DTOs.Orders;
using TheLightStore.Application.Interfaces;

namespace TheLightStore.Controllers.Orders;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrderController> _logger;
    private static readonly Serilog.ILogger OrderLogger = Log.ForContext("OrderProcess", true);

    public OrderController(IOrderService orderService, ILogger<OrderController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllOrders(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        [FromQuery] string? sort = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        // Validate parameters
        if (page < 1) page = 1;
        if (size < 1) size = 10;
        if (size > 100) size = 100; // Limit max page size

        var request = new PagedRequest
        {
            Page = page,
            Size = size,
            Sort = sort,
            Search = search
        };

        var result = await _orderService.GetAllAsync(request, ct);

        if (!result.Success)
            return BadRequest(new { message = result.Message, errors = result.Errors });

        return Ok(new { success = true, data = result.Data, message = result.Message });
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] OrderCreateDto request, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        OrderLogger.Information("=== ORDER PROCESS: CREATE ORDER REQUEST ===");
        OrderLogger.Information("ORDER PROCESS: Full OrderCreateDto: {@OrderRequest}", request);
        
        // Log chi tiết từng phần
        OrderLogger.Information("ORDER PROCESS: Customer Info - UserId: {UserId}, Name: {Name}, Email: {Email}, Phone: {Phone}", 
            request.UserId, request.CustomerName, request.CustomerEmail, request.CustomerPhone);
        
        if (request.ShippingAddress != null)
        {
            OrderLogger.Information("ORDER PROCESS: Shipping Address - {@ShippingAddress}", request.ShippingAddress);
        }
        else
        {
            OrderLogger.Warning("ORDER PROCESS: ShippingAddress is NULL!");
        }
        
        if (request.Items != null && request.Items.Any())
        {
            OrderLogger.Information("ORDER PROCESS: Items count: {Count}", request.Items.Count);
            foreach (var item in request.Items)
            {
                OrderLogger.Information("ORDER PROCESS: CREATE Item - ProductId: {ProductId}, VariantId: {VariantId}, Quantity: {Quantity}, UnitPrice: {Price}", 
                    item.ProductId, item.VariantId, item.Quantity, item.UnitPrice);
            }
        }
        else
        {
            OrderLogger.Warning("ORDER PROCESS: Items is NULL or empty!");
        }

        var result = await _orderService.CreateOrderAsync(request, ct);

        if (!result.Success)
            return BadRequest(new { message = result.Message, errors = result.Errors });

        return Ok(new { success = true, data = result.Data, message = result.Message });
    }

    [HttpGet("{orderId:int}")]
    public async Task<IActionResult> GetOrderById(int orderId, CancellationToken ct = default)
    {
        OrderLogger.Information("=== ORDER PROCESS: GET ORDER BY ID {OrderId} ===", orderId);
        
        var result = await _orderService.GetOrderByIdAsync(orderId, ct);

        if (!result.Success)
        {
            OrderLogger.Warning("ORDER PROCESS: Order {OrderId} not found - {Message}", orderId, result.Message);
            return NotFound(new { message = result.Message, errors = result.Errors });
        }

        // 🔥 Log variant information in order items
        if (result.Data?.Items != null)
        {
            OrderLogger.Information("ORDER PROCESS: Order {OrderId} has {ItemCount} items", orderId, result.Data.Items.Count);
            foreach (var item in result.Data.Items)
            {
                OrderLogger.Information("ORDER PROCESS: Item - ProductId: {ProductId}, VariantId: {VariantId}, ProductName: {ProductName}, VariantName: {VariantName}", 
                    item.ProductId, item.VariantId, item.ProductName, item.VariantName);
            }
        }

        return Ok(new { success = true, data = result.Data, message = result.Message });
    }

    [HttpGet("number/{orderNumber}")]
    public async Task<IActionResult> GetOrderByOrderNumber(string orderNumber, CancellationToken ct = default)
    {
        var result = await _orderService.GetOrderByOrderNumberAsync(orderNumber, ct);

        if (!result.Success)
            return NotFound(new { message = result.Message, errors = result.Errors });

        return Ok(new { success = true, data = result.Data, message = result.Message });
    }

    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetOrdersByUser(int userId, CancellationToken ct = default)
    {
        var result = await _orderService.GetOrdersByUserAsync(userId, ct);

        if (result.Data == null || !result.Data.Any())
        {
            return Ok(new
            {
                message = "No orders found for user",
                data = new List<object>()
            });
        }

        return Ok(new
        {
            message = "Orders retrieved",
            data = result.Data
        });
    }

    [HttpGet("{orderId:int}/history")]
    public async Task<IActionResult> GetOrderHistory(int orderId, CancellationToken ct = default)
    {
        var result = await _orderService.GetOrderHistoryAsync(orderId, ct);

        if (!result.Success)
            return NotFound(new { message = result.Message, errors = result.Errors });

        return Ok(new { success = true, data = result.Data, message = result.Message });
    }



    [HttpPut("{orderId:int}/confirm")]
    public async Task<IActionResult> ConfirmOrder(int orderId, [FromBody] string? adminNotes, CancellationToken ct = default)
    {
        var result = await _orderService.ConfirmOrderAsync(orderId, adminNotes, ct);

        if (!result.Success)
            return BadRequest(new { message = result.Message, errors = result.Errors });

        return Ok(new { success = true, message = result.Message });
    }

    [HttpPut("{orderId}/process")]
    public async Task<IActionResult> ProcessOrder(int orderId, [FromBody] string? processingNotes, CancellationToken ct = default)
    {
        var result = await _orderService.ProcessOrderAsync(orderId, processingNotes, ct);

        if (!result.Success)
            return BadRequest(new { message = result.Message, errors = result.Errors });

        return Ok(new { success = true, message = result.Message });
    }

    [HttpPut("{orderId:int}/ship")]
    public async Task<IActionResult> ShipOrder(int orderId, [FromBody] string? trackingNumber, CancellationToken ct = default)
    {
        var result = await _orderService.ShipOrderAsync(orderId, trackingNumber, ct);

        if (!result.Success)
            return BadRequest(new { message = result.Message, errors = result.Errors });

        return Ok(new { success = true, message = result.Message });
    }

    [HttpPut("{orderId:int}/deliver")]
    public async Task<IActionResult> DeliverOrder(int orderId, CancellationToken ct = default)
    {
        var result = await _orderService.DeliverOrderAsync(orderId, ct);

        if (!result.Success)
            return BadRequest(new { message = result.Message, errors = result.Errors });

        return Ok(new { success = true, message = result.Message });
    }

    [HttpPost("{orderId:int}/cancel")]
    public async Task<IActionResult> CancelOrder(int orderId, [FromBody] CancelOrderRequest? request, CancellationToken ct = default)
    {
        OrderLogger.Information("=== ORDER PROCESS: CANCEL ORDER REQUEST ===");
        OrderLogger.Information("ORDER CANCEL: OrderId: {OrderId}, Reason: {Reason}", orderId, request?.Reason);
        
        var result = await _orderService.CancelOrderAsync(orderId, request?.Reason, ct);

        if (!result.Success)
        {
            OrderLogger.Warning("ORDER CANCEL: Failed for OrderId: {OrderId} - {Message}", orderId, result.Message);
            return BadRequest(new { 
                success = false, 
                message = result.Message, 
                errors = result.Errors 
            });
        }

        OrderLogger.Information("ORDER CANCEL: Completed successfully for OrderId: {OrderId}", orderId);
        return Ok(new { 
            success = true, 
            message = result.Message,
            data = new { 
                orderId = orderId, 
                cancelledAt = DateTime.UtcNow,
                reason = request?.Reason ?? "No reason provided"
            }
        });
    }
}
