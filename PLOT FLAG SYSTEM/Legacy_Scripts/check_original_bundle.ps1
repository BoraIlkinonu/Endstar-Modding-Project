# Check original bundle
$original = "D:\Endstar Plot Flag\PLOT FLAG SYSTEM\felix_original_backup.bundle"
$content = [System.IO.File]::ReadAllBytes($original)
$text = [System.Text.Encoding]::UTF8.GetString($content)

Write-Host "=== ORIGINAL FELIX BUNDLE ===" -ForegroundColor Cyan

Write-Host "Mesh references:" -ForegroundColor Yellow
@("TigerGuy", "LOD0", "LOD1", "LOD2") | ForEach-Object {
    if ($text -match $_) { Write-Host "  [FOUND] $_" -ForegroundColor Green }
    else { Write-Host "  [MISSING] $_" -ForegroundColor Red }
}

Write-Host "Shader references:" -ForegroundColor Yellow
@("Endless_Shader", "Character_NoFade") | ForEach-Object {
    if ($text -match $_) { Write-Host "  [FOUND] $_" -ForegroundColor Green }
    else { Write-Host "  [MISSING] $_" -ForegroundColor Red }
}

Write-Host "Components:" -ForegroundColor Yellow
@("Animator", "LODGroup", "SkinnedMeshRenderer", "Avatar") | ForEach-Object {
    if ($text -match $_) { Write-Host "  [FOUND] $_" -ForegroundColor Green }
    else { Write-Host "  [MISSING] $_" -ForegroundColor Red }
}
