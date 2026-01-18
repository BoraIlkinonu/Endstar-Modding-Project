# Analyze prop filtering system and UI data source

$managedPath = 'C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed'

function Get-TypesSafely($assembly) {
    try { return $assembly.GetTypes() }
    catch [System.Reflection.ReflectionTypeLoadException] {
        return $_.Exception.Types | Where-Object { $_ -ne $null }
    }
}

$gameplayAsm = [System.Reflection.Assembly]::LoadFrom("$managedPath\Gameplay.dll")
$creatorAsm = [System.Reflection.Assembly]::LoadFrom("$managedPath\Creator.dll")

$gameplayTypes = Get-TypesSafely $gameplayAsm
$creatorTypes = Get-TypesSafely $creatorAsm

Write-Host "=== ReferenceFilter Analysis ===" -ForegroundColor Cyan
$referenceFilter = $gameplayTypes | Where-Object { $_.Name -eq 'ReferenceFilter' }
if ($referenceFilter) {
    Write-Host "Full Name: $($referenceFilter.FullName)"
    Write-Host "IsEnum: $($referenceFilter.IsEnum)"

    if ($referenceFilter.IsEnum) {
        Write-Host "`nEnum Values:" -ForegroundColor Yellow
        [Enum]::GetNames($referenceFilter) | ForEach-Object {
            $val = [Enum]::Parse($referenceFilter, $_)
            Write-Host "  $_ = $([int]$val)"
        }
    }
}

Write-Host "`n=== RuntimePropInfo Analysis ===" -ForegroundColor Cyan
$runtimePropInfo = $gameplayTypes | Where-Object { $_.Name -eq 'RuntimePropInfo' }
if ($runtimePropInfo) {
    Write-Host "Full Name: $($runtimePropInfo.FullName)"

    Write-Host "`n--- All Fields ---" -ForegroundColor Yellow
    $fields = $runtimePropInfo.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
    foreach ($f in $fields) {
        Write-Host "  $($f.FieldType.Name) $($f.Name) [Public:$($f.IsPublic)]"
    }

    Write-Host "`n--- All Properties ---" -ForegroundColor Yellow
    $props = $runtimePropInfo.GetProperties([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
    foreach ($p in $props) {
        Write-Host "  $($p.PropertyType.Name) $($p.Name)"
    }
}

Write-Host "`n=== EndlessProp Analysis ===" -ForegroundColor Cyan
$endlessProp = $gameplayTypes | Where-Object { $_.Name -eq 'EndlessProp' }
if ($endlessProp) {
    Write-Host "Full Name: $($endlessProp.FullName)"
    Write-Host "Base Type: $($endlessProp.BaseType.Name)"

    Write-Host "`n--- Key Fields ---" -ForegroundColor Yellow
    $fields = $endlessProp.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance) | Select-Object -First 15
    foreach ($f in $fields) {
        Write-Host "  $($f.FieldType.Name) $($f.Name)"
    }
}

Write-Host "`n=== Creator UI Classes ===" -ForegroundColor Cyan
$uiClasses = $creatorTypes | Where-Object { $_.Name -match 'Prop|Tool|Panel|Grid|List' -and $_.Name -notmatch '\+' }
foreach ($t in $uiClasses | Select-Object -First 20) {
    Write-Host "`n$($t.Name)" -ForegroundColor Green
}

Write-Host "`n=== Searching for PropTool or similar ===" -ForegroundColor Cyan
$propToolTypes = $creatorTypes | Where-Object { $_.FullName -match 'PropTool|PropPanel|PropWindow' }
foreach ($t in $propToolTypes) {
    Write-Host "`nFound: $($t.FullName)" -ForegroundColor Green

    $methods = $t.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::DeclaredOnly)
    Write-Host "Methods:" -ForegroundColor Yellow
    foreach ($m in $methods | Select-Object -First 15) {
        Write-Host "  $($m.Name)"
    }

    $fields = $t.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
    Write-Host "Fields:" -ForegroundColor Yellow
    foreach ($f in $fields | Select-Object -First 10) {
        Write-Host "  $($f.FieldType.Name) $($f.Name)"
    }
}

Write-Host "`n=== TerrainAndPropsLoaded Event ===" -ForegroundColor Cyan
$stageManager = $gameplayTypes | Where-Object { $_.Name -eq 'StageManager' }
if ($stageManager) {
    $eventField = $stageManager.GetField('TerrainAndPropsLoaded', [System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance)
    if ($eventField) {
        Write-Host "Event Type: $($eventField.FieldType.FullName)"
    }
}
