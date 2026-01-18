# List game aa folder
Write-Host "=== AA Folder Contents ==="
Get-ChildItem "C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\" -Recurse | ForEach-Object {
    Write-Host $_.FullName
}

Write-Host ""
Write-Host "=== Looking for catalog files ==="
Get-ChildItem "C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\" -Recurse -Filter "*catalog*" | ForEach-Object {
    Write-Host $_.FullName
}
