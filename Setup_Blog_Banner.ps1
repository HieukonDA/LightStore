# Blog & Banner Setup Script
# Chạy script này để thiết lập hoàn chỉnh chức năng Blog và Banner

Write-Host "=== LIGHTSTORE BLOG & BANNER SETUP ===" -ForegroundColor Green
Write-Host ""

# Step 1: Create migration
Write-Host "Step 1: Creating database migration..." -ForegroundColor Yellow
try {
    dotnet ef migrations add AddBlogBannerFeatures
    Write-Host "✓ Migration created successfully" -ForegroundColor Green
}
catch {
    Write-Host "✗ Failed to create migration: $_" -ForegroundColor Red
    exit 1
}

# Step 2: Update database
Write-Host ""
Write-Host "Step 2: Updating database..." -ForegroundColor Yellow
try {
    dotnet ef database update
    Write-Host "✓ Database updated successfully" -ForegroundColor Green
}
catch {
    Write-Host "✗ Failed to update database: $_" -ForegroundColor Red
    exit 1
}

# Step 3: Setup permissions
Write-Host ""
Write-Host "Step 3: Setting up permissions..." -ForegroundColor Yellow
$connectionString = (Get-Content appsettings.json | ConvertFrom-Json).ConnectionStrings.DefaultConnection

if ([string]::IsNullOrEmpty($connectionString)) {
    Write-Host "✗ Could not find connection string in appsettings.json" -ForegroundColor Red
    Write-Host "Please run the Blog_Banner_Permissions.sql script manually" -ForegroundColor Yellow
} else {
    try {
        # Execute permissions script
        sqlcmd -S "." -d "YourDatabaseName" -i "Blog_Banner_Permissions.sql" -E
        Write-Host "✓ Permissions setup completed" -ForegroundColor Green
    }
    catch {
        Write-Host "⚠ Could not execute permissions script automatically" -ForegroundColor Yellow
        Write-Host "Please run Blog_Banner_Permissions.sql manually against your database" -ForegroundColor Yellow
    }
}

# Step 4: Insert sample data
Write-Host ""
Write-Host "Step 4: Inserting sample data..." -ForegroundColor Yellow
try {
    # Execute sample data script
    sqlcmd -S "." -d "YourDatabaseName" -i "Blog_Banner_SampleData.sql" -E
    Write-Host "✓ Sample data inserted successfully" -ForegroundColor Green
}
catch {
    Write-Host "⚠ Could not execute sample data script automatically" -ForegroundColor Yellow
    Write-Host "Please run Blog_Banner_SampleData.sql manually against your database" -ForegroundColor Yellow
}

# Step 5: Build and test
Write-Host ""
Write-Host "Step 5: Building application..." -ForegroundColor Yellow
try {
    dotnet build --no-restore
    Write-Host "✓ Application built successfully" -ForegroundColor Green
}
catch {
    Write-Host "✗ Failed to build application: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "=== SETUP COMPLETED SUCCESSFULLY ===" -ForegroundColor Green
Write-Host ""
Write-Host "New API endpoints available:" -ForegroundColor Cyan
Write-Host "• Blog Management: /api/blog" -ForegroundColor White
Write-Host "• Banner Management: /api/banner" -ForegroundColor White
Write-Host ""
Write-Host "Access levels:" -ForegroundColor Cyan  
Write-Host "• Admin & Manager: Full CRUD access" -ForegroundColor White
Write-Host "• Public: Read-only access to published content" -ForegroundColor White
Write-Host ""
Write-Host "Documentation: Blog_Banner_API_Documentation.md" -ForegroundColor Yellow
Write-Host ""

# Optional: Start the application
$startApp = Read-Host "Do you want to start the application now? (y/N)"
if ($startApp -eq 'y' -or $startApp -eq 'Y') {
    Write-Host "Starting application..." -ForegroundColor Yellow
    dotnet run
}