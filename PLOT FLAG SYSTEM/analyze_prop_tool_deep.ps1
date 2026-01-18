# Deep Analysis of Specific Prop Tool Classes
# Focus on UIPropToolPanelController, PropTool, ToolManager, UIRuntimePropInfoListController

$managedPath = "C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed"
$outputFile = "D:\Endstar Plot Flag\PLOT FLAG SYSTEM\Zayed_Character_Mod\CustomProps\PROP_TOOL_DEEP_ANALYSIS.md"

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
        return
    }

    Add-Report ""
    Add-Report "## $title"
    Add-Report ""
    Add-Report "**Full Name:** ``$($type.FullName)``"
    Add-Report ""
    Add-Report "**Base Type:** ``$($type.BaseType.FullName)``"
    Add-Report ""

    # Interfaces
    $interfaces = $type.GetInterfaces()
    if ($interfaces.Count -gt 0) {
        Add-Report "### Interfaces ($($interfaces.Count))"
        Add-Report '```'
        foreach ($iface in $interfaces) {
            Add-Report "  $($iface.FullName)"
        }
        Add-Report '```'
        Add-Report ""
    }

    # Fields (ALL)
    $allFlags = [System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::Static
    $fields = $type.GetFields($allFlags)
    Add-Report "### Fields ($($fields.Count))"
    Add-Report '```csharp'
    foreach ($field in $fields | Sort-Object Name) {
        $access = if ($field.IsPublic) { "public" } elseif ($field.IsPrivate) { "private" } elseif ($field.IsFamily) { "protected" } else { "internal" }
        $static = if ($field.IsStatic) { " static" } else { "" }
        $serialized = ""
        try {
            $attrs = $field.GetCustomAttributes($true)
            foreach ($attr in $attrs) {
                if ($attr.GetType().Name -eq "SerializeField") { $serialized = " [SerializeField]" }
            }
        } catch {}
        Add-Report "$access$static$serialized $($field.FieldType.Name) $($field.Name);"
    }
    Add-Report '```'
    Add-Report ""

    # Properties
    $properties = $type.GetProperties($allFlags)
    if ($properties.Count -gt 0) {
        Add-Report "### Properties ($($properties.Count))"
        Add-Report '```csharp'
        foreach ($prop in $properties | Sort-Object Name) {
            $getter = if ($prop.GetGetMethod($true)) { "get;" } else { "" }
            $setter = if ($prop.GetSetMethod($true)) { "set;" } else { "" }
            Add-Report "$($prop.PropertyType.Name) $($prop.Name) { $getter $setter }"
        }
        Add-Report '```'
        Add-Report ""
    }

    # Methods (Declared only)
    $methods = $type.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::DeclaredOnly) | Where-Object { -not $_.IsSpecialName }
    Add-Report "### Methods ($($methods.Count))"
    Add-Report '```csharp'
    foreach ($method in $methods | Sort-Object Name) {
        $access = if ($method.IsPublic) { "public" } elseif ($method.IsPrivate) { "private" } elseif ($method.IsFamily) { "protected" } else { "internal" }
        $static = if ($method.IsStatic) { " static" } else { "" }
        $virtual = if ($method.IsVirtual -and -not $method.IsFinal) { " virtual" } else { "" }
        $override = if ($method.GetBaseDefinition() -ne $method -and $method.IsVirtual) { " override" } else { "" }
        $async = if ($method.Name -match "Async$|<.*>d__") { " async" } else { "" }

        try {
            $params = ($method.GetParameters() | ForEach-Object {
                "$($_.ParameterType.Name) $($_.Name)"
            }) -join ", "
            Add-Report "$access$static$virtual$override$async $($method.ReturnType.Name) $($method.Name)($params)"
        } catch {
            Add-Report "$access $($method.Name)(...)"
        }
    }
    Add-Report '```'
    Add-Report ""

    # Nested Types
    $nestedTypes = $type.GetNestedTypes([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic)
    if ($nestedTypes.Count -gt 0) {
        Add-Report "### Nested Types ($($nestedTypes.Count))"
        Add-Report '```'
        foreach ($nested in $nestedTypes) {
            Add-Report "  $($nested.Name)"
            if ($nested.IsEnum) {
                Add-Report "    (enum)"
                try {
                    $enumValues = [System.Enum]::GetNames($nested)
                    foreach ($val in $enumValues) {
                        Add-Report "      $val"
                    }
                } catch {}
            }
        }
        Add-Report '```'
        Add-Report ""
    }

    # Events
    $events = $type.GetEvents($allFlags)
    if ($events.Count -gt 0) {
        Add-Report "### Events ($($events.Count))"
        Add-Report '```csharp'
        foreach ($event in $events) {
            Add-Report "event $($event.EventHandlerType.Name) $($event.Name)"
        }
        Add-Report '```'
        Add-Report ""
    }
}

Add-Report "# ENDSTAR PROP TOOL - DEEP CLASS ANALYSIS"
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
$gameplayDll = $loadedAssemblies["Gameplay.dll"]

$creatorTypes = Get-TypesSafely $creatorDll
$gameplayTypes = Get-TypesSafely $gameplayDll

Add-Report "---"
Add-Report ""

# 1. ToolManager
$toolManagerType = $creatorTypes | Where-Object { $_.Name -eq "ToolManager" }
Analyze-TypeComplete $toolManagerType "ToolManager (Endless.Creator.LevelEditing.Runtime)"

