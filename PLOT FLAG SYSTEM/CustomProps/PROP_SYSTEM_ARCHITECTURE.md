# ENDSTAR PROP SYSTEM ARCHITECTURE
## Complete Analysis Based on DLL Examination

Generated: 2026-01-06
**Updated: 2026-01-06** - Added runtime discoveries from injection testing
Based on: Props.dll, Gameplay.dll, Creator.dll analysis

---

## CRITICAL RUNTIME DISCOVERIES (2026-01-06)

### loadedPropMap Dictionary Type
```
Dictionary<AssetReference, RuntimePropInfo>
```
- Key is `Endless.Assets.AssetReference`, NOT `String`
- Use `Prop.ToAssetReference()` to create key
- Attempting to use String key throws: "cannot be converted to type 'Endless.Assets.AssetReference'"

### injectedPropIds List Type
```
List<SerializableGuid>
```
- NOT `List<String>`
- Type: `Endless.Shared.DataTypes.SerializableGuid`
- Attempting to add String throws: "cannot be converted to type 'SerializableGuid'"

### Verified baseTypeId
```
f60832a2-63a9-466d-83e8-df441fbf37c9
```
- Extracted from existing props at runtime
- Valid for most basic props

---

## 1. CORE TYPES AND RELATIONSHIPS

### 1.1 The Prop Asset Type
```
Endless.Props.Assets.Prop
├── Inherits: Asset -> AssetCore (NOT ScriptableObject!)
├── Constructor: public Prop() (default constructor available)
└── Key Fields:
    ├── baseTypeId (String) - prop type identifier
    ├── componentIds (List) - component identifiers
    ├── scriptAsset (AssetReference) - linked script
    ├── prefabBundle (AssetReference) - linked prefab bundle
    ├── iconFileInstanceId (Int32) - icon reference
    ├── propLocationOffsets (PropLocationOffset[]) - placement offsets
    ├── bounds (Vector3Int) - bounding box
    └── Inherited: Name, AssetID, Description, AssetVersion, AssetType
```

### 1.2 RuntimePropInfo (What the UI displays)
```
Endless.Gameplay.LevelEditing.PropLibrary+RuntimePropInfo
├── Nested inside PropLibrary
├── Constructor: public RuntimePropInfo()
└── Fields:
    ├── PropData (Prop) - the prop asset data
    ├── Icon (Sprite) - UI icon
    ├── EndlessProp (EndlessProp) - instantiated prefab
    ├── IsLoading (bool) - loading state
    └── IsMissingObject (bool) - missing indicator
```

### 1.3 EndlessProp (The actual game object)
```
Endless.Gameplay.Scripting.EndlessProp
├── Inherits: EndlessBehaviour -> MonoBehaviour
├── Constructor: public EndlessProp()
└── Key Fields:
    ├── Prop - the prop data
    ├── ReferenceFilter - filter type
    └── NavValue (NavType) - navigation properties
```

### 1.4 InjectedProps (Struct for injection data)
```
Endless.Gameplay.LevelEditing.Level.InjectedProps
├── Simple struct (not a class)
├── Constructor: public InjectedProps()
└── Fields:
    ├── Prop Prop - the prop asset
    ├── GameObject TestPrefab - test prefab
    ├── Script TestScript - test script
    └── Sprite Icon - UI icon
```

---

## 2. KEY MANAGERS

### 2.1 StageManager (SINGLETON)
```
Endless.Gameplay.LevelEditing.Level.StageManager
├── Inherits: MonoBehaviourSingleton<StageManager>
├── Access: StageManager.Instance
└── Critical Fields:
    ├── activePropLibrary (PropLibrary) *** THE KEY FIELD ***
    ├── basePropPrefab (EndlessProp) - template for spawning
    ├── loadingPropObject (EndlessProp) - loading placeholder
    ├── missingObjectPrefab (EndlessProp) - missing object placeholder
    ├── stageTemplate (Stage)
    └── offlineStagePrefab (OfflineStage)

InjectProp Method:
├── Signature: Void InjectProp(Prop prop, GameObject testPrefab, Script testScript, Sprite icon)
├── Parameters: 4 (convenience wrapper)
└── Delegates to: PropLibrary.InjectProp with additional internal params
```

