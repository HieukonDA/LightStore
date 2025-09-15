using System.Text.RegularExpressions;
using TheLightStore.Interfaces.Products;

namespace TheLightStore.Services.Products;

public class ProductService : IProductService
{
    private readonly DBContext _context;
    private readonly IProductRepo _productRepo;
    private readonly ILogger<ProductService> _logger;

    public ProductService(DBContext context, IProductRepo productRepo, ILogger<ProductService> logger)
    {
        _context = context;
        _productRepo = productRepo;
        _logger = logger;
    }

    #region basic crud

    public async Task<ServiceResult<ProductDto>> GetByIdAsync(int id)
    {
        try
        {
            _logger.LogInformation("Fetching product with ID {ProductId}", id);
            // validate id 
            if (id <= 0)
            {
                return ServiceResult<ProductDto>.FailureResult("Invalid product ID", new List<string>());
            }


            //call repo
            var product = await _productRepo.GetByIdAsync(id, includeRelated: true);
            if (product == null)
            {
                _logger.LogWarning("Product with ID {ProductId} not found", id);
                return ServiceResult<ProductDto>.FailureResult("Product not found", new List<string>());
            }

            var productDto = MapToDto(product);
            return ServiceResult<ProductDto>.SuccessResult(productDto);
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching product with ID {ProductId}", id);
            return ServiceResult<ProductDto>.FailureResult("An error occurred while processing your request", new List<string> { ex.Message });
        }
    }

    public async Task<ServiceResult<ProductDto>> GetBySlugAsync(string slug)
    {
        try
        {
            _logger.LogInformation("Fetching product with slug {ProductSlug}", slug);
            // validate slug
            if (string.IsNullOrWhiteSpace(slug))
            {
                _logger.LogWarning("Invalid slug provided");
                return ServiceResult<ProductDto>.FailureResult("Invalid slug", new List<string>());
            }

            //call repo
            var product = await _productRepo.GetBySlugAsync(slug, includeRelated: true);
            if (product == null)
            {
                _logger.LogWarning("Product with slug {ProductSlug} not found", slug);
                return ServiceResult<ProductDto>.FailureResult("Product not found", new List<string>());
            }

            var productDto = MapToDto(product);
            return ServiceResult<ProductDto>.SuccessResult(productDto);
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching product with slug {ProductSlug}", slug);
            return ServiceResult<ProductDto>.FailureResult("An error occurred while processing your request", new List<string> { ex.Message });
        }
    }

    public async Task<ServiceResult<PagedResult<ProductListDto>>> GetPagedAsync(PagedRequest pagedRequest)
    {
        try
        {
            _logger.LogInformation("Fetching all products");

            // Validate input
            if (pagedRequest == null)
            {
                return ServiceResult<PagedResult<ProductListDto>>.FailureResult("Paged request cannot be null", new List<string>());
            }

            if (pagedRequest.Page < 1)
            {
                return ServiceResult<PagedResult<ProductListDto>>.FailureResult(
                    "Page number must be greater than 0",
                    new List<string>());
            }

            if (pagedRequest.Size < 1 || pagedRequest.Size > 100)
            {
                return ServiceResult<PagedResult<ProductListDto>>.FailureResult(
                    "Page size must be between 1 and 100",
                    new List<string>());
            }

            // Call repository
            var result = await _productRepo.GetAllAsync(pagedRequest);

            // Check if result is null (shouldn't happen with proper repo implementation)
            if (result == null)
            {
                _logger.LogWarning("Repository returned null result for GetAllProductsAsync");
                return ServiceResult<PagedResult<ProductListDto>>.FailureResult("No data available", new List<string>());
            }

            var productListDtos = result.Items.Select(MapToListDto).ToList();


            // Return success result
            return ServiceResult<PagedResult<ProductListDto>>.SuccessResult(new PagedResult<ProductListDto>
            {
                Items = productListDtos,
                TotalCount = result.TotalCount,
                Page = pagedRequest.Page,
                PageSize = pagedRequest.Size
            });
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching paged products");
            return ServiceResult<PagedResult<ProductListDto>>.FailureResult("An error occurred while processing your request", new List<string> { ex.Message });
        }
    }


