using TheLightStore.Application.Dtos;
using TheLightStore.Domain.Commons.Models;

namespace TheLightStore.Application.Interfaces.Services;

public interface ICategoryService
{
    Task<ResponseResult> GetAll(CategoryDto.CategoryFilterParams filterParams);
    Task<ResponseResult> GetById(long id);
    Task<ResponseResult> Create(CategoryDto.CategoryCreateDto createDto);
    Task<ResponseResult> Update(CategoryDto.CategoryUpdateDto updateDto);
    Task<ResponseResult> Remove(long id);
}