### 2.2 PropLibrary (The prop storage)
```
Endless.Gameplay.LevelEditing.PropLibrary
├── Regular class (NOT MonoBehaviour)
├── Constructor: PropLibrary(Transform prefabSpawnRoot, EndlessProp loadingObjectProp, EndlessProp basePropPrefab, EndlessProp missingObjectPrefab)
└── Critical Fields:
    ├── loadedPropMap (Dictionary) - stores RuntimePropInfo by key
    ├── injectedPropIds (List) - tracks injected prop IDs
    ├── prefabSpawnRoot (Transform) - spawn location
    ├── basePropPrefab (EndlessProp) - base template
    └── missingObjectPrefab (EndlessProp) - placeholder

InjectProp Method:
├── Signature: Void InjectProp(Prop prop, GameObject testPrefab, Script testScript, Sprite icon, Transform prefabSpawnTransform, EndlessProp propPrefab)
├── Parameters: 6 (full version)
├── IMPORTANT: This method is ASYNC internally (<InjectProp>d__14 state machine)
└── Props are NOT immediately available after calling!

Other Key Methods:
├── GetAllRuntimeProps() -> RuntimePropInfo[] (returns all loaded props)
├── GetRuntimePropInfo(AssetReference) -> RuntimePropInfo
├── GetRuntimePropInfo(SerializableGuid) -> RuntimePropInfo
├── TryGetRuntimePropInfo(AssetReference, out RuntimePropInfo) -> bool
├── IsInjectedProp(string propDataAssetID) -> bool
└── AddMissingObjectRuntimePropInfo(RuntimePropInfo) -> void
```

---

## 3. UI COMPONENTS

### 3.1 UIRuntimePropInfoListModel
```
Endless.Creator.UI.UIRuntimePropInfoListModel
├── Inherits: UIBaseLocalFilterableListModel<RuntimePropInfo>
├── This is the data source for the prop tool panel UI
└── Key Methods:
    ├── Set(List<RuntimePropInfo>, bool) - sets the entire list
    ├── Add(RuntimePropInfo, bool) - adds single item
    ├── AddRange(List<RuntimePropInfo>, bool) - adds multiple items
    ├── Synchronize(ReferenceFilter, IReadOnlyList<RuntimePropInfo>) - syncs with source
    └── SetItem(int, RuntimePropInfo, bool) - updates specific item
```

### 3.2 UIPropToolPanelView
```
Endless.Creator.UI.UIPropToolPanelView
├── Contains: runtimePropInfoListModel (UIRuntimePropInfoListModel)
└── This is the actual prop selection panel in the level editor
```

---

## 4. PROP INJECTION FLOW (From Analysis)

```
User Enters Level Editor
         │
         v
StageManager singleton is active
         │
         v
StageManager has activePropLibrary reference
         │
         v
To inject a custom prop:
         │
         ├── Option A: Call StageManager.InjectProp(prop, testPrefab, script, icon)
         │             ↓
         │   StageManager internally calls:
         │   activePropLibrary.InjectProp(prop, testPrefab, script, icon, prefabSpawnRoot, basePropPrefab)
         │
         └── Option B: Directly call PropLibrary.InjectProp with all 6 params
                       (Need access to prefabSpawnRoot and basePropPrefab)

PropLibrary.InjectProp (ASYNC):
         │
         ├── Creates RuntimePropInfo
         ├── Spawns EndlessProp from basePropPrefab
         ├── Adds to loadedPropMap dictionary
         ├── Adds ID to injectedPropIds list
         └── Notifies UI (somehow)

UI Update:
         │
         ├── UIRuntimePropInfoListModel.Synchronize is called
         └── Prop appears in prop tool panel
```

