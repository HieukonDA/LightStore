namespace TheLightStore.Application.Interfaces.Infrastructures;

public interface ICodeService
{
    Task<string> GenerateCustomerCodeAsync();
    string GenerateOtpCode();
}
