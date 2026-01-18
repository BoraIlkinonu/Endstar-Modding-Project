$asm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Creator.dll')
$t = $asm.GetType('Endless.Creator.UI.UIPropToolPanelView')
$method = $t.GetMethod('Start', [System.Reflection.BindingFlags]'Public,NonPublic,Instance,DeclaredOnly')
if ($method) {
    $body = $method.GetMethodBody()
    Write-Host "Method: Start"
    Write-Host "IL Size: $($body.GetILAsByteArray().Length) bytes"
    Write-Host "Local vars: $($body.LocalVariables.Count)"
} else {
    Write-Host "Start method not found"
}
