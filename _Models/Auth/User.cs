

namespace TheLightStore.Models.Auth;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string UserType { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool EmailVerified { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column(TypeName = "datetime2")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}