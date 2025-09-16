using TheLightStore.Interfaces.Payment;

namespace TheLightStore.Services.Payment;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepo _paymentRepo;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(IPaymentRepo paymentRepo, ILogger<PaymentService> logger)
    {
        _paymentRepo = paymentRepo;
        _logger = logger;
    }

    public async Task<OrderPayment> CreatePaymentAsync(int orderId, decimal amount, string method)
    {
        var payment = new OrderPayment
        {
            OrderId = orderId,
            Amount = amount,
            PaymentMethod = method,
            PaymentStatus = "pending",
            PaymentRequestId = Guid.NewGuid(),
            Currency = "VND",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _paymentRepo.AddAsync(payment);
        await _paymentRepo.SaveChangesAsync();

        _logger.LogInformation("Created payment request {PaymentRequestId} for order {OrderId}", payment.PaymentRequestId, orderId);

        return payment;
    }

    public async Task HandlePaymentResultAsync(Guid paymentRequestId, bool isSuccess, string? transactionId = null)
    {
        var payment = await _paymentRepo.GetByRequestIdAsync(paymentRequestId)
                      ?? throw new InvalidOperationException($"Payment request {paymentRequestId} not found");

        if (isSuccess)
        {
            payment.PaymentStatus = "paid";
            payment.TransactionId = transactionId;
            payment.PaidAt = DateTime.UtcNow;
        }
        else
        {
            payment.PaymentStatus = "failed";
            payment.FailedAt = DateTime.UtcNow;
        }

        payment.UpdatedAt = DateTime.UtcNow;

        await _paymentRepo.UpdateAsync(payment);
        await _paymentRepo.SaveChangesAsync();

        _logger.LogInformation("Payment {PaymentRequestId} updated to {Status}", payment.PaymentRequestId, payment.PaymentStatus);
    
    }

    public async Task<OrderPayment> GetPaymentStatusAsync(Guid paymentRequestId)
    {
        var payment = await _paymentRepo.GetByRequestIdAsync(paymentRequestId);
        if (payment == null)
        {
            throw new Exception("Payment not found");
        }
        return payment;
    }
}