    public async Task<ServiceResult<ProductDto>> CreateAsync(CreateProductDto dto)
    {
        try
        {
            _logger.LogInformation("Creating new product");
            // validate dto
            if (dto == null)
            {
                _logger.LogWarning("CreateProductDto is null");
                return ServiceResult<ProductDto>.FailureResult("Invalid product data", new List<string>());
            }

            var validationErrors = await ValidateCreateProductDto(dto);
            if (validationErrors.Any())
            {
                return ServiceResult<ProductDto>.FailureResult("Validation failed", validationErrors);
            }

            // Map DTO to Entity
            var product = MapCreateDtoToEntity(dto);

            // create product from repo
            var createdProduct = await _productRepo.AddAsync(product);
            if (createdProduct == null)
            {
                _logger.LogWarning("Failed to create product");
                return ServiceResult<ProductDto>.FailureResult("Failed to create product", new List<string>());
            }

            // Reload with related data
            var productWithRelations = await _productRepo.GetByIdAsync(createdProduct.Id, includeRelated: true);
            if (productWithRelations == null)
            {
                _logger.LogWarning("Failed to reload created product with relations");
                return ServiceResult<ProductDto>.FailureResult("Product created but failed to load details", new List<string>());
            }

            var productDto = MapToDto(productWithRelations);
            _logger.LogInformation("Successfully created product with ID {ProductId}", createdProduct.Id);

            return ServiceResult<ProductDto>.SuccessResult(productDto);
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating product");
            return ServiceResult<ProductDto>.FailureResult("An error occurred while processing your request", new List<string> { ex.Message });
        }
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        try
        {
            _logger.LogInformation("Deleting product with ID {ProductId}", id);
            // validate id
            if (id <= 0)
            {
                return ServiceResult<bool>.FailureResult("Invalid product ID", new List<string>());
            }

            //check product existing
            var existingProduct = await _productRepo.GetByIdAsync(id);
            if (existingProduct == null)
            {
                _logger.LogWarning("Product with ID {ProductId} not found", id);
                return ServiceResult<bool>.FailureResult("Product not found", new List<string>());
            }

            // Delete product
            var result = await _productRepo.DeleteAsync(id);
            if (!result)
            {
                _logger.LogWarning("Failed to delete product with ID {ProductId}", id);
                return ServiceResult<bool>.FailureResult("Failed to delete product", new List<string>());
            }

            _logger.LogInformation("Successfully deleted product with ID {ProductId}", id);
            return ServiceResult<bool>.SuccessResult(true);
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting product");
            return ServiceResult<bool>.FailureResult("An error occurred while processing your request", new List<string> { ex.Message });
        }
    }

    public async Task<ServiceResult<ProductDto>> UpdateAsync(int id, UpdateProductDto dto)
    {
        try
        {
            _logger.LogInformation("Updating product with ID {ProductId}", id);
            //validate dto
            if (dto == null)
            {
                _logger.LogWarning("UpdateProductDto is null");
                return ServiceResult<ProductDto>.FailureResult("Invalid product data", new List<string>());
            }
            var validationErrors = await ValidateUpdateProductDto(dto, id);
            if (validationErrors.Any())
            {
                return ServiceResult<ProductDto>.FailureResult("Validation failed", validationErrors);
            }

            //check product existing
            var existingProduct = await _productRepo.GetByIdAsync(id, includeRelated: true);
            if (existingProduct == null)
            {
                _logger.LogWarning("Product with ID {ProductId} not found", id);
                return ServiceResult<ProductDto>.FailureResult("Product not found", new List<string>());
            }

            // Map DTO to existing entity (preserve existing values)
            MapUpdateDtoToEntity(dto, existingProduct);


            // Update product
            var updatedProduct = await _productRepo.UpdateAsync(existingProduct);
            if (updatedProduct == null)
            {
                _logger.LogWarning("Failed to update product with ID {ProductId}", id);
                return ServiceResult<ProductDto>.FailureResult("Failed to update product", new List<string>());
            }

            // Reload with updated relations
            var productWithRelations = await _productRepo.GetByIdAsync(id, includeRelated: true);
            var productDto = MapToDto(productWithRelations ?? updatedProduct);

            _logger.LogInformation("Successfully updated product with ID {ProductId}", id);
            return ServiceResult<ProductDto>.SuccessResult(productDto);
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating product");
            return ServiceResult<ProductDto>.FailureResult("An error occurred while processing your request", new List<string> { ex.Message });
        }
    }



