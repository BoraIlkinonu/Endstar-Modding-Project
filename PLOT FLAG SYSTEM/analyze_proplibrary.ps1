$asm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Gameplay.dll')
$plType = $asm.GetType('Endless.Gameplay.LevelEditing.PropLibrary')

Write-Host "=== PropLibrary Methods (Build/Prefab/Create) ===" -ForegroundColor Cyan
$plType.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance) | Where-Object { $_.Name -like '*Build*' -or $_.Name -like '*Prefab*' -or $_.Name -like '*Create*' -or $_.Name -like '*Load*' } | ForEach-Object {
    $params = ($_.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ', '
    Write-Host "$($_.ReturnType.Name) $($_.Name)($params)"
}

Write-Host "`n=== RuntimePropInfo Structure ===" -ForegroundColor Cyan
$rpiType = $asm.GetType('Endless.Gameplay.LevelEditing.PropLibrary+RuntimePropInfo')
Write-Host "Fields:" -ForegroundColor Yellow
$rpiType.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance) | ForEach-Object {
    Write-Host "  $($_.FieldType.Name) $($_.Name)"
}

Write-Host "`n=== InjectProp Method Details ===" -ForegroundColor Cyan
$injectMethod = $plType.GetMethod('InjectProp', [System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance)
if ($injectMethod) {
    Write-Host "Parameters:"
    $injectMethod.GetParameters() | ForEach-Object {
        Write-Host "  $($_.Position): $($_.ParameterType.FullName) $($_.Name)"
    }
}

Write-Host "`n=== StageManager.InjectProp ===" -ForegroundColor Cyan
$smType = $asm.GetType('Endless.Gameplay.LevelEditing.Level.StageManager')
$smInject = $smType.GetMethod('InjectProp', [System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance)
if ($smInject) {
    Write-Host "Parameters:"
    $smInject.GetParameters() | ForEach-Object {
        Write-Host "  $($_.Position): $($_.ParameterType.FullName) $($_.Name)"
    }
}
