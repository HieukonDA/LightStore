
using Microsoft.AspNetCore.Http.Features;

using TheLightStore.Interfaces.Inventory;
using TheLightStore.Interfaces.Notification;
using TheLightStore.Interfaces.Notifications;
using TheLightStore.Interfaces.Orders;
using TheLightStore.Interfaces.Payment;
using TheLightStore.Interfaces.Repository;
using TheLightStore.Interfaces.Search;
using TheLightStore.Repositories.Orders;
using TheLightStore.Repositories.Payment;
using TheLightStore.Services.Auth;
using TheLightStore.Services.BackgroundJobs;
using TheLightStore.Services.Inventory;
using TheLightStore.Services.Notifications;
using TheLightStore.Services.Orders;
using TheLightStore.Services.Payment;
using TheLightStore.Services.Search;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Database
builder.Services.AddDbContext<DBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT Authentication  
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key not found");
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("AuthPolicy", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(15),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            }));
});

// Thêm CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins(
                "http://localhost:5173",      // Vite frontend
                "https://localhost:5264",    // Swagger UI (https)
                "http://localhost:5264",      // Swagger UI (http)
                "https://thelightstore.io.vn"
            ) // domain frontend
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); 
        });
});

// Authorization & Identity
builder.Services.AddAuthorization();
builder.Services.AddIdentityCore<IdentityUser>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<DBContext>()
.AddDefaultTokenProviders();
builder.Services.AddHttpContextAccessor();

//session
builder.Services.AddDistributedMemoryCache(); // cần cho session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // thời gian hết hạn session
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // bắt buộc cho GDPR
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
});

//connect momo api
builder.Services.Configure<MomoConfig>(builder.Configuration.GetSection("MomoAPI"));
builder.Services.AddHttpClient();
builder.Services.AddScoped<IMomoService, MomoService>();


builder.Services.AddHostedService<InventoryCleanupJob>();

builder.Services.AddScoped<IInventoryService, InventoryService>();
// builder.Services.AddScoped<IStockService, StockService>();

builder.Services.AddHostedService<InventoryCleanupJob>();

builder.Services.AddScoped<IInventoryLogRepo, InventoryLogRepo>();

builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderRepo, OrderRepo>();
builder.Services.AddScoped<IOrderItemRepo, OrderItemRepo>();
builder.Services.AddScoped<IOrderStatusHistoryRepo, OrderStatusHistoryRepo>();

builder.Services.AddScoped<IPaymentRepo, PaymentRepo>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddScoped<ISearchService, SearchService>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Memory Cache for RBAC
builder.Services.AddMemoryCache();

// DI Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserRepo, UserRepo>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICategoryRepo, CategoryRepo>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductRepo, ProductRepo>();
builder.Services.AddScoped<IProductVariantRepo, ProductVariantRepo>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IRbacService, RbacService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<ICartItemService, CartItemService>();
builder.Services.AddScoped<ICartSaveService, CartSavedService>();
builder.Services.AddScoped<ICartValidationService, CartValidationService>();
builder.Services.AddScoped<ICartItemRepo, CartItemRepo>();
builder.Services.AddScoped<ISavedCartRepo, SavedCartRepo>();
builder.Services.AddScoped<IShoppingCartRepo, ShoppingCartRepo>();

// Inventory reservation
builder.Services.AddScoped<IInventoryReservationRepo, InventoryReservationRepo>();

// Address service
builder.Services.AddScoped<IAddressService, AddressService>();


builder.Services.AddSingleton<IpRateLimitService>();

// Configure file upload size
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 5 * 1024 * 1024; // 5MB
});

var app = builder.Build();

// Middleware Pipeline - ĐÚY LÀ PHẦN QUAN TRỌNG: SẮP XẾP ĐÚNG THỨ TỰ
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.UseHttpsRedirection();  // 1. HTTPS redirect trước
app.UseCors("AllowFrontend");

// Add request logging middleware for MoMo debugging


app.UseSession();           // 4. Session
app.UseAuthentication();    // 2. Authentication
app.UseAuthorization();     // 3. Authorization  

app.UseRateLimiter();
app.MapControllers();       // 5. Map controllers
// Enable static files
app.UseStaticFiles();
// app.MapIdentityApi<IdentityUser>(); // 6. Map Identity API

app.Run();