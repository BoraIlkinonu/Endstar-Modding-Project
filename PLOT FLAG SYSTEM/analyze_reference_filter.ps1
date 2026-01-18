$gameplayAsm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Gameplay.dll')

Write-Host "=== PropLibrary.PopulateReferenceFilterMap ==="
$propLibType = $gameplayAsm.GetType('Endless.Gameplay.LevelEditing.PropLibrary')
$method = $propLibType.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance) | Where-Object { $_.Name -eq 'PopulateReferenceFilterMap' }

foreach ($m in $method) {
    Write-Host "Method: $($m.Name)"
    Write-Host "Return: $($m.ReturnType.FullName)"
    Write-Host "Parameters:"
    foreach ($p in $m.GetParameters()) {
        Write-Host "  $($p.Position): $($p.ParameterType.FullName) $($p.Name)"
    }
}

Write-Host ""
Write-Host "=== ReferenceFilter enum ==="
$rfType = $gameplayAsm.GetType('Endless.Gameplay.LevelEditing.ReferenceFilter')
if ($rfType) {
    Write-Host "Values:"
    foreach ($name in [System.Enum]::GetNames($rfType)) {
        $val = [System.Enum]::Parse($rfType, $name)
        Write-Host "  $name = $([int]$val)"
    }
}

Write-Host ""
Write-Host "=== Prop fields that might be accessed ==="
$propsAsm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Props.dll')
$propType = $propsAsm.GetType('Endless.Props.Assets.Prop')
$fields = $propType.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::DeclaredOnly)
foreach ($f in $fields) {
    Write-Host "  $($f.FieldType.Name) $($f.Name)"
}
