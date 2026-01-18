# Deep Analysis of Endstar Prop Tool UI System
# This script analyzes the Creator.dll and Gameplay.dll to understand the prop tool

$managedPath = "C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed"
$outputFile = "D:\Endstar Plot Flag\PLOT FLAG SYSTEM\Zayed_Character_Mod\CustomProps\PROP_TOOL_UI_ANALYSIS.md"

# Load assemblies
Write-Host "Loading assemblies..." -ForegroundColor Cyan

# First load dependencies
$unityCoreDll = [System.Reflection.Assembly]::LoadFrom("$managedPath\UnityEngine.CoreModule.dll")
$unityUIDll = [System.Reflection.Assembly]::LoadFrom("$managedPath\UnityEngine.UI.dll")

$creatorDll = [System.Reflection.Assembly]::LoadFrom("$managedPath\Creator.dll")
$gameplayDll = [System.Reflection.Assembly]::LoadFrom("$managedPath\Gameplay.dll")
$propsDll = [System.Reflection.Assembly]::LoadFrom("$managedPath\Props.dll")
$assetsDll = [System.Reflection.Assembly]::LoadFrom("$managedPath\Assets.dll")

$report = @()

function Add-Report($text) {
    $script:report += $text
    Write-Host $text
}

Add-Report "# ENDSTAR PROP TOOL UI DEEP ANALYSIS"
Add-Report "Generated: $(Get-Date)"
Add-Report ""

# ============================================
# SECTION 1: Find all Prop and Tool related types
# ============================================
Add-Report "## SECTION 1: PROP AND TOOL RELATED TYPES"
Add-Report ""

$creatorTypes = $creatorDll.GetTypes()
$propToolTypes = $creatorTypes | Where-Object { $_.Name -match "Prop|Tool" } | Sort-Object Name

Add-Report "### Creator.dll - Prop/Tool Types ($($propToolTypes.Count) found)"
Add-Report '```'
foreach ($type in $propToolTypes) {
    $base = if ($type.BaseType) { " : $($type.BaseType.Name)" } else { "" }
    Add-Report "$($type.FullName)$base"
}
Add-Report '```'
Add-Report ""

# ============================================
# SECTION 2: Analyze UIPropToolPanelView
# ============================================
Add-Report "## SECTION 2: UIPropToolPanelView ANALYSIS"
Add-Report ""

$propPanelType = $creatorTypes | Where-Object { $_.Name -eq "UIPropToolPanelView" }
if (-not $propPanelType) {
    $propPanelType = $creatorTypes | Where-Object { $_.Name -match "PropTool.*Panel" } | Select-Object -First 1
}

if ($propPanelType) {
    Add-Report "### $($propPanelType.FullName)"
    Add-Report "Base Type: $($propPanelType.BaseType.FullName)"
    Add-Report ""

    # Fields
    Add-Report "#### Fields:"
    Add-Report '```'
    $fields = $propPanelType.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::Static)
    foreach ($field in $fields) {
        $access = if ($field.IsPublic) { "public" } elseif ($field.IsPrivate) { "private" } else { "protected" }
        $static = if ($field.IsStatic) { " static" } else { "" }
        Add-Report "$access$static $($field.FieldType.Name) $($field.Name)"
    }
    Add-Report '```'
    Add-Report ""

    # Methods
    Add-Report "#### Methods:"
    Add-Report '```'
    $methods = $propPanelType.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::DeclaredOnly) | Where-Object { -not $_.IsSpecialName }
    foreach ($method in $methods) {
        $params = ($method.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ", "
        Add-Report "$($method.ReturnType.Name) $($method.Name)($params)"
    }
    Add-Report '```'
} else {
    Add-Report "UIPropToolPanelView NOT FOUND"
}
Add-Report ""

# ============================================
# SECTION 3: Analyze UIRuntimePropInfoListModel
# ============================================
Add-Report "## SECTION 3: UIRuntimePropInfoListModel ANALYSIS"
Add-Report ""

