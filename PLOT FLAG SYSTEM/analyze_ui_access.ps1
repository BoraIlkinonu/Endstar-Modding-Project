$asm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Gameplay.dll')

Write-Host "=== RuntimePropInfo - What UI might access ===" -ForegroundColor Cyan
$rpi = $asm.GetType('Endless.Gameplay.LevelEditing.PropLibrary+RuntimePropInfo')

Write-Host "Fields:"
foreach ($f in $rpi.GetFields([System.Reflection.BindingFlags]'Public,NonPublic,Instance')) {
    Write-Host "  $($f.Name) : $($f.FieldType.Name)"
}

Write-Host ""
Write-Host "Properties:"
foreach ($p in $rpi.GetProperties([System.Reflection.BindingFlags]'Public,NonPublic,Instance')) {
    Write-Host "  $($p.PropertyType.Name) $($p.Name) (Get=$($p.CanRead))"

    # Check getter for null checks
    $getter = $p.GetGetMethod($true)
    if ($getter) {
        $body = $getter.GetMethodBody()
        if ($body) {
            $il = $body.GetILAsByteArray()
            Write-Host "    Getter IL size: $($il.Length) bytes"
        }
    }
}

Write-Host ""
Write-Host "=== EndlessProp - Properties that might be accessed ===" -ForegroundColor Cyan
$ep = $asm.GetType('Endless.Gameplay.Scripting.EndlessProp')

# Check for potentially null properties
$propsToCheck = @('ScriptComponent', 'WorldObject', 'TransformMap', 'Prop')
foreach ($propName in $propsToCheck) {
    $prop = $ep.GetProperty($propName, [System.Reflection.BindingFlags]'Public,NonPublic,Instance')
    if ($prop) {
        $getter = $prop.GetGetMethod($true)
        if ($getter) {
            $body = $getter.GetMethodBody()
            if ($body) {
                $il = $body.GetILAsByteArray()
                Write-Host "$propName getter IL: $($il.Length) bytes"

                # Check for null checks (IL for null check includes brfalse/brtrue)
                $hasNullCheck = $false
                for ($i = 0; $i -lt $il.Length; $i++) {
                    if ($il[$i] -eq 0x2C -or $il[$i] -eq 0x2D -or $il[$i] -eq 0x39 -or $il[$i] -eq 0x3A) {
                        $hasNullCheck = $true
                        break
                    }
                }
                Write-Host "  Has null/condition check: $hasNullCheck"
            }
        }
    }
}

Write-Host ""
Write-Host "=== Check UI code for what it accesses ===" -ForegroundColor Cyan
$creatorAsm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Creator.dll')
$uiModel = $creatorAsm.GetType('Endless.Creator.UI.UIRuntimePropInfoListModel')
if ($uiModel) {
    Write-Host "UIRuntimePropInfoListModel methods:"
    foreach ($m in $uiModel.GetMethods([System.Reflection.BindingFlags]'Public,NonPublic,Instance,DeclaredOnly')) {
        $params = $m.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }
        Write-Host "  $($m.ReturnType.Name) $($m.Name)($($params -join ', '))"
    }
}
