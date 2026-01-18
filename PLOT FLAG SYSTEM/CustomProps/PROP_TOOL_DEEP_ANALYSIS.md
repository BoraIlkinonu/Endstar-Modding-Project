# ENDSTAR PROP TOOL - DEEP CLASS ANALYSIS
Generated: 01/06/2026 14:38:38

---


## ToolManager (Endless.Creator.LevelEditing.Runtime)

**Full Name:** `Endless.Creator.LevelEditing.Runtime.ToolManager`

**Base Type:** `Endless.Shared.MonoBehaviourSingleton`1[[Endless.Creator.LevelEditing.Runtime.ToolManager, Creator, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]]`

### Fields (18)
```csharp
private Boolean active;
private EndlessTool activeTool;
private [SerializeField] Single alternateActionDeadZone;
private Boolean alternateInputIsDown;
private Boolean blockToolInput;
private [SerializeField] Boolean enableHotKeys;
private Boolean isMobile;
private Boolean mainInputIsDown;
private Vector3 mousePositionOnFirstAlternatePress;
public UnityEvent`1 OnActiveChange;
public UnityEvent`1 OnSetActiveToolToSameTool;
public UnityEvent`1 OnToolChange;
private PlayerInputActions playerInputActions;
private [SerializeField] Camera raycastCamera;
private EndlessSharedInputActions sharedInputActions;
private Dictionary`2 toolMap;
private [SerializeField] EndlessTool[] tools;
private [SerializeField] UIToolTypeColorDictionary toolTypeColorDictionary;
```

### Properties (13)
```csharp
EndlessTool ActiveTool { get;  }
Boolean CanUseHotKey { get;  }
EventSystem CurrentEventSystem { get;  }
CancellationToken destroyCancellationToken { get;  }
Boolean enabled { get; set; }
GameObject gameObject { get;  }
HideFlags hideFlags { get; set; }
Boolean IsActive { get;  }
Boolean isActiveAndEnabled { get;  }
String name { get; set; }
String tag { get; set; }
Transform transform { get;  }
Boolean useGUILayout { get; set; }
```

### Methods (21)
```csharp
public Void Activate()
private Void ActivateCopyTool(CallbackContext context)
private Void ActivateEmptyTool(CallbackContext context)
private Void ActivateEraseTool(CallbackContext context)
private Void ActivateInspectorTool(CallbackContext context)
private Void ActivateMoveTool(CallbackContext context)
private Void ActivatePaintingTool(CallbackContext context)
private Void ActivatePropBasedTool(CallbackContext context)
private Void ActivateWiringTool(CallbackContext context)
protected virtual override Void Awake()
public EndlessTool GetTool(ToolType type)
public Int32 GetToolHotKey(ToolType type)
public Void OnCreatorEnded()
private Void OnLeavingSession()
public T RequestToolInstance()
public Void SetActiveTool(EndlessTool tool)
public Void SetActiveTool(ToolType toolType)
private Void SetActiveTool_Internal(EndlessTool newActiveTool)
private Void SetActiveToolViaHotKey()
private Void Start()
private Void Update()
```

## ToolState Enum
```
  None
  Pressed
  Held
```

## ToolType Enum
```
  Empty
  Painting
  Prop
  Erase
  Wiring
  Inspector
  Copy
  Move
  GameEditor
  LevelEditor
  Screenshot
```


## PropTool (Endless.Creator.LevelEditing.Runtime)

**Full Name:** `Endless.Creator.LevelEditing.Runtime.PropTool`

**Base Type:** `Endless.Creator.LevelEditing.Runtime.PropBasedTool`

### Fields (7)
```csharp
internal __RpcExecStage __rpc_exec_stage;
internal UInt16 NetworkBehaviourIdCache;
internal List`1 NetworkVariableFields;
internal List`1 NetworkVariableIndexesToReset;
internal HashSet`1 NetworkVariableIndexesToResetSet;
public UnityEvent`1 OnSelectedAssetChanged;
private  scriptWindow;
```

### Properties (48)
```csharp
SerializableGuid ActiveAssetId { get;  }
LineCastHit ActiveLineCastResult { get;  }
Ray ActiveRay { get; set; }
Boolean AutoPlace3DCursor { get; set; }
CancellationToken destroyCancellationToken { get;  }
Boolean enabled { get; set; }
GameObject gameObject { get;  }
Boolean HasNetworkObject { get;  }
HideFlags hideFlags { get; set; }
Sprite Icon { get;  }
HashSet`1 InFlightLoads { get;  }
Boolean IsActive { get;  }
Boolean IsClient { get;  }
Boolean IsHost { get;  }
Boolean IsLoadingProp { get;  }
Boolean IsLocalPlayer { get;  }
Boolean IsMobile { get;  }
Boolean IsOwnedByServer { get; set; }
Boolean IsOwner { get; set; }
Boolean IsServer { get;  }
Boolean IsSpawned { get; set; }
Boolean isActiveAndEnabled { get;  }
SerializableGuid LinecastExclusionId { get; set; }
UInt64 m_TargetIdBeingSynchronized { get;  }
Single MaxSelectionDistance { get;  }
String name { get; set; }
UInt16 NetworkBehaviourId { get; set; }
NetworkManager NetworkManager { get;  }
NetworkObject NetworkObject { get;  }
UInt64 NetworkObjectId { get; set; }
UInt64 OwnerClientId { get; set; }
Boolean PerformsLineCast { get;  }
SerializableGuid PreviousAssetId { get;  }
SerializableGuid PreviousSelectedAssetId { get;  }
Vector3Int PropDimensions { get; set; }
Transform PropGhostTransform { get;  }
Boolean Rotating { get; set; }
RpcTarget RpcTarget { get;  }
SerializableGuid SelectedAssetId { get;  }
Boolean ServerIsHost { get;  }
String tag { get; set; }
Boolean ToolIsPressed { get;  }
ToolState ToolState { get; set; }
ToolType ToolType { get;  }
String ToolTypeName { get;  }
Transform transform { get;  }
UIToolPrompterManager UIToolPrompter { get;  }
Boolean useGUILayout { get; set; }
```

