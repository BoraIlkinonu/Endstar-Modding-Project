$asm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Assets.dll')
$arType = $asm.GetType('Endless.Assets.AssetReference')

Write-Host "=== AssetReference Analysis ===" -ForegroundColor Cyan
Write-Host "FullName: $($arType.FullName)"
Write-Host "IsClass: $($arType.IsClass)"
Write-Host "IsValueType: $($arType.IsValueType)"

Write-Host "`nConstructors:" -ForegroundColor Yellow
$arType.GetConstructors() | ForEach-Object {
    $params = ($_.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ', '
    Write-Host "  AssetReference($params)"
}

Write-Host "`nALL Fields:" -ForegroundColor Yellow
$arType.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance) | ForEach-Object {
    Write-Host "  $($_.FieldType.Name) $($_.Name)"
}

Write-Host "`nALL Properties:" -ForegroundColor Yellow
$arType.GetProperties([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance) | ForEach-Object {
    Write-Host "  $($_.PropertyType.Name) $($_.Name) { get: $($_.CanRead), set: $($_.CanWrite) }"
}

Write-Host "`n=== How to create valid AssetReference ===" -ForegroundColor Cyan
Write-Host "Based on constructors and fields above"
