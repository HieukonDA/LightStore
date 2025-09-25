
using Microsoft.EntityFrameworkCore.Storage;
using TheLightStore.Dtos.Product;

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
       .Include(c => c.ProductImages.Where(img => img.IsPrimary == true)) // ✅ Include primary image cho list
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
            .Include(c => c.ProductImages) // ✅ Include all images for detail view
            .AsQueryable();

        return await query.FirstOrDefaultAsync();
    }

    public async Task<Product?> GetBySlugAsync(string slug, bool includeRelated = false)
    {
        var query = _context.Products
            .Where(c => c.Slug == slug && c.IsActive)
            .Include(c => c.Category)
            .Include(c => c.Brand)
            .Include(c => c.ProductImages) // ✅ Include all images for detail view
            .AsQueryable();

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
            .Include(c => c.Brand)
            .Include(c => c.ProductImages.Where(img => img.IsPrimary == true)) // ✅ Include primary image
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

    public async Task<PagedResult<Product>> GetByCategorySlugAsync(string categorySlug, PagedRequest pagedRequest)
    {
        if (string.IsNullOrWhiteSpace(categorySlug))
            throw new ArgumentException("Category slug is required", nameof(categorySlug));

        // Join với bảng Category theo Slug
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.ProductImages.Where(img => img.IsPrimary == true))
            .Where(p => p.IsActive && p.Category != null && p.Category.Slug == categorySlug)
            .AsQueryable();

        // Search
        if (!string.IsNullOrEmpty(pagedRequest.Search))
        {
            query = query.Where(p => p.Name.Contains(pagedRequest.Search) ||
                                    p.Description.Contains(pagedRequest.Search));
        }

        // Sorting
        if (!string.IsNullOrEmpty(pagedRequest.Sort))
        {
            switch (pagedRequest.Sort.ToLower())
            {
                case "name":
                    query = query.OrderBy(p => p.Name);
                    break;
                case "createdat":
                    query = query.OrderBy(p => p.CreatedAt);
                    break;
                default:
                    query = query.OrderBy(p => p.Name);
                    break;
            }
        }
        else
        {
            query = query.OrderBy(p => p.Name);
        }

        // Total count
        var totalCount = await query.CountAsync();

        // Pagination
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




    public async Task<Dictionary<int, ProductAvailabilityInfo>> GetProductsAvailabilityWithLockAsync(
        List<int> productIds,
        IDbContextTransaction transaction)
    {
        if (productIds == null || productIds.Count == 0)
            return new Dictionary<int, ProductAvailabilityInfo>();

        // ⚠️ SQL Server: dùng WITH (UPDLOCK, ROWLOCK)
        var sql = $@"
            SELECT Id, StockQuantity
            FROM Products WITH (UPDLOCK, ROWLOCK)
            WHERE Id IN ({string.Join(",", productIds)})
        ";

        var result = await _context.Products
            .FromSqlRaw(sql)
            .Select(p => new ProductAvailabilityInfo
            {
                ProductId = p.Id,
                VariantId = null,
                AvailableQuantity = p.StockQuantity
            })
            .ToDictionaryAsync(x => x.ProductId);

        return result;
    }

    #endregion

    #region search method

    public async Task<PagedResult<Product>> SearchAsync(SearchProductsRequest request)
    {
        try
        {
            var baseQuery = _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.IsActive);

            IQueryable<Product> query;

            if (!string.IsNullOrWhiteSpace(request.Query))
            {
                var searchTerm = request.Query.Trim();

                // Xử lý d ↔ đ
                string sqlQuery;
                if (searchTerm.Contains('d'))
                {
                    var searchWithDHook = searchTerm.Replace('d', 'đ');
                    sqlQuery = @"
                        SELECT p.* 
                        FROM Products p
                        LEFT JOIN Brands b ON p.BrandId = b.Id
                        WHERE p.IsActive = 1 
                        AND (
                            p.Name COLLATE Vietnamese_CI_AI LIKE '%' + {0} + '%' OR
                            p.Name COLLATE Vietnamese_CI_AI LIKE '%' + {1} + '%' OR
                            (p.Description IS NOT NULL AND (p.Description COLLATE Vietnamese_CI_AI LIKE '%' + {0} + '%' OR p.Description COLLATE Vietnamese_CI_AI LIKE '%' + {1} + '%')) OR
                            (p.ShortDescription IS NOT NULL AND (p.ShortDescription COLLATE Vietnamese_CI_AI LIKE '%' + {0} + '%' OR p.ShortDescription COLLATE Vietnamese_CI_AI LIKE '%' + {1} + '%')) OR
                            p.Sku COLLATE Vietnamese_CI_AI LIKE '%' + {0} + '%' OR
                            p.Sku COLLATE Vietnamese_CI_AI LIKE '%' + {1} + '%' OR
                            (b.Name IS NOT NULL AND (b.Name COLLATE Vietnamese_CI_AI LIKE '%' + {0} + '%' OR b.Name COLLATE Vietnamese_CI_AI LIKE '%' + {1} + '%'))
                        )";

                    query = _context.Products
                        .FromSqlRaw(sqlQuery, searchTerm, searchWithDHook)
                        .Include(p => p.ProductImages)
                        .Include(p => p.Category)
                        .Include(p => p.Brand);
                }
                else if (searchTerm.Contains('đ'))
                {
                    var searchWithD = searchTerm.Replace('đ', 'd');
                    sqlQuery = @"
                        SELECT p.* 
                        FROM Products p
                        LEFT JOIN Brands b ON p.BrandId = b.Id
                        WHERE p.IsActive = 1 
                        AND (
                            p.Name COLLATE Vietnamese_CI_AI LIKE '%' + {0} + '%' OR
                            p.Name COLLATE Vietnamese_CI_AI LIKE '%' + {1} + '%' OR
                            (p.Description IS NOT NULL AND (p.Description COLLATE Vietnamese_CI_AI LIKE '%' + {0} + '%' OR p.Description COLLATE Vietnamese_CI_AI LIKE '%' + {1} + '%')) OR
                            (p.ShortDescription IS NOT NULL AND (p.ShortDescription COLLATE Vietnamese_CI_AI LIKE '%' + {0} + '%' OR p.ShortDescription COLLATE Vietnamese_CI_AI LIKE '%' + {1} + '%')) OR
                            p.Sku COLLATE Vietnamese_CI_AI LIKE '%' + {0} + '%' OR
                            p.Sku COLLATE Vietnamese_CI_AI LIKE '%' + {1} + '%' OR
                            (b.Name IS NOT NULL AND (b.Name COLLATE Vietnamese_CI_AI LIKE '%' + {0} + '%' OR b.Name COLLATE Vietnamese_CI_AI LIKE '%' + {1} + '%'))
                        )";

                    query = _context.Products
                        .FromSqlRaw(sqlQuery, searchTerm, searchWithD)
                        .Include(p => p.ProductImages)
                        .Include(p => p.Category)
                        .Include(p => p.Brand);
                }
                else
                {
                    // Không có d hoặc đ, dùng query bình thường
                    sqlQuery = @"
                        SELECT p.* 
                        FROM Products p
                        LEFT JOIN Brands b ON p.BrandId = b.Id
                        WHERE p.IsActive = 1 
                        AND (
                            p.Name COLLATE Vietnamese_CI_AI LIKE '%' + {0} + '%' OR
                            (p.Description IS NOT NULL AND p.Description COLLATE Vietnamese_CI_AI LIKE '%' + {0} + '%') OR
                            (p.ShortDescription IS NOT NULL AND p.ShortDescription COLLATE Vietnamese_CI_AI LIKE '%' + {0} + '%') OR
                            p.Sku COLLATE Vietnamese_CI_AI LIKE '%' + {0} + '%' OR
                            (b.Name IS NOT NULL AND b.Name COLLATE Vietnamese_CI_AI LIKE '%' + {0} + '%')
                        )";

                    query = _context.Products
                        .FromSqlRaw(sqlQuery, searchTerm)
                        .Include(p => p.ProductImages)
                        .Include(p => p.Category)
                        .Include(p => p.Brand);
                }
            }
            else
            {
                query = baseQuery;
            }

            // Apply other filters
            if (request.CategoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == request.CategoryId.Value);
            }

            if (request.BrandId.HasValue)
            {
                query = query.Where(p => p.BrandId == request.BrandId.Value);
            }

            if (request.MinPrice.HasValue)
            {
                query = query.Where(p => p.SalePrice >= request.MinPrice.Value);
            }

            if (request.MaxPrice.HasValue)
            {
                query = query.Where(p => p.SalePrice <= request.MaxPrice.Value);
            }

            if (request.InStock.HasValue && request.InStock.Value)
            {
                query = query.Where(p => p.ManageStock == false || p.StockQuantity > 0);
            }

            // Apply sorting
            query = ApplySorting(query, request.SortBy);

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
                PageSize = request.Size,
            };
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<List<Product>> GetProductSuggestionsAsync(string query, int limit = 10)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new List<Product>();
            }

            if (limit <= 0 || limit > 50)
            {
                limit = 10;
            }

            var searchTerm = query.Trim();

            // Xử lý d ↔ đ cho suggestions
            IQueryable<Product> suggestions;

            if (searchTerm.Contains('d'))
            {
                var searchWithDHook = searchTerm.Replace('d', 'đ');
                suggestions = _context.Products
                    .Include(p => p.ProductImages.Where(i => i.IsPrimary == true))
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .Where(p => p.IsActive)
                    .Where(p => p.ManageStock == false || p.StockQuantity > 0)
                    .Where(p =>
                        p.Name.Contains(searchTerm) ||
                        p.Name.Contains(searchWithDHook) ||
                        p.Sku.Contains(searchTerm) ||
                        p.Sku.Contains(searchWithDHook) ||
                        (p.Brand != null && (p.Brand.Name.Contains(searchTerm) || p.Brand.Name.Contains(searchWithDHook)))
                    );
            }
            else if (searchTerm.Contains('đ'))
            {
                var searchWithD = searchTerm.Replace('đ', 'd');
                suggestions = _context.Products
                    .Include(p => p.ProductImages.Where(i => i.IsPrimary == true))
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .Where(p => p.IsActive)
                    .Where(p => p.ManageStock == false || p.StockQuantity > 0)
                    .Where(p =>
                        p.Name.Contains(searchTerm) ||
                        p.Name.Contains(searchWithD) ||
                        p.Sku.Contains(searchTerm) ||
                        p.Sku.Contains(searchWithD) ||
                        (p.Brand != null && (p.Brand.Name.Contains(searchTerm) || p.Brand.Name.Contains(searchWithD)))
                    );
            }
            else
            {
                suggestions = _context.Products
                    .Include(p => p.ProductImages.Where(i => i.IsPrimary == true))
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .Where(p => p.IsActive)
                    .Where(p => p.ManageStock == false || p.StockQuantity > 0)
                    .Where(p =>
                        p.Name.Contains(searchTerm) ||
                        p.Sku.Contains(searchTerm) ||
                        (p.Brand != null && p.Brand.Name.Contains(searchTerm))
                    );
            }

            return await suggestions
                .OrderByDescending(p => p.IsFeatured)
                .ThenBy(p => p.Name)
                .Take(limit)
                .ToListAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<Dictionary<string, int>> GetSearchFiltersAsync(string query)
    {
        try
        {
            var baseQuery = _context.Products.Where(p => p.IsActive);

            // Apply search filter với xử lý d ↔ đ
            if (!string.IsNullOrWhiteSpace(query))
            {
                var searchTerm = query.Trim();

                if (searchTerm.Contains('d'))
                {
                    var searchWithDHook = searchTerm.Replace('d', 'đ');
                    baseQuery = baseQuery.Where(p =>
                        p.Name.Contains(searchTerm) ||
                        p.Name.Contains(searchWithDHook) ||
                        (p.Description != null && (p.Description.Contains(searchTerm) || p.Description.Contains(searchWithDHook))) ||
                        (p.ShortDescription != null && (p.ShortDescription.Contains(searchTerm) || p.ShortDescription.Contains(searchWithDHook))) ||
                        p.Sku.Contains(searchTerm) ||
                        p.Sku.Contains(searchWithDHook) ||
                        (p.Brand != null && (p.Brand.Name.Contains(searchTerm) || p.Brand.Name.Contains(searchWithDHook)))
                    );
                }
                else if (searchTerm.Contains('đ'))
                {
                    var searchWithD = searchTerm.Replace('đ', 'd');
                    baseQuery = baseQuery.Where(p =>
                        p.Name.Contains(searchTerm) ||
                        p.Name.Contains(searchWithD) ||
                        (p.Description != null && (p.Description.Contains(searchTerm) || p.Description.Contains(searchWithD))) ||
                        (p.ShortDescription != null && (p.ShortDescription.Contains(searchTerm) || p.ShortDescription.Contains(searchWithD))) ||
                        p.Sku.Contains(searchTerm) ||
                        p.Sku.Contains(searchWithD) ||
                        (p.Brand != null && (p.Brand.Name.Contains(searchTerm) || p.Brand.Name.Contains(searchWithD)))
                    );
                }
                else
                {
                    baseQuery = baseQuery.Where(p =>
                        p.Name.Contains(searchTerm) ||
                        (p.Description != null && p.Description.Contains(searchTerm)) ||
                        (p.ShortDescription != null && p.ShortDescription.Contains(searchTerm)) ||
                        p.Sku.Contains(searchTerm) ||
                        (p.Brand != null && p.Brand.Name.Contains(searchTerm))
                    );
                }
            }

            var filters = new Dictionary<string, int>();

            // Get category counts
            var categoryFilters = await baseQuery
                .Include(p => p.Category)
                .Where(p => p.Category != null)
                .GroupBy(p => p.Category.Name)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .ToListAsync();

            foreach (var category in categoryFilters)
            {
                filters.Add($"category_{category.Name}", category.Count);
            }

            // Get brand counts
            var brandFilters = await baseQuery
                .Include(p => p.Brand)
                .Where(p => p.Brand != null)
                .GroupBy(p => p.Brand.Name)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .ToListAsync();

            foreach (var brand in brandFilters)
            {
                filters.Add($"brand_{brand.Name}", brand.Count);
            }

            // Get price range info
            var priceStats = await baseQuery
                .Where(p => p.SalePrice > 0)
                .GroupBy(p => 1)
                .Select(g => new
                {
                    MinPrice = g.Min(p => p.SalePrice),
                    MaxPrice = g.Max(p => p.SalePrice),
                    Count = g.Count()
                })
                .FirstOrDefaultAsync();

            if (priceStats != null)
            {
                filters.Add("min_price", (int)priceStats.MinPrice);
                filters.Add("max_price", (int)priceStats.MaxPrice);
                filters.Add("total_products", priceStats.Count);
            }

            // Get stock status counts
            var inStockCount = await baseQuery
                .Where(p => p.ManageStock == false || p.StockQuantity > 0)
                .CountAsync();

            var outOfStockCount = await baseQuery
                .Where(p => p.ManageStock == true && p.StockQuantity <= 0)
                .CountAsync();

            filters.Add("in_stock", inStockCount);
            filters.Add("out_of_stock", outOfStockCount);

            return filters;
        }
        catch (Exception)
        {
            throw;
        }
    }

    #endregion

    #region helper

    private IQueryable<Product> ApplySorting(IQueryable<Product> query, ProductSortBy sortBy)
    {
        return sortBy switch
        {
            ProductSortBy.PriceAsc => query.OrderBy(p => p.SalePrice),
            ProductSortBy.PriceDesc => query.OrderByDescending(p => p.SalePrice),
            ProductSortBy.Newest => query.OrderByDescending(p => p.CreatedAt),
            ProductSortBy.Name => query.OrderBy(p => p.Name),
            ProductSortBy.Popular => query.OrderByDescending(p => p.IsFeatured).ThenBy(p => p.Name),
            ProductSortBy.Rating => query.OrderBy(p => p.Name), // No rating field, fallback to name
            _ => query.OrderByDescending(p => p.IsFeatured).ThenBy(p => p.Name) // Relevance
        };
    }

    #endregion
}