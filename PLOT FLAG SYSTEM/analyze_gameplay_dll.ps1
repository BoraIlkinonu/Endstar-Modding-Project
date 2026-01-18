$managedPath = 'C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed'

function Get-TypesSafely($assembly) {
    try { return $assembly.GetTypes() }
    catch [System.Reflection.ReflectionTypeLoadException] {
        return $_.Exception.Types | Where-Object { $_ -ne $null }
    }
}

$gameplayAsm = [System.Reflection.Assembly]::LoadFrom("$managedPath\Gameplay.dll")
$types = Get-TypesSafely $gameplayAsm

$output = @()
$output += "# GAMEPLAY.DLL - COMPLETE ANALYSIS"
$output += "Generated: $(Get-Date)"
$output += ""

# StageManager - COMPLETE
$output += "## StageManager"
$sm = $types | Where-Object { $_.Name -eq 'StageManager' }
if ($sm) {
    $output += "Namespace: $($sm.Namespace)"
    $output += "Base: $($sm.BaseType.Name)"
    $output += ""

    $output += "### Fields"
    $fields = $sm.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::Static)
    foreach ($f in $fields | Sort-Object Name) {
        $access = if ($f.IsPublic) { "public" } elseif ($f.IsPrivate) { "private" } else { "protected" }
        $static = if ($f.IsStatic) { "static " } else { "" }
        $output += "- $access $static$($f.FieldType.Name) $($f.Name)"
    }
    $output += ""

    $output += "### Properties"
    $props = $sm.GetProperties([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::Static)
    foreach ($p in $props | Sort-Object Name) {
        $output += "- $($p.PropertyType.Name) $($p.Name) { get: $($p.CanRead); set: $($p.CanWrite) }"
    }
    $output += ""

    $output += "### Methods"
    $methods = $sm.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::DeclaredOnly)
    foreach ($m in $methods | Sort-Object Name) {
        $params = ($m.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ', '
        $access = if ($m.IsPublic) { "public" } elseif ($m.IsPrivate) { "private" } else { "protected" }
        $output += "- $access $($m.ReturnType.Name) $($m.Name)($params)"
    }
}

$output += ""
$output += "---"
$output += ""

# PropLibrary - COMPLETE
$output += "## PropLibrary"
$pl = $types | Where-Object { $_.Name -eq 'PropLibrary' }
if ($pl) {
    $output += "Namespace: $($pl.Namespace)"
    $output += "Base: $($pl.BaseType.Name)"
    $output += ""

    $output += "### Fields"
    $fields = $pl.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::Static)
    foreach ($f in $fields | Sort-Object Name) {
        $access = if ($f.IsPublic) { "public" } elseif ($f.IsPrivate) { "private" } else { "protected" }
        $static = if ($f.IsStatic) { "static " } else { "" }
        $output += "- $access $static$($f.FieldType.Name) $($f.Name)"
    }
    $output += ""

    $output += "### Properties"
    $props = $pl.GetProperties([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::Static)
    foreach ($p in $props | Sort-Object Name) {
        $output += "- $($p.PropertyType.Name) $($p.Name) { get: $($p.CanRead); set: $($p.CanWrite) }"
    }
    $output += ""

    $output += "### Methods"
    $methods = $pl.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::DeclaredOnly)
    foreach ($m in $methods | Sort-Object Name) {
        $params = ($m.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ', '
        $access = if ($m.IsPublic) { "public" } elseif ($m.IsPrivate) { "private" } else { "protected" }
        $output += "- $access $($m.ReturnType.Name) $($m.Name)($params)"
    }
}

$output += ""
$output += "---"
$output += ""

# RuntimePropInfo - COMPLETE
$output += "## RuntimePropInfo"
$rpi = $types | Where-Object { $_.FullName -match 'RuntimePropInfo' }
foreach ($r in $rpi) {
    $output += "### $($r.FullName)"
    $output += "IsNested: $($r.IsNested)"
    $output += "IsClass: $($r.IsClass)"
    $output += "IsValueType: $($r.IsValueType)"
    $output += ""

    $output += "#### Fields"
    $fields = $r.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
    foreach ($f in $fields) {
        $access = if ($f.IsPublic) { "public" } elseif ($f.IsPrivate) { "private" } else { "protected" }
        $output += "- $access $($f.FieldType.Name) $($f.Name)"
    }
    $output += ""

    $output += "#### Properties"
    $props = $r.GetProperties([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
    foreach ($p in $props) {
        $output += "- $($p.PropertyType.Name) $($p.Name)"
    }
    $output += ""
}

$output += "---"
$output += ""

# InjectedProps - COMPLETE
$output += "## InjectedProps"
$ip = $types | Where-Object { $_.Name -eq 'InjectedProps' }
if ($ip) {
    $output += "Namespace: $($ip.Namespace)"
    $output += "IsValueType (struct): $($ip.IsValueType)"
    $output += ""

    $output += "### Fields"
    $fields = $ip.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
    foreach ($f in $fields) {
        $access = if ($f.IsPublic) { "public" } elseif ($f.IsPrivate) { "private" } else { "protected" }
        $output += "- $access $($f.FieldType.Name) $($f.Name)"
    }
}

$output += ""
$output += "---"
$output += ""

# EndlessProp - COMPLETE
$output += "## EndlessProp"
$ep = $types | Where-Object { $_.Name -eq 'EndlessProp' }
if ($ep) {
    $output += "Namespace: $($ep.Namespace)"
    $output += "Base: $($ep.BaseType.Name)"
    $output += ""

    $output += "### Fields (first 30)"
    $fields = $ep.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance) | Select-Object -First 30
    foreach ($f in $fields) {
        $access = if ($f.IsPublic) { "public" } elseif ($f.IsPrivate) { "private" } else { "protected" }
        $output += "- $access $($f.FieldType.Name) $($f.Name)"
    }
    $output += ""

    $output += "### Properties (first 20)"
    $props = $ep.GetProperties([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance) | Select-Object -First 20
    foreach ($p in $props) {
        $output += "- $($p.PropertyType.Name) $($p.Name)"
    }
}

# Write to file
$output | Out-File -FilePath "D:\Endstar Plot Flag\PLOT FLAG SYSTEM\GAMEPLAY_DLL_ANALYSIS.md" -Encoding UTF8

Write-Host "Analysis written to GAMEPLAY_DLL_ANALYSIS.md"
