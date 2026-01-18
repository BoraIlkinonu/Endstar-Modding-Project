# Analyze new bundle
$bundle = "D:\Unity_Workshop\Endstar Custom Shader\Library\com.unity.addressables\aa\Windows\StandaloneWindows64\defaultlocalgroup_assets_all_e3621667618b9c4bab15c8fbc36a3582.bundle"

$content = [System.IO.File]::ReadAllBytes($bundle)
$text = [System.Text.Encoding]::UTF8.GetString($content)

Write-Host "=== NEW BUNDLE ANALYSIS ===" -ForegroundColor Cyan
Write-Host "Size: $((Get-Item $bundle).Length) bytes"
Write-Host ""

Write-Host "Mesh references:" -ForegroundColor Yellow
@("TigerGuy", "PearlDiver", "LOD0", "LOD1", "LOD2", "Armature") | ForEach-Object {
    if ($text -match $_) { Write-Host "  [FOUND] $_" -ForegroundColor Green }
    else { Write-Host "  [MISSING] $_" -ForegroundColor Red }
}

Write-Host ""
Write-Host "Components:" -ForegroundColor Yellow
@("SkinnedMesh", "Animator", "LODGroup", "Avatar", "m_Bones", "m_Mesh") | ForEach-Object {
    if ($text -match $_) { Write-Host "  [FOUND] $_" -ForegroundColor Green }
    else { Write-Host "  [MISSING] $_" -ForegroundColor Red }
}

Write-Host ""
Write-Host "Shader/Material:" -ForegroundColor Yellow
@("URP", "Lit", "Endless", "Material", "Shader") | ForEach-Object {
    if ($text -match $_) { Write-Host "  [FOUND] $_" -ForegroundColor Green }
    else { Write-Host "  [MISSING] $_" -ForegroundColor Red }
}
