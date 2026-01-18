# Endstar Assembly Reference

## LAST UPDATED: 2026-01-07 17:45

This document contains verified information from DLL analysis. Use this as the source of truth.

---

## Key Assemblies

| Assembly | Key Types |
|----------|-----------|
| Gameplay.dll | StageManager, PropLibrary, RuntimePropInfo, InjectedProps, EndlessProp |
| Creator.dll | CreatorManager, PropTool, UIPropToolPanelView, UIRuntimePropInfoListModel |
| Props.dll | Prop, Script, Asset |
| Assets.dll | AssetReference |
| Shared.DataTypes.dll | SerializableGuid |

---

## Gameplay.dll

### StageManager
```
Namespace: Endless.Gameplay.LevelEditing.Level
Base: MonoBehaviourSingleton<StageManager>

SINGLETON ACCESS:
  StageManager.Instance (via MonoBehaviourSingleton pattern)

FIELDS (Private):
  PropLibrary activePropLibrary
  List<InjectedProps> injectedProps
  EndlessProp basePropPrefab
  Transform prefabSpawnTransform
  EndlessProp loadingPropObject
  EndlessProp missingObjectPrefab

PROPERTIES (Public):
  PropLibrary ActivePropLibrary { get; }   // Public getter for activePropLibrary

EVENTS:
  UnityEvent<Stage> TerrainAndPropsLoaded
  UnityEvent<?> OnActiveStageChanged
  UnityEvent<?> OnLevelLoaded

METHODS:
  void InjectProp(Prop prop, GameObject testPrefab, Script testScript, Sprite icon)
  Task LoadLevel(LevelState, bool loadLibraryPrefabs, CancellationToken, Action<float>)
  Task FetchAndSpawnPropPrefab(AssetReference assetReference)
  RuntimePropInfo GetMissingObjectInfo(Prop propData)
```

### PropLibrary
```
Namespace: Endless.Gameplay.LevelEditing
Base: Object (NOT MonoBehaviour!)

FIELDS (Private):
  Dictionary<AssetReference, RuntimePropInfo> loadedPropMap    // THE MAIN STORAGE
  List<SerializableGuid> injectedPropIds                       // Tracks injected props (NOT HashSet, NOT string!)
  EndlessProp basePropPrefab
  Transform prefabSpawnRoot
  EndlessProp loadingObjectProp
  EndlessProp missingObjectPrefab
  Dictionary<?> _referenceFilterMap

PROPERTIES:
  RuntimePropInfo this[SerializableGuid assetId] { get; }      // Indexer by GUID
  RuntimePropInfo this[AssetReference assetReference] { get; } // Indexer by AssetRef
  Dictionary<?,?> ReferenceFilterMap { get; }

METHODS:
  void InjectProp(Prop prop, GameObject testPrefab, Script testScript,
                  Sprite icon, Transform prefabSpawnTransform, EndlessProp propPrefab)
  bool IsInjectedProp(string propDataAssetID)
  IReadOnlyList<?> GetReferenceFilteredDefinitionList(ReferenceFilter filter)  // UI USES THIS!
  RuntimePropInfo[] GetAllRuntimeProps()
  RuntimePropInfo GetRuntimePropInfo(SerializableGuid assetId)
  RuntimePropInfo GetRuntimePropInfo(AssetReference assetReference)
  bool TryGetRuntimePropInfo(SerializableGuid assetId, out RuntimePropInfo metadata)
  Task Repopulate(Stage activeStage, CancellationToken cancellationToken)
  Task<List<?>> LoadPropPrefabs(...)
  void UnloadAll()
```

### RuntimePropInfo (Nested: PropLibrary+RuntimePropInfo)
```
Namespace: Endless.Gameplay.LevelEditing.PropLibrary+RuntimePropInfo
Base: Object
IsNested: True

FIELDS (Public):
  Prop PropData
  Sprite Icon
  EndlessProp EndlessProp

PROPERTIES:
  bool IsLoading { get; }
  bool IsMissingObject { get; }
```

### InjectedProps
```
Namespace: Endless.Gameplay.LevelEditing.Level
Base: Object (IsValueType=False, so it's a CLASS)

FIELDS (Public):
  Prop Prop
  GameObject TestPrefab
  Script TestScript
  Sprite Icon
```

### EndlessProp
```
Namespace: Endless.Gameplay.Scripting
Base: EndlessBehaviour (which extends MonoBehaviour)

KEY FIELDS:
  EndlessScriptComponent scriptComponent
  EndlessVisuals endlessVisuals
  bool isNetworked

KEY PROPERTIES:
  Prop Prop { get; }
  ReferenceFilter ReferenceFilter { get; }
  NavType NavValue { get; }
  bool IsNetworked { get; }
  EndlessScriptComponent ScriptComponent { get; }
```

---

## Creator.dll

### CreatorManager
```
Namespace: Endless.Creator
Base: NetworkBehaviourSingleton<CreatorManager>

SINGLETON ACCESS:
  CreatorManager.Instance (via NetworkBehaviourSingleton pattern)

KEY FIELDS:
  LevelEditor levelEditor
  Action OnPropsRepopulated           // ACTION delegate, NOT UnityEvent!
  Action OnRepopulate
  Action OnTerrainRepopulated

EVENTS (UnityEvent):
  UnityEvent OnCreatorStarted
  UnityEvent OnCreatorEnded
  UnityEvent OnLeavingSession
  UnityEvent LevelReverted
  UnityEvent<IReadOnlyList<?>> LocalClientRoleChanged

METHODS:
  void add_OnPropsRepopulated(Action value)     // Subscribe to OnPropsRepopulated
  void remove_OnPropsRepopulated(Action value)  // Unsubscribe
  void add_OnRepopulate(Action value)
  void remove_OnRepopulate(Action value)
  void EnteringCreator()
  void CreatorLoaded()
  void LeavingCreator(bool forceSave)
  Task HandleGameUpdated(Game newGame, Game oldGame)

PROPERTIES:
  LevelEditor LevelEditor { get; }
  SaveLoadManager SaveLoadManager { get; }
```

