# Custom Props Mod - Status and Plan

## What Has Been Done

### 1. Asset Bundle Created
- **Bundle Location:** `C:\Endless Studios\Endless Launcher\Endstar\BepInEx\plugins\custom_props.bundle`
- **Size:** 10.5 MB
- **Contents:**
  - `Assets/Pearl Basket/NewCustomProp.asset` (CustomPropDefinition)
  - `Assets/Pearl Basket/PearlBasket.prefab` (GameObject)
  - `Assets/Pearl Basket/pearl basket icon.png` (Sprite)

### 2. Plugin Successfully Loads Bundle
```
Bundle loaded with 3 assets:
  - Assets/Pearl Basket/NewCustomProp.asset
  - Assets/Pearl Basket/PearlBasket.prefab
  - Assets/Pearl Basket/pearl basket icon.png
```

### 3. Prefab Loads Successfully
```
Loaded prefab from: Assets/Pearl Basket/PearlBasket.prefab
```

### 4. StageManager.InjectProp Called Successfully
```
Calling StageManager.InjectProp...
SUCCESS: Injected pearl_basket via proper API!
```

### 5. Game Does NOT Crash (when only using InjectProp)

---

## The Problem

**The prop does NOT appear in the Creator UI prop tool.**

Even though `StageManager.InjectProp` reports success, the prop is not visible in the prop selection UI.

---

## What We Know About the Game's Prop System

### Key Types (from DLL analysis)
| Type | Location | Purpose |
|------|----------|---------|
| `StageManager` | Gameplay.dll | Singleton, has `InjectProp` method and `injectedProps` field |
| `PropLibrary` | Gameplay.dll | Has `loadedPropMap` dictionary |
| `RuntimePropInfo` | Gameplay.dll | Wrapper for prop data used at runtime |
| `InjectedProps` | Gameplay.dll | Struct with: Prop, TestPrefab, TestScript, Icon |
| `Prop` | Props.dll | ScriptableObject defining a prop |
| `AssetReference` | Assets.dll | Key type for loadedPropMap dictionary |

### Two Collections for Props
1. **`PropLibrary.loadedPropMap`** - `Dictionary<AssetReference, RuntimePropInfo>`
   - This is where the UI reads props from
   - Modifying during load causes "unknown error loading creator" crash

2. **`StageManager.injectedProps`** - `List<InjectedProps>`
   - Where `InjectProp` adds props
   - Purpose unclear - maybe for testing/development?

### InjectProp Method Signature
```csharp
StageManager.InjectProp(Prop prop, GameObject testPrefab, Script testScript, Sprite icon)
```

---

## Unanswered Questions

### 1. Where does `injectedProps` get used?
- Is there a UI section for injected props?
- Does the game read from `injectedProps` anywhere?
- Is it only for internal testing?

### 2. How does the prop UI populate its list?
- Does it ONLY read from `loadedPropMap`?
- Is there an event that triggers UI refresh?
- Is there a separate UI for "custom" or "injected" props?

### 3. When is it SAFE to modify `loadedPropMap`?
- Modifying during load crashes the game
- Is there a post-load event we can hook?
- Does `OnPropsRepopulated` fire at a safe time?

### 4. What makes a valid `RuntimePropInfo`?
- What fields are required?
- What causes crashes when the prop is selected?

---

## KEY DISCOVERY FROM DLL ANALYSIS

### PropLibrary ALSO has InjectProp!
```csharp
// StageManager.InjectProp (what we're using - WRONG?)
void InjectProp(Prop prop, GameObject testPrefab, Script testScript, Sprite icon)

// PropLibrary.InjectProp (probably the RIGHT one!)
void InjectProp(Prop prop, GameObject testPrefab, Script testScript, Sprite icon,
                Transform prefabSpawnTransform, EndlessProp propPrefab)
```

**PropLibrary.InjectProp takes 2 additional parameters:**
- `Transform prefabSpawnTransform` - where to spawn prefabs
- `EndlessProp propPrefab` - the base prop prefab template

### PropLibrary Has Method to Check Injected Props
```csharp
bool IsInjectedProp(string propDataAssetID)
```
This suggests PropLibrary knows about injected props!

### PropLibrary Filtering System
```csharp
Dictionary<?,?> GetReferenceFilterMap()
IReadOnlyList<?> GetReferenceFilteredDefinitionList(ReferenceFilter filter)
```
**The UI likely uses `GetReferenceFilteredDefinitionList` to populate the prop grid!**

### StageManager Fields
```
List<InjectedProps> injectedProps          [Private]
PropLibrary activePropLibrary              [Private]
EndlessProp basePropPrefab                 [Private] <- We need this!
UnityEvent<?,?> TerrainAndPropsLoaded      [Public]  <- Event we could hook!
```

### Next Steps Based on Findings:
1. **Use PropLibrary.InjectProp instead of StageManager.InjectProp**
2. **Get `basePropPrefab` from StageManager to pass to PropLibrary.InjectProp**
3. **Hook `TerrainAndPropsLoaded` event to inject at safe time**
4. **Investigate `GetReferenceFilteredDefinitionList` to understand UI filtering**

---

## Information Needed from DLLs

