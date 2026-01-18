# Research UIRuntimePropInfoListModel and its base class for refresh methods
$asm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Creator.dll')

$listModelType = $asm.GetType('Endless.Creator.UI.UIRuntimePropInfoListModel')
Write-Host "=== UIRuntimePropInfoListModel ==="
Write-Host "Base type: $($listModelType.BaseType.FullName)"
$listModelType.GetMethods([System.Reflection.BindingFlags]'Public,NonPublic,Instance,DeclaredOnly') | ForEach-Object {
    Write-Host "  $($_.Name)($($_.GetParameters() | ForEach-Object { $_.ParameterType.Name } | Join-String -Separator ', '))"
}

# Check base class methods
Write-Host ""
Write-Host "=== Base class methods ==="
$baseType = $listModelType.BaseType
$baseType.GetMethods([System.Reflection.BindingFlags]'Public,NonPublic,Instance,DeclaredOnly') | ForEach-Object {
    Write-Host "  $($_.Name)($($_.GetParameters() | ForEach-Object { $_.ParameterType.Name } | Join-String -Separator ', '))"
}

# Check for events that might trigger refresh
Write-Host ""
Write-Host "=== Events/Fields ==="
$listModelType.GetFields([System.Reflection.BindingFlags]'Public,NonPublic,Instance') | ForEach-Object {
    Write-Host "  Field: $($_.Name) : $($_.FieldType.Name)"
}