### PropTool
```
Namespace: Endless.Creator.LevelEditing.Runtime
Base: PropBasedTool

EVENTS:
  UnityEvent<SerializableGuid> OnSelectedAssetChanged

PROPERTIES:
  SerializableGuid SelectedAssetId { get; }
  SerializableGuid PreviousSelectedAssetId { get; }
  ToolType ToolType { get; }

METHODS:
  void UpdateSelectedAssetId(SerializableGuid selectedAssetId)   // PUBLIC
  void LoadPropPrefab(RuntimePropInfo runtimePropInfo)           // PRIVATE
  void PlaceProp(Vector3, Vector3, SerializableGuid, SerializableGuid, ulong)  // PRIVATE
  void HandleSelected()
  void HandleDeselected()
```

### UIPropToolPanelView
```
Namespace: Endless.Creator.UI
Base: UIItemSelectionToolPanelView<?,?>

FIELDS (Private):
  UIRuntimePropInfoListModel runtimePropInfoListModel
  SerializableGuid selectedAssetId
  bool inCreatorMode

FIELDS (Protected):
  PropTool Tool

METHODS:
  void OnLibraryRepopulated()              // Called when props change - REFRESHES UI
  void OnSelectedAssetChanged(SerializableGuid selectedAssetId)
  void OnCreatorStarted()
  void OnCreatorEnded()
  void Start()
```

### UIRuntimePropInfoListModel
```
Namespace: Endless.Creator.UI
Base: UIBaseLocalFilterableListModel<RuntimePropInfo>

FIELDS (Protected):
  List<RuntimePropInfo> List

METHODS:
  void Synchronize(ReferenceFilter referenceFilter, IReadOnlyList<?> propsToIgnore)
  // ^^^ THIS IS HOW UI GETS ITS PROP LIST!
  // Likely calls PropLibrary.GetReferenceFilteredDefinitionList internally
```

---

## Props.dll

### Prop
```
Namespace: Endless.Props.Assets
Base: Endless.Assets.Asset → Endless.Assets.AssetCore → System.Object
NOTE: Prop is NOT a ScriptableObject! It's a regular C# class with default constructor.

FIELDS (Public):
  string AssetID
  string Name
  string Description
  string AssetType
  string AssetVersion
  string InternalVersion
  RevisionMetaData RevisionMetaData

FIELDS (Private):
  AssetReference prefabBundle
  AssetReference scriptAsset
  string baseTypeId
  List<?> componentIds
  Vector3Int bounds
  bool openSource
  int iconFileInstanceId
  List<?> visualAssets

PROPERTIES:
  AssetReference PrefabAsset { get; }        // Getter for prefabBundle
  AssetReference ScriptAsset { get; set; }   // Getter/setter for scriptAsset
  bool HasScript { get; }
  string BaseTypeId { get; }
  IReadOnlyList<?> ComponentIds { get; }
  bool OpenSource { get; }
  Vector3Int Bounds { get; }
  int IconFileInstanceId { get; set; }
```

### Script
```
Namespace: Endless.Props.Assets
Base: Asset

FIELDS (Public):
  string AssetID
  string Name
  string Description
  string AssetType
  string AssetVersion
  string InternalVersion
  RevisionMetaData RevisionMetaData

FIELDS (Private):
  string body
  string baseTypeId
  List<?> componentIds
  List<?> events
  List<?> receivers
  List<?> inspectorValues
  bool hasErrors
  bool openSource
```

---

## Assets.dll

### AssetReference (Endless.Assets.AssetReference)
```
Namespace: Endless.Assets
Base: Object

FIELDS (Public):
  string AssetID
  string AssetVersion
  string AssetType
  bool UpdateParentVersion

CONSTRUCTORS:
  AssetReference()  // Default constructor

METHODS:
  bool Equals(object obj)
  int GetHashCode()
  string ToString()
  static bool operator ==(AssetReference a, AssetReference b)
  static bool operator !=(AssetReference a, AssetReference b)
```

**NOTE: This is different from UnityEngine.AddressableAssets.AssetReference!**

---

## Shared.DataTypes.dll

### SerializableGuid
```
Namespace: Endless.Shared.DataTypes
Base: ValueType (struct)

FIELDS:
  Guid Guid
  long Long1
  long Long2

CONSTRUCTORS:
  SerializableGuid(long long1, long long2)
  SerializableGuid(Guid guid)
  SerializableGuid(string guidString)

STATIC PROPERTIES:
  SerializableGuid Empty { get; }

STATIC METHODS:
  SerializableGuid NewGuid()
  bool IsValid(Guid guid)

IMPLICIT CONVERSIONS:
  SerializableGuid ↔ Guid
  SerializableGuid ↔ string
```

---

## Enums

### ReferenceFilter
```
Namespace: Endless.Gameplay.LevelEditing (in Gameplay.dll)

Values:
  None = 0
  NonStatic = 1
  Npc = 2
  PhysicsObject = 4
  InventoryItem = 8
  Key = 16
  Resource = 32
```

---

## Enums

### ReferenceFilter (Endless.Gameplay.ReferenceFilter)
```
Namespace: Endless.Gameplay
Values:
  None = 0
  NonStatic = 1
  Npc = 2
  PhysicsObject = 4
  InventoryItem = 8
  Key = 16
  Resource = 32
```

---

## Key Patterns

### Creating AssetReference from Prop
```csharp
// Based on Prop having AssetID field and AssetReference having AssetID field:
var assetRef = new AssetReference();
assetRef.AssetID = prop.AssetID;
assetRef.AssetType = prop.AssetType;
assetRef.AssetVersion = prop.AssetVersion;
```

