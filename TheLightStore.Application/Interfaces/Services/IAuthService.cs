using TheLightStore.Domain.Commons.Models;
using static TheLightStore.Application.Dtos.AuthDto;

namespace TheLightStore.Application.Interfaces.Services;

public interface IAuthService
{
    Task<SuccessResponseResult> Login(LoginDto model);
    Task<SuccessResponseResult> Register(RegisterDto model);
    Task<SuccessResponseResult> ConfirmEmail(string otp, string email);
    Task<ResponseResult> Me();
    Task<ResponseResult> ChangePassword(ChangePasswordDto model);
    Task<ResponseResult> ForgotPassword(ForgotPasswordDto model);
    SuccessResponseResult VerifyOTP(VerifyOtpDto model);
    Task<ResponseResult> ResetPassword(ResetPasswordDto model);
    Task<RegisterResponse> UpdateProfile(UpdateProfileDto model);
    Task<RegisterResponse> ResendOTPRegister(string email);
    Task<SuccessResponseResult> RefreshToken(string refreshToken);
}