### Methods (17)
```csharp
internal virtual override String __getTypeName()
protected virtual override Void __initializeRpcs()
protected virtual override Void __initializeVariables()
private Void AttemptPlaceProp(Vector3 position, Vector3 eulerRotation, SerializableGuid assetId, ServerRpcParams serverRpcParams)
public Void AttemptPlaceProp_ServerRPC(Vector3 position, Vector3 eulerRotation, SerializableGuid assetId, ServerRpcParams serverRpcParams)
public Void EditScript(Boolean readOnly)
public virtual override Void HandleDeselected()
public virtual override Void HandleSelected()
private Void LoadPropPrefab(RuntimePropInfo runtimePropInfo)
private Void OnScriptWindowClosed(SerializableGuid propAssetId)
private Void PlaceProp(Vector3 position, Vector3 eulerRotation, SerializableGuid assetId, SerializableGuid instanceId, UInt64 networkObjectId)
public Void PlaceProp_ClientRPC(Vector3 position, Vector3 eulerRotation, SerializableGuid assetId, SerializableGuid instanceId, UInt64 networkObjectId)
public virtual override Void ToolPressed()
public virtual override Void ToolReleased()
public virtual override Void ToolSecondaryPressed()
public Void UpdateSelectedAssetId(SerializableGuid selectedAssetId)
public virtual override Void UpdateTool()
```

### Nested Types (5)
```
  <>c__DisplayClass14_0
  <AttemptPlaceProp>d__13
  <LoadPropPrefab>d__17
  <PlaceProp>d__15
  <UpdateSelectedAssetId>d__16
```


## UIPropToolPanelController (Endless.Creator.UI)

**Full Name:** `Endless.Creator.UI.UIPropToolPanelController`

**Base Type:** `Endless.Creator.UI.UIItemSelectionToolPanelController`2[[Endless.Creator.LevelEditing.Runtime.PropTool, Creator, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null],[Endless.Gameplay.LevelEditing.PropLibrary+RuntimePropInfo, Gameplay, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]]`

### Fields (2)
```csharp
protected PropTool Tool;
protected [SerializeField] UIBaseToolPanelView`1 View;
```

### Properties (12)
```csharp
CancellationToken destroyCancellationToken { get;  }
Boolean enabled { get; set; }
GameObject gameObject { get;  }
HideFlags hideFlags { get; set; }
UIItemSelectionToolPanelView`2 ItemSelectionToolPanelView { get;  }
Boolean isActiveAndEnabled { get;  }
String name { get; set; }
RectTransform RectTransform { get;  }
String tag { get; set; }
Transform transform { get;  }
Boolean useGUILayout { get; set; }
Boolean VerboseLogging { get; set; }
```

### Methods (1)
```csharp
public virtual override Void Deselect()
```


## UIPropToolPanelView (Endless.Creator.UI)

**Full Name:** `Endless.Creator.UI.UIPropToolPanelView`

**Base Type:** `Endless.Creator.UI.UIItemSelectionToolPanelView`2[[Endless.Creator.LevelEditing.Runtime.PropTool, Creator, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null],[Endless.Gameplay.LevelEditing.PropLibrary+RuntimePropInfo, Gameplay, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]]`

### Fields (7)
```csharp
protected Boolean IsMobile;
private Boolean inCreatorMode;
protected [SerializeField] Single minHeight;
private [SerializeField] UIRuntimePropInfoListModel runtimePropInfoListModel;
private  scriptWindow;
private SerializableGuid selectedAssetId;
protected PropTool Tool;
```

### Properties (18)
```csharp
Boolean CanViewDetail { get;  }
CancellationToken destroyCancellationToken { get;  }
Boolean DisplayOnToolChangeMatchToToolType { get;  }
Boolean enabled { get; set; }
GameObject gameObject { get;  }
Boolean HasSelectedItem { get;  }
HideFlags hideFlags { get; set; }
Boolean IsDisplaying { get;  }
Boolean isActiveAndEnabled { get;  }
Single ListSize { get;  }
UIBaseListView`1 ListView { get;  }
String name { get; set; }
RectTransform RectTransform { get;  }
Boolean SuperVerboseLogging { get; set; }
String tag { get; set; }
Transform transform { get;  }
Boolean useGUILayout { get; set; }
Boolean VerboseLogging { get; set; }
```

### Methods (8)
```csharp
private Void OnCreatorEnded()
private Void OnCreatorStarted()
private Void OnLibraryRepopulated()
private Void OnScriptWindowClosed()
private Void OnSelectedAssetChanged(SerializableGuid selectedAssetId)
protected virtual override Void OnToolChange(EndlessTool activeTool)
private OnWindowDisplayed(...)
protected virtual override Void Start()
```


## UIRuntimePropInfoListController (Endless.Creator.UI)

**Full Name:** `Endless.Creator.UI.UIRuntimePropInfoListController`

**Base Type:** `Endless.Shared.UI.UIBaseLocalFilterableListController`1[[Endless.Gameplay.LevelEditing.PropLibrary+RuntimePropInfo, Gameplay, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]]`

