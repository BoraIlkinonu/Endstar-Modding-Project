# COMPREHENSIVE ENDSTAR PROP TOOL SYSTEM ANALYSIS

**Generated:** 2026-01-06
**Purpose:** Deep dive into why prop tool window doesn't open when editing levels

---

## EXECUTIVE SUMMARY

The prop tool window not opening is likely caused by one of our **Harmony patches interfering with the UI initialization chain**. Based on DLL analysis, the prop tool UI system has a specific flow that must not be interrupted.

---

## SYSTEM ARCHITECTURE

### Key Classes and Their Roles

| Class | Assembly | Purpose |
|-------|----------|---------|
| **ToolManager** | Creator.dll | Singleton that manages all tools, fires OnToolChange event |
| **PropTool** | Creator.dll | The prop placement tool logic |
| **UIPropToolPanelView** | Creator.dll | The visible prop tool panel |
| **UIPropToolPanelController** | Creator.dll | Controller for prop panel |
| **UIRuntimePropInfoListController** | Creator.dll | Controls the prop list UI |
| **UIRuntimePropInfoListModel** | Creator.dll | Data model for prop list |
| **PropLibrary** | Gameplay.dll | Backend storage for RuntimePropInfo |
| **StageManager** | Gameplay.dll | Singleton, owns PropLibrary |
| **CreatorManager** | Creator.dll | Singleton, fires OnCreatorStarted/OnCreatorEnded |

### Inheritance Chain

```
UIPropToolPanelView
  └── UIItemSelectionToolPanelView<PropTool, RuntimePropInfo>
        └── UIDockableToolPanelView<PropTool>
              └── UIBaseToolPanelView<PropTool>
                    └── UIGameObject (Shared.dll)
                          └── MonoBehaviour
```

---

## UI DISPLAY MECHANISM

### UIDisplayAndHideHandler

This is the core class that handles showing/hiding UI panels:

```csharp
// Key fields
TweenCollection displayTweenCollection;  // Animation for showing
TweenCollection hideTweenCollection;     // Animation for hiding
Boolean IsDisplaying;                     // Current state
Boolean handleSetActive;                  // Also toggle GameObject.SetActive

// Key methods
void Display()           // Show with animation
void Hide()              // Hide with animation
void SetToDisplayEnd()   // Snap to shown state
void SetToHideEnd()      // Snap to hidden state
```

### Display Flow (Normal Operation)

```
1. User clicks Prop Tool button
   └── UIToolController.SetActiveTool()

2. UIToolController calls ToolManager.SetActiveTool(ToolType.Prop)
   └── ToolManager.SetActiveTool_Internal()

3. ToolManager.OnToolChange.Invoke(newActiveTool)
   └── UnityEvent fires to all listeners

4. UIBaseToolPanelView.OnToolChange(EndlessTool activeTool)
   └── if (DisplayOnToolChangeMatchToToolType && activeTool.ToolType == this.Tool.ToolType)
   └── Display()

5. Display() shows the panel
   └── UIDisplayAndHideHandler.Display()
   └── TweenCollection animations play
   └── IsDisplaying = true
```

---

## KEY EVENTS AND SUBSCRIPTIONS

### ToolManager Events
```csharp
public UnityEvent<EndlessTool> OnToolChange;        // Fires when tool changes
public UnityEvent<bool> OnActiveChange;              // Fires when active state changes
public UnityEvent<EndlessTool> OnSetActiveToolToSameTool; // Fires when same tool selected
```

### CreatorManager Events
```csharp
public UnityEvent OnCreatorStarted;  // Fires when entering Creator mode
public UnityEvent OnCreatorEnded;    // Fires when exiting Creator mode
public Action OnPropsRepopulated;    // Fires when prop library is repopulated
```

### UIPropToolPanelView Subscriptions (via Start())
```csharp
// Subscribes to these in Start():
ToolManager.OnToolChange += OnToolChange
CreatorManager.OnCreatorStarted += OnCreatorStarted
CreatorManager.OnCreatorEnded += OnCreatorEnded
// PropLibrary repopulation callback
```

---

## PROP LIST SYNCHRONIZATION

### UIRuntimePropInfoListModel.Synchronize()

This is the key method that populates the prop list in the UI:

```csharp
public void Synchronize(ReferenceFilter referenceFilter, IReadOnlyList<RuntimePropInfo> propsToIgnore)
```

**Called when:**
1. Creator mode starts (`OnCreatorStarted`)
2. Prop library is repopulated (`OnLibraryRepopulated`)
3. Panel is displayed

**What it does:**
1. Gets all RuntimePropInfo from PropLibrary
2. Filters by ReferenceFilter
3. Excludes props in `propsToIgnore`
4. Updates the internal List
5. Triggers UI refresh

---

## POTENTIAL FAILURE POINTS

### 1. Harmony Patches on UI Classes

**HIGH RISK** - Patches on these methods could break display:
- `UIRuntimePropInfoListController.Start()`
- `UIRuntimePropInfoListController.Awake()`
- `UIRuntimePropInfoListController.OnEnable()`
- `UIRuntimePropInfoListController.Initialize()`

**From CURRENT_STATUS.md, these patches were DISABLED because they broke the UI:**
```
DISABLED PATCHES:
- UIRuntimePropInfoListController.Start (was breaking prop tool UI)
- UIRuntimePropInfoListController.Awake
- UIRuntimePropInfoListController.OnEnable
- UIRuntimePropInfoListController.Initialize
```

### 2. Missing Event Subscriptions

If `ToolManager.OnToolChange` doesn't have listeners, the panel won't show.
Check that `UIPropToolPanelView.Start()` completes successfully.

### 3. DisplayOnToolChangeMatchToToolType Flag

