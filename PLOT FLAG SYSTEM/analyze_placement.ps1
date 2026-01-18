$asm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Gameplay.dll')

Write-Host "=== PropLibrary Methods (Build/Spawn/Create/Place) ===" -ForegroundColor Cyan
$type = $asm.GetType('Endless.Gameplay.LevelEditing.PropLibrary')
$type.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance) | Where-Object {
    $_.Name -like '*Build*' -or $_.Name -like '*Spawn*' -or $_.Name -like '*Create*' -or $_.Name -like '*Instantiate*' -or $_.Name -like '*Place*' -or $_.Name -like '*Get*Prefab*'
} | ForEach-Object {
    $params = $_.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }
    "$($_.ReturnType.Name) $($_.Name)($($params -join ', '))"
}

Write-Host "`n=== StageManager Methods (Build/Spawn/Create/Place) ===" -ForegroundColor Cyan
$smType = $asm.GetType('Endless.Gameplay.LevelEditing.Level.StageManager')
$smType.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance) | Where-Object {
    $_.Name -like '*Build*' -or $_.Name -like '*Spawn*' -or $_.Name -like '*Create*' -or $_.Name -like '*Instantiate*' -or $_.Name -like '*Place*' -or $_.Name -like '*Get*Prefab*'
} | ForEach-Object {
    $params = $_.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }
    "$($_.ReturnType.Name) $($_.Name)($($params -join ', '))"
}

Write-Host "`n=== PropLibrary Fields related to injected/prefab ===" -ForegroundColor Cyan
$type.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance) | Where-Object {
    $_.Name -like '*inject*' -or $_.Name -like '*prefab*' -or $_.Name -like '*missing*'
} | ForEach-Object {
    "$($_.FieldType.Name) $($_.Name)"
}
