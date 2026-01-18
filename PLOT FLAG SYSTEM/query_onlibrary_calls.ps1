# Check what OnLibraryRepopulated calls by analyzing its IL tokens
$asm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Creator.dll')
$t = $asm.GetType('Endless.Creator.UI.UIPropToolPanelView')
$method = $t.GetMethod('OnLibraryRepopulated', [System.Reflection.BindingFlags]'Public,NonPublic,Instance,DeclaredOnly')
if ($method) {
    $body = $method.GetMethodBody()
    Write-Host "Method: OnLibraryRepopulated"
    Write-Host "IL Size: $($body.GetILAsByteArray().Length) bytes"

    # Get the UIRuntimePropInfoListModel type and its Synchronize method
    $listModelType = $asm.GetType('Endless.Creator.UI.UIRuntimePropInfoListModel')
    if ($listModelType) {
        $syncMethod = $listModelType.GetMethod('Synchronize', [System.Reflection.BindingFlags]'Public,NonPublic,Instance')
        if ($syncMethod) {
            Write-Host "Synchronize method found on UIRuntimePropInfoListModel"
            Write-Host "Synchronize token: $($syncMethod.MetadataToken.ToString('X8'))"
        }
    }

    # Show IL as hex to check for call tokens
    $il = $body.GetILAsByteArray()
    $hexStr = ($il | ForEach-Object { $_.ToString("X2") }) -join " "
    Write-Host "IL bytes (first 100): $($hexStr.Substring(0, [Math]::Min(300, $hexStr.Length)))"
}
