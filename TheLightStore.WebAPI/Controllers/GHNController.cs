using Microsoft.AspNetCore.Mvc;
using TheLightStore.Application.DTOs.GHN;
using TheLightStore.Application.Interfaces;
using TheLightStore.Application.Services;

namespace TheLightStore.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class GHNController : ControllerBase
{
    private readonly ILogger<GHNController> _logger;
    private readonly IGHNService _ghnService;
    private readonly OrderProcessingService _orderProcessingService;

    public GHNController(
        ILogger<GHNController> logger,
        IGHNService ghnService,
        OrderProcessingService orderProcessingService)
    {
        _logger = logger;
        _ghnService = ghnService;
        _orderProcessingService = orderProcessingService;
    }

    /// <summary>
    /// Test tạo đơn GHN với dữ liệu mẫu
    /// </summary>
    /// <returns>Kết quả tạo đơn GHN</returns>
    [HttpPost("test-create-order")]
    public async Task<IActionResult> TestCreateOrder()
    {
        try
        {
            _logger.LogInformation("🧪 GHN TEST: Creating test order");

            var testRequest = new CreateShippingOrderRequest
            {
                client_order_code = $"TEST_{DateTime.Now:yyyyMMddHHmmss}",
                payment_type_id = 2, // Người nhận thanh toán
                required_note = "KHONGCHOXEMHANG",
                note = "Test đơn hàng từ TheLightStore",

                // Thông tin người nhận (test)
                to_name = "Nguyen Van Test",
                to_phone = "0987654321",
                to_address = "72 Thành Thái, Phường 14",
                to_ward_name = "Phường 14",
                to_district_name = "Quận 10", 
                to_province_name = "Hồ Chí Minh",

                // Thông tin đơn hàng
                cod_amount = 200000,
                content = "Test sản phẩm",
                length = 20,
                width = 15,
                height = 10,
                weight = 500,
                service_type_id = 2, // Hàng nhẹ
                insurance_value = 200000,

                items = new List<GHNOrderItem>
                {
                    new GHNOrderItem
                    {
                        name = "Áo thun test",
                        code = "TEST001", 
                        quantity = 1,
                        price = 200000,
                        length = 20,
                        width = 15,
                        height = 5,
                        weight = 500,
                        category = new GHNItemCategory { level1 = "Thời trang" }
                    }
                }
            };

            var result = await _ghnService.CreateShippingOrderAsync(testRequest);

            if (result.Success)
            {
                _logger.LogInformation("✅ GHN TEST: Successfully created test order - GHNOrderCode: {OrderCode}", 
                    result.Data?.order_code);

                return Ok(new 
                {
                    success = true,
                    message = "Test order created successfully",
                    data = result.Data
                });
            }
            else
            {
                _logger.LogWarning("❌ GHN TEST: Failed to create test order - Errors: {@Errors}", result.Errors);
                return BadRequest(new
                {
                    success = false,
                    message = "Failed to create test order",
                    errors = result.Errors
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ GHN TEST ERROR: Exception during test");
            return StatusCode(500, new 
            {
                success = false,
                message = "Internal server error",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Tạo đơn GHN cho đơn hàng có sẵn
    /// </summary>
    /// <param name="orderId">ID đơn hàng</param>
    /// <returns>Kết quả tạo đơn GHN</returns>
    [HttpPost("create-order/{orderId:int}")]
    public async Task<IActionResult> CreateOrderForExistingOrder(int orderId)
    {
        try
        {
            _logger.LogInformation("🚛 GHN: Creating shipping order for OrderId: {OrderId}", orderId);

            var result = await _ghnService.CreateShippingOrderAsync(orderId);

            if (result.Success)
            {
                return Ok(new 
                {
                    success = true,
                    message = "GHN order created successfully",
                    data = result.Data
                });
            }
            else
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Failed to create GHN order",
                    errors = result.Errors
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ GHN ERROR: Exception for OrderId: {OrderId}", orderId);
            return StatusCode(500, new 
            {
                success = false,
                message = "Internal server error",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Xử lý đơn hàng (tự động tạo GHN nếu cần)
    /// </summary>
    /// <param name="orderId">ID đơn hàng</param>
    /// <param name="isPaymentCompleted">Thanh toán đã hoàn tất chưa</param>
    /// <returns>Kết quả xử lý</returns>
    [HttpPost("process-order/{orderId:int}")]
    public async Task<IActionResult> ProcessOrder(int orderId, [FromQuery] bool isPaymentCompleted = false)
    {
        try
        {
            _logger.LogInformation("📦 ORDER: Processing OrderId: {OrderId}, PaymentCompleted: {PaymentCompleted}", 
                orderId, isPaymentCompleted);

            var result = await _orderProcessingService.ProcessOrderAsync(orderId, isPaymentCompleted);

            if (result.Success)
            {
                return Ok(new 
                {
                    success = true,
                    message = result.Message,
                    data = result.Data
                });
            }
            else
            {
                return BadRequest(new
                {
                    success = false,
                    message = result.Message,
                    errors = result.Errors
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ ORDER PROCESSING ERROR: Exception for OrderId: {OrderId}", orderId);
            return StatusCode(500, new 
            {
                success = false,
                message = "Internal server error",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Webhook để nhận thông báo thanh toán hoàn tất
    /// </summary>
    /// <param name="orderId">ID đơn hàng</param>
    /// <returns>Kết quả xử lý</returns>
    [HttpPost("payment-completed/{orderId:int}")]
    public async Task<IActionResult> PaymentCompleted(int orderId)
    {
        try
        {
            _logger.LogInformation("💳 PAYMENT: Payment completed for OrderId: {OrderId}", orderId);

            var result = await _orderProcessingService.HandlePaymentCompletedAsync(orderId);

            if (result.Success)
            {
                return Ok(new 
                {
                    success = true,
                    message = "Payment processed successfully"
                });
            }
            else
            {
                return BadRequest(new
                {
                    success = false,
                    message = result.Message,
                    errors = result.Errors
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ PAYMENT ERROR: Exception for OrderId: {OrderId}", orderId);
            return StatusCode(500, new 
            {
                success = false,
                message = "Internal server error",
                error = ex.Message
            });
        }
    }
}