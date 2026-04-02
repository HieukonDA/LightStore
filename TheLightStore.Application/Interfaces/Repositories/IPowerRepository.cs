using System;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Domain.Entities.Products;
using static TheLightStore.Application.Dtos.PowerDto;

namespace TheLightStore.Application.Interfaces.Repositories;

public interface IPowerRepository
{
    Task<PaginationModel<PowerGetDto>> GetAllAsync(PowerFilterParams parameters);
    Task<PowerGetDto?> GetByIdAsync(long id);
    Task<Power?> GetByCodeAsync(string code);
    Task<bool> ExistsAsync(long id);
    Task<long> CreateAsync(Power entity);
    Task<bool> UpdateAsync(Power entity);
    Task<bool> DeleteAsync(long id);
    Task<List<PowerGetDto>> GetAllActiveAsync();
}
