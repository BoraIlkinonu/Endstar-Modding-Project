# Find ReferenceFilter enum and analyze CalculateReferenceFilter

$gameplayAsm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Gameplay.dll')

Write-Host "=== ReferenceFilter enum (direct lookup) ===" -ForegroundColor Cyan
$rfType = $gameplayAsm.GetType('Endless.Gameplay.ReferenceFilter')
if ($rfType) {
    Write-Host "Found at: $($rfType.FullName)"
    Write-Host "Is Enum: $($rfType.IsEnum)"
    if ($rfType.IsEnum) {
        foreach ($name in [System.Enum]::GetNames($rfType)) {
            $val = [System.Enum]::Parse($rfType, $name)
            Write-Host "  $name = $([int]$val)"
        }
    }
} else {
    Write-Host "Not found at Endless.Gameplay.ReferenceFilter"

    # Try other namespaces
    $rfType = $gameplayAsm.GetType('Endless.Gameplay.LevelEditing.ReferenceFilter')
    if ($rfType) {
        Write-Host "Found at: $($rfType.FullName)"
    }

    $rfType = $gameplayAsm.GetType('Endless.Gameplay.Scripting.ReferenceFilter')
    if ($rfType) {
        Write-Host "Found at: $($rfType.FullName)"
    }
}

Write-Host ""
Write-Host "=== EndlessProp.CalculateReferenceFilter parameters ===" -ForegroundColor Cyan
$epType = $gameplayAsm.GetType('Endless.Gameplay.Scripting.EndlessProp')
$calcMethod = $epType.GetMethod('CalculateReferenceFilter', [System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
if ($calcMethod) {
    Write-Host "Return type: $($calcMethod.ReturnType.FullName)"
    Write-Host "Parameters:"
    foreach ($p in $calcMethod.GetParameters()) {
        Write-Host "  $($p.ParameterType.FullName) $($p.Name)"
    }
}

Write-Host ""
Write-Host "=== Looking at EndlessProp constructor and initialization ===" -ForegroundColor Cyan
$ctors = $epType.GetConstructors([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
foreach ($c in $ctors) {
    Write-Host "Constructor:"
    foreach ($p in $c.GetParameters()) {
        Write-Host "  $($p.ParameterType.Name) $($p.Name)"
    }
}

Write-Host ""
Write-Host "=== EndlessProp.Initialize or Setup methods ===" -ForegroundColor Cyan
$initMethods = $epType.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance) | Where-Object { $_.Name -like '*Init*' -or $_.Name -like '*Setup*' -or $_.Name -like '*Load*' }
foreach ($m in $initMethods) {
    Write-Host "$($m.ReturnType.Name) $($m.Name)("
    foreach ($p in $m.GetParameters()) {
        Write-Host "  $($p.ParameterType.Name) $($p.Name)"
    }
    Write-Host ")"
}

Write-Host ""
Write-Host "=== EndlessProp.Prop property setter ===" -ForegroundColor Cyan
$propProp = $epType.GetProperty('Prop', [System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance)
if ($propProp) {
    Write-Host "Property type: $($propProp.PropertyType.FullName)"
    Write-Host "Can read: $($propProp.CanRead)"
    Write-Host "Can write: $($propProp.CanWrite)"
    $setter = $propProp.GetSetMethod($true)
    if ($setter) {
        Write-Host "Has private setter: True"
    }
}

Write-Host ""
Write-Host "=== What baseTypeId values might exist (looking at Props.dll) ===" -ForegroundColor Cyan
$propsAsm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Props.dll')
# Look for any constants or enums related to base types
$propType = $propsAsm.GetType('Endless.Props.Assets.Prop')
Write-Host "Prop.baseTypeId field:"
$btField = $propType.GetField('baseTypeId', [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
if ($btField) {
    Write-Host "  Type: $($btField.FieldType.FullName)"
}
Write-Host "Prop.BaseTypeId property:"
$btProp = $propType.GetProperty('BaseTypeId', [System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance)
if ($btProp) {
    Write-Host "  Type: $($btProp.PropertyType.FullName)"
    Write-Host "  Can read: $($btProp.CanRead)"
}
