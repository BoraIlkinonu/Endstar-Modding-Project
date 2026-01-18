$asm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Gameplay.dll')
$pl = $asm.GetType('Endless.Gameplay.LevelEditing.PropLibrary')

Write-Host "=== GetReferenceFilteredDefinitionList Method ===" -ForegroundColor Cyan
$grfMethod = $pl.GetMethod('GetReferenceFilteredDefinitionList', [System.Reflection.BindingFlags]'Public,NonPublic,Instance')
if ($grfMethod) {
    $params = $grfMethod.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }
    Write-Host "  Signature: $($grfMethod.ReturnType.Name) GetReferenceFilteredDefinitionList($($params -join ', '))"

    $body = $grfMethod.GetMethodBody()
    $il = $body.GetILAsByteArray()
    Write-Host "  IL size: $($il.Length) bytes"
    Write-Host "  Local vars: $($body.LocalVariables.Count)"
} else {
    Write-Host "  NOT FOUND"
    Write-Host "  Listing all methods..."
    foreach ($m in $pl.GetMethods([System.Reflection.BindingFlags]'Public,NonPublic,Instance')) {
        if ($m.Name -match 'Filter|Reference|Get') {
            $params = $m.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }
            Write-Host "    $($m.ReturnType.Name) $($m.Name)($($params -join ', '))"
        }
    }
}

Write-Host ""
Write-Host "=== dynamicFilters Field - How it's initialized ===" -ForegroundColor Cyan
# Look at constructor
$ctor = $pl.GetConstructor([System.Reflection.BindingFlags]'Public,NonPublic,Instance', $null, @(), $null)
if ($ctor) {
    Write-Host "  Found constructor"
    $body = $ctor.GetMethodBody()
    if ($body) {
        $il = $body.GetILAsByteArray()
        Write-Host "  Constructor IL size: $($il.Length) bytes"
    }
} else {
    Write-Host "  No parameterless constructor"

    # Check all constructors
    foreach ($c in $pl.GetConstructors([System.Reflection.BindingFlags]'Public,NonPublic,Instance')) {
        $params = $c.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }
        Write-Host "    Constructor: ($($params -join ', '))"
    }
}

Write-Host ""
Write-Host "=== Checking if dynamicFilters has a default value in field definition ===" -ForegroundColor Cyan
$dfField = $pl.GetField('dynamicFilters', [System.Reflection.BindingFlags]'Public,NonPublic,Instance')
if ($dfField) {
    Write-Host "  Field type: $($dfField.FieldType.FullName)"
    Write-Host "  IsInitOnly: $($dfField.IsInitOnly)"
    Write-Host "  IsLiteral: $($dfField.IsLiteral)"

    # Check for SerializeField or field attributes
    $attrs = $dfField.GetCustomAttributes($true)
    Write-Host "  Attributes: $($attrs.Count)"
    foreach ($attr in $attrs) {
        Write-Host "    - $($attr.GetType().Name)"
    }
}

Write-Host ""
Write-Host "=== UI Side: UIRuntimePropInfoListModel.Synchronize ===" -ForegroundColor Cyan
$creatorAsm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Creator.dll')
$uiModel = $creatorAsm.GetType('Endless.Creator.UI.UIRuntimePropInfoListModel')
if ($uiModel) {
    $syncMethod = $uiModel.GetMethod('Synchronize', [System.Reflection.BindingFlags]'Public,NonPublic,Instance')
    if ($syncMethod) {
        $params = $syncMethod.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }
        Write-Host "  Signature: $($syncMethod.ReturnType.Name) Synchronize($($params -join ', '))"
    }
} else {
    Write-Host "  UIRuntimePropInfoListModel NOT FOUND"
}
