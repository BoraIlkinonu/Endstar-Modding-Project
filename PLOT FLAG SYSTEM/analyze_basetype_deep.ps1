$asm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Gameplay.dll')
$propsAsm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Props.dll')

# BaseTypeRequirement class
Write-Host "=== BaseTypeRequirement ===" -ForegroundColor Cyan
$btrType = $asm.GetType('Endless.Creator.LevelEditing.Runtime.BaseTypeRequirement')
if ($btrType) {
    Write-Host "Fields:"
    $btrType.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance) | ForEach-Object {
        Write-Host "  $($_.FieldType.Name) $($_.Name)"
    }
    Write-Host "Properties:"
    $btrType.GetProperties() | ForEach-Object {
        Write-Host "  $($_.PropertyType.Name) $($_.Name)"
    }
}

# BaseTypeList class
Write-Host "`n=== BaseTypeList ===" -ForegroundColor Cyan
$btlType = $asm.GetType('Endless.Gameplay.BaseTypeList')
if ($btlType) {
    Write-Host "Fields:"
    $btlType.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance) | ForEach-Object {
        Write-Host "  $($_.FieldType.Name) $($_.Name)"
    }
}

# Look for BaseType class in Props
Write-Host "`n=== BaseType (Props) ===" -ForegroundColor Cyan
$btType = $propsAsm.GetType('Endless.Props.Assets.BaseType')
if ($btType) {
    Write-Host "Fields:"
    $btType.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance) | ForEach-Object {
        Write-Host "  $($_.FieldType.Name) $($_.Name)"
    }
} else {
    # Search for it
    Write-Host "Searching for BaseType..."
    $propsAsm.GetTypes() | Where-Object { $_.Name -like '*BaseType*' } | ForEach-Object {
        Write-Host "Found: $($_.FullName)"
    }
}

# Check EndlessProp.SetupBaseType more carefully
Write-Host "`n=== EndlessProp.SetupBaseType signature ===" -ForegroundColor Cyan
$epType = $asm.GetType('Endless.Gameplay.Scripting.EndlessProp')
$setupMethod = $epType.GetMethod('SetupBaseType', [System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
if ($setupMethod) {
    Write-Host "Return: $($setupMethod.ReturnType.FullName)"
    Write-Host "Parameters:"
    foreach ($p in $setupMethod.GetParameters()) {
        Write-Host "  $($p.ParameterType.FullName) $($p.Name)"
    }
}

# Check what referenceList type is
Write-Host "`n=== Looking for what references get loaded ===" -ForegroundColor Cyan
$epType.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance) | Where-Object {
    $_.Name -like '*Setup*' -or $_.Name -like '*Build*' -or $_.Name -like '*Load*'
} | ForEach-Object {
    try {
        $params = ($_.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ', '
        Write-Host "  $($_.ReturnType.Name) $($_.Name)($params)"
    } catch {}
}
