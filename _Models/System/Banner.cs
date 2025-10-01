using TheLightStore.Models.Auth;

namespace TheLightStore.Models.System;

public class Banner
{
    public int Id { get; set; }
    
    public string Title { get; set; } = null!;
    
    public string? Description { get; set; }
    
    public string ImageUrl { get; set; } = null!;
    
    public string? LinkUrl { get; set; }
    
    public string Position { get; set; } = null!; // homepage, category, product, sidebar
    
    public int SortOrder { get; set; } = 0;
    
    public bool IsActive { get; set; } = true;
    
    public DateTime StartDate { get; set; }
    
    public DateTime? EndDate { get; set; }
    
    public int? CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual User? CreatedByNavigation { get; set; }
}