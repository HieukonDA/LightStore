namespace TheLightStore.Interfaces.Auth;

public interface IAuthService
{
    Task<ServiceResult<AuthResponseDto>> RegisterAsync(RegisterDto registerDto);
    Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginDto loginDto);

    // Task<bool> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);
    // Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
    
    // Task<AuthResponseDto> LoginWithGoogleAsync(GoogleLoginDto googleLoginDto);
}
