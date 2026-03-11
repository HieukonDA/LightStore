using TheLightStore.Application.DTOs.Momo;

namespace TheLightStore.Application.Interfaces;
public interface IMomoService
{
    Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(MomoOnetimePaymentRequest model);
    MomoExecuteResponseModel PaymentExecuteAsync(Dictionary<string, string> queryParams);
    bool ValidateSignature(MomoIPNRequest request);
    
    /// <summary>
    /// Hoàn tiền qua MoMo
    /// </summary>
    /// <param name="orderId">Mã đơn hàng gốc</param>
    /// <param name="transId">ID giao dịch gốc từ MoMo</param>
    /// <param name="amount">Số tiền hoàn</param>
    /// <param name="description">Mô tả lý do hoàn tiền</param>
    /// <returns>Kết quả hoàn tiền</returns>
    Task<MomoRefundResponse> RefundAsync(string orderId, long transId, long amount, string? description = null);
}