    #endregion

    #region get by collection


    public async Task<ServiceResult<PagedResult<ProductListDto>>> GetByCategoryAsync(int categoryId, PagedRequest pagedRequest)
    {
        try
        {
            _logger.LogInformation("Fetching products by category {CategoryId}", categoryId);



            // Validate paged request
            if (pagedRequest == null)
            {
                return ServiceResult<PagedResult<ProductListDto>>.FailureResult("Paged request cannot be null", new List<string>());
            }

            if (pagedRequest.Page < 1)
            {
                return ServiceResult<PagedResult<ProductListDto>>.FailureResult(
                    "Page number must be greater than 0",
                    new List<string>());
            }

            if (pagedRequest.Size < 1 || pagedRequest.Size > 100)
            {
                return ServiceResult<PagedResult<ProductListDto>>.FailureResult(
                    "Page size must be between 1 and 100",
                    new List<string>());
            }

            //validate category id
            if (categoryId <= 0)
            {
                return ServiceResult<PagedResult<ProductListDto>>.FailureResult("Invalid category ID", new List<string>());
            }

            // Call repository để lấy data
            var result = await _productRepo.GetByCategoryAsync(categoryId, pagedRequest);
            if (result == null)
            {
                _logger.LogWarning("Repository returned null result for GetByCategoryAsync");
                return ServiceResult<PagedResult<ProductListDto>>.FailureResult("No data available", new List<string>());
            }

            var productListDtos = result.Items.Select(MapToListDto).ToList();

            return ServiceResult<PagedResult<ProductListDto>>.SuccessResult(new PagedResult<ProductListDto>
            {
                Items = productListDtos,
                TotalCount = result.TotalCount,
                Page = pagedRequest.Page,
                PageSize = pagedRequest.Size
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching products by category {CategoryId}", categoryId);
            return ServiceResult<PagedResult<ProductListDto>>.FailureResult("An error occurred while processing your request", new List<string> { ex.Message });
        }
    }




    #endregion

    #region validation

    private ServiceResult<T> ValidatePagedRequest<T>(PagedRequest pagedRequest)
    {
        if (pagedRequest == null)
        {
            return ServiceResult<T>.FailureResult("Paged request cannot be null", new List<string>());
        }

        if (pagedRequest.Page < 1)
        {
            return ServiceResult<T>.FailureResult("Page number must be greater than 0", new List<string>());
        }

        if (pagedRequest.Size < 1 || pagedRequest.Size > 100)
        {
            return ServiceResult<T>.FailureResult("Page size must be between 1 and 100", new List<string>());
        }

        return ServiceResult<T>.SuccessResult(default(T));
    }

    private async Task<List<string>> ValidateCreateProductDto(CreateProductDto dto)
    {
        var errors = new List<string>();

        // Basic validation
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            errors.Add("Product name is required");
        }

        if (string.IsNullOrWhiteSpace(dto.Sku))
        {
            errors.Add("SKU is required");
        }

        if (dto.BasePrice <= 0)
        {
            errors.Add("Base price must be greater than 0");
        }

        if (dto.CategoryId <= 0)
        {
            errors.Add("Valid category is required");
        }

        // Business logic validation
        if (dto.SalePrice != null && dto.SalePrice > dto.BasePrice)
        {
            errors.Add("Sale price cannot be greater than base price");
        }

        if (!string.IsNullOrWhiteSpace(dto.WarrantyType) && (!dto.WarrantyPeriod.HasValue || dto.WarrantyPeriod <= 0))
        {
            errors.Add("Warranty period is required when warranty type is specified");
        }

        if (!dto.ManageStock && dto.StockQuantity > 0)
        {
            errors.Add("Stock quantity should be 0 when not managing stock");
        }

        if (dto.HasVariants && dto.ManageStock)
        {
            errors.Add("When product has variants, stock should be managed at variant level");
        }

        // Database validation
        if (!string.IsNullOrWhiteSpace(dto.Sku))
        {
            var skuExists = await _productRepo.SlugExistsAsync(dto.Sku);
            if (skuExists)
            {
                errors.Add("SKU already exists");
            }
        }

        // Check if category exists
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId && c.IsActive);
        if (!categoryExists)
        {
            errors.Add("Selected category does not exist");
        }

        // Check if brand exists (if provided)
        if (dto.BrandId.HasValue)
        {
            var brandExists = await _context.Brands.AnyAsync(b => b.Id == dto.BrandId.Value && b.IsActive);
            if (!brandExists)
            {
                errors.Add("Selected brand does not exist");
            }
        }

        return errors;
    }

