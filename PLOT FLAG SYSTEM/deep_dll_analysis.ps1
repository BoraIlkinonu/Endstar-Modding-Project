# Deep DLL Analysis - Understanding the ACTUAL code flow
# Not just signatures, but HOW things connect

$managedPath = 'C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed'

# Load all relevant assemblies
$assemblies = @{}
foreach ($name in @('Gameplay', 'Creator', 'Props', 'Shared', 'UI')) {
    $path = "$managedPath\$name.dll"
    if (Test-Path $path) {
        try {
            $assemblies[$name] = [System.Reflection.Assembly]::LoadFrom($path)
            Write-Host "Loaded: $name" -ForegroundColor Green
        } catch {
            Write-Host "Failed to load: $name" -ForegroundColor Red
        }
    }
}

function Get-TypesSafely($assembly) {
    try { return $assembly.GetTypes() }
    catch [System.Reflection.ReflectionTypeLoadException] {
        return $_.Exception.Types | Where-Object { $_ -ne $null }
    }
    catch { return @() }
}

Write-Host ""
Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host "PART 1: WHERE IS PropLibrary CREATED?" -ForegroundColor Cyan
Write-Host "=" * 80 -ForegroundColor Cyan

# Search ALL types for references to PropLibrary
$propLibraryType = $assemblies['Gameplay'].GetType('Endless.Gameplay.LevelEditing.PropLibrary')
Write-Host ""
Write-Host "PropLibrary full name: $($propLibraryType.FullName)"
Write-Host "PropLibrary is: $($propLibraryType.BaseType.Name)"

# Find all fields of type PropLibrary in ALL assemblies
Write-Host ""
Write-Host "=== Classes that HOLD a PropLibrary reference ===" -ForegroundColor Yellow
foreach ($asmName in $assemblies.Keys) {
    $types = Get-TypesSafely $assemblies[$asmName]
    foreach ($type in $types) {
        $fields = $type.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::Static)
        foreach ($field in $fields) {
            if ($field.FieldType -eq $propLibraryType -or $field.FieldType.Name -eq 'PropLibrary') {
                Write-Host "  $($type.FullName).$($field.Name) [$(if($field.IsStatic){'static '}else{''})$($field.FieldType.Name)]"
            }
        }
    }
}

Write-Host ""
Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host "PART 2: HOW DOES CREATOR MODE ACCESS PROPS?" -ForegroundColor Cyan
Write-Host "=" * 80 -ForegroundColor Cyan

# Look at CreatorManager
$creatorManager = $assemblies['Creator'].GetType('Endless.Creator.CreatorManager')
if ($creatorManager) {
    Write-Host ""
    Write-Host "=== CreatorManager Fields ===" -ForegroundColor Yellow
    $fields = $creatorManager.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
    foreach ($f in $fields | Sort-Object Name) {
        Write-Host "  $($f.FieldType.Name) $($f.Name)"
    }

    Write-Host ""
    Write-Host "=== CreatorManager Properties ===" -ForegroundColor Yellow
    $props = $creatorManager.GetProperties([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
    foreach ($p in $props | Sort-Object Name) {
        Write-Host "  $($p.PropertyType.Name) $($p.Name)"
    }
}

Write-Host ""
Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host "PART 3: PROP TOOL UI - How does it get props?" -ForegroundColor Cyan
Write-Host "=" * 80 -ForegroundColor Cyan

# Find UIPropToolPanelView
foreach ($asmName in $assemblies.Keys) {
    $types = Get-TypesSafely $assemblies[$asmName]
    $propUI = $types | Where-Object { $_.Name -match 'PropTool|PropPanel|PropLibrary.*View' }
    foreach ($type in $propUI) {
        Write-Host ""
        Write-Host "=== $($type.FullName) ===" -ForegroundColor Yellow

        Write-Host "  Fields:" -ForegroundColor Gray
        $fields = $type.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
        foreach ($f in $fields | Select-Object -First 20) {
            Write-Host "    $($f.FieldType.Name) $($f.Name)"
        }

        Write-Host "  Methods:" -ForegroundColor Gray
        $methods = $type.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::DeclaredOnly) | Where-Object { -not $_.IsSpecialName }
        foreach ($m in $methods | Select-Object -First 20) {
            $params = ($m.GetParameters() | ForEach-Object { "$($_.ParameterType.Name)" }) -join ', '
            Write-Host "    $($m.Name)($params)"
        }
    }
}

Write-Host ""
Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host "PART 4: STAGE MANAGER - Entry point to level editing" -ForegroundColor Cyan
Write-Host "=" * 80 -ForegroundColor Cyan

$stageManager = $assemblies['Gameplay'].GetType('Endless.Gameplay.LevelEditing.Level.StageManager')
if ($stageManager) {
    Write-Host ""
    Write-Host "=== StageManager Fields ===" -ForegroundColor Yellow
    $fields = $stageManager.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
    foreach ($f in $fields | Sort-Object Name) {
        Write-Host "  $($f.FieldType.Name) $($f.Name)"
    }

    Write-Host ""
    Write-Host "=== StageManager Methods (all) ===" -ForegroundColor Yellow
    $methods = $stageManager.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::DeclaredOnly) | Where-Object { -not $_.IsSpecialName }
    foreach ($m in $methods | Sort-Object Name) {
        $params = ($m.GetParameters() | ForEach-Object { "$($_.ParameterType.Name)" }) -join ', '
        Write-Host "  $($m.Name)($params) -> $($m.ReturnType.Name)"
    }
}

Write-Host ""
Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host "PART 5: What classes have 'Prop' in their name?" -ForegroundColor Cyan
Write-Host "=" * 80 -ForegroundColor Cyan

foreach ($asmName in $assemblies.Keys) {
    $types = Get-TypesSafely $assemblies[$asmName]
    $propTypes = $types | Where-Object { $_.Name -match '^Prop|Library$' -and $_.IsClass }
    if ($propTypes) {
        Write-Host ""
        Write-Host "=== $asmName ===" -ForegroundColor Yellow
        foreach ($t in $propTypes | Sort-Object Name) {
            Write-Host "  $($t.FullName)"
        }
    }
}

Write-Host ""
Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host "PART 6: InjectedProps class - the existing injection system" -ForegroundColor Cyan
Write-Host "=" * 80 -ForegroundColor Cyan

$injectedProps = $assemblies['Gameplay'].GetType('Endless.Gameplay.LevelEditing.InjectedProps')
if ($injectedProps) {
    Write-Host ""
    Write-Host "=== InjectedProps Fields ===" -ForegroundColor Yellow
    $fields = $injectedProps.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::Static)
    foreach ($f in $fields) {
        Write-Host "  $(if($f.IsStatic){'static '})$($f.FieldType.Name) $($f.Name)"
    }

    Write-Host ""
    Write-Host "=== InjectedProps Methods ===" -ForegroundColor Yellow
    $methods = $injectedProps.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::Static -bor [System.Reflection.BindingFlags]::DeclaredOnly) | Where-Object { -not $_.IsSpecialName }
    foreach ($m in $methods) {
        $params = ($m.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ', '
        Write-Host "  $(if($m.IsStatic){'static '})$($m.Name)($params) -> $($m.ReturnType.Name)"
    }
}

Write-Host ""
Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host "DONE" -ForegroundColor Cyan
