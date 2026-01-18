$managedPath = 'C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed'

function Get-TypesSafely($assembly) {
    try { return $assembly.GetTypes() }
    catch [System.Reflection.ReflectionTypeLoadException] {
        return $_.Exception.Types | Where-Object { $_ -ne $null }
    }
}

# Try to find AssetReference in various assemblies
$assemblies = @(
    "Assets.dll",
    "Unity.Addressables.dll",
    "Unity.ResourceManager.dll",
    "Shared.dll"
)

$output = @()
$output += "# ASSET REFERENCE ANALYSIS"
$output += "Generated: $(Get-Date)"
$output += ""

foreach ($asmName in $assemblies) {
    $asmPath = "$managedPath\$asmName"
    if (Test-Path $asmPath) {
        $output += "## $asmName"
        try {
            $asm = [System.Reflection.Assembly]::LoadFrom($asmPath)
            $types = Get-TypesSafely $asm

            # Look for AssetReference
            $assetRefTypes = $types | Where-Object { $_.Name -match 'AssetReference' }
            foreach ($t in $assetRefTypes) {
                $output += ""
                $output += "### $($t.FullName)"
                $output += "Base: $($t.BaseType.Name)"
                $output += ""

                $output += "#### Fields"
                $fields = $t.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
                foreach ($f in $fields | Select-Object -First 15) {
                    $access = if ($f.IsPublic) { "public" } elseif ($f.IsPrivate) { "private" } else { "protected" }
                    $output += "- $access $($f.FieldType.Name) $($f.Name)"
                }
                $output += ""

                $output += "#### Properties"
                $props = $t.GetProperties([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance) | Select-Object -First 10
                foreach ($p in $props) {
                    $output += "- $($p.PropertyType.Name) $($p.Name)"
                }
                $output += ""

                $output += "#### Constructors"
                $ctors = $t.GetConstructors()
                foreach ($c in $ctors) {
                    $params = ($c.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ', '
                    $output += "- ctor($params)"
                }
            }

            # Look for SerializableGuid
            $guidTypes = $types | Where-Object { $_.Name -eq 'SerializableGuid' }
            foreach ($t in $guidTypes) {
                $output += ""
                $output += "### $($t.FullName)"
                $output += "Base: $($t.BaseType.Name)"
                $output += ""

                $output += "#### Fields"
                $fields = $t.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
                foreach ($f in $fields) {
                    $access = if ($f.IsPublic) { "public" } elseif ($f.IsPrivate) { "private" } else { "protected" }
                    $output += "- $access $($f.FieldType.Name) $($f.Name)"
                }
                $output += ""

                $output += "#### Constructors"
                $ctors = $t.GetConstructors()
                foreach ($c in $ctors) {
                    $params = ($c.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ', '
                    $output += "- ctor($params)"
                }

                $output += ""
                $output += "#### Static Methods"
                $methods = $t.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Static) | Where-Object { $_.Name -notmatch 'get_|set_|op_' }
                foreach ($m in $methods | Select-Object -First 10) {
                    $params = ($m.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ', '
                    $output += "- static $($m.ReturnType.Name) $($m.Name)($params)"
                }
            }
        }
        catch {
            $output += "Error loading: $_"
        }
        $output += ""
        $output += "---"
        $output += ""
    }
}

# Write to file
$output | Out-File -FilePath "D:\Endstar Plot Flag\PLOT FLAG SYSTEM\ASSETS_DLL_ANALYSIS.md" -Encoding UTF8

Write-Host "Analysis written to ASSETS_DLL_ANALYSIS.md"
