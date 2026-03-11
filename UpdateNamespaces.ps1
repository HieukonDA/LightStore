$files = Get-ChildItem -Path @('TheLightStore.Domain','TheLightStore.Application','TheLightStore.Infrastructure','TheLightStore.WebAPI') -Filter '*.cs' -Recurse -ErrorAction SilentlyContinue
$count = 0
foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    $original = $content
    
    # Update namespaces
    $content = $content -replace 'namespace TheLightStore\.Models\.Product', 'namespace TheLightStore.Domain.Entities.Products'
    $content = $content -replace 'namespace TheLightStore\.Models\.Orders_Carts', 'namespace TheLightStore.Domain.Entities.Orders'
    $content = $content -replace 'namespace TheLightStore\.Models\.Auth', 'namespace TheLightStore.Domain.Entities.Auth'
    $content = $content -replace 'namespace TheLightStore\.Models\.Blogs', 'namespace TheLightStore.Domain.Entities.Blogs'
    $content = $content -replace 'namespace TheLightStore\.Models\.Coupons_Discounts', 'namespace TheLightStore.Domain.Entities.Coupons'
    $content = $content -replace 'namespace TheLightStore\.Models\.ProductReviews', 'namespace TheLightStore.Domain.Entities.Reviews'
    $content = $content -replace 'namespace TheLightStore\.Models\.Notifications', 'namespace TheLightStore.Domain.Entities.Notifications'
    $content = $content -replace 'namespace TheLightStore\.Models\.Shipping', 'namespace TheLightStore.Domain.Entities.Shipping'
    $content = $content -replace 'namespace TheLightStore\.Models\.Category', 'namespace TheLightStore.Domain.Entities.Shared'
    $content = $content -replace 'namespace TheLightStore\.Models\.Attribute', 'namespace TheLightStore.Domain.Entities.Shared'
    $content = $content -replace 'namespace TheLightStore\.Models\.System', 'namespace TheLightStore.Domain.Entities.Shared'
    $content = $content -replace 'namespace TheLightStore\.Models\.Inventories', 'namespace TheLightStore.Domain.Entities.Shared'
    $content = $content -replace 'namespace TheLightStore\.Models\.RateLimitModels', 'namespace TheLightStore.Domain.Entities.Shared'
    $content = $content -replace 'namespace TheLightStore\.Models\.Momo', 'namespace TheLightStore.Application.DTOs.Momo'
    $content = $content -replace 'namespace TheLightStore\.Models', 'namespace TheLightStore.Domain.Entities'
    
    $content = $content -replace 'namespace TheLightStore\.Dtos\.Product', 'namespace TheLightStore.Application.DTOs.Products'
    $content = $content -replace 'namespace TheLightStore\.Dtos\.Orders', 'namespace TheLightStore.Application.DTOs.Orders'
    $content = $content -replace 'namespace TheLightStore\.Dtos\.Auth', 'namespace TheLightStore.Application.DTOs.Auth'
    $content = $content -replace 'namespace TheLightStore\.Dtos', 'namespace TheLightStore.Application.DTOs'
    
    $content = $content -replace 'namespace TheLightStore\.Services\.Orders', 'namespace TheLightStore.Application.Services'
    $content = $content -replace 'namespace TheLightStore\.Services\.Product', 'namespace TheLightStore.Application.Services'
    $content = $content -replace 'namespace TheLightStore\.Services\.cs', 'namespace TheLightStore.Application.Services'
    $content = $content -replace 'namespace TheLightStore\.Services', 'namespace TheLightStore.Application.Services'
    
    $content = $content -replace 'namespace TheLightStore\.Repositories', 'namespace TheLightStore.Infrastructure.Repositories'
    $content = $content -replace 'namespace TheLightStore\.Datas', 'namespace TheLightStore.Infrastructure.Persistence'
    $content = $content -replace 'namespace TheLightStore\.Hubs', 'namespace TheLightStore.Infrastructure.SignalR'
    
    $content = $content -replace 'namespace TheLightStore\.Interfaces', 'namespace TheLightStore.Application.Interfaces'
    
    if ($content -ne $original) {
        Set-Content -Path $file.FullName -Value $content -NoNewline
        $count++
        Write-Host "Updated: $($file.Name)" -ForegroundColor Green
    }
}
Write-Host "Total updated: $count files" -ForegroundColor Cyan
