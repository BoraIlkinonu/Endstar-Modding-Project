$assetsAsm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Assets.dll')
$propsAsm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Props.dll')

Write-Host "=== ASSET CLASS FIELDS ==="
$assetType = $assetsAsm.GetType('Endless.Assets.Asset')
$fields = $assetType.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
foreach ($f in $fields) {
    Write-Host "  $($f.FieldType.FullName) $($f.Name)"
}

Write-Host ""
Write-Host "=== ASSETCORE CLASS FIELDS ==="
$assetCoreType = $assetsAsm.GetType('Endless.Assets.AssetCore')
$fields2 = $assetCoreType.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
foreach ($f in $fields2) {
    Write-Host "  $($f.FieldType.FullName) $($f.Name)"
}

Write-Host ""
Write-Host "=== PROP CLASS FIELDS (declared only) ==="
$propType = $propsAsm.GetType('Endless.Props.Assets.Prop')
$fields3 = $propType.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::DeclaredOnly)
foreach ($f in $fields3) {
    Write-Host "  $($f.FieldType.FullName) $($f.Name)"
}
