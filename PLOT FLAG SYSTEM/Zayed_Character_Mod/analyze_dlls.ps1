# Analyze Endstar Animation System
$dllPath = "C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed"

Write-Host "=== ANALYZING ENDSTAR ANIMATION SYSTEM ===" -ForegroundColor Cyan

# Load key DLLs
$dlls = @("Gameplay.dll", "Creator.dll", "Assembly-CSharp.dll", "RootMotion.dll")

foreach ($dll in $dlls) {
    $fullPath = Join-Path $dllPath $dll
    if (Test-Path $fullPath) {
        Write-Host "`n=== $dll ===" -ForegroundColor Yellow
        try {
            $asm = [System.Reflection.Assembly]::LoadFrom($fullPath)
            $types = $asm.GetTypes() | Where-Object {
                $_.Name -match 'Anim|Character|Appearance|Player|Rig|Bone|Skeleton|Avatar|Motion' -and
                -not $_.Name.Contains('<')
            } | Sort-Object Name

            foreach ($t in $types) {
                Write-Host "  $($t.FullName)"

                # Show public methods for key types
                if ($t.Name -match 'AppearanceAnimator|CharacterAnimation|PlayerCharacter') {
                    Write-Host "    Methods:" -ForegroundColor Green
                    $t.GetMethods() | Where-Object { $_.IsPublic -and -not $_.IsSpecialName } |
                        Select-Object -First 20 | ForEach-Object {
                            Write-Host "      - $($_.Name)($($_.GetParameters().Name -join ', '))"
                        }
                    Write-Host "    Fields:" -ForegroundColor Green
                    $t.GetFields() | Select-Object -First 10 | ForEach-Object {
                        Write-Host "      - $($_.Name) : $($_.FieldType.Name)"
                    }
                }
            }
        } catch {
            Write-Host "  Error loading: $_" -ForegroundColor Red
        }
    }
}

# Look specifically for AppearanceAnimator
Write-Host "`n=== SEARCHING FOR AppearanceAnimator ===" -ForegroundColor Cyan
foreach ($dll in $dlls) {
    $fullPath = Join-Path $dllPath $dll
    if (Test-Path $fullPath) {
        try {
            $asm = [System.Reflection.Assembly]::LoadFrom($fullPath)
            $type = $asm.GetTypes() | Where-Object { $_.Name -eq 'AppearanceAnimator' }
            if ($type) {
                Write-Host "`nFOUND in $dll" -ForegroundColor Green
                Write-Host "Full Name: $($type.FullName)"
                Write-Host "Base Type: $($type.BaseType.Name)"

                Write-Host "`nAll Fields:" -ForegroundColor Yellow
                $type.GetFields([System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Public) | ForEach-Object {
                    Write-Host "  $($_.Name) : $($_.FieldType.Name)"
                }

                Write-Host "`nAll Methods:" -ForegroundColor Yellow
                $type.GetMethods([System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::DeclaredOnly) | ForEach-Object {
                    Write-Host "  $($_.Name)($($_.GetParameters().Name -join ', '))"
                }
            }
        } catch {}
    }
}
