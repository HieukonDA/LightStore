using System;
using TheLightStore.Domain.Commons.Models;
using static TheLightStore.Application.Dtos.BaseTypeDto;

namespace TheLightStore.Application.Interfaces.Services;

public interface IBaseTypeService
{
    Task<ResponseResult> GetAll(BaseTypeFilterParams parameters);
    Task<ResponseResult> GetById(long id);
    Task<ResponseResult> Create(BaseTypeCreateDto model);
    Task<ResponseResult> Update(BaseTypeUpdateDto model);
    Task<ResponseResult> Remove(long id);
}
