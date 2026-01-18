Set-Location "D:\Endstar Plot Flag\PLOT FLAG SYSTEM\EndstarPropInjector"
& "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" "EndstarPropInjector.csproj" "-p:Configuration=Release"

$sourceDll = ".\bin\Release\EndstarPropInjector.dll"
$destFolder = "C:\Endless Studios\Endless Launcher\Endstar\BepInEx\plugins"
$destDll = "$destFolder\EndstarPropInjector.dll"

if (Test-Path $sourceDll) {
    Write-Host "Copying $sourceDll to $destFolder..."
    Copy-Item -Path $sourceDll -Destination $destDll -Force
    Write-Host "Build complete. DLL copied to plugins folder."
    Get-Item $destDll | Select-Object Name, Length, LastWriteTime
} else {
    Write-Host "ERROR: Build failed - DLL not found at $sourceDll"
}
