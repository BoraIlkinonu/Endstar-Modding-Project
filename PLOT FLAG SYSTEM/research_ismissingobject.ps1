# Research: What does IsMissingObject actually do?
# Target: RuntimePropInfo.IsMissingObject property

$asm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Gameplay.dll')
$rpi = $asm.GetType('Endless.Gameplay.LevelEditing.PropLibrary+RuntimePropInfo')

Write-Host "=== RuntimePropInfo.IsMissingObject Property ===" -ForegroundColor Cyan

$prop = $rpi.GetProperty('IsMissingObject', [System.Reflection.BindingFlags]'Public,NonPublic,Instance')
if ($prop) {
    Write-Host "Found property: IsMissingObject"
    Write-Host "  Type: $($prop.PropertyType.Name)"
    Write-Host "  CanRead: $($prop.CanRead)"
    Write-Host "  CanWrite: $($prop.CanWrite)"

    $getter = $prop.GetGetMethod($true)
    if ($getter) {
        $body = $getter.GetMethodBody()
        $il = $body.GetILAsByteArray()
        Write-Host "  Getter IL size: $($il.Length) bytes"
        Write-Host "  Getter IL: $($il | ForEach-Object { $_.ToString('X2') }) -join ' ')"
    }
}

Write-Host ""
Write-Host "=== Search for code that READS IsMissingObject ===" -ForegroundColor Cyan

# Check PropLibrary methods that might use IsMissingObject
$pl = $asm.GetType('Endless.Gameplay.LevelEditing.PropLibrary')
foreach ($m in $pl.GetMethods([System.Reflection.BindingFlags]'Public,NonPublic,Instance,DeclaredOnly')) {
    $body = $m.GetMethodBody()
    if ($body) {
        $il = $body.GetILAsByteArray()
        # Look for calls to get_IsMissingObject (would reference the property getter token)
        if ($il.Length -gt 20) {
            # Just report methods with substantial IL that might use it
            Write-Host "  $($m.Name) - IL size: $($il.Length) bytes"
        }
    }
}

Write-Host ""
Write-Host "=== Check PopulateReferenceFilterMap for IsMissingObject handling ===" -ForegroundColor Cyan
$populateMethod = $pl.GetMethod('PopulateReferenceFilterMap', [System.Reflection.BindingFlags]'Public,NonPublic,Instance')
if ($populateMethod) {
    $body = $populateMethod.GetMethodBody()
    $il = $body.GetILAsByteArray()
    Write-Host "PopulateReferenceFilterMap IL size: $($il.Length) bytes"
    Write-Host "Local variables:"
    foreach ($v in $body.LocalVariables) {
        Write-Host "  [$($v.LocalIndex)] $($v.LocalType.Name)"
    }

    # Dump full IL for analysis
    Write-Host ""
    Write-Host "Full IL bytes:"
    $hex = ($il | ForEach-Object { $_.ToString('X2') }) -join ' '
    Write-Host $hex
}

Write-Host ""
Write-Host "=== Check Creator.dll for IsMissingObject usage ===" -ForegroundColor Cyan
$creatorAsm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Creator.dll')

# Check UIRuntimePropInfoListModel
$uiModel = $creatorAsm.GetType('Endless.Creator.UI.UIRuntimePropInfoListModel')
if ($uiModel) {
    Write-Host "UIRuntimePropInfoListModel methods:"
    foreach ($m in $uiModel.GetMethods([System.Reflection.BindingFlags]'Public,NonPublic,Instance,DeclaredOnly')) {
        $params = $m.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }
        Write-Host "  $($m.ReturnType.Name) $($m.Name)($($params -join ', '))"
    }
}
