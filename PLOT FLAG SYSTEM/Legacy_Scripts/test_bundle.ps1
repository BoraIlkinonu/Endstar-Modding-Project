# Copy bundle
Copy-Item -Path "D:\Unity_Workshop\Endstar Custom Shader\Library\com.unity.addressables\aa\Windows\StandaloneWindows64\defaultlocalgroup_assets_all_e3621667618b9c4bab15c8fbc36a3582.bundle" -Destination "C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\StandaloneWindows64\felix_assets_all_8ce5cfff23e50dfcaa729aa03940bfd7.bundle" -Force

# Verify
$file = Get-Item "C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\StandaloneWindows64\felix_assets_all_8ce5cfff23e50dfcaa729aa03940bfd7.bundle"
Write-Host "Bundle replaced!"
Write-Host "Size: $($file.Length) bytes"
Write-Host "Modified: $($file.LastWriteTime)"

# Launch game directly (bypass launcher)
Start-Process "C:\Endless Studios\Endless Launcher\Endstar\Endstar.exe"
Write-Host "Endstar launched!"
