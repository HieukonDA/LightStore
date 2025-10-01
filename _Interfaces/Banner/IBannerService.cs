using TheLightStore.Dtos;
using TheLightStore.Dtos.Banners;
using TheLightStore.Dtos.Paging;

namespace TheLightStore.Interfaces.Banners;

public interface IBannerService
{
    Task<ApiResponse<PagedResult<BannerListDto>>> GetBannersAsync(int pageNumber = 1, int pageSize = 10, string? position = null, bool? isActive = null);
    Task<ApiResponse<BannerDto>> GetBannerByIdAsync(int id);
    Task<ApiResponse<BannerDto>> CreateBannerAsync(CreateBannerDto createDto, int createdBy);
    Task<ApiResponse<BannerDto>> UpdateBannerAsync(int id, UpdateBannerDto updateDto);
    Task<ApiResponse<bool>> DeleteBannerAsync(int id);
    Task<ApiResponse<bool>> ToggleBannerStatusAsync(int id);
    
    // Public methods for frontend
    Task<ApiResponse<List<PublicBannerDto>>> GetActiveBannersAsync(string? position = null);
}