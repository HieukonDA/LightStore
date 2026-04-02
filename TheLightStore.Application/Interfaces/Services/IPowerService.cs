using System;
using TheLightStore.Domain.Commons.Models;
using static TheLightStore.Application.Dtos.PowerDto;

namespace TheLightStore.Application.Interfaces.Services;

public interface IPowerService
{
    Task<ResponseResult> GetAll(PowerFilterParams parameters);
    Task<ResponseResult> GetById(long id);
    Task<ResponseResult> Create(PowerCreateDto model);
    Task<ResponseResult> Update(PowerUpdateDto model);
    Task<ResponseResult> Remove(long id);
}
