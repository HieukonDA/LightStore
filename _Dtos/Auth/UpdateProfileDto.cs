using System.ComponentModel.DataAnnotations;

namespace TheLightStore.Dtos.Auth;

public class UpdateProfileDto
{
    [StringLength(100, MinimumLength = 1, ErrorMessage = "First name must be between 1 and 100 characters.")]
    public string? FirstName { get; set; }
    
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Last name must be between 1 and 100 characters.")]
    public string? LastName { get; set; }
    
    [Phone(ErrorMessage = "Invalid phone number format.")]
    [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters.")]
    public string? Phone { get; set; }
    
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    [StringLength(200, ErrorMessage = "Email cannot exceed 200 characters.")]
    public string? Email { get; set; }
}