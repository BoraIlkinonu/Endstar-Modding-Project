$managedPath = 'C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed'
$gameplayDll = [System.Reflection.Assembly]::LoadFrom("$managedPath\Gameplay.dll")
$propLibraryType = $gameplayDll.GetType('Endless.Gameplay.LevelEditing.PropLibrary')

Write-Host '=== PropLibrary.Repopulate Overloads ===' -ForegroundColor Yellow
$methods = $propLibraryType.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance) | Where-Object { $_.Name -eq 'Repopulate' }
foreach ($m in $methods) {
    $params = ($m.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ', '
    Write-Host "  $($m.Name)($params) -> $($m.ReturnType.Name)"
}

Write-Host ''
Write-Host '=== PropLibrary.GetAllRuntimeProps Overloads ===' -ForegroundColor Yellow
$methods = $propLibraryType.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance) | Where-Object { $_.Name -eq 'GetAllRuntimeProps' }
foreach ($m in $methods) {
    $params = ($m.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ', '
    Write-Host "  $($m.Name)($params) -> $($m.ReturnType.Name)"
}
