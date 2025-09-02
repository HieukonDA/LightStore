
namespace TheLightStore.Repositories.Products;

public class ProductRepo : IProductRepo
{
    private readonly DBContext _context;

    public ProductRepo(DBContext context)
    {
        _context = context;
    }


    #region  basic crud
    public async Task<PagedResult<Product>> GetAllAsync(PagedRequest request)
    {
        var query = _context.Products
       .Where(c => c.IsActive)
       .Include(c => c.Category)
       .Include(c => c.Brand)
       .AsQueryable();

        // Apply search if provided
        if (!string.IsNullOrEmpty(request.Search))
        {
            query = query.Where(c => c.Name.Contains(request.Search) ||
                                    c.Description.Contains(request.Search));
        }

        // Apply sorting
        if (!string.IsNullOrEmpty(request.Sort))
        {
            switch (request.Sort.ToLower())
            {
                case "name":
                    query = query.OrderBy(c => c.Name);
                    break;
                case "createdat":
                    query = query.OrderBy(c => c.CreatedAt);
                    break;
                default:
                    query = query.OrderBy(c => c.Name);
                    break;
            }
        }
        else
        {
            query = query.OrderBy(c => c.Name);
        }

        // Get total count
        var totalCount = await query.CountAsync();

        // Apply pagination
        var products = await query
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .ToListAsync();

        return new PagedResult<Product>
        {
            Items = products,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.Size
        };


    }

    public async Task<Product?> GetByIdAsync(int id, bool includeRelated = false)
    {
        var query = _context.Products
            .Where(c => c.Id == id && c.IsActive)
            .Include(c => c.Category)
            .Include(c => c.Brand)
            .AsQueryable();

        return await query.FirstOrDefaultAsync();
    }

    public async Task<Product?> GetBySlugAsync(string slug, bool includeRelated = false)
    {
        var query = _context.Products
            .Where(c => c.Slug == slug && c.IsActive)
            .Include(c => c.Category)
            .Include(c => c.Brand)
            .AsQueryable();

        if (includeRelated)
        {
            query = query.Include(c => c.Category);
            query = query.Include(c => c.Brand);
        }

        return await query.FirstOrDefaultAsync();
    }

    public async Task<Product> AddAsync(Product product)
    {
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task<Product> UpdateAsync(Product product)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return false;

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Products.AnyAsync(c => c.Id == id && c.IsActive);
    }

    public async Task<bool> SlugExistsAsync(string slug, int? excludeId = null)
    {
        return await _context.Products.AnyAsync(c => c.Slug == slug && c.IsActive && c.Id != excludeId);
    }



    #endregion

    #region get by feature
    
    public async Task<PagedResult<Product>> GetByCategoryAsync(int categoryId, PagedRequest pagedRequest)
    {
        var query = _context.Products
            .Where(c => c.CategoryId == categoryId && c.IsActive)      
            .Include(c => c.Category)
            //.Include(c => c.Brand)
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
                    query = query.OrderBy(c => c.Name);
                    break;
            }
        }
        else
        {
            query = query.OrderBy(c => c.Name);
        }

        // Get total count
        var totalCount = await query.CountAsync();

        // Apply pagination
        var products = await query
            .Skip((pagedRequest.Page - 1) * pagedRequest.Size)
            .Take(pagedRequest.Size)
            .ToListAsync();

        return new PagedResult<Product>
        {
            Items = products,
            TotalCount = totalCount,
            Page = pagedRequest.Page,
            PageSize = pagedRequest.Size
        };
    }
        
    #endregion
}