### Subscribing to OnPropsRepopulated
```csharp
// OnPropsRepopulated is an Action delegate, NOT UnityEvent
// Use reflection to call add_OnPropsRepopulated method

var creatorManager = CreatorManager.Instance;
var addMethod = typeof(CreatorManager).GetMethod("add_OnPropsRepopulated", ...);
Action callback = () => { /* handle repopulation */ };
addMethod.Invoke(creatorManager, new object[] { callback });
```

### Getting Props for UI
```csharp
// UI calls UIRuntimePropInfoListModel.Synchronize()
// Which internally calls PropLibrary.GetReferenceFilteredDefinitionList(filter)
// This returns props from loadedPropMap filtered by ReferenceFilter
```

---

## CORRECT PROP INJECTION APPROACH (DLL-Verified)

### Step 1: Use the OFFICIAL InjectProp method

PropLibrary has an official InjectProp method:
```csharp
void InjectProp(
    Prop prop,                      // The prop data
    GameObject testPrefab,          // The visual prefab (can be null for injected props?)
    Script testScript,              // Script asset (can be null)
    Sprite icon,                    // Icon for UI
    Transform prefabSpawnTransform, // Where to parent spawned props
    EndlessProp propPrefab          // Base prefab for creating EndlessProp instances
)
```

### Step 2: EndlessProp.CalculateReferenceFilter(Prop prop)

This method calculates the ReferenceFilter from the Prop's baseTypeId field:
```csharp
// EndlessProp has:
void CalculateReferenceFilter(Prop prop)  // Sets internal ReferenceFilter based on prop data
ReferenceFilter ReferenceFilter { get; }  // Public getter, private setter
Prop Prop { get; set; }                   // Has private setter
```

### Step 3: Clone Existing Prop

CRITICAL: The Prop MUST have valid:
- baseTypeId (string) - determines ReferenceFilter category
- componentIds (List)
- prefabBundle (AssetReference)
- All other fields from existing prop

Clone approach:
```csharp
// 1. Get first existing prop from loadedPropMap
// 2. Create new Prop instance with Activator.CreateInstance
// 3. Copy ALL fields via reflection
// 4. Only change: AssetID, Name, Description
```

### Step 4: _referenceFilterMap Structure

```
Dictionary<ReferenceFilter, List<RuntimePropInfo>> _referenceFilterMap

This maps each filter category to a list of props in that category.
PopulateReferenceFilterMap() reads from loadedPropMap and categorizes
props by their EndlessProp.ReferenceFilter value.
```

### Step 5: Flow Summary

1. Clone existing Prop -> newProp
2. Get basePropPrefab from PropLibrary (EndlessProp component)
3. Get prefabSpawnRoot from PropLibrary (Transform)
4. Call PropLibrary.InjectProp(newProp, visualPrefab, null, icon, prefabSpawnRoot, basePropPrefab)
5. The InjectProp method internally:
   - Creates RuntimePropInfo
   - Adds to loadedPropMap
   - Adds to injectedPropIds
   - Calls CalculateReferenceFilter internally?
   - Updates UI

### Alternative: Direct Injection (what we tried)

If calling official InjectProp doesn't work, manual approach:
1. Clone Prop
2. Instantiate basePropPrefab.gameObject -> endlessPropGO
3. Get EndlessProp component
4. Set EndlessProp.Prop = newProp
5. Call EndlessProp.CalculateReferenceFilter(newProp)
6. Create RuntimePropInfo(PropData=newProp, Icon=icon, EndlessProp=endlessProp)
7. Add to loadedPropMap[assetRef] = runtimePropInfo
8. Add to injectedPropIds
9. Call PopulateReferenceFilterMap() to update filter map

---

## CRITICAL FINDING: StageManager.InjectProp vs PropLibrary.InjectProp

### StageManager.InjectProp (CORRECT METHOD TO USE)
```
void InjectProp(Prop prop, GameObject testPrefab, Script testScript, Sprite icon)
```
- Only 4 parameters
- Adds to `StageManager.injectedProps` list (List<InjectedProps>)
- InjectedProps stores TestPrefab SEPARATELY from Prop.prefabBundle
- BuildPrefab then knows to use testPrefab instead of prefabBundle

### PropLibrary.InjectProp (INTERNAL METHOD)
```
void InjectProp(Prop prop, GameObject testPrefab, Script testScript, Sprite icon, Transform prefabSpawnTransform, EndlessProp propPrefab)
```
- 6 parameters
- Does NOT add to StageManager.injectedProps
- This is why our prop had merged visuals - it still loaded prefabBundle!

### InjectedProps Class
```
Fields:
  Prop Prop
  GameObject TestPrefab      <-- This is what BuildPrefab should use!
  Script TestScript
  Sprite Icon
```

### SOLUTION (UPDATED after testing)
Call BOTH methods:
1. `StageManager.InjectProp(prop, testPrefab, script, icon)` - stores testPrefab in injectedProps
2. `PropLibrary.InjectProp(prop, testPrefab, script, icon, prefabSpawnRoot, basePropPrefab)` - adds to loadedPropMap

