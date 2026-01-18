# Find where props are loaded from - search for Manager/Service types
$managedPath = 'C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed'

Write-Host "=== Searching for PropLibrary consumers ===" -ForegroundColor Cyan

# Load assemblies
$gameplayAsm = [System.Reflection.Assembly]::LoadFrom("$managedPath\Gameplay.dll")
$creatorAsm = [System.Reflection.Assembly]::LoadFrom("$managedPath\Creator.dll")

# Check CreatorManager for how it gets props
Write-Host "`n=== CreatorManager - how does it load props? ===" -ForegroundColor Yellow
$creatorMgrType = $creatorAsm.GetType('Endless.Creator.CreatorManager')
if ($creatorMgrType) {
    Write-Host "Fields:"
    $creatorMgrType.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance) | ForEach-Object {
        Write-Host "  $($_.FieldType.Name) $($_.Name)"
    } | Select-Object -First 30
}

Write-Host "`n=== Searching for 'Service' or 'Provider' types ===" -ForegroundColor Cyan
Get-ChildItem "$managedPath\*.dll" | ForEach-Object {
    try {
        $asm = [System.Reflection.Assembly]::LoadFrom($_.FullName)
        $types = $asm.GetTypes() 2>$null
        $svcTypes = $types | Where-Object {
            ($_.Name -like '*AssetService*' -or $_.Name -like '*PropService*' -or $_.Name -like '*ContentService*' -or $_.Name -like '*LibraryService*') -and -not $_.Name.Contains('+')
        }
        if ($svcTypes) {
            Write-Host "`n[$($_.Name)]:" -ForegroundColor Yellow
            $svcTypes | ForEach-Object { Write-Host "  $($_.FullName)" }
        }
    } catch {}
}

Write-Host "`n=== Prop.prefabBundle - How is it resolved? ===" -ForegroundColor Cyan
# Check AssetReference type
$assetsAsm = [System.Reflection.Assembly]::LoadFrom("$managedPath\Assets.dll")
$assetRefType = $assetsAsm.GetType('Endless.Assets.AssetReference')
if ($assetRefType) {
    Write-Host "AssetReference methods:"
    $assetRefType.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::DeclaredOnly) | ForEach-Object {
        Write-Host "  $($_.ReturnType.Name) $($_.Name)"
    }
}

Write-Host "`n=== BaseTypeReferences (Props.dll) ===" -ForegroundColor Cyan
$propsAsm = [System.Reflection.Assembly]::LoadFrom("$managedPath\Props.dll")
$baseTypeRefType = $propsAsm.GetType('Endless.Props.ReferenceComponents.BaseTypeReferences')
if ($baseTypeRefType) {
    Write-Host "Fields:"
    $baseTypeRefType.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::Static) | ForEach-Object {
        Write-Host "  $($_.FieldType.Name) $($_.Name)"
    }
    Write-Host "Methods:"
    $baseTypeRefType.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Static -bor [System.Reflection.BindingFlags]::DeclaredOnly) | ForEach-Object {
        Write-Host "  $($_.ReturnType.Name) $($_.Name)"
    }
}
