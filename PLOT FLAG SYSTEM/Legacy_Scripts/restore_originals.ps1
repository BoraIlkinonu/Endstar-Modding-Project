# Restore original catalog
$backupCatalog = "C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\catalog_original_backup.json"
$catalog = "C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\catalog.json"
if (Test-Path $backupCatalog) {
    Copy-Item $backupCatalog $catalog -Force
    Write-Host "Restored original catalog" -ForegroundColor Green
}

# Restore original felix bundle
$backupBundle = "D:\Endstar Plot Flag\PLOT FLAG SYSTEM\felix_original_backup.bundle"
$dest = "C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\StandaloneWindows64\felix_assets_all_8ce5cfff23e50dfcaa729aa03940bfd7.bundle"
Copy-Item $backupBundle $dest -Force
Write-Host "Restored original felix bundle" -ForegroundColor Green

Write-Host ""
Write-Host "Files restored. Original Felix should work now." -ForegroundColor Cyan
