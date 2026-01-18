# Analyze UIDisplayAndHideHandler and Complete Display Mechanism
# This is the final piece needed to understand why prop tool doesn't open

$managedPath = "C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed"
$outputFile = "D:\Endstar Plot Flag\PLOT FLAG SYSTEM\Zayed_Character_Mod\CustomProps\DISPLAY_MECHANISM_ANALYSIS.md"

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
    Add-Report "**Base Type:** ``$($type.BaseType.FullName)``"
    Add-Report ""

    $allFlags = [System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::Static

    $fields = $type.GetFields($allFlags)
    if ($fields.Count -gt 0) {
        Add-Report "### Fields"
        Add-Report '```csharp'
        foreach ($field in $fields | Sort-Object Name) {
            Add-Report "$($field.FieldType.Name) $($field.Name)"
        }
        Add-Report '```'
        Add-Report ""
    }

    $methods = $type.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::DeclaredOnly) | Where-Object { -not $_.IsSpecialName }
    if ($methods.Count -gt 0) {
        Add-Report "### Methods"
        Add-Report '```csharp'
        foreach ($method in $methods | Sort-Object Name) {
            $access = if ($method.IsPublic) { "public" } elseif ($method.IsPrivate) { "private" } else { "protected" }
            $virtual = if ($method.IsVirtual) { " virtual" } else { "" }
            try {
                $params = ($method.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ", "
                Add-Report "$access$virtual $($method.ReturnType.Name) $($method.Name)($params)"
            } catch {
                Add-Report "$access $($method.Name)(...)"
            }
        }
        Add-Report '```'
        Add-Report ""
    }
}

Add-Report "# ENDSTAR PROP TOOL - DISPLAY MECHANISM ANALYSIS"
Add-Report "Generated: $(Get-Date)"
Add-Report ""

# Load assemblies
Write-Host "Loading assemblies..." -ForegroundColor Cyan
$allDlls = Get-ChildItem "$managedPath\*.dll"
$loadedAssemblies = @{}

foreach ($dll in $allDlls) {
    try {
        $asm = [System.Reflection.Assembly]::LoadFrom($dll.FullName)
        $loadedAssemblies[$dll.Name] = $asm
    } catch {}
}

$creatorDll = $loadedAssemblies["Creator.dll"]
$sharedDll = $loadedAssemblies["Shared.dll"]

$creatorTypes = Get-TypesSafely $creatorDll
$sharedTypes = Get-TypesSafely $sharedDll

# 1. UIDisplayAndHideHandler
Add-Report "---"
$displayHandler = $sharedTypes | Where-Object { $_.Name -eq "UIDisplayAndHideHandler" }
if (-not $displayHandler) {
    $displayHandler = $creatorTypes | Where-Object { $_.Name -eq "UIDisplayAndHideHandler" }
}
Analyze-TypeComplete $displayHandler "UIDisplayAndHideHandler"

# 2. TweenCollection (used for animations)
$tweenCollection = $sharedTypes | Where-Object { $_.Name -eq "TweenCollection" }
if (-not $tweenCollection) {
    $tweenCollection = $creatorTypes | Where-Object { $_.Name -eq "TweenCollection" }
}
Analyze-TypeComplete $tweenCollection "TweenCollection"

# 3. Find all types that implement OnToolChange
Add-Report "---"
Add-Report ""
Add-Report "# TYPES WITH OnToolChange METHOD"
Add-Report ""

foreach ($type in $creatorTypes) {
    $onToolChange = $type.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance) |
        Where-Object { $_.Name -eq "OnToolChange" }
    if ($onToolChange) {
        Add-Report "### $($type.Name)"
        Add-Report '```csharp'
        foreach ($method in $onToolChange) {
            try {
                $params = ($method.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ", "
                Add-Report "$($method.ReturnType.Name) OnToolChange($params)"
            } catch {
                Add-Report "OnToolChange(...)"
            }
        }
        Add-Report '```'
        Add-Report ""
    }
}

# 4. Analyze UIBaseToolPanelView in detail (has Display/Hide)
Add-Report "---"
$baseToolPanelView = $creatorTypes | Where-Object { $_.Name -eq "UIBaseToolPanelView`1" }
Analyze-TypeComplete $baseToolPanelView "UIBaseToolPanelView<T> (Contains Display/Hide)"

# 5. Find what listens to ToolManager.OnToolChange
Add-Report "---"
Add-Report ""
Add-Report "# TOOLMANAGER EVENT SUBSCRIPTION ANALYSIS"
Add-Report ""

Add-Report "ToolManager has these events:"
Add-Report "- OnToolChange (UnityEvent<EndlessTool>)"
Add-Report "- OnActiveChange (UnityEvent<bool>)"
Add-Report "- OnSetActiveToolToSameTool (UnityEvent<EndlessTool>)"
Add-Report ""

