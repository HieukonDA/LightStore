using TheLightStore.Dtos.Orders;
using TheLightStore.Interfaces.Notifications;
using TheLightStore.Interfaces.Orders;
using TheLightStore.Interfaces.Payment;
using Serilog;

namespace TheLightStore.Services.Payment;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepo _paymentRepo;
    private readonly IOrderRepo _orderRepo;
    private readonly IInventoryService _inventoryService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<PaymentService> _logger;
    private readonly IMomoService _momoService;
    private readonly IConfiguration _configuration;
    private static readonly Serilog.ILogger OrderLogger = Log.ForContext("OrderProcess", true);

    public PaymentService(IPaymentRepo paymentRepo, IOrderRepo orderRepo, IInventoryService inventoryService, INotificationService notificationService, ILogger<PaymentService> logger, IMomoService momoService, IConfiguration configuration)
    {
        _paymentRepo = paymentRepo;
        _orderRepo = orderRepo;
        _inventoryService = inventoryService;
        _notificationService = notificationService;
        _logger = logger;
        _momoService = momoService;
        _configuration = configuration;
    }

    public async Task<OrderPaymentDto> CreatePaymentAsync(int orderId, decimal amount, string method)
    {
        var paymentRequestId = Guid.NewGuid().ToString();
        
        OrderLogger.Information("=== ORDER PROCESS: PAYMENT CREATE ====");
        OrderLogger.Information("Creating payment for OrderId: {OrderId}, Amount: {Amount}, Method: {Method}", orderId, amount, method);
        OrderLogger.Information("Generated PaymentRequestId: {PaymentRequestId}", paymentRequestId);
        
        var payment = new OrderPaymentDto
        {
            OrderId = orderId,
            Amount = amount,
            PaymentMethod = method,
            PaymentStatus = "pending",
            PaymentRequestId = paymentRequestId,
            Currency = "VND",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (method.ToLower() == "momo")
        {
            try
            {
                // Gọi Momo
                OrderLogger.Information("=== ORDER PROCESS: PAYMENT MOMO CALL ====");
                OrderLogger.Information("Calling MoMo API with OrderInfo (PaymentRequestId): {OrderInfo}", payment.PaymentRequestId);
                
                var momoResponse = await _momoService.CreatePaymentAsync(new MomoOnetimePaymentRequest
                {
                    OrderInfo = payment.PaymentRequestId.ToString(),
                    Amount = (long)amount,
                    RedirectUrl = _configuration["MomoAPI:ReturnUrl"]!,
                    IpnUrl = _configuration["MomoAPI:IpnUrl"]!,
                    ExtraData = null
                });
                
                _logger.LogInformation("MoMo API Response - ResultCode: {ResultCode}, PayUrl: {PayUrl}", momoResponse.ResultCode, momoResponse.PayUrl);

                if (momoResponse.ResultCode != 0)
                {
                    _logger.LogWarning("Momo payment failed with ResultCode {ResultCode} for OrderId {OrderId}",
                        momoResponse.ResultCode, orderId);
                    payment.PaymentStatus = "failed";
                }
                else
                {
                    payment.CheckoutUrl = momoResponse.PayUrl;
                    payment.QrCodeUrl = momoResponse.QrCodeUrl;
                    payment.PaymentStatus = "pending";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception when calling Momo API for OrderId {OrderId}", orderId);
                payment.PaymentStatus = "failed";
            }
        }

        var paymentEntity = new OrderPayment
        {
            OrderId = payment.OrderId,
            Amount = payment.Amount,
            PaymentMethod = payment.PaymentMethod,
            PaymentStatus = payment.PaymentStatus,
            PaymentRequestId = payment.PaymentRequestId,
            Currency = payment.Currency,
            TransactionId = payment.TransactionId,
            PaidAt = payment.PaidAt,
            FailedAt = payment.FailedAt,
            CreatedAt = payment.CreatedAt,
            UpdatedAt = payment.UpdatedAt
        };

        await _paymentRepo.AddAsync(paymentEntity);
        await _paymentRepo.SaveChangesAsync();

        _logger.LogInformation("Created payment request {PaymentRequestId} for order {OrderId}", payment.PaymentRequestId, orderId);

        return payment;
    }

    public async Task HandlePaymentResultAsync(string paymentRequestId, bool isSuccess, string? transactionId = null)
    {
        OrderLogger.Information("=== ORDER PROCESS: PAYMENT RESULT HANDLING ====");
        OrderLogger.Information("HandlePaymentResult - PaymentRequestId: {PaymentRequestId}, Success: {IsSuccess}, TransactionId: {TransactionId}", 
            paymentRequestId, isSuccess, transactionId);
            
        var payment = await _paymentRepo.GetByRequestIdAsync(paymentRequestId);
        
        if (payment == null)
        {
            OrderLogger.Error("=== ORDER PROCESS: PAYMENT NOT FOUND ====");
            _logger.LogError("Payment not found for PaymentRequestId: {PaymentRequestId}", paymentRequestId);
            throw new InvalidOperationException($"Payment request {paymentRequestId} not found");
        }
        
        OrderLogger.Information("Found payment record - OrderId: {OrderId}, Amount: {Amount}, Status: {Status}", 
            payment.OrderId, payment.Amount, payment.PaymentStatus);

        if (isSuccess)
        {
            payment.PaymentStatus = "paid";
            payment.TransactionId = transactionId;
            payment.PaidAt = DateTime.UtcNow;

            // Xác nhận đặt giữ và cập nhật trạng thái đơn hàng
            var order = await _orderRepo.GetByIdAsync(payment.OrderId); // Giả sử OrderRepo được inject
            if (order != null)
            {
                OrderLogger.Information("=== ORDER PROCESS: PAYMENT SUCCESS PROCESSING ====");
                OrderLogger.Information("Processing successful payment for OrderId: {OrderId}, OrderNumber: {OrderNumber}", 
                    order.Id, order.OrderNumber);
                OrderLogger.Information("About to commit inventory reservations for OrderId: {OrderId}", order.Id);
                
                await _inventoryService.CommitReservationsAsync(order.Id.ToString());
                
                OrderLogger.Information("Inventory commit completed for OrderId: {OrderId}", order.Id);
                order.OrderStatus = OrderStatus.Confirmed;
                await _orderRepo.UpdateAsync(order);
                await _orderRepo.SaveChangesAsync();

                // Gửi thông báo thanh toán thành công cho admin
                await _notificationService.NotifyPaymentSuccessAsync(order);
                
                // Gửi thông báo thanh toán thành công cho customer
                if (order.UserId.HasValue)
                {
                    await _notificationService.NotifyCustomerPaymentAsync(order.UserId.Value, order, true, "MoMo");
                }
            }
        }
        else
        {
            payment.PaymentStatus = "failed";
            payment.FailedAt = DateTime.UtcNow;

            // Giải phóng đặt giữ và hủy đơn hàng
            var order = await _orderRepo.GetByIdAsync(payment.OrderId);
            if (order != null)
            {
                await _inventoryService.ReleaseReservationsAsync(order.Id.ToString());
                var oldStatus = order.OrderStatus.ToString();
                order.OrderStatus = OrderStatus.Cancelled;
                await _orderRepo.UpdateAsync(order);
                await _orderRepo.SaveChangesAsync();
                
                // Thông báo thanh toán thất bại cho admin
                await _notificationService.NotifyOrderUpdateAsync(order, oldStatus, "Cancelled");
                
                // Gửi thông báo thanh toán thất bại cho customer
                if (order.UserId.HasValue)
                {
                    await _notificationService.NotifyCustomerPaymentAsync(order.UserId.Value, order, false, "MoMo");
                }
            }
        }

        payment.UpdatedAt = DateTime.UtcNow;

        await _paymentRepo.UpdateAsync(payment);
        await _paymentRepo.SaveChangesAsync();

        OrderLogger.Information("Payment {PaymentRequestId} updated to {Status}", payment.PaymentRequestId, payment.PaymentStatus);
    
    }

    public async Task<OrderPayment> GetPaymentStatusAsync(string paymentRequestId)
    {
        var payment = await _paymentRepo.GetByRequestIdAsync(paymentRequestId);
        if (payment == null)
        {
            throw new Exception("Payment not found");
        }
        return payment;
    }

}