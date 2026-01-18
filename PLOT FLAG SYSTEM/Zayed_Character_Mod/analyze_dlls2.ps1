# Analyze Endstar Animation System - with exception handling
$dllPath = "C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed"

Write-Host "=== ANALYZING ENDSTAR ANIMATION SYSTEM ===" -ForegroundColor Cyan

# Load all dependencies first
$allDlls = Get-ChildItem $dllPath -Filter "*.dll"
foreach ($dll in $allDlls) {
    try {
        [System.Reflection.Assembly]::LoadFrom($dll.FullName) | Out-Null
    } catch {}
}

# Now analyze key DLLs
$dlls = @("Gameplay.dll", "Creator.dll", "Shared.dll")

foreach ($dllName in $dlls) {
    $fullPath = Join-Path $dllPath $dllName
    if (Test-Path $fullPath) {
        Write-Host "`n=== $dllName ===" -ForegroundColor Yellow
        try {
            $asm = [System.Reflection.Assembly]::LoadFrom($fullPath)
            try {
                $types = $asm.GetTypes()
            } catch [System.Reflection.ReflectionTypeLoadException] {
                $types = $_.Exception.Types | Where-Object { $_ -ne $null }
            }

            $animTypes = $types | Where-Object {
                $_ -ne $null -and
                $_.Name -match 'Anim|Character|Appearance|Rig|Motion|Controller' -and
                -not $_.Name.Contains('<') -and
                -not $_.Name.Contains('>')
            } | Sort-Object Name

            foreach ($t in $animTypes) {
                Write-Host "  $($t.FullName)" -ForegroundColor White
            }
        } catch {
            Write-Host "  Error: $_" -ForegroundColor Red
        }
    }
}

# Search for specific class
Write-Host "`n=== SEARCHING FOR KEY CLASSES ===" -ForegroundColor Cyan
$searchTerms = @("AppearanceAnimator", "CharacterAnimator", "PlayerAnimator", "AnimationController", "CharacterCosmetics")

foreach ($term in $searchTerms) {
    Write-Host "`nSearching for: $term" -ForegroundColor Yellow
    foreach ($asm in [System.AppDomain]::CurrentDomain.GetAssemblies()) {
        try {
            $types = $asm.GetTypes()
        } catch [System.Reflection.ReflectionTypeLoadException] {
            $types = $_.Exception.Types | Where-Object { $_ -ne $null }
        } catch {
            continue
        }

        $found = $types | Where-Object { $_ -ne $null -and $_.Name -eq $term }
        if ($found) {
            Write-Host "  FOUND in $($asm.GetName().Name)" -ForegroundColor Green
            Write-Host "    FullName: $($found.FullName)"
            Write-Host "    BaseType: $($found.BaseType)"

            # Show fields
            Write-Host "    Fields:" -ForegroundColor Cyan
            try {
                $found.GetFields([System.Reflection.BindingFlags]::Instance -bor
                    [System.Reflection.BindingFlags]::NonPublic -bor
                    [System.Reflection.BindingFlags]::Public) |
                    Select-Object -First 15 | ForEach-Object {
                        Write-Host "      - $($_.Name) : $($_.FieldType.Name)"
                    }
            } catch {}

            # Show methods
            Write-Host "    Methods:" -ForegroundColor Cyan
            try {
                $found.GetMethods([System.Reflection.BindingFlags]::Instance -bor
                    [System.Reflection.BindingFlags]::NonPublic -bor
                    [System.Reflection.BindingFlags]::Public -bor
                    [System.Reflection.BindingFlags]::DeclaredOnly) |
                    Select-Object -First 20 | ForEach-Object {
                        $params = ($_.GetParameters() | ForEach-Object { $_.ParameterType.Name }) -join ", "
                        Write-Host "      - $($_.Name)($params) -> $($_.ReturnType.Name)"
                    }
            } catch {}
        }
    }
}
