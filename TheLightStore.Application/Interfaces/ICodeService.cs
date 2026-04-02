namespace TheLightStore.Application.Interfaces;

public interface ICodeService
{
    Task<string> GenerateCustomerCodeAsync();
    string GenerateOtpCode();
}
