# Find props with minimal asset references to use as clone source

Add-Type -AssemblyName System.Core

$asm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Props.dll')
$propType = $asm.GetType('Endless.Props.Assets.Prop')

# Get field accessors
$nameField = $propType.GetField('Name', [System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
$prefabBundleField = $propType.GetField('prefabBundle', [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
$visualAssetsField = $propType.GetField('visualAssets', [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
$scriptAssetField = $propType.GetField('scriptAsset', [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
$componentIdsField = $propType.GetField('componentIds', [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)

Write-Host "Prop fields found:"
Write-Host "  nameField: $($nameField -ne $null)"
Write-Host "  prefabBundleField: $($prefabBundleField -ne $null)"
Write-Host "  visualAssetsField: $($visualAssetsField -ne $null)"
Write-Host "  scriptAssetField: $($scriptAssetField -ne $null)"
Write-Host "  componentIdsField: $($componentIdsField -ne $null)"

# Check if name field is on base class
$baseType = $propType
while ($baseType -ne $null) {
    Write-Host "Checking type: $($baseType.Name)"
    $f = $baseType.GetField('Name', [System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::DeclaredOnly)
    if ($f) {
        Write-Host "  Found Name field in $($baseType.Name)"
    }
    $baseType = $baseType.BaseType
}
