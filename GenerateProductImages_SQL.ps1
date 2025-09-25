# PowerShell script to generate SQL INSERT statements for ProductImages
# Reads from wwwroot/product folders and creates SQL file for database import

$outputFile = "ProductImages_SQLInsert.sql"
$productPath = "wwwroot\product"
$baseUrl = "/product"  # Base URL for images

# Initialize output content
$content = @()

# Header for the SQL file
$content += "-- ProductImages SQL INSERT Script"
$content += "-- Generated on: $(Get-Date)"
$content += "-- Total products to process: 60"
$content += ""
$content += "USE [TheLightStoreDB]"  # Change database name as needed
$content += "GO"
$content += ""

Write-Host "Generating SQL INSERT statements..." -ForegroundColor Green

$totalImages = 0

# Loop through product folders 1 to 60
for ($productId = 1; $productId -le 60; $productId++) {
    $folderName = "product$productId"
    $folderPath = Join-Path $productPath $folderName
    
    if (Test-Path $folderPath) {
        Write-Host "Processing folder: $folderName" -ForegroundColor Yellow
        
        # Get all image files in the folder
        $imageFiles = Get-ChildItem $folderPath -File | Where-Object { 
            $_.Extension -match '\.(jpg|jpeg|png|gif|webp)$' 
        } | Sort-Object Name
        
        if ($imageFiles.Count -gt 0) {
            $content += "-- Images for Product $productId"
            $sortOrder = 1
            
            foreach ($imageFile in $imageFiles) {
                $imageUrl = "$baseUrl/$folderName/$($imageFile.Name)"
                $altText = "Product $productId Image $sortOrder"
                $isPrimary = if ($sortOrder -eq 1) { 1 } else { 0 }  # First image is primary
                $createdAt = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
                
                # Generate SQL INSERT statement
                $sqlInsert = "INSERT INTO [ProductImages] ([ProductId], [ImageUrl], [AltText], [IsPrimary], [SortOrder], [CreatedAt]) VALUES ($productId, '$imageUrl', '$altText', $isPrimary, $sortOrder, '$createdAt');"
                $content += $sqlInsert
                
                Write-Host "  - Added: $($imageFile.Name) (Primary: $($isPrimary -eq 1))" -ForegroundColor Cyan
                $sortOrder++
                $totalImages++
            }
            $content += ""  # Empty line after each product
        } else {
            Write-Host "  - No images found in $folderName" -ForegroundColor Red
            $content += "-- No images found for Product $productId"
            $content += ""
        }
    } else {
        Write-Host "  - Folder not found: $folderName" -ForegroundColor Red
        $content += "-- Folder not found: $folderName"
        $content += ""
    }
}

# Add footer
$content += "-- Script completed"
$content += "-- Total images processed: $totalImages"
$content += "GO"

# Write to output file
$content | Out-File -FilePath $outputFile -Encoding UTF8

Write-Host "`nSQL INSERT file generated: $outputFile" -ForegroundColor Green
Write-Host "Total images processed: $totalImages" -ForegroundColor Green
Write-Host "Total SQL lines: $($content.Count)" -ForegroundColor Green

# Display sample content
Write-Host "`nSample SQL statements:" -ForegroundColor Magenta
$content | Where-Object { $_ -like "INSERT INTO*" } | Select-Object -First 5 | ForEach-Object { Write-Host $_ -ForegroundColor White }

Write-Host "`nScript completed!" -ForegroundColor Green
Write-Host "You can now run the SQL file: $outputFile" -ForegroundColor Yellow