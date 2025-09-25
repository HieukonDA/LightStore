# PowerShell script to generate ProductImage bulk insert data
# Reads from wwwroot/product folders and creates TXT file for database import

$outputFile = "ProductImages_BulkInsert.txt"
$productPath = "wwwroot\product"
$baseUrl = "/product"  # Base URL for images

# Initialize output content
$content = @()

# Header for the output file
$content += "-- ProductImage Bulk Insert Script"
$content += "-- Generated on: $(Get-Date)"
$content += "-- Format: ProductId, ImageUrl, AltText, IsPrimary, SortOrder, CreatedAt"
$content += ""

Write-Host "Starting to process product folders..." -ForegroundColor Green

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
            $sortOrder = 1
            
            foreach ($imageFile in $imageFiles) {
                $imageUrl = "$baseUrl/$folderName/$($imageFile.Name)"
                $altText = "$folderName image $sortOrder"
                $isPrimary = if ($sortOrder -eq 1) { 1 } else { 0 }  # First image is primary
                $createdAt = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
                
                # Format for SQL INSERT or CSV
                $line = "$productId`t$imageUrl`t$altText`t$isPrimary`t$sortOrder`t$createdAt"
                $content += $line
                
                Write-Host "  - Added: $($imageFile.Name) (Primary: $($isPrimary -eq 1))" -ForegroundColor Cyan
                $sortOrder++
            }
        } else {
            Write-Host "  - No images found in $folderName" -ForegroundColor Red
        }
    } else {
        Write-Host "  - Folder not found: $folderName" -ForegroundColor Red
    }
}

# Write to output file
$content | Out-File -FilePath $outputFile -Encoding UTF8

Write-Host "`nBulk insert file generated: $outputFile" -ForegroundColor Green
Write-Host "Total lines: $($content.Count - 4)" -ForegroundColor Green

# Display sample content
Write-Host "`nSample content:" -ForegroundColor Magenta
$content | Select-Object -First 10 | ForEach-Object { Write-Host $_ }

Write-Host "`nScript completed!" -ForegroundColor Green