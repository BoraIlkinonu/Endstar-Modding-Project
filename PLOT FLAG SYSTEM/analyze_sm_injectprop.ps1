$asm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Gameplay.dll')

$smType = $asm.GetType('Endless.Gameplay.LevelEditing.Level.StageManager')

Write-Host "=== StageManager.InjectProp ===" -ForegroundColor Cyan
$method = $smType.GetMethod('InjectProp', [System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance)
if ($method) {
    Write-Host "Parameters:"
    $method.GetParameters() | ForEach-Object {
        "  $($_.Position): $($_.ParameterType.FullName) $($_.Name)"
    }

    $body = $method.GetMethodBody()
    if ($body) {
        Write-Host "`nLocal variables:"
        $body.LocalVariables | ForEach-Object {
            "  $($_.LocalIndex): $($_.LocalType.FullName)"
        }
    }
}

Write-Host "`n=== StageManager injectedProps field ===" -ForegroundColor Cyan
$field = $smType.GetField('injectedProps', [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
if ($field) {
    Write-Host "Type: $($field.FieldType.FullName)"
}

Write-Host "`n=== InjectedPropInfo (if exists) ===" -ForegroundColor Cyan
$ipiType = $asm.GetType('Endless.Gameplay.LevelEditing.Level.StageManager+InjectedPropInfo')
if ($ipiType) {
    Write-Host "Fields:"
    $ipiType.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance) | ForEach-Object {
        "  $($_.FieldType.Name) $($_.Name)"
    }
} else {
    Write-Host "InjectedPropInfo type not found"
}

# Check for nested types
Write-Host "`n=== StageManager Nested Types ===" -ForegroundColor Cyan
$smType.GetNestedTypes([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic) | ForEach-Object {
    Write-Host "  $($_.Name)"
    $_.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance) | ForEach-Object {
        "    $($_.FieldType.Name) $($_.Name)"
    }
}
