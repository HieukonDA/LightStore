using TheLightStore.Application.DTOs;
using TheLightStore.Application.DTOs.Paging;
using TheLightStore.Application.DTOs.Stats;
using TheLightStore.Application.DTOs.Categories;

namespace TheLightStore.Application.Interfaces;
public interface ICategoryService
{
    //crud
    Task<ServiceResult<PagedResult<CategoryDto>>> GetAllCategoriesAsync(PagedRequest pagedRequest);
    Task<ServiceResult<CategoryDto>> GetCategoryByIdAsync(int id);
    Task<ServiceResult<CategoryDto>> CreateCategoryAsync(CategoryDto categoryDto);
    Task<ServiceResult<CategoryDto>> UpdateCategoryAsync(int id, CategoryDto categoryDto);
    Task<ServiceResult<bool>> DeleteCategoryAsync(int id);

    // admin
    Task<ServiceResult<List<CategoryStatsDto>>> GetCategoryStatsAsync();
}