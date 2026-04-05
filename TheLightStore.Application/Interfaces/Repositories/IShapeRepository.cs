using System;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Domain.Entities.Products;
using static TheLightStore.Application.Dtos.ShapeDto;

namespace TheLightStore.Application.Interfaces.Repositories;

public interface IShapeRepository
{
    Task<PaginationModel<ShapeGetDto>> GetAllAsync(ShapeFilterParams parameters);
    Task<ShapeGetDto?> GetByIdAsync(long id);
    Task<Shape?> GetByCodeAsync(string code);
    Task<bool> ExistsAsync(long id);
    Task<long> CreateAsync(Shape entity);
    Task<bool> UpdateAsync(Shape entity);
    Task<bool> DeleteAsync(long id);
    Task<List<ShapeGetDto>> GetAllActiveAsync();
}
