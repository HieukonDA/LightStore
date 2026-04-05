using System;
using TheLightStore.Domain.Commons.Models;
using static TheLightStore.Application.Dtos.ShapeDto;

namespace TheLightStore.Application.Interfaces.Services;

public interface IShapeService
{
    Task<ResponseResult> GetAll(ShapeFilterParams parameters);
    Task<ResponseResult> GetById(long id);
    Task<ResponseResult> Create(ShapeCreateDto model);
    Task<ResponseResult> Update(ShapeUpdateDto model);
    Task<ResponseResult> Remove(long id);
}
