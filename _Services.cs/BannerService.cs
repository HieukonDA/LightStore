using TheLightStore.Dtos;
using TheLightStore.Dtos.Banners;
using TheLightStore.Dtos.Paging;
using TheLightStore.Interfaces.Banners;
using TheLightStore.Interfaces.Repository;
using TheLightStore.Models.System;

namespace TheLightStore.Services.cs;

public class BannerService : IBannerService
{
    private readonly IBannerRepository _bannerRepository;

    public BannerService(IBannerRepository bannerRepository)
    {
        _bannerRepository = bannerRepository;
    }

    public async Task<ApiResponse<PagedResult<BannerListDto>>> GetBannersAsync(int pageNumber = 1, int pageSize = 10, string? position = null, bool? isActive = null)
    {
        try
        {
            var banners = await _bannerRepository.GetBannersAsync(pageNumber, pageSize, position, isActive);
            var totalCount = await _bannerRepository.GetBannersCountAsync(position, isActive);

            var bannerDtos = banners.Select(b => new BannerListDto
            {
                Id = b.Id,
                Title = b.Title,
                ImageUrl = b.ImageUrl,
                Position = b.Position,
                SortOrder = b.SortOrder,
                IsActive = b.IsActive,
                StartDate = b.StartDate,
                EndDate = b.EndDate,
                CreatedByName = b.CreatedByNavigation != null 
                    ? $"{b.CreatedByNavigation.FirstName} {b.CreatedByNavigation.LastName}".Trim() 
                    : "System",
                CreatedAt = b.CreatedAt
            }).ToList();

            var pagedResult = new PagedResult<BannerListDto>
            {
                Items = bannerDtos,
                TotalCount = totalCount,
                Page = pageNumber,
                PageSize = pageSize
            };

            return new ApiResponse<PagedResult<BannerListDto>>
            {
                Success = true,
                Data = pagedResult,
                Message = "Banners retrieved successfully"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<PagedResult<BannerListDto>>
            {
                Success = false,
                Message = $"Error retrieving banners: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<BannerDto>> GetBannerByIdAsync(int id)
    {
        try
        {
            var banner = await _bannerRepository.GetBannerByIdAsync(id);
            if (banner == null)
            {
                return new ApiResponse<BannerDto>
                {
                    Success = false,
                    Message = "Banner not found"
                };
            }

            var bannerDto = new BannerDto
            {
                Id = banner.Id,
                Title = banner.Title,
                Description = banner.Description,
                ImageUrl = banner.ImageUrl,
                LinkUrl = banner.LinkUrl,
                Position = banner.Position,
                SortOrder = banner.SortOrder,
                IsActive = banner.IsActive,
                StartDate = banner.StartDate,
                EndDate = banner.EndDate,
                CreatedBy = banner.CreatedBy,
                CreatedByName = banner.CreatedByNavigation != null 
                    ? $"{banner.CreatedByNavigation.FirstName} {banner.CreatedByNavigation.LastName}".Trim() 
                    : "System",
                CreatedAt = banner.CreatedAt,
                UpdatedAt = banner.UpdatedAt
            };

            return new ApiResponse<BannerDto>
            {
                Success = true,
                Data = bannerDto,
                Message = "Banner retrieved successfully"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<BannerDto>
            {
                Success = false,
                Message = $"Error retrieving banner: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<BannerDto>> CreateBannerAsync(CreateBannerDto createDto, int createdBy)
    {
        try
        {
            var banner = new Banner
            {
                Title = createDto.Title,
                Description = createDto.Description,
                ImageUrl = createDto.ImageUrl,
                LinkUrl = createDto.LinkUrl,
                Position = createDto.Position,
                SortOrder = createDto.SortOrder,
                IsActive = createDto.IsActive,
                StartDate = createDto.StartDate,
                EndDate = createDto.EndDate,
                CreatedBy = createdBy
            };

            var createdBanner = await _bannerRepository.CreateBannerAsync(banner);
            
            return await GetBannerByIdAsync(createdBanner.Id);
        }
        catch (Exception ex)
        {
            return new ApiResponse<BannerDto>
            {
                Success = false,
                Message = $"Error creating banner: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<BannerDto>> UpdateBannerAsync(int id, UpdateBannerDto updateDto)
    {
        try
        {
            var existingBanner = await _bannerRepository.GetBannerByIdAsync(id);
            if (existingBanner == null)
            {
                return new ApiResponse<BannerDto>
                {
                    Success = false,
                    Message = "Banner not found"
                };
            }

            existingBanner.Title = updateDto.Title;
            existingBanner.Description = updateDto.Description;
            existingBanner.ImageUrl = updateDto.ImageUrl;
            existingBanner.LinkUrl = updateDto.LinkUrl;
            existingBanner.Position = updateDto.Position;
            existingBanner.SortOrder = updateDto.SortOrder;
            existingBanner.IsActive = updateDto.IsActive;
            existingBanner.StartDate = updateDto.StartDate;
            existingBanner.EndDate = updateDto.EndDate;

            var updatedBanner = await _bannerRepository.UpdateBannerAsync(existingBanner);
            
            return await GetBannerByIdAsync(updatedBanner.Id);
        }
        catch (Exception ex)
        {
            return new ApiResponse<BannerDto>
            {
                Success = false,
                Message = $"Error updating banner: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<bool>> DeleteBannerAsync(int id)
    {
        try
        {
            var deleted = await _bannerRepository.DeleteBannerAsync(id);
            if (!deleted)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Banner not found"
                };
            }

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Banner deleted successfully"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = $"Error deleting banner: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<bool>> ToggleBannerStatusAsync(int id)
    {
        try
        {
            var banner = await _bannerRepository.GetBannerByIdAsync(id);
            if (banner == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Banner not found"
                };
            }

            banner.IsActive = !banner.IsActive;
            await _bannerRepository.UpdateBannerAsync(banner);

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = $"Banner {(banner.IsActive ? "activated" : "deactivated")} successfully"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = $"Error toggling banner status: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<List<PublicBannerDto>>> GetActiveBannersAsync(string? position = null)
    {
        try
        {
            var banners = await _bannerRepository.GetActiveBannersAsync(position);

            var bannerDtos = banners.Select(b => new PublicBannerDto
            {
                Id = b.Id,
                Title = b.Title,
                Description = b.Description,
                ImageUrl = b.ImageUrl,
                LinkUrl = b.LinkUrl,
                Position = b.Position,
                SortOrder = b.SortOrder
            }).ToList();

            return new ApiResponse<List<PublicBannerDto>>
            {
                Success = true,
                Data = bannerDtos,
                Message = "Active banners retrieved successfully"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<PublicBannerDto>>
            {
                Success = false,
                Message = $"Error retrieving active banners: {ex.Message}"
            };
        }
    }


}