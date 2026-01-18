$gameplayAsm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Gameplay.dll')

Write-Host "=== RuntimePropInfo CLASS ==="
$rpiType = $gameplayAsm.GetType('Endless.Gameplay.LevelEditing.PropLibrary+RuntimePropInfo')

Write-Host ""
Write-Host "--- Fields ---"
$fields = $rpiType.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
foreach ($f in $fields) {
    Write-Host "  $($f.FieldType.FullName) $($f.Name)"
}

Write-Host ""
Write-Host "--- Constructors ---"
$ctors = $rpiType.GetConstructors([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
foreach ($c in $ctors) {
    Write-Host "  $($c.ToString())"
}

Write-Host ""
Write-Host "=== injectedPropIds field type ==="
$plType = $gameplayAsm.GetType('Endless.Gameplay.LevelEditing.PropLibrary')
$injectedField = $plType.GetField('injectedPropIds', [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
Write-Host "  Type: $($injectedField.FieldType.FullName)"

# Check generic arguments
if ($injectedField.FieldType.IsGenericType) {
    Write-Host "  Generic Args:"
    foreach ($arg in $injectedField.FieldType.GetGenericArguments()) {
        Write-Host "    $($arg.FullName)"
    }
}
