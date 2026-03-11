using System.Diagnostics.Contracts;
using Microsoft.SqlServer.Server;
using System.Text.Json.Serialization;
using System.Text;
using System.Security.Cryptography;

namespace TheLightStore.Application.DTOs.Momo;

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

// 💰 MoMo Refund Models
public class MomoRefundRequest
{
    public string partnerCode { get; set; } = null!;
    public string orderId { get; set; } = null!;
    public string requestId { get; set; } = null!;
    public long amount { get; set; }
    public long transId { get; set; }
    public string lang { get; set; } = "vi";
    public string? description { get; set; }
    public string signature { get; set; } = null!;
}

public class MomoRefundResponse
{
    public string partnerCode { get; set; } = null!;
    public string orderId { get; set; } = null!;
    public string requestId { get; set; } = null!;
    public long amount { get; set; }
    public long transId { get; set; }
    public int resultCode { get; set; }
    public string message { get; set; } = null!;
    public long responseTime { get; set; }
}

public class MomoIPNRequest
{
    public string OrderType { get; set; }
    public long Amount { get; set; }
    public string PartnerCode { get; set; }
    public string OrderId { get; set; }
    public string ExtraData { get; set; }
    public string Signature { get; set; }
    public long TransId { get; set; }
    public long ResponseTime { get; set; }
    public int ResultCode { get; set; }
    public string Message { get; set; }
    public string PayType { get; set; }
    public string RequestId { get; set; }
    public string OrderInfo { get; set; }
}

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
