# Compare original Felix bundle vs our custom bundle
Write-Host "=== ORIGINAL FELIX BUNDLE ===" -ForegroundColor Cyan
$original = "D:\Endstar Plot Flag\PLOT FLAG SYSTEM\felix_original_backup.bundle"
$custom = "D:\Unity_Workshop\Endstar Custom Shader\Library\com.unity.addressables\aa\Windows\StandaloneWindows64\defaultlocalgroup_assets_all_e3621667618b9c4bab15c8fbc36a3582.bundle"

Write-Host "Original: $(Get-Item $original | Select-Object -ExpandProperty Length) bytes"
Write-Host "Custom:   $(Get-Item $custom | Select-Object -ExpandProperty Length) bytes"

Write-Host ""
Write-Host "First 100 bytes of ORIGINAL:" -ForegroundColor Yellow
$bytes = [System.IO.File]::ReadAllBytes($original)
[System.Text.Encoding]::ASCII.GetString($bytes[0..200]) -replace '[^\x20-\x7E]', '.'

Write-Host ""
Write-Host "First 100 bytes of CUSTOM:" -ForegroundColor Yellow
$bytes2 = [System.IO.File]::ReadAllBytes($custom)
[System.Text.Encoding]::ASCII.GetString($bytes2[0..200]) -replace '[^\x20-\x7E]', '.'
