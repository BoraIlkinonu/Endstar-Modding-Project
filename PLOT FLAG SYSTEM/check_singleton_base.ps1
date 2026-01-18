# Check how MonoBehaviourSingleton and NetworkBehaviourSingleton work

$managedPath = 'C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed'
$sharedDll = [System.Reflection.Assembly]::LoadFrom("$managedPath\Shared.dll")

function Get-TypesSafely($assembly) {
    try { return $assembly.GetTypes() }
    catch [System.Reflection.ReflectionTypeLoadException] {
        return $_.Exception.Types | Where-Object { $_ -ne $null }
    }
    catch { return @() }
}

$types = Get-TypesSafely $sharedDll

Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host "MonoBehaviourSingleton<T>" -ForegroundColor Cyan
Write-Host "=" * 80 -ForegroundColor Cyan

$mbSingleton = $types | Where-Object { $_.Name -match 'MonoBehaviourSingleton' -and $_.IsGenericTypeDefinition }
if ($mbSingleton) {
    Write-Host "Found: $($mbSingleton.FullName)"

    Write-Host ""
    Write-Host "Static Properties:" -ForegroundColor Yellow
    $props = $mbSingleton.GetProperties([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Static)
    foreach ($p in $props) {
        Write-Host "  [static] $($p.PropertyType.Name) $($p.Name)"
    }

    Write-Host ""
    Write-Host "Static Fields:" -ForegroundColor Yellow
    $fields = $mbSingleton.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Static)
    foreach ($f in $fields) {
        Write-Host "  [static] $($f.FieldType.Name) $($f.Name)"
    }
}

Write-Host ""
Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host "NetworkBehaviourSingleton<T>" -ForegroundColor Cyan
Write-Host "=" * 80 -ForegroundColor Cyan

$nbSingleton = $types | Where-Object { $_.Name -match 'NetworkBehaviourSingleton' -and $_.IsGenericTypeDefinition }
if ($nbSingleton) {
    Write-Host "Found: $($nbSingleton.FullName)"

    Write-Host ""
    Write-Host "Static Properties:" -ForegroundColor Yellow
    $props = $nbSingleton.GetProperties([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Static)
    foreach ($p in $props) {
        Write-Host "  [static] $($p.PropertyType.Name) $($p.Name)"
    }

    Write-Host ""
    Write-Host "Static Fields:" -ForegroundColor Yellow
    $fields = $nbSingleton.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Static)
    foreach ($f in $fields) {
        Write-Host "  [static] $($f.FieldType.Name) $($f.Name)"
    }

    Write-Host ""
    Write-Host "Instance Properties:" -ForegroundColor Yellow
    $props = $nbSingleton.GetProperties([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance)
    foreach ($p in $props) {
        Write-Host "  $($p.PropertyType.Name) $($p.Name)"
    }
}

Write-Host ""
Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host "Actual StageManager.Instance access" -ForegroundColor Cyan
Write-Host "=" * 80 -ForegroundColor Cyan

$gameplayDll = [System.Reflection.Assembly]::LoadFrom("$managedPath\Gameplay.dll")
$stageManagerType = $gameplayDll.GetType('Endless.Gameplay.LevelEditing.Level.StageManager')

# Check inherited static properties
Write-Host "StageManager static members (including inherited):" -ForegroundColor Yellow
$props = $stageManagerType.GetProperties([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Static -bor [System.Reflection.BindingFlags]::FlattenHierarchy)
foreach ($p in $props) {
    Write-Host "  [static] $($p.PropertyType.Name) $($p.Name)"
}

$fields = $stageManagerType.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Static -bor [System.Reflection.BindingFlags]::FlattenHierarchy)
foreach ($f in $fields) {
    Write-Host "  [static field] $($f.FieldType.Name) $($f.Name)"
}

Write-Host ""
Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host "DONE" -ForegroundColor Cyan