### Interfaces (1)
```
  Endless.Shared.Validation.IValidatable
```

### Fields (2)
```csharp
protected [SerializeField] Boolean IgnoreValidation;
protected UIBaseLocalFilterableListModel`1 LocalFilterableListModel;
```

### Properties (18)
```csharp
Boolean CanNavigate { get;  }
Boolean CaseSensitive { get;  }
CancellationToken destroyCancellationToken { get;  }
Boolean enabled { get; set; }
GameObject gameObject { get;  }
HideFlags hideFlags { get; set; }
Boolean isActiveAndEnabled { get;  }
UIListCellSwitch ListCellSwitch { get;  }
UIBaseListModel`1 Model { get;  }
String name { get; set; }
RectTransform RectTransform { get;  }
String StringFilter { get;  }
Boolean SuperVerboseLogging { get; set; }
String tag { get; set; }
Transform transform { get;  }
Boolean useGUILayout { get; set; }
Boolean VerboseLogging { get; set; }
UIBaseListView`1 View { get;  }
```

### Methods (1)
```csharp
protected virtual override Boolean IncludeInFilteredResults(RuntimePropInfo item)
```


## UIRuntimePropInfoListModel (Endless.Creator.UI)

**Full Name:** `Endless.Creator.UI.UIRuntimePropInfoListModel`

**Base Type:** `Endless.Shared.UI.UIBaseLocalFilterableListModel`1[[Endless.Gameplay.LevelEditing.PropLibrary+RuntimePropInfo, Gameplay, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]]`

### Interfaces (1)
```
  Endless.Shared.UI.IListModel
```

### Fields (2)
```csharp
private [SerializeField] Contexts <Context>k__BackingField;
protected List`1 List;
```

### Properties (46)
```csharp
Boolean AddButtonInserted { get; set; }
Boolean AddButtonIsInteractable { get;  }
UnityEvent`1 AddButtonIsInteractableChangedUnityEvent { get;  }
Boolean AddButtonNeedsInserting { get;  }
UnityEvent ClearedUnityEvent { get;  }
Contexts Context { get; set; }
Int32 Count { get;  }
Int32 DataCount { get;  }
Comparison`1 DefaultSort { get;  }
Comparison`1 DefaultSort { get;  }
CancellationToken destroyCancellationToken { get;  }
Boolean DisplayAddButton { get; set; }
Boolean enabled { get; set; }
IReadOnlyList`1 FilteredList { get;  }
GameObject gameObject { get;  }
HideFlags hideFlags { get; set; }
RuntimePropInfo Item { get;  }
UnityEvent`1 ItemAddedUnityEvent { get;  }
UnityEvent`2 ItemInsertedUnityEvent { get;  }
UnityEvent`2 ItemRemovedUnityEvent { get;  }
UnityEvent`1 ItemSelectedUnityEvent { get;  }
UnityEvent`2 ItemSetUnityEvent { get;  }
UnityEvent`2 ItemSwappedUnityEvent { get;  }
UnityEvent`1 ItemUnselectedUnityEvent { get;  }
Boolean isActiveAndEnabled { get;  }
Boolean MinimumSelectionCountTo1 { get;  }
UnityEvent ModelChangedUnityEvent { get;  }
String name { get; set; }
UnityEvent`1 RangeAddedUnityEvent { get;  }
UnityEvent`3 RangeRemovedUnityEvent { get;  }
IReadOnlyList`1 ReadOnlyList { get;  }
IReadOnlyList`1 ReadOnlySelectedList { get;  }
RectTransform RectTransform { get;  }
Boolean RestrictSelectionCountTo1 { get;  }
IReadOnlyList`1 SelectedTypedList { get;  }
UnityEvent`2 SelectionChangedUnityEvent { get;  }
UnityEvent`1 SortChangedUnityEvent { get;  }
Boolean SortOnChange { get;  }
SortOrders SortOrder { get;  }
UnityEvent`1 SortOrderChangedUnityEvent { get;  }
Boolean SuperVerboseLogging { get; set; }
String tag { get; set; }
Transform transform { get;  }
Boolean useGUILayout { get; set; }
Boolean UserCanRemove { get;  }
Boolean VerboseLogging { get; set; }
```

### Methods (1)
```csharp
public Void Synchronize(ReferenceFilter referenceFilter, IReadOnlyList`1 propsToIgnore)
```

### Nested Types (2)
```
  Contexts
    (enum)
      PropTool
      InventoryLibraryReference
  <>c
