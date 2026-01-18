$asm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Gameplay.dll')

Write-Host "=== Searching for PropLibrary in Gameplay.dll ===" -ForegroundColor Cyan
try {
    $types = $asm.GetTypes()
    foreach ($t in $types) {
        if ($t.Name -match 'PropLibrary') {
            Write-Host "  Found: $($t.FullName)"
        }
    }
} catch {
    Write-Host "GetTypes failed, using exported types..."
    foreach ($t in $asm.GetExportedTypes()) {
        if ($t.Name -match 'PropLibrary') {
            Write-Host "  Found: $($t.FullName)"
        }
    }
}

Write-Host ""
Write-Host "=== Searching for types with _referenceFilterMap ===" -ForegroundColor Cyan
try {
    foreach ($t in $asm.GetExportedTypes()) {
        $field = $t.GetField('_referenceFilterMap', [System.Reflection.BindingFlags]'Public,NonPublic,Instance,Static')
        if ($field) {
            Write-Host "  Found on: $($t.FullName)"
            Write-Host "  Field Type: $($field.FieldType.FullName)"
        }
    }
} catch {
    Write-Host "  Error: $_"
}

Write-Host ""
Write-Host "=== Direct type lookup attempts ===" -ForegroundColor Cyan
$namespaces = @(
    'Endless.Gameplay.PropLibrary',
    'Endless.PropLibrary',
    'Endless.Gameplay.Scripting.PropLibrary',
    'Endless.Creator.PropLibrary',
    'PropLibrary'
)
foreach ($ns in $namespaces) {
    $t = $asm.GetType($ns)
    if ($t) {
        Write-Host "  SUCCESS: $ns"
    }
}

Write-Host ""
Write-Host "=== All public types in Gameplay.dll ===" -ForegroundColor Cyan
try {
    foreach ($t in $asm.GetExportedTypes()) {
        if ($t.Name -match 'Prop' -or $t.Name -match 'Library' -or $t.Name -match 'Stage') {
            Write-Host "  $($t.FullName)"
        }
    }
} catch {
    Write-Host "  Error listing types"
}
