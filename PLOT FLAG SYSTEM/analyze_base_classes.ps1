# Analyze Base Classes for Prop Tool Panel
# UIItemSelectionToolPanelView, UIDockableToolPanelView, UIBaseToolPanelView

$managedPath = "C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed"
$outputFile = "D:\Endstar Plot Flag\PLOT FLAG SYSTEM\Zayed_Character_Mod\CustomProps\BASE_CLASSES_ANALYSIS.md"

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
        return $_.Exception.Types | Where-Object { $_ -ne $null }
    }
    catch {
        return @()
    }
}

function Analyze-TypeComplete($type, $title) {
    if (-not $type) {
        Add-Report "### $title NOT FOUND"
        Add-Report ""
        return
    }

    Add-Report ""
    Add-Report "## $title"
    Add-Report ""
    Add-Report "**Full Name:** ``$($type.FullName)``"
    Add-Report ""
    Add-Report "**Base Type:** ``$($type.BaseType.FullName)``"
    Add-Report ""

    $allFlags = [System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::Static

    # Fields
    $fields = $type.GetFields($allFlags)
    if ($fields.Count -gt 0) {
        Add-Report "### Fields ($($fields.Count))"
        Add-Report '```csharp'
        foreach ($field in $fields | Sort-Object Name) {
            $access = if ($field.IsPublic) { "public" } elseif ($field.IsPrivate) { "private" } else { "protected" }
            $static = if ($field.IsStatic) { " static" } else { "" }
            Add-Report "$access$static $($field.FieldType.Name) $($field.Name);"
        }
        Add-Report '```'
        Add-Report ""
    }

    # Methods (focus on important ones)
    $methods = $type.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::DeclaredOnly) | Where-Object { -not $_.IsSpecialName }
    if ($methods.Count -gt 0) {
        Add-Report "### Methods ($($methods.Count))"
        Add-Report '```csharp'
        foreach ($method in $methods | Sort-Object Name) {
            $access = if ($method.IsPublic) { "public" } elseif ($method.IsPrivate) { "private" } else { "protected" }
            $virtual = if ($method.IsVirtual) { " virtual" } else { "" }
            try {
                $params = ($method.GetParameters() | ForEach-Object {
                    "$($_.ParameterType.Name) $($_.Name)"
                }) -join ", "
                Add-Report "$access$virtual $($method.ReturnType.Name) $($method.Name)($params)"
            } catch {
                Add-Report "$access $($method.Name)(...)"
            }
        }
        Add-Report '```'
        Add-Report ""
    }
}

Add-Report "# ENDSTAR PROP TOOL - BASE CLASSES ANALYSIS"
Add-Report "Generated: $(Get-Date)"
Add-Report ""

# Load ALL DLLs
Write-Host "Loading assemblies..." -ForegroundColor Cyan

$allDlls = Get-ChildItem "$managedPath\*.dll"
$loadedAssemblies = @{}

foreach ($dll in $allDlls) {
    try {
        $asm = [System.Reflection.Assembly]::LoadFrom($dll.FullName)
        $loadedAssemblies[$dll.Name] = $asm
    }
    catch {}
}

Write-Host "Loaded $($loadedAssemblies.Count) assemblies" -ForegroundColor Green

$creatorDll = $loadedAssemblies["Creator.dll"]
$sharedDll = $loadedAssemblies["Shared.dll"]

$creatorTypes = Get-TypesSafely $creatorDll
$sharedTypes = Get-TypesSafely $sharedDll

Add-Report "---"
Add-Report ""

# 1. UIItemSelectionToolPanelView (generic base of UIPropToolPanelView)
$itemSelectionView = $creatorTypes | Where-Object { $_.Name -match "UIItemSelectionToolPanelView" }
foreach ($type in $itemSelectionView) {
    Analyze-TypeComplete $type "UIItemSelectionToolPanelView"
}

# 2. UIItemSelectionToolPanelController
$itemSelectionController = $creatorTypes | Where-Object { $_.Name -match "UIItemSelectionToolPanelController" }
foreach ($type in $itemSelectionController) {
    Analyze-TypeComplete $type "UIItemSelectionToolPanelController"
}

# 3. UIDockableToolPanelView
$dockableView = $creatorTypes | Where-Object { $_.Name -match "UIDockableToolPanelView" }
foreach ($type in $dockableView) {
    Analyze-TypeComplete $type "UIDockableToolPanelView"
}

# 4. UIDockableToolPanelController
$dockableController = $creatorTypes | Where-Object { $_.Name -match "UIDockableToolPanelController" }
foreach ($type in $dockableController) {
    Analyze-TypeComplete $type "UIDockableToolPanelController"
}

# 5. UIBaseToolPanelView
$baseView = $creatorTypes | Where-Object { $_.Name -eq "UIBaseToolPanelView`1" }
foreach ($type in $baseView) {
    Analyze-TypeComplete $type "UIBaseToolPanelView (Generic)"
}

# 6. UIBaseToolPanelController
$baseController = $creatorTypes | Where-Object { $_.Name -eq "UIBaseToolPanelController`1" }
foreach ($type in $baseController) {
    Analyze-TypeComplete $type "UIBaseToolPanelController (Generic)"
}

