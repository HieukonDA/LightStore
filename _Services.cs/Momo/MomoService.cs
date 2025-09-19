using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace TheLightStore.Services.Momo;

public class MomoService : IMomoService
{
    private readonly IOptions<MomoConfig> _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<MomoService> _logger;

    public MomoService(IOptions<MomoConfig> options, IHttpClientFactory httpClientFactory, ILogger<MomoService> logger)
    {
        _options = options;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }
    public async Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(MomoOnetimePaymentRequest request)
    {
        var partnerCode = _options.Value.PartnerCode;
        var accessKey = _options.Value.AccessKey;
        var secretKey = _options.Value.SecretKey;

        var requestId = Guid.NewGuid().ToString();
        var orderId = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();

        // rawData đúng chuẩn MoMo
        var rawData =
            $"accessKey={accessKey}" +
            $"&amount={request.Amount}" +
            $"&extraData={request.ExtraData ?? ""}" +
            $"&ipnUrl={request.IpnUrl}" +
            $"&orderId={orderId}" +
            $"&orderInfo={request.OrderInfo}" +
            $"&partnerCode={partnerCode}" +
            $"&redirectUrl={request.RedirectUrl}" +
            $"&requestId={requestId}" +
            $"&requestType=captureWallet";

        var signature = ComputeHmacSha256(rawData, secretKey);

        var finalRequest = new
        {
            partnerCode,
            accessKey,
            requestId,
            orderId,
            orderInfo = request.OrderInfo,
            amount = request.Amount.ToString(),
            redirectUrl = request.RedirectUrl,
            ipnUrl = request.IpnUrl,
            extraData = request.ExtraData ?? "",
            requestType = "captureWallet",
            signature,
            lang = "vi"
        };

        using var client = _httpClientFactory.CreateClient();
        var content = new StringContent(
            JsonSerializer.Serialize(finalRequest),
            Encoding.UTF8,
            "application/json");

        var url = _options.Value.PaymentUrl;
        var response = await client.PostAsync(url, content);

        var body = await response.Content.ReadAsStringAsync();
        _logger.LogInformation("MoMo CreatePayment Response: {Body}", body);

        response.EnsureSuccessStatusCode();
        _logger.LogInformation("Raw response from MoMo: {Body}", body);


        var result = JsonSerializer.Deserialize<MomoCreatePaymentResponseModel>(body);
        if (result == null)
            throw new Exception("Failed to deserialize Momo payment response.");

        return result;
    }

    
    public MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection)
    {
        var amount = collection["amount"].ToString();
        var orderInfo = collection["orderInfo"].ToString();
        var orderId = collection["orderId"].ToString();
        var errorCode = collection["errorCode"].ToString();
        var partnerCode = collection["partnerCode"].ToString();
        var accessKey = collection["accessKey"].ToString();
        var transId = collection["transId"].ToString();
        var responseTime = collection["responseTime"].ToString();
        var message = collection["message"].ToString();
        var localMessage = collection["localMessage"].ToString();
        var signature = collection["signature"].ToString();
        var requestId = collection["requestId"].ToString();
        var orderType = collection["orderType"].ToString();
        var payType = collection["payType"].ToString();
        var extraData = collection["extraData"].ToString();

        _logger.LogInformation("Momo callback received. OrderId: {OrderId}, Amount: {Amount}, ErrorCode: {ErrorCode}, TransId: {TransId}",
            orderId, amount, errorCode, transId);

        // build lại rawData để verify chữ ký
        var rawData =
            $"partnerCode={partnerCode}" +
            $"&accessKey={accessKey}" +
            $"&requestId={requestId}" +
            $"&amount={amount}" +
            $"&orderId={orderId}" +
            $"&orderInfo={orderInfo}" +
            $"&orderType={orderType}" +
            $"&transId={transId}" +
            $"&message={message}" +
            $"&localMessage={localMessage}" +
            $"&responseTime={responseTime}" +
            $"&errorCode={errorCode}" +
            $"&payType={payType}" +
            $"&extraData={extraData}";

        var computedSignature = ComputeHmacSha256(rawData, _options.Value.SecretKey);
        _logger.LogInformation("Momo callback signature verification. ComputedSignature: {ComputedSignature}, ReceivedSignature: {Signature}",
            computedSignature, signature);

        if (computedSignature != signature)
        {
            throw new Exception("Invalid signature from MoMo");
        }

        return new MomoExecuteResponseModel()
        {
            Amount = decimal.Parse(amount),
            OrderId = orderId,
            OrderInfo = orderInfo,
            FullName = "" // MoMo không trả về tên KH, cần map từ Order của bạn
        };
    }


    public bool ValidateSignature(MomoConfig request)
    {
        var secretKey = _options.Value.SecretKey;
        if (string.IsNullOrEmpty(secretKey))
            throw new InvalidOperationException("Momo SecretKey not configured");

        // Chuẩn rawData string theo thứ tự Momo yêu cầu
        var rawData =
            $"partnerCode={request.PartnerCode}" +
            $"&orderId={request.OrderId}" +
            $"&requestId={request.RequestId}" +
            $"&amount={request.Amount}" +
            $"&orderInfo={request.OrderInfo}" +
            $"&orderType={request.OrderType}" +
            $"&transId={request.TransId}" +
            $"&resultCode={request.ErrorCode}" +
            $"&message={request.Message}" +
            $"&payType={request.PayType}" +
            $"&responseTime={request.ResponseTime}" +
            $"&extraData={request.ExtraData}";

        // Tính toán chữ ký HMACSHA256
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawData));
        var computedSignature = BitConverter.ToString(hash).Replace("-", "").ToLower();

        return computedSignature == request.Signature;
    }


    private string ComputeHmacSha256(string message, string secretKey)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secretKey);
        var messageBytes = Encoding.UTF8.GetBytes(message);

        byte[] hashBytes;

        using (var hmac = new HMACSHA256(keyBytes))
        {
            hashBytes = hmac.ComputeHash(messageBytes);
        }

        var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

        return hashString;
    }

}