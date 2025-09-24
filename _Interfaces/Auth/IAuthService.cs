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

    //admin
    Task<ServiceResult<int>> GetTotalCustomersCountAsync(DateTime? fromDate = null, DateTime? toDate = null);
    // admin - Customer management
    Task<ServiceResult<PagedResult<UserDto>>> GetCustomersAsync(PagedRequest request); // danh sách theo page
    Task<ServiceResult<UserDto>> GetCustomerByIdAsync(int customerId); // xem chi tiết
    Task<ServiceResult<bool>> DeleteCustomerAsync(int customerId); // xóa khách hàng
}
