namespace TheLightStore.Dtos.Auth;

public class OtpData
{
    public string Email { get; set; }
    public string Otp { get; set; }
    public DateTime ExpiresAt { get; set; }
    public int Attempts { get; set; } = 0;
}

public class PendingRegistration
{
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Phone { get; set; }
    public string PasswordHash { get; set; }
    public string Otp { get; set; }
    public DateTime ExpiresAt { get; set; }
    public int Attempts { get; set; }
}


/// <summary>
/// DTO để xác thực OTP trong quá trình đăng ký tài khoản
/// </summary>
public class VerifyRegistrationOtpDto
{
    /// <summary>
    /// Email đã dùng để đăng ký và nhận OTP
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// Mã OTP 6 số được gửi qua email
    /// </summary>
    public string Otp { get; set; } = null!;
}