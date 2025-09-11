namespace TheLightStore.Interfaces.Auth;

public interface IAuthService
{
    Task<ServiceResult<AuthResponseDto>> RegisterAsync(RegisterDto registerDto);
    Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginDto loginDto);

    Task<ServiceResult<bool>> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);
    Task<ServiceResult<bool>> ChangePasswordAsync(ChangePasswordDto changePasswordDto);

    Task<ServiceResult<bool>> ResetPasswordWithOtpAsync(ResetPasswordDto resetDto);
    Task<ServiceResult<bool>> ResendOtpAsync(string email);

    // Task<AuthResponseDto> LoginWithGoogleAsync(GoogleLoginDto googleLoginDto);
}
