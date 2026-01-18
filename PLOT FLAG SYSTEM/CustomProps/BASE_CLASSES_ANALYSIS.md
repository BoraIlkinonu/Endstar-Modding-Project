# ENDSTAR PROP TOOL - BASE CLASSES ANALYSIS
Generated: 01/06/2026 14:39:47

---


## UIItemSelectionToolPanelView

**Full Name:** `Endless.Creator.UI.UIItemSelectionToolPanelView`2`

**Base Type:** ``

### Fields (12)
```csharp
private UIBaseListView`1 <ListView>k__BackingField;
private InterfaceReference`1 detailController;
private UIDisplayAndHideHandler detailDisplayAndHideHandler;
private InterfaceReference`1 detailView;
private TweenCollection dockTweenCollection;
private TweenCollection floatingSelectedItemContainerDisplayTweenCollection;
private TweenCollection floatingSelectedItemContainerHideTweenCollection;
private InterfaceReference`1 floatingSelectedItemView;
protected Boolean IsMobile;
protected Single minHeight;
protected T Tool;
private TweenCollection undockTweenCollection;
```

### Methods (7)
```csharp
public virtual Void Display()
public Void Dock()
public virtual Void Hide()
protected Void OnItemSelectionEmpty()
protected virtual Void Start()
public Void Undock()
public Void ViewSelectedItem(TItemType itemType)
```


## UIItemSelectionToolPanelController

**Full Name:** `Endless.Creator.UI.UIItemSelectionToolPanelController`2`

**Base Type:** ``

### Fields (5)
```csharp
private UIItemSelectionToolPanelView`2 <ItemSelectionToolPanelView>k__BackingField;
private UIButton deselectButton;
private UIButton infoButton;
protected T Tool;
protected UIBaseToolPanelView`1 View;
```

### Methods (3)
```csharp
public virtual Void Deselect()
protected virtual Void Start()
private Void ViewInfo()
```


## UIDockableToolPanelView

**Full Name:** `Endless.Creator.UI.UIDockableToolPanelView`1`

**Base Type:** ``

### Fields (7)
```csharp
private UIDisplayAndHideHandler dockingDisplayAndHideHandler;
private Boolean isDocked;
protected Single minHeight;
protected T Tool;
private Image toolIconImage;
private UIToolTypeColorDictionary toolTypeColorDictionary;
private UIDisplayAndHideHandler undockButtonDisplayAndHideHandler;
```

### Methods (6)
```csharp
public virtual Void Display()
public virtual Void Dock()
public virtual Void Hide()
protected virtual Void Start()
private Void ToggleDockIfActivatedSameTool(EndlessTool activeTool)
public virtual Void Undock()
```


## UIDockableToolPanelController

**Full Name:** `Endless.Creator.UI.UIDockableToolPanelController`

**Base Type:** `Endless.Shared.UI.UIGameObject`

### Fields (4)
```csharp
private InterfaceReference`1 dockableToolPanelView;
private UIButton dockButton;
private UIButton undockButton;
private Boolean verboseLogging;
```

### Methods (1)
```csharp
private Void Start()
```


## UIBaseLocalFilterableListModel

**Full Name:** `Endless.Shared.UI.UIBaseLocalFilterableListModel`1`

**Base Type:** ``

### Fields (8)
```csharp
private Comparison`1 <ActiveSort>k__BackingField;
private UnityEvent`1 <SortChangedUnityEvent>k__BackingField;
private Boolean <SortOnChange>k__BackingField;
private Func`2 activeFilter;
private List`1 filteredList;
protected List`1 List;
private Dictionary`2 originalIndices;
public static Action`2 SortChangedAction;
```

### Methods (16)
```csharp
public virtual Void Add(T item, Boolean triggerEvents)
public virtual Void Clear(Boolean triggerEvents)
protected virtual Void DebugList()
public virtual Void Filter(Func`2 predicate, Boolean triggerEvents)
private Void HandleFilterAndSort(Boolean displayAddButtonCache, Boolean triggerEvents)
public virtual Void Insert(Int32 index, T item, Boolean triggerEvents)
protected virtual Void InsertAddButton(Boolean triggerEvents)
public Void ReFilter(Boolean triggerEvents)
public virtual Void RemoveAt(Int32 index, Boolean triggerEvents)
public Void RemoveFilteredAt(Int32 index, Boolean triggerEvents)
public virtual Void RemoveRange(Int32 index, Int32 count, Boolean triggerEvents)
public Void ReSort(Boolean triggerEvents)
public virtual Void Set(List`1 list, Boolean triggerEvents)
public virtual Void SetSortOrder(SortOrders value)
public Void Sort(Comparison`1 comparison, Boolean triggerEvents)
public virtual Void Swap(Int32 indexA, Int32 indexB, Boolean triggerEvents)
```

---

# DISPLAY/SHOW/HIDE METHODS SEARCH

### UIScreenshotToolView
```csharp
Void EnableCanvas(Boolean enabled)
Void Display()
Void Hide()
Void DisplayScreenshotCount()
Boolean get_enabled()
Void set_enabled(Boolean value)
Boolean get_isActiveAndEnabled()
HideFlags get_hideFlags()
Void set_hideFlags(HideFlags value)
```

### UIToolView
```csharp
Boolean get_enabled()
Void set_enabled(Boolean value)
Boolean get_isActiveAndEnabled()
HideFlags get_hideFlags()
Void set_hideFlags(HideFlags value)
```

### UIBaseToolPanelView`1
```csharp
Boolean get_DisplayOnToolChangeMatchToToolType()
Boolean get_IsDisplaying()
Void Display()
Void Hide()
Boolean get_enabled()
Void set_enabled(Boolean value)
Boolean get_isActiveAndEnabled()
HideFlags get_hideFlags()
Void set_hideFlags(HideFlags value)
```

