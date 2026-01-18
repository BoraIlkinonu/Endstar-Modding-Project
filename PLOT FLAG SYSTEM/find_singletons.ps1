# Find singleton patterns, static instances, and entry points

$managedPath = 'C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed'

$assemblies = @{}
foreach ($name in @('Gameplay', 'Creator', 'Props', 'Shared', 'UI')) {
    $path = "$managedPath\$name.dll"
    if (Test-Path $path) {
        try { $assemblies[$name] = [System.Reflection.Assembly]::LoadFrom($path) } catch {}
    }
}

function Get-TypesSafely($assembly) {
    try { return $assembly.GetTypes() }
    catch [System.Reflection.ReflectionTypeLoadException] {
        return $_.Exception.Types | Where-Object { $_ -ne $null }
    }
    catch { return @() }
}

Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host "FINDING SINGLETON PATTERNS AND STATIC ACCESSORS" -ForegroundColor Cyan
Write-Host "=" * 80 -ForegroundColor Cyan

# Look for static Instance properties
foreach ($asmName in $assemblies.Keys) {
    $types = Get-TypesSafely $assemblies[$asmName]
    foreach ($type in $types) {
        # Static Instance property
        $instanceProp = $type.GetProperty('Instance', [System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Static)
        if ($instanceProp) {
            Write-Host "SINGLETON: $($type.FullName).Instance -> $($instanceProp.PropertyType.Name)" -ForegroundColor Green
        }

        # Static Current property
        $currentProp = $type.GetProperty('Current', [System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Static)
        if ($currentProp) {
            Write-Host "CURRENT: $($type.FullName).Current -> $($currentProp.PropertyType.Name)" -ForegroundColor Yellow
        }
    }
}

Write-Host ""
Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host "StageManager - How to access it?" -ForegroundColor Cyan
Write-Host "=" * 80 -ForegroundColor Cyan

$stageManager = $assemblies['Gameplay'].GetType('Endless.Gameplay.LevelEditing.Level.StageManager')

Write-Host "Static properties:" -ForegroundColor Yellow
$props = $stageManager.GetProperties([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Static)
foreach ($p in $props) {
    Write-Host "  [static] $($p.PropertyType.Name) $($p.Name)"
}

Write-Host ""
Write-Host "Static fields:" -ForegroundColor Yellow
$fields = $stageManager.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Static)
foreach ($f in $fields) {
    Write-Host "  [static] $($f.FieldType.Name) $($f.Name)"
}

Write-Host ""
Write-Host "Base class: $($stageManager.BaseType.FullName)" -ForegroundColor Yellow

Write-Host ""
Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host "CreatorManager - How to access it?" -ForegroundColor Cyan
Write-Host "=" * 80 -ForegroundColor Cyan

$creatorManager = $assemblies['Creator'].GetType('Endless.Creator.CreatorManager')

Write-Host "Static properties:" -ForegroundColor Yellow
$props = $creatorManager.GetProperties([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Static)
foreach ($p in $props) {
    Write-Host "  [static] $($p.PropertyType.Name) $($p.Name)"
}

Write-Host ""
Write-Host "Static fields:" -ForegroundColor Yellow
$fields = $creatorManager.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Static)
foreach ($f in $fields) {
    Write-Host "  [static] $($f.FieldType.Name) $($f.Name)"
}

Write-Host ""
Write-Host "Base class: $($creatorManager.BaseType.FullName)" -ForegroundColor Yellow

# Check if it inherits from MonoBehaviour
$current = $creatorManager
while ($current -ne $null) {
    if ($current.Name -eq 'MonoBehaviour' -or $current.Name -eq 'NetworkBehaviour') {
        Write-Host "*** Is MonoBehaviour/NetworkBehaviour - can use FindObjectOfType ***" -ForegroundColor Green
        break
    }
    $current = $current.BaseType
}

Write-Host ""
Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host "Who HOLDS references to StageManager?" -ForegroundColor Cyan
Write-Host "=" * 80 -ForegroundColor Cyan

foreach ($asmName in $assemblies.Keys) {
    $types = Get-TypesSafely $assemblies[$asmName]
    foreach ($type in $types) {
        $fields = $type.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::Static)
        foreach ($field in $fields) {
            if ($field.FieldType.Name -eq 'StageManager') {
                Write-Host "  $($type.FullName).$($field.Name) $(if($field.IsStatic){'[static]'})"
            }
        }
    }
}

Write-Host ""
Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host "AssetReference class - How is it created?" -ForegroundColor Cyan
Write-Host "=" * 80 -ForegroundColor Cyan

$assetRefType = $assemblies['Shared'].GetType('Endless.Assets.AssetReference')
if (-not $assetRefType) {
    # Try loading Assets.dll
    $assetsPath = "$managedPath\Assets.dll"
    if (Test-Path $assetsPath) {
        $assemblies['Assets'] = [System.Reflection.Assembly]::LoadFrom($assetsPath)
        $assetRefType = $assemblies['Assets'].GetType('Endless.Assets.AssetReference')
    }
}

if ($assetRefType) {
    Write-Host "Full Name: $($assetRefType.FullName)"

    Write-Host ""
    Write-Host "Constructors:" -ForegroundColor Yellow
    $ctors = $assetRefType.GetConstructors([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
    foreach ($c in $ctors) {
        $params = ($c.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ', '
        Write-Host "  AssetReference($params)"
    }

    Write-Host ""
    Write-Host "Fields:" -ForegroundColor Yellow
    $fields = $assetRefType.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
    foreach ($f in $fields) {
        Write-Host "  $($f.FieldType.Name) $($f.Name)"
    }

    Write-Host ""
    Write-Host "Static methods (factory?):" -ForegroundColor Yellow
    $methods = $assetRefType.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Static)
    foreach ($m in $methods) {
        try {
            $params = ($m.GetParameters() | ForEach-Object { "$($_.ParameterType.Name)" }) -join ', '
            Write-Host "  [static] $($m.Name)($params) -> $($m.ReturnType.Name)"
        } catch {}
    }
}

Write-Host ""
Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host "Prop.ToAssetReference method?" -ForegroundColor Cyan
Write-Host "=" * 80 -ForegroundColor Cyan

$propType = $assemblies['Props'].GetType('Endless.Props.Assets.Prop')
$toAssetRef = $propType.GetMethod('ToAssetReference', [System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance)
if ($toAssetRef) {
    Write-Host "Prop.ToAssetReference() -> $($toAssetRef.ReturnType.Name)" -ForegroundColor Green
} else {
    # Check base classes
    $current = $propType
    while ($current -ne $null -and $current -ne [object]) {
        $method = $current.GetMethod('ToAssetReference', [System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::DeclaredOnly)
        if ($method) {
            Write-Host "Found in $($current.Name): ToAssetReference() -> $($method.ReturnType.Name)" -ForegroundColor Green
        }
        $current = $current.BaseType
    }
}

Write-Host ""
Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host "DONE" -ForegroundColor Cyan
