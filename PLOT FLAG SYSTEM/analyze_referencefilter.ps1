$asm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Gameplay.dll')
$plType = $asm.GetType('Endless.Gameplay.LevelEditing.PropLibrary')

Write-Host "=== PropLibrary Fields ===" -ForegroundColor Cyan
$plType.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance) | ForEach-Object {
    Write-Host "  $($_.FieldType.FullName) $($_.Name)"
}

Write-Host "`n=== ReferenceFilter Class ===" -ForegroundColor Cyan
$rfType = $asm.GetType('Endless.Gameplay.LevelEditing.ReferenceFilter')
if ($rfType) {
    Write-Host "Fields:"
    $rfType.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance) | ForEach-Object {
        Write-Host "  $($_.FieldType.Name) $($_.Name)"
    }
}

# Look for base type related things
Write-Host "`n=== PropRequirements in StageManager ===" -ForegroundColor Cyan
$smType = $asm.GetType('Endless.Gameplay.LevelEditing.Level.StageManager')
$smType.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance) | Where-Object {
    $_.Name -like '*Requirement*' -or $_.Name -like '*baseType*' -or $_.Name -like '*lookup*'
} | ForEach-Object {
    Write-Host "  $($_.FieldType.FullName) $($_.Name)"
}

# Check what BuildBaseTypeRequirementsLookup does
Write-Host "`n=== BuildBaseTypeRequirementsLookup ===" -ForegroundColor Cyan
$buildMethod = $smType.GetMethod('BuildBaseTypeRequirementsLookup', [System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
if ($buildMethod) {
    Write-Host "Return: $($buildMethod.ReturnType.FullName)"
    Write-Host "Parameters:"
    foreach ($p in $buildMethod.GetParameters()) {
        Write-Host "  $($p.ParameterType.FullName) $($p.Name)"
    }
}

# Check InjectProp to see what it validates
Write-Host "`n=== Looking for how InjectProp adds to map ===" -ForegroundColor Cyan
$injectMethod = $plType.GetMethod('InjectProp', [System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance)
if ($injectMethod) {
    Write-Host "Method: $($injectMethod.Name)"
    Write-Host "Return: $($injectMethod.ReturnType.FullName)"
    Write-Host "Parameters:"
    foreach ($p in $injectMethod.GetParameters()) {
        Write-Host "  $($p.ParameterType.FullName) $($p.Name)"
    }
}
