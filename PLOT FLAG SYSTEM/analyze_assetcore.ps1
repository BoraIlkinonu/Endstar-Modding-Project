$assetsAsm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Assets.dll')

Write-Host "=== ASSETCORE CLASS ==="
$assetCoreType = $assetsAsm.GetType('Endless.Assets.AssetCore')

if ($assetCoreType -eq $null) {
    Write-Host "AssetCore not found in Assets.dll"
    Write-Host ""
    Write-Host "Looking for all types containing 'Asset':"
    $assetsAsm.GetTypes() | Where-Object { $_.Name -like '*Asset*' } | ForEach-Object {
        Write-Host "  $($_.FullName) -> Base: $($_.BaseType.FullName)"
    }
} else {
    Write-Host "Full Name: $($assetCoreType.FullName)"
    Write-Host "Base Type: $($assetCoreType.BaseType.FullName)"

    Write-Host ""
    Write-Host "--- Base Type Chain ---"
    $t = $assetCoreType
    while ($t -ne $null) {
        Write-Host "  $($t.FullName)"
        $t = $t.BaseType
    }
}

Write-Host ""
Write-Host "=== ASSET CLASS in Assets.dll ==="
$assetType = $assetsAsm.GetType('Endless.Assets.Asset')
if ($assetType) {
    Write-Host "Full Name: $($assetType.FullName)"
    Write-Host "Base Type: $($assetType.BaseType.FullName)"
}
