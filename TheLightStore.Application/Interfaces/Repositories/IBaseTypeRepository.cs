using System;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Domain.Entities.Products;
using static TheLightStore.Application.Dtos.BaseTypeDto;

namespace TheLightStore.Application.Interfaces.Repositories;

public interface IBaseTypeRepository
{
    Task<PaginationModel<BaseTypeGetDto>> GetAllAsync(BaseTypeFilterParams parameters);
    Task<BaseTypeGetDto?> GetByIdAsync(long id);
    Task<BaseType?> GetByCodeAsync(string code);
    Task<bool> ExistsAsync(long id);
    Task<long> CreateAsync(BaseType entity);
    Task<bool> UpdateAsync(BaseType entity);
    Task<bool> DeleteAsync(long id);
    Task<List<BaseTypeGetDto>> GetAllActiveAsync();
}
