# Deep analysis of AppearanceAnimator and animation system
$dllPath = "C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed"

# Load all DLLs
Get-ChildItem $dllPath -Filter "*.dll" | ForEach-Object {
    try { [System.Reflection.Assembly]::LoadFrom($_.FullName) | Out-Null } catch {}
}

Write-Host "=== DEEP ANALYSIS OF ANIMATION SYSTEM ===" -ForegroundColor Cyan

# Find AppearanceAnimator
$gameplayAsm = [System.Reflection.Assembly]::LoadFrom("$dllPath\Gameplay.dll")
try { $types = $gameplayAsm.GetTypes() }
catch [System.Reflection.ReflectionTypeLoadException] { $types = $_.Exception.Types | Where-Object { $_ -ne $null } }

$appearanceAnimator = $types | Where-Object { $_.Name -eq 'AppearanceAnimator' }

if ($appearanceAnimator) {
    Write-Host "`n=== AppearanceAnimator FULL ANALYSIS ===" -ForegroundColor Yellow

    # ALL fields
    Write-Host "`nALL FIELDS:" -ForegroundColor Cyan
    $appearanceAnimator.GetFields([System.Reflection.BindingFlags]::Instance -bor
        [System.Reflection.BindingFlags]::NonPublic -bor
        [System.Reflection.BindingFlags]::Public) | ForEach-Object {
            $access = if ($_.IsPublic) { "public" } elseif ($_.IsPrivate) { "private" } else { "protected" }
            Write-Host "  [$access] $($_.Name) : $($_.FieldType.Name)"
        }

    # ALL methods
    Write-Host "`nALL METHODS:" -ForegroundColor Cyan
    $appearanceAnimator.GetMethods([System.Reflection.BindingFlags]::Instance -bor
        [System.Reflection.BindingFlags]::NonPublic -bor
        [System.Reflection.BindingFlags]::Public -bor
        [System.Reflection.BindingFlags]::DeclaredOnly) | ForEach-Object {
            $access = if ($_.IsPublic) { "public" } elseif ($_.IsPrivate) { "private" } else { "protected" }
            $params = ($_.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ", "
            Write-Host "  [$access] $($_.Name)($params) -> $($_.ReturnType.Name)"
        }
}

# Find AnimationClipInfo and AnimationClipSet
Write-Host "`n=== AnimationClipInfo ===" -ForegroundColor Yellow
$clipInfo = $types | Where-Object { $_.Name -eq 'AnimationClipInfo' }
if ($clipInfo) {
    $clipInfo.GetFields([System.Reflection.BindingFlags]::Instance -bor
        [System.Reflection.BindingFlags]::NonPublic -bor
        [System.Reflection.BindingFlags]::Public) | ForEach-Object {
            Write-Host "  $($_.Name) : $($_.FieldType.Name)"
        }
}

Write-Host "`n=== AnimationClipSet ===" -ForegroundColor Yellow
$clipSet = $types | Where-Object { $_.Name -eq 'AnimationClipSet' }
if ($clipSet) {
    $clipSet.GetFields([System.Reflection.BindingFlags]::Instance -bor
        [System.Reflection.BindingFlags]::NonPublic -bor
        [System.Reflection.BindingFlags]::Public) | ForEach-Object {
            Write-Host "  $($_.Name) : $($_.FieldType.Name)"
        }
    $clipSet.GetMethods([System.Reflection.BindingFlags]::Instance -bor
        [System.Reflection.BindingFlags]::Public -bor
        [System.Reflection.BindingFlags]::DeclaredOnly) | ForEach-Object {
            $params = ($_.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ", "
            Write-Host "  $($_.Name)($params) -> $($_.ReturnType.Name)"
        }
}

# Look at AppearanceController
Write-Host "`n=== AppearanceController ===" -ForegroundColor Yellow
$appearanceController = $types | Where-Object { $_.Name -eq 'AppearanceController' }
if ($appearanceController) {
    $appearanceController.GetFields([System.Reflection.BindingFlags]::Instance -bor
        [System.Reflection.BindingFlags]::NonPublic -bor
        [System.Reflection.BindingFlags]::Public) | ForEach-Object {
            Write-Host "  $($_.Name) : $($_.FieldType.Name)"
        }
}

# Look for animation-related enums
Write-Host "`n=== Animation Enums ===" -ForegroundColor Yellow
$animEnums = $types | Where-Object { $_.IsEnum -and $_.Name -match 'Anim' }
foreach ($enum in $animEnums) {
    Write-Host "`n  $($enum.Name):" -ForegroundColor Green
    [System.Enum]::GetNames($enum) | ForEach-Object {
        Write-Host "    - $_"
    }
}

# Look at CharacterCosmeticsDefinition
Write-Host "`n=== CharacterCosmeticsDefinition ===" -ForegroundColor Yellow
$cosmetics = $types | Where-Object { $_.Name -eq 'CharacterCosmeticsDefinition' }
if ($cosmetics) {
    Write-Host "  Fields:"
    $cosmetics.GetFields([System.Reflection.BindingFlags]::Instance -bor
        [System.Reflection.BindingFlags]::NonPublic -bor
        [System.Reflection.BindingFlags]::Public) | ForEach-Object {
            Write-Host "    $($_.Name) : $($_.FieldType.Name)"
        }
}

# Look for AnimatorOverrideController usage
Write-Host "`n=== Searching for AnimatorOverrideController usage ===" -ForegroundColor Yellow
$allMethods = $types | ForEach-Object {
    try {
        $_.GetMethods([System.Reflection.BindingFlags]::Instance -bor
            [System.Reflection.BindingFlags]::NonPublic -bor
            [System.Reflection.BindingFlags]::Public)
    } catch {}
} | Where-Object { $_ -ne $null }

$overrideMethods = $allMethods | Where-Object {
    $_.ToString() -match 'AnimatorOverrideController|RuntimeAnimatorController'
}
$overrideMethods | Select-Object -First 10 | ForEach-Object {
    Write-Host "  $($_.DeclaringType.Name).$($_.Name)"
}