```


## UIRuntimePropInfoListView (Endless.Creator.UI)

**Full Name:** `Endless.Creator.UI.UIRuntimePropInfoListView`

**Base Type:** `Endless.Shared.UI.UIBaseListView`1[[Endless.Gameplay.LevelEditing.PropLibrary+RuntimePropInfo, Gameplay, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]]`

### Interfaces (2)
```
  Endless.Shared.UI.IListView
  Endless.Shared.Validation.IValidatable
```

### Fields (3)
```csharp
private UnityEvent`1 <OnCellSelected>k__BackingField;
protected  ActiveCellSource;
protected [SerializeField] Boolean IgnoreValidation;
```

### Properties (35)
```csharp
Boolean ActiveCellSourceIsRow { get;  }
 BeforeCellDespawnUnityEvent { get;  }
 CellDespawnedUnityEvent { get;  }
UnityEvent CellSourceChangedUnityEvent { get;  }
 CellSpawnedUnityEvent { get;  }
CellTypes CellType { get;  }
 CellVisibilityChangedUnityEvent { get;  }
Single CompleteHeight { get;  }
CancellationToken destroyCancellationToken { get;  }
Boolean enabled { get; set; }
GameObject gameObject { get;  }
HideFlags hideFlags { get; set; }
Boolean isActiveAndEnabled { get;  }
ListCellSizeTypes ListCellSizeType { get;  }
UnityEvent`1 ListCellSizeTypeChangedUnityEvent { get;  }
Single LookAheadAfter { get; set; }
Single LookAheadBefore { get; set; }
Boolean Loop { get; set; }
UIBaseListModel`1 Model { get;  }
String name { get; set; }
UnityEvent`1 OnCellSelected { get; set; }
RectTransform RectTransform { get;  }
Int32 RowItemCount { get;  }
Directions ScrollDirection { get;  }
UnityEvent`2 ScrollerScrolledUnityEvent { get;  }
Single ScrollPosition { get; set; }
UIScrollRect ScrollRect { get;  }
UnityEvent SnappedToEndUnityEvent { get;  }
UnityEvent SnappedToStartUnityEvent { get;  }
 SnappedUnityEvent { get;  }
Boolean SuperVerboseLogging { get; set; }
String tag { get; set; }
Transform transform { get;  }
Boolean useGUILayout { get; set; }
Boolean VerboseLogging { get; set; }
```

### Methods (0)
```csharp
```


## EndlessTool (Base Class)

**Full Name:** `Endless.Creator.LevelEditing.Runtime.EndlessTool`

**Base Type:** `Unity.Netcode.NetworkBehaviour`

### Fields (15)
```csharp
internal __RpcExecStage __rpc_exec_stage;
private LineCastHit <ActiveLineCastResult>k__BackingField;
private Ray <ActiveRay>k__BackingField;
private Boolean <AutoPlace3DCursor>k__BackingField;
private Boolean <PerformsLineCast>k__BackingField;
private ToolState <ToolState>k__BackingField;
private [SerializeField] Single fallbackVoidDistance;
private [SerializeField] Sprite icon;
private [SerializeField] Single lineCastScalar;
private [SerializeField] Single maxSelectionDistance;
internal UInt16 NetworkBehaviourIdCache;
internal List`1 NetworkVariableFields;
internal List`1 NetworkVariableIndexesToReset;
internal HashSet`1 NetworkVariableIndexesToResetSet;
private [SerializeField] Boolean useIntersectionFor3DCursor;
```

### Properties (37)
```csharp
LineCastHit ActiveLineCastResult { get; set; }
Ray ActiveRay { get; set; }
Boolean AutoPlace3DCursor { get; set; }
CancellationToken destroyCancellationToken { get;  }
Boolean enabled { get; set; }
GameObject gameObject { get;  }
Boolean HasNetworkObject { get;  }
HideFlags hideFlags { get; set; }
Sprite Icon { get;  }
Boolean IsActive { get;  }
Boolean IsClient { get;  }
Boolean IsHost { get;  }
Boolean IsLocalPlayer { get;  }
Boolean IsMobile { get;  }
Boolean IsOwnedByServer { get; set; }
Boolean IsOwner { get; set; }
Boolean IsServer { get;  }
Boolean IsSpawned { get; set; }
Boolean isActiveAndEnabled { get;  }
UInt64 m_TargetIdBeingSynchronized { get;  }
Single MaxSelectionDistance { get;  }
String name { get; set; }
UInt16 NetworkBehaviourId { get; set; }
NetworkManager NetworkManager { get;  }
NetworkObject NetworkObject { get;  }
UInt64 NetworkObjectId { get; set; }
UInt64 OwnerClientId { get; set; }
Boolean PerformsLineCast { get;  }
RpcTarget RpcTarget { get;  }
Boolean ServerIsHost { get;  }
String tag { get; set; }
ToolState ToolState { get; set; }
ToolType ToolType { get;  }
String ToolTypeName { get;  }
Transform transform { get;  }
UIToolPrompterManager UIToolPrompter { get;  }
Boolean useGUILayout { get; set; }
```

