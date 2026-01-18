$managedPath = 'C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed'

function Get-TypesSafely($assembly) {
    try { return $assembly.GetTypes() }
    catch [System.Reflection.ReflectionTypeLoadException] {
        return $_.Exception.Types | Where-Object { $_ -ne $null }
    }
}

$dlls = Get-ChildItem "$managedPath\*.dll" | Select-Object -First 50

foreach ($dll in $dlls) {
    try {
        $asm = [System.Reflection.Assembly]::LoadFrom($dll.FullName)
        $types = Get-TypesSafely $asm
        $guidType = $types | Where-Object { $_.Name -eq 'SerializableGuid' }
        if ($guidType) {
            Write-Host "FOUND in $($dll.Name): $($guidType.FullName)" -ForegroundColor Green

            Write-Host "  Fields:" -ForegroundColor Yellow
            $fields = $guidType.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
            foreach ($f in $fields) {
                Write-Host "    $($f.FieldType.Name) $($f.Name)"
            }

            Write-Host "  Constructors:" -ForegroundColor Yellow
            $ctors = $guidType.GetConstructors()
            foreach ($c in $ctors) {
                $params = ($c.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ', '
                Write-Host "    ctor($params)"
            }

            Write-Host "  Key Methods:" -ForegroundColor Yellow
            $methods = $guidType.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Static -bor [System.Reflection.BindingFlags]::DeclaredOnly)
            foreach ($m in $methods | Select-Object -First 10) {
                $params = ($m.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ', '
                Write-Host "    static $($m.ReturnType.Name) $($m.Name)($params)"
            }
        }
    }
    catch {
        # Skip errors
    }
}
