$managedPath = 'C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed'
$gameplayDll = [System.Reflection.Assembly]::LoadFrom("$managedPath\Gameplay.dll")
$propLibraryType = $gameplayDll.GetType('Endless.Gameplay.LevelEditing.PropLibrary')

Write-Host '=== All PropLibrary Methods ===' -ForegroundColor Cyan
$methods = $propLibraryType.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::DeclaredOnly)
foreach ($m in $methods | Sort-Object Name) {
    $params = ($m.GetParameters() | ForEach-Object { "$($_.ParameterType.Name)" }) -join ', '
    Write-Host "  $($m.Name)($params) -> $($m.ReturnType.Name)"
}

Write-Host ''
Write-Host '=== Constructors ===' -ForegroundColor Cyan
$ctors = $propLibraryType.GetConstructors([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
foreach ($c in $ctors) {
    $params = ($c.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ', '
    Write-Host "  PropLibrary($params)"
}

Write-Host ''
Write-Host '=== Check if Repopulate is virtual/override ===' -ForegroundColor Cyan
$repop = $propLibraryType.GetMethod('Repopulate', [System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance)
if ($repop) {
    Write-Host "  IsVirtual: $($repop.IsVirtual)"
    Write-Host "  IsFinal: $($repop.IsFinal)"
    Write-Host "  DeclaringType: $($repop.DeclaringType.Name)"
}

Write-Host ''
Write-Host '=== Base class ===' -ForegroundColor Cyan
Write-Host "  PropLibrary base: $($propLibraryType.BaseType.Name)"
if ($propLibraryType.BaseType -ne [object]) {
    $baseMethods = $propLibraryType.BaseType.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance) | Where-Object { $_.Name -match 'Repopulate|GetAll' }
    foreach ($m in $baseMethods) {
        Write-Host "    $($m.Name) in $($m.DeclaringType.Name)"
    }
}
