using Microsoft.EntityFrameworkCore;
using TheLightStore.Datas;
using TheLightStore.Interfaces.Repository;
using TheLightStore.Models.System;

namespace TheLightStore.Repositories;

public class BannerRepository : IBannerRepository
{
    private readonly DBContext _context;

    public BannerRepository(DBContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Banner>> GetBannersAsync(int pageNumber, int pageSize, string? position = null, bool? isActive = null)
    {
        var query = _context.Banners
            .Include(b => b.CreatedByNavigation)
            .AsQueryable();

        if (!string.IsNullOrEmpty(position))
        {
            query = query.Where(b => b.Position == position);
        }

        if (isActive.HasValue)
        {
            query = query.Where(b => b.IsActive == isActive.Value);
        }

        return await query
            .OrderBy(b => b.Position)
            .ThenBy(b => b.SortOrder)
            .ThenByDescending(b => b.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetBannersCountAsync(string? position = null, bool? isActive = null)
    {
        var query = _context.Banners.AsQueryable();

        if (!string.IsNullOrEmpty(position))
        {
            query = query.Where(b => b.Position == position);
        }

        if (isActive.HasValue)
        {
            query = query.Where(b => b.IsActive == isActive.Value);
        }

        return await query.CountAsync();
    }

    public async Task<Banner?> GetBannerByIdAsync(int id)
    {
        return await _context.Banners
            .Include(b => b.CreatedByNavigation)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<Banner> CreateBannerAsync(Banner banner)
    {
        banner.CreatedAt = DateTime.UtcNow;
        
        _context.Banners.Add(banner);
        await _context.SaveChangesAsync();
        
        return await GetBannerByIdAsync(banner.Id) ?? banner;
    }

    public async Task<Banner> UpdateBannerAsync(Banner banner)
    {
        banner.UpdatedAt = DateTime.UtcNow;
        
        _context.Banners.Update(banner);
        await _context.SaveChangesAsync();
        
        return await GetBannerByIdAsync(banner.Id) ?? banner;
    }

    public async Task<bool> DeleteBannerAsync(int id)
    {
        var banner = await _context.Banners.FindAsync(id);
        if (banner == null) return false;

        _context.Banners.Remove(banner);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Banner>> GetActiveBannersAsync(string? position = null)
    {
        var query = _context.Banners
            .Where(b => b.IsActive && 
                       b.StartDate <= DateTime.UtcNow &&
                       (b.EndDate == null || b.EndDate >= DateTime.UtcNow))
            .AsQueryable();

        if (!string.IsNullOrEmpty(position))
        {
            query = query.Where(b => b.Position == position);
        }

        return await query
            .OrderBy(b => b.SortOrder)
            .ToListAsync();
    }
}