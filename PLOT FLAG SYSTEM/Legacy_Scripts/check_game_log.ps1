$logPath = "$env:USERPROFILE\AppData\LocalLow\Endless Studios\Endstar\Player.log"

if (Test-Path $logPath) {
    Write-Host "=== GAME LOG (last 200 lines, filtered) ===" -ForegroundColor Cyan
    $content = Get-Content $logPath -Tail 200

    $patterns = @("error", "exception", "fail", "felix", "addressable", "bundle", "cosmetic", "asset", "load")

    foreach ($line in $content) {
        foreach ($pattern in $patterns) {
            if ($line -match $pattern) {
                Write-Host $line
                break
            }
        }
    }
} else {
    Write-Host "Log file not found at: $logPath" -ForegroundColor Red
}
