using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace TheLightStore.Services.Momo;

public class MomoService : IMomoService
{
    private readonly IOptions<MomoOptionModel> _options;
    private readonly IHttpClientFactory _httpClientFactory;

    public MomoService(IOptions<MomoOptionModel> options, IHttpClientFactory httpClientFactory)
    {
        _options = options;
        _httpClientFactory = httpClientFactory;
    }
    public async Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(OrderInfoModel model)
    {
        model.OrderId = DateTime.UtcNow.Ticks.ToString();
        model.OrderInfo = "Khách hàng: " + model.FullName + ". Nội dung: " + model.OrderInfo;
        var rawData =
            $"partnerCode={_options.Value.PartnerCode}" +
            $"&accessKey={_options.Value.AccessKey}" +
            $"&requestId={model.OrderId}" +
            $"&amount={model.Amount}" +
            $"&orderId={model.OrderId}" +
            $"&orderInfo={model.OrderInfo}" +
            $"&returnUrl={_options.Value.ReturnUrl}" +
            $"&notifyUrl={_options.Value.NotifyUrl}" +
            $"&extraData=";

        var signature = ComputeHmacSha256(rawData, _options.Value.SecretKey);

        // Create an object representing the request data
        var requestData = new
        {
            accessKey = _options.Value.AccessKey,
            partnerCode = _options.Value.PartnerCode,
            requestType = _options.Value.RequestType,
            notifyUrl = _options.Value.NotifyUrl,
            returnUrl = _options.Value.ReturnUrl,
            orderId = model.OrderId,
            amount = model.Amount.ToString(),
            orderInfo = model.OrderInfo,
            requestId = model.OrderId,
            extraData = "",
            signature = signature
        };
        
        using var client = _httpClientFactory.CreateClient();

        // Serialize the request data to a JSON string
        var jsonContent = JsonSerializer.Serialize(requestData);

        // Create a StringContent object with the JSON payload and set the content type
        var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        // Send the POST request to the Momo API
        var response = await client.PostAsync(_options.Value.Url, httpContent);

        // Ensure the request was successful
        response.EnsureSuccessStatusCode();

        // Read the response content as a string
        var responseContent = await response.Content.ReadAsStringAsync();

        // Deserialize the JSON response to the model
        var momoResponse = JsonSerializer.Deserialize<MomoCreatePaymentResponseModel>(
            responseContent,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );

        return momoResponse;

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

        if (computedSignature != signature)
        {
            throw new Exception("Invalid signature from MoMo");
        }

        return new MomoExecuteResponseModel()
        {
            Amount = amount,
            OrderId = orderId,
            OrderInfo = orderInfo,
            FullName = "" // MoMo không trả về tên KH, cần map từ Order của bạn
        };
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