**Why both?**
- StageManager.InjectProp only adds to injectedProps list (for BuildPrefab to use testPrefab)
- It does NOT add to loadedPropMap (so prop won't appear in UI)
- injectedProps is normally processed during LoadLibraryPrefabs, but we inject AFTER loading
- PropLibrary.InjectProp adds to loadedPropMap and injectedPropIds for UI visibility

---

## KNOWN ISSUES & TESTING RESULTS (2026-01-07)

### CONFIRMED: What BREAKS the Prop Tool Window

| Component | Breaks Prop Tool? | Notes |
|-----------|-------------------|-------|
| BuildPrefabPatch (Harmony on async method) | **YES** | Patching EndlessProp.BuildPrefab Task return breaks prop tool display |
| StageManager.InjectProp (4 params) | **NO** | Safe to call - game's official API |
| InjectSingleProp (manual loadedPropMap add) | **TESTING** | Isolated test in progress |
| FixEndlessPropReferenceFilter | **TESTING** | Not yet isolated |
| PopulateReferenceFilterMap call | **TESTING** | Not yet isolated |

### Testing Methodology

1. **Disable ALL injection** → Prop tool opens ✓
2. **Enable StageManager.InjectProp ONLY** → Prop tool opens ✓
3. **Add InjectSingleProp** → Testing...
4. **Add FixEndlessPropReferenceFilter** → Not yet tested
5. **Add PopulateReferenceFilterMap** → Not yet tested

### Key Insight: Harmony + Async Methods

**DO NOT** use Harmony to patch async methods that return `Task`. The prop tool system calls `BuildPrefab` for display purposes, and wrapping the Task return breaks the UI flow.

```csharp
// BAD - This breaks prop tool:
[HarmonyPostfix]
static void Postfix(ref Task __result) {
    __result = WrapTask(__result); // BREAKS UI
}

// The prop tool calls BuildPrefab during display
// and expects the original Task behavior
```

### Current Working Approach (2026-01-07)

```csharp
// Step 1: StageManager.InjectProp (SAFE)
stageManager.InjectProp(prop, prefab, null, icon);

// Step 2-4: Being isolated to find which breaks prop tool
// - Manual loadedPropMap injection
// - FixEndlessPropReferenceFilter
// - PopulateReferenceFilterMap call
```

---

## DEEP DLL ANALYSIS (2026-01-07 18:30)

### PopulateReferenceFilterMap Implementation

```
Method: PropLibrary.PopulateReferenceFilterMap()
Returns: void
Parameters: 0
IL Size: 234 bytes
Exception Handlers: 3 (Finally blocks for iterator disposal)

Local Variables:
  [0] IEnumerable<RuntimePropInfo> - loadedPropMap.Values
  [1] IEnumerator<RuntimePropInfo>
  [2] ReferenceFilter - from dynamicFilters iteration
  [3] Enumerator - dictionary enumerator
  [4] RuntimePropInfo - current prop being processed
  [5] ReferenceFilter - from EndlessProp.ReferenceFilter
```

**Algorithm (deduced from IL):**
1. Clears/recreates _referenceFilterMap dictionary
2. Iterates through dynamicFilters[] array
3. For each filter, creates new List<RuntimePropInfo>
4. Iterates through loadedPropMap.Values
5. For each RuntimePropInfo:
   - Reads `runtimePropInfo.EndlessProp.ReferenceFilter`
   - If (filter & EndlessProp.ReferenceFilter) matches, adds to list
6. Stores list in _referenceFilterMap[filter]

**CRITICAL:** If `RuntimePropInfo.EndlessProp` is null, accessing `.ReferenceFilter` throws NullReferenceException.

### RuntimePropInfo Complete Structure

```csharp
// Nested class: Endless.Gameplay.LevelEditing.PropLibrary+RuntimePropInfo
public class RuntimePropInfo
{
    // PUBLIC FIELDS (required by PopulateReferenceFilterMap)
    public Prop PropData;           // The prop data
    public Sprite Icon;             // UI icon
    public EndlessProp EndlessProp; // REQUIRED - must not be null!

    // PROPERTIES (backed by hidden fields)
    public bool IsLoading { get; set; }      // <IsLoading>k__BackingField
    public bool IsMissingObject { get; set; } // <IsMissingObject>k__BackingField
}
```

### EndlessProp.ReferenceFilter Analysis

```csharp
// Property: EndlessProp.ReferenceFilter
// Type: Endless.Gameplay.ReferenceFilter (enum)
// Getter: 7 bytes IL - simple backing field return
// Backing field: <ReferenceFilter>k__BackingField

// The ReferenceFilter is SET via CalculateReferenceFilter(Prop prop)
void CalculateReferenceFilter(Prop prop)
{
    // IL Size: 125 bytes
    // Uses: BaseTypeDefinition, ComponentDefinition
    // Reads prop.BaseTypeId and prop.ComponentIds
    // Sets <ReferenceFilter>k__BackingField based on type/component analysis
}
```

### ROOT CAUSE IDENTIFIED

**Problem:** Using `basePropPrefab` directly as `RuntimePropInfo.EndlessProp` causes issues because:

1. `basePropPrefab` is a SHARED prefab template
2. Its `ReferenceFilter` was calculated for ITS OWN prop data, not our custom prop
3. When `PopulateReferenceFilterMap` iterates:
   - All RuntimePropInfos using basePropPrefab have SAME ReferenceFilter
   - This may conflict with existing entries or cause filter mismatches
4. The prefab may have special Unity lifecycle state issues when accessed during iteration

### CORRECT SOLUTION (Evidence-Based)

**Option A: Instantiate New EndlessProp**
```csharp
// 1. Instantiate new EndlessProp from basePropPrefab
var newEndlessPropGO = UnityEngine.Object.Instantiate(basePropPrefab.gameObject);
var newEndlessProp = newEndlessPropGO.GetComponent<EndlessProp>();

// 2. Set the Prop data
// EndlessProp.Prop has setter (verified in DLL)
newEndlessProp.Prop = ourPropInstance;

// 3. Calculate correct ReferenceFilter for OUR prop
newEndlessProp.CalculateReferenceFilter(ourPropInstance);

// 4. Use in RuntimePropInfo
runtimePropInfo.EndlessProp = newEndlessProp;
```

**Option B: Add Directly to _referenceFilterMap**
```csharp
// Skip PopulateReferenceFilterMap entirely
// Add our RuntimePropInfo directly to the appropriate filter list
var filterMap = _referenceFilterMapField.GetValue(propLibrary);
var noneList = filterMap[ReferenceFilter.None]; // or appropriate filter
noneList.Add(ourRuntimePropInfo);
```

**Option C: Use missingObjectPrefab Correctly**
```csharp
// missingObjectPrefab is designed for missing/placeholder props
// But requires IsMissingObject = true
runtimePropInfo.EndlessProp = missingObjectPrefab;
runtimePropInfo.IsMissingObject = true;
// Then PopulateReferenceFilterMap should handle it as "missing"
```

---

## CRITICAL DISCOVERY: dynamicFilters (2026-01-07 19:30)

### dynamicFilters Array Contents - DLL VERIFIED

```
Field: PropLibrary.dynamicFilters
Type: ReferenceFilter[] (readonly)
Initialized in constructor

IL Analysis of Constructor:
  [1] ldc.i4.2 (array size = 2)
  [2] newarr ReferenceFilter[]
  [8] ldc.i4.0 (index 0)
  [9] ldc.i4.2 (value 2 = Npc)
  [12] ldc.i4.1 (index 1)
  [13] ldc.i4.8 (value 8 = InventoryItem)
  [15] stfld dynamicFilters

RESULT: dynamicFilters = { Npc (2), InventoryItem (8) }

ReferenceFilter enum values:
  None = 0           <-- NOT in dynamicFilters!
  NonStatic = 1      <-- NOT in dynamicFilters!
  Npc = 2            <-- IN dynamicFilters ✓
  PhysicsObject = 4  <-- NOT in dynamicFilters!
  InventoryItem = 8  <-- IN dynamicFilters ✓
  Key = 16           <-- NOT in dynamicFilters!
  Resource = 32      <-- NOT in dynamicFilters!
```

### WHY Props with ReferenceFilter=None Don't Appear

PopulateReferenceFilterMap algorithm:
```
foreach (filter in dynamicFilters)  // Only Npc, InventoryItem
{
    List<RuntimePropInfo> list = new();
    foreach (rpi in loadedPropMap.Values)
    {
        if ((filter & rpi.EndlessProp.ReferenceFilter) != 0)  // BITWISE AND
            list.Add(rpi);
    }
    _referenceFilterMap[filter] = list;
}
```

With ReferenceFilter = None (0):
- filter=Npc (2):         2 & 0 = 0  → NOT added
- filter=InventoryItem (8): 8 & 0 = 0  → NOT added

**RESULT: Props with ReferenceFilter=0 appear in NO category!**

### SOLUTION (DLL-Verified)

Set ReferenceFilter to a value IN dynamicFilters:
- `ReferenceFilter = Npc (2)` → Appears in NPC category
- `ReferenceFilter = InventoryItem (8)` → Appears in Inventory category

```csharp
// WRONG - prop won't appear in UI:
var noneValue = Enum.ToObject(filterEnumType, 0); // None

// CORRECT - prop appears in InventoryItem category:
var inventoryValue = Enum.ToObject(filterEnumType, 8); // InventoryItem
filterProp.SetValue(newEndlessProp, inventoryValue);
```

---

## RESEARCH: IsMissingObject Flag (2026-01-07 21:00)

### DLL Analysis

```
RuntimePropInfo.IsMissingObject:
  Type: Boolean
  CanRead: True
  CanWrite: True
  Backing field: <IsMissingObject>k__BackingField (token 0x04001803)
  Getter token: 0x06001EEA
  Getter IL: 02 7B 03 18 00 04 2A (7 bytes - simple field return)
```

### CRITICAL FINDING: Synchronize Does NOT Check IsMissingObject

Analyzed `UIRuntimePropInfoListModel.Synchronize` IL (268 bytes):
- Token 0x06001EEA (IsMissingObject getter) is NOT present in IL
- Token 0x04001803 (backing field) is NOT present in IL

**Synchronize IGNORES IsMissingObject flag.**

### What Synchronize Actually Checks

```
Synchronize IL tokens accessed:
  [172] callvirt 0x0A000104 = get_IsLoading()
  [79, 182] ldfld 0x0A0004D0 = PropData field
  [84, 187] ldfld 0x0A00006E = sub-field (likely AssetId)
  [209] ldfld 0x0A000105 = PropData
  [214] callvirt 0x0A000727 = PropData.BaseTypeId getter
```

Flow:
1. Check IsLoading - if true, skip prop
2. Access PropData.AssetId for HashSet contains check
3. Access PropData.BaseTypeId for category matching
4. Add to list if passes all checks

### IsMissingObject Purpose (Inferred from other methods)

Used by:
- `AddMissingObjectRuntimePropInfo` - IL size 24 bytes
- `LoadPropPrefab` - IL size 106 bytes
- `UnloadProp` - IL size 227 bytes

The flag appears to be used for:
- Displaying placeholder icon/prefab in the world
- Handling missing asset loading gracefully
- NOT for filtering in the prop tool UI

**CONCLUSION: Setting IsMissingObject=true does NOT prevent UI crashes if other data is invalid.**

---

## RESEARCH: PropData.BaseTypeId Usage (2026-01-07 21:00)

### Field Analysis

```
Prop class (Props.dll):
  baseTypeId : String (private field)
  BaseTypeId : String (public property, getter returns field)
  Getter IL: 02 7B A0 00 00 04 2A (7 bytes)
```

### Methods Using BaseTypeId

```
PropLibrary.GetBaseTypeList(String propDataBaseTypeId):
  Return: List<RuntimePropInfo>
  IL size: 26 bytes
  IL: 02 17 8D 41 00 00 01 25 16 03 28 A3 00 00 0A A4 41 00 00 01 28 E7 1E 00 06 2A

  Algorithm:
  1. Create array of size 1
  2. Store baseTypeId string in array
  3. Call internal lookup method
```

### Current Injection Code Sets baseTypeId

```csharp
// From CreatePropInstance():
SetFieldDeep(prop, "baseTypeId", _validBaseTypeId ?? "treasure");
```

The code attempts to use a valid baseTypeId from existing props, falling back to "treasure".

### Potential Crash Point

If `GetBaseTypeList(baseTypeId)` is called and:
- baseTypeId is null → NullReferenceException
- baseTypeId doesn't exist in database → Returns empty list (graceful)

**Need to verify: Does Synchronize call GetBaseTypeList?**

Looking at token 0x0A000728 in Synchronize IL (position 230) - this could be the lookup call.

---

## RESEARCH: RuntimePropInfo Methods (2026-01-07 21:00)

### Methods That Access Game Database

```
RuntimePropInfo.GetBaseTypeDefinition():
  Return: ComponentDefinition
  IL size: 39 bytes
  Local vars: [0] BaseTypeDefinition
  Accesses: PropData field, looks up definition by baseTypeId

RuntimePropInfo.GetComponentDefinitions():
  Return: List<ComponentDefinition>
  IL size: 86 bytes
  Local vars: [0] List, [1] IEnumerator, [2] String, [3] ComponentDefinition
  Iterates componentIds, looks up each ComponentDefinition

RuntimePropInfo.GetAllDefinitions():
  Return: List<ComponentDefinition>
  IL size: 60 bytes
  Combines base type + component definitions
```

### Crash Risk

These methods access game's definition database. For custom props:
- Our baseTypeId might not exist → Returns null
- Our componentIds are empty → Returns empty list

If UI code calls these methods without null checks, crash occurs.

---

## WHY ReferenceFilter=0 Worked But ReferenceFilter=8 Crashes

### Hypothesis (DLL-Verified)

With ReferenceFilter=0 (None):
- Our prop is NOT added to any _referenceFilterMap list
- PopulateReferenceFilterMap skips it (bitwise AND = 0)
- Synchronize never iterates over our prop
- No crash because our prop is never processed

With ReferenceFilter=8 (InventoryItem):
- Our prop IS added to InventoryItem list in _referenceFilterMap
- PopulateReferenceFilterMap includes it (8 & 8 = 8 ≠ 0)
- Synchronize iterates over our prop
- Accesses PropData.BaseTypeId → "treasure" (valid string)
- Calls GetBaseTypeList("treasure") or similar lookup
- If "treasure" doesn't exist in database → crash or invalid state

### Solution Approaches

1. **Use REAL baseTypeId from existing prop** - Copy from a real loaded prop
2. **Skip filter map entirely** - Add prop but with ReferenceFilter that won't match
3. **Investigate what baseTypeId values exist** - Find valid ones to use

---

## RESEARCH: Prop Tool Opening Flow (2026-01-07 21:30)

### UIPropToolPanelView Methods

```
Start() - IL size: 138 bytes (initialization)
OnToolChange(EndlessTool) - IL size: 89 bytes (called when switching tools)
OnLibraryRepopulated() - IL size: 82 bytes (called when props reload)
```

### OnToolChange Key Calls

```
[39] callvirt 0x06000559
[50] callvirt 0x06001150  <- Likely Synchronize call
[78] callvirt 0x06001150  <- Called again
[83] callvirt 0x06001159
```

Token 0x06001150 appears twice - this is probably UIRuntimePropInfoListModel.Synchronize.

### Crash Point Analysis

The prop tool crash with ReferenceFilter=8 (InventoryItem) occurs because:
1. OnToolChange calls Synchronize for the InventoryItem category
2. Synchronize iterates props in that category
3. Our custom prop is in that list
4. Something about our prop causes an exception (likely invalid data access)

With ReferenceFilter=0 (None):
- Our prop is NOT in any _referenceFilterMap category
- Synchronize never iterates over our prop
- No crash because our prop data is never accessed

---

## NEXT STEP: Test ReferenceFilter=Npc(2)

### Rationale (DLL-Verified)

dynamicFilters contains BOTH Npc(2) and InventoryItem(8).

If the crash is specific to InventoryItem handling:
- Npc props might have different/simpler display code
- Testing Npc(2) would reveal if crash is filter-specific

If the crash is generic to any filter:
- Npc(2) would also crash
- Then we know the issue is in how our prop data is structured

### Before Testing

Need to verify:
1. What data does Npc category display code access?
2. Is there any difference between Npc and InventoryItem handling?

This requires analyzing the UI filter selection code in more detail.

---

## CRITICAL DISCOVERY: Synchronize Accesses EndlessProp.ReferenceFilter (2026-01-07 22:00)

### DLL-Verified IL Analysis

```
UIRuntimePropInfoListModel.Synchronize IL:
  Position 209: ldfld RuntimePropInfo.EndlessProp
  Position 214: callvirt EndlessProp.get_ReferenceFilter

Decoded:
  1. Load RuntimePropInfo from iterator
  2. Load EndlessProp field from RuntimePropInfo
  3. Call getter for ReferenceFilter property on EndlessProp
  4. If EndlessProp is null → NullReferenceException!
```

### Why ReferenceFilter=0 Works

```
ReferenceFilter = None (0):
  - PopulateReferenceFilterMap: Our prop NOT added to any filter list
    (because dynamicFilters = {Npc(2), InventoryItem(8)} and bitwise AND with 0 = 0)
  - Synchronize: Never iterates our prop
  - EndlessProp.ReferenceFilter: Never accessed
  - Result: NO CRASH
```

### Why ReferenceFilter≠0 Crashes

```
ReferenceFilter = Npc(2) or InventoryItem(8):
  - PopulateReferenceFilterMap: Our prop IS added to filter list
  - Synchronize: Iterates props in that category
  - Accesses RuntimePropInfo.EndlessProp → may be null/destroyed
  - Accesses EndlessProp.ReferenceFilter → NullReferenceException!
  - Result: CRASH
```

### Root Cause

The code instantiates EndlessProp as a scene GameObject:
```csharp
var newEndlessPropGO = UnityEngine.Object.Instantiate(basePropPrefab.gameObject);
```

Scene objects can be destroyed on scene changes. When Synchronize runs later:
- EndlessProp reference points to destroyed object
- Unity returns null when accessing destroyed objects
- Accessing .ReferenceFilter on null → crash

### FIX APPLIED (2026-01-07)

Added DontDestroyOnLoad to keep EndlessProp persistent:
```csharp
var newEndlessPropGO = UnityEngine.Object.Instantiate(basePropPrefab.gameObject);
newEndlessPropGO.name = $"CustomProp_{propData.PropId}";
UnityEngine.Object.DontDestroyOnLoad(newEndlessPropGO);  // CRITICAL FIX
newEndlessPropGO.SetActive(false);  // Disable to prevent rendering/physics
```

This ensures EndlessProp survives scene changes and remains accessible to Synchronize.

---

## TEST RESULTS LOG

### Test: ReferenceFilter=Npc(2) WITHOUT DontDestroyOnLoad (2026-01-07)

**Result: Prop tool crashed (didn't open)**

Confirms the crash is NOT filter-specific - ANY non-zero ReferenceFilter crashes.
This supports the hypothesis that EndlessProp access causes the crash.

### Test: ReferenceFilter=Npc(2) WITH DontDestroyOnLoad (2026-01-07)

**Result: REVERTED** - DontDestroyOnLoad was speculative, not DLL-verified

---

## DLL RESEARCH: loadedPropMap Population Chain (2026-01-08)

### Methods That ADD to loadedPropMap (DLL-Verified)

```
Method                              | Operation          | IL Offset
------------------------------------|--------------------|---------
<InjectProp>d__14.MoveNext          | Dictionary.Add     | [312]
<SpawnPropPrefab>d__33.MoveNext     | Dictionary.set_Item| [967], [1224]
<FetchAndSpawnPropPrefab>d__32.MoveNext | Dictionary.set_Item | [566]
<PreloadData>d__30.MoveNext         | Dictionary.set_Item| [820]
<PreloadData>d__30.MoveNext         | Dictionary.Add     | [1088]
AddMissingObjectRuntimePropInfo     | Dictionary.set_Item| [18]
```

### Call Chain for Native Prop Loading

```
CreatorManager.HandleGameUpdated (async state machine)
  ├─ calls IsRepopulateRequired()
  ├─ if true: calls PropLibrary.Repopulate(Stage, CancellationToken)
  │     └─ Repopulate calls LoadPropPrefabs(...)
  │           └─ LoadPropPrefabs calls SpawnPropPrefab(...)
  │                 └─ SpawnPropPrefab calls Dictionary.set_Item (POPULATES loadedPropMap)
  └─ fires OnPropsRepopulated event (Action delegate)
```

### Key Methods (DLL Signatures)

```
PropLibrary.Repopulate(Stage activeStage, CancellationToken cancellationToken) : Task
PropLibrary.LoadPropPrefabs(...) : Task  // 1666 bytes IL in state machine
PropLibrary.SpawnPropPrefab(...) : Task  // 1341 bytes IL in state machine
PropLibrary.InjectProp(Prop, GameObject, Script, Sprite, Transform, EndlessProp) : void
  └─ State machine <InjectProp>d__14 has 405 bytes IL
  └─ At IL[39]: TryGetValue (check if exists)
  └─ At IL[312]: Dictionary.Add (adds to loadedPropMap)
  └─ At IL[339]: List.Add (adds to injectedPropIds)
```

### Why loadedPropMap Stayed Empty (51 Attempts)

```
CONDITION FOR POPULATION:
  HandleGameUpdated only runs when:
    1. User is in Creator mode (CreatorManager.Instance exists)
    2. Game detects changes requiring repopulation
    3. IsRepopulateRequired() returns true

IF USER IS IN MAIN MENU:
  - CreatorManager may not be active
  - HandleGameUpdated doesn't trigger
  - Repopulate never called
  - loadedPropMap stays empty FOREVER
```

### Correct Injection Strategy (Evidence-Based)

**Option A: Wait for OnPropsRepopulated Event**
- Subscribe to CreatorManager.OnPropsRepopulated
- Only inject AFTER native props are loaded
- Requires user to be in Creator mode first

**Option B: Hook into LoadPropPrefabs**
- Patch LoadPropPrefabs to include our prop in the loading
- Runs during normal prop loading
- More integrated with game flow

**Option C: Force Repopulate Call**
- After detecting CreatorManager.Instance
- Call PropLibrary.Repopulate() directly
- Risk: may conflict with game's loading state

### CreatorManager.OnPropsRepopulated

```
Field: OnPropsRepopulated : Action
Type: System.Action (multicast delegate)
Usage: Fired after PropLibrary.Repopulate completes

To subscribe:
  var currentAction = (Action)OnPropsRepopulatedField.GetValue(creatorManager);
  Action myHandler = () => { /* inject props here */ };
  var combined = (Action)Delegate.Combine(currentAction, myHandler);
  OnPropsRepopulatedField.SetValue(creatorManager, combined);
```

---

## SESSION FINDINGS (2026-01-08 Session 2)

### RESOLVED: Prop Tool Crash (FormatException)

**Root Cause:** AssetID must be valid GUID format (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx)

**Error from Player.log:**
```
FormatException: Guid should contain 32 digits with 4 dashes
at Endless.Creator.LevelEditing.Runtime.ToolManager.ActivatePropBasedTool
```

**Fix:** Generate deterministic GUID from PropId using MD5 hash:
```csharp
private static Guid GenerateGuidFromString(string input)
{
    using (var md5 = MD5.Create())
    {
        byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        return new Guid(hash);
    }
}

// Usage:
var assetGuid = GenerateGuidFromString(propData.PropId);
SetFieldDeep(prop, "AssetID", assetGuid.ToString());
```

**Result:** Prop tool now opens without crash.

---

### DISPROVED: Field vs Property Hypothesis

**Hypothesis:** `activePropLibrary` field and `ActivePropLibrary` property return different instances.

**Test Result:**
```
[DIAG] PropLibrary (via FIELD) hashcode: 1897418584
[DIAG] PropLibrary (via PROPERTY) hashcode: 1897418584
[DIAG] SAME INSTANCE? True
```

**Conclusion:** Field and property return the SAME instance. This is NOT the cause of issues.

---

### VERIFIED: loadedPropMap Population Sequence

**Finding:** loadedPropMap starts empty (Count=0) and fills over time:
```
[Attempt 1-60] Count value (via FIELD): 0
[Attempt 61] loadedPropMap has 75 props
[Attempt 66] loadedPropMap has 76 props AND _referenceFilterMap is ready
```

**Conclusion:** Must wait for both loadedPropMap AND _referenceFilterMap to be populated before injecting.

---

### VERIFIED: Injection Successfully Adds to _referenceFilterMap

**Log Evidence:**
```
[FILTER_MAP] Category InventoryItem: 9 props  (was 8, +1 for our prop)
Added RuntimePropInfo with NEW EndlessProp for pearl_basket
```

**Conclusion:** Prop IS being added to internal data structures correctly.

---

### CRITICAL: UI Categories Are Internal, Not Visible

**User Clarification:** The prop tool shows ALL props in a flat grid. There are NO visible category tabs/filters in the UI. Categories (Npc, InventoryItem, NonStatic, etc.) are INTERNAL data organization only.

**Implication:** Changing ReferenceFilter category does NOT affect which category tab user sees (there are none). The prop should appear in the flat grid regardless of category.

---

### UNRESOLVED: Prop Not Visible in UI Despite Correct Injection

**Current Status:**
1. Prop tool opens (no crash) ✓
2. Prop added to loadedPropMap ✓
3. Prop added to _referenceFilterMap (InventoryItem: 9 props) ✓
4. Prop NOT visible in UI grid ✗

**Event Sequence from Log:**
```
Injection complete - stopping poll loop
=== OnCreatorStarted event fired! (New session detected) ===
[Helper] Destroyed
```

**Observation:** OnCreatorStarted fires AFTER injection completes.

---

### NEXT INVESTIGATION: UI Data Source

**Question:** How does UIRuntimePropInfoListModel.Synchronize get its props?

**From DLL Analysis:**
```
METHOD: Void Synchronize(ReferenceFilter referenceFilter, IReadOnlyList`1 propsToIgnore)
FIELD: List : List`1
```

**Possible Causes (require verification):**
1. UI list populated before our injection
2. OnCreatorStarted triggers repopulation
3. UI uses data source other than _referenceFilterMap

**Possible Solutions (require research):**
1. Inject before UI Synchronize runs
2. Call Synchronize again after injection
3. Hook Synchronize to add our prop
4. Add directly to UIRuntimePropInfoListModel.List

---

## CURRENT STATUS (2026-01-08)

### Problem Summary
1. ~~Prop tool crashes~~ FIXED (GUID format)
2. ~~loadedPropMap empty~~ RESOLVED (wait for population)
3. Prop injected to data structures but NOT visible in UI

### Next Steps (Research-Based)
1. ~~Research when UIRuntimePropInfoListModel.Synchronize is called~~ DONE
2. Determine how to refresh UI after injection
3. Or identify correct injection point before UI loads

---

## RESEARCH: When Synchronize is Called (2026-01-08)

### DLL-Verified Analysis

```
UIRuntimePropInfoListModel.Synchronize
  Token: 0x06000559
  Signature: Void Synchronize(ReferenceFilter referenceFilter, IReadOnlyList`1 propsToIgnore)
