using TheLightStore.Dtos.Orders;

namespace TheLightStore.Interfaces.Payment;

public interface IPaymentService
{
    /// <summary>
    /// Khởi tạo giao dịch thanh toán mới (Pending).
    /// </summary>
    Task<OrderPaymentDto> CreatePaymentAsync(int orderId, decimal amount, string method);

    /// <summary>
    /// Xử lý callback/response từ Payment Gateway (ví dụ: Momo, VNPay).
    /// </summary>
    Task HandlePaymentResultAsync(string paymentRequestId, bool isSuccess, string? transactionId = null);

    /// <summary>
    /// Kiểm tra trạng thái giao dịch.
    /// </summary>
    Task<OrderPayment> GetPaymentStatusAsync(string paymentRequestId);
}