---

## 5. CRITICAL FINDINGS

### 5.1 Prop is NOT a ScriptableObject
- `Prop` inherits from `Asset -> AssetCore -> Object`
- It has a public default constructor: `new Prop()`
- Do NOT use `ScriptableObject.CreateInstance<Prop>()`

### 5.2 PropLibrary.InjectProp is ASYNC
- The method signature shows `void` but IL analysis reveals `<InjectProp>d__14` async state machine
- Props won't be immediately available after calling InjectProp
- Need to wait or check for completion

### 5.3 Stage Must Exist
- Injection only works when a Stage is active (inside level editor)
- In MainScene/lobby, there is no Stage, so injection fails
- Must hook into stage creation or level load event

### 5.4 RuntimePropInfo Connects Everything
- UI displays RuntimePropInfo
- PropLibrary stores RuntimePropInfo in loadedPropMap
- RuntimePropInfo contains: PropData, Icon, EndlessProp

### 5.5 StageManager is the Gateway
- Access via `StageManager.Instance`
- Get PropLibrary via `activePropLibrary` field
- Get base prefabs via `basePropPrefab`, `missingObjectPrefab`

---

## 6. REQUIRED COMPONENTS FOR INJECTION

To successfully inject a custom prop, we need:

1. **Prop instance** - Create with `new Prop()` and set:
   - AssetID (unique GUID string)
   - Name (display name)
   - baseTypeId (prop type)
   - bounds (bounding box)

2. **EndlessProp prefab** - Either:
   - Clone from StageManager.basePropPrefab
   - Or use our custom prefab with EndlessProp component

3. **Sprite icon** - Runtime-created sprite for UI

4. **Transform for spawn root** - From PropLibrary.prefabSpawnRoot

5. **Wait for async completion** - InjectProp is async

---

## 7. UI REFRESH MECHANISM

Based on analysis, UI updates happen via:
- `UIRuntimePropInfoListModel.Synchronize(ReferenceFilter, IReadOnlyList<RuntimePropInfo>)`
- This is called when props change
- May need to trigger manually after injection

Potential trigger points:
- After PropLibrary.InjectProp completes
- When GetAllRuntimeProps() is called by UI
- When filter changes

---

## 8. IMPLEMENTATION REQUIREMENTS

### 8.1 Timing
- Must inject AFTER Stage is created/loaded
- Hook into: `StageManager.RegisterStage` or level load complete

### 8.2 References Needed
- StageManager.Instance
- StageManager.activePropLibrary
- StageManager.basePropPrefab
- StageManager.prefabSpawnRoot (from activePropLibrary)

### 8.3 Prop Creation
```csharp
// Create Prop instance
var propType = Type.GetType("Endless.Props.Assets.Prop, Props");
var prop = Activator.CreateInstance(propType);

// Set required fields via reflection
SetField(prop, "Name", "Custom Prop Name");
SetField(prop, "AssetID", Guid.NewGuid().ToString());
SetField(prop, "baseTypeId", "some_base_type_id"); // Need to find valid ID
SetField(prop, "bounds", new Vector3Int(1, 1, 1));
```

### 8.4 Injection Call
```csharp
// Via StageManager (easier, 4 params)
stageManager.InjectProp(prop, testPrefab, script, icon);

// Or via PropLibrary directly (need all refs)
propLibrary.InjectProp(prop, testPrefab, script, icon, spawnRoot, basePrefab);
```

---

## 9. NEXT STEPS FOR IMPLEMENTATION

1. Hook into stage creation (patch StageManager.RegisterStage or similar)
2. Create proper Prop instances with all required fields
3. Create or clone EndlessProp prefab
4. Call InjectProp with correct parameters
5. Wait for async completion
6. Force UI refresh if needed via UIRuntimePropInfoListModel.Synchronize

---

This document is based on actual DLL analysis, not assumptions.
