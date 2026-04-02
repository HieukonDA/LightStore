namespace TheLightStore.Application.Interfaces.Infrastructures;

public interface ICurrentUserService
{
    string? GetCurrentUserId();
    string? GetUserEmail();
    string? GetUsername();
}
