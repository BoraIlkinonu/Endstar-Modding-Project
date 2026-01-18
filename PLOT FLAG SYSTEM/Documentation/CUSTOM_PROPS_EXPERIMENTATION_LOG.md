# Custom Props Plugin - Experimentation Log

## Objective
Create a BepInEx plugin to inject unlimited custom props into Endstar's native prop system.

---

## Phase 1: Game System Discovery

### Types Found via Reflection

**PropLibrary** (`Endless.Gameplay.LevelEditing.PropLibrary`)
- Key method: `InjectProp(Prop, GameObject, Script, Sprite, Transform, EndlessProp)`
- Other methods: `GetAllRuntimeProps`, `FetchAndSpawnPropPrefab`, `IsInjectedProp`
- NOT a MonoBehaviour in scene - access mechanism unknown

**RuntimePropInfo** (`Endless.Gameplay.LevelEditing.PropLibrary+RuntimePropInfo`)
- Nested class inside PropLibrary
- Fields: `PropData (Prop)`, `Icon (Sprite)`, `EndlessProp (EndlessProp)`
- Properties: `IsLoading`, `IsMissingObject`

**Prop** (`Endless.Props.Assets.Prop` in Props.dll)
- ScriptableObject base class: `Asset`
- Key fields:
  - `Name`, `AssetID`, `Description`
  - `baseTypeId` - prop category (e.g., "treasure")
  - `prefabBundle` - AssetReference to 3D model
  - `scriptAsset` - AssetReference to Lua script
  - `iconFileInstanceId` - icon reference

**EndlessProp** (`Endless.Gameplay.Scripting.EndlessProp`)
- MonoBehaviour component attached to spawned props
- Properties: `Prop`, `WorldObject`, `TransformMap`

**InjectedProps** (`Endless.Gameplay.LevelEditing.Level.InjectedProps`)
- Fields: `Prop`, `TestPrefab (GameObject)`, `TestScript`, `Icon (Sprite)`
- Designed for injecting props with direct references (not Addressables)

---

## Phase 2: PropLibrary Search Experiments

### The Core Problem
PropLibrary type was found, but no INSTANCE could be located in the scene.

### Search Methods Attempted

| Method | Result |
|--------|--------|
| `FindObjectOfType(PropLibraryType)` | Failed - returns null |
| Search all MonoBehaviours for "PropLibrary" | Not found - PropLibrary is NOT a MonoBehaviour |
| `Resources.FindObjectsOfTypeAll()` | Failed |
| Static Instance/Current/Singleton property | No such property exists |
| Search through PropLibraryReference | Found 0 references with PropLibrary field |
| Search through PropTool | PropTool found but has no PropLibrary field |
| Search through Stage | Stage not found in MainScene |

### PropTool Analysis
Found `Endless.Creator.LevelEditing.Runtime.PropTool` in scene with fields:
- `scriptWindow`, `OnSelectedAssetChanged`
- Network-related fields (`NetworkBehaviourIdCache`, etc.)
- NO PropLibrary reference

### Scene Analysis
- Only `MainScene` detected via `SceneManager.sceneLoaded`
- Game appears to load gameplay content additively without triggering scene events
- PropTool and UI components exist but no Stage or PropLibrary

---

## Phase 3: Unity Lifecycle Experiments

### The Problem
BepInEx plugins inherit from `BaseUnityPlugin` (MonoBehaviour) but don't receive Unity lifecycle callbacks.

### Experiments

#### 1. Direct Update() in Plugin
```csharp
private void Update() { /* search code */ }
```
**Result:** Never called - BepInEx plugins don't receive Update()

#### 2. Coroutine from Awake()
```csharp
StartCoroutine(PropLibrarySearchCoroutine());
```
**Result:** Coroutine starts but `WaitForSeconds` never completes

#### 3. InvokeRepeating from Awake()
```csharp
InvokeRepeating("SearchMethod", 5f, 3f);
```
**Result:** Never invoked

#### 4. SceneManager.sceneLoaded Event
```csharp
SceneManager.sceneLoaded += OnSceneLoaded;
```
**Result:** Works! Event fires for MainScene (mode: 2)
- But StartCoroutine in callback fails with NullReferenceException

