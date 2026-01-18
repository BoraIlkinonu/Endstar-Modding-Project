$asm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Gameplay.dll')
$ep = $asm.GetType('Endless.Gameplay.Scripting.EndlessProp')

Write-Host "=== ReferenceFilter Property Setter Analysis ===" -ForegroundColor Cyan

$prop = $ep.GetProperty('ReferenceFilter', [System.Reflection.BindingFlags]'Public,NonPublic,Instance')
if ($prop) {
    Write-Host "CanWrite: $($prop.CanWrite)"

    $setter = $prop.GetSetMethod($true)
    if ($setter) {
        $body = $setter.GetMethodBody()
        $il = $body.GetILAsByteArray()
        Write-Host "Setter IL size: $($il.Length) bytes"

        Write-Host "IL dump:"
        for ($i = 0; $i -lt $il.Length; $i++) {
            $hex = $il[$i].ToString('X2')
            Write-Host -NoNewline "$hex "
        }
        Write-Host ""

        # Simple setter would be: ldarg.0, ldarg.1, stfld, ret = 8 bytes
        # If it's longer, there's additional logic
        if ($il.Length > 10) {
            Write-Host "WARNING: Setter has additional logic beyond simple field assignment!"
        } else {
            Write-Host "Setter is simple field assignment"
        }
    }
}

Write-Host ""
Write-Host "=== Check for backing field ===" -ForegroundColor Cyan
$backingField = $ep.GetField('<ReferenceFilter>k__BackingField', [System.Reflection.BindingFlags]'NonPublic,Instance')
if ($backingField) {
    Write-Host "Found backing field: <ReferenceFilter>k__BackingField"
    Write-Host "Type: $($backingField.FieldType.Name)"
}

Write-Host ""
Write-Host "=== Analyze if setting value 8 could cause issues ===" -ForegroundColor Cyan
$rf = $asm.GetType('Endless.Gameplay.ReferenceFilter')
$val8 = [Enum]::ToObject($rf, 8)
Write-Host "Value 8 corresponds to: $val8"
Write-Host "Is defined: $([Enum]::IsDefined($rf, 8))"
