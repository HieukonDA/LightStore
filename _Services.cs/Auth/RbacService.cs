using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace TheLightStore.Services.Auth;

public class RbacService : IRbacService
{
    private readonly DBContext _context;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(15);

    public RbacService(DBContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<bool> HasPermissionAsync(int userId, string permission)
    {
        var cacheKey = $"user_permissions_{userId}";

        if (!_cache.TryGetValue(cacheKey, out HashSet<string> userPermissions))
        {
            userPermissions = (await GetUserPermissionsInternalAsync(userId)).ToHashSet();
            _cache.Set(cacheKey, userPermissions, _cacheExpiration);
        }

        return userPermissions.Contains(permission);
    }

    public async Task<bool> HasRoleAsync(int userId, string roleName)
    {
        var cacheKey = $"user_roles_{userId}";

        if (!_cache.TryGetValue(cacheKey, out HashSet<string> userRoles))
        {
            var roles = await GetUserRolesInternalAsync(userId);
            userRoles = roles.ToHashSet();
            _cache.Set(cacheKey, userRoles, _cacheExpiration);
        }

        return userRoles.Contains(roleName);
    }

    public async Task<IEnumerable<string>> GetUserPermissionsAsync(int userId)
    {
        return await GetUserPermissionsInternalAsync(userId);
    }

    public async Task<IEnumerable<string>> GetUserRolesAsync(int userId)
    {
        return await GetUserRolesInternalAsync(userId);
    }

    public async Task<bool> AssignRoleToUserAsync(int userId, int roleId)
    {
        try
        {
            var existingUserRole = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

            if (existingUserRole != null)
            {
                if (!existingUserRole.IsActive)
                {
                    existingUserRole.IsActive = true;
                    existingUserRole.AssignedAt = DateTime.UtcNow;
                }
                return true;
            }

            var userRole = new UserRole
            {
                UserId = userId,
                RoleId = roleId,
                AssignedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();

            // Clear cache
            InvalidateUserCache(userId);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> RemoveRoleFromUserAsync(int userId, int roleId)
    {
        try
        {
            var userRole = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

            if (userRole != null)
            {
                userRole.IsActive = false;
                await _context.SaveChangesAsync();

                // Clear cache
                InvalidateUserCache(userId);

                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }
    
    // ================= Internal helper methods =================

    private async Task<IEnumerable<string>> GetUserRolesInternalAsync(int userId)
    {
        return await _context.UserRoles
            .Where(ur => ur.UserId == userId && ur.IsActive)
            .Include(ur => ur.Role)
            .Select(ur => ur.Role.Name)
            .ToListAsync();
    }

    private async Task<IEnumerable<string>> GetUserPermissionsInternalAsync(int userId)
    {
        return await _context.UserRoles
            .Where(ur => ur.UserId == userId && ur.IsActive)
            .Include(ur => ur.Role)
            .ThenInclude(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .SelectMany(ur => ur.Role.RolePermissions
                .Where(rp => rp.IsActive)
                .Select(rp => rp.Permission.Name))
            .Distinct()
            .ToListAsync();
    }

    private void InvalidateUserCache(int userId)
    {
        _cache.Remove($"user_roles_{userId}");
        _cache.Remove($"user_permissions_{userId}");
    }
}