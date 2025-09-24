using System.Diagnostics.Contracts;
using Microsoft.SqlServer.Server;

namespace TheLightStore.Models.Momo;

public class MomoConfig
{
    public string ConfigName => "Momo";
    public string PartnerCode { get; set; }
    public string ReturnUrl { get; set; }
    public string IpnUrl { get; set; }
    public string AccessKey { get; set; }
    public string SecretKey { get; set; }
    public string PaymentUrl { get; set; }
    public string RequestType { get; set; }
    public string OrderId { get; set; }
    public string OrderType { get; set; } = null!;
    public Guid RequestId { get; set; }
    public long Amount { get; set; }
    public string OrderInfo { get; set; }
    public long ResponseTime { get; set; }
    public long TransId { get; set; }
    public int ResultCode { get; set; }
    public string Message { get; set; }
    public string PayType { get; set; }
    public string ExtraData { get; set; }
    public string Signature { get; set; }
}

public class MomoCreatePaymentResponseModel
{
	[JsonPropertyName("partnerCode")]
    public string PartnerCode { get; set; }

    [JsonPropertyName("orderId")]
    public string OrderId { get; set; }

    [JsonPropertyName("requestId")]
    public string RequestId { get; set; }

    [JsonPropertyName("amount")]
    public long Amount { get; set; }

    [JsonPropertyName("responseTime")]
    public long ResponseTime { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("resultCode")]
    public int ResultCode { get; set; }

    [JsonPropertyName("payUrl")]
    public string PayUrl { get; set; }

    [JsonPropertyName("deeplink")]
    public string Deeplink { get; set; }

    [JsonPropertyName("qrCodeUrl")]
    public string QrCodeUrl { get; set; }

    [JsonPropertyName("deeplinkMiniApp")]
    public string DeeplinkMiniApp { get; set; }
}


public class MomoExecuteResponseModel
{
    public string OrderId { get; set; }
    public decimal Amount { get; set; }
    public string FullName { get; set; }
    public string OrderInfo { get; set; }
}

public class MomoOnetimePaymentRequest
{
    public string OrderInfo { get; set; } = null!;
    public long Amount { get; set; }
    public string RedirectUrl { get; set; } = null!;
    public string IpnUrl { get; set; } = null!;
    public string? ExtraData { get; set; }
}

    // public (bool, string) GetLink(string PaymentUrl)
    // {
    //     using HttpClient client = new HttpClient();
    //     var requestData = JsonSerializer.Serialize(this, new JsonSerializerOptions
    //     {
    //         PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    //         WriteIndented = true
    //     });
    //     var requestContent = new StringContent(requestData, Encoding.UTF8, "application/json");

    //     var createPaymentLinkRes = client.PostAsync(PaymentUrl, requestContent).Result;

    //     if (createPaymentLinkRes.IsSuccessStatusCode)
    //     {
    //         var responseContent = createPaymentLinkRes.Content.ReadAsStringAsync().Result;
    //         var responseData = JsonSerializer.Deserialize<MomoOnetimePaymentResponse>(responseContent);

    //         if (responseData.resultCode == 0)
    //         {
    //             return (true, responseData.payUrl);
    //         }
    //         else
    //         {
    //             return (false, responseData.message);
    //         }
    //     }
    //     else
    //     {
    //         return (false, createPaymentLinkRes.ReasonPhrase);
    //     }

    // }
// }

public static class Helper
{
    internal static string ComputeHmacSha256(string rawData, string secretKey)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secretKey);
        var messageBytes = Encoding.UTF8.GetBytes(rawData);

        byte[] hashBytes;

        using (var hmac = new HMACSHA256(keyBytes))
        {
            hashBytes = hmac.ComputeHash(messageBytes);
        }

        var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

        return hashString;
    }
}
