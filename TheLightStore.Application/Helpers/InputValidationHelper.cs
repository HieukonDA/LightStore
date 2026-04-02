using System;
using System.Text.RegularExpressions;
using TheLightStore.Domain.Constants;

namespace TheLightStore.Application.Helpers;

public class InputValidationHelper
{
    public static bool IsEmailFormat(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return false;
        return Regex.IsMatch(input!.Trim(), ValidationPatterns.EmailPattern, RegexOptions.IgnoreCase);
    }

    public static bool IsPhoneFormat(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return false;
        return Regex.IsMatch(input!.Trim(), ValidationPatterns.PhonePattern);
    }

    /// <summary>Normalize phone: convert +84xxx... -> 0xxx... and trim spaces</summary>
    public static string NormalizePhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return string.Empty;
        var p = phone!.Trim();
        if (p.StartsWith("+84")) p = "0" + p.Substring(3);
        return p;
    }

    /// <summary>Normalize email: trim + lowercase</summary>
    public static string NormalizeEmail(string? email)
    {
        return string.IsNullOrWhiteSpace(email) ? string.Empty : email!.Trim().ToLowerInvariant();
    }
}
