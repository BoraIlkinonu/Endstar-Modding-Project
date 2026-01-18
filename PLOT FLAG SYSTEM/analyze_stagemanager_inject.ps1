# Analyze what StageManager.InjectProp does and if it calls PropLibrary.InjectProp

$gameplayAsm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Gameplay.dll')

Write-Host "=== StageManager.InjectProp details ===" -ForegroundColor Cyan
$smType = $gameplayAsm.GetType('Endless.Gameplay.LevelEditing.Level.StageManager')
$injectMethod = $smType.GetMethod('InjectProp', [System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance)

if ($injectMethod) {
    Write-Host "Found: $($injectMethod.Name)"
    Write-Host "Return: $($injectMethod.ReturnType.FullName)"
    Write-Host "Parameters:"
    foreach ($p in $injectMethod.GetParameters()) {
        Write-Host "  $($p.ParameterType.FullName) $($p.Name)"
    }
}

Write-Host ""
Write-Host "=== StageManager fields that might be involved ===" -ForegroundColor Cyan
$fields = $smType.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
foreach ($f in $fields) {
    if ($f.Name -like '*inject*' -or $f.Name -like '*prop*' -or $f.Name -like '*library*') {
        Write-Host "  $($f.FieldType.Name) $($f.Name)"
    }
}

Write-Host ""
Write-Host "=== Does StageManager have activePropLibrary? ===" -ForegroundColor Cyan
$aplField = $smType.GetField('activePropLibrary', [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
if ($aplField) {
    Write-Host "Yes: $($aplField.FieldType.FullName)"
}
$aplProp = $smType.GetProperty('ActivePropLibrary', [System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance)
if ($aplProp) {
    Write-Host "Property: $($aplProp.PropertyType.FullName)"
}

Write-Host ""
Write-Host "=== Looking for when injectedProps is used ===" -ForegroundColor Cyan
# Check methods that might process injectedProps
$methods = $smType.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::DeclaredOnly)
foreach ($m in $methods) {
    if ($m.Name -like '*Load*' -or $m.Name -like '*Populate*' -or $m.Name -like '*Build*' -or $m.Name -like '*Process*') {
        Write-Host "$($m.ReturnType.Name) $($m.Name)"
    }
}

Write-Host ""
Write-Host "=== PropLibrary - does it have a method to add RuntimePropInfo directly? ===" -ForegroundColor Cyan
$propLibType = $gameplayAsm.GetType('Endless.Gameplay.LevelEditing.PropLibrary')
$methods = $propLibType.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::DeclaredOnly)
foreach ($m in $methods) {
    if ($m.Name -like '*Add*' -or $m.Name -like '*Insert*' -or $m.Name -like '*Register*') {
        Write-Host "$($m.ReturnType.Name) $($m.Name)"
        foreach ($p in $m.GetParameters()) {
            Write-Host "  $($p.ParameterType.Name) $($p.Name)"
        }
    }
}