If this property returns `false`, the panel won't auto-display when tool is selected.
It's defined in `UIBaseToolPanelView<T>`.

### 4. Null UIDisplayAndHideHandler

If the handler isn't initialized, calling `Display()` will fail silently.

### 5. Tool Reference Not Set

`UIPropToolPanelView` has a `Tool` field that must reference the `PropTool` instance.
If null, the tool type matching will fail.

---

## PROP LIBRARY TO UI DATA FLOW

```
PropLibrary.loadedPropMap (Dictionary<AssetReference, RuntimePropInfo>)
       │
       ▼
PropLibrary.GetAllRuntimeProps() returns RuntimePropInfo[]
       │
       ▼
UIRuntimePropInfoListModel.Synchronize() receives the list
       │
       ▼
UIRuntimePropInfoListModel.List (internal storage)
       │
       ▼
UIRuntimePropInfoListView displays the items
```

**Critical:** Our custom prop MUST be in `GetAllRuntimeProps()` result for UI to show it.
We confirmed the prop IS returned (76 props including "Pearl Basket").

---

## RECOMMENDED INVESTIGATION STEPS

### Step 1: Check if OnToolChange is firing

Add logging patch:
```csharp
[HarmonyPatch(typeof(ToolManager), "SetActiveTool_Internal")]
static void Postfix(EndlessTool newActiveTool)
{
    Log.LogInfo($"Tool changed to: {newActiveTool?.ToolType}");
}
```

### Step 2: Check if UIPropToolPanelView receives the event

Add logging patch:
```csharp
[HarmonyPatch(typeof(UIPropToolPanelView), "OnToolChange")]
static void Prefix(EndlessTool activeTool)
{
    Log.LogInfo($"UIPropToolPanelView.OnToolChange: {activeTool?.ToolType}");
}
```

### Step 3: Check if Display() is called

Add logging patch:
```csharp
[HarmonyPatch]
static MethodBase TargetMethod()
{
    // Target the Display method in the base class
    var type = AccessTools.TypeByName("Endless.Creator.UI.UIItemSelectionToolPanelView`2");
    return AccessTools.Method(type, "Display");
}

static void Prefix()
{
    Log.LogInfo("UIItemSelectionToolPanelView.Display() called");
}
```

### Step 4: Check for exceptions

Wrap everything in try/catch to find hidden exceptions.

---

## SAFE HOOKS FOR PROP INJECTION

Based on analysis, these are the SAFE places to hook:

### ✅ SAFE
1. `PropLibrary.GetAllRuntimeProps()` - Postfix to append custom props
2. `StageManager.RegisterStage()` - Postfix to inject after stage is ready
3. `PropLibrary.InjectProp()` - Postfix to log injection results
4. `CreatorManager.OnPropsRepopulated` - Event callback for re-injection

### ⚠️ USE CAUTION
1. `UIRuntimePropInfoListModel.Synchronize()` - Can modify, but be careful
2. `PropLibrary.loadedPropMap` - Direct manipulation works but needs proper types

### ❌ AVOID (Will Break UI)
1. `UIRuntimePropInfoListController.Start/Awake/OnEnable/Initialize`
2. `UIPropToolPanelView.Start`
3. `UIBaseToolPanelView.OnToolChange` - Don't break the chain!

---

## CURRENT STATE OF YOUR PATCHES

From `CURRENT_STATUS.md`:

**ACTIVE PATCHES:**
```
- StageManager.RegisterStage (Postfix) - triggers injection ✅
- StageManager.LoadLevel (Prefix) - detects level loading ✅
- PropLibrary.GetAllRuntimeProps (Prefix + Postfix) ✅
- PropLibrary.FetchAndSpawnPropPrefab (Postfix) ✅
- PropLibrary.InjectProp (Postfix) ✅
- PropLibraryReference.GetReference (Postfix) ✅
```

**DISABLED PATCHES (Good!):**
```
- UIRuntimePropInfoListController.Start ❌
- UIRuntimePropInfoListController.Awake ❌
- UIRuntimePropInfoListController.OnEnable ❌
- UIRuntimePropInfoListController.Initialize ❌
```

---

## ROOT CAUSE HYPOTHESIS

The prop tool window not opening is likely due to:

1. **A remaining active patch** that interferes with the UI display chain
2. **Exception in StartupStart()** of a UI component that prevents event subscription
3. **Missing Tool reference** in UIPropToolPanelView
4. **UICreatorVisibilityHandler** not properly showing the panel

---

## NEXT STEPS

1. **Temporarily disable ALL patches** and verify prop tool opens normally
2. **Re-enable patches one by one** to identify which one breaks the UI
3. **Add diagnostic logging** to track the display flow
4. **Check BepInEx log** for any exceptions during level load
5. **Verify CreatorManager events** are firing properly

---

## KEY TYPE SIGNATURES FOR REFERENCE

### RuntimePropInfo (Nested in PropLibrary)
```csharp
public class RuntimePropInfo
{
    public Prop PropData;        // The prop asset data
    public Sprite Icon;          // UI icon
    public EndlessProp EndlessProp;  // The prefab instance (can be null)
    public bool IsLoading;
    public bool IsMissingObject;
}
```

### ToolType Enum
```csharp
public enum ToolType
{
    Empty = 0,
    Painting = 1,
    Prop = 2,       // This is what we care about
    Erase = 3,
    Wiring = 4,
    Inspector = 5,
    Copy = 6,
    Move = 7,
    GameEditor = 8,
    LevelEditor = 9,
    Screenshot = 10
}
```

---

*Analysis based on deep DLL examination of Creator.dll, Gameplay.dll, Props.dll, and Shared.dll*