#### 5. Helper GameObject with MonoBehaviour
```csharp
var helperGO = new GameObject("CustomPropsHelper");
DontDestroyOnLoad(helperGO);
var helper = helperGO.AddComponent<PropSearchHelper>();
```

**Helper with Update():** Update() never called
**Helper with Start():** Start() never called
**Helper with OnEnable():** OnEnable() IS called!
**Helper with InvokeRepeating in OnEnable():** InvokeRepeating never fires

#### 6. .NET System.Threading.Timer
```csharp
_timer = new System.Threading.Timer(TimerCallback, null, 3000, 5000);
```
**Result:** Timer fires but:
- BepInEx logging doesn't work from background threads
- Unity API calls from background thread may fail

---

## Phase 4: Components Found in Scene

When searching MainScene, these prop-related components were found:

| Component | GameObject |
|-----------|------------|
| `UIPropToolPanelController` | Prop Tool Panel |
| `PropTool` | Prop Tool |
| `PropLocationMarker` | Prop Location Marker |
| `UIRuntimePropInfoView` | Runtime Prop Info |
| `UIRuntimePropInfoListView` | Runtime Prop Info Vertical List |
| `UIRuntimePropInfoListController` | Filterable List Controller |
| `UIRuntimePropInfoPresenter` | Runtime Prop Info |
| `UIPropToolPanelView` | Prop Tool Panel |

None of these have a direct PropLibrary reference.

---

## Key Findings

### What Works
1. SceneManager.sceneLoaded event fires
2. Helper GameObject OnEnable() is called
3. Reflection to find game types works perfectly
4. Test prop registration works (PropRegistry)

### What Doesn't Work
1. Any Unity timing mechanism (Update, Coroutine, InvokeRepeating)
2. Finding PropLibrary instance in scene
3. BepInEx logging from background threads

### Architecture Insights
1. PropLibrary is likely a **ScriptableObject asset** or **service**, not a scene component
2. Game uses **Addressables** for prop loading (AssetReference fields)
3. PropTool works with **asset IDs** (SerializableGuid), not direct PropLibrary access
4. Stage component only exists in gameplay levels, not MainScene

---

## Phase 5: Harmony Patching Solution (IMPLEMENTED)

### The Solution
Since PropLibrary can't be found via scene search, we intercept it via **Harmony patches** on its methods.

### Patches Applied
1. **PropLibrary.GetAllRuntimeProps** - Called when UI displays props
2. **PropLibrary.FetchAndSpawnPropPrefab** - Called when spawning props
3. **PropLibrary.InjectProp** - Called when game injects props
4. **PropLibrary Constructor** - Called when instance is created

### How It Works
```csharp
// Harmony prefix method - captures __instance parameter
private static void GetAllRuntimePropsPrefix(object __instance)
{
    if (_capturedPropLibrary == null)
    {
        _capturedPropLibrary = __instance;
        // Now we have the PropLibrary instance!
        ScheduleInjection();
    }
}
```

### Expected Log Output
When the game calls any patched method, you should see:
```
=== CAPTURED PropLibrary via GetAllRuntimeProps: [instance] ===
Scheduling prop injection...
=== Injecting custom props into PropLibrary ===
```

---

## Phase 6: PropLibraryReference Discovery

### Key Finding
`PropLibraryReference.GetReference()` returns `RuntimePropInfo`, NOT `PropLibrary`!
```
Found 1 GetReference methods:
  Endless.Gameplay.LevelEditing.PropLibrary+RuntimePropInfo GetReference()
```

This means `PropLibraryReference` is a reference to a **single prop**, not the entire library.

### Updated Approach
Since PropLibrary methods aren't being called when expected, now patching UI components:

1. **UIRuntimePropInfoListModel** - Analyzes fields/methods for PropLibrary references
2. **UIRuntimePropInfoListController** - Patches Awake/Start/OnEnable to search fields
3. **Field scanning** - When UI initializes, scans all fields for PropLibrary type