$listModelType = $creatorTypes | Where-Object { $_.Name -eq "UIRuntimePropInfoListModel" }
if (-not $listModelType) {
    $listModelType = $creatorTypes | Where-Object { $_.Name -match "RuntimeProp.*List" } | Select-Object -First 1
}

if ($listModelType) {
    Add-Report "### $($listModelType.FullName)"
    Add-Report "Base Type: $($listModelType.BaseType.FullName)"
    Add-Report ""

    # Fields
    Add-Report "#### Fields:"
    Add-Report '```'
    $fields = $listModelType.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::Static)
    foreach ($field in $fields) {
        Add-Report "$($field.FieldType.Name) $($field.Name)"
    }
    Add-Report '```'
    Add-Report ""

    # Methods
    Add-Report "#### Methods:"
    Add-Report '```'
    $methods = $listModelType.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance) | Where-Object { -not $_.IsSpecialName }
    foreach ($method in $methods) {
        $params = ($method.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ", "
        Add-Report "$($method.ReturnType.Name) $($method.Name)($params)"
    }
    Add-Report '```'
} else {
    Add-Report "UIRuntimePropInfoListModel NOT FOUND"
}
Add-Report ""

# ============================================
# SECTION 4: Find UI Panel Controllers
# ============================================
Add-Report "## SECTION 4: UI PANEL CONTROLLERS"
Add-Report ""

$controllerTypes = $creatorTypes | Where-Object { $_.Name -match "Controller|Manager" -and $_.Name -match "UI|Tool|Panel" } | Sort-Object Name

Add-Report "### Controller/Manager Types ($($controllerTypes.Count) found)"
Add-Report '```'
foreach ($type in $controllerTypes) {
    Add-Report "$($type.FullName) : $($type.BaseType.Name)"
}
Add-Report '```'
Add-Report ""

# ============================================
# SECTION 5: Tool State Machine
# ============================================
Add-Report "## SECTION 5: TOOL STATE MACHINE"
Add-Report ""

$toolStateTypes = $creatorTypes | Where-Object { $_.Name -match "Tool.*State|State.*Tool|CreatorTool" } | Sort-Object Name

Add-Report "### Tool State Types ($($toolStateTypes.Count) found)"
foreach ($type in $toolStateTypes) {
    Add-Report ""
    Add-Report "#### $($type.Name)"
    Add-Report "Full: $($type.FullName)"
    Add-Report "Base: $($type.BaseType.Name)"

    # Key methods
    $methods = $type.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::DeclaredOnly) | Where-Object { -not $_.IsSpecialName } | Select-Object -First 15
    if ($methods.Count -gt 0) {
        Add-Report "Methods:"
        Add-Report '```'
        foreach ($method in $methods) {
            Add-Report "  $($method.ReturnType.Name) $($method.Name)()"
        }
        Add-Report '```'
    }
}
Add-Report ""

# ============================================
# SECTION 6: CreatorMode / Main Controller
# ============================================
Add-Report "## SECTION 6: CREATOR MODE MAIN CONTROLLER"
Add-Report ""

$creatorModeTypes = $creatorTypes | Where-Object { $_.Name -match "CreatorMode|LevelEditor|EditorController" } | Sort-Object Name

