using System;
using TheLightStore.Application.Dtos;
using TheLightStore.Application.Interfaces.Repositories;
using TheLightStore.Application.Interfaces.Services;
using TheLightStore.Application.Mappings;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Domain.Entities.Products;
using static TheLightStore.Application.Dtos.ShapeDto;

namespace TheLightStore.Application.Services;

public class ShapeService : IShapeService
{
    private readonly IShapeRepository _repository;

    public ShapeService(IShapeRepository repository)
    {
        _repository = repository;
    }

    public async Task<ResponseResult> GetAll(ShapeFilterParams parameters)
    {
        try
        {
            var result = await _repository.GetAllAsync(parameters);
            return new SuccessResponseResult(result, "Get shape list successfully");
        }
        catch (Exception ex)
        {
            return new ErrorResponseResult($"Error getting shape list: {ex.Message}");
        }
    }

    public async Task<ResponseResult> GetById(long id)
    {
        try
        {
            if (id <= 0)
                return new ErrorResponseResult("Shape ID must be greater than 0");

            var shape = await _repository.GetByIdAsync(id);
            if (shape == null)
                return new ErrorResponseResult($"Shape with ID {id} not found");

            return new SuccessResponseResult(shape, "Get shape successfully");
        }
        catch (Exception ex)
        {
            return new ErrorResponseResult($"Error getting shape: {ex.Message}");
        }
    }

    public async Task<ResponseResult> Create(ShapeCreateDto model)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                return new ErrorResponseResult("Shape name is required");

            if (!string.IsNullOrWhiteSpace(model.Code))
            {
                var existingByCode = await _repository.GetByCodeAsync(model.Code);
                if (existingByCode != null)
                    return new ErrorResponseResult($"Shape with code '{model.Code}' already exists");
            }

            var shape = ShapeMappings.CreateDtoToEntity(model);
            var id = await _repository.CreateAsync(shape);
            
            return new SuccessResponseResult(new { Id = id }, "Shape created successfully");
        }
        catch (Exception ex)
        {
            return new ErrorResponseResult($"Error creating shape: {ex.Message}");
        }
    }

    public async Task<ResponseResult> Update(ShapeUpdateDto model)
    {
        try
        {
            if (model.Id <= 0)
                return new ErrorResponseResult("Shape ID must be greater than 0");

            var existingContent = await _repository.GetByIdAsync(model.Id);
            if (existingContent == null)
                return new ErrorResponseResult($"Shape with ID {model.Id} not found");

            var existingByCode = await _repository.GetByCodeAsync(model.Code!);
            if (existingByCode != null && existingByCode.Id != model.Id)
                return new ErrorResponseResult($"Shape with code '{model.Code}' already exists");

            var shape = new Shape { Id = model.Id };
            var updated = ShapeMappings.UpdateDtoToEntity(model, shape);
            
            var success = await _repository.UpdateAsync(updated);
            if (!success)
                return new ErrorResponseResult("Failed to update shape");

            return new SuccessResponseResult(new { Id = model.Id }, "Shape updated successfully");
        }
        catch (Exception ex)
        {
            return new ErrorResponseResult($"Error updating shape: {ex.Message}");
        }
    }

    public async Task<ResponseResult> Remove(long id)
    {
        try
        {
            if (id <= 0)
                return new ErrorResponseResult("Shape ID must be greater than 0");

            var exists = await _repository.ExistsAsync(id);
            if (!exists)
                return new ErrorResponseResult($"Shape with ID {id} not found");

            var success = await _repository.DeleteAsync(id);
            if (!success)
                return new ErrorResponseResult("Failed to delete shape");

            return new SuccessResponseResult(null, "Shape deleted successfully");
        }
        catch (Exception ex)
        {
            return new ErrorResponseResult($"Error deleting shape: {ex.Message}");
        }
    }
}
