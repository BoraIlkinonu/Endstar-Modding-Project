# Deep Animation Runtime Analysis
$dllPath = "C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed"

Write-Host "=== DEEP ANIMATION RUNTIME ANALYSIS ===" -ForegroundColor Cyan

# Load all DLLs
Get-ChildItem $dllPath -Filter "*.dll" | ForEach-Object {
    try { [System.Reflection.Assembly]::LoadFrom($_.FullName) | Out-Null } catch {}
}

$gameplayAsm = [System.Reflection.Assembly]::LoadFrom("$dllPath\Gameplay.dll")
try { $types = $gameplayAsm.GetTypes() }
catch [System.Reflection.ReflectionTypeLoadException] { $types = $_.Exception.Types | Where-Object { $_ -ne $null } }

# 1. AppearanceAnimator - Full method analysis
Write-Host "`n=== AppearanceAnimator - ALL METHODS WITH DETAILS ===" -ForegroundColor Yellow
$appearanceAnimator = $types | Where-Object { $_.Name -eq 'AppearanceAnimator' }
if ($appearanceAnimator) {
    $methods = $appearanceAnimator.GetMethods([System.Reflection.BindingFlags]::Instance -bor
        [System.Reflection.BindingFlags]::NonPublic -bor
        [System.Reflection.BindingFlags]::Public -bor
        [System.Reflection.BindingFlags]::DeclaredOnly)

    foreach ($method in $methods) {
        $access = if ($method.IsPublic) { "public" } elseif ($method.IsPrivate) { "private" } else { "protected" }
        $params = ($method.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ", "
        Write-Host "  [$access] $($method.Name)($params) -> $($method.ReturnType.Name)" -ForegroundColor White
    }
}

# 2. AppearanceController - How it uses AppearanceAnimator
Write-Host "`n=== AppearanceController - METHODS ===" -ForegroundColor Yellow
$appearanceController = $types | Where-Object { $_.Name -eq 'AppearanceController' }
if ($appearanceController) {
    $methods = $appearanceController.GetMethods([System.Reflection.BindingFlags]::Instance -bor
        [System.Reflection.BindingFlags]::NonPublic -bor
        [System.Reflection.BindingFlags]::Public -bor
        [System.Reflection.BindingFlags]::DeclaredOnly)

    foreach ($method in $methods) {
        $access = if ($method.IsPublic) { "public" } elseif ($method.IsPrivate) { "private" } else { "protected" }
        $params = ($method.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ", "
        Write-Host "  [$access] $($method.Name)($params)" -ForegroundColor White
    }
}

# 3. Look for CharacterCosmetics related classes
Write-Host "`n=== CHARACTER COSMETICS CLASSES ===" -ForegroundColor Yellow
$cosmeticsTypes = $types | Where-Object { $_.Name -match 'Cosmetic' -and -not $_.Name.Contains('<') }
foreach ($t in $cosmeticsTypes) {
    Write-Host "`n  $($t.FullName)" -ForegroundColor Green

    # Fields
    $fields = $t.GetFields([System.Reflection.BindingFlags]::Instance -bor
        [System.Reflection.BindingFlags]::NonPublic -bor
        [System.Reflection.BindingFlags]::Public)
    if ($fields.Count -gt 0) {
        Write-Host "    Fields:" -ForegroundColor Cyan
        foreach ($f in $fields | Select-Object -First 10) {
            Write-Host "      $($f.Name) : $($f.FieldType.Name)"
        }
    }
}

# 4. Look for Animation setup/initialization
Write-Host "`n=== SEARCHING FOR ANIMATOR SETUP METHODS ===" -ForegroundColor Yellow
$allMethods = @()
foreach ($t in $types) {
    try {
        $methods = $t.GetMethods([System.Reflection.BindingFlags]::Instance -bor
            [System.Reflection.BindingFlags]::NonPublic -bor
            [System.Reflection.BindingFlags]::Public -bor
            [System.Reflection.BindingFlags]::DeclaredOnly)
        foreach ($m in $methods) {
            if ($m.Name -match 'Animator|Animation|InitCosmetics|SetupCharacter|LoadCosmetics') {
                $allMethods += [PSCustomObject]@{
                    Class = $t.Name
                    Method = $m.Name
                    Params = ($m.GetParameters() | ForEach-Object { $_.ParameterType.Name }) -join ", "
                }
            }
        }
    } catch {}
}
$allMethods | Sort-Object Class, Method | ForEach-Object {
    Write-Host "  $($_.Class).$($_.Method)($($_.Params))"
}

# 5. Look for AnimatorController or RuntimeAnimatorController usage
Write-Host "`n=== ANIMATOR CONTROLLER REFERENCES ===" -ForegroundColor Yellow
foreach ($t in $types) {
    try {
        $fields = $t.GetFields([System.Reflection.BindingFlags]::Instance -bor
            [System.Reflection.BindingFlags]::NonPublic -bor
            [System.Reflection.BindingFlags]::Public)
        foreach ($f in $fields) {
            if ($f.FieldType.Name -match 'AnimatorController|RuntimeAnimatorController|AnimatorOverrideController') {
                Write-Host "  $($t.Name).$($f.Name) : $($f.FieldType.Name)"
            }
        }
    } catch {}
}

# 6. NpcAnimator analysis
Write-Host "`n=== NpcAnimator (might show animation pattern) ===" -ForegroundColor Yellow
$npcAnimator = $types | Where-Object { $_.Name -eq 'NpcAnimator' }
if ($npcAnimator) {
    Write-Host "  Fields:" -ForegroundColor Cyan
    $npcAnimator.GetFields([System.Reflection.BindingFlags]::Instance -bor
        [System.Reflection.BindingFlags]::NonPublic -bor
        [System.Reflection.BindingFlags]::Public) | ForEach-Object {
            Write-Host "    $($_.Name) : $($_.FieldType.Name)"
        }

    Write-Host "  Methods:" -ForegroundColor Cyan
    $npcAnimator.GetMethods([System.Reflection.BindingFlags]::Instance -bor
        [System.Reflection.BindingFlags]::NonPublic -bor
        [System.Reflection.BindingFlags]::Public -bor
        [System.Reflection.BindingFlags]::DeclaredOnly) | ForEach-Object {
            $params = ($_.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ", "
            Write-Host "    $($_.Name)($params)"
        }
}

# 7. Look at how cosmetics are spawned and animator is assigned
Write-Host "`n=== COSMETICS SPAWNING/SETUP ===" -ForegroundColor Yellow
$spawnMethods = @()
foreach ($t in $types) {
    try {
        $methods = $t.GetMethods([System.Reflection.BindingFlags]::Instance -bor
            [System.Reflection.BindingFlags]::NonPublic -bor
            [System.Reflection.BindingFlags]::Public -bor
            [System.Reflection.BindingFlags]::DeclaredOnly)
        foreach ($m in $methods) {
            if ($m.Name -match 'Spawn|Instantiate|Create.*Cosmetic|Setup.*Appearance|Init.*Character') {
                $spawnMethods += [PSCustomObject]@{
                    Class = $t.Name
                    Method = $m.Name
                    ReturnType = $m.ReturnType.Name
                }
            }
        }
    } catch {}
}
$spawnMethods | Sort-Object Class | ForEach-Object {
    Write-Host "  $($_.Class).$($_.Method)() -> $($_.ReturnType)"
}

# 8. AnimationClipSet analysis - how clips are organized
Write-Host "`n=== AnimationClipSet & AnimationClipInfo ===" -ForegroundColor Yellow
$clipSet = $types | Where-Object { $_.Name -eq 'AnimationClipSet' }
if ($clipSet) {
    Write-Host "AnimationClipSet:" -ForegroundColor Green
    $clipSet.GetFields([System.Reflection.BindingFlags]::Instance -bor
        [System.Reflection.BindingFlags]::NonPublic -bor
        [System.Reflection.BindingFlags]::Public) | ForEach-Object {
            Write-Host "  $($_.Name) : $($_.FieldType.Name)"
        }
    $clipSet.GetMethods([System.Reflection.BindingFlags]::Instance -bor
        [System.Reflection.BindingFlags]::Public -bor
        [System.Reflection.BindingFlags]::DeclaredOnly) | ForEach-Object {
            Write-Host "  $($_.Name)()"
        }
}

$clipInfo = $types | Where-Object { $_.Name -eq 'AnimationClipInfo' }
if ($clipInfo) {
    Write-Host "`nAnimationClipInfo:" -ForegroundColor Green
    $clipInfo.GetFields([System.Reflection.BindingFlags]::Instance -bor
        [System.Reflection.BindingFlags]::NonPublic -bor
        [System.Reflection.BindingFlags]::Public) | ForEach-Object {
            Write-Host "  $($_.Name) : $($_.FieldType.Name)"
        }
}

Write-Host "`n=== ANALYSIS COMPLETE ===" -ForegroundColor Cyan
