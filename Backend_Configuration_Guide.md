# Backend Configuration Guide - SignalR Notifications

## 1. Cài đặt Dependencies

### Packages Required
```xml
<!-- TheLightStore.csproj -->
<PackageReference Include="Microsoft.AspNetCore.SignalR" Version="7.0.0" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="7.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="7.0.0" />
<PackageReference Include="System.Text.Json" Version="7.0.0" />
```

## 2. Program.cs Configuration

```csharp
using TheLightStore.Hubs;
using TheLightStore.Interfaces.Notifications;
using TheLightStore.Services.Notifications;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Services
builder.Services.AddControllers();
builder.Services.AddDbContext<DBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. JWT Authentication (Required for SignalR)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtKey = builder.Configuration["Jwt:Key"];
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
        
        // ✅ QUAN TRỌNG: Cấu hình JWT cho SignalR
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                
                if (!string.IsNullOrEmpty(accessToken) && 
                    path.StartsWithSegments("/notificationHub"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

// 3. CORS (Required for SignalR from different origins)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            "http://localhost:5173",      // Vite dev server
            "https://localhost:5264",     // HTTPS
            "https://yourdomain.com"      // Production
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials(); // ✅ BẮT BUỘC cho SignalR
    });
});

// 4. SignalR Service
builder.Services.AddSignalR(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        options.EnableDetailedErrors = true; // Debug mode
    }
    
    // Optional: Configure timeouts
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
    options.KeepAliveInterval = TimeSpan.FromSeconds(30);
});

// 5. Notification Services
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// 6. Other services...
builder.Services.AddAuthorization();

var app = builder.Build();

// 7. Configure Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");        // ✅ CORS phải trước Authentication
app.UseAuthentication();             // ✅ Authentication
app.UseAuthorization();              // ✅ Authorization
app.MapControllers();

// 8. ✅ QUAN TRỌNG: Map SignalR Hub
app.MapHub<NotificationHub>("/notificationHub");

app.Run();
```

## 3. Database Migration

### Tạo Migration
```bash
# Navigate to project directory
cd TheLightStore

# Add migration
dotnet ef migrations add AddNotificationsTable

# Update database
dotnet ef database update
```

### Manual SQL Script (nếu cần)
```sql
-- Chạy script từ file: Notifications_Table_Script.sql
USE [YourDatabaseName]
GO

-- Execute the complete script to create:
-- 1. Notifications table
-- 2. Indexes for performance
-- 3. Stored procedures
-- 4. Sample data
```

## 4. Environment Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=LightStore_DB;Trusted_Connection=true;TrustServerCertificate=true"
  },
  "Jwt": {
    "Key": "your-super-secret-key-minimum-32-characters-long",
    "Issuer": "LightStore",
    "Audience": "LightStore-Users",
    "ExpiryMinutes": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore.SignalR": "Debug",
      "Microsoft.AspNetCore.Http.Connections": "Debug"
    }
  }
}
```

### appsettings.Development.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=LightStore_Dev;Trusted_Connection=true;TrustServerCertificate=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore.SignalR": "Trace",
      "TheLightStore.Hubs": "Debug",
      "TheLightStore.Services.Notifications": "Debug"
    }
  }
}
```

### appsettings.Production.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=prod-server;Database=LightStore_Prod;User Id=sa;Password=YourPassword;TrustServerCertificate=true"
  },
  "Jwt": {
    "Key": "production-secret-key-very-long-and-secure",
    "Issuer": "LightStore-Prod",
    "Audience": "LightStore-Users"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore.SignalR": "Information"
    }
  }
}
```

## 5. Service Registration

### Dependency Injection Setup
```csharp
// Program.cs - Service Registration Section

// Core Services
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

// Repositories
builder.Services.AddScoped<IOrderRepo, OrderRepo>();
builder.Services.AddScoped<IUserRepo, UserRepo>();

// Hub Context is automatically registered by AddSignalR()
// IHubContext<NotificationHub> will be available for injection
```

## 6. Authentication Integration

### JWT Token Format
```json
{
  "sub": "123",           // User ID
  "email": "user@email.com",
  "name": "User Name",
  "role": ["Customer"],   // or ["Admin", "Staff"]
  "iat": 1634567890,
  "exp": 1634571490
}
```

### Role-based Authorization
```csharp
// In Controllers
[Authorize(Roles = "Admin,Staff")]     // Admin endpoints
[Authorize(Roles = "Customer")]        // Customer endpoints
[Authorize]                            // Any authenticated user

// In SignalR Hub
[Authorize(Roles = "Admin,Staff")]
public async Task JoinAdminGroup() { }

