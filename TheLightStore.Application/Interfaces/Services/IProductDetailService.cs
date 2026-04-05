using TheLightStore.Application.Dtos;
using TheLightStore.Domain.Commons.Models;

namespace TheLightStore.Application.Interfaces.Services;

public interface IProductDetailService
{
    Task<ResponseResult> GetAll(ProductDetailDto.ProductDetailFilterParams filterParams);
    Task<ResponseResult> GetById(long id);
    Task<ResponseResult> CreateMultiple(ProductDetailDto.ListProductDetailCreateDto createDto);
    Task<ResponseResult> Update(ProductDetailDto.ProductDetailUpdateDto updateDto);
    Task<ResponseResult> Remove(long id);
    Task<ResponseResult> RemoveMultiple(ProductDetailDto.ListProductDetailRemoveDto removeDto);
    Task<ResponseResult> RemainingProductQuantity(long productId);
}