    private async Task<List<string>> ValidateUpdateProductDto(UpdateProductDto dto, int productId)
    {
        var errors = new List<string>();

        // Basic validation
        if (dto.Name != null && string.IsNullOrWhiteSpace(dto.Name))
        {
            errors.Add("Product name cannot be empty");
        }

        if (dto.Sku != null && string.IsNullOrWhiteSpace(dto.Sku))
        {
            errors.Add("SKU cannot be empty");
        }

        if (dto.BasePrice != null && dto.BasePrice <= 0)
        {
            errors.Add("Base price must be greater than 0");
        }

        if (dto.CategoryId.HasValue && dto.CategoryId <= 0)
        {
            errors.Add("Valid category is required");
        }

        // Business logic validation
        if (dto.SalePrice != null && dto.BasePrice != null && dto.SalePrice > dto.BasePrice)
        {
            errors.Add("Sale price cannot be greater than base price");
        }

        if (dto.WarrantyType != null && !string.IsNullOrWhiteSpace(dto.WarrantyType) &&
            (!dto.WarrantyPeriod.HasValue || dto.WarrantyPeriod <= 0))
        {
            errors.Add("Warranty period is required when warranty type is specified");
        }

        if (dto.ManageStock.HasValue && !dto.ManageStock.Value &&
            dto.StockQuantity.HasValue && dto.StockQuantity > 0)
        {
            errors.Add("Stock quantity should be 0 when not managing stock");
        }

        if (dto.HasVariants.HasValue && dto.HasVariants.Value &&
            dto.ManageStock.HasValue && dto.ManageStock.Value)
        {
            errors.Add("When product has variants, stock should be managed at variant level");
        }

        // Database validation
        if (dto.Sku != null)
        {
            var skuExists = await _productRepo.SlugExistsAsync(dto.Sku, productId);
            if (skuExists)
            {
                errors.Add("SKU already exists");
            }
        }

        // Check if category exists
        if (dto.CategoryId.HasValue)
        {
            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId.Value && c.IsActive);
            if (!categoryExists)
            {
                errors.Add("Selected category does not exist");
            }
        }

        // Check if brand exists (if provided)
        if (dto.BrandId.HasValue)
        {
            var brandExists = await _context.Brands.AnyAsync(b => b.Id == dto.BrandId.Value && b.IsActive);
            if (!brandExists)
            {
                errors.Add("Selected brand does not exist");
            }
        }

