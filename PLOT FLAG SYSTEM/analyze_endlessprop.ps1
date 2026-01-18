$asm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Gameplay.dll')
$ep = $asm.GetType('Endless.Gameplay.Scripting.EndlessProp')

Write-Host "=== EndlessProp FIELDS ==="
foreach ($f in $ep.GetFields([System.Reflection.BindingFlags]'Public,NonPublic,Instance')) {
    Write-Host "  $($f.Name) : $($f.FieldType.Name)"
}

Write-Host ""
Write-Host "=== EndlessProp.Prop PROPERTY ==="
$propProp = $ep.GetProperty('Prop', [System.Reflection.BindingFlags]'Public,NonPublic,Instance')
if ($propProp) {
    Write-Host "  Type: $($propProp.PropertyType.Name)"
    Write-Host "  CanRead: $($propProp.CanRead)"
}

Write-Host ""
Write-Host "=== EndlessProp METHODS related to ReferenceFilter ==="
foreach ($m in $ep.GetMethods([System.Reflection.BindingFlags]'Public,NonPublic,Instance')) {
    if ($m.Name -match 'Reference' -or $m.Name -match 'Filter' -or $m.Name -match 'Calculate') {
        $params = $m.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }
        Write-Host "  $($m.ReturnType.Name) $($m.Name)($($params -join ', '))"
    }
}

Write-Host ""
Write-Host "=== CalculateReferenceFilter METHOD DETAILS ==="
$calcMethod = $ep.GetMethod('CalculateReferenceFilter', [System.Reflection.BindingFlags]'Public,NonPublic,Instance')
if ($calcMethod) {
    $body = $calcMethod.GetMethodBody()
    Write-Host "  IL Size: $($body.GetILAsByteArray().Length) bytes"
    Write-Host "  Local vars: $($body.LocalVariables.Count)"
    foreach ($v in $body.LocalVariables) {
        Write-Host "    Local[$($v.LocalIndex)]: $($v.LocalType.Name)"
    }
}
