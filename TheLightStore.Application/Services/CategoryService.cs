using System;
using TheLightStore.Application.Dtos;
using TheLightStore.Application.Interfaces.Repositories;
using TheLightStore.Application.Interfaces.Services;
using TheLightStore.Application.Mappings;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Domain.Entities.Products;

namespace TheLightStore.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<ResponseResult> GetAll(CategoryDto.CategoryFilterParams filterParams)
    {
        try
        {
            var result = await _categoryRepository.GetAllAsync(filterParams);
            return new SuccessResponseResult(result);
        }
        catch (Exception ex)
        {
            return new ErrorResponseResult(ex.Message);
        }
    }

    public async Task<ResponseResult> GetById(long id)
    {
        if (id <= 0)
            return new ErrorResponseResult("ID phải lớn hơn 0");

        var result = await _categoryRepository.GetByIdAsync(id);
        if (result == null)
            return new ErrorResponseResult("Không tìm thấy phân loại");

        return new SuccessResponseResult(result);
    }

    public async Task<ResponseResult> Create(CategoryDto.CategoryCreateDto createDto)
    {
        if (string.IsNullOrWhiteSpace(createDto.Name))
            return new ErrorResponseResult("Tên phân loại là bắt buộc");

        if (!string.IsNullOrWhiteSpace(createDto.Code))
        {
            var existingCategory = await _categoryRepository.GetByCodeAsync(createDto.Code);
            if (existingCategory != null)
                return new ErrorResponseResult("Mã phân loại đã tồn tại");
        }

        var category = CategoryMappings.CreateDtoToEntity(createDto);
        category.CreatedDate = DateTime.UtcNow;
        category.IsActive = createDto.IsActive ?? true;

        var id = await _categoryRepository.CreateAsync(category);
        return new SuccessResponseResult(id);
    }

    public async Task<ResponseResult> Update(CategoryDto.CategoryUpdateDto updateDto)
    {
        if (updateDto.Id <= 0)
            return new ErrorResponseResult("ID phải lớn hơn 0");

        var existingCategory = await _categoryRepository.GetByIdAsync(updateDto.Id);
        if (existingCategory == null)
            return new ErrorResponseResult("Không tìm thấy phân loại");

        if (!string.IsNullOrWhiteSpace(updateDto.Code))
        {
            var categoryWithSameCode = await _categoryRepository.GetByCodeAsync(updateDto.Code);
            if (categoryWithSameCode != null && categoryWithSameCode.Id != updateDto.Id)
                return new ErrorResponseResult("Mã phân loại đã tồn tại");
        }

        var category = await _categoryRepository.GetByIdAsync(updateDto.Id);
        var updatedCategory = CategoryMappings.UpdateDtoToEntity(updateDto, new Category { Id = updateDto.Id });
        updatedCategory.UpdatedDate = DateTime.UtcNow;

        var result = await _categoryRepository.UpdateAsync(updatedCategory);
        return result 
            ? new SuccessResponseResult("Cập nhật phân loại thành công")
            : new ErrorResponseResult("Cập nhật phân loại thất bại");
    }

    public async Task<ResponseResult> Remove(long id)
    {
        if (id <= 0)
            return new ErrorResponseResult("ID phải lớn hơn 0");

        var exists = await _categoryRepository.ExistsAsync(id);
        if (!exists)
            return new ErrorResponseResult("Không tìm thấy phân loại");

        var result = await _categoryRepository.DeleteAsync(id);
        return result 
            ? new SuccessResponseResult("Xóa phân loại thành công")
            : new ErrorResponseResult("Xóa phân loại thất bại");
    }
}
