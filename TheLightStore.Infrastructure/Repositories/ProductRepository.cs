using System;
using Microsoft.EntityFrameworkCore;
using TheLightStore.Application.Interfaces.Repositories;
using TheLightStore.Domain.Entities.Products;
using TheLightStore.Infrastructure.Persistence;

namespace TheLightStore.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly DBContext _context;

    public ProductRepository(DBContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(long id)
    {
        return await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<bool> ExistsAsync(long id)
    {
        return await _context.Products.AnyAsync(p => p.Id == id);
    }
}
