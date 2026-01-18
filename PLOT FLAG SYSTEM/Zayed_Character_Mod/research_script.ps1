# Research script for Endstar animation system
$dllPath = "C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Assembly-CSharp.dll"

Write-Host "Loading Assembly-CSharp.dll..." -ForegroundColor Yellow
[Reflection.Assembly]::LoadFrom($dllPath) | Out-Null

$types = [AppDomain]::CurrentDomain.GetAssemblies() | ForEach-Object { $_.GetTypes() } 2>$null

# Find AppearanceAnimator
$aaType = $types | Where-Object { $_.Name -eq 'AppearanceAnimator' }

if ($aaType) {
    Write-Host "`n=== AppearanceAnimator Class ===" -ForegroundColor Green
    Write-Host "Full Name: $($aaType.FullName)"
    Write-Host "Base Type: $($aaType.BaseType.Name)"

    Write-Host "`n=== Fields ===" -ForegroundColor Cyan
    $fields = $aaType.GetFields([Reflection.BindingFlags]::Instance -bor [Reflection.BindingFlags]::NonPublic -bor [Reflection.BindingFlags]::Public)
    foreach ($f in $fields) {
        Write-Host "  [$($f.FieldType.Name)] $($f.Name)"
    }

    Write-Host "`n=== Methods (declared) ===" -ForegroundColor Cyan
    $methods = $aaType.GetMethods([Reflection.BindingFlags]::Instance -bor [Reflection.BindingFlags]::NonPublic -bor [Reflection.BindingFlags]::Public -bor [Reflection.BindingFlags]::DeclaredOnly)
    foreach ($m in $methods) {
        $params = ($m.GetParameters() | ForEach-Object { $_.ParameterType.Name }) -join ', '
        Write-Host "  $($m.ReturnType.Name) $($m.Name)($params)"
    }
} else {
    Write-Host "AppearanceAnimator not found!" -ForegroundColor Red
}

# Find animation-related classes
Write-Host "`n=== Animation-Related Classes ===" -ForegroundColor Green
$animClasses = $types | Where-Object { $_.Name -like '*Anim*' -and $_.IsClass } | Select-Object -First 30
foreach ($c in $animClasses) {
    Write-Host "  $($c.FullName)"
}

# Find CharacterAppearance or similar
Write-Host "`n=== Character Appearance Classes ===" -ForegroundColor Green
$charClasses = $types | Where-Object { $_.Name -like '*Appearance*' -and $_.IsClass }
foreach ($c in $charClasses) {
    Write-Host "  $($c.FullName)"
}