### UICopyToolPanelView
```csharp
Void Display()
Void Hide()
Boolean get_DisplayOnToolChangeMatchToToolType()
Boolean get_IsDisplaying()
Boolean get_enabled()
Void set_enabled(Boolean value)
Boolean get_isActiveAndEnabled()
HideFlags get_hideFlags()
Void set_hideFlags(HideFlags value)
```

### UIDockableToolPanelView`1
```csharp
Void Display()
Void Hide()
Boolean get_DisplayOnToolChangeMatchToToolType()
Boolean get_IsDisplaying()
Boolean get_enabled()
Void set_enabled(Boolean value)
Boolean get_isActiveAndEnabled()
HideFlags get_hideFlags()
Void set_hideFlags(HideFlags value)
```

### UIEraseToolPanelView
```csharp
Void Display()
Void Hide()
Boolean get_DisplayOnToolChangeMatchToToolType()
Boolean get_IsDisplaying()
Boolean get_enabled()
Void set_enabled(Boolean value)
Boolean get_isActiveAndEnabled()
HideFlags get_hideFlags()
Void set_hideFlags(HideFlags value)
```

### UIInspectorToolPanelView
```csharp
PropEntry get_PropEntry()
Void set_PropEntry(PropEntry value)
Boolean get_DisplayOnToolChangeMatchToToolType()
Void Display()
Void Hide()
Boolean get_IsDisplaying()
Boolean get_enabled()
Void set_enabled(Boolean value)
Boolean get_isActiveAndEnabled()
HideFlags get_hideFlags()
Void set_hideFlags(HideFlags value)
```

### UIItemSelectionToolPanelView`2
```csharp
Void Display()
Void Hide()
Boolean get_DisplayOnToolChangeMatchToToolType()
Boolean get_IsDisplaying()
Boolean get_enabled()
Void set_enabled(Boolean value)
Boolean get_isActiveAndEnabled()
HideFlags get_hideFlags()
Void set_hideFlags(HideFlags value)
```

### UIPaintingToolPanelView
```csharp
Void OnEnable()
Void OnDisable()
Void Display()
Void Hide()
Boolean get_DisplayOnToolChangeMatchToToolType()
Boolean get_IsDisplaying()
Boolean get_enabled()
Void set_enabled(Boolean value)
Boolean get_isActiveAndEnabled()
HideFlags get_hideFlags()
Void set_hideFlags(HideFlags value)
```

### UIPropToolPanelView
```csharp
OnWindowDisplayed(...)
Void OnScriptWindowClosed()
Void Display()
Void Hide()
Boolean get_DisplayOnToolChangeMatchToToolType()
Boolean get_IsDisplaying()
Boolean get_enabled()
Void set_enabled(Boolean value)
Boolean get_isActiveAndEnabled()
HideFlags get_hideFlags()
Void set_hideFlags(HideFlags value)
```

---

# IDockableToolPanelView INTERFACE

## IDockableToolPanelView

### Methods
```csharp
Void Dock()
Void Undock()
```
---

# CREATOR MODE LIFECYCLE

Key Events from CreatorManager:
- OnCreatorStarted
- OnCreatorEnded
- OnPropsRepopulated

UIPropToolPanelView subscribes to these via:
- OnCreatorStarted()
- OnCreatorEnded()
- OnLibraryRepopulated()

---

# TOOL CHANGE FLOW

ToolManager.OnToolChange event fires when tool changes.
UIPropToolPanelView.OnToolChange(EndlessTool activeTool) receives this.

Expected flow:
1. User clicks Prop Tool button
2. ToolManager.SetActiveTool(ToolType.Prop) called
3. ToolManager fires OnToolChange event
4. UIPropToolPanelView.OnToolChange receives event
5. Panel should display/show

---

# VISIBILITY HANDLERS

Found 5 visibility-related types:

- Endless.Creator.UI.UICreatorVisibilityHandler
- Endless.Creator.UI.UIRoleVisibilityHandler
- Endless.Creator.UI.UIActiveLevelRoleVisibilityHandler
- Endless.Creator.UI.UIScreenshotCanvasVisibilityHandler
- Endless.Creator.UI.UIScreenshotToolVisibilityHandler
