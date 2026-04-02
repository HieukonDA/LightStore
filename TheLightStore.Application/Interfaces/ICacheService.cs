namespace TheLightStore.Application.Interfaces;

public interface ICacheService
{
    void Set(string key, string subKey, string value, int expiryMinutes);
    string? Get(string key, string subKey);
    void Remove(string key, string subKey);
}
