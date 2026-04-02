namespace TheLightStore.Application.Interfaces;

public interface IConfigurationService
{
    string? GetJwtKey();
    string? GetJwtRefreshKey();
    string? GetJwtIssuer();
    string? GetJwtAudience();
    string? GetConfirmEmailUrl();
    int GetOtpExpiryMinutes();
}
