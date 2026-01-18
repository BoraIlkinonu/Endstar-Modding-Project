$asm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Gameplay.dll')

Write-Host "=== PropLibrary Fields ===" -ForegroundColor Cyan
$pl = $asm.GetType('Endless.Gameplay.PropLibrary')
foreach ($f in $pl.GetFields([System.Reflection.BindingFlags]'Public,NonPublic,Instance,Static')) {
    Write-Host "  $($f.Name) : $($f.FieldType.Name) (Static=$($f.IsStatic))"
}

Write-Host ""
Write-Host "=== PropLibrary Methods related to Filter ===" -ForegroundColor Cyan
foreach ($m in $pl.GetMethods([System.Reflection.BindingFlags]'Public,NonPublic,Instance,Static')) {
    if ($m.Name -match 'Filter|Reference|Populate|Props') {
        $params = $m.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }
        Write-Host "  $($m.ReturnType.Name) $($m.Name)($($params -join ', '))"
    }
}

Write-Host ""
Write-Host "=== _referenceFilterMap field type details ===" -ForegroundColor Cyan
$rfmField = $pl.GetField('_referenceFilterMap', [System.Reflection.BindingFlags]'NonPublic,Instance')
if ($rfmField) {
    Write-Host "  Full Type: $($rfmField.FieldType.FullName)"
    Write-Host "  Generic Args:"
    foreach ($ga in $rfmField.FieldType.GetGenericArguments()) {
        Write-Host "    - $($ga.FullName)"
    }
}

Write-Host ""
Write-Host "=== ReferenceFilter enum (search all loaded assemblies) ===" -ForegroundColor Cyan
$allDlls = @(
    'C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Gameplay.dll',
    'C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Assets.dll',
    'C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Props.dll',
    'C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Creator.dll'
)
foreach ($dll in $allDlls) {
    $a = [System.Reflection.Assembly]::LoadFrom($dll)
    foreach ($t in $a.GetTypes()) {
        if ($t.Name -eq 'ReferenceFilter' -and $t.IsEnum) {
            Write-Host "  Found in: $dll"
            Write-Host "  Full name: $($t.FullName)"
            [System.Enum]::GetValues($t) | ForEach-Object { Write-Host "    $($_.ToString()) = $([int]$_)" }
        }
    }
}

Write-Host ""
Write-Host "=== dynamicFilters static field search ===" -ForegroundColor Cyan
foreach ($dll in $allDlls) {
    $a = [System.Reflection.Assembly]::LoadFrom($dll)
    foreach ($t in $a.GetTypes()) {
        $dynField = $t.GetField('dynamicFilters', [System.Reflection.BindingFlags]'Public,NonPublic,Static,Instance')
        if ($dynField) {
            Write-Host "  Found on: $($t.FullName)"
            Write-Host "  Type: $($dynField.FieldType.FullName)"
            Write-Host "  IsStatic: $($dynField.IsStatic)"
        }
    }
}
