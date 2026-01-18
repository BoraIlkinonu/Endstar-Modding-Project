$asm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Gameplay.dll')

Write-Host "=== Checking if InventoryItem has special handling ===" -ForegroundColor Cyan

# Check EndlessProp for any InventoryItem-specific code
$ep = $asm.GetType('Endless.Gameplay.Scripting.EndlessProp')
Write-Host ""
Write-Host "EndlessProp methods mentioning Inventory:" -ForegroundColor Yellow
foreach ($m in $ep.GetMethods([System.Reflection.BindingFlags]'Public,NonPublic,Instance')) {
    if ($m.Name -match 'Inventory' -or $m.Name -match 'Item') {
        $params = $m.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }
        Write-Host "  $($m.ReturnType.Name) $($m.Name)($($params -join ', '))"
    }
}

# Check EndlessProp for special properties related to ReferenceFilter
Write-Host ""
Write-Host "EndlessProp properties:" -ForegroundColor Yellow
foreach ($p in $ep.GetProperties([System.Reflection.BindingFlags]'Public,NonPublic,Instance')) {
    Write-Host "  $($p.PropertyType.Name) $($p.Name) (Get=$($p.CanRead), Set=$($p.CanWrite))"
}

# Check PropLibrary for any InventoryItem-specific handling
$pl = $asm.GetType('Endless.Gameplay.LevelEditing.PropLibrary')
Write-Host ""
Write-Host "PropLibrary methods:" -ForegroundColor Yellow
foreach ($m in $pl.GetMethods([System.Reflection.BindingFlags]'Public,NonPublic,Instance')) {
    $params = $m.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }
    Write-Host "  $($m.ReturnType.Name) $($m.Name)($($params -join ', '))"
}

# Specifically check GetReferenceFilteredDefinitionList
Write-Host ""
Write-Host "=== GetReferenceFilteredDefinitionList IL Analysis ===" -ForegroundColor Cyan
$grfMethod = $pl.GetMethod('GetReferenceFilteredDefinitionList', [System.Reflection.BindingFlags]'Public,NonPublic,Instance')
if ($grfMethod) {
    $body = $grfMethod.GetMethodBody()
    $il = $body.GetILAsByteArray()
    Write-Host "  IL size: $($il.Length) bytes"

    # Dump all IL bytes to understand the logic
    Write-Host "  IL dump:"
    for ($i = 0; $i -lt $il.Length; $i++) {
        $hex = $il[$i].ToString('X2')
        Write-Host -NoNewline "$hex "
    }
    Write-Host ""
}
