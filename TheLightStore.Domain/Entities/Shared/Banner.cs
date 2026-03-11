using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TheLightStore.Domain.Entities.Auth;

using TheLightStore.Domain.Entities.Products;
using TheLightStore.Domain.Entities.Orders;
using TheLightStore.Domain.Entities.Auth;
using TheLightStore.Domain.Entities.Blogs;
using TheLightStore.Domain.Entities.Carts;
using TheLightStore.Domain.Entities.Coupons;
using TheLightStore.Domain.Entities.Notifications;
using TheLightStore.Domain.Entities.Reviews;
using TheLightStore.Domain.Entities.Shipping;
using TheLightStore.Domain.Entities.Shared;
namespace TheLightStore.Domain.Entities.Shared;

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