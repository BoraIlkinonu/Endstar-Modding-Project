# Deep analysis of how Synchronize uses BaseTypeId
# Decode the exact IL flow to understand crash behavior

$creatorAsm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Creator.dll')
$uiModel = $creatorAsm.GetType('Endless.Creator.UI.UIRuntimePropInfoListModel')
$syncMethod = $uiModel.GetMethod('Synchronize', [System.Reflection.BindingFlags]'Public,NonPublic,Instance')

Write-Host "=== Full Synchronize IL Decode ===" -ForegroundColor Cyan

$body = $syncMethod.GetMethodBody()
$il = $body.GetILAsByteArray()

Write-Host "IL length: $($il.Length) bytes"
Write-Host "Local variables:"
foreach ($v in $body.LocalVariables) {
    Write-Host "  [$($v.LocalIndex)] $($v.LocalType.Name)"
}

Write-Host ""
Write-Host "=== Key IL sections around BaseTypeId access ===" -ForegroundColor Cyan

# Position 209-235 accesses PropData.BaseTypeId
Write-Host ""
Write-Host "Positions 205-245 (BaseTypeId handling):"

for ($i = 205; $i -lt [Math]::Min(245, $il.Length); $i++) {
    $op = $il[$i]
    $hex = $op.ToString('X2')

    switch ($op) {
        0x02 { Write-Host "[$i] ldarg.0 (this)" }
        0x03 { Write-Host "[$i] ldarg.1" }
        0x04 { Write-Host "[$i] ldarg.2" }
        0x06 { Write-Host "[$i] ldloc.0" }
        0x07 { Write-Host "[$i] ldloc.1" }
        0x08 { Write-Host "[$i] ldloc.2" }
        0x09 { Write-Host "[$i] ldloc.3" }
        0x11 {
            $local = $il[$i + 1]
            Write-Host "[$i] ldloc.s $local"
            $i++
        }
        0x13 {
            $local = $il[$i + 1]
            Write-Host "[$i] stloc.s $local"
            $i++
        }
        0x28 {
            $token = [BitConverter]::ToInt32($il, $i + 1)
            Write-Host "[$i] call 0x$($token.ToString('X8'))"
            $i += 4
        }
        0x6F {
            $token = [BitConverter]::ToInt32($il, $i + 1)
            Write-Host "[$i] callvirt 0x$($token.ToString('X8'))"
            $i += 4
        }
        0x7B {
            $token = [BitConverter]::ToInt32($il, $i + 1)
            Write-Host "[$i] ldfld 0x$($token.ToString('X8'))"
            $i += 4
        }
        0x2C {
            $offset = $il[$i + 1]
            Write-Host "[$i] brfalse.s +$offset (SKIP if null/false)"
            $i++
        }
        0x2D {
            $offset = $il[$i + 1]
            Write-Host "[$i] brtrue.s +$offset (SKIP if true)"
            $i++
        }
        0x8C {
            $token = [BitConverter]::ToInt32($il, $i + 1)
            Write-Host "[$i] box 0x$($token.ToString('X8'))"
            $i += 4
        }
        default { Write-Host "[$i] $hex" }
    }
}

Write-Host ""
Write-Host "=== Analysis ===" -ForegroundColor Cyan
Write-Host "Looking for null checks on PropData or BaseTypeId..."
Write-Host "If there's a brfalse after loading PropData, it handles null gracefully"
Write-Host "If no check, accessing null PropData would crash"
