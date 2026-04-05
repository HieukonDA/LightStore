using System;
using TheLightStore.Application.Dtos;
using TheLightStore.Application.Interfaces.Repositories;
using TheLightStore.Application.Interfaces.Services;
using TheLightStore.Application.Mappings;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Domain.Entities.Products;
using static TheLightStore.Application.Dtos.ColorTemperatureDto;

namespace TheLightStore.Application.Services;

public class ColorTemperatureService : IColorTemperatureService
{
    private readonly IColorTemperatureRepository _repository;

    public ColorTemperatureService(IColorTemperatureRepository repository)
    {
        _repository = repository;
    }

    public async Task<ResponseResult> GetAll(ColorTemperatureFilterParams parameters)
    {
        try
        {
            var result = await _repository.GetAllAsync(parameters);
            return new SuccessResponseResult(result, "Get color temperature list successfully");
        }
        catch (Exception ex)
        {
            return new ErrorResponseResult($"Error getting color temperature list: {ex.Message}");
        }
    }

    public async Task<ResponseResult> GetById(long id)
    {
        try
        {
            if (id <= 0)
                return new ErrorResponseResult("Color temperature ID must be greater than 0");

            var colorTemperature = await _repository.GetByIdAsync(id);
            if (colorTemperature == null)
                return new ErrorResponseResult($"Color temperature with ID {id} not found");

            return new SuccessResponseResult(colorTemperature, "Get color temperature successfully");
        }
        catch (Exception ex)
        {
            return new ErrorResponseResult($"Error getting color temperature: {ex.Message}");
        }
    }

    public async Task<ResponseResult> Create(ColorTemperatureCreateDto model)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                return new ErrorResponseResult("Color temperature name is required");

            if (!string.IsNullOrWhiteSpace(model.Code))
            {
                var existingByCode = await _repository.GetByCodeAsync(model.Code);
                if (existingByCode != null)
                    return new ErrorResponseResult($"Color temperature with code '{model.Code}' already exists");
            }

            var colorTemperature = ColorTemperatureMappings.CreateDtoToEntity(model);
            var id = await _repository.CreateAsync(colorTemperature);
            
            return new SuccessResponseResult(new { Id = id }, "Color temperature created successfully");
        }
        catch (Exception ex)
        {
            return new ErrorResponseResult($"Error creating color temperature: {ex.Message}");
        }
    }

    public async Task<ResponseResult> Update(ColorTemperatureUpdateDto model)
    {
        try
        {
            if (model.Id <= 0)
                return new ErrorResponseResult("Color temperature ID must be greater than 0");

            var existingContent = await _repository.GetByIdAsync(model.Id);
            if (existingContent == null)
                return new ErrorResponseResult($"Color temperature with ID {model.Id} not found");

            var existingByCode = await _repository.GetByCodeAsync(model.Code!);
            if (existingByCode != null && existingByCode.Id != model.Id)
                return new ErrorResponseResult($"Color temperature with code '{model.Code}' already exists");

            var colorTemperature = new ColorTemperature { Id = model.Id };
            var updated = ColorTemperatureMappings.UpdateDtoToEntity(model, colorTemperature);
            
            var success = await _repository.UpdateAsync(updated);
            if (!success)
                return new ErrorResponseResult("Failed to update color temperature");

            return new SuccessResponseResult(new { Id = model.Id }, "Color temperature updated successfully");
        }
        catch (Exception ex)
        {
            return new ErrorResponseResult($"Error updating color temperature: {ex.Message}");
        }
    }

    public async Task<ResponseResult> Remove(long id)
    {
        try
        {
            if (id <= 0)
                return new ErrorResponseResult("Color temperature ID must be greater than 0");

            var exists = await _repository.ExistsAsync(id);
            if (!exists)
                return new ErrorResponseResult($"Color temperature with ID {id} not found");

            var success = await _repository.DeleteAsync(id);
            if (!success)
                return new ErrorResponseResult("Failed to delete color temperature");

            return new SuccessResponseResult(null, "Color temperature deleted successfully");
        }
        catch (Exception ex)
        {
            return new ErrorResponseResult($"Error deleting color temperature: {ex.Message}");
        }
    }
}
