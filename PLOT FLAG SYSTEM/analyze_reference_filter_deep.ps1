# Deep analysis of ReferenceFilter and how EndlessProp gets its value

$gameplayAsm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Gameplay.dll')

Write-Host "=== Finding ReferenceFilter type ===" -ForegroundColor Cyan
$allTypes = $gameplayAsm.GetTypes()
$rfTypes = $allTypes | Where-Object { $_.Name -eq 'ReferenceFilter' }
foreach ($t in $rfTypes) {
    Write-Host "Found: $($t.FullName)"
    if ($t.IsEnum) {
        Write-Host "  Is Enum"
        foreach ($name in [System.Enum]::GetNames($t)) {
            $val = [System.Enum]::Parse($t, $name)
            Write-Host "    $name = $([int]$val)"
        }
    }
}

Write-Host ""
Write-Host "=== EndlessProp.ReferenceFilter setter ===" -ForegroundColor Cyan
$epType = $gameplayAsm.GetType('Endless.Gameplay.Scripting.EndlessProp')
$rfProp = $epType.GetProperty('ReferenceFilter', [System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance)
if ($rfProp) {
    Write-Host "Property type: $($rfProp.PropertyType.FullName)"
    Write-Host "Can read: $($rfProp.CanRead)"
    Write-Host "Can write: $($rfProp.CanWrite)"
    $getter = $rfProp.GetGetMethod()
    $setter = $rfProp.GetSetMethod()
    Write-Host "Has getter: $($getter -ne $null)"
    Write-Host "Has setter: $($setter -ne $null)"
}

Write-Host ""
Write-Host "=== Searching for methods that SET ReferenceFilter ===" -ForegroundColor Cyan
# Look at EndlessProp methods
$methods = $epType.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
foreach ($m in $methods) {
    if ($m.Name -like '*Reference*' -or $m.Name -like '*Filter*' -or $m.Name -like '*Init*' -or $m.Name -like '*Setup*' -or $m.Name -like '*Configure*') {
        Write-Host "$($m.Name)("
        foreach ($p in $m.GetParameters()) {
            Write-Host "  $($p.ParameterType.Name) $($p.Name)"
        }
        Write-Host ")"
    }
}

Write-Host ""
Write-Host "=== PropLibrary - all methods (for understanding flow) ===" -ForegroundColor Cyan
$propLibType = $gameplayAsm.GetType('Endless.Gameplay.LevelEditing.PropLibrary')
$methods = $propLibType.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::DeclaredOnly)
foreach ($m in $methods) {
    $params = ($m.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ', '
    Write-Host "  $($m.ReturnType.Name) $($m.Name)($params)"
}

Write-Host ""
Write-Host "=== Looking for GetReferenceFilteredDefinitionList ===" -ForegroundColor Cyan
$getRefMethod = $propLibType.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance) | Where-Object { $_.Name -eq 'GetReferenceFilteredDefinitionList' }
foreach ($m in $getRefMethod) {
    Write-Host "Method: $($m.Name)"
    Write-Host "Return: $($m.ReturnType.FullName)"
    foreach ($p in $m.GetParameters()) {
        Write-Host "  Param: $($p.ParameterType.FullName) $($p.Name)"
    }
}
