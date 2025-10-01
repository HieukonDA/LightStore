using TheLightStore.Models.System;

namespace TheLightStore.Interfaces.Repository;

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