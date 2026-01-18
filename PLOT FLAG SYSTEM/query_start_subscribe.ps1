# Check that Start subscribes to OnPropsRepopulated
$asm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Creator.dll')
$t = $asm.GetType('Endless.Creator.UI.UIPropToolPanelView')
$method = $t.GetMethod('Start', [System.Reflection.BindingFlags]'Public,NonPublic,Instance,DeclaredOnly')
if ($method) {
    $body = $method.GetMethodBody()
    Write-Host "Method: Start"
    Write-Host "IL Size: $($body.GetILAsByteArray().Length) bytes"

    # Get CreatorManager and check for add_OnPropsRepopulated
    $cmAsm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Creator.dll')
    $cmType = $cmAsm.GetType('Endless.Creator.CreatorManager')
    $addMethod = $cmType.GetMethod('add_OnPropsRepopulated', [System.Reflection.BindingFlags]'Public,NonPublic,Instance')
    if ($addMethod) {
        Write-Host "add_OnPropsRepopulated token: $($addMethod.MetadataToken.ToString('X8'))"
    }

    # Show IL bytes
    $il = $body.GetILAsByteArray()
    $hexStr = ($il | ForEach-Object { $_.ToString("X2") }) -join " "
    Write-Host "Start IL (first 200 bytes): $($hexStr.Substring(0, [Math]::Min(600, $hexStr.Length)))"
}
