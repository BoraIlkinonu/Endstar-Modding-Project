$asm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Gameplay.dll')
$pl = $asm.GetType('Endless.Gameplay.LevelEditing.PropLibrary')

Write-Host "=== PropLibrary Type Check ===" -ForegroundColor Cyan
if ($pl) {
    Write-Host "  PropLibrary found!"
} else {
    Write-Host "  PropLibrary NOT found, listing types..."
    foreach ($t in $asm.GetTypes()) {
        if ($t.Name -match 'PropLibrary') {
            Write-Host "  Found: $($t.FullName)"
            $pl = $t
        }
    }
}

Write-Host ""
Write-Host "=== PropLibrary Fields containing 'filter' or 'dynamic' ===" -ForegroundColor Cyan
foreach ($f in $pl.GetFields([System.Reflection.BindingFlags]'Public,NonPublic,Instance,Static')) {
    if ($f.Name -match 'filter|dynamic|Filter|Dynamic' -or $f.FieldType.Name -match 'ReferenceFilter') {
        Write-Host "  $($f.Name) : $($f.FieldType.FullName) (Static=$($f.IsStatic))"

        # If it's a static array, try to get initial values
        if ($f.IsStatic -and $f.FieldType.IsArray) {
            Write-Host "    Checking for default values..."
        }
    }
}

Write-Host ""
Write-Host "=== PopulateReferenceFilterMap IL Analysis ===" -ForegroundColor Cyan
$method = $pl.GetMethod('PopulateReferenceFilterMap', [System.Reflection.BindingFlags]'Public,NonPublic,Instance')
if ($method) {
    $body = $method.GetMethodBody()
    $il = $body.GetILAsByteArray()
    Write-Host "  IL bytes: $($il.Length)"

    # Look for field references in IL (ldsfld/ldfld = 0x7E/0x7B)
    Write-Host "  Looking for field references..."
    for ($i = 0; $i -lt $il.Length; $i++) {
        if ($il[$i] -eq 0x7E -or $il[$i] -eq 0x7B) {
            $opcode = if ($il[$i] -eq 0x7E) { "ldsfld" } else { "ldfld" }
            # Token is next 4 bytes (little-endian)
            if ($i + 4 -lt $il.Length) {
                $token = [BitConverter]::ToInt32($il, $i + 1)
                Write-Host "    [$i] $opcode token=0x$($token.ToString('X8'))"
            }
        }
    }
}

Write-Host ""
Write-Host "=== ReferenceFilter Enum Values ===" -ForegroundColor Cyan
$rf = $asm.GetType('Endless.Gameplay.ReferenceFilter')
if ($rf) {
    Write-Host "  Found: $($rf.FullName)"
    foreach ($val in [System.Enum]::GetValues($rf)) {
        Write-Host "    $val = $([int]$val)"
    }
} else {
    # Search all types
    foreach ($t in $asm.GetTypes()) {
        if ($t.Name -eq 'ReferenceFilter') {
            Write-Host "  Found: $($t.FullName)"
            foreach ($val in [System.Enum]::GetValues($t)) {
                Write-Host "    $val = $([int]$val)"
            }
        }
    }
}
