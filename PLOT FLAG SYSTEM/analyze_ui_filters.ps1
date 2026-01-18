$creatorAsm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Creator.dll')

Write-Host "=== UIPropToolPanelView Analysis ===" -ForegroundColor Cyan
$uiPanel = $creatorAsm.GetType('Endless.Creator.UI.UIPropToolPanelView')
if ($uiPanel) {
    Write-Host "  Found UIPropToolPanelView"

    # Look for filter-related fields
    foreach ($f in $uiPanel.GetFields([System.Reflection.BindingFlags]'Public,NonPublic,Instance')) {
        if ($f.Name -match 'filter|Filter|category|Category') {
            Write-Host "  Field: $($f.Name) : $($f.FieldType.Name)"
        }
    }

    # Look for filter-related methods
    Write-Host ""
    Write-Host "  Methods related to filter/category:"
    foreach ($m in $uiPanel.GetMethods([System.Reflection.BindingFlags]'Public,NonPublic,Instance')) {
        if ($m.Name -match 'filter|Filter|category|Category|Synchronize|Refresh') {
            $params = $m.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }
            Write-Host "    $($m.ReturnType.Name) $($m.Name)($($params -join ', '))"
        }
    }
}

Write-Host ""
Write-Host "=== Looking for filter categories in UI ===" -ForegroundColor Cyan

# Search for any type that might contain filter categories
foreach ($t in $creatorAsm.GetTypes()) {
    if ($t.Name -match 'PropTool' -or $t.Name -match 'Category') {
        foreach ($f in $t.GetFields([System.Reflection.BindingFlags]'Public,NonPublic,Instance,Static')) {
            if ($f.FieldType.IsArray -and $f.FieldType.Name -match 'ReferenceFilter') {
                Write-Host "  Found on $($t.Name): $($f.Name)"
            }
        }
    }
}

Write-Host ""
Write-Host "=== Base class UIItemSelectionToolPanelView ===" -ForegroundColor Cyan
if ($uiPanel) {
    $baseType = $uiPanel.BaseType
    Write-Host "  Base: $($baseType.FullName)"

    if ($baseType) {
        foreach ($f in $baseType.GetFields([System.Reflection.BindingFlags]'Public,NonPublic,Instance')) {
            if ($f.Name -match 'filter|Filter|category' -or $f.FieldType.Name -match 'ReferenceFilter') {
                Write-Host "  Field: $($f.Name) : $($f.FieldType.Name)"
            }
        }
    }
}

Write-Host ""
Write-Host "=== PropLibrary.ReferenceFilterMap property ===" -ForegroundColor Cyan
$gameplayAsm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Gameplay.dll')
$pl = $gameplayAsm.GetType('Endless.Gameplay.LevelEditing.PropLibrary')
$rfmProp = $pl.GetProperty('ReferenceFilterMap', [System.Reflection.BindingFlags]'Public,NonPublic,Instance')
if ($rfmProp) {
    Write-Host "  Return type: $($rfmProp.PropertyType.FullName)"
} else {
    Write-Host "  NOT FOUND as property, checking as field getter..."
    $getter = $pl.GetMethod('get_ReferenceFilterMap', [System.Reflection.BindingFlags]'Public,NonPublic,Instance')
    if ($getter) {
        Write-Host "  Found getter: $($getter.ReturnType.FullName)"
    }
}