### Architecture Understanding (UPDATED from Library Analysis)
```
PropLibrary (REGULAR CLASS - not MonoBehaviour/ScriptableObject!)
  Constructor: PropLibrary(Transform, EndlessProp, EndlessProp, EndlessProp)
  Fields:
    - loadedPropMap (Dictionary)
    - injectedPropIds (List)
    - prefabSpawnRoot (Transform)
    - basePropPrefab, missingObjectPrefab, loadingObjectProp (EndlessProp)
  Methods:
    - InjectProp(Prop, GameObject, Script, Sprite, Transform, EndlessProp)
    - GetAllRuntimeProps() -> RuntimePropInfo[]
    - GetRuntimePropInfo(AssetReference) -> RuntimePropInfo

StageManager
  - Has <LoadPropLibraryReferences>d__74 async method
  - LIKELY HOLDS PropLibrary instance
  - Need to find StageManager in scene when level loads

PropLibraryReference (struct)
  └── GetReference() -> RuntimePropInfo (single prop reference)

UI Layer:
  UIRuntimePropInfoListController
    └── UIRuntimePropInfoListModel
      └── displays RuntimePropInfo items
```

### Key Discovery
PropLibrary is **NOT** a ScriptableObject or MonoBehaviour - it's a regular class!
- Created by StageManager with constructor arguments
- Must find StageManager to access PropLibrary instance
- InjectProp method exists and takes: Prop, GameObject, Script, Sprite, Transform, EndlessProp

---

## Next Steps to Try (If Patching Fails)

1. **Find service locator** - Search for a ServiceLocator, DependencyInjection, or Manager class that provides PropLibrary

2. **Patch AddressableAssets** - Hook into Addressables loading to intercept when PropLibrary asset is loaded

3. **Wait for Stage** - Only search when a Stage component exists (actual gameplay level)

4. **Analyze game startup** - Use Harmony to patch game initialization and trace how PropLibrary is created/accessed

---

## Files Created

| File | Purpose |
|------|---------|
| `CustomPropsPlugin.cs` | Main BepInEx plugin |
| `PropIntegration.cs` | Reflection-based type discovery and injection |
| `PropRegistry.cs` | Local prop storage |
| `CustomPropPickup.cs` | Pickup behavior component |
| `PropSearchHelper` | Helper MonoBehaviour for lifecycle |

---

## Plugin Status

- **Loads:** Yes
- **Discovers Types:** Yes (all key types found)
- **Registers Test Prop:** Yes
- **Harmony Patches Applied:** Yes (4 methods patched)
- **Captures PropLibrary Instance:** Pending (via Harmony patches)
- **Injects Props:** Ready to inject when PropLibrary captured

### Testing Steps
1. Launch Endstar with the plugin enabled
2. Enter a level with the prop tool (open Creation Hub or edit a level)
3. Open the prop panel - this triggers `GetAllRuntimeProps`
4. Check BepInEx log for `=== CAPTURED PropLibrary ===` message
5. If captured, check for injection success messages

---

## Phase 7: StageManager Solution (SUCCESS!)

### Key Discovery
**StageManager** (`Endless.Gameplay.LevelEditing.Level.StageManager`) is the answer!

```
StageManager
  - Inherits: MonoBehaviourSingleton<StageManager> (SINGLETON!)
  - Field: activePropLibrary (PropLibrary) ← THIS IS WHAT WE NEED
  - Method: InjectProp(Prop prop, GameObject testPrefab, Script testScript, Sprite icon)
```