### 1. PropLibrary.InjectProp Implementation
- What does it add to loadedPropMap?
- Does it handle the UI integration?
- What's the relationship with `IsInjectedProp`?

### 2. GetReferenceFilteredDefinitionList
- What is `ReferenceFilter`?
- Does it include injected props?
- How does the UI call this?

### 3. RuntimePropInfo Structure
- All required fields
- How is it constructed in InjectProp?

### 4. basePropPrefab / EndlessProp
- What is this template prefab?
- Is it required for spawning?

---

## ADDITIONAL FINDINGS FROM DLL ANALYSIS

### RuntimePropInfo Structure (COMPLETE)
```csharp
class RuntimePropInfo {
    public Prop PropData;           // The prop definition
    public Sprite Icon;             // Icon for UI
    public EndlessProp EndlessProp; // The spawned prefab instance
    public bool IsLoading { get; }
    public bool IsMissingObject { get; }
}
```
**Only 3 public fields needed!** Much simpler than expected.

### ReferenceFilter Enum (for filtering)
```csharp
enum ReferenceFilter {
    None = 0,
    NonStatic = 1,
    Npc = 2,
    PhysicsObject = 4,
    InventoryItem = 8,
    Key = 16,
    Resource = 32
}
```

### UI Data Binding Chain
```
UIPropToolPanelView
  └── UIRuntimePropInfoListModel runtimePropInfoListModel  <- UI reads from here!
  └── OnLibraryRepopulated()                                <- Called when props change!
```

### Key Events
- `StageManager.TerrainAndPropsLoaded` - `UnityEvent<Stage>` - SAFE injection point!
- `UIPropToolPanelView.OnLibraryRepopulated()` - Refreshes UI prop list

### PropTool Class (handles prop placement)
```csharp
class PropTool {
    SerializableGuid SelectedAssetId;
    UnityEvent<SerializableGuid> OnSelectedAssetChanged;
    void LoadPropPrefab(RuntimePropInfo runtimePropInfo);
    void PlaceProp(...);
}
```

---

## REVISED IMPLEMENTATION PLAN

### Step 1: Hook TerrainAndPropsLoaded Event
```csharp
// After props are loaded, this event fires - SAFE to inject here
StageManager.Instance.TerrainAndPropsLoaded.AddListener(OnTerrainAndPropsLoaded);
```

### Step 2: Use PropLibrary.InjectProp (not StageManager)
```csharp
// Get required references
var stageManager = StageManager.Instance;
var propLibrary = stageManager.activePropLibrary;
var basePropPrefab = stageManager.basePropPrefab;  // Need this!

// Call PropLibrary.InjectProp with full parameters
propLibrary.InjectProp(prop, prefab, script, icon, spawnTransform, basePropPrefab);
```

### Step 3: Trigger UI Refresh
```csharp
// After injection, trigger OnLibraryRepopulated to refresh UI
// May need to call CreatorManager.OnPropsRepopulated or similar
```

---

## Information Needed at Runtime

### 1. Contents of `injectedProps` after InjectProp
- Is our prop actually in there?
- What does the InjectedProps struct look like?

### 2. How does the game load props normally?
- Trace the flow from bundle to `loadedPropMap`
- When does `OnPropsRepopulated` fire?

### 3. UI Data Binding
- What object does the prop grid UI bind to?
- Is there a refresh/reload method?

---

## Proposed Plan

### Phase 1: Deep DLL Analysis
1. Decompile `Gameplay.dll` and find:
   - Full implementation of `StageManager.InjectProp`
   - All references to `injectedProps`
   - How `loadedPropMap` is populated

2. Find the Creator UI class that displays props:
   - What namespace/class?
   - What data source does it use?

### Phase 2: Runtime Inspection
1. Fix F12 diagnostic to dump `injectedProps` contents
2. Add hotkey to force UI refresh
3. Log all calls to prop-related methods during load

### Phase 3: Implementation Options

**Option A: Add to loadedPropMap at SAFE time**
- Find exact safe timing (after all loading complete)
- May require hooking a late event

**Option B: Find/Create UI for injectedProps**
- Investigate if game has UI for injected props
- Or patch UI to also read from `injectedProps`

**Option C: Mimic game's normal prop loading**
- Create proper Addressables bundle matching game format
- Register in catalog.json
- Let game load it naturally

---

## Files Modified

- `D:\Endstar Plot Flag\PLOT FLAG SYSTEM\Zayed_Character_Mod\CustomProps\CustomPropsPlugin.cs`
- `D:\Endstar Plot Flag\PLOT FLAG SYSTEM\Zayed_Character_Mod\CustomProps\ProperPropInjector.cs`
- `D:\Unity_Workshop\Endstar Custom Shader\Assets\Editor\BuildCustomPropsBundle.cs`
- `D:\Unity_Workshop\Endstar Custom Shader\Assets\Pearl Basket\*` (Unity assets)

## Bundle Built From
- Unity Project: `D:\Unity_Workshop\Endstar Custom Shader`
- Built via: Addressables system
- Output: `Library\com.unity.addressables\aa\Windows\StandaloneWindows64\customprops_assets_all_*.bundle`
