using System;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Domain.Entities.Products;
using static TheLightStore.Application.Dtos.ColorTemperatureDto;

namespace TheLightStore.Application.Interfaces.Repositories;

public interface IColorTemperatureRepository
{
    Task<PaginationModel<ColorTemperatureGetDto>> GetAllAsync(ColorTemperatureFilterParams parameters);
    Task<ColorTemperatureGetDto?> GetByIdAsync(long id);
    Task<ColorTemperature?> GetByCodeAsync(string code);
    Task<bool> ExistsAsync(long id);
    Task<long> CreateAsync(ColorTemperature entity);
    Task<bool> UpdateAsync(ColorTemperature entity);
    Task<bool> DeleteAsync(long id);
    Task<List<ColorTemperatureGetDto>> GetAllActiveAsync();
}
