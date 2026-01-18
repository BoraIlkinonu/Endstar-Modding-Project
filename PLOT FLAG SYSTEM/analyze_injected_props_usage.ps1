# Analyze how StageManager.injectedProps is used

$gameplayAsm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Gameplay.dll')

Write-Host "=== StageManager.injectedProps field ===" -ForegroundColor Cyan
$smType = $gameplayAsm.GetType('Endless.Gameplay.LevelEditing.Level.StageManager')
$injPropsField = $smType.GetField('injectedProps', [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
if ($injPropsField) {
    Write-Host "Type: $($injPropsField.FieldType.FullName)"
}

Write-Host ""
Write-Host "=== StageManager methods that use injectedProps ===" -ForegroundColor Cyan
$methods = $smType.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::DeclaredOnly)
foreach ($m in $methods) {
    if ($m.Name -like '*Inject*' -or $m.Name -like '*Test*' -or $m.Name -like '*Build*') {
        Write-Host ""
        Write-Host "--- $($m.Name) ---"
        Write-Host "Return: $($m.ReturnType.Name)"
        foreach ($p in $m.GetParameters()) {
            Write-Host "  $($p.ParameterType.Name) $($p.Name)"
        }
    }
}

Write-Host ""
Write-Host "=== How does PropLibrary.InjectProp use the parameters? ===" -ForegroundColor Cyan
Write-Host "Looking at what it stores in RuntimePropInfo..."

$propLibType = $gameplayAsm.GetType('Endless.Gameplay.LevelEditing.PropLibrary')
$injectMethod = $propLibType.GetMethod('InjectProp', [System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance)
Write-Host "InjectProp parameters:"
foreach ($p in $injectMethod.GetParameters()) {
    Write-Host "  $($p.ParameterType.Name) $($p.Name)"
}

Write-Host ""
Write-Host "=== Check if there's a way to force testPrefab usage ===" -ForegroundColor Cyan
# Look for properties or methods related to test mode
$testMethods = $methods | Where-Object { $_.Name -like '*Test*' -or $_.Name -like '*Debug*' -or $_.Name -like '*Override*' }
foreach ($m in $testMethods) {
    Write-Host "$($m.ReturnType.Name) $($m.Name)"
}
