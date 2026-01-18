$assetsAsm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Assets.dll')

Write-Host "=== AssetReference CLASS ==="
$arType = $assetsAsm.GetType('Endless.Assets.AssetReference')

Write-Host ""
Write-Host "--- Fields ---"
$fields = $arType.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
foreach ($f in $fields) {
    Write-Host "  $($f.FieldType.FullName) $($f.Name)"
}

Write-Host ""
Write-Host "--- Properties ---"
$props = $arType.GetProperties([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
foreach ($p in $props) {
    Write-Host "  $($p.PropertyType.FullName) $($p.Name)"
}

Write-Host ""
Write-Host "--- Constructors ---"
$ctors = $arType.GetConstructors([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
foreach ($c in $ctors) {
    Write-Host "  $($c.ToString())"
}

Write-Host ""
Write-Host "--- GetHashCode and Equals ---"
$methods = $arType.GetMethods() | Where-Object { $_.Name -eq 'GetHashCode' -or $_.Name -eq 'Equals' }
foreach ($m in $methods) {
    if ($m.DeclaringType.FullName -eq 'Endless.Assets.AssetReference') {
        Write-Host "  $($m.ToString())"
    }
}
