# Research: What happens when prop tool opens?
# What methods are called that could crash?

$creatorAsm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Creator.dll')

Write-Host "=== UIPropToolPanelView Start/Initialize methods ===" -ForegroundColor Cyan

$uiPanel = $creatorAsm.GetType('Endless.Creator.UI.UIPropToolPanelView')
if ($uiPanel) {
    $startMethod = $uiPanel.GetMethod('Start', [System.Reflection.BindingFlags]'Public,NonPublic,Instance')
    if ($startMethod) {
        $body = $startMethod.GetMethodBody()
        if ($body) {
            Write-Host "Start() IL size: $($body.GetILAsByteArray().Length) bytes"
            Write-Host "Local variables:"
            foreach ($v in $body.LocalVariables) {
                Write-Host "  [$($v.LocalIndex)] $($v.LocalType.Name)"
            }
        }
    }

    # Check OnToolChange - called when switching to prop tool
    $onToolChange = $uiPanel.GetMethod('OnToolChange', [System.Reflection.BindingFlags]'Public,NonPublic,Instance')
    if ($onToolChange) {
        $body = $onToolChange.GetMethodBody()
        if ($body) {
            Write-Host ""
            Write-Host "OnToolChange() IL size: $($body.GetILAsByteArray().Length) bytes"

            # Decode key calls
            $il = $body.GetILAsByteArray()
            Write-Host "Key method calls:"
            for ($i = 0; $i -lt $il.Length; $i++) {
                if ($il[$i] -eq 0x28 -or $il[$i] -eq 0x6F) {
                    $opname = if ($il[$i] -eq 0x28) { "call" } else { "callvirt" }
                    $token = [BitConverter]::ToInt32($il, $i + 1)
                    Write-Host "  [$i] $opname 0x$($token.ToString('X8'))"
                    $i += 4
                }
            }
        }
    }

    # Check OnLibraryRepopulated
    $onRepop = $uiPanel.GetMethod('OnLibraryRepopulated', [System.Reflection.BindingFlags]'Public,NonPublic,Instance')
    if ($onRepop) {
        $body = $onRepop.GetMethodBody()
        if ($body) {
            Write-Host ""
            Write-Host "OnLibraryRepopulated() IL size: $($body.GetILAsByteArray().Length) bytes"
        }
    }
}

Write-Host ""
Write-Host "=== Check UIRuntimePropInfoListModel initialization ===" -ForegroundColor Cyan

$uiModel = $creatorAsm.GetType('Endless.Creator.UI.UIRuntimePropInfoListModel')
if ($uiModel) {
    # Check constructor
    foreach ($ctor in $uiModel.GetConstructors([System.Reflection.BindingFlags]'Public,NonPublic,Instance')) {
        $body = $ctor.GetMethodBody()
        if ($body) {
            Write-Host "Constructor IL size: $($body.GetILAsByteArray().Length) bytes"
        }
    }
}

Write-Host ""
Write-Host "=== Check PropTool class ===" -ForegroundColor Cyan

$propTool = $creatorAsm.GetType('Endless.Creator.PropTool')
if ($propTool) {
    Write-Host "Found PropTool"
    foreach ($m in $propTool.GetMethods([System.Reflection.BindingFlags]'Public,NonPublic,Instance,DeclaredOnly')) {
        $params = $m.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }
        Write-Host "  $($m.ReturnType.Name) $($m.Name)($($params -join ', '))"
    }
}
