$managedPath = 'C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed'

function Get-TypesSafely($assembly) {
    try { return $assembly.GetTypes() }
    catch [System.Reflection.ReflectionTypeLoadException] {
        return $_.Exception.Types | Where-Object { $_ -ne $null }
    }
}

$propsAsm = [System.Reflection.Assembly]::LoadFrom("$managedPath\Props.dll")
$types = Get-TypesSafely $propsAsm

$output = @()
$output += "# PROPS.DLL - COMPLETE ANALYSIS"
$output += "Generated: $(Get-Date)"
$output += ""

# Prop class
$output += "## Prop (ScriptableObject)"
$prop = $types | Where-Object { $_.Name -eq 'Prop' -and -not $_.IsNested }
if ($prop) {
    $output += "Namespace: $($prop.Namespace)"
    $output += "Base: $($prop.BaseType.Name)"
    $output += ""

    $output += "### Fields"
    $fields = $prop.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
    foreach ($f in $fields | Sort-Object Name) {
        $access = if ($f.IsPublic) { "public" } elseif ($f.IsPrivate) { "private" } else { "protected" }
        $output += "- $access $($f.FieldType.Name) $($f.Name)"
    }
    $output += ""

    $output += "### Properties"
    $props = $prop.GetProperties([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
    foreach ($p in $props | Sort-Object Name) {
        $output += "- $($p.PropertyType.Name) $($p.Name) { get: $($p.CanRead); set: $($p.CanWrite) }"
    }
}

$output += ""
$output += "---"
$output += ""

# Script class
$output += "## Script"
$script = $types | Where-Object { $_.Name -eq 'Script' -and -not $_.IsNested }
if ($script) {
    $output += "Namespace: $($script.Namespace)"
    $output += "Base: $($script.BaseType.Name)"
    $output += ""

    $output += "### Fields"
    $fields = $script.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
    foreach ($f in $fields | Sort-Object Name) {
        $access = if ($f.IsPublic) { "public" } elseif ($f.IsPrivate) { "private" } else { "protected" }
        $output += "- $access $($f.FieldType.Name) $($f.Name)"
    }
}

$output += ""
$output += "---"
$output += ""

# All types in Props.dll
$output += "## All Public Types in Props.dll"
$publicTypes = $types | Where-Object { $_.IsPublic -and -not $_.IsNested } | Sort-Object FullName
foreach ($t in $publicTypes) {
    $output += "- $($t.FullName) (Base: $($t.BaseType.Name))"
}

# Write to file
$output | Out-File -FilePath "D:\Endstar Plot Flag\PLOT FLAG SYSTEM\PROPS_DLL_ANALYSIS.md" -Encoding UTF8

Write-Host "Analysis written to PROPS_DLL_ANALYSIS.md"
