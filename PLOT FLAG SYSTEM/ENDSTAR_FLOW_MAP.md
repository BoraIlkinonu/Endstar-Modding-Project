# Endstar Dynamic Flow Map

## LAST UPDATED: 2026-01-07 17:45

---

## 1. INITIALIZATION FLOW

### From DLL Analysis:
- `StageManager` extends `MonoBehaviourSingleton<T>` - Unity singleton
- `CreatorManager` extends `NetworkBehaviourSingleton<T>` - Networked singleton
- `PropLibrary` extends `Object` - Plain C# class, NOT a MonoBehaviour

### Key Methods for Loading:
```
StageManager.LoadLevel(LevelState, bool loadLibraryPrefabs, CancellationToken, Action<float>)
  └── LoadPropLibraryReferences(LevelState, CancellationToken, Action<float>)
      └── PropLibrary.LoadPropPrefabs(LevelState, Transform, EndlessProp, EndlessProp, CancellationToken, Action)
          └── Populates loadedPropMap
```

### Creator Mode Initialization:
```
CreatorManager.EnteringCreator()
CreatorManager.CreatorLoaded()
CreatorManager.OnCreatorStarted (UnityEvent) - fires when creator starts
```

---

## 2. SCENE FLOW

### CONFIRMED:
- "MainScene" is the main scene that contains EVERYTHING
- Creator mode is NOT a separate scene
- Creator mode is a STATE within MainScene
- Props are loaded when a LEVEL is loaded, not when scene loads

### State Detection:
- `CreatorManager.OnCreatorStarted` - fires when entering creator
- `CreatorManager.OnCreatorEnded` - fires when leaving creator
- `UIPropToolPanelView.inCreatorMode` (bool) - tracks if in creator mode

---

## 3. PROP SYSTEM FLOW

### CONFIRMED Architecture:
```
StageManager (MonoBehaviourSingleton)
  ├── activePropLibrary: PropLibrary      [PRIVATE field, PUBLIC property getter]
  ├── injectedProps: List<InjectedProps>  [PRIVATE]
  ├── basePropPrefab: EndlessProp         [PRIVATE]
  ├── prefabSpawnTransform: Transform     [PRIVATE]
  └── InjectProp(Prop, GameObject, Script, Sprite)  [PUBLIC]
        └── Calls PropLibrary.InjectProp internally? (NEED TO VERIFY)

PropLibrary (plain Object, NOT MonoBehaviour)
  ├── loadedPropMap: Dictionary<AssetReference, RuntimePropInfo>  [PRIVATE]
  ├── injectedPropIds: HashSet<?>         [PRIVATE - tracks injected props!]
  ├── basePropPrefab: EndlessProp         [PRIVATE]
  ├── prefabSpawnRoot: Transform          [PRIVATE]
  ├── InjectProp(Prop, GameObject, Script, Sprite, Transform, EndlessProp)  [PUBLIC, 6 params]
  ├── IsInjectedProp(string propDataAssetID)  [PUBLIC]
  ├── GetReferenceFilteredDefinitionList(ReferenceFilter)  [PUBLIC - UI uses this!]
  ├── GetAllRuntimeProps()                [PUBLIC]
  └── Repopulate(Stage, CancellationToken)  [PUBLIC - triggers repopulation]

RuntimePropInfo (nested class: PropLibrary+RuntimePropInfo)
  ├── PropData: Prop          [PUBLIC field]
  ├── Icon: Sprite            [PUBLIC field]
  ├── EndlessProp: EndlessProp  [PUBLIC field]
  ├── IsLoading: bool         [property with backing field]
  └── IsMissingObject: bool   [property with backing field]

InjectedProps (class, NOT struct!)
  ├── Prop: Prop
  ├── TestPrefab: GameObject
  ├── TestScript: Script
  └── Icon: Sprite
```

### KEY FINDING: Two InjectProp Methods!
1. `StageManager.InjectProp(Prop, GameObject, Script, Sprite)` - 4 params
2. `PropLibrary.InjectProp(Prop, GameObject, Script, Sprite, Transform, EndlessProp)` - 6 params

StageManager.InjectProp adds to `injectedProps` list.
PropLibrary.InjectProp likely adds to `loadedPropMap` AND `injectedPropIds`.

---

## 4. UI FLOW

### CONFIRMED Architecture:
```
UIPropToolPanelView (extends UIItemSelectionToolPanelView)
  ├── runtimePropInfoListModel: UIRuntimePropInfoListModel  [PRIVATE]
  ├── selectedAssetId: SerializableGuid   [PRIVATE]
  ├── inCreatorMode: bool                 [PRIVATE]
  ├── Tool: PropTool                      [PROTECTED]
  ├── OnLibraryRepopulated()              [Called when props change!]
  ├── OnSelectedAssetChanged(SerializableGuid)
  ├── OnCreatorStarted()
  └── OnCreatorEnded()

UIRuntimePropInfoListModel (extends UIBaseLocalFilterableListModel)
  ├── List: List<RuntimePropInfo>         [PROTECTED]
  └── Synchronize(ReferenceFilter, IReadOnlyList<> propsToIgnore)  [GETS PROP LIST!]

PropTool (extends PropBasedTool)
  ├── OnSelectedAssetChanged: UnityEvent<SerializableGuid>  [PUBLIC]
  ├── SelectedAssetId: SerializableGuid   [property]
  ├── LoadPropPrefab(RuntimePropInfo)     [PRIVATE]
  ├── UpdateSelectedAssetId(SerializableGuid)  [PUBLIC]
  └── PlaceProp(...)                      [PRIVATE]
```

