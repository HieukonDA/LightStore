using System;
using TheLightStore.Application.Dtos;
using TheLightStore.Application.Interfaces.Repositories;
using TheLightStore.Application.Interfaces.Services;
using TheLightStore.Application.Mappings;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Domain.Entities.Products;
using static TheLightStore.Application.Dtos.PowerDto;

namespace TheLightStore.Application.Services;

public class PowerService : IPowerService
{
    private readonly IPowerRepository _repository;

    public PowerService(IPowerRepository repository)
    {
        _repository = repository;
    }

    public async Task<ResponseResult> GetAll(PowerFilterParams parameters)
    {
        try
        {
            var result = await _repository.GetAllAsync(parameters);
            return new SuccessResponseResult(result, "Get power list successfully");
        }
        catch (Exception ex)
        {
            return new ErrorResponseResult($"Error getting power list: {ex.Message}");
        }
    }

    public async Task<ResponseResult> GetById(long id)
    {
        try
        {
            if (id <= 0)
                return new ErrorResponseResult("Power ID must be greater than 0");

            var power = await _repository.GetByIdAsync(id);
            if (power == null)
                return new ErrorResponseResult($"Power with ID {id} not found");

            return new SuccessResponseResult(power, "Get power successfully");
        }
        catch (Exception ex)
        {
            return new ErrorResponseResult($"Error getting power: {ex.Message}");
        }
    }

    public async Task<ResponseResult> Create(PowerCreateDto model)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                return new ErrorResponseResult("Power name is required");

            if (!string.IsNullOrWhiteSpace(model.Code))
            {
                var existingByCode = await _repository.GetByCodeAsync(model.Code);
                if (existingByCode != null)
                    return new ErrorResponseResult($"Power with code '{model.Code}' already exists");
            }

            var power = PowerMappings.CreateDtoToEntity(model);
            var id = await _repository.CreateAsync(power);
            
            return new SuccessResponseResult(new { Id = id }, "Power created successfully");
        }
        catch (Exception ex)
        {
            return new ErrorResponseResult($"Error creating power: {ex.Message}");
        }
    }

    public async Task<ResponseResult> Update(PowerUpdateDto model)
    {
        try
        {
            if (model.Id <= 0)
                return new ErrorResponseResult("Power ID must be greater than 0");

            if (string.IsNullOrWhiteSpace(model.Name))
                return new ErrorResponseResult("Power name is required");

            var existing = await _repository.GetByIdAsync(model.Id);
            if (existing == null)
                return new ErrorResponseResult($"Power with ID {model.Id} not found");

            if (!string.IsNullOrWhiteSpace(model.Code) && model.Code != existing.Code)
            {
                var existingByCode = await _repository.GetByCodeAsync(model.Code);
                if (existingByCode != null)
                    return new ErrorResponseResult($"Power with code '{model.Code}' already exists");
            }

            var power = await _repository.GetByCodeAsync(existing.Code ?? "");
            if (power == null)
                return new ErrorResponseResult("Failed to fetch existing power for update");

            PowerMappings.UpdateDtoToEntity(model, power);
            var success = await _repository.UpdateAsync(power);
            
            if (!success)
                return new ErrorResponseResult("Failed to update power");

            return new SuccessResponseResult(null, "Power updated successfully");
        }
        catch (Exception ex)
        {
            return new ErrorResponseResult($"Error updating power: {ex.Message}");
        }
    }

    public async Task<ResponseResult> Remove(long id)
    {
        try
        {
            if (id <= 0)
                return new ErrorResponseResult("Power ID must be greater than 0");

            var exists = await _repository.ExistsAsync(id);
            if (!exists)
                return new ErrorResponseResult($"Power with ID {id} not found");

            var success = await _repository.DeleteAsync(id);
            if (!success)
                return new ErrorResponseResult("Failed to delete power");

            return new SuccessResponseResult(null, "Power deleted successfully");
        }
        catch (Exception ex)
        {
            return new ErrorResponseResult($"Error deleting power: {ex.Message}");
        }
    }
}
