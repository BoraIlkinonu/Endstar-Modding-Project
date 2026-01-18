$asm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Creator.dll')

Write-Host "=== CreatorManager Methods related to Props/Filter ===" -ForegroundColor Cyan
$cm = $asm.GetType('Endless.Creator.CreatorManager')
foreach ($m in $cm.GetMethods([System.Reflection.BindingFlags]'Public,NonPublic,Instance,Static')) {
    if ($m.Name -match 'Prop|Filter|Populate|Reference') {
        $params = $m.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }
        Write-Host "  $($m.ReturnType.Name) $($m.Name)($($params -join ', '))"
    }
}

Write-Host ""
Write-Host "=== _referenceFilterMap field details ===" -ForegroundColor Cyan
$field = $cm.GetField('_referenceFilterMap', [System.Reflection.BindingFlags]'NonPublic,Instance')
if ($field) {
    Write-Host "  Type: $($field.FieldType.FullName)"
}

Write-Host ""
Write-Host "=== ReferenceFilter enum from Assets.dll ===" -ForegroundColor Cyan
$asmAssets = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Assets.dll')
$rf = $asmAssets.GetType('Endless.Assets.ReferenceFilter')
if ($rf) {
    Write-Host "  Found in Assets.dll"
    [System.Enum]::GetValues($rf) | ForEach-Object { Write-Host "  $($_.ToString()) = $([int]$_)" }
} else {
    Write-Host "  Not found in Assets.dll, checking Creator.dll..."
    $rf = $asm.GetType('Endless.Creator.ReferenceFilter')
    if ($rf) {
        [System.Enum]::GetValues($rf) | ForEach-Object { Write-Host "  $($_.ToString()) = $([int]$_)" }
    }
}

Write-Host ""
Write-Host "=== dynamicFilters field ===" -ForegroundColor Cyan
$dfField = $cm.GetField('dynamicFilters', [System.Reflection.BindingFlags]'Public,NonPublic,Instance,Static')
if ($dfField) {
    Write-Host "  Type: $($dfField.FieldType.FullName)"
    Write-Host "  IsStatic: $($dfField.IsStatic)"
} else {
    Write-Host "  Not found directly, searching all fields..."
    foreach ($f in $cm.GetFields([System.Reflection.BindingFlags]'Public,NonPublic,Instance,Static')) {
        if ($f.Name -match 'filter' -or $f.FieldType.Name -match 'ReferenceFilter') {
            Write-Host "  $($f.Name) : $($f.FieldType.Name) (Static=$($f.IsStatic))"
        }
    }
}

Write-Host ""
Write-Host "=== Methods that return/use ReferenceFilter[] ===" -ForegroundColor Cyan
foreach ($m in $cm.GetMethods([System.Reflection.BindingFlags]'Public,NonPublic,Instance,Static')) {
    if ($m.ReturnType.Name -match 'ReferenceFilter' -or $m.Name -match 'dynamic') {
        $params = $m.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }
        Write-Host "  $($m.ReturnType.Name) $($m.Name)($($params -join ', '))"
    }
}
