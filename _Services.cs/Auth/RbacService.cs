using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace TheLightStore.Services.Auth;

public class RbacService : IRbacService
{
    private readonly DBContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<RbacService> _logger;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(15);

    public RbacService(DBContext context, IMemoryCache cache, ILogger<RbacService> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
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

    public async Task<bool> UpdateUserRoleAsync(int userId, int roleId)
    {
        _logger.LogInformation("Starting role update - UserId: {UserId}, NewRoleId: {RoleId}", userId, roleId);
        
        try
        {
            // Kiểm tra role có tồn tại không
            var roleExists = await _context.Roles.AnyAsync(r => r.Id == roleId);
            if (!roleExists)
            {
                _logger.LogError("Role not found - RoleId: {RoleId}", roleId);
                return false;
            }

            // Kiểm tra user có tồn tại không
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                _logger.LogError("User not found - UserId: {UserId}", userId);
                return false;
            }

            // Deactivate all current roles for this user
            var currentRoles = await _context.UserRoles
                .Where(ur => ur.UserId == userId && ur.IsActive)
                .ToListAsync();

            _logger.LogInformation("Found {Count} active roles to deactivate for user {UserId}", currentRoles.Count, userId);

            foreach (var role in currentRoles)
            {
                role.IsActive = false;
                _logger.LogDebug("Deactivating role {RoleId} for user {UserId}", role.RoleId, userId);
            }

            // Assign the new role
            var existingUserRole = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

            if (existingUserRole != null)
            {
                existingUserRole.IsActive = true;
                existingUserRole.AssignedAt = DateTime.UtcNow;
                _logger.LogInformation("Reactivating existing role {RoleId} for user {UserId}", roleId, userId);
            }
            else
            {
                var userRole = new UserRole
                {
                    UserId = userId,
                    RoleId = roleId,
                    AssignedAt = DateTime.UtcNow,
                    IsActive = true
                };
                _context.UserRoles.Add(userRole);
                _logger.LogInformation("Creating new role assignment {RoleId} for user {UserId}", roleId, userId);
            }

            var saveResult = await _context.SaveChangesAsync();
            _logger.LogInformation("SaveChanges result: {SaveResult} records affected for user {UserId}", saveResult, userId);

            // Clear cache
            InvalidateUserCache(userId);
            
            _logger.LogInformation("Role update completed successfully - UserId: {UserId}, NewRoleId: {RoleId}", userId, roleId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update user role - UserId: {UserId}, RoleId: {RoleId}", userId, roleId);
            return false;
        }
    }
    
    // ================= Internal helper methods =================

    private async Task<IEnumerable<string>> GetUserRolesInternalAsync(int userId)
    {
        var userRoles = await _context.UserRoles
            .Where(ur => ur.UserId == userId && ur.IsActive)
            .Include(ur => ur.Role)
            .ToListAsync();

        _logger.LogDebug("GetUserRoles - UserId: {UserId}, Found {Count} active roles: {Roles}", 
            userId, 
            userRoles.Count, 
            string.Join(", ", userRoles.Select(ur => $"ID={ur.RoleId}:Name={ur.Role.Name}")));

        return userRoles.Select(ur => ur.Role.Name).ToList();
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