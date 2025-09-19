using Microsoft.AspNetCore.Mvc;
using TheLightStore.DTOs.Orders;
using TheLightStore.Interfaces.Orders;

namespace TheLightStore.Controllers.Orders;

[ApiController]
[Route("api/v1/[controller]")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrderController> _logger;

    public OrderController(IOrderService orderService, ILogger<OrderController> logger)
    {
        _orderService = orderService;
        _logger = logger;
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

    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetOrdersByUser(int userId, CancellationToken ct = default)
    {
        var result = await _orderService.GetOrdersByUserAsync(userId, ct);

        if (!result.Success)
            return NotFound(new { message = result.Message, errors = result.Errors });

        return Ok(new { success = true, data = result.Data, message = result.Message });
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
