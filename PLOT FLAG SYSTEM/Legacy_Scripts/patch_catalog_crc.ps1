# Patch game catalog to accept our bundle's CRC

$catalogPath = "C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\catalog.json"
$backupPath = "C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\catalog.json.backup_original"

# Backup original if not already backed up
if (-not (Test-Path $backupPath)) {
    Copy-Item $catalogPath $backupPath
    Write-Host "Created backup: $backupPath" -ForegroundColor Green
}

# Read catalog
$catalog = Get-Content $catalogPath -Raw

Write-Host "=== PATCHING CRC ===" -ForegroundColor Cyan

# The CRC values are stored in m_ExtraDataString as base64 encoded data
# We need to find and replace the felix bundle's CRC

# Old CRC (from error): 3bdbe6ba (decimal: 1004422842)
# New CRC (calculated): 2ff5dd23 (decimal: 804019491)

# The CRC is stored as decimal in the JSON
# Let's search for the felix bundle entry and its CRC

# Felix bundle hash from filename: 8ce5cfff23e50dfcaa729aa03940bfd7
$felixHash = "8ce5cfff23e50dfcaa729aa03940bfd7"

Write-Host "Looking for felix bundle reference: $felixHash"

if ($catalog -match $felixHash) {
    Write-Host "Found felix bundle reference in catalog" -ForegroundColor Green
} else {
    Write-Host "Felix bundle hash not found in catalog!" -ForegroundColor Red
}

# The CRC is in m_Crc field. Looking for pattern like "m_Crc":XXXXXXX
# Old CRC hex 3bdbe6ba = decimal 1004422842
# But the actual CRC might be different. Let's check what CRC our bundle has.

# Calculate our bundle's CRC
$bundlePath = "C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\StandaloneWindows64\felix_assets_all_8ce5cfff23e50dfcaa729aa03940bfd7.bundle"
$bytes = [System.IO.File]::ReadAllBytes($bundlePath)

# Unity uses CRC32
# The error says "calculated 2ff5dd23" which is hex for our bundle's CRC

# Convert hex to decimal
$ourCrcHex = "2ff5dd23"
$ourCrcDec = [Convert]::ToUInt32($ourCrcHex, 16)
Write-Host "Our bundle CRC: 0x$ourCrcHex = $ourCrcDec"

# The original CRC from error
$origCrcHex = "3bdbe6ba"
$origCrcDec = [Convert]::ToUInt32($origCrcHex, 16)
Write-Host "Original expected CRC: 0x$origCrcHex = $origCrcDec"

# The m_ExtraDataString contains base64 encoded bundle options including CRC
# Let's try to decode and modify it

# Actually, let's try a simpler approach - the CRC might be in plain in the JSON
# Looking for patterns like "m_Crc":1004422842 or similar decimal values

# Search for the original CRC decimal value
if ($catalog -match [regex]::Escape($origCrcDec.ToString())) {
    Write-Host "Found original CRC as decimal in catalog" -ForegroundColor Green
    $catalog = $catalog -replace [regex]::Escape($origCrcDec.ToString()), $ourCrcDec.ToString()
    Write-Host "Replaced CRC: $origCrcDec -> $ourCrcDec" -ForegroundColor Green
} else {
    Write-Host "Original CRC decimal not found directly in catalog" -ForegroundColor Yellow
    Write-Host "CRC is likely in base64 encoded m_ExtraDataString" -ForegroundColor Yellow
}

# Write modified catalog
Set-Content $catalogPath $catalog -NoNewline

Write-Host ""
Write-Host "Catalog patched. Restart the game to test." -ForegroundColor Cyan
