$propsAsm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Props.dll')

Write-Host "=== PROP CLASS ANALYSIS ==="
$propType = $propsAsm.GetType('Endless.Props.Assets.Prop')

Write-Host ""
Write-Host "--- Full Name ---"
Write-Host $propType.FullName

Write-Host ""
Write-Host "--- Base Type Chain ---"
$t = $propType
while ($t -ne $null) {
    Write-Host "  $($t.FullName)"
    $t = $t.BaseType
}

Write-Host ""
Write-Host "--- Constructors ---"
$ctors = $propType.GetConstructors([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
foreach ($c in $ctors) {
    Write-Host "  $($c.ToString())"
}

Write-Host ""
Write-Host "--- Static Methods (factory methods?) ---"
$staticMethods = $propType.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Static)
foreach ($m in $staticMethods) {
    if ($m.DeclaringType.FullName -eq 'Endless.Props.Assets.Prop') {
        Write-Host "  $($m.ReturnType.Name) $($m.Name)()"
    }
}

Write-Host ""
Write-Host "=== ASSET BASE CLASS ==="
$assetType = $propsAsm.GetType('Endless.Props.Assets.Asset')

Write-Host ""
Write-Host "--- Constructors ---"
$ctors2 = $assetType.GetConstructors([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
foreach ($c in $ctors2) {
    Write-Host "  $($c.ToString())"
}

Write-Host ""
Write-Host "--- Is Abstract? ---"
Write-Host $assetType.IsAbstract
