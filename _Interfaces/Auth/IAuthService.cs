namespace TheLightStore.Interfaces.Auth;

public interface IAuthService
{
    // Đăng nhập
    Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginDto loginDto);
    
    // Đăng ký 2 bước: gửi OTP + xác thực OTP
    Task<ServiceResult<bool>> SendRegistrationOtpAsync(RegisterDto registerDto);
    Task<ServiceResult<AuthResponseDto>> VerifyRegistrationOtpAsync(VerifyRegistrationOtpDto verifyDto);

    // Quên mật khẩu
    Task<ServiceResult<bool>> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);
    Task<ServiceResult<bool>> ResetPasswordWithOtpAsync(ResetPasswordDto resetDto);
    Task<ServiceResult<bool>> ResendOtpAsync(string email);
    
    // Đổi mật khẩu (đã đăng nhập)
    Task<ServiceResult<bool>> ChangePasswordAsync(ChangePasswordDto changePasswordDto);

    // Task<AuthResponseDto> LoginWithGoogleAsync(GoogleLoginDto googleLoginDto);

    //admin
    Task<ServiceResult<int>> GetTotalCustomersCountAsync(DateTime? fromDate = null, DateTime? toDate = null);
    // admin - Customer management
    Task<ServiceResult<PagedResult<UserDto>>> GetCustomersAsync(PagedRequest request); // danh sách theo page
    Task<ServiceResult<UserDto>> GetCustomerByIdAsync(int customerId); // xem chi tiết
    Task<ServiceResult<bool>> DeleteCustomerAsync(int customerId); // xóa khách hàng
}
