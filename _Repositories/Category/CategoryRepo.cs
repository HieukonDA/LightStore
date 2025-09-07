

namespace TheLightStore.Repositories.Category;

public class CategoryRepo : ICategoryRepo
{
    private readonly DBContext _context;

    public CategoryRepo(DBContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<CategoryDto>> GetAllCategoriesAsync(PagedRequest pagedRequest)
    {
        var query = _context.Categories
       .Where(c => c.IsActive)
       .AsQueryable();

        // Apply search if provided
        if (!string.IsNullOrEmpty(pagedRequest.Search))
        {
            query = query.Where(c => c.Name.Contains(pagedRequest.Search) ||
                                    c.Description.Contains(pagedRequest.Search));
        }

        // Apply sorting
        if (!string.IsNullOrEmpty(pagedRequest.Sort))
        {
            switch (pagedRequest.Sort.ToLower())
            {
                case "name":
                    query = query.OrderBy(c => c.Name);
                    break;
                case "createdat":
                    query = query.OrderBy(c => c.CreatedAt);
                    break;
                default:
                    query = query.OrderBy(c => c.SortOrder).ThenBy(c => c.Name);
                    break;
            }
        }
        else
        {
            query = query.OrderBy(c => c.SortOrder).ThenBy(c => c.Name);
        }
        
        // Get total count
        var totalCount = await query.CountAsync();

        // Apply pagination
        var categories = await query
            .Skip((pagedRequest.Page - 1) * pagedRequest.Size)
            .Take(pagedRequest.Size)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                Description = c.Description,
                ImageUrl = c.ImageUrl,
                ParentId = c.ParentId,
                SortOrder = c.SortOrder,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .ToListAsync();

        return new PagedResult<CategoryDto>
        {
            Items = categories,
            TotalCount = totalCount,
            Page = pagedRequest.Page,
            PageSize = pagedRequest.Size
        };
    }

    public async Task<CategoryDto> GetCategoryByIdAsync(int id)
    {
        var query = _context.Categories
       .Where(c => c.IsActive && c.Id == id)
       .AsQueryable();

        var categories = await query
            .Take(1)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                Description = c.Description,
                ImageUrl = c.ImageUrl,
                ParentId = c.ParentId,
                SortOrder = c.SortOrder,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .ToListAsync();

        return categories.FirstOrDefault() ?? new CategoryDto();
    }

    public async Task<CategoryDto> CreateCategoryAsync(CategoryDto categoryDto)
    {
        // check category existing
        var existingCategory = await _context.Categories
            .Where(c => c.IsActive && c.Name == categoryDto.Name)
            .FirstOrDefaultAsync();

        if (existingCategory != null)
        {
            throw new Exception("Category already exists");
        }

        var category = new TheLightStore.Models.Categories.Category
        {
            Name = categoryDto.Name,
            Slug = categoryDto.Slug,
            Description = categoryDto.Description,
            ImageUrl = categoryDto.ImageUrl,
            ParentId = categoryDto.ParentId,
            SortOrder = categoryDto.SortOrder,
            IsActive = categoryDto.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        categoryDto.Id = category.Id;
        return categoryDto;
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        // check category existing
        var existingCategory = await _context.Categories
            .Where(c => c.IsActive && c.Id == id)
            .FirstOrDefaultAsync();

        if (existingCategory == null)
        {
            return false;
            throw new Exception("Category not found");
        }

        _context.Categories.Remove(existingCategory);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<CategoryDto> UpdateCategoryAsync(int id, CategoryDto categoryDto)
    {
        var existingCategory = await _context.Categories
            .Where(c => c.IsActive && c.Id == id)
            .FirstOrDefaultAsync();

        if (existingCategory == null)
        {
            throw new Exception("Category not found");
        }

        existingCategory.Name = categoryDto.Name;
        existingCategory.Slug = categoryDto.Slug;
        existingCategory.Description = categoryDto.Description;
        existingCategory.ImageUrl = categoryDto.ImageUrl;
        existingCategory.ParentId = categoryDto.ParentId;
        existingCategory.SortOrder = categoryDto.SortOrder;
        existingCategory.IsActive = categoryDto.IsActive;
        existingCategory.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return categoryDto;
    }


}