using System;
using Microsoft.EntityFrameworkCore;
using TheLightStore.Application.Interfaces;
using TheLightStore.Infrastructure.Persistence;

namespace TheLightStore.Infrastructure.Services;

public class CodeService : ICodeService
{
    private static readonly object _lock = new();
    private readonly Random _random = new();
    private readonly DBContext _context;
    
    public CodeService(DBContext context)
    {
        _context = context;
    }
    
    public async Task<string> GenerateCustomerCodeAsync()
    {
        return await GenerateUniqueCodeAsync("CTM", 5, async (code) =>
                await _context.Customers.AnyAsync(x => x.Code == code)
            );
    }

    public string GenerateOtpCode()
    {
        return GenerateRandomCode(6);
    }

    public string GenerateRandomCode(int length = 6)
    {
        var random = new Random();
        var otpCode = new char[length];
        for (int i = 0; i < length; i++)
        {
            otpCode[i] = (char)random.Next('0', '9' + 1);
        }
        return new string(otpCode);
    }

    public async Task<string> GenerateUniqueCodeAsync(
            string prefix,
            int numberLength,
            Func<string, Task<bool>> existsCheck,
            int maxAttempts = 10)
    {
        for (int i = 0; i < maxAttempts; i++)
        {
            var number = GenerateRandomNumber(numberLength);
            var code = $"{prefix}-{number}";

            if (!await existsCheck(code))
                return code;
        }

        throw new Exception($"Cannot generate unique code with prefix '{prefix}' after {maxAttempts} attempts");
    }

    private string GenerateRandomNumber(int length)
    {
        lock (_lock)
        {
            var min = (int)Math.Pow(10, length - 1);
            var max = (int)Math.Pow(10, length) - 1;
            return _random.Next(min, max).ToString();
        }
    }
}
