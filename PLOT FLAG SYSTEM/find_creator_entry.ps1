# Find the correct entry point for Creator mode
# We need to find what method is called when entering edit mode

$managedPath = "C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed"

# Load assemblies
$allDlls = Get-ChildItem "$managedPath\*.dll"
$loadedAssemblies = @{}

foreach ($dll in $allDlls) {
    try {
        $asm = [System.Reflection.Assembly]::LoadFrom($dll.FullName)
        $loadedAssemblies[$dll.Name] = $asm
    } catch {}
}

function Get-TypesSafely($assembly) {
    try { return $assembly.GetTypes() }
    catch [System.Reflection.ReflectionTypeLoadException] {
        return $_.Exception.Types | Where-Object { $_ -ne $null }
    }
    catch { return @() }
}

$creatorDll = $loadedAssemblies["Creator.dll"]
$gameplayDll = $loadedAssemblies["Gameplay.dll"]

$creatorTypes = Get-TypesSafely $creatorDll
$gameplayTypes = Get-TypesSafely $gameplayDll

Write-Host "=== SEARCHING FOR CREATOR MODE ENTRY POINTS ===" -ForegroundColor Cyan
Write-Host ""

# 1. Find CreatorManager - likely the main entry point
Write-Host "=== CreatorManager ===" -ForegroundColor Yellow
$creatorManager = $creatorTypes | Where-Object { $_.Name -eq "CreatorManager" }
if ($creatorManager) {
    Write-Host "Found: $($creatorManager.FullName)"
    Write-Host "Base: $($creatorManager.BaseType.Name)"

    # Check for singleton
    $instance = $creatorManager.GetProperty("Instance", [System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Static)
    if ($instance) { Write-Host "*** HAS SINGLETON Instance ***" -ForegroundColor Green }

    # Methods
    Write-Host ""
    Write-Host "Methods:"
    $methods = $creatorManager.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::DeclaredOnly) | Where-Object { -not $_.IsSpecialName }
    foreach ($m in $methods | Sort-Object Name) {
        $params = ($m.GetParameters() | ForEach-Object { "$($_.ParameterType.Name)" }) -join ", "
        Write-Host "  $($m.Name)($params) -> $($m.ReturnType.Name)"
    }

    # Events
    Write-Host ""
    Write-Host "Events:"
    $events = $creatorManager.GetEvents()
    foreach ($e in $events) {
        Write-Host "  $($e.Name) ($($e.EventHandlerType.Name))"
    }

    # Fields
    Write-Host ""
    Write-Host "Key Fields:"
    $fields = $creatorManager.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
    foreach ($f in $fields | Where-Object { $_.Name -match "prop|library|stage" }) {
        Write-Host "  $($f.FieldType.Name) $($f.Name)"
    }
}

Write-Host ""
Write-Host "=== StageManager Methods ===" -ForegroundColor Yellow
$stageManager = $gameplayTypes | Where-Object { $_.Name -eq "StageManager" }
if ($stageManager) {
    $methods = $stageManager.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::DeclaredOnly) | Where-Object { -not $_.IsSpecialName }
    foreach ($m in $methods | Sort-Object Name) {
        $params = ($m.GetParameters() | ForEach-Object { "$($_.ParameterType.Name)" }) -join ", "
        Write-Host "  $($m.Name)($params) -> $($m.ReturnType.Name)"
    }
}

Write-Host ""
Write-Host "=== PropLibrary Methods ===" -ForegroundColor Yellow
$propLibrary = $gameplayTypes | Where-Object { $_.Name -eq "PropLibrary" }
if ($propLibrary) {
    $methods = $propLibrary.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::DeclaredOnly) | Where-Object { -not $_.IsSpecialName }
    foreach ($m in $methods | Sort-Object Name) {
        $params = ($m.GetParameters() | ForEach-Object { "$($_.ParameterType.Name)" }) -join ", "
        Write-Host "  $($m.Name)($params) -> $($m.ReturnType.Name)"
    }
}

Write-Host ""
Write-Host "=== Looking for Start/Enter/Initialize methods ===" -ForegroundColor Yellow
$entryMethods = @()
foreach ($type in $creatorTypes) {
    $methods = $type.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance) |
        Where-Object { $_.Name -match "^(Start|Enter|Begin|Initialize|OnEnable|Awake).*Creator|^Creator.*(Start|Enter|Begin)" }
    foreach ($m in $methods) {
        Write-Host "  $($type.Name).$($m.Name)()"
    }
}

Write-Host ""
Write-Host "=== Looking for LoadProps/PopulateProps methods ===" -ForegroundColor Yellow
foreach ($type in ($creatorTypes + $gameplayTypes)) {
    $methods = $type.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance) |
        Where-Object { $_.Name -match "Load.*Prop|Populate.*Prop|Init.*Prop|Setup.*Prop|Repopulate" }
    foreach ($m in $methods) {
        $params = ($m.GetParameters() | ForEach-Object { "$($_.ParameterType.Name)" }) -join ", "
        Write-Host "  $($type.Name).$($m.Name)($params)"
    }
}

Write-Host ""
Write-Host "=== PropLibrary constructor ===" -ForegroundColor Yellow
if ($propLibrary) {
    $ctors = $propLibrary.GetConstructors([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
    foreach ($c in $ctors) {
        $params = ($c.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ", "
        Write-Host "  PropLibrary($params)"
    }
}

Write-Host ""
Write-Host "=== DONE ===" -ForegroundColor Cyan
