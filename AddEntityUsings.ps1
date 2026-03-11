# Add proper using statements to all Domain entities

$entityUsings = @"
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
"@

$files = Get-ChildItem "TheLightStore.Domain/Entities" -Filter "*.cs" -Recurse

foreach ($file in $files) {
    try {
        $content = Get-Content $file.FullName -Raw
        
        # Skip if already has these usings
        if ($content -match "using TheLightStore\.Domain\.Entities\.Products;") {
            continue
        }
        
        # Find where to insert (after existing usings, before namespace)
        if ($content -match "(using\s+[^;]+;[\r\n]+)+\s*(namespace\s+)") {
            # Insert before namespace
            $content = $content -replace "(namespace\s+TheLightStore\.Domain\.Entities[^;]+;)", "$entityUsings`r`n`$1"
        }
        else {
            # No existing usings, add before namespace
            $content = $content -replace "(namespace\s+TheLightStore\.Domain\.Entities[^;]+;)", "$entityUsings`r`n`$1"
        }
        
        Set-Content -Path $file.FullName -Value $content -NoNewline
        Write-Host "Added entity usings: $($file.Name)" -ForegroundColor Green
    }
    catch {
        Write-Warning "Error processing $($file.Name): $_"
    }
}

Write-Host "`n✅ Added entity usings to all Domain files" -ForegroundColor Cyan
