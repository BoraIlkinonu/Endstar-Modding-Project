# Search for prefab paths inside bundles
$original = "D:\Endstar Plot Flag\PLOT FLAG SYSTEM\felix_original_backup.bundle"
$custom = "D:\Unity_Workshop\Endstar Custom Shader\Library\com.unity.addressables\aa\Windows\StandaloneWindows64\defaultlocalgroup_assets_all_e3621667618b9c4bab15c8fbc36a3582.bundle"

Write-Host "=== Searching ORIGINAL Felix bundle ===" -ForegroundColor Cyan
$content = [System.IO.File]::ReadAllBytes($original)
$text = [System.Text.Encoding]::UTF8.GetString($content)

# Search for Felix
if ($text -match "Felix") {
    Write-Host "Found 'Felix' in original bundle" -ForegroundColor Green
}

# Search for CharacterCosmetics paths
$matches = [regex]::Matches($text, "CharacterCosmetics[^\x00]{10,100}")
Write-Host "CharacterCosmetics paths found in ORIGINAL:"
$matches | Select-Object -First 5 | ForEach-Object { Write-Host "  $_" }

# Search for TigerGuy (Felix mesh name)
if ($text -match "TigerGuy") {
    Write-Host "Found 'TigerGuy' mesh reference" -ForegroundColor Green
}

Write-Host ""
Write-Host "=== Searching CUSTOM bundle ===" -ForegroundColor Cyan
$content2 = [System.IO.File]::ReadAllBytes($custom)
$text2 = [System.Text.Encoding]::UTF8.GetString($content2)

if ($text2 -match "Felix") {
    Write-Host "Found 'Felix' in custom bundle" -ForegroundColor Green
} else {
    Write-Host "NO 'Felix' found in custom bundle" -ForegroundColor Red
}

$matches2 = [regex]::Matches($text2, "CharacterCosmetics[^\x00]{10,100}")
Write-Host "CharacterCosmetics paths found in CUSTOM:"
$matches2 | Select-Object -First 5 | ForEach-Object { Write-Host "  $_" }

# Check for PearlDiver or other mesh names
if ($text2 -match "PearlDiver") {
    Write-Host "Found 'PearlDiver' in custom bundle" -ForegroundColor Green
}
if ($text2 -match "TigerGuy") {
    Write-Host "Found 'TigerGuy' in custom bundle" -ForegroundColor Green
}

# Search for internal bundle name
Write-Host ""
Write-Host "=== Internal bundle names ===" -ForegroundColor Yellow
$bundleNameMatch1 = [regex]::Match($text, "CAB-[a-f0-9]{32}")
$bundleNameMatch2 = [regex]::Match($text2, "CAB-[a-f0-9]{32}")
Write-Host "Original CAB ID: $($bundleNameMatch1.Value)"
Write-Host "Custom CAB ID: $($bundleNameMatch2.Value)"
