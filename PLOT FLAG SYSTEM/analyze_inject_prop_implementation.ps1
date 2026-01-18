# Deep analysis of StageManager.InjectProp and injectedProps usage
# Goal: Understand what InjectProp does and where injectedProps is used

$managedPath = 'C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed'

Add-Type -Path "$managedPath\Gameplay.dll"

function Get-TypesSafely($assembly) {
    try { return $assembly.GetTypes() }
    catch [System.Reflection.ReflectionTypeLoadException] {
        return $_.Exception.Types | Where-Object { $_ -ne $null }
    }
}

$gameplayAsm = [System.Reflection.Assembly]::LoadFrom("$managedPath\Gameplay.dll")
$types = Get-TypesSafely $gameplayAsm

Write-Host "=== StageManager Analysis ===" -ForegroundColor Cyan
$stageManager = $types | Where-Object { $_.Name -eq 'StageManager' }

if ($stageManager) {
    Write-Host "`nFull Name: $($stageManager.FullName)"
    Write-Host "Base Type: $($stageManager.BaseType.Name)"

    Write-Host "`n--- InjectProp Method ---" -ForegroundColor Yellow
    $injectMethod = $stageManager.GetMethods() | Where-Object { $_.Name -eq 'InjectProp' }
    foreach ($m in $injectMethod) {
        Write-Host "  $($m.ReturnType.Name) InjectProp("
        foreach ($p in $m.GetParameters()) {
            Write-Host "    $($p.ParameterType.Name) $($p.Name)"
        }
        Write-Host "  )"
    }

    Write-Host "`n--- injectedProps Field ---" -ForegroundColor Yellow
    $injectedPropsField = $stageManager.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance) | Where-Object { $_.Name -eq 'injectedProps' }
    if ($injectedPropsField) {
        Write-Host "  Type: $($injectedPropsField.FieldType.FullName)"
        Write-Host "  IsPublic: $($injectedPropsField.IsPublic)"
    }

    Write-Host "`n--- All Fields containing 'prop' ---" -ForegroundColor Yellow
    $propFields = $stageManager.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance) | Where-Object { $_.Name -match 'prop|Prop' }
    foreach ($f in $propFields) {
        Write-Host "  $($f.FieldType.Name) $($f.Name) [Public:$($f.IsPublic)]"
    }

    Write-Host "`n--- All Methods containing 'prop' or 'inject' ---" -ForegroundColor Yellow
    $propMethods = $stageManager.GetMethods() | Where-Object { $_.Name -match 'prop|Prop|inject|Inject' }
    foreach ($m in $propMethods) {
        $params = ($m.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ', '
        Write-Host "  $($m.ReturnType.Name) $($m.Name)($params)"
    }
}

Write-Host "`n=== InjectedProps Struct ===" -ForegroundColor Cyan
$injectedPropsType = $types | Where-Object { $_.Name -eq 'InjectedProps' }
if ($injectedPropsType) {
    Write-Host "Full Name: $($injectedPropsType.FullName)"
    Write-Host "IsValueType: $($injectedPropsType.IsValueType)"

    Write-Host "`n--- Fields ---" -ForegroundColor Yellow
    $fields = $injectedPropsType.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
    foreach ($f in $fields) {
        Write-Host "  $($f.FieldType.Name) $($f.Name)"
    }
}

Write-Host "`n=== PropLibrary Analysis ===" -ForegroundColor Cyan
$propLibrary = $types | Where-Object { $_.Name -eq 'PropLibrary' }
if ($propLibrary) {
    Write-Host "Full Name: $($propLibrary.FullName)"

    Write-Host "`n--- loadedPropMap Field ---" -ForegroundColor Yellow
    $loadedPropMapField = $propLibrary.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance) | Where-Object { $_.Name -eq 'loadedPropMap' }
    if ($loadedPropMapField) {
        Write-Host "  Type: $($loadedPropMapField.FieldType.FullName)"
    }

    Write-Host "`n--- All Methods ---" -ForegroundColor Yellow
    $methods = $propLibrary.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::DeclaredOnly)
    foreach ($m in $methods) {
        $params = ($m.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ', '
        Write-Host "  $($m.ReturnType.Name) $($m.Name)($params)"
    }
}

Write-Host "`n=== Searching for UI classes that use props ===" -ForegroundColor Cyan
$uiTypes = $types | Where-Object { $_.Name -match 'PropTool|PropUI|PropGrid|PropList|PropSelect|PropPanel' }
foreach ($t in $uiTypes) {
    Write-Host "`nFound: $($t.FullName)" -ForegroundColor Green
    $methods = $t.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::DeclaredOnly)
    foreach ($m in $methods | Select-Object -First 10) {
        Write-Host "  $($m.Name)"
    }
}
