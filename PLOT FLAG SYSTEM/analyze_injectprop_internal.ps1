# Analyze what InjectProp does internally and what EndlessProp needs

$gameplayAsm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Gameplay.dll')

Write-Host "=== RuntimePropInfo ALL fields ===" -ForegroundColor Cyan
$rpiType = $gameplayAsm.GetType('Endless.Gameplay.LevelEditing.PropLibrary+RuntimePropInfo')
$fields = $rpiType.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
foreach ($f in $fields) {
    Write-Host "  $($f.FieldType.Name) $($f.Name)"
}

Write-Host ""
Write-Host "=== RuntimePropInfo constructors ===" -ForegroundColor Cyan
$ctors = $rpiType.GetConstructors([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
foreach ($c in $ctors) {
    Write-Host "Constructor:"
    foreach ($p in $c.GetParameters()) {
        Write-Host "  $($p.ParameterType.Name) $($p.Name)"
    }
}

Write-Host ""
Write-Host "=== EndlessProp - what it does on spawn ===" -ForegroundColor Cyan
$epType = $gameplayAsm.GetType('Endless.Gameplay.Scripting.EndlessProp')
$methods = $epType.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance) | Where-Object { $_.Name -like '*Spawn*' -or $_.Name -like '*Visual*' -or $_.Name -like '*Prefab*' -or $_.Name -like '*Load*' }
foreach ($m in $methods) {
    Write-Host "$($m.ReturnType.Name) $($m.Name)("
    foreach ($p in $m.GetParameters()) {
        Write-Host "  $($p.ParameterType.Name) $($p.Name)"
    }
    Write-Host ")"
}

Write-Host ""
Write-Host "=== EndlessVisuals type ===" -ForegroundColor Cyan
$evType = $gameplayAsm.GetType('Endless.Gameplay.EndlessVisuals')
if ($evType) {
    Write-Host "Found: $($evType.FullName)"
    $fields = $evType.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
    foreach ($f in $fields) {
        Write-Host "  $($f.FieldType.Name) $($f.Name)"
    }
} else {
    Write-Host "Not found in Gameplay.dll"
}

Write-Host ""
Write-Host "=== Looking for how testPrefab is used ===" -ForegroundColor Cyan
# Search for methods that take GameObject
$propLibType = $gameplayAsm.GetType('Endless.Gameplay.LevelEditing.PropLibrary')
$methods = $propLibType.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::DeclaredOnly)
foreach ($m in $methods) {
    foreach ($p in $m.GetParameters()) {
        if ($p.ParameterType.Name -eq 'GameObject') {
            Write-Host "$($m.Name) takes GameObject param: $($p.Name)"
        }
    }
}
