namespace TheLightStore.Dtos.Auth;

public class OtpData
{
    public string Email { get; set; }
    public string Otp { get; set; }
    public DateTime ExpiresAt { get; set; }
    public int Attempts { get; set; } = 0;
}