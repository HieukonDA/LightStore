# Fix Domain Entities - Remove duplicates and add proper usings

$files = Get-ChildItem "TheLightStore.Domain/Entities" -Filter "*.cs" -Recurse

foreach ($file in $files) {
    try {
        $content = Get-Content $file.FullName -Raw
        
        # Remove duplicate using directives
        $lines = $content -split "`r?`n"
        $uniqueUsings = @{}
        $newLines = @()
        $inUsingSection = $true
        
        foreach ($line in $lines) {
            if ($line -match '^using\s+([^;]+);') {
                $usingStatement = $matches[1]
                if (-not $uniqueUsings.ContainsKey($usingStatement)) {
                    $uniqueUsings[$usingStatement] = $true
                    $newLines += $line
                }
            }
            elseif ($line -match '^namespace') {
                $inUsingSection = $false
                $newLines += $line
            }
            else {
                $newLines += $line
            }
        }
        
        $newContent = $newLines -join "`r`n"
        Set-Content -Path $file.FullName -Value $newContent -NoNewline
        Write-Host "Cleaned: $($file.Name)" -ForegroundColor Green
    }
    catch {
        Write-Warning "Error processing $($file.Name): $_"
    }
}

Write-Host "`n✅ Cleaned all Domain entities" -ForegroundColor Cyan