[Authorize(Roles = "Customer")]  
public async Task JoinCustomerGroup() { }
```

## 7. Error Handling & Logging

### Global Exception Handler
```csharp
// Add to Program.cs
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// In middleware pipeline
app.UseExceptionHandler();
```

### SignalR Error Handling
```csharp
// In NotificationHub.cs
public override async Task OnDisconnectedAsync(Exception? exception)
{
    if (exception != null)
    {
        _logger.LogError(exception, "SignalR connection error for {ConnectionId}", 
            Context.ConnectionId);
    }
    await base.OnDisconnectedAsync(exception);
}
```

### Notification Service Error Handling
```csharp
// In NotificationService.cs
public async Task SendNotificationAsync(...)
{
    try
    {
        // Send notification
        await _hubContext.Clients.Group("AdminGroup")
            .SendAsync("ReceiveNotification", notification);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to send notification: {Title}", notification.Title);
        
        // Don't throw - notification failure shouldn't break business logic
        // Consider fallback mechanisms (email, database flag, etc.)
    }
}
```

## 8. Performance Configuration

### SignalR Performance Settings
```csharp
builder.Services.AddSignalR(options =>
{
    // Connection timeout
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
    options.KeepAliveInterval = TimeSpan.FromSeconds(30);
    
    // Message size limits
    options.MaximumReceiveMessageSize = 32 * 1024; // 32KB
    
    // Streaming
    options.StreamingFrameCountLimit = 10;
    
    // Development only
    if (builder.Environment.IsDevelopment())
    {
        options.EnableDetailedErrors = true;
    }
});
```

### Database Performance
```csharp
// In DBContext
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder.UseSqlServer(connectionString, options =>
    {
        options.CommandTimeout(30);
        options.EnableRetryOnFailure(maxRetryCount: 3);
    });
}
```

## 9. Security Configuration

### Rate Limiting for SignalR
```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("SignalRPolicy", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User?.Identity?.Name ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100, // 100 messages per minute
                Window = TimeSpan.FromMinutes(1)
            }));
});

// Apply to Hub
[EnableRateLimiting("SignalRPolicy")]
public class NotificationHub : Hub { }
```

### Input Validation
```csharp
// In NotificationService
public async Task CreateNotificationAsync(CreateNotificationDto dto)
{
    // Validate input
    if (string.IsNullOrWhiteSpace(dto.Title))
        throw new ArgumentException("Title is required");
        
    if (dto.Title.Length > 255)
        throw new ArgumentException("Title too long");
        
    // Sanitize content
    dto.Content = HtmlEncoder.Default.Encode(dto.Content);
    
    // Continue with processing...
}
```

## 10. Testing Configuration

### Unit Test Setup
```csharp
// Test project configuration
public class NotificationServiceTests
{
    private readonly Mock<IHubContext<NotificationHub>> _hubContextMock;
    private readonly Mock<DBContext> _dbContextMock;
    private readonly NotificationService _service;
    
    public NotificationServiceTests()
    {
        _hubContextMock = new Mock<IHubContext<NotificationHub>>();
        _dbContextMock = new Mock<DBContext>();
        _service = new NotificationService(
            Mock.Of<IEmailService>(),
            _hubContextMock.Object,
            _dbContextMock.Object,
            Mock.Of<ILogger<NotificationService>>());
    }
    
    [Fact]
    public async Task Should_Send_Notification_To_AdminGroup()
    {
        // Test implementation
    }
}
```

### Integration Test
```csharp
public class SignalRIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    
    public SignalRIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }
    
    [Fact]
    public async Task Should_Connect_To_NotificationHub()
    {
        // Create test client with authentication
        // Connect to SignalR hub
        // Verify connection and message delivery
    }
}
```

## 11. Deployment Checklist

### Production Preparation
- [ ] Update connection strings
- [ ] Configure HTTPS/SSL certificates  
- [ ] Set production JWT secrets
- [ ] Enable CORS for production domains
- [ ] Configure logging levels
- [ ] Set up database indexes
- [ ] Configure rate limiting
- [ ] Test authentication flow
- [ ] Verify SignalR connectivity
- [ ] Monitor performance metrics

### Environment Variables
```bash
# Set in production environment
ASPNETCORE_ENVIRONMENT=Production
JWT_KEY=your-production-secret-key
DB_CONNECTION=production-connection-string
CORS_ORIGINS=https://yourdomain.com
```

### Health Checks
```csharp
builder.Services.AddHealthChecks()
    .AddDbContext<DBContext>()
    .AddSignalRHub("notificationHub");
    
app.MapHealthChecks("/health");
```