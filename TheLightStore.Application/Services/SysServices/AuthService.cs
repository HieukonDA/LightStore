using System;
using TheLightStore.Application.DTOs;
using TheLightStore.Application.DTOs.Auth;
using TheLightStore.Application.DTOs.Paging;
using TheLightStore.Application.Interfaces;
using TheLightStore.Domain.Commons.Models;

namespace TheLightStore.Application.Services.SysServices;

public class AuthService : IAuthService
{
    public async Task<ResponseResult> Me()
    {
        try
        {
            
        }
        catch (Exception ex)
        {
            return new ErrorResponseResult($"Failed to retrieve user info: {ex.Message}");
        }
    }

    public Task<ServiceResult<bool>> ChangePasswordAsync(ChangePasswordDto changePasswordDto)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<bool>> DeleteCustomerAsync(int customerId)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<bool>> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<List<RoleDto>>> GetAllRolesAsync()
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<UserDto>> GetCustomerByIdAsync(int customerId)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<UserRoleDetailDto>> GetCustomerRolesDetailAsync(int customerId)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<PagedResult<UserDto>>> GetCustomersAsync(PagedRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<int>> GetTotalCustomersCountAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginDto loginDto)
    {
        throw new NotImplementedException();
    }

    

    public Task<ServiceResult<bool>> ResendOtpAsync(string email)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<bool>> ResetPasswordWithOtpAsync(ResetPasswordDto resetDto)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<bool>> SendRegistrationOtpAsync(RegisterDto registerDto)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<bool>> UpdateCustomerRoleAsync(int customerId, UpdateCustomerRoleDto updateRoleDto)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<UserDto>> UpdateProfileAsync(UpdateProfileDto updateProfileDto)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<AuthResponseDto>> VerifyRegistrationOtpAsync(VerifyRegistrationOtpDto verifyDto)
    {
        throw new NotImplementedException();
    }
}
