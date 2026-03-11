$files = Get-ChildItem "TheLightStore.Domain/Entities" -Filter "*.cs" -Recurse

$usingsToAdd = @"
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

"@

foreach ($file in $files) {
    try {
        $content = Get-Content $file.FullName -Raw
        
        # Skip if already has proper usings
        if ($content -match "using System;") {
            continue
        }
        
        # Add usings at the top
        if ($content -match "namespace") {
            $content = $usingsToAdd + $content
            Set-Content -Path $file.FullName -Value $content -NoNewline
            Write-Host "Updated: $($file.Name)" -ForegroundColor Green
        }
    }
    catch {
        Write-Warning "Error processing $($file.Name): $_"
    }
}

Write-Host "`n✅ Added using statements to Domain entities" -ForegroundColor Cyan
