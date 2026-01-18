# Deep Analysis of Endstar Prop Tool UI System - V2
# Handles missing dependencies properly

$managedPath = "C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed"
$outputFile = "D:\Endstar Plot Flag\PLOT FLAG SYSTEM\Zayed_Character_Mod\CustomProps\PROP_TOOL_UI_ANALYSIS.md"

$report = @()

function Add-Report($text) {
    $script:report += $text
    Write-Host $text
}

function Get-TypesSafely($assembly) {
    try {
        return $assembly.GetTypes()
    }
    catch [System.Reflection.ReflectionTypeLoadException] {
        # Return the types that could be loaded
        return $_.Exception.Types | Where-Object { $_ -ne $null }
    }
    catch {
        Write-Host "Error loading types from $($assembly.GetName().Name): $($_.Exception.Message)" -ForegroundColor Yellow
        return @()
    }
}

Add-Report "# ENDSTAR PROP TOOL UI DEEP ANALYSIS"
Add-Report "Generated: $(Get-Date)"
Add-Report ""

# Load ALL DLLs from managed folder to satisfy dependencies
Write-Host "Loading ALL assemblies from Managed folder..." -ForegroundColor Cyan

$allDlls = Get-ChildItem "$managedPath\*.dll"
$loadedAssemblies = @{}

foreach ($dll in $allDlls) {
    try {
        $asm = [System.Reflection.Assembly]::LoadFrom($dll.FullName)
        $loadedAssemblies[$dll.Name] = $asm
    }
    catch {
        # Ignore load errors for dependencies
    }
}

Write-Host "Loaded $($loadedAssemblies.Count) assemblies" -ForegroundColor Green

# Get key assemblies
$creatorDll = $loadedAssemblies["Creator.dll"]
$gameplayDll = $loadedAssemblies["Gameplay.dll"]
$propsDll = $loadedAssemblies["Props.dll"]
$assetsDll = $loadedAssemblies["Assets.dll"]

Add-Report "## LOADED ASSEMBLIES"
Add-Report ""
Add-Report "| Assembly | Status |"
Add-Report "|----------|--------|"
Add-Report "| Creator.dll | $(if ($creatorDll) { 'Loaded' } else { 'MISSING' }) |"
Add-Report "| Gameplay.dll | $(if ($gameplayDll) { 'Loaded' } else { 'MISSING' }) |"
Add-Report "| Props.dll | $(if ($propsDll) { 'Loaded' } else { 'MISSING' }) |"
Add-Report "| Assets.dll | $(if ($assetsDll) { 'Loaded' } else { 'MISSING' }) |"
Add-Report ""

# ============================================
# SECTION 1: Creator.dll Types
# ============================================
Add-Report "## SECTION 1: CREATOR.DLL ALL TYPES"
Add-Report ""

