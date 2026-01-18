$asm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Gameplay.dll')
$propsAsm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Props.dll')

# Check Prop class for baseTypeId field details
Write-Host "=== Prop.baseTypeId Field ===" -ForegroundColor Cyan
$propType = $propsAsm.GetType('Endless.Props.Assets.Prop')
$baseTypeIdField = $propType.GetField('baseTypeId', [System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
if ($baseTypeIdField) {
    Write-Host "Field Type: $($baseTypeIdField.FieldType.FullName)"
}

# Check all Prop fields for asset references
Write-Host "`n=== ALL Prop Fields ===" -ForegroundColor Cyan
$propType.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance) | ForEach-Object {
    Write-Host "  $($_.FieldType.Name) $($_.Name)"
}

# Check EndlessVisuals class
Write-Host "`n=== EndlessVisuals Structure ===" -ForegroundColor Cyan
$evType = $asm.GetType('Endless.Gameplay.Scripting.EndlessVisuals')
if ($evType) {
    Write-Host "Fields:" -ForegroundColor Yellow
    $evType.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance) | ForEach-Object {
        Write-Host "  $($_.FieldType.Name) $($_.Name)"
    }
    Write-Host "`nMethods:" -ForegroundColor Yellow
    $evType.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance) | Where-Object {
        $_.Name -like '*Load*' -or $_.Name -like '*Set*' -or $_.Name -like '*Visual*' -or $_.Name -like '*Build*' -or $_.Name -like '*Init*' -or $_.Name -like '*Add*'
    } | ForEach-Object {
        try {
            $params = ($_.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ', '
            Write-Host "  $($_.ReturnType.Name) $($_.Name)($params)"
        } catch {}
    }
}

# Look for PropBaseType class
Write-Host "`n=== Looking for BaseType related classes ===" -ForegroundColor Cyan
$propsAsm.GetTypes() | Where-Object { $_.Name -like '*BaseType*' -or $_.Name -like '*Base*Type*' } | ForEach-Object {
    Write-Host "Found: $($_.FullName)"
    Write-Host "  Fields:"
    $_.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance) | ForEach-Object {
        Write-Host "    $($_.FieldType.Name) $($_.Name)"
    }
}
