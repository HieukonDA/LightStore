namespace TheLightStore.Datas;
using TheLightStore.Models.Attributes;


public partial class DBContext : DbContext
{
    public DBContext(DbContextOptions<DBContext> options) : base(options) { }

    public virtual DbSet<Address> Addresses { get; set; }

    public virtual DbSet<Attribute> Attributes { get; set; }

    public virtual DbSet<AttributeValue> AttributeValues { get; set; }

    public virtual DbSet<BlogCategory> BlogCategories { get; set; }

    public virtual DbSet<BlogPost> BlogPosts { get; set; }

    public virtual DbSet<Brand> Brands { get; set; }

    public virtual DbSet<CartItem> CartItems { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Coupon> Coupons { get; set; }

    public virtual DbSet<CouponUsage> CouponUsages { get; set; }

    public virtual DbSet<GuestSession> GuestSessions { get; set; }

    public virtual DbSet<InventoryLog> InventoryLogs { get; set; }

    public virtual DbSet<InventoryReservation> InventoryReservations { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderAddress> OrderAddresses { get; set; }

    public virtual DbSet<OrderInvoice> OrderInvoices { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<OrderPayment> OrderPayments { get; set; }

    public virtual DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductAttribute> ProductAttributes { get; set; }

    public virtual DbSet<ProductImage> ProductImages { get; set; }

    public virtual DbSet<ProductReview> ProductReviews { get; set; }

    public virtual DbSet<ProductVariant> ProductVariants { get; set; }

    public virtual DbSet<ReviewHelpfulVote> ReviewHelpfulVotes { get; set; }

    public virtual DbSet<SavedCart> SavedCarts { get; set; }

    public virtual DbSet<ShippingZone> ShippingZones { get; set; }

    public virtual DbSet<ShoppingCart> ShoppingCarts { get; set; }

    public virtual DbSet<SystemSetting> SystemSettings { get; set; }

    public virtual DbSet<User> Users { get; set; }

    // onmodel creating method for fluent api configurations if any are needed in the future
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Addresse__3214EC07B3EBD8BB");

            entity.HasIndex(e => e.AddressType, "IX_Addresses_AddressType");

            entity.HasIndex(e => e.UserId, "IX_Addresses_UserId");

            entity.Property(e => e.AddressLine1).HasMaxLength(255);
            entity.Property(e => e.AddressLine2).HasMaxLength(255);
            entity.Property(e => e.AddressType)
                .HasMaxLength(50)
                .HasDefaultValue("shipping");
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.District).HasMaxLength(100);
            entity.Property(e => e.IsDefault).HasDefaultValue(false);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.PostalCode).HasMaxLength(10);
            entity.Property(e => e.Province).HasMaxLength(100);
            entity.Property(e => e.RecipientName).HasMaxLength(100);
            entity.Property(e => e.Ward).HasMaxLength(100);

            entity.HasOne(d => d.User).WithMany(p => p.Addresses)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Addresses__UserI__70DDC3D8");
        });

        modelBuilder.Entity<Attribute>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Attribut__3214EC07F2811D54");

            entity.HasIndex(e => e.IsFilterable, "IX_Attributes_IsFilterable");

            entity.HasIndex(e => e.IsVariantAttribute, "IX_Attributes_IsVariantAttribute");

            entity.HasIndex(e => e.Name, "UQ__Attribut__737584F69AF38843").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DisplayName).HasMaxLength(100);
            entity.Property(e => e.InputType).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsFilterable).HasDefaultValue(true);
            entity.Property(e => e.IsVariantAttribute).HasDefaultValue(false);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);
            entity.Property(e => e.Unit).HasMaxLength(20);
        });

        modelBuilder.Entity<AttributeValue>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Attribut__3214EC07BCADCB85");

            entity.HasIndex(e => e.AttributeId, "IX_AttributeValues_AttributeId");

            entity.HasIndex(e => new { e.AttributeId, e.Value }, "UQ_AttributeValues_AttributeValue").IsUnique();

            entity.Property(e => e.ColorCode).HasMaxLength(7);
            entity.Property(e => e.DisplayValue).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);
            entity.Property(e => e.Value).HasMaxLength(255);

            entity.HasOne(d => d.Attribute).WithMany(p => p.AttributeValues)
                .HasForeignKey(d => d.AttributeId)
                .HasConstraintName("FK__Attribute__Attri__76969D2E");
        });

        modelBuilder.Entity<BlogCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BlogCate__3214EC07C8884BA5");

            entity.HasIndex(e => e.IsActive, "IX_BlogCategories_IsActive");

            entity.HasIndex(e => e.Slug, "UQ__BlogCate__BC7B5FB615D4A314").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Slug).HasMaxLength(100);
        });

        modelBuilder.Entity<BlogPost>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BlogPost__3214EC070E3580E3");

            entity.HasIndex(e => e.BlogCategoryId, "IX_BlogPosts_Category");

            entity.HasIndex(e => e.PublishedAt, "IX_BlogPosts_PublishedAt");

            entity.HasIndex(e => e.Slug, "IX_BlogPosts_Slug");

            entity.HasIndex(e => e.Status, "IX_BlogPosts_Status");

            entity.HasIndex(e => e.Slug, "UQ__BlogPost__BC7B5FB62839E170").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FeaturedImage).HasMaxLength(500);
            entity.Property(e => e.IsFeatured).HasDefaultValue(false);
            entity.Property(e => e.MetaTitle).HasMaxLength(255);
            entity.Property(e => e.PublishedAt).HasColumnType("datetime");
            entity.Property(e => e.Slug).HasMaxLength(255);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("draft");
            entity.Property(e => e.Title).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Author).WithMany(p => p.BlogPosts)
                .HasForeignKey(d => d.AuthorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BlogPosts__Autho__0D7A0286");

            entity.HasOne(d => d.BlogCategory).WithMany(p => p.BlogPosts)
                .HasForeignKey(d => d.BlogCategoryId)
                .HasConstraintName("FK__BlogPosts__BlogC__0C85DE4D");
        });

        modelBuilder.Entity<Brand>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Brands__3214EC07C9740BC7");

            entity.HasIndex(e => e.IsActive, "IX_Brands_IsActive");

            entity.HasIndex(e => e.Name, "UQ__Brands__737584F63B2BA8CD").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LogoUrl).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CartItem__3214EC07F4CE097E");

            entity.HasIndex(e => e.CartId, "IX_CartItems_CartId");

            entity.HasIndex(e => new { e.ProductId, e.VariantId }, "IX_CartItems_ProductVariant");

            entity.HasIndex(e => new { e.CartId, e.ProductId, e.VariantId }, "UQ_CartItem").IsUnique();

            entity.Property(e => e.AddedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Cart).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.CartId)
                .HasConstraintName("FK__CartItems__CartI__1B9317B3");

            entity.HasOne(d => d.Product).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CartItems__Produ__1C873BEC");

            entity.HasOne(d => d.Variant).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.VariantId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__CartItems__Varia__1D7B6025");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Categori__3214EC07F59D81A6");

            entity.HasIndex(e => e.IsActive, "IX_Categories_IsActive");

            entity.HasIndex(e => e.ParentId, "IX_Categories_ParentId");

            entity.HasIndex(e => e.Slug, "IX_Categories_Slug");

            entity.HasIndex(e => e.Slug, "UQ__Categori__BC7B5FB6ACA58668").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Slug).HasMaxLength(100);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .HasForeignKey(d => d.ParentId)
                .HasConstraintName("FK__Categorie__Paren__4E88ABD4");
        });

        modelBuilder.Entity<Coupon>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Coupons__3214EC079C005790");

            entity.HasIndex(e => e.Code, "IX_Coupons_Code");

            entity.HasIndex(e => new { e.StartDate, e.EndDate }, "IX_Coupons_Dates");

            entity.HasIndex(e => e.IsActive, "IX_Coupons_IsActive");

            entity.HasIndex(e => e.Code, "UQ__Coupons__A25C5AA720EB865F").IsUnique();

            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DiscountType).HasMaxLength(50);
            entity.Property(e => e.DiscountValue).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MaximumDiscountAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.MinimumOrderAmount)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.UsageLimit).HasDefaultValue(0);
            entity.Property(e => e.UsageLimitPerCustomer).HasDefaultValue(1);
            entity.Property(e => e.UsedCount).HasDefaultValue(0);
        });

        modelBuilder.Entity<CouponUsage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CouponUs__3214EC076ED1E1AF");

            entity.ToTable("CouponUsage");

            entity.HasIndex(e => e.CouponId, "IX_CouponUsage_CouponId");

            entity.HasIndex(e => e.OrderId, "IX_CouponUsage_OrderId");

            entity.HasIndex(e => e.UserId, "IX_CouponUsage_UserId");

            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UsedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Coupon).WithMany(p => p.CouponUsages)
                .HasForeignKey(d => d.CouponId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CouponUsa__Coupo__7D0E9093");

            entity.HasOne(d => d.Order).WithMany(p => p.CouponUsages)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CouponUsa__Order__7E02B4CC");

            entity.HasOne(d => d.User).WithMany(p => p.CouponUsages)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__CouponUsa__UserI__7EF6D905");
        });

        modelBuilder.Entity<GuestSession>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__GuestSes__3214EC07C167A330");

            entity.HasIndex(e => e.ExpiresAt, "IX_GuestSessions_ExpiresAt");

            entity.HasIndex(e => e.GuestEmail, "IX_GuestSessions_GuestEmail");

            entity.HasIndex(e => e.LastActivity, "IX_GuestSessions_LastActivity");

            entity.Property(e => e.Id).HasMaxLength(255);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ExpiresAt).HasColumnType("datetime");
            entity.Property(e => e.GuestEmail).HasMaxLength(255);
            entity.Property(e => e.GuestName).HasMaxLength(100);
            entity.Property(e => e.GuestPhone).HasMaxLength(20);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.LastActivity).HasColumnType("datetime");
        });

        modelBuilder.Entity<InventoryLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Inventor__3214EC07D10BC26A");

            entity.HasIndex(e => e.CreatedAt, "IX_InventoryLogs_CreatedAt");

            entity.HasIndex(e => new { e.ProductId, e.VariantId }, "IX_InventoryLogs_ProductVariant");

            entity.HasIndex(e => new { e.ReferenceType, e.ReferenceId }, "IX_InventoryLogs_Reference");

            entity.Property(e => e.ChangeType).HasMaxLength(50);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Reason).HasMaxLength(100);
            entity.Property(e => e.ReferenceType).HasMaxLength(50);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.InventoryLogs)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__Inventory__Creat__16CE6296");

            entity.HasOne(d => d.Product).WithMany(p => p.InventoryLogs)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Inventory__Produ__14E61A24");

            entity.HasOne(d => d.Variant).WithMany(p => p.InventoryLogs)
                .HasForeignKey(d => d.VariantId)
                .HasConstraintName("FK__Inventory__Varia__15DA3E5D");
        });

        modelBuilder.Entity<InventoryReservation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Inventor__3214EC07EE4E9A21");

            entity.HasIndex(e => new { e.CartId, e.SessionId }, "IX_InventoryReservations_CartSession");

            entity.HasIndex(e => e.ReservedUntil, "IX_InventoryReservations_Expires");

            entity.HasIndex(e => new { e.ProductId, e.VariantId }, "IX_InventoryReservations_ProductVariant");

            entity.HasIndex(e => e.Status, "IX_InventoryReservations_Status");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ReservedUntil).HasColumnType("datetime");
            entity.Property(e => e.SessionId).HasMaxLength(255);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("active");

            entity.HasOne(d => d.Cart).WithMany(p => p.InventoryReservations)
                .HasForeignKey(d => d.CartId)
                .HasConstraintName("FK__Inventory__CartI__46B27FE2");

            entity.HasOne(d => d.Order).WithMany(p => p.InventoryReservations)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK__Inventory__Order__489AC854");

            entity.HasOne(d => d.Product).WithMany(p => p.InventoryReservations)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Inventory__Produ__44CA3770");

            entity.HasOne(d => d.Session).WithMany(p => p.InventoryReservations)
                .HasForeignKey(d => d.SessionId)
                .HasConstraintName("FK__Inventory__Sessi__498EEC8D");

            entity.HasOne(d => d.User).WithMany(p => p.InventoryReservations)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Inventory__UserI__47A6A41B");

            entity.HasOne(d => d.Variant).WithMany(p => p.InventoryReservations)
                .HasForeignKey(d => d.VariantId)
                .HasConstraintName("FK__Inventory__Varia__45BE5BA9");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Orders__3214EC07A397024A");

            entity.HasIndex(e => e.CustomerEmail, "IX_Orders_CustomerEmail");

            entity.HasIndex(e => e.OrderDate, "IX_Orders_OrderDate");

            entity.HasIndex(e => e.OrderNumber, "IX_Orders_OrderNumber");

            entity.HasIndex(e => e.OrderStatus, "IX_Orders_OrderStatus");

            entity.HasIndex(e => e.UserId, "IX_Orders_UserId");

            entity.HasIndex(e => e.OrderNumber, "UQ__Orders__CAC5E743B9C5A6E0").IsUnique();

            entity.Property(e => e.CancelledAt).HasColumnType("datetime");
            entity.Property(e => e.ConfirmedAt).HasColumnType("datetime");
            entity.Property(e => e.CustomerEmail).HasMaxLength(255);
            entity.Property(e => e.CustomerName).HasMaxLength(100);
            entity.Property(e => e.CustomerPhone).HasMaxLength(20);
            entity.Property(e => e.DeliveredAt).HasColumnType("datetime");
            entity.Property(e => e.DiscountAmount)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)");
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.OrderNumber).HasMaxLength(50);
            entity.Property(e => e.OrderStatus)
                .HasMaxLength(50)
                .HasDefaultValue("pending");
            entity.Property(e => e.ShippedAt).HasColumnType("datetime");
            entity.Property(e => e.ShippingCost)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Subtotal).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TaxAmount)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.VersionNumber).HasDefaultValue(1);

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__Orders__UserId__3F115E1A");
        });

        modelBuilder.Entity<OrderAddress>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OrderAdd__3214EC07B4E6249F");

            entity.HasIndex(e => e.AddressType, "IX_OrderAddresses_AddressType");

            entity.HasIndex(e => e.OrderId, "IX_OrderAddresses_OrderId");

            entity.HasIndex(e => new { e.OrderId, e.AddressType }, "UQ_OrderAddressType").IsUnique();

            entity.Property(e => e.AddressLine1).HasMaxLength(255);
            entity.Property(e => e.AddressLine2).HasMaxLength(255);
            entity.Property(e => e.AddressType).HasMaxLength(50);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.District).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.PostalCode).HasMaxLength(10);
            entity.Property(e => e.Province).HasMaxLength(100);
            entity.Property(e => e.RecipientName).HasMaxLength(100);
            entity.Property(e => e.Ward).HasMaxLength(100);

            entity.HasOne(d => d.Order).WithMany(p => p.OrderAddresses)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK__OrderAddr__Order__5E8A0973");
        });

        modelBuilder.Entity<OrderInvoice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OrderInv__3214EC072742356B");

            entity.HasIndex(e => e.InvoiceNumber, "IX_OrderInvoices_InvoiceNumber");

            entity.HasIndex(e => e.OrderId, "IX_OrderInvoices_OrderId");

            entity.HasIndex(e => e.OrderId, "UQ_OrderInvoice").IsUnique();

            entity.Property(e => e.CompanyName).HasMaxLength(255);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IndividualName).HasMaxLength(100);
            entity.Property(e => e.InvoiceFileUrl).HasMaxLength(500);
            entity.Property(e => e.InvoiceNumber).HasMaxLength(50);
            entity.Property(e => e.InvoiceRequired).HasDefaultValue(true);
            entity.Property(e => e.InvoiceType)
                .HasMaxLength(50)
                .HasDefaultValue("individual");
            entity.Property(e => e.IssuedAt).HasColumnType("datetime");
            entity.Property(e => e.TaxCode).HasMaxLength(20);

            entity.HasOne(d => d.Order).WithOne(p => p.OrderInvoice)
                .HasForeignKey<OrderInvoice>(d => d.OrderId)
                .HasConstraintName("FK__OrderInvo__Order__6FB49575");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OrderIte__3214EC07C206C0FF");

            entity.HasIndex(e => e.OrderId, "IX_OrderItems_OrderId");

            entity.HasIndex(e => e.ProductId, "IX_OrderItems_ProductId");

            entity.Property(e => e.ProductName).HasMaxLength(255);
            entity.Property(e => e.ProductSku).HasMaxLength(100);
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.VariantName).HasMaxLength(255);

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK__OrderItem__Order__72910220");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderItem__Produ__73852659");

            entity.HasOne(d => d.Variant).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.VariantId)
                .HasConstraintName("FK__OrderItem__Varia__74794A92");
        });

        modelBuilder.Entity<OrderPayment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OrderPay__3214EC074F53125D");

            entity.HasIndex(e => e.OrderId, "IX_OrderPayments_OrderId");

            entity.HasIndex(e => e.PaymentStatus, "IX_OrderPayments_Status");

            entity.HasIndex(e => e.TransactionId, "IX_OrderPayments_TransactionId");

            entity.Property(e => e.Amount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Currency)
                .HasMaxLength(3)
                .HasDefaultValue("VND");
            entity.Property(e => e.FailedAt).HasColumnType("datetime");
            entity.Property(e => e.PaidAt).HasColumnType("datetime");
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(50)
                .HasDefaultValue("pending");
            entity.Property(e => e.TransactionId).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderPayments)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK__OrderPaym__Order__671F4F74");
        });

        modelBuilder.Entity<OrderStatusHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OrderSta__3214EC07C5BE0FCA");

            entity.ToTable("OrderStatusHistory");

            entity.HasIndex(e => e.ChangedAt, "IX_OrderStatusHistory_ChangedAt");

            entity.HasIndex(e => e.OrderId, "IX_OrderStatusHistory_OrderId");

            entity.Property(e => e.ChangedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NewStatus).HasMaxLength(50);
            entity.Property(e => e.OldStatus).HasMaxLength(50);

            entity.HasOne(d => d.ChangedByNavigation).WithMany(p => p.OrderStatusHistories)
                .HasForeignKey(d => d.ChangedBy)
                .HasConstraintName("FK__OrderStat__Chang__793DFFAF");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderStatusHistories)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK__OrderStat__Order__7849DB76");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Products__3214EC075E0DD240");

            entity.HasIndex(e => e.BrandId, "IX_Products_Brand");

            entity.HasIndex(e => e.CategoryId, "IX_Products_Category");

            entity.HasIndex(e => e.HasVariants, "IX_Products_HasVariants");

            entity.HasIndex(e => e.IsActive, "IX_Products_IsActive");

            entity.HasIndex(e => e.IsFeatured, "IX_Products_IsFeatured");

            entity.HasIndex(e => e.Sku, "IX_Products_Sku");

            entity.HasIndex(e => e.Slug, "IX_Products_Slug");

            entity.HasIndex(e => e.Slug, "UQ__Products__BC7B5FB6CB6881F4").IsUnique();

            entity.HasIndex(e => e.Sku, "UQ__Products__CA1FD3C5CB0EBDD6").IsUnique();

            entity.Property(e => e.AllowBackorder).HasDefaultValue(false);
            entity.Property(e => e.BasePrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Dimensions).HasMaxLength(50);
            entity.Property(e => e.HasVariants).HasDefaultValue(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsFeatured).HasDefaultValue(false);
            entity.Property(e => e.ManageStock).HasDefaultValue(true);
            entity.Property(e => e.MetaTitle).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Origin).HasMaxLength(100);
            entity.Property(e => e.SalePrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Sku).HasMaxLength(100);
            entity.Property(e => e.Slug).HasMaxLength(255);
            entity.Property(e => e.StockAlertThreshold).HasDefaultValue(10);
            entity.Property(e => e.StockQuantity).HasDefaultValue(0);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.VersionNumber).HasDefaultValue(1);
            entity.Property(e => e.WarrantyType).HasMaxLength(100);
            entity.Property(e => e.Weight).HasColumnType("decimal(8, 2)");

            entity.HasOne(d => d.Brand).WithMany(p => p.Products)
                .HasForeignKey(d => d.BrandId)
                .HasConstraintName("FK__Products__BrandI__04E4BC85");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Products__Catego__03F0984C");
        });

        modelBuilder.Entity<ProductAttribute>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ProductA__3214EC07A38541C3");

            entity.HasIndex(e => e.AttributeId, "IX_ProductAttributes_AttributeId");

            entity.HasIndex(e => e.ProductId, "IX_ProductAttributes_ProductId");

            entity.HasIndex(e => new { e.ProductId, e.AttributeId }, "UQ_ProductAttribute").IsUnique();

            entity.Property(e => e.CustomValue).HasMaxLength(255);
            entity.Property(e => e.DisplayOrder).HasDefaultValue(0);

            entity.HasOne(d => d.Attribute).WithMany(p => p.ProductAttributes)
                .HasForeignKey(d => d.AttributeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ProductAt__Attri__1332DBDC");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductAttributes)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__ProductAt__Produ__123EB7A3");

            entity.HasOne(d => d.Value).WithMany(p => p.ProductAttributes)
                .HasForeignKey(d => d.ValueId)
                .HasConstraintName("FK__ProductAt__Value__14270015");
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ProductI__3214EC07368CB9F3");

            entity.HasIndex(e => e.IsPrimary, "IX_ProductImages_IsPrimary");

            entity.HasIndex(e => e.ProductId, "IX_ProductImages_ProductId");

            entity.HasIndex(e => e.VariantId, "IX_ProductImages_VariantId");

            entity.Property(e => e.AltText).HasMaxLength(255);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.IsPrimary).HasDefaultValue(false);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);

            entity.HasOne(d => d.Product).WithMany(p => p.ProductImages)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__ProductIm__Produ__57DD0BE4");

            entity.HasOne(d => d.Variant).WithMany(p => p.ProductImages)
                .HasForeignKey(d => d.VariantId)
                .HasConstraintName("FK__ProductIm__Varia__58D1301D");
        });

        modelBuilder.Entity<ProductReview>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ProductR__3214EC07D325731B");

            entity.HasIndex(e => e.ApprovedAt, "IX_ProductReviews_ApprovedAt");

            entity.HasIndex(e => e.ProductId, "IX_ProductReviews_ProductId");

            entity.HasIndex(e => e.Rating, "IX_ProductReviews_Rating");

            entity.HasIndex(e => e.Status, "IX_ProductReviews_Status");

            entity.Property(e => e.ApprovedAt).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CustomerEmail).HasMaxLength(255);
            entity.Property(e => e.CustomerName).HasMaxLength(100);
            entity.Property(e => e.HelpfulCount).HasDefaultValue(0);
            entity.Property(e => e.IsVerifiedPurchase).HasDefaultValue(false);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("pending");
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.Order).WithMany(p => p.ProductReviews)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__ProductRe__Order__09746778");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductReviews)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__ProductRe__Produ__078C1F06");

            entity.HasOne(d => d.User).WithMany(p => p.ProductReviews)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__ProductRe__UserI__0880433F");
        });

        modelBuilder.Entity<ProductVariant>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ProductV__3214EC0799B3AB15");

            entity.HasIndex(e => e.IsActive, "IX_ProductVariants_IsActive");

            entity.HasIndex(e => e.ProductId, "IX_ProductVariants_ProductId");

            entity.HasIndex(e => e.Sku, "IX_ProductVariants_Sku");

            entity.HasIndex(e => e.Sku, "UQ__ProductV__CA1FD3C5EF9BDE88").IsUnique();

            entity.Property(e => e.CostPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Dimensions).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.SalePrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Sku).HasMaxLength(100);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);
            entity.Property(e => e.StockQuantity).HasDefaultValue(0);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.VersionNumber).HasDefaultValue(1);
            entity.Property(e => e.Weight).HasColumnType("decimal(8, 2)");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductVariants)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__ProductVa__Produ__1CBC4616");
        });

        modelBuilder.Entity<ReviewHelpfulVote>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ReviewHe__3214EC070DB54F6A");

            entity.HasIndex(e => e.ReviewId, "IX_ReviewHelpfulVotes_ReviewId");

            entity.HasIndex(e => e.UserId, "IX_ReviewHelpfulVotes_UserId");

            entity.HasIndex(e => new { e.ReviewId, e.IpAddress }, "UQ_IpReviewVote").IsUnique();

            entity.HasIndex(e => new { e.ReviewId, e.UserId }, "UQ_UserReviewVote").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IpAddress).HasMaxLength(45);

            entity.HasOne(d => d.Review).WithMany(p => p.ReviewHelpfulVotes)
                .HasForeignKey(d => d.ReviewId)
                .HasConstraintName("FK__ReviewHel__Revie__0F2D40CE");

            entity.HasOne(d => d.User).WithMany(p => p.ReviewHelpfulVotes)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__ReviewHel__UserI__10216507");
        });

        modelBuilder.Entity<SavedCart>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SavedCar__3214EC07DC08C381");

            entity.HasIndex(e => e.UserId, "IX_SavedCarts_UserId");

            entity.Property(e => e.CartName)
                .HasMaxLength(100)
                .HasDefaultValue("Gi? hàng dã luu");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ItemsCount).HasDefaultValue(0);

            entity.HasOne(d => d.User).WithMany(p => p.SavedCarts)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__SavedCart__UserI__2334397B");
        });

        modelBuilder.Entity<ShippingZone>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Shipping__3214EC0753F6535E");

            entity.HasIndex(e => e.IsActive, "IX_ShippingZones_IsActive");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EstimatedDeliveryDays).HasMaxLength(20);
            entity.Property(e => e.FreeShippingThreshold)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.ShippingCost).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.SortOrder).HasDefaultValue(0);
        });

        modelBuilder.Entity<ShoppingCart>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Shopping__3214EC07C37390A8");

            entity.HasIndex(e => e.SessionId, "IX_ShoppingCarts_SessionId");

            entity.HasIndex(e => e.UpdatedAt, "IX_ShoppingCarts_UpdatedAt");

            entity.HasIndex(e => e.UserId, "IX_ShoppingCarts_UserId");

            entity.HasIndex(e => e.SessionId, "UQ_ShoppingCarts_SessionId").IsUnique();

            entity.HasIndex(e => e.UserId, "UQ_ShoppingCarts_UserId").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ItemsCount).HasDefaultValue(0);
            entity.Property(e => e.SessionId).HasMaxLength(255);
            entity.Property(e => e.Subtotal)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.User).WithOne(p => p.ShoppingCart)
                .HasForeignKey<ShoppingCart>(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__ShoppingC__UserI__245D67DE");
        });

        modelBuilder.Entity<SystemSetting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SystemSe__3214EC07DADDA628");

            entity.HasIndex(e => e.Category, "IX_SystemSettings_Category");

            entity.HasIndex(e => e.SettingKey, "IX_SystemSettings_Key");

            entity.HasIndex(e => e.IsPublic, "IX_SystemSettings_Public");

            entity.HasIndex(e => e.SettingKey, "UQ__SystemSe__01E719AD8EC996DF").IsUnique();

            entity.Property(e => e.Category)
                .HasMaxLength(50)
                .HasDefaultValue("general");
            entity.Property(e => e.IsPublic).HasDefaultValue(false);
            entity.Property(e => e.SettingKey).HasMaxLength(100);
            entity.Property(e => e.SettingType).HasMaxLength(50);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC0750CAEB05");

            entity.HasIndex(e => e.Email, "IX_Users_Email");

            entity.HasIndex(e => e.UserType, "IX_Users_UserType");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534ABC31759").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.EmailVerified).HasDefaultValue(false);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.UserType).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

}