if ($creatorDll) {
    $creatorTypes = Get-TypesSafely $creatorDll
    Add-Report "Total types loaded from Creator.dll: $($creatorTypes.Count)"
    Add-Report ""

    # Group by namespace
    $namespaces = $creatorTypes | Group-Object { $_.Namespace } | Sort-Object Name
    Add-Report "### Namespaces:"
    Add-Report '```'
    foreach ($ns in $namespaces) {
        Add-Report "$($ns.Name): $($ns.Count) types"
    }
    Add-Report '```'
    Add-Report ""

    # Find Prop-related types
    $propTypes = $creatorTypes | Where-Object { $_.Name -match "Prop" } | Sort-Object Name
    Add-Report "### Prop-related types ($($propTypes.Count)):"
    Add-Report '```'
    foreach ($type in $propTypes) {
        Add-Report "$($type.FullName)"
    }
    Add-Report '```'
    Add-Report ""

    # Find Tool-related types
    $toolTypes = $creatorTypes | Where-Object { $_.Name -match "Tool" } | Sort-Object Name
    Add-Report "### Tool-related types ($($toolTypes.Count)):"
    Add-Report '```'
    foreach ($type in $toolTypes) {
        Add-Report "$($type.FullName)"
    }
    Add-Report '```'
    Add-Report ""

    # Find UI types
    $uiTypes = $creatorTypes | Where-Object { $_.Name -match "^UI" } | Sort-Object Name
    Add-Report "### UI types ($($uiTypes.Count)):"
    Add-Report '```'
    foreach ($type in $uiTypes) {
        Add-Report "$($type.FullName)"
    }
    Add-Report '```'
    Add-Report ""

    # Find Panel/View types
    $panelTypes = $creatorTypes | Where-Object { $_.Name -match "Panel|View" } | Sort-Object Name
    Add-Report "### Panel/View types ($($panelTypes.Count)):"
    Add-Report '```'
    foreach ($type in $panelTypes) {
        Add-Report "$($type.FullName)"
    }
    Add-Report '```'
    Add-Report ""

    # Find Controller types
    $controllerTypes = $creatorTypes | Where-Object { $_.Name -match "Controller" } | Sort-Object Name
    Add-Report "### Controller types ($($controllerTypes.Count)):"
    Add-Report '```'
    foreach ($type in $controllerTypes) {
        Add-Report "$($type.FullName)"
    }
    Add-Report '```'
    Add-Report ""

    # Find Manager types
    $managerTypes = $creatorTypes | Where-Object { $_.Name -match "Manager" } | Sort-Object Name
    Add-Report "### Manager types ($($managerTypes.Count)):"
    Add-Report '```'
    foreach ($type in $managerTypes) {
        Add-Report "$($type.FullName)"
    }
    Add-Report '```'
    Add-Report ""
}

# ============================================
# SECTION 2: Gameplay.dll Types
# ============================================
Add-Report "## SECTION 2: GAMEPLAY.DLL ALL TYPES"
Add-Report ""

