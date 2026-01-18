$asm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Gameplay.dll')
$pl = $asm.GetType('Endless.Gameplay.LevelEditing.PropLibrary')

Write-Host "=== PropLibrary Constructor IL Analysis ===" -ForegroundColor Cyan

$ctor = $pl.GetConstructors([System.Reflection.BindingFlags]'Public,NonPublic,Instance')[0]
$body = $ctor.GetMethodBody()
$il = $body.GetILAsByteArray()

Write-Host "Constructor IL size: $($il.Length) bytes"
Write-Host ""

# Common IL opcodes for array initialization:
# newarr = 0x8D (create new array)
# stelem = 0x9C/0xA0 (store element)
# ldc.i4.X = various (load constant int)

Write-Host "Looking for array constants and initialization..."
Write-Host ""

# Print all IL bytes with context
$i = 0
while ($i -lt $il.Length) {
    $opcode = $il[$i]

    switch ($opcode) {
        0x16 { Write-Host "[$i] ldc.i4.0 (push 0)"; $i++ }
        0x17 { Write-Host "[$i] ldc.i4.1 (push 1)"; $i++ }
        0x18 { Write-Host "[$i] ldc.i4.2 (push 2)"; $i++ }
        0x19 { Write-Host "[$i] ldc.i4.3 (push 3)"; $i++ }
        0x1A { Write-Host "[$i] ldc.i4.4 (push 4)"; $i++ }
        0x1B { Write-Host "[$i] ldc.i4.5 (push 5)"; $i++ }
        0x1C { Write-Host "[$i] ldc.i4.6 (push 6)"; $i++ }
        0x1D { Write-Host "[$i] ldc.i4.7 (push 7)"; $i++ }
        0x1E { Write-Host "[$i] ldc.i4.8 (push 8)"; $i++ }
        0x1F { Write-Host "[$i] ldc.i4.s $($il[$i+1]) (push $($il[$i+1]))"; $i += 2 }
        0x20 { $val = [BitConverter]::ToInt32($il, $i+1); Write-Host "[$i] ldc.i4 $val"; $i += 5 }
        0x8D { $token = [BitConverter]::ToInt32($il, $i+1); Write-Host "[$i] newarr (token 0x$($token.ToString('X8')))"; $i += 5 }
        0x9C { Write-Host "[$i] stelem.ref"; $i++ }
        0xA0 { Write-Host "[$i] stelem (store element)"; $i += 5 }
        0x9F { Write-Host "[$i] stelem.i4 (store int32)"; $i++ }
        0x7B { $token = [BitConverter]::ToInt32($il, $i+1); Write-Host "[$i] ldfld (token 0x$($token.ToString('X8')))"; $i += 5 }
        0x7D { $token = [BitConverter]::ToInt32($il, $i+1); Write-Host "[$i] stfld (token 0x$($token.ToString('X8')))"; $i += 5 }
        0x25 { Write-Host "[$i] dup"; $i++ }
        0x28 { $token = [BitConverter]::ToInt32($il, $i+1); Write-Host "[$i] call (token 0x$($token.ToString('X8')))"; $i += 5 }
        default { $i++ }
    }
}

Write-Host ""
Write-Host "=== Looking for stfld to dynamicFilters ===" -ForegroundColor Cyan
$dfField = $pl.GetField('dynamicFilters', [System.Reflection.BindingFlags]'Public,NonPublic,Instance')
Write-Host "dynamicFilters metadata token: 0x$($dfField.MetadataToken.ToString('X8'))"
