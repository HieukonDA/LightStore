namespace TheLightStore.Application.Interfaces;

public interface ICurrentUserService
{
    string? GetCurrentUserId();
    string? GetUserEmail();
    string? GetUsername();
}