        return errors;
    }

    #endregion

    #region mapping

    private ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            CategoryId = product.CategoryId,
            BrandId = product.BrandId,
            Name = product.Name,
            Slug = product.Slug,
            ShortDescription = product.ShortDescription,
            Description = product.Description,
            Sku = product.Sku,
            BasePrice = product.BasePrice,
            SalePrice = product.SalePrice,
            FinalPrice = CalculateFinalPrice(product.BasePrice, product.SalePrice),
            IsOnSale = IsProductOnSale(product.BasePrice, product.SalePrice),
            DiscountPercentage = CalculateDiscountPercentage(product.BasePrice, product.SalePrice),
            Weight = product.Weight,
            Dimensions = product.Dimensions,
            Origin = product.Origin,
            WarrantyType = product.WarrantyType,
            WarrantyPeriod = product.WarrantyPeriod,
            ManageStock = product.ManageStock,
            StockQuantity = product.StockQuantity,
            StockAlertThreshold = product.StockAlertThreshold,
            AllowBackorder = product.AllowBackorder,
            IsInStock = product.StockQuantity > 0,
            VersionNumber = product.VersionNumber,
            MetaTitle = product.MetaTitle,
            MetaDescription = product.MetaDescription,
            IsActive = product.IsActive,
            IsFeatured = product.IsFeatured,
            HasVariants = product.HasVariants,
            IsNewProduct = IsNewProduct(product.CreatedAt),
            ThumbnailUrl = null, // TODO: Implement image handling
            Images = null, // TODO: Implement image handling
            Rating = 0, // TODO: Calculate from reviews
            ReviewCount = 0, // TODO: Count from reviews
            ViewCount = 0, // TODO: Implement view tracking
            Category = product.Category != null ? new CategoryDto
            {
                Id = product.Category.Id,
                Name = product.Category.Name,
                Slug = product.Category.Slug ?? GenerateSlug(product.Category.Name)
            } : new CategoryDto(),
            Brand = product.Brand != null ? new BrandDto
            {
                Id = product.Brand.Id,
                Name = product.Brand.Name,
                LogoUrl = product.Brand.LogoUrl
            } : null,
            Variants = null, // TODO: Implement variants
            Attributes = null, // TODO: Implement attributes
            Specifications = null, // TODO: Implement specifications
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }

    private ProductListDto MapToListDto(Product product)
    {
        return new ProductListDto
        {
            Id = product.Id,
            Name = product.Name,
            Slug = product.Slug,
            ShortDescription = product.ShortDescription,
            Sku = product.Sku,
            BasePrice = product.BasePrice,
            SalePrice = product.SalePrice,
            FinalPrice = CalculateFinalPrice(product.BasePrice, product.SalePrice),
            IsOnSale = IsProductOnSale(product.BasePrice, product.SalePrice),
            DiscountPercentage = CalculateDiscountPercentage(product.BasePrice, product.SalePrice),
            ThumbnailUrl = null, // TODO: Implement image handling
            IsInStock = product.StockQuantity > 0,
            StockQuantity = product.StockQuantity,
            IsActive = product.IsActive,
            IsFeatured = product.IsFeatured,
            IsNewProduct = IsNewProduct(product.CreatedAt),
            Rating = 0, // TODO: Calculate from reviews
            ReviewCount = 0, // TODO: Count from reviews
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? string.Empty,
            BrandId = product.BrandId,
            BrandName = product.Brand?.Name,
            CreatedAt = product.CreatedAt
        };
    }

    private Product MapCreateDtoToEntity(CreateProductDto dto)
    {
        return new Product
        {
            Name = dto.Name,
            Slug = GenerateSlug(dto.Name),
            CategoryId = dto.CategoryId,
            BrandId = dto.BrandId,
            ShortDescription = dto.ShortDescription,
            Description = dto.Description,
            Sku = dto.Sku,
            BasePrice = dto.BasePrice,
            SalePrice = dto.SalePrice,
            Weight = dto.Weight,
            Dimensions = dto.Dimensions,
            Origin = dto.Origin,
            WarrantyType = dto.WarrantyType,
            WarrantyPeriod = dto.WarrantyPeriod,
            ManageStock = dto.ManageStock,
            StockQuantity = dto.StockQuantity,
            StockAlertThreshold = dto.StockAlertThreshold,
            AllowBackorder = dto.AllowBackorder,
            MetaTitle = string.IsNullOrWhiteSpace(dto.MetaTitle) ? dto.Name : dto.MetaTitle,
            MetaDescription = dto.MetaDescription,
            IsActive = dto.IsActive,
            IsFeatured = dto.IsFeatured,
            HasVariants = dto.HasVariants,
            VersionNumber = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private void MapUpdateDtoToEntity(UpdateProductDto dto, Product existingProduct)
    {
        if (dto.Name != null)
        {
            existingProduct.Name = dto.Name;
            existingProduct.Slug = GenerateSlug(dto.Name);
        }

        if (dto.CategoryId.HasValue)
            existingProduct.CategoryId = dto.CategoryId.Value;

        if (dto.BrandId.HasValue)
            existingProduct.BrandId = dto.BrandId.Value;

        if (dto.Sku != null)
            existingProduct.Sku = dto.Sku;

        if (dto.ShortDescription != null)
            existingProduct.ShortDescription = dto.ShortDescription;

        if (dto.Description != null)
            existingProduct.Description = dto.Description;

        if (dto.BasePrice != null)
            existingProduct.BasePrice = dto.BasePrice;

        if (dto.SalePrice != null)
            existingProduct.SalePrice = dto.SalePrice;

        if (dto.Weight.HasValue)
            existingProduct.Weight = dto.Weight;

        if (dto.Dimensions != null)
            existingProduct.Dimensions = dto.Dimensions;

        if (dto.Origin != null)
            existingProduct.Origin = dto.Origin;

        if (dto.WarrantyType != null)
            existingProduct.WarrantyType = dto.WarrantyType;

        if (dto.WarrantyPeriod.HasValue)
            existingProduct.WarrantyPeriod = dto.WarrantyPeriod;

        if (dto.ManageStock.HasValue)
            existingProduct.ManageStock = dto.ManageStock.Value;

        if (dto.StockQuantity.HasValue)
            existingProduct.StockQuantity = dto.StockQuantity.Value;

        if (dto.StockAlertThreshold.HasValue)
            existingProduct.StockAlertThreshold = dto.StockAlertThreshold.Value;

        if (dto.AllowBackorder.HasValue)
            existingProduct.AllowBackorder = dto.AllowBackorder.Value;

        if (dto.MetaTitle != null)
            existingProduct.MetaTitle = dto.MetaTitle;

        if (dto.MetaDescription != null)
            existingProduct.MetaDescription = dto.MetaDescription;

        if (dto.IsActive.HasValue)
            existingProduct.IsActive = dto.IsActive.Value;

        if (dto.IsFeatured.HasValue)
            existingProduct.IsFeatured = dto.IsFeatured.Value;

        if (dto.HasVariants.HasValue)
            existingProduct.HasVariants = dto.HasVariants.Value;

        existingProduct.UpdatedAt = DateTime.UtcNow;
        existingProduct.VersionNumber += 1;
    }

    #endregion

    #region helper
    
        private decimal? CalculateFinalPrice(decimal? basePrice, decimal? salePrice)
    {
        if (!basePrice.HasValue) return null;
        if (!salePrice.HasValue) return basePrice;
        return salePrice < basePrice ? salePrice : basePrice;
    }

    private bool IsProductOnSale(decimal? basePrice, decimal? salePrice)
    {
        return salePrice.HasValue && basePrice.HasValue && salePrice < basePrice;
    }

    private decimal CalculateDiscountPercentage(decimal? basePrice, decimal? salePrice)
    {
        if (!IsProductOnSale(basePrice, salePrice) || !basePrice.HasValue || basePrice == 0)
            return 0;

        var discount = basePrice.Value - salePrice!.Value;
        return Math.Round((discount / basePrice.Value) * 100, 2);
    }

    private bool IsNewProduct(DateTime createdAt)
    {
        return createdAt >= DateTime.UtcNow.AddDays(-30); // Products created within last 30 days are considered new
    }

    private string GenerateSlug(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return string.Empty;

        // Convert to lowercase and replace spaces with hyphens
        var slug = name.Trim().ToLowerInvariant().Replace(" ", "-");

        // Remove invalid characters (keep only alphanumeric, hyphens, and underscores)
        slug = Regex.Replace(slug, @"[^a-z0-9\-_]", "");

        // Remove consecutive hyphens
        slug = Regex.Replace(slug, @"\-{2,}", "-");

        // Remove leading and trailing hyphens
        slug = slug.Trim('-');

        return slug;
    }
    #endregion
}
              