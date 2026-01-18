# Analyze the PROPER InjectProp method

$managedPath = 'C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed'
$gameplayDll = [System.Reflection.Assembly]::LoadFrom("$managedPath\Gameplay.dll")

$stageManager = $gameplayDll.GetType('Endless.Gameplay.LevelEditing.Level.StageManager')
$propLibrary = $gameplayDll.GetType('Endless.Gameplay.LevelEditing.PropLibrary')

Write-Host "=== StageManager.InjectProp ===" -ForegroundColor Cyan
$method = $stageManager.GetMethod('InjectProp', [System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance)
if ($method) {
    Write-Host "Found!"
    Write-Host "Parameters:"
    foreach ($p in $method.GetParameters()) {
        Write-Host "  $($p.Position): $($p.ParameterType.FullName) $($p.Name)"
    }
    Write-Host "Returns: $($method.ReturnType.Name)"
}

Write-Host ""
Write-Host "=== PropLibrary.InjectProp ===" -ForegroundColor Cyan
$method = $propLibrary.GetMethod('InjectProp', [System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance)
if ($method) {
    Write-Host "Found!"
    Write-Host "Parameters:"
    foreach ($p in $method.GetParameters()) {
        Write-Host "  $($p.Position): $($p.ParameterType.FullName) $($p.Name)"
    }
    Write-Host "Returns: $($method.ReturnType.Name)"
    Write-Host "IsAsync: $($method.ReturnType.Name -match 'Task')"
}

Write-Host ""
Write-Host "=== InjectedProps class (what gets stored) ===" -ForegroundColor Cyan

function Get-TypesSafely($assembly) {
    try { return $assembly.GetTypes() }
    catch [System.Reflection.ReflectionTypeLoadException] {
        return $_.Exception.Types | Where-Object { $_ -ne $null }
    }
}

$types = Get-TypesSafely $gameplayDll
$injectedPropsType = $types | Where-Object { $_.FullName -eq 'Endless.Gameplay.LevelEditing.Level.InjectedProps' }
if ($injectedPropsType) {
    Write-Host "Fields:"
    $fields = $injectedPropsType.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance)
    foreach ($f in $fields) {
        Write-Host "  $($f.FieldType.Name) $($f.Name)"
    }
}

Write-Host ""
Write-Host "=== StageManager.injectedProps field ===" -ForegroundColor Cyan
$field = $stageManager.GetField('injectedProps', [System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
if ($field) {
    Write-Host "Type: $($field.FieldType.FullName)"
    Write-Host "IsPublic: $($field.IsPublic)"
}
