using System;
using TheLightStore.Domain.Commons.Models;
using static TheLightStore.Application.Dtos.ColorTemperatureDto;

namespace TheLightStore.Application.Interfaces.Services;

public interface IColorTemperatureService
{
    Task<ResponseResult> GetAll(ColorTemperatureFilterParams parameters);
    Task<ResponseResult> GetById(long id);
    Task<ResponseResult> Create(ColorTemperatureCreateDto model);
    Task<ResponseResult> Update(ColorTemperatureUpdateDto model);
    Task<ResponseResult> Remove(long id);
}
