$propsAsm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Props.dll')

$propType = $propsAsm.GetType('Endless.Props.Assets.Prop')

Write-Host "=== Prop Type Info ===" -ForegroundColor Cyan
Write-Host "IsClass: $($propType.IsClass)"
Write-Host "IsValueType: $($propType.IsValueType)"
Write-Host "IsSealed: $($propType.IsSealed)"
Write-Host "BaseType: $($propType.BaseType.FullName)"

Write-Host "`n=== prefabBundle field ===" -ForegroundColor Cyan
$field = $propType.GetField('prefabBundle', [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
if ($field) {
    Write-Host "Type: $($field.FieldType.FullName)"
    Write-Host "IsInitOnly: $($field.IsInitOnly)"
    Write-Host "IsPrivate: $($field.IsPrivate)"
}

# Check AssetReference type
$assetsAsm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Assets.dll')
$arType = $assetsAsm.GetType('Endless.Assets.AssetReference')
Write-Host "`n=== AssetReference Type Info ===" -ForegroundColor Cyan
Write-Host "IsClass: $($arType.IsClass)"
Write-Host "IsValueType: $($arType.IsValueType)"
if ($arType.IsValueType) {
    Write-Host "*** AssetReference is a STRUCT - assignment creates COPY! ***" -ForegroundColor Red
}
