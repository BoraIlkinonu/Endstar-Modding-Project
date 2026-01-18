# Research: What happens when UI displays props in a filter category?
# Focus: Why does ReferenceFilter=8 break prop tool but ReferenceFilter=0 doesn't?

$creatorAsm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Creator.dll')

Write-Host "=== UIPropToolPanelView - How it displays props ===" -ForegroundColor Cyan
$uiPanel = $creatorAsm.GetType('Endless.Creator.UI.UIPropToolPanelView')
if ($uiPanel) {
    Write-Host "Methods:"
    foreach ($m in $uiPanel.GetMethods([System.Reflection.BindingFlags]'Public,NonPublic,Instance,DeclaredOnly')) {
        $params = $m.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }
        Write-Host "  $($m.ReturnType.Name) $($m.Name)($($params -join ', '))"
    }

    Write-Host ""
    Write-Host "Fields:"
    foreach ($f in $uiPanel.GetFields([System.Reflection.BindingFlags]'Public,NonPublic,Instance')) {
        Write-Host "  $($f.Name) : $($f.FieldType.Name)"
    }
}

Write-Host ""
Write-Host "=== UIRuntimePropInfoListModel.Synchronize IL Analysis ===" -ForegroundColor Cyan
$uiModel = $creatorAsm.GetType('Endless.Creator.UI.UIRuntimePropInfoListModel')
if ($uiModel) {
    $syncMethod = $uiModel.GetMethod('Synchronize', [System.Reflection.BindingFlags]'Public,NonPublic,Instance')
    if ($syncMethod) {
        $params = $syncMethod.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }
        Write-Host "Signature: Void Synchronize($($params -join ', '))"

        $body = $syncMethod.GetMethodBody()
        if ($body) {
            $il = $body.GetILAsByteArray()
            Write-Host "IL size: $($il.Length) bytes"
            Write-Host "Local variables: $($body.LocalVariables.Count)"
            foreach ($v in $body.LocalVariables) {
                Write-Host "  [$($v.LocalIndex)] $($v.LocalType.FullName)"
            }

            Write-Host ""
            Write-Host "IL bytes:"
            $hex = ($il | ForEach-Object { $_.ToString('X2') }) -join ' '
            Write-Host $hex
        }
    }
}

Write-Host ""
Write-Host "=== Base class: UIBaseLocalFilterableListModel ===" -ForegroundColor Cyan
if ($uiModel) {
    $baseType = $uiModel.BaseType
    Write-Host "Base: $($baseType.FullName)"

    if ($baseType) {
        Write-Host "Base methods that might filter/display:"
        foreach ($m in $baseType.GetMethods([System.Reflection.BindingFlags]'Public,NonPublic,Instance,DeclaredOnly')) {
            if ($m.Name -match 'Filter|Display|Show|Add|Remove|Clear|Sync|Update') {
                $params = $m.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }
                Write-Host "  $($m.ReturnType.Name) $($m.Name)($($params -join ', '))"
            }
        }
    }
}

Write-Host ""
Write-Host "=== Check if RuntimePropInfo has any display-related methods ===" -ForegroundColor Cyan
$gameplayAsm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Gameplay.dll')
$rpi = $gameplayAsm.GetType('Endless.Gameplay.LevelEditing.PropLibrary+RuntimePropInfo')
Write-Host "RuntimePropInfo methods:"
foreach ($m in $rpi.GetMethods([System.Reflection.BindingFlags]'Public,NonPublic,Instance,DeclaredOnly')) {
    $params = $m.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }
    Write-Host "  $($m.ReturnType.Name) $($m.Name)($($params -join ', '))"
}
