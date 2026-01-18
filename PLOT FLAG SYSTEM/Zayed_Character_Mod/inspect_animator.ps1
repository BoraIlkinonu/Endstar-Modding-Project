$managedPath = 'C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed'

Get-ChildItem "$managedPath\*.dll" | ForEach-Object {
    try {
        $dll = [System.Reflection.Assembly]::LoadFrom($_.FullName)
        $types = $dll.GetTypes() | Where-Object { $_.Name -match 'AppearanceAnimator' }
        if ($types) {
            Write-Host "Found in: $($_.Name)" -ForegroundColor Green
            $types | ForEach-Object {
                Write-Host "  $($_.FullName)" -ForegroundColor Yellow
            }
        }
    } catch { }
}
