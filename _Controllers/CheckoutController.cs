using TheLightStore.Interfaces.Orders;
using TheLightStore.Interfaces.Payment;
using Serilog;

namespace TheLightStore.Controllers.Checkout;

[ApiController]
[Route("api/v1/[controller]")]
public class CheckoutController : ControllerBase
{
    private readonly IMomoService _momoService;
    private readonly IOrderRepo _orderRepo;
    private readonly IPaymentService _paymentService;
    private readonly IOrderService _orderService;
    private readonly ILogger<CheckoutController> _logger;
    private static readonly Serilog.ILogger OrderLogger = Log.ForContext("OrderProcess", true);

    public CheckoutController(
        IMomoService momoService,
        IOrderRepo orderRepo,
        IPaymentService paymentService,
        IOrderService orderService,
        ILogger<CheckoutController> logger)
    {
        _momoService = momoService;
        _orderRepo = orderRepo;
        _paymentService = paymentService;
        _orderService = orderService;
        _logger = logger;
    }

    [HttpPost("momo-ipn")]
    public async Task<IActionResult> MomoNotify([FromBody] MomoIPNRequest ipn)
    {
        _logger.LogInformation("=== MOMO IPN RECEIVED ===");
        _logger.LogInformation("IPN Data: {@Ipn}", ipn);
        _logger.LogInformation("Request Headers: {Headers}",
            string.Join(", ", Request.Headers.Select(h => $"{h.Key}={h.Value}")));

        // Log raw request body
        Request.EnableBuffering();
        Request.Body.Position = 0;
        using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        Request.Body.Position = 0;
        _logger.LogInformation("Raw Request Body: {Body}", body);

        try
        {
            if (ipn == null)
            {
                _logger.LogError("IPN request is null");
                return BadRequest(new { message = "Invalid IPN data" });
            }

            OrderLogger.Information("=== ORDER PROCESS: PAYMENT MOMO CALLBACK ====");
            OrderLogger.Information("IPN Details - OrderId: {OrderId}, Amount: {Amount}, ResultCode: {ResultCode}, RequestId: {RequestId}, TransId: {TransId}",
                ipn.OrderId, ipn.Amount, ipn.ResultCode, ipn.RequestId, ipn.TransId);
            OrderLogger.Information("IPN OrderInfo (PaymentRequestId): {OrderInfo}", ipn.OrderInfo);
            OrderLogger.Information("IPN ID Mapping - MoMoOrderId: {OrderId}, MoMoRequestId: {RequestId}, OrderInfo: {OrderInfo}", 
                ipn.OrderId, ipn.RequestId, ipn.OrderInfo);

            // 1. Validate chữ ký
            if (!_momoService.ValidateSignature(ipn))
            {
                _logger.LogWarning("Invalid signature for payment request {RequestId}", ipn.RequestId);
                return BadRequest(new { message = "Invalid signature" });
            }

            _logger.LogInformation("Signature validation passed for RequestId: {RequestId}", ipn.RequestId);

            bool isSuccess = ipn.ResultCode == 0;
            OrderLogger.Information("Payment result: Success={IsSuccess}, ResultCode={ResultCode}", isSuccess, ipn.ResultCode);

            // Xử lý thành công
            await _paymentService.HandlePaymentResultAsync(ipn.OrderInfo, isSuccess, ipn.TransId.ToString());

            OrderLogger.Information("Successfully processed Momo IPN for RequestId {RequestId}, OrderInfo = {OrderInfo}, Success = {IsSuccess}",
                    ipn.RequestId, ipn.OrderInfo, isSuccess);

            // 4. Trả về response cho Momo
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Momo IPN for RequestId {RequestId}", ipn?.RequestId);
            return StatusCode(500, "Error processing IPN");
        }

    }


    [HttpPost("test-ipn")]
    public IActionResult TestIpn([FromBody] object body)
    {
        Console.WriteLine("=== TEST IPN RECEIVED ===");
        Console.WriteLine(body);
        return Ok(new { message = "received" });
    }

    [HttpGet("test-logs")]
    public IActionResult TestLogs()
    {
        OrderLogger.Information("=== ORDER PROCESS: PAYMENT TEST LOGS ====");
        OrderLogger.Information("Testing ORDER PROCESS file logging at {Time}", DateTime.Now);
        
        // These regular logs won't appear in file because they don't contain ORDER PROCESS
        _logger.LogInformation("Regular log - Test logging endpoint called at {Time}", DateTime.Now);
        _logger.LogWarning("Regular log - This is a test warning log");
        _logger.LogError("Regular log - This is a test error log");
        
        Console.WriteLine("=== CONSOLE OUTPUT TEST ===");
        Console.WriteLine($"Console log test at {DateTime.Now}");
        
        return Ok(new { 
            message = "ORDER PROCESS logs written to file. Check logs/order-process-*.txt", 
            timestamp = DateTime.Now,
            logLevels = new[] { "Information", "Warning", "Error" }
        });
    }

    [HttpGet("test-commit/{orderId}")]
    public async Task<IActionResult> TestCommitInventory(int orderId)
    {
        try
        {
            OrderLogger.Information("=== ORDER PROCESS: TEST COMMIT INVENTORY ====");
            OrderLogger.Information("Manual test commit for OrderId: {OrderId}", orderId);
            
            // Sử dụng InventoryService trực tiếp để test
            // Tôi sẽ thêm IInventoryService vào constructor
            
            return Ok(new { 
                message = "Test commit logged. Check order-process log file",
                orderId = orderId,
                timestamp = DateTime.Now
            });
        }
        catch (Exception ex)
        {
            OrderLogger.Error(ex, "ORDER PROCESS: Error testing commit for order {OrderId}", orderId);
            return StatusCode(500, new { message = ex.Message });
        }
    }




    [HttpPost("payment")]
    public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequest request)
    {
        if (request.Amount <= 0)
        {
            return BadRequest(new { message = "Amount must be greater than zero" });
        }

        try
        {
            var payment = await _paymentService.CreatePaymentAsync(
                request.OrderId,
                request.Amount,
                request.PaymentMethod
            );

            return Ok(payment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment for order {OrderId}", request.OrderId);
            return StatusCode(500, new { message = "Failed to create payment" });
        }
    }

    [HttpGet("test-stock/{productId}")]
    public IActionResult TestStock(int productId)
    {
        try
        {
            OrderLogger.Information("ORDER PROCESS: Testing stock for ProductId: {ProductId}", productId);
            
            var result = new
            {
                ProductId = productId,
                Message = "Check logs for detailed stock information",
                Timestamp = DateTime.UtcNow
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            OrderLogger.Error(ex, "ORDER PROCESS: Error testing stock for ProductId: {ProductId}", productId);
            return StatusCode(500, ex.Message);
        }
    }
}


public class MomoIPNRequest
{
    public string OrderType { get; set; }
    public long Amount { get; set; }
    public string PartnerCode { get; set; }
    public string OrderId { get; set; }
    public string ExtraData { get; set; }
    public string Signature { get; set; }
    public long TransId { get; set; }
    public long ResponseTime { get; set; }
    public int ResultCode { get; set; }
    public string Message { get; set; }
    public string PayType { get; set; }
    public string RequestId { get; set; }
    public string OrderInfo { get; set; }
}

   


public class CreatePaymentRequest
{
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = "MoMo"; // Mặc định là MoMo
}
