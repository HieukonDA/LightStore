using TheLightStore.Controllers.Checkout;

namespace TheLightStore.Interfaces.Momo;

public interface IMomoService
{
    Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(MomoOnetimePaymentRequest model);
    MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection);
    bool ValidateSignature(MomoIPNRequest request);
}