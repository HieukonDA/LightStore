using System;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Domain.Entities.Products;
using static TheLightStore.Application.Dtos.ProductDto;

namespace TheLightStore.Application.Interfaces.Repositories;

public interface IProductRepository
{
    Task<PaginationModel<ProductGetDto>> GetAllAsync(ProductFilterParams filterParams);
    Task<ProductGetDto?> GetByIdAsync(long id);
    Task<Product?> GetProductWithDetailsAsync(long id);
    Task<bool> ExistsAsync(long id);
    Task<bool> ExistsByCodeAsync(string code, long excludeId = 0);
    Task<long> CreateAsync(Product product);
    Task<bool> UpdateAsync(Product product);
    Task<bool> DeleteAsync(long id);
    Task<List<ProductGetDto>> GetAllActiveAsync();
    Task<bool> HasOrderDetailsAsync(List<long> productDetailIds);
}
