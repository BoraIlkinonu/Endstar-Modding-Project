$asm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Creator.dll')
$t = $asm.GetType('Endless.Creator.UI.UIPropToolPanelView')
$t.GetMethods([System.Reflection.BindingFlags]'Public,NonPublic,Instance,DeclaredOnly') | ForEach-Object { $_.Name }
