$asm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Gameplay.dll')
$epType = $asm.GetType('Endless.Gameplay.Scripting.EndlessProp')

Write-Host "=== EndlessProp - ALL Setup Methods ===" -ForegroundColor Cyan

$epType.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance) | Where-Object {
    $_.Name -like '*Setup*'
} | ForEach-Object {
    Write-Host ""
    Write-Host "METHOD: $($_.Name)" -ForegroundColor Yellow
    Write-Host "  Return Type: $($_.ReturnType.FullName)"
    Write-Host "  Parameters:"
    $_.GetParameters() | ForEach-Object {
        Write-Host "    $($_.Position): $($_.ParameterType.FullName) $($_.Name)"
    }
}

Write-Host ""
Write-Host "=== EndlessProp.BuildPrefab ===" -ForegroundColor Cyan
$buildMethod = $epType.GetMethod('BuildPrefab', [System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
if ($buildMethod) {
    Write-Host "Return Type: $($buildMethod.ReturnType.FullName)"
    Write-Host "Parameters:"
    $buildMethod.GetParameters() | ForEach-Object {
        Write-Host "  $($_.Position): $($_.ParameterType.FullName) $($_.Name)"
    }
}

Write-Host ""
Write-Host "=== ComponentDefinition Structure ===" -ForegroundColor Cyan
$cdType = $asm.GetType('Endless.Gameplay.ComponentDefinition')
if ($cdType) {
    Write-Host "Fields:"
    $cdType.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance) | ForEach-Object {
        Write-Host "  $($_.FieldType.Name) $($_.Name)"
    }
}
