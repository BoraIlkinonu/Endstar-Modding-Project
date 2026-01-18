$lastSize = 0
$timeout = 60
$elapsed = 0

Write-Host "Monitoring BepInEx log for hook triggers... (timeout: ${timeout}s)" -ForegroundColor Cyan
Write-Host "Please enter Creator mode in the game!" -ForegroundColor Yellow
Write-Host ""

while ($elapsed -lt $timeout) {
    $logFile = 'C:\Endless Studios\Endless Launcher\Endstar\BepInEx\LogOutput.log'
    $currentSize = (Get-Item $logFile -ErrorAction SilentlyContinue).Length

    if ($currentSize -gt $lastSize) {
        $newContent = Get-Content $logFile -Tail 30
        $hooks = $newContent | Select-String -Pattern 'Repopulate called|PropLibrary constructed|GetAllRuntimeProps|Captured PropLibrary|Injecting|SUCCESS|Injection complete'

        if ($hooks) {
            Write-Host "=== HOOK TRIGGERS DETECTED ===" -ForegroundColor Green
            foreach ($line in $hooks) {
                Write-Host $line.Line
            }
            Write-Host ""
        }
        $lastSize = $currentSize
    }

    Start-Sleep -Seconds 2
    $elapsed += 2

    # Show progress
    if ($elapsed % 10 -eq 0) {
        Write-Host "... waiting ($elapsed/$timeout seconds)" -ForegroundColor DarkGray
    }
}

Write-Host ""
Write-Host "=== Latest log entries ===" -ForegroundColor Yellow
Get-Content $logFile -Tail 40