# 7. UIBaseLocalFilterableListModel (base of UIRuntimePropInfoListModel)
$filterableModel = $sharedTypes | Where-Object { $_.Name -match "UIBaseLocalFilterableListModel" }
if (-not $filterableModel) {
    $filterableModel = $creatorTypes | Where-Object { $_.Name -match "UIBaseLocalFilterableListModel" }
}
foreach ($type in $filterableModel) {
    Analyze-TypeComplete $type "UIBaseLocalFilterableListModel"
}

# 8. UIBaseListModel
$baseListModel = $sharedTypes | Where-Object { $_.Name -eq "UIBaseListModel`1" }
if (-not $baseListModel) {
    $baseListModel = $creatorTypes | Where-Object { $_.Name -eq "UIBaseListModel`1" }
}
foreach ($type in $baseListModel) {
    Analyze-TypeComplete $type "UIBaseListModel (Generic)"
}

# 9. Search for Display/Show/Hide methods in all tool-related types
Add-Report "---"
Add-Report ""
Add-Report "# DISPLAY/SHOW/HIDE METHODS SEARCH"
Add-Report ""

$toolViewTypes = $creatorTypes | Where-Object { $_.Name -match "Tool.*View|Panel.*View" }
foreach ($type in $toolViewTypes) {
    $displayMethods = $type.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance) |
        Where-Object { $_.Name -match "Display|Show|Hide|SetActive|Enable|Disable|Open|Close|SetVisib" }

    if ($displayMethods.Count -gt 0) {
        Add-Report "### $($type.Name)"
        Add-Report '```csharp'
        foreach ($method in $displayMethods) {
            try {
                $params = ($method.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ", "
                Add-Report "$($method.ReturnType.Name) $($method.Name)($params)"
            } catch {
                Add-Report "$($method.Name)(...)"
            }
        }
        Add-Report '```'
        Add-Report ""
    }
}

# 10. Find IDockableToolPanelView interface
Add-Report "---"
Add-Report ""
Add-Report "# IDockableToolPanelView INTERFACE"
Add-Report ""

$dockableInterface = $creatorTypes | Where-Object { $_.Name -eq "IDockableToolPanelView" }
foreach ($type in $dockableInterface) {
    Add-Report "## $($type.Name)"
    Add-Report ""
    $methods = $type.GetMethods()
    if ($methods.Count -gt 0) {
        Add-Report "### Methods"
        Add-Report '```csharp'
        foreach ($method in $methods) {
            try {
                $params = ($method.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ", "
                Add-Report "$($method.ReturnType.Name) $($method.Name)($params)"
            } catch {
                Add-Report "$($method.Name)(...)"
            }
        }
        Add-Report '```'
    }
    $properties = $type.GetProperties()
    if ($properties.Count -gt 0) {
        Add-Report ""
        Add-Report "### Properties"
        Add-Report '```csharp'
        foreach ($prop in $properties) {
            Add-Report "$($prop.PropertyType.Name) $($prop.Name)"
        }
        Add-Report '```'
    }
}

# 11. Find CreatorManager events and their subscribers
Add-Report "---"
Add-Report ""
Add-Report "# CREATOR MODE LIFECYCLE"
Add-Report ""
Add-Report "Key Events from CreatorManager:"
Add-Report "- OnCreatorStarted"
Add-Report "- OnCreatorEnded"
Add-Report "- OnPropsRepopulated"
Add-Report ""
Add-Report "UIPropToolPanelView subscribes to these via:"
Add-Report "- OnCreatorStarted()"
Add-Report "- OnCreatorEnded()"
Add-Report "- OnLibraryRepopulated()"
Add-Report ""

# 12. ToolManager.OnToolChange subscribers
Add-Report "---"
Add-Report ""
Add-Report "# TOOL CHANGE FLOW"
Add-Report ""
Add-Report "ToolManager.OnToolChange event fires when tool changes."
Add-Report "UIPropToolPanelView.OnToolChange(EndlessTool activeTool) receives this."
Add-Report ""
Add-Report "Expected flow:"
Add-Report "1. User clicks Prop Tool button"
Add-Report "2. ToolManager.SetActiveTool(ToolType.Prop) called"
Add-Report "3. ToolManager fires OnToolChange event"
Add-Report "4. UIPropToolPanelView.OnToolChange receives event"
Add-Report "5. Panel should display/show"
Add-Report ""

# 13. Check if there are any visibility handlers
Add-Report "---"
Add-Report ""
Add-Report "# VISIBILITY HANDLERS"
Add-Report ""

$visibilityTypes = $creatorTypes | Where-Object { $_.Name -match "Visibility" }
Add-Report "Found $($visibilityTypes.Count) visibility-related types:"
Add-Report ""
foreach ($type in $visibilityTypes) {
    Add-Report "- $($type.FullName)"
}

# Save report
$report | Out-File -FilePath $outputFile -Encoding UTF8

Write-Host ""
Write-Host "Report saved to: $outputFile" -ForegroundColor Green
Write-Host ""
Write-Host "=== BASE CLASSES ANALYSIS COMPLETE ===" -ForegroundColor Cyan
