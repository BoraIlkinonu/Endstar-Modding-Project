$asm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Gameplay.dll')

Write-Host "=== RuntimePropInfo Structure ===" -ForegroundColor Cyan
$rpiType = $asm.GetType('Endless.Gameplay.LevelEditing.PropLibrary+RuntimePropInfo')
Write-Host "Fields:"
$rpiType.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance) | ForEach-Object {
    "  $($_.FieldType.Name) $($_.Name)"
}

Write-Host "`n=== PropLibrary.InjectProp IL Analysis ===" -ForegroundColor Cyan
$plType = $asm.GetType('Endless.Gameplay.LevelEditing.PropLibrary')
$injectMethod = $plType.GetMethod('InjectProp', [System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance)
if ($injectMethod) {
    Write-Host "Method: $($injectMethod.ReturnType.Name) $($injectMethod.Name)"
    Write-Host "Parameters:"
    $injectMethod.GetParameters() | ForEach-Object {
        "  $($_.Position): $($_.ParameterType.Name) $($_.Name)"
    }

    # Try to get method body info
    $body = $injectMethod.GetMethodBody()
    if ($body) {
        Write-Host "`nLocal variables:"
        $body.LocalVariables | ForEach-Object {
            "  $($_.LocalIndex): $($_.LocalType.Name)"
        }
    }
}

Write-Host "`n=== What calls SpawnPropPrefab? ===" -ForegroundColor Cyan
$plType.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance) | ForEach-Object {
    $params = $_.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }
    "$($_.ReturnType.Name) $($_.Name)($($params -join ', '))"
}
