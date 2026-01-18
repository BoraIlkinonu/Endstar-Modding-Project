$managedPath = 'C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed'

function Get-TypesSafely($assembly) {
    try { return $assembly.GetTypes() }
    catch [System.Reflection.ReflectionTypeLoadException] {
        return $_.Exception.Types | Where-Object { $_ -ne $null }
    }
}

$output = @()
$output += "# SERIALIZABLE GUID ANALYSIS"
$output += "Generated: $(Get-Date)"
$output += ""

# Check Shared.dll first
$sharedAsm = [System.Reflection.Assembly]::LoadFrom("$managedPath\Shared.dll")
$types = Get-TypesSafely $sharedAsm

$guidType = $types | Where-Object { $_.Name -eq 'SerializableGuid' }
if ($guidType) {
    $output += "## SerializableGuid"
    $output += "Namespace: $($guidType.Namespace)"
    $output += "Base: $($guidType.BaseType.Name)"
    $output += "IsValueType (struct): $($guidType.IsValueType)"
    $output += ""

    $output += "### Fields"
    $fields = $guidType.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::Static)
    foreach ($f in $fields) {
        $access = if ($f.IsPublic) { "public" } elseif ($f.IsPrivate) { "private" } else { "protected" }
        $static = if ($f.IsStatic) { "static " } else { "" }
        $output += "- $access $static$($f.FieldType.Name) $($f.Name)"
    }
    $output += ""

    $output += "### Constructors"
    $ctors = $guidType.GetConstructors()
    foreach ($c in $ctors) {
        $params = ($c.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ', '
        $output += "- ctor($params)"
    }
    $output += ""

    $output += "### Methods"
    $methods = $guidType.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::Static -bor [System.Reflection.BindingFlags]::DeclaredOnly)
    foreach ($m in $methods) {
        $params = ($m.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ', '
        $static = if ($m.IsStatic) { "static " } else { "" }
        $output += "- $static$($m.ReturnType.Name) $($m.Name)($params)"
    }
}

# Also find ToAssetReference method
$output += ""
$output += "---"
$output += ""
$output += "## Methods that convert to AssetReference"

$assetsAsm = [System.Reflection.Assembly]::LoadFrom("$managedPath\Assets.dll")
$assetsTypes = Get-TypesSafely $assetsAsm

$assetRefType = $assetsTypes | Where-Object { $_.Name -eq 'AssetReference' }
if ($assetRefType) {
    $output += "### Endless.Assets.AssetReference"

    $output += "#### All Methods"
    $methods = $assetRefType.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::Static -bor [System.Reflection.BindingFlags]::DeclaredOnly)
    foreach ($m in $methods) {
        $params = ($m.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ', '
        $static = if ($m.IsStatic) { "static " } else { "" }
        $output += "- $static$($m.ReturnType.Name) $($m.Name)($params)"
    }
}

# Write to file
$output | Out-File -FilePath "D:\Endstar Plot Flag\PLOT FLAG SYSTEM\SERIALIZABLEGUID_ANALYSIS.md" -Encoding UTF8

Write-Host "Analysis written to SERIALIZABLEGUID_ANALYSIS.md"
