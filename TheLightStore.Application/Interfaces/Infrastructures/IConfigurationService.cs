namespace TheLightStore.Application.Interfaces.Infrastructures;

public interface IConfigurationService
{
    string? GetJwtKey();
    string? GetJwtRefreshKey();
    string? GetJwtIssuer();
    string? GetJwtAudience();
    string? GetConfirmEmailUrl();
    int GetOtpExpiryMinutes();
}