# 2. ToolState enum
$toolStateType = $creatorTypes | Where-Object { $_.Name -eq "ToolState" }
if ($toolStateType) {
    Add-Report "## ToolState Enum"
    Add-Report '```'
    try {
        if ($toolStateType.IsEnum) {
            $enumValues = [System.Enum]::GetNames($toolStateType)
            foreach ($val in $enumValues) {
                Add-Report "  $val"
            }
        }
    } catch {
        Add-Report "  Error getting enum values"
    }
    Add-Report '```'
    Add-Report ""
}

# 3. ToolType enum
$toolTypeType = $creatorTypes | Where-Object { $_.Name -eq "ToolType" }
if ($toolTypeType) {
    Add-Report "## ToolType Enum"
    Add-Report '```'
    try {
        if ($toolTypeType.IsEnum) {
            $enumValues = [System.Enum]::GetNames($toolTypeType)
            foreach ($val in $enumValues) {
                Add-Report "  $val"
            }
        }
    } catch {
        Add-Report "  Error getting enum values"
    }
    Add-Report '```'
    Add-Report ""
}

# 4. PropTool
$propToolType = $creatorTypes | Where-Object { $_.Name -eq "PropTool" }
Analyze-TypeComplete $propToolType "PropTool (Endless.Creator.LevelEditing.Runtime)"

# 5. UIPropToolPanelController
$propPanelControllerType = $creatorTypes | Where-Object { $_.Name -eq "UIPropToolPanelController" }
Analyze-TypeComplete $propPanelControllerType "UIPropToolPanelController (Endless.Creator.UI)"

# 6. UIPropToolPanelView
$propPanelViewType = $creatorTypes | Where-Object { $_.Name -eq "UIPropToolPanelView" }
Analyze-TypeComplete $propPanelViewType "UIPropToolPanelView (Endless.Creator.UI)"

# 7. UIRuntimePropInfoListController
$propListControllerType = $creatorTypes | Where-Object { $_.Name -eq "UIRuntimePropInfoListController" }
Analyze-TypeComplete $propListControllerType "UIRuntimePropInfoListController (Endless.Creator.UI)"

# 8. UIRuntimePropInfoListModel
$propListModelType = $creatorTypes | Where-Object { $_.Name -eq "UIRuntimePropInfoListModel" }
Analyze-TypeComplete $propListModelType "UIRuntimePropInfoListModel (Endless.Creator.UI)"

# 9. UIRuntimePropInfoListView
$propListViewType = $creatorTypes | Where-Object { $_.Name -eq "UIRuntimePropInfoListView" }
Analyze-TypeComplete $propListViewType "UIRuntimePropInfoListView (Endless.Creator.UI)"

# 10. EndlessTool (base class)
$endlessToolType = $creatorTypes | Where-Object { $_.Name -eq "EndlessTool" }
Analyze-TypeComplete $endlessToolType "EndlessTool (Base Class)"

# 11. PropLibrary from Gameplay
$propLibraryType = $gameplayTypes | Where-Object { $_.Name -eq "PropLibrary" }
Analyze-TypeComplete $propLibraryType "PropLibrary (Endless.Gameplay.LevelEditing)"

# 12. StageManager from Gameplay
$stageManagerType = $gameplayTypes | Where-Object { $_.Name -eq "StageManager" }
Analyze-TypeComplete $stageManagerType "StageManager (Endless.Gameplay.LevelEditing.Level)"

# 13. CreatorManager
$creatorManagerType = $creatorTypes | Where-Object { $_.Name -eq "CreatorManager" }
Analyze-TypeComplete $creatorManagerType "CreatorManager"

# 14. UIToolController
$toolControllerType = $creatorTypes | Where-Object { $_.Name -eq "UIToolController" }
Analyze-TypeComplete $toolControllerType "UIToolController"

# Summary section
Add-Report "---"
Add-Report ""
Add-Report "# ANALYSIS SUMMARY"
Add-Report ""
Add-Report "## Key Classes Identified"
Add-Report ""
Add-Report "| Class | Purpose |"
Add-Report "|-------|---------|"
Add-Report "| ToolManager | Manages all tools, handles tool switching |"
Add-Report "| PropTool | The prop placement tool logic |"
Add-Report "| UIPropToolPanelController | Controls the prop tool UI panel |"
Add-Report "| UIPropToolPanelView | The visual component of prop tool panel |"
Add-Report "| UIRuntimePropInfoListController | Controls the list of props in UI |"
Add-Report "| UIRuntimePropInfoListModel | Data model for the prop list |"
Add-Report "| PropLibrary | Backend storage for all props |"
Add-Report "| StageManager | Manages stages/levels, owns PropLibrary |"
Add-Report ""

Add-Report "## Potential Hook Points"
Add-Report ""
Add-Report "Based on analysis, these are the key methods to investigate:"
Add-Report ""
Add-Report "1. **ToolManager.SelectTool** - Called when switching tools"
Add-Report "2. **PropTool activation** - When prop tool becomes active"
Add-Report "3. **UIPropToolPanelController.Initialize/Show** - Panel initialization"
Add-Report "4. **UIRuntimePropInfoListModel.Synchronize** - Syncs props with UI"
Add-Report "5. **PropLibrary.GetAllRuntimeProps** - Returns all available props"
Add-Report ""

# Save report
$report | Out-File -FilePath $outputFile -Encoding UTF8

Write-Host ""
Write-Host "Report saved to: $outputFile" -ForegroundColor Green
Write-Host ""
Write-Host "=== DEEP ANALYSIS COMPLETE ===" -ForegroundColor Cyan
