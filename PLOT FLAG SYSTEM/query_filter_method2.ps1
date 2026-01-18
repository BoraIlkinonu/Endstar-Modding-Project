# Check UIRuntimePropInfoListModel base type methods
$asm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Creator.dll')

$listModelType = $asm.GetType('Endless.Creator.UI.UIRuntimePropInfoListModel')
Write-Host "Type: $($listModelType.FullName)"
Write-Host "Base: $($listModelType.BaseType.FullName)"

# Get all methods including inherited
$allMethods = $listModelType.GetMethods([System.Reflection.BindingFlags]'Public,NonPublic,Instance')
foreach ($m in $allMethods) {
    $params = $m.GetParameters()
    $paramStr = ($params | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ', '
    if ($m.Name -match '^(Filter|Sort|Refresh|Handle|Add|ReFilter|ReSort)$') {
        Write-Host "  $($m.Name)($paramStr) : $($m.ReturnType.Name) [from $($m.DeclaringType.Name)]"
    }
}
