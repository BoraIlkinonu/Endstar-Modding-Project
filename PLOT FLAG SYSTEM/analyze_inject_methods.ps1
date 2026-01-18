$gameplayAsm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Gameplay.dll')
$propsAsm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Props.dll')

Write-Host "=== PropLibrary.PopulateReferenceFilterMap ===" -ForegroundColor Cyan
$propLibType = $gameplayAsm.GetType('Endless.Gameplay.LevelEditing.PropLibrary')
$methods = $propLibType.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance) | Where-Object { $_.Name -eq 'PopulateReferenceFilterMap' }

foreach ($m in $methods) {
    Write-Host ""
    Write-Host "Method: $($m.Name)"
    Write-Host "Return: $($m.ReturnType.FullName)"
    Write-Host "Parameters:"
    foreach ($p in $m.GetParameters()) {
        Write-Host "  $($p.Position): $($p.ParameterType.FullName) $($p.Name)"
    }
}

Write-Host ""
Write-Host "=== _referenceFilterMap field type ===" -ForegroundColor Cyan
$field = $propLibType.GetField('_referenceFilterMap', [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
if ($field) {
    Write-Host "Field type: $($field.FieldType.FullName)"
    Write-Host "Generic args:"
    foreach ($arg in $field.FieldType.GetGenericArguments()) {
        Write-Host "  $($arg.FullName)"
    }
}

Write-Host ""
Write-Host "=== ReferenceFilter enum ===" -ForegroundColor Cyan
$rfType = $gameplayAsm.GetType('Endless.Gameplay.LevelEditing.ReferenceFilter')
if ($rfType) {
    Write-Host "Values:"
    foreach ($name in [System.Enum]::GetNames($rfType)) {
        $val = [System.Enum]::Parse($rfType, $name)
        Write-Host "  $name = $([int]$val)"
    }
}

Write-Host ""
Write-Host "=== StageManager.InjectProp ===" -ForegroundColor Cyan
$smType = $gameplayAsm.GetType('Endless.Gameplay.LevelEditing.Level.StageManager')
$injectMethods = $smType.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance) | Where-Object { $_.Name -eq 'InjectProp' }

foreach ($m in $injectMethods) {
    Write-Host ""
    Write-Host "Method: $($m.Name)"
    Write-Host "Return: $($m.ReturnType.FullName)"
    Write-Host "Parameters:"
    foreach ($p in $m.GetParameters()) {
        Write-Host "  $($p.Position): $($p.ParameterType.FullName) $($p.Name)"
    }
}

Write-Host ""
Write-Host "=== PropLibrary.InjectProp ===" -ForegroundColor Cyan
$libInjectMethods = $propLibType.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance) | Where-Object { $_.Name -eq 'InjectProp' }

foreach ($m in $libInjectMethods) {
    Write-Host ""
    Write-Host "Method: $($m.Name)"
    Write-Host "Return: $($m.ReturnType.FullName)"
    Write-Host "Parameters:"
    foreach ($p in $m.GetParameters()) {
        Write-Host "  $($p.Position): $($p.ParameterType.FullName) $($p.Name)"
    }
}

Write-Host ""
Write-Host "=== Prop all fields (including base classes) ===" -ForegroundColor Cyan
$propType = $propsAsm.GetType('Endless.Props.Assets.Prop')
$currentType = $propType
while ($currentType -ne $null -and $currentType -ne [System.Object]) {
    Write-Host ""
    Write-Host "--- $($currentType.FullName) ---"
    $fields = $currentType.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::DeclaredOnly)
    foreach ($f in $fields) {
        Write-Host "  $($f.FieldType.Name) $($f.Name)"
    }
    $currentType = $currentType.BaseType
}

Write-Host ""
Write-Host "=== EndlessProp key fields and properties ===" -ForegroundColor Cyan
$epType = $gameplayAsm.GetType('Endless.Gameplay.Scripting.EndlessProp')
if ($epType) {
    Write-Host "Base: $($epType.BaseType.FullName)"
    Write-Host ""
    Write-Host "Fields:"
    $fields = $epType.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::DeclaredOnly)
    foreach ($f in $fields) {
        Write-Host "  $($f.FieldType.Name) $($f.Name)"
    }
    Write-Host ""
    Write-Host "Properties:"
    $props = $epType.GetProperties([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::DeclaredOnly)
    foreach ($p in $props) {
        Write-Host "  $($p.PropertyType.Name) $($p.Name)"
    }
}