### Why StageManager Works
1. It's a **MonoBehaviourSingleton** - accessible via `StageManager.Instance`
2. It holds `activePropLibrary` field - the PropLibrary instance we couldn't find
3. It has its own `InjectProp` method with **4 parameters** (simpler than PropLibrary's 6)

### StageManager.InjectProp Signature
```csharp
public void InjectProp(
    Prop prop,           // The prop data object
    GameObject testPrefab, // 3D model prefab
    Script testScript,   // Lua script (can be null)
    Sprite icon          // UI icon (REQUIRED for visibility!)
)
```

### Implementation
```csharp
// Find StageManager singleton
var instanceProp = _stageManagerType.GetProperty("Instance",
    BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
var stageManager = instanceProp.GetValue(null);

// Get activePropLibrary (for reference)
var propLibField = _stageManagerType.GetField("activePropLibrary",
    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
var propLib = propLibField.GetValue(stageManager);

// Inject via StageManager
var injectMethod = stageManager.GetType().GetMethod("InjectProp");
injectMethod.Invoke(stageManager, new object[] { propInstance, prefab, null, icon });
```

### Prop Type Discovery
**Critical Finding:** `Prop` is NOT a ScriptableObject!

```
Prop type chain: Prop -> Asset -> AssetCore -> Object
Is ScriptableObject: False
```

Solution: Use regular constructor instead of `ScriptableObject.CreateInstance()`:
```csharp
var ctor = PropIntegration.PropType.GetConstructor(Type.EmptyTypes);
propInstance = ctor.Invoke(null);
```

### Log Output (SUCCESS!)
```
Found StageManager via Instance: Stage Manager (Endless.Gameplay.LevelEditing.Level.StageManager)
Found activePropLibrary: Endless.Gameplay.LevelEditing.PropLibrary
=== Injecting custom props via StageManager ===
StageManager.InjectProp has 4 parameters:
  - prop: Endless.Props.Assets.Prop
  - testPrefab: UnityEngine.GameObject
  - testScript: Endless.Props.Assets.Script
  - icon: UnityEngine.Sprite
Injecting: custom_pearl_basket
Constructor result: { AssetCore: { Name: , AssetID: } }
Created Prop instance: { Name: Pearl Basket, AssetID: custom_pearl_basket, AssetType: Prop }
StageManager.InjectProp SUCCESS! Result:
```

### Current Issue
**Prop not visible in UI** because `icon` parameter is null.
- The prop is injected successfully
- But without an icon, it doesn't appear in the prop tool panel
- Need to create a runtime Sprite or load from asset bundle

---

## Phase 8: Icon Creation (IN PROGRESS)

### Problem
Props need a valid `Sprite` icon to appear in the UI prop list.

### Solutions
1. **Runtime Texture** - Create a simple colored Texture2D and convert to Sprite
2. **Asset Bundle** - Build proper icons in Unity and load at runtime
3. **Steal from game** - Find and clone an existing game prop's icon

### Next Steps
1. Create runtime icon using Texture2D
2. Pass icon to StageManager.InjectProp
3. Verify prop appears in UI
4. Build proper Unity asset bundle with real models and icons

---

## Architecture Summary

```
┌─────────────────────────────────────────────────────────────┐
│                    ENDSTAR PROP SYSTEM                       │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  StageManager (MonoBehaviourSingleton)                      │
│    ├── Instance (static property) ← ACCESS POINT            │
│    ├── activePropLibrary: PropLibrary                       │
│    └── InjectProp(Prop, GameObject, Script, Sprite)         │
│                           │                                  │
│                           ▼                                  │
│  PropLibrary (regular class)                                │
│    ├── loadedPropMap: Dictionary                            │
│    ├── injectedPropIds: List                                │
│    ├── GetAllRuntimeProps() → RuntimePropInfo[]             │
│    └── InjectProp(Prop, GO, Script, Sprite, Transform, EP)  │
│                           │                                  │
│                           ▼                                  │
│  RuntimePropInfo (nested class)                             │
│    ├── PropData: Prop                                       │
│    ├── Icon: Sprite ← REQUIRED FOR UI                       │
│    └── EndlessProp                                          │
│                           │                                  │
│                           ▼                                  │
│  UI Layer                                                   │
│    └── UIRuntimePropInfoListController                      │
│        └── displays RuntimePropInfo with icons              │
│                                                              │
└─────────────────────────────────────────────────────────────┘

Prop (Endless.Props.Assets.Prop)
  ├── NOT a ScriptableObject (inherits Asset → AssetCore → Object)
  ├── Create via: new Prop() (parameterless constructor)
  └── Fields: Name, AssetID, Description, baseTypeId, openSource
```

---

## Plugin Status (UPDATED)

| Feature | Status |
|---------|--------|
| Plugin Loads | ✅ Working |
| Type Discovery | ✅ Working |
| StageManager Access | ✅ Working |
| PropLibrary Access | ✅ Working |
| Prop Creation | ✅ Working |
| StageManager.InjectProp | ✅ Working |
| Prop Visible in UI | ❌ Needs Icon |
| Custom 3D Models | ❌ Needs AssetBundle |
