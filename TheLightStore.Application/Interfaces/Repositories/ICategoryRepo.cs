using TheLightStore.Application.DTOs.Paging;
using TheLightStore.Application.DTOs.Categories;
using TheLightStore.Application.DTOs.Stats;

namespace TheLightStore.Application.Interfaces.Repositories;

public interface ICategoryRepo
{
    Task<PagedResult<CategoryDto>> GetAllCategoriesAsync(PagedRequest pagedRequest);
    Task<CategoryDto> GetCategoryByIdAsync(int id);
    Task<CategoryDto> CreateCategoryAsync(CategoryDto categoryDto);
    Task<CategoryDto> UpdateCategoryAsync(int id, CategoryDto categoryDto);
    Task<bool> DeleteCategoryAsync(int id);

    //===== satisfy for admin: Category Pie Chart
    Task<List<CategoryStatsDto>> GetCategoryStatsAsync(); 
}
