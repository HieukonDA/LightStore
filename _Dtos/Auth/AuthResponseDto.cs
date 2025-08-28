namespace TheLightStore.Dtos.Auth;

public class AuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty; 
    public DateTime ExpireAt { get; set; } = DateTime.Now;
    public User User { get; set; } = new User();
}