# 6. Find UICreatorVisibilityHandler
Add-Report "---"
$creatorVisibilityHandler = $creatorTypes | Where-Object { $_.Name -eq "UICreatorVisibilityHandler" }
Analyze-TypeComplete $creatorVisibilityHandler "UICreatorVisibilityHandler"

# 7. Find any Awake/Start/OnEnable in UIPropToolPanelView that might subscribe to events
Add-Report "---"
Add-Report ""
Add-Report "# PROP TOOL PANEL LIFECYCLE METHODS"
Add-Report ""

$propPanelView = $creatorTypes | Where-Object { $_.Name -eq "UIPropToolPanelView" }
if ($propPanelView) {
    $lifecycleMethods = $propPanelView.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance) |
        Where-Object { $_.Name -match "^(Awake|Start|OnEnable|OnDisable|OnDestroy)$" }

    Add-Report "### UIPropToolPanelView Lifecycle Methods"
    if ($lifecycleMethods.Count -gt 0) {
        Add-Report '```csharp'
        foreach ($method in $lifecycleMethods) {
            Add-Report "$($method.Name)()"
        }
        Add-Report '```'
    } else {
        Add-Report "No lifecycle methods found directly in UIPropToolPanelView (may be inherited)"
    }
}

# 8. Look for base classes with lifecycle methods
Add-Report ""
Add-Report "### Base Class Hierarchy for UIPropToolPanelView"
Add-Report ""
Add-Report '```'
Add-Report "UIPropToolPanelView"
Add-Report "  └── UIItemSelectionToolPanelView<PropTool, RuntimePropInfo>"
Add-Report "        └── UIDockableToolPanelView<PropTool>"
Add-Report "              └── UIBaseToolPanelView<PropTool>"
Add-Report "                    └── UIGameObject (Shared.dll)"
Add-Report "                          └── MonoBehaviour"
Add-Report '```'
Add-Report ""

# 9. UIGameObject base class
$uiGameObject = $sharedTypes | Where-Object { $_.Name -eq "UIGameObject" }
Analyze-TypeComplete $uiGameObject "UIGameObject (Base of UI Components)"

# 10. Summary of the display flow
Add-Report "---"
Add-Report ""
Add-Report "# COMPLETE DISPLAY FLOW ANALYSIS"
Add-Report ""
Add-Report "## Expected Flow When Prop Tool is Selected"
Add-Report ""
Add-Report '```'
Add-Report "1. User clicks Prop Tool button in toolbar"
Add-Report "   └── UIToolController.SetActiveTool() is called"
Add-Report ""
Add-Report "2. UIToolController calls ToolManager.SetActiveTool(ToolType.Prop)"
Add-Report "   └── ToolManager.SetActiveTool_Internal(EndlessTool) is called"
Add-Report ""
Add-Report "3. ToolManager invokes OnToolChange.Invoke(newActiveTool)"
Add-Report "   └── All listeners receive the event"
Add-Report ""
Add-Report "4. UIBaseToolPanelView.OnToolChange(EndlessTool activeTool) receives event"
Add-Report "   └── Checks if DisplayOnToolChangeMatchToToolType is true"
Add-Report "   └── Checks if activeTool.ToolType matches this panel's tool type"
Add-Report "   └── If match, calls Display()"
Add-Report ""
Add-Report "5. Display() method shows the panel"
Add-Report "   └── May use UIDisplayAndHideHandler"
Add-Report "   └── May use TweenCollection for animations"
Add-Report "   └── Sets IsDisplaying = true"
Add-Report '```'
Add-Report ""
Add-Report "## What Could Go Wrong"
Add-Report ""
Add-Report "1. **OnToolChange not subscribed** - Event handler not connected in Start/Awake"
Add-Report "2. **DisplayOnToolChangeMatchToToolType is false** - Panel won't auto-show"
Add-Report "3. **ToolType mismatch** - Panel's expected tool doesn't match"
Add-Report "4. **Display() throws exception** - Something in display logic fails"
Add-Report "5. **UIDisplayAndHideHandler null** - Display handler not initialized"
Add-Report "6. **Harmony patch interferes** - Our patches might break the flow"
Add-Report ""
Add-Report "## Key Investigation Points"
Add-Report ""
Add-Report "1. Check if ToolManager.OnToolChange has any listeners"
Add-Report "2. Verify UIPropToolPanelView.OnToolChange is subscribed"
Add-Report "3. Check if Display() is actually called"
Add-Report "4. Look for exceptions in the display process"
Add-Report "5. Verify our Harmony patches aren't breaking anything"
Add-Report ""

# Save report
$report | Out-File -FilePath $outputFile -Encoding UTF8

Write-Host ""
Write-Host "Report saved to: $outputFile" -ForegroundColor Green
Write-Host ""
Write-Host "=== DISPLAY MECHANISM ANALYSIS COMPLETE ===" -ForegroundColor Cyan
