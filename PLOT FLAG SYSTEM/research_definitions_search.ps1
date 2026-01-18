# Search for ComponentDefinition and BaseTypeDefinition across all DLLs

$dllPath = "C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed"

$dlls = @(
    "$dllPath\Props.dll",
    "$dllPath\Gameplay.dll",
    "$dllPath\Assets.dll",
    "$dllPath\Creator.dll",
    "$dllPath\Shared.dll"
)

Write-Host "=== Searching for ComponentDefinition ===" -ForegroundColor Cyan

foreach ($dll in $dlls) {
    try {
        $asm = [System.Reflection.Assembly]::LoadFrom($dll)
        $found = $false
        foreach ($t in $asm.GetExportedTypes()) {
            if ($t.Name -eq 'ComponentDefinition') {
                Write-Host "Found in: $dll"
                Write-Host "  Full name: $($t.FullName)"
                $found = $true

                Write-Host "  Fields:"
                foreach ($f in $t.GetFields([System.Reflection.BindingFlags]'Public,NonPublic,Instance')) {
                    Write-Host "    $($f.Name) : $($f.FieldType.Name)"
                }
            }
        }
    } catch {
        # Skip load errors
    }
}

Write-Host ""
Write-Host "=== Searching for BaseTypeDefinition ===" -ForegroundColor Cyan

foreach ($dll in $dlls) {
    try {
        $asm = [System.Reflection.Assembly]::LoadFrom($dll)
        foreach ($t in $asm.GetExportedTypes()) {
            if ($t.Name -eq 'BaseTypeDefinition') {
                Write-Host "Found in: $dll"
                Write-Host "  Full name: $($t.FullName)"

                Write-Host "  Fields:"
                foreach ($f in $t.GetFields([System.Reflection.BindingFlags]'Public,NonPublic,Instance')) {
                    Write-Host "    $($f.Name) : $($f.FieldType.Name)"
                }
            }
        }
    } catch {
        # Skip load errors
    }
}

Write-Host ""
Write-Host "=== Check Prop fields more carefully ===" -ForegroundColor Cyan
$propsAsm = [System.Reflection.Assembly]::LoadFrom("$dllPath\Props.dll")
$prop = $propsAsm.GetType('Endless.Props.Assets.Prop')
if ($prop) {
    Write-Host "Prop.baseTypeId field:"
    $baseTypeIdField = $prop.GetField('baseTypeId', [System.Reflection.BindingFlags]'Public,NonPublic,Instance')
    if ($baseTypeIdField) {
        Write-Host "  Type: $($baseTypeIdField.FieldType.FullName)"
    }
}

Write-Host ""
Write-Host "=== Check what existing props have for baseTypeId at runtime ===" -ForegroundColor Cyan
Write-Host "(This requires runtime access - we can only analyze statically here)"
Write-Host ""
Write-Host "From injection code, _validBaseTypeId is obtained from:"
Write-Host "  GetValidBaseTypeId(loadedPropMap) - reads first prop's BaseTypeId"
Write-Host ""
Write-Host "Fallback: 'treasure'"
