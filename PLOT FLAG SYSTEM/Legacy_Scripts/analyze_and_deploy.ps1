# Analyze new bundle
$bundle = "D:\Unity_Workshop\Endstar Custom Shader\Library\com.unity.addressables\aa\Windows\StandaloneWindows64\defaultlocalgroup_assets_all_89aecf0d389953239779b1e21b10bd51.bundle"

$content = [System.IO.File]::ReadAllBytes($bundle)
$text = [System.Text.Encoding]::UTF8.GetString($content)

Write-Host "=== NEW BUNDLE ANALYSIS ===" -ForegroundColor Cyan
Write-Host "Size: $((Get-Item $bundle).Length) bytes"
Write-Host ""

Write-Host "Mesh references:" -ForegroundColor Yellow
@("TigerGuy", "LOD0", "LOD1", "LOD2") | ForEach-Object {
    if ($text -match $_) { Write-Host "  [FOUND] $_" -ForegroundColor Green }
    else { Write-Host "  [MISSING] $_" -ForegroundColor Red }
}

Write-Host ""
Write-Host "Components:" -ForegroundColor Yellow
@("SkinnedMesh", "m_Bones", "m_Mesh", "Armature") | ForEach-Object {
    if ($text -match $_) { Write-Host "  [FOUND] $_" -ForegroundColor Green }
    else { Write-Host "  [MISSING] $_" -ForegroundColor Red }
}

Write-Host ""
Write-Host "=== DEPLOYING TO GAME ===" -ForegroundColor Cyan

# Copy bundle
$dest = "C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\StandaloneWindows64\felix_assets_all_8ce5cfff23e50dfcaa729aa03940bfd7.bundle"
Copy-Item -Path $bundle -Destination $dest -Force

$file = Get-Item $dest
Write-Host "Bundle replaced!" -ForegroundColor Green
Write-Host "Size: $($file.Length) bytes"
Write-Host "Modified: $($file.LastWriteTime)"

Write-Host ""
Write-Host "=== LAUNCHING GAME ===" -ForegroundColor Cyan
Start-Process "C:\Endless Studios\Endless Launcher\Endstar\Endstar.exe"
Write-Host "Endstar launched! Go test Felix character." -ForegroundColor Green
