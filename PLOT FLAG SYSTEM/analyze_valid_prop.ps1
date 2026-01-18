# Analyze what makes a VALID prop that won't crash the game

$managedPath = 'C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed'
$propsDll = [System.Reflection.Assembly]::LoadFrom("$managedPath\Props.dll")
$assetsDll = [System.Reflection.Assembly]::LoadFrom("$managedPath\Assets.dll")

$propType = $propsDll.GetType('Endless.Props.Assets.Prop')

Write-Host "=== ALL Prop Fields (full inheritance chain) ===" -ForegroundColor Cyan
Write-Host ""

$current = $propType
while ($current -ne $null -and $current -ne [object]) {
    Write-Host "--- $($current.FullName) ---" -ForegroundColor Yellow
    $fields = $current.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::DeclaredOnly)
    foreach ($f in $fields) {
        $isRequired = ""
        # Check if field type is a reference type (might need to be non-null)
        if (-not $f.FieldType.IsValueType -and $f.FieldType -ne [string]) {
            $isRequired = " [REFERENCE TYPE]"
        }
        Write-Host "  $($f.FieldType.Name) $($f.Name)$isRequired"
    }
    Write-Host ""
    $current = $current.BaseType
}

Write-Host ""
Write-Host "=== AssetReference structure ===" -ForegroundColor Cyan
$assetRefType = $assetsDll.GetType('Endless.Assets.AssetReference')
if ($assetRefType) {
    $fields = $assetRefType.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
    foreach ($f in $fields) {
        Write-Host "  $($f.FieldType.Name) $($f.Name)"
    }
}

Write-Host ""
Write-Host "=== What RuntimePropInfo needs ===" -ForegroundColor Cyan
$gameplayDll = [System.Reflection.Assembly]::LoadFrom("$managedPath\Gameplay.dll")

function Get-TypesSafely($assembly) {
    try { return $assembly.GetTypes() }
    catch [System.Reflection.ReflectionTypeLoadException] {
        return $_.Exception.Types | Where-Object { $_ -ne $null }
    }
    catch { return @() }
}

$types = Get-TypesSafely $gameplayDll
$rpiType = $types | Where-Object { $_.Name -eq 'RuntimePropInfo' }
if ($rpiType) {
    $fields = $rpiType.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
    foreach ($f in $fields) {
        Write-Host "  $($f.FieldType.Name) $($f.Name)"
    }
}
