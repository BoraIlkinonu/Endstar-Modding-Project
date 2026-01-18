# ENDSTAR PROP TOOL - DISPLAY MECHANISM ANALYSIS
Generated: 01/06/2026 14:40:49

---

## UIDisplayAndHideHandler

**Full Name:** `Endless.Shared.UI.UIDisplayAndHideHandler`
**Base Type:** `Endless.Shared.UI.UIGameObject`

### Fields
```csharp
Boolean <IsDisplaying>k__BackingField
TweenCollection displayTweenCollection
Boolean handleSetActive
TweenCollection hideTweenCollection
Boolean ignoreValidation
Boolean initialized
UnityEvent OnDisplayComplete
UnityEvent OnDisplayStart
UnityEvent OnHideComplete
UnityEvent OnHideStart
Boolean verboseLogging
```

### Methods
```csharp
public Void AddDisplayDelay(Single delay)
public Void CancelAnyTweens()
public Void CancelDisplayTweens()
public Void CancelHideTweens()
public Void Display()
public Void Display(Action onTweenComplete)
public Void Hide()
public Void Hide(Action onTweenComplete)
private Void Initialize()
private Void OnDisplayTweensComplete()
private Void OnHideTweensComplete()
public Void Set(Boolean display)
public Void SetDisplayDelay(Single delay)
public Void SetDisplayDuration(Single inSeconds)
public Void SetToDisplayEnd(Boolean triggerUnityEvent)
public Void SetToDisplayStart(Boolean triggerUnityEvent)
public Void SetToHideEnd(Boolean triggerUnityEvent)
public Void SetToHideStart(Boolean triggerUnityEvent)
public Void Toggle()
public virtual Void Validate()
```


## TweenCollection

**Full Name:** `Endless.Shared.Tweens.TweenCollection`
**Base Type:** `UnityEngine.MonoBehaviour`

### Fields
```csharp
Int32 activeTweensCount
UnityEvent OnAllTweenCompleted
Action pendingOnCompleteCallback
BaseTween[] tweens
Int32 tweensCompletedCount
Boolean verboseLogging
```

### Methods
```csharp
public Void Cancel()
private Void CleanupTweenListeners()
public Void ForceDone(Boolean triggerOnDoneEvents)
public Boolean IsAnyTweening()
private Void OnChildTweenCompleted()
private Void OnDestroy()
public Void OnTween(Single interpolation)
public Void SetDelay(Single delay)
public Void SetInSeconds(Single inSeconds)
public Void SetToEnd()
public Void SetToStart()
public Void Tween(Action onTweenComplete)
public Void Tween()
public virtual Void Validate()
public Void ValidateForNumberOfTweens(Int32 numberOfTweens)
```

---

# TYPES WITH OnToolChange METHOD

### UIScreenshotToolView
```csharp
Void OnToolChange(EndlessTool activeTool)
```

### UIScreenshotToolVisibilityHandler
```csharp
Void OnToolChange(EndlessTool activeTool)
```

### UIToolView
```csharp
Void OnToolChange(EndlessTool activeTool)
```

### UIBaseToolPanelView`1
```csharp
Void OnToolChange(EndlessTool activeTool)
```

### UICopyToolPanelView
```csharp
Void OnToolChange(EndlessTool activeTool)
```

### UIDockableToolPanelView`1
```csharp
Void OnToolChange(EndlessTool activeTool)
```

### UIEraseToolPanelView
```csharp
Void OnToolChange(EndlessTool activeTool)
```

### UIInspectorToolPanelView
```csharp
Void OnToolChange(EndlessTool activeTool)
```

### UIItemSelectionToolPanelView`2
```csharp
Void OnToolChange(EndlessTool activeTool)
```

### UIPaintingToolPanelView
```csharp
Void OnToolChange(EndlessTool activeTool)
```

### UIPropToolPanelView
```csharp
Void OnToolChange(EndlessTool activeTool)
```

### UIDisplayGameEditorWindowHandler
```csharp
Void OnToolChange(EndlessTool activeTool)
```

### UIDisplayLevelEditorWindowHandler
```csharp
Void OnToolChange(EndlessTool activeTool)
```

### UIWiringManager
```csharp
Void OnToolChange(EndlessTool newTool)
```

---
### UIBaseToolPanelView<T> (Contains Display/Hide) NOT FOUND

---

# TOOLMANAGER EVENT SUBSCRIPTION ANALYSIS

ToolManager has these events:
- OnToolChange (UnityEvent<EndlessTool>)
- OnActiveChange (UnityEvent<bool>)
- OnSetActiveToolToSameTool (UnityEvent<EndlessTool>)

---

## UICreatorVisibilityHandler

**Full Name:** `Endless.Creator.UI.UICreatorVisibilityHandler`
**Base Type:** `Endless.Shared.UI.UIGameObject`

### Fields
```csharp
UIDisplayAndHideHandler[] displayOnCreatorStarted
UIDisplayAndHideHandler[] hideOnCreatorEnded
UIDisplayAndHideHandler rootDisplayAndHideHandler
Boolean verboseLogging
```

### Methods
```csharp
private Void Display()
private Void Hide()
private Void OnCreatorEnded()
private Void OnCreatorStarted()
private IEnumerator Start()
```

---

# PROP TOOL PANEL LIFECYCLE METHODS

### UIPropToolPanelView Lifecycle Methods
```csharp
Start()
```

### Base Class Hierarchy for UIPropToolPanelView

```
UIPropToolPanelView
  â”â
        â”â
              â”â
                    â”â
                          â”â
```


## UIGameObject (Base of UI Components)

**Full Name:** `Endless.Shared.UI.UIGameObject`
**Base Type:** `UnityEngine.MonoBehaviour`

### Fields
```csharp
RectTransform rectTransform
```

---

# COMPLETE DISPLAY FLOW ANALYSIS

## Expected Flow When Prop Tool is Selected

```
1. User clicks Prop Tool button in toolbar
   â”â

2. UIToolController calls ToolManager.SetActiveTool(ToolType.Prop)
   â”â

3. ToolManager invokes OnToolChange.Invoke(newActiveTool)
   â”â

4. UIBaseToolPanelView.OnToolChange(EndlessTool activeTool) receives event
   â”â
   â”â
   â”â

5. Display() method shows the panel
   â”â
   â”â
   â”â
```

## What Could Go Wrong

1. **OnToolChange not subscribed** - Event handler not connected in Start/Awake
2. **DisplayOnToolChangeMatchToToolType is false** - Panel won't auto-show
3. **ToolType mismatch** - Panel's expected tool doesn't match
4. **Display() throws exception** - Something in display logic fails
5. **UIDisplayAndHideHandler null** - Display handler not initialized
6. **Harmony patch interferes** - Our patches might break the flow

## Key Investigation Points

1. Check if ToolManager.OnToolChange has any listeners
2. Verify UIPropToolPanelView.OnToolChange is subscribed
3. Check if Display() is actually called
4. Look for exceptions in the display process
5. Verify our Harmony patches aren't breaking anything

