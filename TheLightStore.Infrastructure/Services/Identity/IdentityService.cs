using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TheLightStore.Application.Interfaces.Services;
using TheLightStore.Application.Mappings;
using TheLightStore.Infrastructure.Persistence;
using TheLightStore.Infrastructure.Persistence.SysEntities;
using static TheLightStore.Application.Dtos.AuthDto;

namespace TheLightStore.Infrastructure.Services.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<Users> _userManager;
    private readonly DBContext _context;

    public IdentityService(
        UserManager<Users> userManager,
        DBContext context)
    {
        _userManager = userManager;
        _context = context;
    }
    
    public async Task<MeDto?> FindByNameAsync(string userName)
    {
        var user = await _userManager.FindByNameAsync(userName);
        if (user == null) return null;

        return AuthMapping.EntityToVModel(user);
    }

    public async Task<MeDto?> FindByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return null;

        return AuthMapping.EntityToVModel(user);
    }

    public async Task<MeDto?> FindByIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return null;

        return AuthMapping.EntityToVModel(user);
    }

    public async Task<bool> CheckPasswordAsync(string userId, string password)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        return await _userManager.CheckPasswordAsync(user, password);
    }

    public async Task<(bool success, string[]? errors)> CreateAsync(
        string userName,
        string password,
        string? email = null,
        string? phoneNumber = null)
    {
        var newUser = new Users
        {
            UserName = userName,
            Email = email,
            PhoneNumber = phoneNumber,
            CreatedDate = DateTime.UtcNow,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(newUser, password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToArray();
            return (false, errors);
        }

        return (true, null);
    }

    public async Task<(bool success, string[]? errors)> UpdateAsync(MeDto userDto)
    {
        var user = await _userManager.FindByIdAsync(userDto.UserId!);
        if (user == null)
            return (false, new[] { "User not found" });

        // Map DTO properties to entity
        user.Email = userDto.Email;
        user.FirstName = userDto.FirstName;
        user.LastName = userDto.LastName;
        user.IsActive = userDto.IsActive ?? true;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToArray();
            return (false, errors);
        }

        return (true, null);
    }

    public async Task<(bool success, string[]? errors)> ChangePasswordAsync(
        string userId,
        string currentPassword,
        string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return (false, new[] { "User not found" });

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToArray();
            return (false, errors);
        }

        return (true, null);
    }

    public async Task<string> GeneratePasswordResetTokenAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new InvalidOperationException("User not found");

        return await _userManager.GeneratePasswordResetTokenAsync(user);
    }

    public async Task<(bool success, string[]? errors)> ResetPasswordAsync(
        string userId,
        string token,
        string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return (false, new[] { "User not found" });

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToArray();
            return (false, errors);
        }

        return (true, null);
    }

    public async Task<string?> GetUserRoleAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return null;

        // Get role from UserRoles relationship if exists
        var userRole = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .FirstOrDefaultAsync();

        if (userRole != null)
        {
            var role = await _context.Roles
                .Where(r => r.Id == userRole)
                .Select(r => r.Name)
                .FirstOrDefaultAsync();
            return role;
        }

        return null;
    }

    public async Task<bool> UserExistsAsync(string identifier)
    {
        return await _userManager.Users.AnyAsync(u =>
            u.UserName == identifier ||
            u.Email == identifier ||
            u.PhoneNumber == identifier);
    }

    public async Task<IEnumerable<MeDto>?> GetAllUsersAsync()
    {
        var users = await _userManager.Users.ToListAsync();
        return users.Select(AuthMapping.EntityToVModel).ToList();
    }

    public async Task<long?> GetRoleByNameAsync(string roleName)
    {
        var role = await _context.Roles
            .Where(r => r.Name == roleName)
            .Select(r => r.Id)
            .FirstOrDefaultAsync();
        return role;
    }
}
