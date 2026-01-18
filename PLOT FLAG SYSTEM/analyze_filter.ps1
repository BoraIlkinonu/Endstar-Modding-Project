# Load dependencies first
$managedPath = 'C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed'

$null = [System.Reflection.Assembly]::LoadFrom("$managedPath\UnityEngine.dll")
$null = [System.Reflection.Assembly]::LoadFrom("$managedPath\UnityEngine.CoreModule.dll")
$propsAsm = [System.Reflection.Assembly]::LoadFrom("$managedPath\Props.dll")
$null = [System.Reflection.Assembly]::LoadFrom("$managedPath\Assets.dll")
$gameplayAsm = [System.Reflection.Assembly]::LoadFrom("$managedPath\Gameplay.dll")

Write-Host "=== Loading Props.dll types ==="
try {
    $propsTypes = $propsAsm.GetTypes()
    Write-Host "Loaded $($propsTypes.Count) types"
} catch [System.Reflection.ReflectionTypeLoadException] {
    $propsTypes = $_.Exception.Types | Where-Object { $_ -ne $null }
    Write-Host "Loaded $($propsTypes.Count) types (some failed)"
}

Write-Host ""
Write-Host "=== Looking for Prop type ==="
foreach ($t in $propsTypes) {
    if ($t.Name -eq 'Prop') {
        Write-Host "Found: $($t.FullName)"

        Write-Host "  Filter-related fields:"
        $fields = $t.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
        foreach ($f in $fields) {
            $lower = $f.Name.ToLower()
            if ($lower -like '*filter*' -or $lower -like '*reference*' -or $lower -like '*type*') {
                Write-Host "    $($f.Name): $($f.FieldType.Name)"
            }
        }

        Write-Host "  All public fields:"
        foreach ($f in $fields) {
            if ($f.IsPublic) {
                Write-Host "    $($f.Name): $($f.FieldType.Name)"
            }
        }
    }
}