### Methods (19)
```csharp
internal virtual override String __getTypeName()
protected virtual override Void __initializeRpcs()
protected virtual override Void __initializeVariables()
public virtual Void CreatorExited()
protected virtual SerializableGuid GetExcludedAssetId()
public virtual Void HandleDeselected()
public virtual Void HandleSelected()
public Void PerformAndCacheLineCast()
protected Boolean PerformRaycast(RaycastHit& hit, Int32 layerMask)
private Boolean RayOriginIsOutOfBounds(Vector3 activeRayOrigin)
public virtual Void Reset()
public virtual Void SessionEnded()
public Void Set3DCursorUsesIntersection(Boolean val)
public virtual Void ToolHeld()
public virtual Void ToolPressed()
public virtual Void ToolReleased()
public virtual Void ToolSecondaryPressed()
protected Void Update3DCursorLocation(SerializableGuid excludedId)
public virtual Void UpdateTool()
```


## PropLibrary (Endless.Gameplay.LevelEditing)

**Full Name:** `Endless.Gameplay.LevelEditing.PropLibrary`

**Base Type:** `System.Object`

### Fields (9)
```csharp
private Dictionary`2 _referenceFilterMap;
private EndlessProp basePropPrefab;
private ReferenceFilter[] dynamicFilters;
private HashSet`1 inflightLoadRequests;
private List`1 injectedPropIds;
private Dictionary`2 loadedPropMap;
private EndlessProp loadingObjectProp;
private EndlessProp missingObjectPrefab;
private Transform prefabSpawnRoot;
```

### Properties (3)
```csharp
RuntimePropInfo Item { get;  }
RuntimePropInfo Item { get;  }
Dictionary`2 ReferenceFilterMap { get;  }
```

### Methods (25)
```csharp
public Void AddMissingObjectRuntimePropInfo(RuntimePropInfo missingObjectInfo)
public Task FetchAndSpawnPropPrefab(AssetReference assetReference, CancellationToken cancelToken, EndlessProp propPrefab, EndlessProp missingObjectPrefab, List`1 modifiedIds)
public RuntimePropInfo[] GetAllRuntimeProps()
public AssetReference[] GetAssetReferences()
public List`1 GetBaseTypeList(String propDataBaseTypeId)
public List`1 GetBaseTypeList(SerializableGuid[] propDataBaseTypeId)
public IReadOnlyList`1 GetReferenceFilteredDefinitionList(ReferenceFilter filter)
public RuntimePropInfo GetRuntimePropInfo(SerializableGuid assetId)
public RuntimePropInfo GetRuntimePropInfo(AssetReference assetReference)
public Void InjectProp(Prop prop, GameObject testPrefab, Script testScript, Sprite icon, Transform prefabSpawnTransform, EndlessProp propPrefab)
public Boolean IsInjectedProp(String propDataAssetID)
public Boolean IsRepopulateRequired(Game newGame, Game oldGame)
private Task LoadPropPrefab(EndlessProp propPrefab, EndlessProp missingObjectPrefab, CancellationToken cancelToken, Action`1 progressCallback, Prop prop, List`1 modifiedIds)
public Task`1 LoadPropPrefabs(LevelState levelState, Transform prefabSpawnTransform, EndlessProp propPrefab, EndlessProp missingObjectPrefab, CancellationToken cancelToken, Action`2 progressCallback)
private Void PopulateReferenceFilterMap()
public Task PreloadData(CancellationToken cancelToken, List`1 previouslyMissingProps, Action`2 propLoadingUpdate)
public Task`1 Repopulate(Stage activeStage, CancellationToken cancellationToken)
private Boolean SanitizeMemberChanges(PropEntry propEntry)
private Boolean SanitizeWireBundles(LevelState levelState, PropEntry propEntry)
private Task SpawnPropPrefab(AssetReference assetReference, CancellationToken cancelToken, EndlessProp propPrefab, EndlessProp missingObjectPrefab, List`1 modifiedIds, Prop prop)
public Boolean TryGetRuntimePropInfo(SerializableGuid assetId, RuntimePropInfo& metadata)
public Boolean TryGetRuntimePropInfo(AssetReference assetReference, RuntimePropInfo& metadata)
public Void UnloadAll()
private Void UnloadProp(RuntimePropInfo info, Boolean dataUnload)
public List`1 UnloadPropsNotInGameLibrary()
```

### Nested Types (21)
```
  RuntimePropInfo
  <>c
  <>c__DisplayClass15_0
  <>c__DisplayClass25_0
  <>c__DisplayClass28_0
  <>c__DisplayClass30_0
  <>c__DisplayClass30_1
  <>c__DisplayClass31_0
  <>c__DisplayClass33_0
  <>c__DisplayClass35_0
  <>c__DisplayClass36_0
  <>c__DisplayClass37_0
  <>c__DisplayClass38_0
  <>c__DisplayClass41_0
  <FetchAndSpawnPropPrefab>d__32
  <InjectProp>d__14
  <LoadPropPrefab>d__16
  <LoadPropPrefabs>d__15
  <PreloadData>d__30
  <Repopulate>d__36
  <SpawnPropPrefab>d__33
```


## StageManager (Endless.Gameplay.LevelEditing.Level)

**Full Name:** `Endless.Gameplay.LevelEditing.Level.StageManager`

