# Detailed bundle content search
$custom = "D:\Unity_Workshop\Endstar Custom Shader\Library\com.unity.addressables\aa\Windows\StandaloneWindows64\defaultlocalgroup_assets_all_e3621667618b9c4bab15c8fbc36a3582.bundle"

$content = [System.IO.File]::ReadAllBytes($custom)
$text = [System.Text.Encoding]::UTF8.GetString($content)

Write-Host "=== CUSTOM BUNDLE DETAILED ANALYSIS ===" -ForegroundColor Cyan
Write-Host ""

# Search for mesh names
Write-Host "Mesh references:" -ForegroundColor Yellow
@("TigerGuy", "PearlDiver", "LOD0", "LOD1", "LOD2", "SkinnedMesh") | ForEach-Object {
    if ($text -match $_) {
        Write-Host "  [FOUND] $_" -ForegroundColor Green
    } else {
        Write-Host "  [MISSING] $_" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "Shader references:" -ForegroundColor Yellow
@("Endless_Shader", "Character_NoFade", "URP", "Lit", "Standard") | ForEach-Object {
    if ($text -match $_) {
        Write-Host "  [FOUND] $_" -ForegroundColor Green
    } else {
        Write-Host "  [MISSING] $_" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "Component references:" -ForegroundColor Yellow
@("Animator", "LODGroup", "SkinnedMeshRenderer", "Avatar", "Humanoid") | ForEach-Object {
    if ($text -match $_) {
        Write-Host "  [FOUND] $_" -ForegroundColor Green
    } else {
        Write-Host "  [MISSING] $_" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "Searching for all readable strings containing 'Assets/':" -ForegroundColor Yellow
$assetPaths = [regex]::Matches($text, "Assets/[A-Za-z0-9_/\.]{5,80}")
$assetPaths | Select-Object -Unique | ForEach-Object { Write-Host "  $($_.Value)" }
