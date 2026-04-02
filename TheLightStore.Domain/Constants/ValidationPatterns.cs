using System;

namespace TheLightStore.Domain.Constants;

public class ValidationPatterns
{
    public const string EmailPattern = @"^[A-Za-z0-9._%+-]+@(?!.*?\.\.)(?!\.)(?!\.\.)[A-Za-z0-9.-]+(?<!\.)\.(?:[A-Za-z]{2,4}(?<!\.)\.)?[A-Za-z]{2,4}$";
    public const string PhonePattern = @"^(?:\+?84|0)(?:3|5|7|8|9)\d{8}$";
}