### UI Refresh Flow:
```
CreatorManager.OnPropsRepopulated (Action delegate, NOT UnityEvent!)
  └── UIPropToolPanelView.OnLibraryRepopulated()
      └── UIRuntimePropInfoListModel.Synchronize(filter, ignoreList)
          └── PropLibrary.GetReferenceFilteredDefinitionList(filter)
              └── Returns props from loadedPropMap
```

---

## 5. EVENT FLOW

### CONFIRMED Events:
| Event | Type | Location | Purpose |
|-------|------|----------|---------|
| TerrainAndPropsLoaded | UnityEvent<Stage> | StageManager | Fires after terrain/props loaded |
| OnActiveStageChanged | UnityEvent<?> | StageManager | Fires when stage changes |
| OnLevelLoaded | UnityEvent<?> | StageManager | Fires when level loaded |
| OnPropsRepopulated | Action (delegate) | CreatorManager | Fires after props repopulated |
| OnRepopulate | Action | CreatorManager | Fires during repopulation |
| OnCreatorStarted | UnityEvent | CreatorManager | Fires when entering creator |
| OnCreatorEnded | UnityEvent | CreatorManager | Fires when leaving creator |
| OnSelectedAssetChanged | UnityEvent<SerializableGuid> | PropTool | Fires when prop selected |

---

## 6. DISCOVERIES LOG

### 2026-01-06 - RUNTIME DATA CAPTURED!

**AssetReference (loadedPropMap key) structure:**
```
AssetID: "d478af78-fbf5-4fe0-95eb-2ce923f50861"  ← GUID string
AssetVersion: "0.0.0"
AssetType: "prop"
UpdateParentVersion: false
```

**Prop (RuntimePropInfo.PropData) structure:**
```
AssetID: (matches AssetReference.AssetID)
Name: "Treasure"
baseTypeId: "f60832a2-63a9-466d-83e8-df441fbf37c9"
scriptAsset: AssetReference to script
prefabBundle: AssetReference to endless-prefab
iconFileInstanceId: 1230568
```

**PropTool.UpdateSelectedAssetId receives:**
- SerializableGuid: "b22e6e75-366c-4a73-bec1-b475576abce5"
- This GUID matches a Prop's AssetID

**To inject a custom prop:**
1. Create AssetReference with unique AssetID (GUID string)
2. Create Prop ScriptableObject with matching AssetID, Name, etc.
3. Create RuntimePropInfo with PropData, Icon, EndlessProp
4. Add to loadedPropMap dictionary
5. Add AssetID to injectedPropIds HashSet
6. Trigger OnPropsRepopulated to refresh UI

### 2026-01-06 - DLL Analysis
- MainScene contains everything, creator is a state not a scene
- PropLibrary is NOT a MonoBehaviour, just a plain C# object
- RuntimePropInfo is NESTED inside PropLibrary as `PropLibrary+RuntimePropInfo`
- InjectedProps is a CLASS not a struct (IsValueType=False in analysis was wrong label)
- PropLibrary has `injectedPropIds` HashSet - tracks injected props separately!
- UI uses `UIRuntimePropInfoListModel.Synchronize()` which calls `GetReferenceFilteredDefinitionList()`
- `OnPropsRepopulated` is an Action delegate, NOT a UnityEvent - use add_/remove_ methods

### 2026-01-06 - Runtime Test
- loadedPropMap was 0 because we checked before level was loaded
- basePropPrefab exists and is named "Endless Prop"
- DiagnosticRunner with separate GameObject works for Update()

---

## 7. OPEN QUESTIONS (Prioritized)

### HIGH PRIORITY:
1. Does StageManager.InjectProp call PropLibrary.InjectProp internally?
2. What exactly does PropLibrary.InjectProp do to loadedPropMap?
3. When does CreatorManager.OnPropsRepopulated fire?
4. How to properly subscribe to OnPropsRepopulated (it's Action, not UnityEvent)?

### MEDIUM PRIORITY:
5. What is the AssetReference key format in loadedPropMap?
6. How does GetReferenceFilteredDefinitionList filter props?
7. Does IsInjectedProp check injectedPropIds HashSet?

### LOW PRIORITY:
8. What are the ReferenceFilter enum values actually used for?
9. What triggers PropLibrary.Repopulate()?

---

## 8. TESTING LOG (2026-01-07)

### Prop Tool Window Issue - RESOLVED

**Problem:** Prop tool window would not open when CustomProps plugin was enabled.

**Root Cause:** `BuildPrefabPatch` was using Harmony to patch `EndlessProp.BuildPrefab` async method. Wrapping the Task return broke the prop tool UI flow.

**Solution:** Disabled BuildPrefabPatch entirely. Prop tool now opens.

### Isolation Testing Results

| Test | Prop Tool Opens? |
|------|------------------|
| Plugin disabled | YES |
| Plugin enabled, ALL injection disabled | YES |
| Plugin enabled, StageManager.InjectProp ONLY | YES |
| Plugin enabled, + InjectSingleProp | TESTING |
| Plugin enabled, + FixEndlessPropReferenceFilter | NOT YET |
| Plugin enabled, + PopulateReferenceFilterMap | NOT YET |

### Key Learning: Harmony + Async

**NEVER** use Harmony postfix to modify `Task` returns from async methods. The game's UI systems depend on the original Task behavior.

---

## 9. NEXT STEPS

1. ~~Update DiagnosticPlugin to dump data AFTER level load~~ DONE
2. ~~Subscribe to OnPropsRepopulated using reflection~~ DONE
3. Complete isolation testing to find which injection step breaks prop tool
4. Once culprit found, fix that specific step
5. Verify custom prop appears in UI after fix
