namespace TheLightStore.Interfaces.Repository;

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