Add-Report "### Creator Mode Types ($($creatorModeTypes.Count) found)"
foreach ($type in $creatorModeTypes) {
    Add-Report ""
    Add-Report "#### $($type.Name)"
    Add-Report '```'
    Add-Report "Full: $($type.FullName)"
    Add-Report "Base: $($type.BaseType.FullName)"

    # Check for singleton
    $instanceField = $type.GetField("Instance", [System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Static)
    $instanceProp = $type.GetProperty("Instance", [System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Static)
    if ($instanceField -or $instanceProp) {
        Add-Report "*** SINGLETON - has Instance ***"
    }

    # Fields
    $fields = $type.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance) | Select-Object -First 20
    foreach ($field in $fields) {
        Add-Report "  $($field.FieldType.Name) $($field.Name)"
    }
    Add-Report '```'
}
Add-Report ""

# ============================================
# SECTION 7: Find Show/Open/Activate methods
# ============================================
Add-Report "## SECTION 7: SHOW/OPEN/ACTIVATE METHODS"
Add-Report ""

$searchTerms = @("Show", "Open", "Activate", "Enable", "Display", "SetVisible", "SetActive")

foreach ($type in $propToolTypes) {
    $relevantMethods = $type.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance) |
        Where-Object {
            $methodName = $_.Name
            $searchTerms | ForEach-Object { $methodName -match $_ } | Where-Object { $_ } | Select-Object -First 1
        }

    if ($relevantMethods) {
        Add-Report "### $($type.Name)"
        Add-Report '```'
        foreach ($method in $relevantMethods) {
            $params = ($method.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ", "
            Add-Report "$($method.ReturnType.Name) $($method.Name)($params)"
        }
        Add-Report '```'
        Add-Report ""
    }
}

# ============================================
# SECTION 8: Gameplay.dll - PropLibrary Analysis
# ============================================
Add-Report "## SECTION 8: PropLibrary (Gameplay.dll)"
Add-Report ""

$gameplayTypes = $gameplayDll.GetTypes()
$propLibType = $gameplayTypes | Where-Object { $_.Name -eq "PropLibrary" }

if ($propLibType) {
    Add-Report "### PropLibrary"
    Add-Report '```'

    # Fields
    $fields = $propLibType.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::Static)
    Add-Report "Fields:"
    foreach ($field in $fields) {
        Add-Report "  $($field.FieldType.Name) $($field.Name)"
    }

    Add-Report ""
    Add-Report "Methods:"
    $methods = $propLibType.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::DeclaredOnly) | Where-Object { -not $_.IsSpecialName }
    foreach ($method in $methods) {
        $params = ($method.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ", "
        Add-Report "  $($method.ReturnType.Name) $($method.Name)($params)"
    }

    Add-Report ""
    Add-Report "Events:"
    $events = $propLibType.GetEvents()
    foreach ($event in $events) {
        Add-Report "  $($event.EventHandlerType.Name) $($event.Name)"
    }

    Add-Report '```'

    # Nested types (RuntimePropInfo)
    $nestedTypes = $propLibType.GetNestedTypes([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic)
    if ($nestedTypes.Count -gt 0) {
        Add-Report ""
        Add-Report "### Nested Types"
        foreach ($nested in $nestedTypes) {
            Add-Report ""
            Add-Report "#### $($nested.Name)"
            Add-Report '```'
            $nFields = $nested.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
            foreach ($field in $nFields) {
                Add-Report "  $($field.FieldType.Name) $($field.Name)"
            }
            Add-Report '```'
        }
    }
}
Add-Report ""

# ============================================
# SECTION 9: StageManager Analysis
# ============================================
Add-Report "## SECTION 9: StageManager (Gameplay.dll)"
Add-Report ""

$stageManagerType = $gameplayTypes | Where-Object { $_.Name -eq "StageManager" }

if ($stageManagerType) {
    Add-Report "### StageManager"
    Add-Report '```'

    # Fields
    $fields = $stageManagerType.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::Static)
    Add-Report "Fields:"
    foreach ($field in $fields) {
        Add-Report "  $($field.FieldType.Name) $($field.Name)"
    }

    Add-Report ""
    Add-Report "Key Methods:"
    $methods = $stageManagerType.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::DeclaredOnly) | Where-Object { -not $_.IsSpecialName }
    foreach ($method in $methods) {
        $params = ($method.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ", "
        Add-Report "  $($method.ReturnType.Name) $($method.Name)($params)"
    }
    Add-Report '```'
}
Add-Report ""

# ============================================
# SECTION 10: UI List View and Controller
# ============================================
Add-Report "## SECTION 10: UI LIST VIEW COMPONENTS"
Add-Report ""

