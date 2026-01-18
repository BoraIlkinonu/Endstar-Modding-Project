# Endstar Prop System - Complete Flow Map

## KEY SINGLETONS (Access Points)

```
StageManager.Instance          -> StageManager (MonoBehaviourSingleton)
CreatorManager.Instance        -> CreatorManager (NetworkBehaviourSingleton)
```

## DATA STRUCTURES

### InjectedProps (Endless.Gameplay.LevelEditing.Level.InjectedProps)
```csharp
class InjectedProps {
    public Prop Prop;              // The prop definition
    public GameObject TestPrefab;   // Optional test prefab
    public Script TestScript;       // Optional test script
    public Sprite Icon;             // Display icon
}
```

### RuntimePropInfo (Endless.Gameplay.LevelEditing.PropLibrary+RuntimePropInfo)
```csharp
class RuntimePropInfo {
    Prop PropData;                 // The actual prop
    Sprite Icon;                   // Display icon
    EndlessProp EndlessProp;       // Runtime prefab instance
    bool IsLoading;
    bool IsMissingObject;
}
```

### Prop (Endless.Props.Assets.Prop) extends Asset extends AssetCore
```csharp
class Prop {
    // From AssetCore
    string Name;
    string AssetID;
    string AssetVersion;
    string AssetType;

    // From Asset
    string Description;

    // Prop-specific
    string baseTypeId;             // Category (e.g., "treasure", "decoration")
    bool openSource;               // Can be used in Creator
    Vector3Int bounds;             // Size bounds
    List<string> componentIds;
    AssetReference prefabBundle;
    // ... more

    // KEY METHOD
    AssetReference ToAssetReference();  // Creates key for loadedPropMap
}
```

### AssetReference (Endless.Assets.AssetReference)
```csharp
class AssetReference {
    string AssetID;
    string AssetVersion;
    string AssetType;
    bool UpdateParentVersion;
}
```

## STORAGE LOCATIONS

### StageManager
```
StageManager.Instance
    ├── activePropLibrary: PropLibrary     <- THE ACTIVE PROP LIBRARY
    ├── injectedProps: List<InjectedProps> <- LIST OF INJECTED PROPS
    └── InjectProp(Prop, GameObject, Script, Sprite) -> void
```

### PropLibrary
```
PropLibrary
    ├── loadedPropMap: Dictionary<AssetReference, RuntimePropInfo>  <- MAIN PROP STORAGE
    ├── injectedPropIds: List<string>      <- IDs of injected props
    ├── prefabSpawnRoot: Transform
    ├── basePropPrefab: EndlessProp
    └── InjectProp(Prop, GameObject, Script, Sprite, Transform, EndlessProp) -> void
```

### CreatorManager
```
CreatorManager.Instance
    ├── OnPropsRepopulated: Action         <- EVENT when props are repopulated
    ├── OnRepopulate: Action
    └── propLibraryCancelTokenSource
```

## FLOW: HOW PROPS GET TO THE UI

```
1. GAME STARTUP / LEVEL LOAD
   └── StageManager.Instance is created (MonoBehaviourSingleton)
       └── activePropLibrary = new PropLibrary(...)
           └── PropLibrary constructor receives:
               - prefabSpawnRoot: Transform
               - loadingObjectProp: EndlessProp
               - basePropPrefab: EndlessProp
               - missingObjectPrefab: EndlessProp

2. ENTERING CREATOR MODE
   └── CreatorManager starts
       └── PropLibrary.Repopulate(Stage, CancellationToken) is called
           └── This is an ASYNC method returning Task<T>
           └── Loads all props from game library into loadedPropMap
           └── When done, CreatorManager.OnPropsRepopulated is invoked

3. PROP TOOL UI DISPLAY
   └── UIPropToolPanelView
       ├── OnLibraryRepopulated() <- Called when CreatorManager.OnPropsRepopulated fires
       └── runtimePropInfoListModel.Synchronize(filter, propList)
           └── This updates the UI list with props from PropLibrary

4. GETTING ALL PROPS
   └── PropLibrary.GetAllRuntimeProps() -> RuntimePropInfo[]
       └── Returns all props in loadedPropMap
```

## FLOW: HOW TO INJECT A CUSTOM PROP

