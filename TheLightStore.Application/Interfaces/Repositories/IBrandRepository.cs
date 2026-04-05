using System;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Domain.Entities.Products;
using static TheLightStore.Application.Dtos.BrandDto;

namespace TheLightStore.Application.Interfaces.Repositories;

public interface IBrandRepository
{
    Task<PaginationModel<BrandGetDto>> GetAllAsync(BrandFilterParams filterParams);
    Task<BrandGetDto?> GetByIdAsync(long id);
    Task<Brand?> GetByCodeAsync(string code);
    Task<bool> ExistsAsync(long id);
    Task<long> CreateAsync(Brand brand);
    Task<bool> UpdateAsync(Brand brand);
    Task<bool> DeleteAsync(long id);
    Task<List<BrandGetDto>> GetAllActiveAsync();
}
