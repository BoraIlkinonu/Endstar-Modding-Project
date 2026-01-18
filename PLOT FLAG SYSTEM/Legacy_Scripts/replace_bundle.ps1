# Kill any Endstar processes
Get-Process | ForEach-Object {
    if ($_.Name -like "*Endstar*" -or $_.Name -like "*Endless*") {
        Stop-Process -Id $_.Id -Force
    }
}

Start-Sleep -Seconds 2

# Copy the bundle
$source = "D:\Unity_Workshop\Endstar Custom Shader\Library\com.unity.addressables\aa\Windows\StandaloneWindows64\defaultlocalgroup_assets_all_0768a836c1e79c3ffde8b4df162a0964.bundle"
$dest = "C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\StandaloneWindows64\felix_assets_all_8ce5cfff23e50dfcaa729aa03940bfd7.bundle"

Copy-Item -Path $source -Destination $dest -Force

Write-Host "Bundle replaced successfully!"
