using System;
using TheLightStore.Application.Dtos;
using TheLightStore.Application.Interfaces.Repositories;
using TheLightStore.Application.Interfaces.Services;
using TheLightStore.Application.Mappings;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Domain.Entities.Products;
using static TheLightStore.Application.Dtos.BaseTypeDto;

namespace TheLightStore.Application.Services;

public class BaseTypeService : IBaseTypeService
{
    private readonly IBaseTypeRepository _repository;

    public BaseTypeService(IBaseTypeRepository repository)
    {
        _repository = repository;
    }

    public async Task<ResponseResult> GetAll(BaseTypeFilterParams parameters)
    {
        try
        {
            var result = await _repository.GetAllAsync(parameters);
            return new SuccessResponseResult(result, "Get base type list successfully");
        }
        catch (Exception ex)
        {
            return new ErrorResponseResult($"Error getting base type list: {ex.Message}");
        }
    }

    public async Task<ResponseResult> GetById(long id)
    {
        try
        {
            if (id <= 0)
                return new ErrorResponseResult("Base type ID must be greater than 0");

            var baseType = await _repository.GetByIdAsync(id);
            if (baseType == null)
                return new ErrorResponseResult($"Base type with ID {id} not found");

            return new SuccessResponseResult(baseType, "Get base type successfully");
        }
        catch (Exception ex)
        {
            return new ErrorResponseResult($"Error getting base type: {ex.Message}");
        }
    }

    public async Task<ResponseResult> Create(BaseTypeCreateDto model)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                return new ErrorResponseResult("Base type name is required");

            if (!string.IsNullOrWhiteSpace(model.Code))
            {
                var existingByCode = await _repository.GetByCodeAsync(model.Code);
                if (existingByCode != null)
                    return new ErrorResponseResult($"Base type with code '{model.Code}' already exists");
            }

            var baseType = BaseTypeMappings.CreateDtoToEntity(model);
            var id = await _repository.CreateAsync(baseType);
            
            return new SuccessResponseResult(new { Id = id }, "Base type created successfully");
        }
        catch (Exception ex)
        {
            return new ErrorResponseResult($"Error creating base type: {ex.Message}");
        }
    }

    public async Task<ResponseResult> Update(BaseTypeUpdateDto model)
    {
        try
        {
            if (model.Id <= 0)
                return new ErrorResponseResult("Base type ID must be greater than 0");

            var existingContent = await _repository.GetByIdAsync(model.Id);
            if (existingContent == null)
                return new ErrorResponseResult($"Base type with ID {model.Id} not found");

            var existingByCode = await _repository.GetByCodeAsync(model.Code!);
            if (existingByCode != null && existingByCode.Id != model.Id)
                return new ErrorResponseResult($"Base type with code '{model.Code}' already exists");

            var baseType = new BaseType { Id = model.Id };
            var updated = BaseTypeMappings.UpdateDtoToEntity(model, baseType);
            
            var success = await _repository.UpdateAsync(updated);
            if (!success)
                return new ErrorResponseResult("Failed to update base type");

            return new SuccessResponseResult(new { Id = model.Id }, "Base type updated successfully");
        }
        catch (Exception ex)
        {
            return new ErrorResponseResult($"Error updating base type: {ex.Message}");
        }
    }

    public async Task<ResponseResult> Remove(long id)
    {
        try
        {
            if (id <= 0)
                return new ErrorResponseResult("Base type ID must be greater than 0");

            var exists = await _repository.ExistsAsync(id);
            if (!exists)
                return new ErrorResponseResult($"Base type with ID {id} not found");

            var success = await _repository.DeleteAsync(id);
            if (!success)
                return new ErrorResponseResult("Failed to delete base type");

            return new SuccessResponseResult(null, "Base type deleted successfully");
        }
        catch (Exception ex)
        {
            return new ErrorResponseResult($"Error deleting base type: {ex.Message}");
        }
    }
}
