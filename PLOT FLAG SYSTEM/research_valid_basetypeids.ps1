# Research: What valid baseTypeId values exist in the game?
# Need to find real baseTypeId values from loaded props

$gameplayAsm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Gameplay.dll')
$propsAsm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Props.dll')

Write-Host "=== Looking for BaseTypeDefinition class ===" -ForegroundColor Cyan

# BaseTypeDefinition might define what baseTypeIds exist
foreach ($t in $propsAsm.GetTypes()) {
    if ($t.Name -match 'BaseType') {
        Write-Host "  Found: $($t.FullName)"

        # Check for fields/properties that might list available baseTypeIds
        foreach ($f in $t.GetFields([System.Reflection.BindingFlags]'Public,NonPublic,Instance,Static')) {
            Write-Host "    Field: $($f.Name) : $($f.FieldType.Name)"
        }
    }
}

Write-Host ""
Write-Host "=== Check ComponentDefinition (includes baseType info) ===" -ForegroundColor Cyan
$compDef = $propsAsm.GetType('Endless.Props.ComponentDefinition')
if ($compDef) {
    Write-Host "Found ComponentDefinition"
    foreach ($f in $compDef.GetFields([System.Reflection.BindingFlags]'Public,NonPublic,Instance')) {
        Write-Host "  Field: $($f.Name) : $($f.FieldType.Name)"
    }
}

Write-Host ""
Write-Host "=== Check BaseTypeDefinition ===" -ForegroundColor Cyan
$baseTypeDef = $propsAsm.GetType('Endless.Props.BaseTypeDefinition')
if ($baseTypeDef) {
    Write-Host "Found BaseTypeDefinition"
    foreach ($f in $baseTypeDef.GetFields([System.Reflection.BindingFlags]'Public,NonPublic,Instance')) {
        Write-Host "  Field: $($f.Name) : $($f.FieldType.Name)"
    }

    # Check for ID or Name field
    $idField = $baseTypeDef.GetProperty('id', [System.Reflection.BindingFlags]'Public,NonPublic,Instance')
    if ($idField) {
        Write-Host "  Has 'id' property: $($idField.PropertyType.Name)"
    }
}

Write-Host ""
Write-Host "=== Check PropLibrary for baseType lookup methods ===" -ForegroundColor Cyan
$pl = $gameplayAsm.GetType('Endless.Gameplay.LevelEditing.PropLibrary')
foreach ($m in $pl.GetMethods([System.Reflection.BindingFlags]'Public,NonPublic,Instance,Static')) {
    if ($m.Name -match 'BaseType' -or $m.Name -match 'Definition') {
        $params = $m.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }
        Write-Host "  $($m.ReturnType.Name) $($m.Name)($($params -join ', '))"
    }
}

Write-Host ""
Write-Host "=== Check for PropLibrary static fields that might hold definitions ===" -ForegroundColor Cyan
foreach ($f in $pl.GetFields([System.Reflection.BindingFlags]'Public,NonPublic,Static')) {
    Write-Host "  Static: $($f.Name) : $($f.FieldType.Name)"
}

Write-Host ""
Write-Host "=== Looking for AssetManager or similar that holds definitions ===" -ForegroundColor Cyan
foreach ($t in $gameplayAsm.GetTypes()) {
    if ($t.Name -match 'AssetManager' -or $t.Name -match 'DefinitionManager' -or $t.Name -match 'PropManager') {
        Write-Host "  Found: $($t.FullName)"
    }
}

# Also check Assets.dll
$assetsAsm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Assets.dll')
foreach ($t in $assetsAsm.GetTypes()) {
    if ($t.Name -match 'BaseType' -or $t.Name -match 'PropDefinition') {
        Write-Host "  Assets.dll: $($t.FullName)"
    }
}
