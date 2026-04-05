using System;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Domain.Entities.Products;
using static TheLightStore.Application.Dtos.CategoryDto;

namespace TheLightStore.Application.Interfaces.Repositories;

public interface ICategoryRepository
{
    Task<PaginationModel<CategoryGetDto>> GetAllAsync(CategoryFilterParams filterParams);
    Task<CategoryGetDto?> GetByIdAsync(long id);
    Task<Category?> GetByCodeAsync(string code);
    Task<bool> ExistsAsync(long id);
    Task<long> CreateAsync(Category category);
    Task<bool> UpdateAsync(Category category);
    Task<bool> DeleteAsync(long id);
    Task<List<CategoryGetDto>> GetAllActiveAsync();
}
