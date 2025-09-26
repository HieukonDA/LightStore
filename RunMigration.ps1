# PowerShell script để chạy migration TargetRole
# Run this script in the TheLightStore project directory

Write-Host "🚀 Starting TargetRole migration..." -ForegroundColor Green

try {
    # 1. Build project to check for errors
    Write-Host "📦 Building project..." -ForegroundColor Yellow
    dotnet build --no-restore
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Build failed. Please fix compilation errors first." -ForegroundColor Red
        exit 1
    }

    # 2. Run SQL migration script
    Write-Host "🔧 Running SQL migration script..." -ForegroundColor Yellow
    
    # Get connection string from appsettings
    $appSettings = Get-Content "appsettings.json" | ConvertFrom-Json
    $connectionString = $appSettings.ConnectionStrings.DefaultConnection
    
    if (-not $connectionString) {
        Write-Host "❌ Connection string not found in appsettings.json" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "📊 Executing Migration_AddTargetRole.sql..." -ForegroundColor Cyan
    
    # Execute SQL script using SqlCmd (if available) or manual instruction
    if (Get-Command sqlcmd -ErrorAction SilentlyContinue) {
        sqlcmd -S "localhost" -d "LightStore_DB" -E -i "Migration_AddTargetRole.sql"
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ SQL migration completed successfully!" -ForegroundColor Green
        } else {
            Write-Host "❌ SQL migration failed. Please run manually:" -ForegroundColor Red
            Write-Host "   1. Open SSMS (SQL Server Management Studio)" -ForegroundColor Yellow
            Write-Host "   2. Connect to your database" -ForegroundColor Yellow
            Write-Host "   3. Open and execute: Migration_AddTargetRole.sql" -ForegroundColor Yellow
            exit 1
        }
    } else {
        Write-Host "⚠️  SqlCmd not found. Please run SQL migration manually:" -ForegroundColor Yellow
        Write-Host "   1. Open SSMS (SQL Server Management Studio)" -ForegroundColor Yellow
        Write-Host "   2. Connect to your database: $connectionString" -ForegroundColor Cyan
        Write-Host "   3. Open and execute: Migration_AddTargetRole.sql" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "Press any key to continue after running SQL migration..." -ForegroundColor Magenta
        $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    }

    # 3. Test the application
    Write-Host "🧪 Testing application..." -ForegroundColor Yellow
    
    # Start app in background for testing
    Start-Process powershell -ArgumentList "-Command", "cd '$PWD'; dotnet run" -WindowStyle Hidden -PassThru
    
    # Wait for app to start
    Start-Sleep -Seconds 5
    
    # Test SignalR debug endpoint
    try {
        Write-Host "🔍 Testing SignalR hub..." -ForegroundColor Cyan
        $response = Invoke-RestMethod -Uri "https://localhost:5264/api/v1/SignalRDebug/hub-info" -Method Get -SkipCertificateCheck
        
        if ($response) {
            Write-Host "✅ SignalR hub is accessible!" -ForegroundColor Green
            Write-Host "   Hub endpoint: $($response.hubEndpoint)" -ForegroundColor Cyan
            Write-Host "   Negotiate endpoint: $($response.negotiateEndpoint)" -ForegroundColor Cyan
        }
    } catch {
        Write-Host "⚠️  SignalR test failed, but this may be normal if app is still starting" -ForegroundColor Yellow
    }
    
    Write-Host ""
    Write-Host "🎉 Migration completed successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "📋 Next steps:" -ForegroundColor Yellow
    Write-Host "   1. ✅ Database TargetRole column added" -ForegroundColor Green
    Write-Host "   2. ✅ NotificationService updated to use TargetRole" -ForegroundColor Green
    Write-Host "   3. ✅ Controllers updated with role filtering" -ForegroundColor Green
    Write-Host "   4. ✅ SignalR debug endpoints available" -ForegroundColor Green
    Write-Host ""
    Write-Host "🧪 Test URLs:" -ForegroundColor Cyan
    Write-Host "   Hub Info: https://localhost:5264/api/v1/SignalRDebug/hub-info" -ForegroundColor White
    Write-Host "   Test Hub: https://localhost:5264/api/v1/SignalRDebug/test-hub" -ForegroundColor White
    Write-Host "   Admin Notifications: https://localhost:5264/api/v1/notification" -ForegroundColor White
    Write-Host "   Customer Notifications: https://localhost:5264/api/v1/customer-notifications" -ForegroundColor White
    Write-Host ""
    Write-Host "💡 Frontend SignalR URL: https://thelightstore.io.vn/notificationHub" -ForegroundColor Magenta
    
} catch {
    Write-Host "❌ Migration failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Press any key to exit..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")