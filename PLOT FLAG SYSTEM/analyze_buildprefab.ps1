# Deep analysis of EndlessProp.BuildPrefab to understand how it handles prefabBundle vs testPrefab

$gameplayAsm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Gameplay.dll')

Write-Host "=== EndlessProp.BuildPrefab - Full Analysis ===" -ForegroundColor Cyan
$epType = $gameplayAsm.GetType('Endless.Gameplay.Scripting.EndlessProp')

$buildMethod = $epType.GetMethod('BuildPrefab', [System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
if ($buildMethod) {
    Write-Host "Method found: $($buildMethod.Name)"
    Write-Host "Return type: $($buildMethod.ReturnType.FullName)"
    Write-Host "Parameters:"
    foreach ($p in $buildMethod.GetParameters()) {
        Write-Host "  $($p.ParameterType.FullName) $($p.Name)"
    }
}

Write-Host ""
Write-Host "=== EndlessProp ALL methods (looking for prefab/visual related) ===" -ForegroundColor Cyan
$methods = $epType.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::DeclaredOnly)
foreach ($m in $methods) {
    $params = ($m.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ', '
    Write-Host "  $($m.ReturnType.Name) $($m.Name)($params)"
}

Write-Host ""
Write-Host "=== Looking for how prop visuals are loaded ===" -ForegroundColor Cyan
# Search for methods related to loading/spawning
$loadMethods = $methods | Where-Object { $_.Name -like '*Load*' -or $_.Name -like '*Spawn*' -or $_.Name -like '*Create*' -or $_.Name -like '*Build*' -or $_.Name -like '*Visual*' }
foreach ($m in $loadMethods) {
    Write-Host ""
    Write-Host "--- $($m.Name) ---"
    Write-Host "Return: $($m.ReturnType.FullName)"
    foreach ($p in $m.GetParameters()) {
        Write-Host "  Param: $($p.ParameterType.FullName) $($p.Name)"
    }
}

Write-Host ""
Write-Host "=== PropLibrary.InjectProp - what it does ===" -ForegroundColor Cyan
$propLibType = $gameplayAsm.GetType('Endless.Gameplay.LevelEditing.PropLibrary')
$injectMethod = $propLibType.GetMethod('InjectProp', [System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance)
if ($injectMethod) {
    Write-Host "Parameters:"
    foreach ($p in $injectMethod.GetParameters()) {
        Write-Host "  $($p.ParameterType.FullName) $($p.Name)"
    }
}

Write-Host ""
Write-Host "=== RuntimePropInfo - checking if it stores testPrefab separately ===" -ForegroundColor Cyan
$rpiType = $gameplayAsm.GetType('Endless.Gameplay.LevelEditing.PropLibrary+RuntimePropInfo')
$fields = $rpiType.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
foreach ($f in $fields) {
    Write-Host "  $($f.FieldType.FullName) $($f.Name)"
}

Write-Host ""
Write-Host "=== InjectedProps class - what data it stores ===" -ForegroundColor Cyan
$injPropsType = $gameplayAsm.GetType('Endless.Gameplay.LevelEditing.Level.InjectedProps')
if ($injPropsType) {
    $fields = $injPropsType.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
    foreach ($f in $fields) {
        Write-Host "  $($f.FieldType.FullName) $($f.Name)"
    }
}
