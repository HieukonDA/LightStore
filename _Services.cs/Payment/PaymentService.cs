using TheLightStore.Dtos.Orders;
using TheLightStore.Interfaces.Notifications;
using TheLightStore.Interfaces.Orders;
using TheLightStore.Interfaces.Payment;

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
        var payment = new OrderPaymentDto
        {
            OrderId = orderId,
            Amount = amount,
            PaymentMethod = method,
            PaymentStatus = "pending",
            PaymentRequestId = Guid.NewGuid().ToString(),
            Currency = "VND",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (method.ToLower() == "momo")
        {
            try
            {
                // Gọi Momo
                var momoResponse = await _momoService.CreatePaymentAsync(new MomoOnetimePaymentRequest
                {
                    OrderInfo = payment.PaymentRequestId.ToString(),
                    Amount = (long)amount,
                    RedirectUrl = _configuration["MomoAPI:ReturnUrl"]!,
                    IpnUrl = _configuration["MomoAPI:IpnUrl"]!,
                    ExtraData = null
                });

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
        var payment = await _paymentRepo.GetByRequestIdAsync(paymentRequestId)
                      ?? throw new InvalidOperationException($"Payment request {paymentRequestId} not found");

        if (isSuccess)
        {
            payment.PaymentStatus = "paid";
            payment.TransactionId = transactionId;
            payment.PaidAt = DateTime.UtcNow;

            // Xác nhận đặt giữ và cập nhật trạng thái đơn hàng
            var order = await _orderRepo.GetByIdAsync(payment.OrderId); // Giả sử OrderRepo được inject
            if (order != null)
            {
                await _inventoryService.CommitReservationsAsync(order.Id.ToString());
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

        _logger.LogInformation("Payment {PaymentRequestId} updated to {Status}", payment.PaymentRequestId, payment.PaymentStatus);
    
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