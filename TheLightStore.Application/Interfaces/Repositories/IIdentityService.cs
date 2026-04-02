using System;
using TheLightStore.Application.Dtos;
using TheLightStore.Domain.Commons.Models;
using static TheLightStore.Application.Dtos.AuthDto;

namespace TheLightStore.Application.Interfaces.Repositories;

public interface IIdentityService
{
    Task<MeDto?> FindByNameAsync(string userName);

    Task<MeDto?> FindByEmailAsync(string email);

    Task<MeDto?> FindByIdAsync(string userId);

    Task<bool> CheckPasswordAsync(string userId, string password);

    Task<(bool success, string[]? errors)> CreateAsync(string userName, string password, string? email = null, string? phoneNumber = null);

    Task<(bool success, string[]? errors)> UpdateAsync(MeDto user);

    Task<(bool success, string[]? errors)> ChangePasswordAsync(string userId, string currentPassword, string newPassword);

    Task<string> GeneratePasswordResetTokenAsync(string userId);

    Task<(bool success, string[]? errors)> ResetPasswordAsync(string userId, string token, string newPassword);

    Task<string?> GetUserRoleAsync(string userId);

    Task<bool> UserExistsAsync(string identifier);

    Task<IEnumerable<MeDto>?> GetAllUsersAsync();
    Task<long?> GetRoleByNameAsync(string roleName);
}