$listTypes = $creatorTypes | Where-Object { $_.Name -match "List.*View|List.*Controller|ListView" } | Sort-Object Name

Add-Report "### List View/Controller Types ($($listTypes.Count) found)"
foreach ($type in $listTypes) {
    Add-Report ""
    Add-Report "#### $($type.Name)"
    Add-Report "Base: $($type.BaseType.Name)"

    # Key methods
    $methods = $type.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::DeclaredOnly) |
        Where-Object { -not $_.IsSpecialName } |
        Select-Object -First 10
    if ($methods.Count -gt 0) {
        Add-Report '```'
        foreach ($method in $methods) {
            Add-Report "  $($method.ReturnType.Name) $($method.Name)()"
        }
        Add-Report '```'
    }
}
Add-Report ""

# ============================================
# SECTION 11: UIRuntimePropInfoListController
# ============================================
Add-Report "## SECTION 11: UIRuntimePropInfoListController DEEP ANALYSIS"
Add-Report ""

$listControllerType = $creatorTypes | Where-Object { $_.Name -eq "UIRuntimePropInfoListController" }

if ($listControllerType) {
    Add-Report "### $($listControllerType.FullName)"
    Add-Report "Base Type: $($listControllerType.BaseType.FullName)"
    Add-Report ""

    # ALL Fields
    Add-Report "#### ALL Fields:"
    Add-Report '```'
    $fields = $listControllerType.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::Static)
    foreach ($field in $fields) {
        $access = if ($field.IsPublic) { "public" } elseif ($field.IsPrivate) { "private" } else { "protected" }
        Add-Report "$access $($field.FieldType.FullName) $($field.Name)"
    }
    Add-Report '```'
    Add-Report ""

    # ALL Methods
    Add-Report "#### ALL Methods:"
    Add-Report '```'
    $methods = $listControllerType.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::DeclaredOnly) | Where-Object { -not $_.IsSpecialName }
    foreach ($method in $methods) {
        $access = if ($method.IsPublic) { "public" } elseif ($method.IsPrivate) { "private" } else { "protected" }
        $params = ($method.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ", "
        Add-Report "$access $($method.ReturnType.Name) $($method.Name)($params)"
    }
    Add-Report '```'
} else {
    Add-Report "UIRuntimePropInfoListController NOT FOUND"

    # Search for alternatives
    $alternatives = $creatorTypes | Where-Object { $_.Name -match "PropInfo.*Controller|Prop.*List.*Controller" }
    if ($alternatives) {
        Add-Report ""
        Add-Report "Alternatives found:"
        foreach ($alt in $alternatives) {
            Add-Report "  $($alt.FullName)"
        }
    }
}
Add-Report ""

# ============================================
# SECTION 12: Find what initializes the prop list
# ============================================
Add-Report "## SECTION 12: PROP LIST INITIALIZATION CHAIN"
Add-Report ""

$initTypes = $creatorTypes | Where-Object {
    $_.Name -match "Prop" -and
    ($_.GetMethods() | Where-Object { $_.Name -match "Init|Setup|Load|Populate|Refresh|Sync" }).Count -gt 0
}

Add-Report "### Types with Init/Setup/Load/Populate/Refresh/Sync methods"
foreach ($type in $initTypes) {
    $initMethods = $type.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance) |
        Where-Object { $_.Name -match "Init|Setup|Load|Populate|Refresh|Sync" }

    if ($initMethods.Count -gt 0) {
        Add-Report ""
        Add-Report "#### $($type.Name)"
        Add-Report '```'
        foreach ($method in $initMethods) {
            $params = ($method.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ", "
            Add-Report "$($method.ReturnType.Name) $($method.Name)($params)"
        }
        Add-Report '```'
    }
}

# Save report
$report | Out-File -FilePath $outputFile -Encoding UTF8

Write-Host ""
Write-Host "Report saved to: $outputFile" -ForegroundColor Green
Write-Host ""
Write-Host "=== ANALYSIS COMPLETE ===" -ForegroundColor Cyan
