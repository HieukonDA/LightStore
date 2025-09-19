using TheLightStore.Interfaces.Orders;
using TheLightStore.Interfaces.Payment;

namespace TheLightStore.Controllers.Checkout;

[Route("api/v1/checkout")]
[ApiController]
public class CheckoutController : ControllerBase
{
    private readonly IMomoService _momoService;
    private readonly IOrderRepo _orderRepo;
    private readonly IPaymentService _paymentService;
    private readonly ILogger<CheckoutController> _logger;

    public CheckoutController(
        IMomoService momoService,
        IOrderRepo orderRepo,
        IPaymentService paymentService,
        ILogger<CheckoutController> logger)
    {
        _momoService = momoService;
        _orderRepo = orderRepo;
        _paymentService = paymentService;
        _logger = logger;
    }

    // [HttpPost("create-momo-payment")]
    // public async Task<IActionResult> CreateMomoPayment([FromBody] CreatePaymentRequest request)
    // {
    //     try
    //     {
    //         // Validate request
    //         if (request?.OrderId <= 0)
    //         {
    //             return BadRequest("Invalid order ID");
    //         }

    //         // Get order
    //         var order = await _orderRepo.GetByIdAsync(request.OrderId);
    //         if (order == null)
    //         {
    //             _logger.LogWarning("Order not found: {OrderId}", request.OrderId);
    //             return NotFound("Order not found");
    //         }

    //         // Find pending payment
    //         var payment = order.OrderPayments?.FirstOrDefault(p => p.PaymentStatus == "pending");
    //         if (payment == null)
    //         {
    //             _logger.LogWarning("No pending payment found for order: {OrderId}", request.OrderId);
    //             return BadRequest("No pending payment found");
    //         }

    //         // Validate order amount
    //         if (order.TotalAmount <= 0)
    //         {
    //             return BadRequest("Invalid order amount");
    //         }

    //         // Create order info for MoMo
    //         var orderInfo = new MomoOnetimePaymentRequest
    //         {
    //             orderId = payment.PaymentRequestId.ToString(),
    //             amount = order.TotalAmount, // ✅ Theo model gốc là string
    //             fullName = order.CustomerName ?? "Guest",
    //             orderInfo = $"Thanh toán đơn hàng {order.OrderNumber}"
    //         };

    //         _logger.LogInformation("Creating MoMo payment for order {OrderId}, amount {Amount}",
    //                              order.Id, order.TotalAmount);

    //         var response = await _momoService.CreatePaymentAsync(orderInfo);

    //         // Update payment with MoMo transaction info
    //         payment.TransactionId = response.RequestId;
    //         payment.PaymentMethod = "MoMo";
    //         await _orderRepo.SaveChangesAsync(); // Assuming this exists

    //         return Ok(new CreatePaymentResponse
    //         {
    //             Success = true,
    //             PayUrl = response.PayUrl,
    //             QrCodeUrl = response.QrCodeUrl,
    //             OrderId = order.Id,
    //             Amount = order.TotalAmount,
    //             RequestId = response.RequestId
    //         });
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Invalid argument when creating MoMo payment for order {OrderId}", request?.OrderId);
    //         return BadRequest($"Invalid request: {ex.Message}");
    //     }
    // }

    // [HttpGet("momo-callback")]
    // public async Task<IActionResult> MomoCallback([FromQuery] IQueryCollection query)
    // {
    //     try
    //     {
    //         _logger.LogInformation("Received MoMo callback with query: {Query}",
    //                              string.Join(", ", query.Select(x => $"{x.Key}={x.Value}")));

    //         var executeResult = _momoService.PaymentExecuteAsync(query);

    //         if (executeResult == null)
    //         {
    //             _logger.LogError("MoMo callback returned null result");
    //             return BadRequest("Invalid callback data");
    //         }

    //         // Parse PaymentRequestId from OrderId
    //         if (!Guid.TryParse(executeResult.OrderId, out var paymentRequestId))
    //         {
    //             _logger.LogError("Invalid PaymentRequestId format: {OrderId}", executeResult.OrderId);
    //             return BadRequest("Invalid payment request ID");
    //         }

    //         var isSuccess = query["errorCode"] == "0"; // ✅ Đúng theo code gốc
    //         var transactionId = query["transId"].ToString();

    //         _logger.LogInformation("Processing MoMo payment result - PaymentRequestId: {PaymentRequestId}, Success: {Success}, TransactionId: {TransactionId}",
    //                              paymentRequestId, isSuccess, transactionId);

    //         // Process payment result
    //         await _paymentService.HandlePaymentResultAsync(paymentRequestId, isSuccess, transactionId);

    //         // Return appropriate redirect to Frontend
    //         var frontendBaseUrl = "https://localhost:5173"; // ✅ Frontend URL is correct here

    //         if (isSuccess)
    //         {
    //             _logger.LogInformation("MoMo payment successful for PaymentRequestId: {PaymentRequestId}", paymentRequestId);
    //             return Redirect($"{frontendBaseUrl}/order/success?orderId={paymentRequestId}");
    //         }
    //         else
    //         {
    //             _logger.LogWarning("MoMo payment failed for PaymentRequestId: {PaymentRequestId}, ErrorCode: {ErrorCode}",
    //                              paymentRequestId, query["errorCode"]);
    //             return Redirect($"{frontendBaseUrl}/order/failure?orderId={paymentRequestId}&error={query["message"]}");
    //         }
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Error processing MoMo callback");
    //         return StatusCode(500, "Error processing payment callback");
    //     }
    // }


    [HttpPost("momo-ipn")]
    public async Task<IActionResult> MomoNotify([FromBody] MomoConfig ipn)
    {

        try
        {
            // 1. Validate chữ ký
            if (!_momoService.ValidateSignature(ipn))
            {
                _logger.LogWarning("Invalid signature for payment request {RequestId}", ipn.RequestId);
                return BadRequest(new { message = "Invalid signature" });
            }

            bool isSuccess = ipn.ErrorCode == 0;

            // Xử lý thành công
            await _paymentService.HandlePaymentResultAsync(ipn.RequestId, isSuccess, ipn.TransId.ToString());

            _logger.LogInformation("Processed Momo IPN for RequestId {RequestId}, Success = {IsSuccess}",
                    ipn.RequestId, isSuccess);

            // 4. Trả về response cho Momo
            return Ok(new
            {
                message = "success",
                resultCode = 0
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Momo IPN for RequestId {RequestId}", ipn.RequestId);
            return StatusCode(500, "Error processing IPN");
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
}



public class CreatePaymentRequest
{
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = "MoMo"; // Mặc định là MoMo
}
