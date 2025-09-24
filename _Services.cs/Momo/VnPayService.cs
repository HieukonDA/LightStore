using System.Globalization;
using System.Net;

namespace TheLightStore.Services.Momo;

public class VnPay
{
    public const string VERSION = "2.1.0";
    private readonly SortedList<string, string> _requestData = new(new VnPayCompare());
    private readonly SortedList<string, string> _responseData = new(new VnPayCompare());

    public void AddRequestData(string key, string value)
    {
        if (!string.IsNullOrEmpty(value))
            _requestData[key] = value;
    }

    public void AddResponseData(string key, string value)
    {
        if (!string.IsNullOrEmpty(value))
            _responseData[key] = value;
    }

    public string GetResponseData(string key)
    {
        return _responseData.TryGetValue(key, out var retValue) ? retValue : string.Empty;
    }

    #region Request
    public string CreateRequestUrl(string baseUrl, string vnp_HashSecret)
    {
        var data = new StringBuilder();
        foreach (var kv in _requestData)
        {
            data.Append($"{WebUtility.UrlEncode(kv.Key)}={WebUtility.UrlEncode(kv.Value)}&");
        }

        if (data.Length > 0)
            data.Remove(data.Length - 1, 1); // remove last '&'

        var signData = data.ToString();
        string vnp_SecureHash = HashAndGetIP.HmacSHA512(vnp_HashSecret, signData);
        return $"{baseUrl}?{signData}&vnp_SecureHash={vnp_SecureHash}";
    }
    #endregion

    #region Response
    public bool ValidateSignature(string inputHash, string secretKey)
    {
        string rspRaw = GetResponseRawData();
        string myChecksum = HashAndGetIP.HmacSHA512(secretKey, rspRaw);
        return myChecksum.Equals(inputHash, StringComparison.OrdinalIgnoreCase);
    }

    private string GetResponseRawData()
    {
        _responseData.Remove("vnp_SecureHashType");
        _responseData.Remove("vnp_SecureHash");

        var data = new StringBuilder();
        foreach (var kv in _responseData)
        {
            data.Append($"{WebUtility.UrlEncode(kv.Key)}={WebUtility.UrlEncode(kv.Value)}&");
        }

        if (data.Length > 0)
            data.Remove(data.Length - 1, 1);

        return data.ToString();
    }
    #endregion
}

public class VnPayCompare : IComparer<string>
{
    public int Compare(string x, string y)
    {
        if (x == y) return 0;
        if (x == null) return -1;
        if (y == null) return 1;
        return CompareInfo.GetCompareInfo("en-US").Compare(x, y, CompareOptions.Ordinal);
    }
}