using TheLightStore.Application.Dtos;
using TheLightStore.Domain.Commons.Models;

namespace TheLightStore.Application.Interfaces.Services;

public interface IProductService
{
    Task<ResponseResult> GetAll(ProductDto.ProductFilterParams filterParams);
    Task<ResponseResult> GetById(long id);
    Task<ResponseResult> CreateByAdmin(ProductDto.ProductCreateDto createDto);
    Task<ResponseResult> UpdateByAdmin(ProductDto.UpdateExtraDto updateDto);
    Task<ResponseResult> UpdateByUserLogin(ProductDto.ProductUpdateDto updateDto);
    Task<ResponseResult> RemoveByAdmin(long id);
}
