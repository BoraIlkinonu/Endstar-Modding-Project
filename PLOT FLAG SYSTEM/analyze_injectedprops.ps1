# Deep dive into InjectedProps and the prop data flow

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
Write-Host "InjectedProps class (the CORRECT one)" -ForegroundColor Cyan
Write-Host "=" * 80 -ForegroundColor Cyan

$types = Get-TypesSafely $assemblies['Gameplay']
$injectedPropsType = $types | Where-Object { $_.FullName -eq 'Endless.Gameplay.LevelEditing.Level.InjectedProps' }

if ($injectedPropsType) {
    Write-Host "Full Name: $($injectedPropsType.FullName)"
    Write-Host "Base: $($injectedPropsType.BaseType.FullName)"
    Write-Host "Is Class: $($injectedPropsType.IsClass)"
    Write-Host "Is Struct: $($injectedPropsType.IsValueType)"

    Write-Host ""
    Write-Host "Constructors:" -ForegroundColor Yellow
    $ctors = $injectedPropsType.GetConstructors([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
    foreach ($c in $ctors) {
        $params = ($c.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ', '
        Write-Host "  InjectedProps($params)"
    }

    Write-Host ""
    Write-Host "ALL Fields:" -ForegroundColor Yellow
    $fields = $injectedPropsType.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::Static)
    foreach ($f in $fields) {
        Write-Host "  $(if($f.IsStatic){'[static] '})$(if($f.IsPublic){'public '}else{'private '})$($f.FieldType.FullName) $($f.Name)"
    }

    Write-Host ""
    Write-Host "ALL Properties:" -ForegroundColor Yellow
    $props = $injectedPropsType.GetProperties([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::Static)
    foreach ($p in $props) {
        Write-Host "  $($p.PropertyType.Name) $($p.Name)"
    }

    Write-Host ""
    Write-Host "ALL Methods:" -ForegroundColor Yellow
    $methods = $injectedPropsType.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::Static -bor [System.Reflection.BindingFlags]::DeclaredOnly)
    foreach ($m in $methods) {
        if (-not $m.IsSpecialName) {
            try {
                $params = ($m.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ', '
                Write-Host "  $(if($m.IsStatic){'[static] '})$($m.Name)($params) -> $($m.ReturnType.Name)"
            } catch {}
        }
    }
}

Write-Host ""
Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host "RuntimePropInfo struct" -ForegroundColor Cyan
Write-Host "=" * 80 -ForegroundColor Cyan

$runtimePropInfo = $types | Where-Object { $_.Name -eq 'RuntimePropInfo' }
if ($runtimePropInfo) {
    Write-Host "Full Name: $($runtimePropInfo.FullName)"
    Write-Host "Is Struct: $($runtimePropInfo.IsValueType)"

    Write-Host ""
    Write-Host "ALL Fields:" -ForegroundColor Yellow
    $fields = $runtimePropInfo.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
    foreach ($f in $fields) {
        Write-Host "  $($f.FieldType.Name) $($f.Name)"
    }

    Write-Host ""
    Write-Host "ALL Properties:" -ForegroundColor Yellow
    $props = $runtimePropInfo.GetProperties([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
    foreach ($p in $props) {
        Write-Host "  $($p.PropertyType.Name) $($p.Name)"
    }
}

Write-Host ""
Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host "Prop class from Props.dll" -ForegroundColor Cyan
Write-Host "=" * 80 -ForegroundColor Cyan

$propType = $assemblies['Props'].GetType('Endless.Props.Assets.Prop')
if ($propType) {
    Write-Host "Full Name: $($propType.FullName)"
    Write-Host "Base: $($propType.BaseType.FullName)"

    Write-Host ""
    Write-Host "Constructors:" -ForegroundColor Yellow
    $ctors = $propType.GetConstructors([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
    foreach ($c in $ctors) {
        $params = ($c.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ', '
        Write-Host "  Prop($params)"
    }

    Write-Host ""
    Write-Host "ALL Fields (inheritance chain):" -ForegroundColor Yellow
    $current = $propType
    while ($current -ne $null -and $current -ne [object]) {
        Write-Host "  --- $($current.Name) ---" -ForegroundColor Gray
        $fields = $current.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::DeclaredOnly)
        foreach ($f in $fields) {
            Write-Host "    $($f.FieldType.Name) $($f.Name)"
        }
        $current = $current.BaseType
    }
}

Write-Host ""
Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host "How PropLibrary stores props - loadedPropMap" -ForegroundColor Cyan
Write-Host "=" * 80 -ForegroundColor Cyan

$propLibrary = $assemblies['Gameplay'].GetType('Endless.Gameplay.LevelEditing.PropLibrary')
$loadedPropMapField = $propLibrary.GetField('loadedPropMap', [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
if ($loadedPropMapField) {
    Write-Host "Field: loadedPropMap"
    Write-Host "Type: $($loadedPropMapField.FieldType.FullName)"

    if ($loadedPropMapField.FieldType.IsGenericType) {
        $genericArgs = $loadedPropMapField.FieldType.GetGenericArguments()
        Write-Host "Key Type: $($genericArgs[0].FullName)"
        Write-Host "Value Type: $($genericArgs[1].FullName)"
    }
}

Write-Host ""
Write-Host "ALL PropLibrary fields:" -ForegroundColor Yellow
$fields = $propLibrary.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
foreach ($f in $fields) {
    Write-Host "  $($f.FieldType.Name) $($f.Name)"
}

Write-Host ""
Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host "DONE" -ForegroundColor Cyan
