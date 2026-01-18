# Research: What happens when PropData.BaseTypeId is empty/null?
# This could be the crash point in Synchronize

$propsAsm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Props.dll')
$prop = $propsAsm.GetType('Endless.Props.Assets.Prop')

Write-Host "=== Prop class fields ===" -ForegroundColor Cyan
foreach ($f in $prop.GetFields([System.Reflection.BindingFlags]'Public,NonPublic,Instance')) {
    Write-Host "  $($f.Name) : $($f.FieldType.Name)"
}

Write-Host ""
Write-Host "=== BaseTypeId property ===" -ForegroundColor Cyan
$baseTypeIdProp = $prop.GetProperty('BaseTypeId', [System.Reflection.BindingFlags]'Public,NonPublic,Instance')
if ($baseTypeIdProp) {
    Write-Host "Type: $($baseTypeIdProp.PropertyType.Name)"
    $getter = $baseTypeIdProp.GetGetMethod($true)
    if ($getter) {
        $body = $getter.GetMethodBody()
        $il = $body.GetILAsByteArray()
        Write-Host "Getter IL: $(($il | ForEach-Object { $_.ToString('X2') }) -join ' ')"
    }
}

Write-Host ""
Write-Host "=== Check what Synchronize does with BaseTypeId ===" -ForegroundColor Cyan

# Token 0x0A000728 in Synchronize - what is it?
# Let's check if PropLibrary has a method that takes baseTypeId
$gameplayAsm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Gameplay.dll')
$pl = $gameplayAsm.GetType('Endless.Gameplay.LevelEditing.PropLibrary')

Write-Host "PropLibrary methods that take string parameter:"
foreach ($m in $pl.GetMethods([System.Reflection.BindingFlags]'Public,NonPublic,Instance,Static')) {
    $params = $m.GetParameters()
    foreach ($p in $params) {
        if ($p.ParameterType.Name -eq 'String') {
            $allParams = $params | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }
            Write-Host "  $($m.ReturnType.Name) $($m.Name)($($allParams -join ', '))"
            break
        }
    }
}

Write-Host ""
Write-Host "=== GetBaseTypeList method (called from Synchronize?) ===" -ForegroundColor Cyan
$getBaseTypeList = $pl.GetMethod('GetBaseTypeList', [System.Reflection.BindingFlags]'Public,NonPublic,Instance', $null, @([string]), $null)
if ($getBaseTypeList) {
    $params = $getBaseTypeList.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }
    Write-Host "Signature: $($getBaseTypeList.ReturnType.Name) GetBaseTypeList($($params -join ', '))"

    $body = $getBaseTypeList.GetMethodBody()
    if ($body) {
        $il = $body.GetILAsByteArray()
        Write-Host "IL size: $($il.Length) bytes"
        Write-Host "IL: $(($il | ForEach-Object { $_.ToString('X2') }) -join ' ')"
    }
}

Write-Host ""
Write-Host "=== Check if there's null handling for BaseTypeId ===" -ForegroundColor Cyan
Write-Host "Looking at what happens when baseTypeId is null or empty string..."

# The key question: does any code check for null/empty baseTypeId before using it?
# Or does it blindly use it and crash?

$creatorAsm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Creator.dll')
$uiModel = $creatorAsm.GetType('Endless.Creator.UI.UIRuntimePropInfoListModel')
$syncMethod = $uiModel.GetMethod('Synchronize', [System.Reflection.BindingFlags]'Public,NonPublic,Instance')
$body = $syncMethod.GetMethodBody()

Write-Host "Synchronize local variables:"
foreach ($v in $body.LocalVariables) {
    Write-Host "  [$($v.LocalIndex)] $($v.LocalType.Name)"
}
