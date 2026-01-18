$asm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Gameplay.dll')
$ep = $asm.GetType('Endless.Gameplay.Scripting.EndlessProp')

Write-Host "=== CalculateReferenceFilter Method Analysis ===" -ForegroundColor Cyan
$method = $ep.GetMethod('CalculateReferenceFilter', [System.Reflection.BindingFlags]'Public,NonPublic,Instance')
if ($method) {
    $params = $method.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }
    Write-Host "  Signature: void CalculateReferenceFilter($($params -join ', '))"

    $body = $method.GetMethodBody()
    $il = $body.GetILAsByteArray()
    Write-Host "  IL size: $($il.Length) bytes"
    Write-Host "  Local vars: $($body.LocalVariables.Count)"
    foreach ($v in $body.LocalVariables) {
        Write-Host "    [$($v.LocalIndex)] $($v.LocalType.FullName)"
    }
}

Write-Host ""
Write-Host "=== What determines ReferenceFilter value? ===" -ForegroundColor Cyan
# Search for BaseTypeDefinition and ComponentDefinition
$propsAsm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Props.dll')

# Find BaseTypeDefinition
Write-Host "Searching for BaseTypeDefinition..."
foreach ($t in $propsAsm.GetTypes()) {
    if ($t.Name -match 'BaseType' -or $t.Name -match 'Component') {
        Write-Host "  Found: $($t.FullName)"

        # Check if it has filter field
        $filterField = $t.GetField('filter', [System.Reflection.BindingFlags]'Public,NonPublic,Instance')
        if ($filterField) {
            Write-Host "    Has 'filter' field of type: $($filterField.FieldType.Name)"
        }

        $filterProp = $t.GetProperty('filter', [System.Reflection.BindingFlags]'Public,NonPublic,Instance')
        if (-not $filterProp) {
            $filterProp = $t.GetProperty('Filter', [System.Reflection.BindingFlags]'Public,NonPublic,Instance')
        }
        if ($filterProp) {
            Write-Host "    Has 'Filter' property of type: $($filterProp.PropertyType.Name)"
        }
    }
}

Write-Host ""
Write-Host "=== Analyzing CalculateReferenceFilter IL for constants ===" -ForegroundColor Cyan
if ($method) {
    $il = $method.GetMethodBody().GetILAsByteArray()
    $i = 0
    while ($i -lt $il.Length) {
        $opcode = $il[$i]
        switch ($opcode) {
            0x16 { Write-Host "[$i] ldc.i4.0 (None=0)"; $i++ }
            0x17 { Write-Host "[$i] ldc.i4.1 (NonStatic=1)"; $i++ }
            0x18 { Write-Host "[$i] ldc.i4.2 (Npc=2)"; $i++ }
            0x19 { Write-Host "[$i] ldc.i4.3"; $i++ }
            0x1A { Write-Host "[$i] ldc.i4.4 (PhysicsObject=4)"; $i++ }
            0x1B { Write-Host "[$i] ldc.i4.5"; $i++ }
            0x1C { Write-Host "[$i] ldc.i4.6"; $i++ }
            0x1D { Write-Host "[$i] ldc.i4.7"; $i++ }
            0x1E { Write-Host "[$i] ldc.i4.8 (InventoryItem=8)"; $i++ }
            0x1F { Write-Host "[$i] ldc.i4.s $($il[$i+1])"; $i += 2 }
            0x20 { $val = [BitConverter]::ToInt32($il, $i+1); Write-Host "[$i] ldc.i4 $val"; $i += 5 }
            0x60 { Write-Host "[$i] or (bitwise OR)"; $i++ }
            default { $i++ }
        }
    }
}
