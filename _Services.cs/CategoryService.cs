
using Microsoft.AspNetCore.Http.HttpResults;

namespace TheLightStore.Services;

public class CategoryService : ICategoryService
{
    readonly private DBContext _context;
    readonly private ICategoryRepo _categoryRepo;
    readonly private ILogger<CategoryService> _logger;

    public CategoryService(DBContext context, ICategoryRepo categoryRepo, ILogger<CategoryService> logger)
    {
        _context = context;
        _categoryRepo = categoryRepo;
        _logger = logger;
    }

    public async Task<ServiceResult<PagedResult<CategoryDto>>> GetAllCategoriesAsync(PagedRequest pagedRequest)
    {
        try
        {
            _logger.LogInformation("Fetching all categories");

            // Validate input
            if (pagedRequest == null)
            {
                return ServiceResult<PagedResult<CategoryDto>>.FailureResult("Paged request cannot be null", new List<string>());
            }

            if (pagedRequest.Page < 1)
            {
                return ServiceResult<PagedResult<CategoryDto>>.FailureResult(
                    "Page number must be greater than 0",
                    new List<string>());
            }

            if (pagedRequest.Size < 1 || pagedRequest.Size > 100)
            {
                return ServiceResult<PagedResult<CategoryDto>>.FailureResult(
                    "Page size must be between 1 and 100",
                    new List<string>());
            }

            // Call repository
            var result = await _categoryRepo.GetAllCategoriesAsync(pagedRequest);

            // Check if result is null (shouldn't happen with proper repo implementation)
            if (result == null)
            {
                _logger.LogWarning("Repository returned null result for GetAllCategoriesAsync");
                return ServiceResult<PagedResult<CategoryDto>>.FailureResult("No data available", new List<string>());
            }

            // Return success result
            return ServiceResult<PagedResult<CategoryDto>>.SuccessResult(result);
        }
        catch (System.Exception ex)
        {
             _logger.LogWarning(ex, "Invalid argument provided to GetAllCategoriesAsync");
            return ServiceResult<PagedResult<CategoryDto>>.FailureResult(ex.Message, new List<string>());
        }
    }

    public async Task<ServiceResult<CategoryDto>> GetCategoryByIdAsync(int id)
    {
        try
        {
            _logger.LogInformation("get category by id");

            //check category
            var category = await _categoryRepo.GetCategoryByIdAsync(id);

            return ServiceResult<CategoryDto>.SuccessResult(category);
        }
        catch (System.Exception ex)
        {
            _logger.LogWarning("Error occurred while fetching category by id");
            return ServiceResult<CategoryDto>.FailureResult(ex.Message, new List<string>());
        }
    }

    public async Task<ServiceResult<CategoryDto>> CreateCategoryAsync(CategoryDto categoryDto)
    {
        try
        {
            _logger.LogInformation("Creating new category");

            // Validate input
            if (categoryDto == null)
            {
                return ServiceResult<CategoryDto>.FailureResult("Category DTO cannot be null", new List<string>());
            }

            // Call repository
            var result = await _categoryRepo.CreateCategoryAsync(categoryDto);

            return ServiceResult<CategoryDto>.SuccessResult(result);
        }
        catch (System.Exception ex)
        {
            _logger.LogWarning("Error occurred while creating category");
            return ServiceResult<CategoryDto>.FailureResult(ex.Message, new List<string>());
        }
    }

    public async Task<ServiceResult<bool>> DeleteCategoryAsync(int id)
    {
        try
        {
            _logger.LogInformation("Deleting category");

            // Call repository
            var result = await _categoryRepo.DeleteCategoryAsync(id);

            return ServiceResult<bool>.SuccessResult(result);
        }
        catch (System.Exception ex)
        {
            _logger.LogWarning("Error occurred while deleting category");
            return ServiceResult<bool>.FailureResult(ex.Message, new List<string>());
        }
    }
    

    

    public async Task<ServiceResult<CategoryDto>> UpdateCategoryAsync(int id, CategoryDto categoryDto)
    {
        try
        {
            _logger.LogInformation("Updating category");
            // validate category
            if (categoryDto == null)
            {
                return ServiceResult<CategoryDto>.FailureResult("Category DTO cannot be null", new List<string>());
            }

            // Call repository
            var result = await _categoryRepo.UpdateCategoryAsync(id, categoryDto);

            return ServiceResult<CategoryDto>.SuccessResult(result);
        }
        catch (System.Exception ex)
        {
            _logger.LogWarning("Error occurred while updating category");
            return ServiceResult<CategoryDto>.FailureResult(ex.Message, new List<string>());
        }
    }

}