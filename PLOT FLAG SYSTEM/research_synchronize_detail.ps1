# Deep analysis of UIRuntimePropInfoListModel.Synchronize
# Looking for what could crash with our custom prop

$creatorAsm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Creator.dll')
$uiModel = $creatorAsm.GetType('Endless.Creator.UI.UIRuntimePropInfoListModel')

Write-Host "=== Synchronize Method Detail ===" -ForegroundColor Cyan

$syncMethod = $uiModel.GetMethod('Synchronize', [System.Reflection.BindingFlags]'Public,NonPublic,Instance')
$body = $syncMethod.GetMethodBody()
$il = $body.GetILAsByteArray()

Write-Host "IL Analysis - Looking for potential crash points:"
Write-Host ""

# Decode key IL sequences
$i = 0
while ($i -lt $il.Length) {
    $op = $il[$i]

    # Look for callvirt (6F) and call (28) instructions
    if ($op -eq 0x6F -or $op -eq 0x28) {
        $token = [BitConverter]::ToInt32($il, $i + 1)
        $opname = if ($op -eq 0x6F) { "callvirt" } else { "call" }
        Write-Host "[$i] $opname token 0x$($token.ToString('X8'))"
        $i += 5
    }
    # Look for ldfld (7B)
    elseif ($op -eq 0x7B) {
        $token = [BitConverter]::ToInt32($il, $i + 1)
        Write-Host "[$i] ldfld token 0x$($token.ToString('X8'))"
        $i += 5
    }
    # Look for branch instructions that might indicate null checks
    elseif ($op -eq 0x2C -or $op -eq 0x2D) {
        $offset = $il[$i + 1]
        $opname = if ($op -eq 0x2C) { "brfalse.s" } else { "brtrue.s" }
        Write-Host "[$i] $opname +$offset (null/condition check)"
        $i += 2
    }
    else {
        $i++
    }
}

Write-Host ""
Write-Host "=== Check if IsMissingObject is accessed in Synchronize ===" -ForegroundColor Cyan

# IsMissingObject backing field token in RuntimePropInfo
$gameplayAsm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Gameplay.dll')
$rpi = $gameplayAsm.GetType('Endless.Gameplay.LevelEditing.PropLibrary+RuntimePropInfo')
$isMissingField = $rpi.GetField('<IsMissingObject>k__BackingField', [System.Reflection.BindingFlags]'NonPublic,Instance')
if ($isMissingField) {
    Write-Host "IsMissingObject backing field token: 0x$($isMissingField.MetadataToken.ToString('X8'))"
}

$isMissingProp = $rpi.GetProperty('IsMissingObject', [System.Reflection.BindingFlags]'Public,Instance')
if ($isMissingProp) {
    $getter = $isMissingProp.GetGetMethod()
    if ($getter) {
        Write-Host "IsMissingObject getter token: 0x$($getter.MetadataToken.ToString('X8'))"
    }
}

Write-Host ""
Write-Host "=== Analyze what happens when prop is added to UI list ===" -ForegroundColor Cyan

# The Add method on the base class
$baseType = $uiModel.BaseType
if ($baseType) {
    $addMethod = $baseType.GetMethod('Add', [System.Reflection.BindingFlags]'Public,NonPublic,Instance')
    if ($addMethod) {
        $params = $addMethod.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }
        Write-Host "Add method: $($addMethod.ReturnType.Name) Add($($params -join ', '))"

        $body = $addMethod.GetMethodBody()
        if ($body) {
            Write-Host "IL size: $($body.GetILAsByteArray().Length) bytes"
        }
    }
}