```
OPTION A: Via StageManager.InjectProp (4 params)
   StageManager.Instance.InjectProp(prop, testPrefab, testScript, icon)
   └── Creates InjectedProps object
   └── Adds to StageManager.injectedProps list
   └── Calls PropLibrary.InjectProp(...)

OPTION B: Via PropLibrary.InjectProp (6 params)
   PropLibrary.InjectProp(prop, testPrefab, testScript, icon, prefabTransform, propPrefab)
   └── Creates RuntimePropInfo
   └── Creates AssetReference key using prop.ToAssetReference()
   └── Adds to loadedPropMap dictionary
   └── Adds prop ID to injectedPropIds list

OPTION C: Direct loadedPropMap manipulation
   propLibrary.loadedPropMap[assetRef] = runtimePropInfo
   └── Requires creating valid AssetReference key
   └── Requires creating valid RuntimePropInfo value
```

## CRITICAL TIMING

```
WHEN TO INJECT:
   ├── AFTER PropLibrary is created (StageManager.Instance.activePropLibrary != null)
   ├── AFTER Repopulate completes (props are loaded)
   └── BEFORE UI requests props (GetAllRuntimeProps)

HOOK POINTS:
   1. PropLibrary constructor (earliest - but may not have all prefabs ready)
   2. PropLibrary.Repopulate (after async completion)
   3. CreatorManager.OnPropsRepopulated event (safest - all props loaded)
   4. PropLibrary.GetAllRuntimeProps (lazy - inject when UI asks)
```

## WHY CURRENT HOOKS AREN'T FIRING

```
PROBLEM: Harmony patches on PropLibrary methods aren't being called

POSSIBLE REASONS:
1. PropLibrary may already be instantiated BEFORE patches are applied
   - Patches are applied in Awake/Start
   - PropLibrary might be created in scene initialization

2. Method signature mismatch
   - Repopulate returns Task<T> (async)
   - GetAllRuntimeProps returns RuntimePropInfo[]
   - Need to verify exact signatures match

3. The actual call path might be different
   - Methods might be called via interfaces
   - Methods might be inlined or optimized

SOLUTION APPROACH:
1. Hook into EVENTS instead of methods
   - CreatorManager.OnPropsRepopulated is an Action (delegate)
   - Can subscribe to it at runtime

2. Use FindObjectOfType to get instances
   - StageManager.Instance
   - CreatorManager.Instance

3. Use polling/coroutine approach
   - Wait for activePropLibrary to be non-null
   - Then inject directly into loadedPropMap
```

## RECOMMENDED INJECTION APPROACH

```csharp
// 1. Get StageManager singleton
var stageManagerType = gameplayAsm.GetType("Endless.Gameplay.LevelEditing.Level.StageManager");
var instanceProp = stageManagerType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
var stageManager = instanceProp.GetValue(null);

// 2. Get activePropLibrary
var propLibField = stageManagerType.GetField("activePropLibrary", BindingFlags.Public | BindingFlags.Instance);
var propLibrary = propLibField.GetValue(stageManager);

// 3. Wait for it to be non-null, then inject
if (propLibrary != null) {
    // Get loadedPropMap
    var loadedPropMapField = propLibraryType.GetField("loadedPropMap", BindingFlags.NonPublic | BindingFlags.Instance);
    var loadedPropMap = loadedPropMapField.GetValue(propLibrary);

    // Create Prop instance
    var prop = CreatePropInstance();

    // Create AssetReference key
    var assetRef = prop.ToAssetReference();

    // Create RuntimePropInfo value
    var runtimePropInfo = CreateRuntimePropInfo(prop, icon);

    // Add to dictionary
    loadedPropMap.Add(assetRef, runtimePropInfo);
}
```

## SUBSCRIBING TO OnPropsRepopulated

```csharp
// 1. Get CreatorManager
var creatorManagerType = creatorAsm.GetType("Endless.Creator.CreatorManager");
var instanceProp = creatorManagerType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
var creatorManager = instanceProp.GetValue(null);

// 2. Get OnPropsRepopulated field (it's an Action)
var onPropsRepopulatedField = creatorManagerType.GetField("OnPropsRepopulated",
    BindingFlags.Public | BindingFlags.Instance);
var currentAction = onPropsRepopulatedField.GetValue(creatorManager) as Action;

// 3. Add our handler
Action myHandler = () => {
    Log.LogInfo("Props repopulated! Injecting custom props...");
    InjectCustomProps();
};

var newAction = (Action)Delegate.Combine(currentAction, myHandler);
onPropsRepopulatedField.SetValue(creatorManager, newAction);
```