if ($gameplayDll) {
    $gameplayTypes = Get-TypesSafely $gameplayDll
    Add-Report "Total types loaded from Gameplay.dll: $($gameplayTypes.Count)"
    Add-Report ""

    # Group by namespace
    $namespaces = $gameplayTypes | Group-Object { $_.Namespace } | Sort-Object Name
    Add-Report "### Namespaces:"
    Add-Report '```'
    foreach ($ns in $namespaces) {
        Add-Report "$($ns.Name): $($ns.Count) types"
    }
    Add-Report '```'
    Add-Report ""

    # Find PropLibrary
    $propLibType = $gameplayTypes | Where-Object { $_.Name -eq "PropLibrary" }
    if ($propLibType) {
        Add-Report "### PropLibrary FOUND"
        Add-Report "Full Name: $($propLibType.FullName)"
        Add-Report "Base Type: $($propLibType.BaseType.FullName)"
        Add-Report ""

        # Fields
        try {
            $fields = $propLibType.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::Static)
            Add-Report "#### Fields ($($fields.Count)):"
            Add-Report '```'
            foreach ($field in $fields) {
                Add-Report "  $($field.FieldType.Name) $($field.Name)"
            }
            Add-Report '```'
        }
        catch {
            Add-Report "Error getting fields: $($_.Exception.Message)"
        }

        # Methods
        try {
            $methods = $propLibType.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::DeclaredOnly) | Where-Object { -not $_.IsSpecialName }
            Add-Report ""
            Add-Report "#### Methods ($($methods.Count)):"
            Add-Report '```'
            foreach ($method in $methods) {
                try {
                    $params = ($method.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ", "
                    Add-Report "  $($method.ReturnType.Name) $($method.Name)($params)"
                }
                catch {
                    Add-Report "  $($method.Name)(...)"
                }
            }
            Add-Report '```'
        }
        catch {
            Add-Report "Error getting methods: $($_.Exception.Message)"
        }

        # Nested types
        try {
            $nestedTypes = $propLibType.GetNestedTypes([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic)
            if ($nestedTypes.Count -gt 0) {
                Add-Report ""
                Add-Report "#### Nested Types ($($nestedTypes.Count)):"
                foreach ($nested in $nestedTypes) {
                    Add-Report ""
                    Add-Report "##### $($nested.Name)"
                    Add-Report '```'
                    try {
                        $nFields = $nested.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
                        foreach ($field in $nFields) {
                            Add-Report "  $($field.FieldType.Name) $($field.Name)"
                        }
                    }
                    catch {
                        Add-Report "  Error getting nested fields"
                    }
                    Add-Report '```'
                }
            }
        }
        catch {
            Add-Report "Error getting nested types: $($_.Exception.Message)"
        }
        Add-Report ""
    }

    # Find StageManager
    $stageManagerType = $gameplayTypes | Where-Object { $_.Name -eq "StageManager" }
    if ($stageManagerType) {
        Add-Report "### StageManager FOUND"
        Add-Report "Full Name: $($stageManagerType.FullName)"
        Add-Report "Base Type: $($stageManagerType.BaseType.Name)"
        Add-Report ""

        # Fields
        try {
            $fields = $stageManagerType.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::Static)
            Add-Report "#### Fields ($($fields.Count)):"
            Add-Report '```'
            foreach ($field in $fields) {
                Add-Report "  $($field.FieldType.Name) $($field.Name)"
            }
            Add-Report '```'
        }
        catch {
            Add-Report "Error getting fields: $($_.Exception.Message)"
        }

        # Methods
        try {
            $methods = $stageManagerType.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::DeclaredOnly) | Where-Object { -not $_.IsSpecialName }
            Add-Report ""
            Add-Report "#### Public Methods ($($methods.Count)):"
            Add-Report '```'
            foreach ($method in $methods) {
                try {
                    $params = ($method.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ", "
                    Add-Report "  $($method.ReturnType.Name) $($method.Name)($params)"
                }
                catch {
                    Add-Report "  $($method.Name)(...)"
                }
            }
            Add-Report '```'
        }
        catch {
            Add-Report "Error getting methods: $($_.Exception.Message)"
        }
        Add-Report ""
    }

    # Find Prop-related types
    $propTypes = $gameplayTypes | Where-Object { $_.Name -match "Prop" } | Sort-Object Name
    Add-Report "### Prop-related types in Gameplay.dll ($($propTypes.Count)):"
    Add-Report '```'
    foreach ($type in $propTypes) {
        Add-Report "$($type.FullName)"
    }
    Add-Report '```'
    Add-Report ""

    # Find Stage-related types
    $stageTypes = $gameplayTypes | Where-Object { $_.Name -match "Stage" } | Sort-Object Name
    Add-Report "### Stage-related types ($($stageTypes.Count)):"
    Add-Report '```'
    foreach ($type in $stageTypes) {
        Add-Report "$($type.FullName)"
    }
    Add-Report '```'
    Add-Report ""
}

# ============================================
# SECTION 3: Props.dll Types
# ============================================
Add-Report "## SECTION 3: PROPS.DLL ALL TYPES"
Add-Report ""

if ($propsDll) {
    $propsTypes = Get-TypesSafely $propsDll
    Add-Report "Total types loaded from Props.dll: $($propsTypes.Count)"
    Add-Report ""

    Add-Report "### All types in Props.dll:"
    Add-Report '```'
    foreach ($type in $propsTypes | Sort-Object Name) {
        Add-Report "$($type.FullName)"
    }
    Add-Report '```'
    Add-Report ""

    # Analyze Prop class
    $propClass = $propsTypes | Where-Object { $_.Name -eq "Prop" }
    if ($propClass) {
        Add-Report "### Prop Class DETAILED ANALYSIS"
        Add-Report "Full Name: $($propClass.FullName)"
        Add-Report "Base Type: $($propClass.BaseType.FullName)"
        Add-Report ""

        # Fields
        try {
            $fields = $propClass.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
            Add-Report "#### Fields ($($fields.Count)):"
            Add-Report '```'
            foreach ($field in $fields) {
                Add-Report "  $($field.FieldType.Name) $($field.Name)"
            }
            Add-Report '```'
        }
        catch {
            Add-Report "Error getting fields: $($_.Exception.Message)"
        }

        # Methods
        try {
            $methods = $propClass.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::DeclaredOnly) | Where-Object { -not $_.IsSpecialName }
            Add-Report ""
            Add-Report "#### Methods ($($methods.Count)):"
            Add-Report '```'
            foreach ($method in $methods) {
                try {
                    $params = ($method.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ", "
                    Add-Report "  $($method.ReturnType.Name) $($method.Name)($params)"
                }
                catch {
                    Add-Report "  $($method.Name)(...)"
                }
            }
            Add-Report '```'
        }
        catch {
            Add-Report "Error getting methods: $($_.Exception.Message)"
        }
    }
}

