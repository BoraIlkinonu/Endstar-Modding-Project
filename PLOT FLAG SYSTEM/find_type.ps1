$asm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Gameplay.dll')
$asm.GetTypes() | Where-Object { $_.Name -like '*PropLibrary*' } | ForEach-Object { $_.FullName }
