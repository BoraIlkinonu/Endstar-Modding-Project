# Load all DLLs from the Managed folder to resolve dependencies
$managedPath = 'C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed'

# First load common dependencies
$deps = @(
    'mscorlib.dll',
    'System.dll',
    'System.Core.dll',
    'UnityEngine.dll',
    'UnityEngine.CoreModule.dll'
)

foreach ($dep in $deps) {
    $depPath = Join-Path $managedPath $dep
    if (Test-Path $depPath) {
        try { [System.Reflection.Assembly]::LoadFrom($depPath) | Out-Null } catch {}
    }
}

# Load the game DLLs
$gameDlls = @('Assets.dll', 'Props.dll', 'Gameplay.dll')
foreach ($dll in $gameDlls) {
    $dllPath = Join-Path $managedPath $dll
    if (Test-Path $dllPath) {
        try { [System.Reflection.Assembly]::LoadFrom($dllPath) | Out-Null } catch {}
    }
}

# Now analyze Gameplay.dll
$asm = [System.Reflection.Assembly]::LoadFrom((Join-Path $managedPath 'Gameplay.dll'))

try {
    $types = $asm.GetTypes()
    $propLibrary = $types | Where-Object { $_.Name -eq 'PropLibrary' }

    if ($propLibrary) {
        Write-Host "=== FOUND: $($propLibrary.FullName) ==="
        Write-Host ""
        Write-Host "=== PopulateReferenceFilterMap METHOD ==="
        $method = $propLibrary.GetMethod('PopulateReferenceFilterMap', [System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
        if ($method) {
            Write-Host "Found: $($method.ReturnType.Name) $($method.Name)()"
            Write-Host ""

            # Get the method body info
            $body = $method.GetMethodBody()
            if ($body) {
                Write-Host "Local variables: $($body.LocalVariables.Count)"
                Write-Host "Max stack: $($body.MaxStackSize)"
            }
        } else {
            Write-Host "Method not found directly"
        }

        Write-Host ""
        Write-Host "=== ALL METHODS containing 'Filter' or 'Reference' ==="
        $propLibrary.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::Static) | Where-Object { $_.Name -like '*Filter*' -or $_.Name -like '*Reference*' } | ForEach-Object {
            $params = $_.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }
            Write-Host "  $($_.ReturnType.Name) $($_.Name)($($params -join ', '))"
        }

        Write-Host ""
        Write-Host "=== FIELDS containing 'Filter' or 'Reference' ==="
        $propLibrary.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::Static) | Where-Object { $_.Name -like '*filter*' -or $_.Name -like '*reference*' -or $_.Name -like '*Filter*' -or $_.Name -like '*Reference*' } | ForEach-Object {
            Write-Host "  $($_.FieldType.Name) $($_.Name)"
        }
    }
} catch {
    Write-Host "Error getting types: $($_.Exception.Message)"

    # Fallback: try to get exported types only
    Write-Host ""
    Write-Host "=== Trying exported types ==="
    $asm.GetExportedTypes() | Where-Object { $_.Name -like '*PropLibrary*' } | ForEach-Object {
        Write-Host $_.FullName
    }
}