**Base Type:** `Endless.Shared.MonoBehaviourSingleton`1[[Endless.Gameplay.LevelEditing.Level.StageManager, Gameplay, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]]`

### Fields (30)
```csharp
private AudioLibrary activeAudioLibrary;
private SerializableGuid activeLevelGuid;
private PropLibrary activePropLibrary;
private RuntimePalette activeTerrainPalette;
private [SerializeField] EndlessProp basePropPrefab;
private [SerializeField] BaseTypeList baseTypeList;
private Dictionary`2 baseTypeRequirementLookup;
private [SerializeField] List`1 baseTypeRequirements;
private Dictionary`2 cachedLevelStates;
private [SerializeField] CollisionLibrary collisionLibrary;
private [SerializeField] ComponentList componentList;
private Dictionary`2 destroyedObjectMapByStage;
private [SerializeField] Sprite fallbackDisplayIcon;
private [SerializeField] GameObject fallbackTerrain;
private List`1 injectedProps;
private SerializableGuid lastGameGuid;
private [SerializeField] EndlessProp loadingPropObject;
private [SerializeField] GameObject loadingTerrain;
private [SerializeField] EndlessProp missingObjectPrefab;
private [SerializeField] OfflineStage offlineStagePrefab;
public UnityEvent`1 OnActiveStageChanged;
public UnityEvent`1 OnLevelLoaded;
private Dictionary`2 persistantPropStateMap;
private [SerializeField] Transform prefabSpawnTransform;
private Dictionary`2 propRequirementLookup;
private [SerializeField] List`1 propRequirements;
private Dictionary`2 spawnedLevels;
private Dictionary`2 spawnedOfflineStages;
private [SerializeField] Stage stageTemplate;
public UnityEvent`1 TerrainAndPropsLoaded;
```

### Properties (19)
```csharp
AudioLibrary ActiveAudioLibrary { get;  }
SerializableGuid ActiveLevelGuid { get;  }
PropLibrary ActivePropLibrary { get;  }
Stage ActiveStage { get;  }
RuntimePalette ActiveTerrainPalette { get;  }
BaseTypeList BaseTypeList { get;  }
IReadOnlyDictionary`2 BaseTypeRequirementLookup { get;  }
ComponentList ComponentList { get;  }
CancellationToken destroyCancellationToken { get;  }
Boolean enabled { get; set; }
GameObject gameObject { get;  }
HideFlags hideFlags { get; set; }
Boolean isActiveAndEnabled { get;  }
String name { get; set; }
Boolean PreloadContent { get;  }
IReadOnlyDictionary`2 PropRequirementLookup { get;  }
String tag { get; set; }
Transform transform { get;  }
Boolean useGUILayout { get; set; }
```

### Methods (41)
```csharp
protected virtual override Void Awake()
private Void BuildBaseTypeRequirementsLookup()
private Void BuildPropRequirementsLookup()
public Void ClearDestroyedObjectMap()
public Void ClearPersistantPropStates()
public Task FetchAndSpawnPropPrefab(AssetReference assetReference)
public Void FlushLoadedAndSpawnedStages(Boolean destroyStageObjects)
public BaseTypeDefinition GetBaseTypeDefinition(Type type)
public ComponentDefinition GetComponentDefinition(Type type)
public RuntimePropInfo GetMissingObjectInfo(Prop propData)
public OfflineStage GetNewOfflineStage(AssetReference reference, String stageName)
public Object GetPropState(SerializableGuid instanceId, String componentName)
private Void HandleGameplayCleanup()
private Void HandleLevelLoaded(SerializableGuid mapId)
public Void InjectProp(Prop prop, GameObject testPrefab, Script testScript, Sprite icon)
public Boolean IsPropDestroyed(SerializableGuid instanceId)
public Void LeavingSession()
public Boolean LevelIsLoaded(SerializableGuid levelId)
public Task LoadLevel(LevelState levelState, Boolean loadLibraryPrefabs, CancellationToken cancelToken, Action`1 progressCallback)
public Task LoadLibraryPrefabs(LevelState levelState, CancellationToken cancelToken, Action`1 progressCallback)
private Task LoadPropLibraryReferences(LevelState levelState, CancellationToken cancelToken, Action`1 loadingProgressCallback)
private Task LoadTerrainLibraryPrefabs(LevelState levelState, CancellationToken cancelToken, Action`1 loadingProgressCallback)
public Void LoadTilesetByIndex(Int32 tilesetIndex)
public Void PrepareForLevelChange(SerializableGuid levelToChangeTo)
public Void PropDestroyed(SerializableGuid instanceId)
public Void PropFailedToLoad(PropEntry propEntry)
private Void PurgeCachedLevels()
private Void PurgeOfflineStages()
public Void RegisterStage(Stage stage)
public Void RemoveOfflineStage(AssetReference reference)
public Task RepopulateAudioLibrary(CancellationToken cancellationToken)
public Void SavePropState(SerializableGuid instanceId, String componentName, Object newState)
public Void SetJoinLevelId(SerializableGuid levelId)
public Boolean SignaturesMatch(Int32[] signatureOne, Int32[] signatureTwo)
public Void Start()
public Boolean TryGetCachedLevel(SerializableGuid levelId, String versionNumber, LevelState& levelState)
public Boolean TryGetComponentDefinition(Type type, ComponentDefinition& definition)
public Boolean TryGetDataTypeSignature(List`1 eventInfos, String memberName, Int32[]& signature)
public Boolean TryGetOfflineStage(AssetReference reference, OfflineStage& offlineStage)
public Void UnloadAll()
public Void UpdateStageVersion(AssetReference oldReference, AssetReference newReference)
```

### Nested Types (12)
```
  <>c
  <>c__DisplayClass74_0
  <>c__DisplayClass75_0
  <>c__DisplayClass88_0
  <>c__DisplayClass89_0
  <FetchAndSpawnPropPrefab>d__84
  <LoadLevel>d__72
  <LoadLibraryPrefabs>d__73
  <LoadPropLibraryReferences>d__74
  <LoadTerrainLibraryPrefabs>d__75
  <LoadTilesetByIndex>d__85
  <RepopulateAudioLibrary>d__70
