using System.ComponentModel.DataAnnotations;

namespace TheLightStore.Dtos.Auth;

public class UpdateCustomerRoleDto
{
    [Required(ErrorMessage = "Role ID is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Role ID must be a positive number.")]
    public int RoleId { get; set; }
    
    [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters.")]
    public string? Reason { get; set; } // Lý do thay đổi role
}