namespace TheLightStore.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using TheLightStore.Domain.Entities.Customers;
using TheLightStore.Domain.Entities.Employees;
using TheLightStore.Domain.Entities.Products;
using TheLightStore.Infrastructure.Persistence.SysEntities;


public partial class DBContext : DbContext
{
    public DBContext(DbContextOptions<DBContext> options) : base(options) { }
    
    // Auth-related DbSets
    public virtual DbSet<Users> Users { get; set; }
    public virtual DbSet<SysRole> Roles { get; set; }
    public virtual DbSet<SysPermission> Permissions { get; set; }
    public virtual DbSet<SysUserRole> UserRoles { get; set; }
    public virtual DbSet<SysRolePermission> RolePermissions { get; set; }
    
    // System-related DbSets (Location, File, API)
    public virtual DbSet<SysFile> Files { get; set; }
    public virtual DbSet<SysLanguage> Languages { get; set; }
    public virtual DbSet<SysProvince> Provinces { get; set; }
    public virtual DbSet<SysWard> Wards { get; set; }
    
    // Customer-related DbSets
    public virtual DbSet<Customer> Customers { get; set; }
    public virtual DbSet<CustomerType> CustomerTypes { get; set; }
    
    // Employee-related DbSets
    public virtual DbSet<Employee> Employees { get; set; }
    
    // Product-related DbSets
    public virtual DbSet<Product> Products { get; set; }
    public virtual DbSet<ProductDetail> ProductDetails { get; set; }
    public virtual DbSet<Category> Categories { get; set; }
    public virtual DbSet<Brand> Brands { get; set; }
    public virtual DbSet<Power> Powers { get; set; }
    public virtual DbSet<ColorTemperature> ColorTemperatures { get; set; }
    public virtual DbSet<Shape> Shapes { get; set; }
    public virtual DbSet<BaseType> BaseTypes { get; set; }
    public virtual DbSet<ProductImage> ProductImages { get; set; }
    public virtual DbSet<Promotion> Promotions { get; set; }
    public virtual DbSet<ProductPromotion> ProductPromotions { get; set; }

    // onmodel creating method for fluent api configurations
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ===========================
        // Auth-related Configurations
        // ===========================
        
        // Users Configuration (extends IdentityUser)
        modelBuilder.Entity<Users>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(128);  // IdentityUser standard length
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.UserName).HasMaxLength(256);  // IdentityUser standard
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.DomainUserId).HasMaxLength(255);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.Note).HasMaxLength(1000);
            entity.Property(e => e.Birthday).HasColumnType("date");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
        });

        // SysRole Configuration
        modelBuilder.Entity<SysRole>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            entity.HasMany(r => r.UserRoles).WithOne(ur => ur.Role).HasForeignKey(ur => ur.RoleId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(r => r.RolePermissions).WithOne(rp => rp.Role).HasForeignKey(rp => rp.RoleId).OnDelete(DeleteBehavior.Cascade);
        });

        // SysPermission Configuration
        modelBuilder.Entity<SysPermission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Module).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            
            entity.HasMany(p => p.RolePermissions).WithOne(rp => rp.Permission).HasForeignKey(rp => rp.PermissionId).OnDelete(DeleteBehavior.Cascade);
        });

        // SysUserRole Configuration
        modelBuilder.Entity<SysUserRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId });
            entity.Property(e => e.UserId).HasMaxLength(128);  // Match Users.Id length
            entity.Property(e => e.AssignedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            
            entity.HasOne(ur => ur.User).WithMany().HasForeignKey(ur => ur.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(ur => ur.Role).WithMany(r => r.UserRoles).HasForeignKey(ur => ur.RoleId).OnDelete(DeleteBehavior.Cascade);
        });

        // SysRolePermission Configuration
        modelBuilder.Entity<SysRolePermission>(entity =>
        {
            entity.HasKey(e => new { e.RoleId, e.PermissionId });
            entity.Property(e => e.AssignedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            
            entity.HasOne(rp => rp.Role).WithMany(r => r.RolePermissions).HasForeignKey(rp => rp.RoleId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(rp => rp.Permission).WithMany(p => p.RolePermissions).HasForeignKey(rp => rp.PermissionId).OnDelete(DeleteBehavior.Cascade);
        });

        // ===========================
        // System-related Configurations
        // ===========================
        
        // SysFile Configuration
        modelBuilder.Entity<SysFile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Path).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
        });

        // SysLanguage Configuration
        modelBuilder.Entity<SysLanguage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.IsDefault).HasDefaultValue(false);
            entity.Property(e => e.FlagPath).HasMaxLength(500);
            
            entity.HasOne(l => l.Image).WithMany().HasForeignKey(l => l.ImageId).OnDelete(DeleteBehavior.SetNull);
        });

        // SysProvince Configuration
        modelBuilder.Entity<SysProvince>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            
            entity.HasMany(p => p.SysWards).WithOne(w => w.Province).HasForeignKey(w => w.ProvinceId).OnDelete(DeleteBehavior.Cascade);
        });

        // SysWard Configuration
        modelBuilder.Entity<SysWard>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ProvinceId).IsRequired();
            
            entity.HasOne(w => w.Province).WithMany(p => p.SysWards).HasForeignKey(w => w.ProvinceId).OnDelete(DeleteBehavior.Cascade);
        });

        // ===========================
        // Customer-related Configurations
        // ===========================
        
        // CustomerType Configuration
        modelBuilder.Entity<CustomerType>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Points).HasDefaultValue(0);
            entity.Property(e => e.PercentDiscount).HasDefaultValue(0);
        });

        // Customer Configuration
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.IdentityUserId).IsRequired().HasMaxLength(128);  // Match Users.Id length
            entity.Property(e => e.Description).HasMaxLength(500);
            
            entity.Property(e => e.TotalPoints).HasDefaultValue(0L);
            entity.Property(e => e.TotalPayment).HasColumnType("decimal(18, 2)").HasDefaultValue(0m);
            entity.Property(e => e.TotalCompletedOrder).HasDefaultValue(0L);
            entity.Property(e => e.TotalCancelOrder).HasDefaultValue(0L);
            entity.Property(e => e.TotalBuyingProduct).HasDefaultValue(0L);
            
            entity.HasOne(c => c.CustomerType).WithMany().HasForeignKey(c => c.CustomerTypeId).OnDelete(DeleteBehavior.SetNull);
        });

        // ===========================
        // Employee-related Configurations
        // ===========================
        
        // Employee Configuration
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(128);  // Match Users.Id length
            
            entity.HasOne<Users>().WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