```


## CreatorManager

**Full Name:** `Endless.Creator.CreatorManager`

**Base Type:** `Endless.Shared.NetworkBehaviourSingleton`1[[Endless.Creator.CreatorManager, Creator, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]]`

### Fields (41)
```csharp
internal __RpcExecStage __rpc_exec_stage;
private RpcReceiveState <RpcReceiveState>k__BackingField;
private List`1 activeLevelSendCoroutines;
private CancellationTokenSource assetVersionCancellationSource;
private CancellationTokenSource audioLibraryCancelTokenSource;
private CancellationTokenSource autoRefreshUserRoleCancelTokenSource;
private List`1 cachedRpcs;
private Boolean canSendClientLevelDetails;
private List`1 clientsAwaitingLevelDetails;
private LevelState cloudLevelState;
private static Int32 FRAGMENT_BYTES;
private static Single FRAGMENT_SEND_DELAY;
private Game gamePreRepopulate;
private CancellationTokenSource gameUpdatedCancelTokenSource;
private List`1 levelDataFragments;
private [SerializeField] LevelEditor levelEditor;
public UnityEvent LevelReverted;
private String loadedLevelData;
public UnityEvent`1 LocalClientRoleChanged;
internal UInt16 NetworkBehaviourIdCache;
internal List`1 NetworkVariableFields;
internal List`1 NetworkVariableIndexesToReset;
internal HashSet`1 NetworkVariableIndexesToResetSet;
public UnityEvent OnCreatorEnded;
public UnityEvent OnCreatorStarted;
public UnityEvent OnLeavingSession;
private Action OnPropsRepopulated;
private Action OnRepopulate;
private Action OnTerrainRepopulated;
private SerializableGuid originalSubscribedAssetId;
private Patch[] patches;
private SerializableGuid previousProjectId;
private CancellationTokenSource propLibraryCancelTokenSource;
private Coroutine refreshUserRolesCoroutine;
private SaveLoadManager saveLoadManager;
private CancellationTokenSource serverLoadLevelCancellationSource;
private SerializableGuid targetLevelId;
private CancellationTokenSource terrainRepopulationCancelTokenSource;
private List`1 userRolesForLevel;
private Boolean waitingForLevel;
private Boolean waitingForTargetLevelData;
```

### Properties (28)
```csharp
CancellationToken destroyCancellationToken { get;  }
Boolean enabled { get; set; }
GameObject gameObject { get;  }
Boolean HasNetworkObject { get;  }
HideFlags hideFlags { get; set; }
Boolean IsClient { get;  }
Boolean IsHost { get;  }
Boolean IsLocalPlayer { get;  }
Boolean IsOwnedByServer { get; set; }
Boolean IsOwner { get; set; }
Boolean IsServer { get;  }
Boolean IsSpawned { get; set; }
Boolean isActiveAndEnabled { get;  }
LevelEditor LevelEditor { get;  }
UInt64 m_TargetIdBeingSynchronized { get;  }
String name { get; set; }
UInt16 NetworkBehaviourId { get; set; }
NetworkManager NetworkManager { get;  }
NetworkObject NetworkObject { get;  }
UInt64 NetworkObjectId { get; set; }
UInt64 OwnerClientId { get; set; }
RpcReceiveState RpcReceiveState { get; set; }
RpcTarget RpcTarget { get;  }
SaveLoadManager SaveLoadManager { get;  }
Boolean ServerIsHost { get;  }
String tag { get; set; }
Transform transform { get;  }
Boolean useGUILayout { get; set; }
```

### Methods (48)
```csharp
internal virtual override String __getTypeName()
protected virtual override Void __initializeRpcs()
protected virtual override Void __initializeVariables()
private Task <OnWebsocketReconnected>b__53_0(Game newGame, Game oldGame)
private LevelState <RetrieveAndLoadServerLevel>b__66_0()
private Boolean <ValidateConnectedUsers>b__64_0(UInt64 clientId)
public Void AddCachedRpc(Action cachedRpc)
public Task ApplyCachedRpcs(CancellationToken cancelToken)
public Void AssetUpdated(AssetUpdatedMetaData assetUpdatedMetaData)
private Void AutoRefreshUserRoles(CancellationToken cancelToken)
private Void CancelTokens()
public Void ClearCachedRPCs()
public Void CreatorLoaded()
private Void EndingLevelFragments_ClientRpc(Boolean complete, ClientRpcParams rpcParams)
private Void EnterCachingState_ClientRpc(ClientRpcParams rpcParams)
public Void EnteringCreator()
private Void ForcePlayersToReload_ClientRpc()
public Void ForcePlayersToReload_ServerRpc()
public CancellationToken GetAssetVersionCancellationToken()
public Task HandleGameUpdated(Game newGame, Game oldGame)
private Void HandleNewPlayer(UInt64 clientId)
public Void LeavingCreator(Boolean forceSave)
public Void LeavingSession()
private Void LevelFragmentSend_ClientRpc(LevelDataFragment data, ClientRpcParams rpcParams)
public virtual override Void OnDestroy()
private Void OnLevelRightsChanged(IReadOnlyList`1 roles)
private Void OnMatchmakingStarted()
public virtual override Void OnNetworkSpawn()
private Void OnWebsocketReconnected()
public Task PerformInitialLevelLoad(SerializableGuid levelId, Action`1 progressCallback, CancellationToken cancelToken)
public Void ReceiveCachedLevelDetails_ClientRpc(SerializableGuid levelId, String version, ClientRpcParams rpcParams)
private Void RequestLevelDetails_ServerRpc(ServerRpcParams rpcParams)
public Task RetrieveAndLoadServerLevel(CancellationToken cancelToken, Action`1 progressCallback)
private Void RetrieveLevelStateOnClient(SerializableGuid levelId, String version)
private ValueTuple`2 Sanitize(LevelState levelState)
private Boolean SanitizeMemberChanges(LevelState levelState)
private Boolean SanitizeWireBundles(LevelState levelState)
private Void SendDetailsToClient(ClientRpcParams rpcParams)
private Void SendDetailsToClients(List`1 awaitingClients)
public IEnumerator SendLevelData(Byte[] bytes, ClientRpcParams rpcParams)
public Void SetRPCReceiveState(RpcReceiveState rpcReceiveState)
private Void Start()
private Void StartingLevelFragment_ClientRpc(ClientRpcParams rpcParams)
private Void UnHookOnMatchStartEventsAndHideScreenCover()
private Void Update()
public Task`1 UserCanEditLevel(Int32 userId)
public Task`1 UserCanEditLevel(UInt64 clientId)
private Void ValidateConnectedUsers()
```

### Nested Types (19)
```
  LevelDataFragment
  <<OnWebsocketReconnected>b__53_0>d
  <>c
  <>c__DisplayClass56_0
  <>c__DisplayClass62_0
  <>c__DisplayClass63_0
  <ApplyCachedRpcs>d__77
  <AssetUpdated>d__87
  <AutoRefreshUserRoles>d__65
  <HandleGameUpdated>d__86
  <OnWebsocketReconnected>d__53
  <PerformInitialLevelLoad>d__60
  <RetrieveAndLoadServerLevel>d__66
  <RetrieveLevelStateOnClient>d__76
  <SendDetailsToClient>d__71
  <SendLevelData>d__67
  <UserCanEditLevel>d__84
  <UserCanEditLevel>d__85
  <ValidateConnectedUsers>d__64
```

### Events (3)
```csharp
event Action OnRepopulate
event Action OnTerrainRepopulated
event Action OnPropsRepopulated
```


## UIToolController

**Full Name:** `Endless.Creator.UI.UIToolController`

**Base Type:** `Endless.Shared.UI.UIGameObject`

### Fields (4)
```csharp
private EndlessTool endlessTool;
private [SerializeField] UIButton setActiveToolButton;
private [SerializeField] ToolType toolType;
private [SerializeField] Boolean verboseLogging;
```

### Properties (10)
```csharp
CancellationToken destroyCancellationToken { get;  }
Boolean enabled { get; set; }
GameObject gameObject { get;  }
HideFlags hideFlags { get; set; }
Boolean isActiveAndEnabled { get;  }
String name { get; set; }
RectTransform RectTransform { get;  }
String tag { get; set; }
Transform transform { get;  }
Boolean useGUILayout { get; set; }
```

### Methods (2)
```csharp
private Void SetActiveTool()
private Void Start()
```

---

# ANALYSIS SUMMARY

## Key Classes Identified

| Class | Purpose |
|-------|---------|
| ToolManager | Manages all tools, handles tool switching |
| PropTool | The prop placement tool logic |
| UIPropToolPanelController | Controls the prop tool UI panel |
| UIPropToolPanelView | The visual component of prop tool panel |
| UIRuntimePropInfoListController | Controls the list of props in UI |
| UIRuntimePropInfoListModel | Data model for the prop list |
| PropLibrary | Backend storage for all props |
| StageManager | Manages stages/levels, owns PropLibrary |

## Potential Hook Points

Based on analysis, these are the key methods to investigate:

1. **ToolManager.SelectTool** - Called when switching tools
2. **PropTool activation** - When prop tool becomes active
3. **UIPropToolPanelController.Initialize/Show** - Panel initialization
4. **UIRuntimePropInfoListModel.Synchronize** - Syncs props with UI
5. **PropLibrary.GetAllRuntimeProps** - Returns all available props

