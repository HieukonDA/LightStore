using TheLightStore.Application.Dtos;
using TheLightStore.Domain.Commons.Models;

namespace TheLightStore.Application.Interfaces.Services;

public interface IBrandService
{
    Task<ResponseResult> GetAll(BrandDto.BrandFilterParams filterParams);
    Task<ResponseResult> GetById(long id);
    Task<ResponseResult> Create(BrandDto.BrandCreateDto createDto);
    Task<ResponseResult> Update(BrandDto.BrandUpdateDto updateDto);
    Task<ResponseResult> Remove(long id);
}
