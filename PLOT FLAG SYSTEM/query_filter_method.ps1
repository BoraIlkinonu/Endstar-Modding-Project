# Check HandleFilterAndSort and other refresh methods for their signatures
$asm = [System.Reflection.Assembly]::LoadFrom('C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\Shared.dll')

# Find UIBaseLocalFilterableListModel generic type
$types = $asm.GetTypes() | Where-Object { $_.Name -like '*UIBaseLocalFilterableListModel*' }
foreach ($t in $types) {
    Write-Host "Type: $($t.FullName)"
    $methods = $t.GetMethods([System.Reflection.BindingFlags]'Public,NonPublic,Instance,DeclaredOnly')
    foreach ($m in $methods) {
        $params = $m.GetParameters()
        $paramStr = ($params | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ', '
        if ($m.Name -match 'Filter|Sort|Refresh|Handle|Add') {
            Write-Host "  $($m.Name)($paramStr) : $($m.ReturnType.Name)"
        }
    }
}
