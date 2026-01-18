$managedPath = 'C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed'

function Get-TypesSafely($assembly) {
    try { return $assembly.GetTypes() }
    catch [System.Reflection.ReflectionTypeLoadException] {
        return $_.Exception.Types | Where-Object { $_ -ne $null }
    }
}

$creatorAsm = [System.Reflection.Assembly]::LoadFrom("$managedPath\Creator.dll")
$types = Get-TypesSafely $creatorAsm

$output = @()
$output += "# CREATOR.DLL - COMPLETE ANALYSIS"
$output += "Generated: $(Get-Date)"
$output += ""

# CreatorManager
$output += "## CreatorManager"
$cm = $types | Where-Object { $_.Name -eq 'CreatorManager' }
if ($cm) {
    $output += "Namespace: $($cm.Namespace)"
    $output += "Base: $($cm.BaseType.Name)"
    $output += ""

    $output += "### Fields"
    $fields = $cm.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::Static)
    foreach ($f in $fields | Sort-Object Name) {
        $access = if ($f.IsPublic) { "public" } elseif ($f.IsPrivate) { "private" } else { "protected" }
        $static = if ($f.IsStatic) { "static " } else { "" }
        $output += "- $access $static$($f.FieldType.Name) $($f.Name)"
    }
    $output += ""

    $output += "### Properties"
    $props = $cm.GetProperties([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::Static)
    foreach ($p in $props | Sort-Object Name) {
        $output += "- $($p.PropertyType.Name) $($p.Name) { get: $($p.CanRead); set: $($p.CanWrite) }"
    }
    $output += ""

    $output += "### Methods (first 50)"
    $methods = $cm.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::DeclaredOnly) | Select-Object -First 50
    foreach ($m in $methods | Sort-Object Name) {
        $params = ($m.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ', '
        $access = if ($m.IsPublic) { "public" } elseif ($m.IsPrivate) { "private" } else { "protected" }
        $output += "- $access $($m.ReturnType.Name) $($m.Name)($params)"
    }
}

$output += ""
$output += "---"
$output += ""

# PropTool
$output += "## PropTool"
$pt = $types | Where-Object { $_.Name -eq 'PropTool' }
if ($pt) {
    $output += "Namespace: $($pt.Namespace)"
    $output += "Base: $($pt.BaseType.Name)"
    $output += ""

    $output += "### Fields"
    $fields = $pt.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::Static)
    foreach ($f in $fields | Sort-Object Name) {
        $access = if ($f.IsPublic) { "public" } elseif ($f.IsPrivate) { "private" } else { "protected" }
        $static = if ($f.IsStatic) { "static " } else { "" }
        $output += "- $access $static$($f.FieldType.Name) $($f.Name)"
    }
    $output += ""

    $output += "### Methods"
    $methods = $pt.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::DeclaredOnly)
    foreach ($m in $methods | Sort-Object Name) {
        $params = ($m.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ', '
        $access = if ($m.IsPublic) { "public" } elseif ($m.IsPrivate) { "private" } else { "protected" }
        $output += "- $access $($m.ReturnType.Name) $($m.Name)($params)"
    }
}

$output += ""
$output += "---"
$output += ""

# UIPropToolPanelView and related UI classes
$output += "## UI Classes Related to Props"
$uiClasses = $types | Where-Object { $_.Name -match 'Prop.*Panel|Prop.*View|Prop.*UI|RuntimePropInfo.*Model' }
foreach ($ui in $uiClasses) {
    $output += ""
    $output += "### $($ui.Name)"
    $output += "Namespace: $($ui.Namespace)"
    $output += "Base: $($ui.BaseType.Name)"

    $output += "#### Fields"
    $fields = $ui.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance) | Select-Object -First 20
    foreach ($f in $fields) {
        $access = if ($f.IsPublic) { "public" } elseif ($f.IsPrivate) { "private" } else { "protected" }
        $output += "- $access $($f.FieldType.Name) $($f.Name)"
    }

    $output += "#### Methods"
    $methods = $ui.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::DeclaredOnly) | Select-Object -First 20
    foreach ($m in $methods) {
        $params = ($m.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ', '
        $output += "- $($m.ReturnType.Name) $($m.Name)($params)"
    }
}

$output += ""
$output += "---"
$output += ""

# Find any method that calls GetReferenceFilteredDefinitionList or populates prop UI
$output += "## Types that reference PropLibrary"
$propLibRefTypes = $types | Where-Object {
    $_.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance) |
    Where-Object { $_.FieldType.Name -match 'PropLibrary' }
}
foreach ($t in $propLibRefTypes) {
    $output += "- $($t.FullName)"
}

# Write to file
$output | Out-File -FilePath "D:\Endstar Plot Flag\PLOT FLAG SYSTEM\CREATOR_DLL_ANALYSIS.md" -Encoding UTF8

Write-Host "Analysis written to CREATOR_DLL_ANALYSIS.md"
