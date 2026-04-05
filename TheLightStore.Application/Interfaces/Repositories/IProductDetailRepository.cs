using System;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Domain.Entities.Products;
using static TheLightStore.Application.Dtos.ProductDetailDto;

namespace TheLightStore.Application.Interfaces.Repositories;

public interface IProductDetailRepository
{
    Task<PaginationModel<ProductDetailGetDto>> GetAllAsync(ProductDetailFilterParams filterParams);
    Task<ProductDetailGetDto?> GetByIdAsync(long id);
    Task<bool> ExistsAsync(long id);
    Task<long> CreateAsync(ProductDetail productDetail);
    Task<bool> UpdateAsync(ProductDetail productDetail);
    Task<bool> DeleteAsync(long id);
    Task<List<ProductDetailGetDto>> GetAllActiveAsync();
    Task<List<ProductDetail>> GetProductDetailsByProductIdAsync(long productId);
}
