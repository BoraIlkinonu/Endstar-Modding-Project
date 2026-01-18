# Trace the ACTUAL prop flow - StageManager -> PropLibrary -> UI

$managedPath = 'C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed'

$assemblies = @{}
foreach ($name in @('Gameplay', 'Creator', 'Props', 'Shared', 'UI')) {
    $path = "$managedPath\$name.dll"
    if (Test-Path $path) {
        try {
            $assemblies[$name] = [System.Reflection.Assembly]::LoadFrom($path)
        } catch {}
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
Write-Host "STAGEMANAGER.InjectProp - The injection method" -ForegroundColor Cyan
Write-Host "=" * 80 -ForegroundColor Cyan

$stageManager = $assemblies['Gameplay'].GetType('Endless.Gameplay.LevelEditing.Level.StageManager')
$injectMethod = $stageManager.GetMethod('InjectProp', [System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance)
if ($injectMethod) {
    Write-Host ""
    Write-Host "Method: InjectProp" -ForegroundColor Yellow
    Write-Host "Parameters:"
    foreach ($p in $injectMethod.GetParameters()) {
        Write-Host "  $($p.ParameterType.FullName) $($p.Name)"
    }
    Write-Host "Returns: $($injectMethod.ReturnType.FullName)"
}

Write-Host ""
Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host "StageManager.injectedProps field" -ForegroundColor Cyan
Write-Host "=" * 80 -ForegroundColor Cyan

$injectedPropsField = $stageManager.GetField('injectedProps', [System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
if ($injectedPropsField) {
    Write-Host "Type: $($injectedPropsField.FieldType.FullName)"

    # Get the generic argument
    if ($injectedPropsField.FieldType.IsGenericType) {
        $genericArgs = $injectedPropsField.FieldType.GetGenericArguments()
        foreach ($arg in $genericArgs) {
            Write-Host "  Generic arg: $($arg.FullName)"
        }
    }
}

Write-Host ""
Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host "InjectedProps class DEEP DIVE" -ForegroundColor Cyan
Write-Host "=" * 80 -ForegroundColor Cyan

$injectedPropsType = $assemblies['Gameplay'].GetType('Endless.Gameplay.LevelEditing.InjectedProps')
if ($injectedPropsType) {
    Write-Host "Found: $($injectedPropsType.FullName)"
    Write-Host "Base: $($injectedPropsType.BaseType.FullName)"

    Write-Host ""
    Write-Host "ALL Fields:" -ForegroundColor Yellow
    $fields = $injectedPropsType.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::Static)
    foreach ($f in $fields) {
        Write-Host "  $(if($f.IsStatic){'[static] '})$($f.FieldType.Name) $($f.Name)"
    }

    Write-Host ""
    Write-Host "ALL Methods:" -ForegroundColor Yellow
    $methods = $injectedPropsType.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::Static -bor [System.Reflection.BindingFlags]::DeclaredOnly)
    foreach ($m in $methods) {
        if (-not $m.IsSpecialName) {
            try {
                $params = ($m.GetParameters() | ForEach-Object { "$($_.ParameterType.Name)" }) -join ', '
                Write-Host "  $(if($m.IsStatic){'[static] '})$($m.Name)($params) -> $($m.ReturnType.Name)"
            } catch {}
        }
    }

    Write-Host ""
    Write-Host "Properties:" -ForegroundColor Yellow
    $props = $injectedPropsType.GetProperties([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::Static)
    foreach ($p in $props) {
        Write-Host "  $($p.PropertyType.Name) $($p.Name)"
    }
} else {
    Write-Host "InjectedProps type NOT FOUND - searching..." -ForegroundColor Red

    $types = Get-TypesSafely $assemblies['Gameplay']
    $matches = $types | Where-Object { $_.Name -match 'Inject' }
    foreach ($t in $matches) {
        Write-Host "  Found: $($t.FullName)"
    }
}

Write-Host ""
Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host "PropLibrary.InjectProp method" -ForegroundColor Cyan
Write-Host "=" * 80 -ForegroundColor Cyan

$propLibrary = $assemblies['Gameplay'].GetType('Endless.Gameplay.LevelEditing.PropLibrary')
$plInjectMethod = $propLibrary.GetMethod('InjectProp', [System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
if ($plInjectMethod) {
    Write-Host "Method: PropLibrary.InjectProp" -ForegroundColor Yellow
    Write-Host "Parameters:"
    foreach ($p in $plInjectMethod.GetParameters()) {
        Write-Host "  $($p.ParameterType.FullName) $($p.Name)"
    }
    Write-Host "Returns: $($plInjectMethod.ReturnType.FullName)"
    Write-Host "IsAsync: $($plInjectMethod.ReturnType.Name -match 'Task')"
}

Write-Host ""
Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host "UIRuntimePropInfoListModel - the UI data model" -ForegroundColor Cyan
Write-Host "=" * 80 -ForegroundColor Cyan

$types = Get-TypesSafely $assemblies['Creator']
$listModel = $types | Where-Object { $_.Name -eq 'UIRuntimePropInfoListModel' }
if ($listModel) {
    Write-Host "Found: $($listModel.FullName)"

    Write-Host ""
    Write-Host "Fields:" -ForegroundColor Yellow
    $fields = $listModel.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
    foreach ($f in $fields) {
        Write-Host "  $($f.FieldType.Name) $($f.Name)"
    }

    Write-Host ""
    Write-Host "Methods:" -ForegroundColor Yellow
    $methods = $listModel.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::DeclaredOnly)
    foreach ($m in $methods) {
        if (-not $m.IsSpecialName) {
            try {
                $params = ($m.GetParameters() | ForEach-Object { "$($_.ParameterType.Name)" }) -join ', '
                Write-Host "  $($m.Name)($params) -> $($m.ReturnType.Name)"
            } catch {}
        }
    }
}

Write-Host ""
Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host "How does UIPropToolPanelView get props?" -ForegroundColor Cyan
Write-Host "=" * 80 -ForegroundColor Cyan

$uiPropView = $types | Where-Object { $_.Name -eq 'UIPropToolPanelView' }
if ($uiPropView) {
    Write-Host ""
    Write-Host "ALL UIPropToolPanelView Fields:" -ForegroundColor Yellow
    $fields = $uiPropView.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
    foreach ($f in $fields) {
        Write-Host "  $($f.FieldType.Name) $($f.Name)"
    }

    Write-Host ""
    Write-Host "ALL UIPropToolPanelView Methods:" -ForegroundColor Yellow
    $methods = $uiPropView.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::DeclaredOnly)
    foreach ($m in $methods) {
        if (-not $m.IsSpecialName) {
            try {
                $params = ($m.GetParameters() | ForEach-Object { "$($_.ParameterType.Name)" }) -join ', '
                Write-Host "  $($m.Name)($params) -> $($m.ReturnType.Name)"
            } catch {}
        }
    }
}

Write-Host ""
Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host "DONE" -ForegroundColor Cyan
