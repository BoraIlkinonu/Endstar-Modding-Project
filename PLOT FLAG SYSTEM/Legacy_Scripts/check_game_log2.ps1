$logPath = "$env:USERPROFILE\AppData\LocalLow\Endless Studios\Endstar\Player.log"

if (Test-Path $logPath) {
    Write-Host "=== SEARCHING FOR BUNDLE/ADDRESSABLE ERRORS ===" -ForegroundColor Cyan
    $content = Get-Content $logPath

    $foundLines = @()
    foreach ($line in $content) {
        if ($line -match "Exception|Cannot load|Bundle|Addressable|Felix|CharacterCosmetic|Failed to load|missing|null reference" -and $line -notmatch "Bucket layout|Subsections") {
            $foundLines += $line
        }
    }

    Write-Host "Found $($foundLines.Count) relevant lines"
    $foundLines | Select-Object -Last 50 | ForEach-Object { Write-Host $_ }
} else {
    Write-Host "Log file not found" -ForegroundColor Red
}
