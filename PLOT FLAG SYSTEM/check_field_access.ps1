# Check exact field modifiers for activePropLibrary and OnPropsRepopulated

$managedPath = 'C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed'
$gameplayDll = [System.Reflection.Assembly]::LoadFrom("$managedPath\Gameplay.dll")
$creatorDll = [System.Reflection.Assembly]::LoadFrom("$managedPath\Creator.dll")

$stageManager = $gameplayDll.GetType('Endless.Gameplay.LevelEditing.Level.StageManager')
$creatorManager = $creatorDll.GetType('Endless.Creator.CreatorManager')

Write-Host "=== StageManager.activePropLibrary ===" -ForegroundColor Cyan
$field = $stageManager.GetField('activePropLibrary', [System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
if ($field) {
    Write-Host "Found: $($field.Name)"
    Write-Host "IsPublic: $($field.IsPublic)"
    Write-Host "IsPrivate: $($field.IsPrivate)"
    Write-Host "IsFamily (protected): $($field.IsFamily)"
    Write-Host "Type: $($field.FieldType.Name)"
} else {
    Write-Host "NOT FOUND with NonPublic"

    # List ALL fields
    Write-Host ""
    Write-Host "All fields:" -ForegroundColor Yellow
    $allFields = $stageManager.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
    foreach ($f in $allFields | Where-Object { $_.Name -match 'prop|library' }) {
        Write-Host "  $($f.Name) - Public:$($f.IsPublic) Private:$($f.IsPrivate) Type:$($f.FieldType.Name)"
    }
}

Write-Host ""
Write-Host "=== CreatorManager.OnPropsRepopulated ===" -ForegroundColor Cyan
$field = $creatorManager.GetField('OnPropsRepopulated', [System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
if ($field) {
    Write-Host "Found: $($field.Name)"
    Write-Host "IsPublic: $($field.IsPublic)"
    Write-Host "IsPrivate: $($field.IsPrivate)"
    Write-Host "Type: $($field.FieldType.Name)"
} else {
    Write-Host "NOT FOUND"

    Write-Host ""
    Write-Host "All fields containing 'Prop' or 'Repopulate':" -ForegroundColor Yellow
    $allFields = $creatorManager.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
    foreach ($f in $allFields | Where-Object { $_.Name -match 'prop|repopulate' }) {
        Write-Host "  $($f.Name) - Public:$($f.IsPublic) Private:$($f.IsPrivate) Type:$($f.FieldType.Name)"
    }
}