# ============================================
# SECTION 4: Assets.dll Types
# ============================================
Add-Report "## SECTION 4: ASSETS.DLL KEY TYPES"
Add-Report ""

if ($assetsDll) {
    $assetsTypes = Get-TypesSafely $assetsDll
    Add-Report "Total types loaded from Assets.dll: $($assetsTypes.Count)"
    Add-Report ""

    # Find AssetReference
    $assetRefType = $assetsTypes | Where-Object { $_.Name -eq "AssetReference" }
    if ($assetRefType) {
        Add-Report "### AssetReference FOUND"
        Add-Report "Full Name: $($assetRefType.FullName)"
        Add-Report ""

        # Fields
        try {
            $fields = $assetRefType.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
            Add-Report "#### Fields:"
            Add-Report '```'
            foreach ($field in $fields) {
                Add-Report "  $($field.FieldType.Name) $($field.Name)"
            }
            Add-Report '```'
        }
        catch {
            Add-Report "Error getting fields"
        }

        # Constructors
        try {
            $ctors = $assetRefType.GetConstructors()
            Add-Report ""
            Add-Report "#### Constructors ($($ctors.Count)):"
            Add-Report '```'
            foreach ($ctor in $ctors) {
                $params = ($ctor.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ", "
                Add-Report "  AssetReference($params)"
            }
            Add-Report '```'
        }
        catch {
            Add-Report "Error getting constructors"
        }
    }

    # Find Asset and AssetCore base types
    $assetBaseTypes = $assetsTypes | Where-Object { $_.Name -match "^Asset$|^AssetCore$" }
    Add-Report "### Asset Base Types:"
    Add-Report '```'
    foreach ($type in $assetBaseTypes) {
        Add-Report "$($type.FullName) : $($type.BaseType.Name)"
    }
    Add-Report '```'
}

# ============================================
# SECTION 5: Search for RuntimePropInfo
# ============================================
Add-Report "## SECTION 5: SEARCHING FOR RuntimePropInfo ACROSS ALL ASSEMBLIES"
Add-Report ""

$allTypes = @()
foreach ($asmEntry in $loadedAssemblies.GetEnumerator()) {
    $types = Get-TypesSafely $asmEntry.Value
    foreach ($type in $types) {
        if ($type.Name -match "RuntimeProp") {
            Add-Report "Found: $($type.FullName) in $($asmEntry.Key)"
        }
    }
}

# ============================================
# SECTION 6: Search for UI List related types
# ============================================
Add-Report ""
Add-Report "## SECTION 6: SEARCHING FOR UI LIST TYPES"
Add-Report ""

foreach ($asmEntry in $loadedAssemblies.GetEnumerator()) {
    $types = Get-TypesSafely $asmEntry.Value
    foreach ($type in $types) {
        if ($type.Name -match "List.*Model|List.*View|List.*Controller") {
            Add-Report "Found: $($type.FullName) in $($asmEntry.Key)"
        }
    }
}

# ============================================
# SECTION 7: Search for Tool Panel types
# ============================================
Add-Report ""
Add-Report "## SECTION 7: SEARCHING FOR TOOL PANEL TYPES"
Add-Report ""

foreach ($asmEntry in $loadedAssemblies.GetEnumerator()) {
    $types = Get-TypesSafely $asmEntry.Value
    foreach ($type in $types) {
        if ($type.Name -match "Tool.*Panel|Panel.*Tool") {
            Add-Report "Found: $($type.FullName) in $($asmEntry.Key)"
        }
    }
}

# Save report
$report | Out-File -FilePath $outputFile -Encoding UTF8

Write-Host ""
Write-Host "Report saved to: $outputFile" -ForegroundColor Green
Write-Host ""
Write-Host "=== ANALYSIS COMPLETE ===" -ForegroundColor Cyan