```

### Methods That Call Synchronize

| Method | Token | IL Offset | Trigger |
|--------|-------|-----------|---------|
| UIPropToolPanelView.OnToolChange | 0x06000B89 | 39 | User switches to prop tool |
| UIPropToolPanelView.OnLibraryRepopulated | 0x06000B8A | 32 | OnPropsRepopulated event fires |

### Methods That Do NOT Call Synchronize

| Method | Token | What It Does |
|--------|-------|--------------|
| UIPropToolPanelView.Start | 0x06000B88 | Subscribes to events via CreatorManager.add_OnPropsRepopulated |
| UIPropToolPanelView.OnCreatorStarted | 0x06000B8C | Just logs (DebugUtility.LogMethod) |
| UIPropToolPanelView.OnCreatorEnded | 0x06000B8D | Just logs |

### Event Subscription Chain (from Start IL)

```
Start() calls:
  - UIItemSelectionToolPanelView`2.Start (base class)
  - NetworkBehaviourSingleton`1.get_Instance (CreatorManager)
  - CreatorManager.add_OnPropsRepopulated  ← Subscribes OnLibraryRepopulated!
  - UnityEvent`1.AddListener
```

### ROOT CAUSE: Event Timing

**Sequence of events:**
```
1. User enters Creator mode
2. HandleGameUpdated → Repopulate() → populates loadedPropMap + _referenceFilterMap
3. OnPropsRepopulated fires → OnLibraryRepopulated → Synchronize (UI list populated)
4. Our injection detects _referenceFilterMap is ready
5. Our injection adds prop to loadedPropMap + _referenceFilterMap
6. OnCreatorStarted fires (does NOT call Synchronize)
```

**Result:** Synchronize runs BEFORE our injection. Prop is in data structures, but UI's internal List was already populated without it.

### Solution Options

**Option 1: Call Synchronize again after injection**
- Get UIPropToolPanelView instance
- Get its runtimePropInfoListModel field
- Call Synchronize() to refresh UI list

**Option 2: Inject during OnPropsRepopulated**
- Subscribe our injection to OnPropsRepopulated
- Inject BEFORE OnLibraryRepopulated runs
- Synchronize will include our prop

**Option 3: Add directly to UIRuntimePropInfoListModel.List**
- Access the UI's internal List field
- Add our RuntimePropInfo directly
- Bypass Synchronize entirely
