# Research: RuntimePropInfo.GetBaseTypeDefinition and GetComponentDefinitions
# These might crash when accessing data that doesn't exist for custom props

$gameplayAsm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Gameplay.dll')
$rpi = $gameplayAsm.GetType('Endless.Gameplay.LevelEditing.PropLibrary+RuntimePropInfo')

Write-Host "=== GetBaseTypeDefinition IL Analysis ===" -ForegroundColor Cyan
$method = $rpi.GetMethod('GetBaseTypeDefinition', [System.Reflection.BindingFlags]'Public,NonPublic,Instance')
if ($method) {
    Write-Host "Return type: $($method.ReturnType.Name)"

    $body = $method.GetMethodBody()
    if ($body) {
        $il = $body.GetILAsByteArray()
        Write-Host "IL size: $($il.Length) bytes"
        Write-Host "Local variables:"
        foreach ($v in $body.LocalVariables) {
            Write-Host "  [$($v.LocalIndex)] $($v.LocalType.Name)"
        }

        Write-Host "IL bytes:"
        $hex = ($il | ForEach-Object { $_.ToString('X2') }) -join ' '
        Write-Host $hex
    }
}

Write-Host ""
Write-Host "=== GetComponentDefinitions IL Analysis ===" -ForegroundColor Cyan
$method2 = $rpi.GetMethod('GetComponentDefinitions', [System.Reflection.BindingFlags]'Public,NonPublic,Instance')
if ($method2) {
    Write-Host "Return type: $($method2.ReturnType.Name)"

    $body = $method2.GetMethodBody()
    if ($body) {
        $il = $body.GetILAsByteArray()
        Write-Host "IL size: $($il.Length) bytes"
        Write-Host "Local variables:"
        foreach ($v in $body.LocalVariables) {
            Write-Host "  [$($v.LocalIndex)] $($v.LocalType.Name)"
        }

        Write-Host "IL bytes:"
        $hex = ($il | ForEach-Object { $_.ToString('X2') }) -join ' '
        Write-Host $hex
    }
}

Write-Host ""
Write-Host "=== GetAllDefinitions IL Analysis ===" -ForegroundColor Cyan
$method3 = $rpi.GetMethod('GetAllDefinitions', [System.Reflection.BindingFlags]'Public,NonPublic,Instance')
if ($method3) {
    Write-Host "Return type: $($method3.ReturnType.Name)"

    $body = $method3.GetMethodBody()
    if ($body) {
        $il = $body.GetILAsByteArray()
        Write-Host "IL size: $($il.Length) bytes"
        Write-Host "IL bytes:"
        $hex = ($il | ForEach-Object { $_.ToString('X2') }) -join ' '
        Write-Host $hex
    }
}

Write-Host ""
Write-Host "=== Check what Synchronize calls on RuntimePropInfo ===" -ForegroundColor Cyan
Write-Host "From earlier IL analysis, Synchronize accesses:"
Write-Host "  - PropData field (to get AssetReference/BaseTypeId)"
Write-Host "  - IsLoading property"
Write-Host "  - Possibly GetBaseTypeDefinition() for filtering"

Write-Host ""
Write-Host "=== Check PropData.BaseTypeId access ===" -ForegroundColor Cyan
$propsAsm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Props.dll')
$prop = $propsAsm.GetType('Endless.Props.Assets.Prop')
$baseTypeIdField = $prop.GetField('baseTypeId', [System.Reflection.BindingFlags]'NonPublic,Instance')
if ($baseTypeIdField) {
    Write-Host "baseTypeId field type: $($baseTypeIdField.FieldType.Name)"
}

$baseTypeIdProp = $prop.GetProperty('BaseTypeId', [System.Reflection.BindingFlags]'Public,Instance')
if ($baseTypeIdProp) {
    Write-Host "BaseTypeId property type: $($baseTypeIdProp.PropertyType.Name)"
    $getter = $baseTypeIdProp.GetGetMethod()
    if ($getter) {
        $body = $getter.GetMethodBody()
        $il = $body.GetILAsByteArray()
        Write-Host "Getter IL size: $($il.Length) bytes"
    }
}
