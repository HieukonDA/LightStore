using Microsoft.AspNetCore.Mvc;
using TheLightStore.DTOs.Orders;
using TheLightStore.Interfaces.Orders;

namespace TheLightStore.Controllers.Orders;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrderController> _logger;

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

        var result = await _orderService.CreateOrderAsync(request, ct);

        if (!result.Success)
            return BadRequest(new { message = result.Message, errors = result.Errors });

        return Ok(new { success = true, data = result.Data, message = result.Message });
    }

    [HttpGet("{orderId:int}")]
    public async Task<IActionResult> GetOrderById(int orderId, CancellationToken ct = default)
    {
        var result = await _orderService.GetOrderByIdAsync(orderId, ct);

        if (!result.Success)
            return NotFound(new { message = result.Message, errors = result.Errors });

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
    public async Task<IActionResult> CancelOrder(int orderId,[FromBody] CancelOrderRequest? request, CancellationToken ct = default)
    {
        var result = await _orderService.CancelOrderAsync(orderId, request?.Reason, ct);

        if (!result.Success)
            return BadRequest(new { message = result.Message, errors = result.Errors });

        return Ok(new { success = true, message = result.Message });
    }
}
