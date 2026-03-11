using TheLightStore.Domain.Entities.Shared;

namespace TheLightStore.Application.Interfaces.Repositories;

public interface IBannerRepository
{
    Task<IEnumerable<Banner>> GetBannersAsync(int pageNumber, int pageSize, string? position = null, bool? isActive = null);
    Task<int> GetBannersCountAsync(string? position = null, bool? isActive = null);
    Task<Banner?> GetBannerByIdAsync(int id);
    Task<Banner> CreateBannerAsync(Banner banner);
    Task<Banner> UpdateBannerAsync(Banner banner);
    Task<bool> DeleteBannerAsync(int id);
    Task<IEnumerable<Banner>> GetActiveBannersAsync(string? position = null);
}
