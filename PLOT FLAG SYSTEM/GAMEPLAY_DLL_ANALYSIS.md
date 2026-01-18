# GAMEPLAY.DLL - COMPLETE ANALYSIS
Generated: 01/06/2026 17:25:41

## StageManager
Namespace: Endless.Gameplay.LevelEditing.Level
Base: MonoBehaviourSingleton`1

### Fields
- private AudioLibrary activeAudioLibrary
- private SerializableGuid activeLevelGuid
- private PropLibrary activePropLibrary
- private RuntimePalette activeTerrainPalette
- private EndlessProp basePropPrefab
- private BaseTypeList baseTypeList
- private Dictionary`2 baseTypeRequirementLookup
- private List`1 baseTypeRequirements
- private Dictionary`2 cachedLevelStates
- private CollisionLibrary collisionLibrary
- private ComponentList componentList
- private Dictionary`2 destroyedObjectMapByStage
- private Sprite fallbackDisplayIcon
- private GameObject fallbackTerrain
- private List`1 injectedProps
- private SerializableGuid lastGameGuid
- private EndlessProp loadingPropObject
- private GameObject loadingTerrain
- private EndlessProp missingObjectPrefab
- private OfflineStage offlineStagePrefab
- public UnityEvent`1 OnActiveStageChanged
- public UnityEvent`1 OnLevelLoaded
- private Dictionary`2 persistantPropStateMap
- private Transform prefabSpawnTransform
- private Dictionary`2 propRequirementLookup
- private List`1 propRequirements
- private Dictionary`2 spawnedLevels
- private Dictionary`2 spawnedOfflineStages
- private Stage stageTemplate
- public UnityEvent`1 TerrainAndPropsLoaded

### Properties
- AudioLibrary ActiveAudioLibrary { get: True; set: False }
- SerializableGuid ActiveLevelGuid { get: True; set: False }
- PropLibrary ActivePropLibrary { get: True; set: False }
- Stage ActiveStage { get: True; set: False }
- RuntimePalette ActiveTerrainPalette { get: True; set: False }
- BaseTypeList BaseTypeList { get: True; set: False }
- IReadOnlyDictionary`2 BaseTypeRequirementLookup { get: True; set: False }
- ComponentList ComponentList { get: True; set: False }
- CancellationToken destroyCancellationToken { get: True; set: False }
- Boolean enabled { get: True; set: True }
- GameObject gameObject { get: True; set: False }
- HideFlags hideFlags { get: True; set: True }
- Boolean isActiveAndEnabled { get: True; set: False }
- String name { get: True; set: True }
- Boolean PreloadContent { get: True; set: False }
- IReadOnlyDictionary`2 PropRequirementLookup { get: True; set: False }
- String tag { get: True; set: True }
- Transform transform { get: True; set: False }
- Boolean useGUILayout { get: True; set: True }

### Methods
- protected Void Awake()
- private Void BuildBaseTypeRequirementsLookup()
- private Void BuildPropRequirementsLookup()
- public Void ClearDestroyedObjectMap()
- public Void ClearPersistantPropStates()
- public Task FetchAndSpawnPropPrefab(AssetReference assetReference)
- public Void FlushLoadedAndSpawnedStages(Boolean destroyStageObjects)
- public AudioLibrary get_ActiveAudioLibrary()
- public SerializableGuid get_ActiveLevelGuid()
- public PropLibrary get_ActivePropLibrary()
- public Stage get_ActiveStage()
- public RuntimePalette get_ActiveTerrainPalette()
- public BaseTypeList get_BaseTypeList()
- public IReadOnlyDictionary`2 get_BaseTypeRequirementLookup()
- public ComponentList get_ComponentList()
- public Boolean get_PreloadContent()
- public IReadOnlyDictionary`2 get_PropRequirementLookup()
- public BaseTypeDefinition GetBaseTypeDefinition(Type type)
- public ComponentDefinition GetComponentDefinition(Type type)
- public RuntimePropInfo GetMissingObjectInfo(Prop propData)
- public OfflineStage GetNewOfflineStage(AssetReference reference, String stageName)
- public Object GetPropState(SerializableGuid instanceId, String componentName)
- private Void HandleGameplayCleanup()
- private Void HandleLevelLoaded(SerializableGuid mapId)
- public Void InjectProp(Prop prop, GameObject testPrefab, Script testScript, Sprite icon)
- public Boolean IsPropDestroyed(SerializableGuid instanceId)
- public Void LeavingSession()
- public Boolean LevelIsLoaded(SerializableGuid levelId)
- public Task LoadLevel(LevelState levelState, Boolean loadLibraryPrefabs, CancellationToken cancelToken, Action`1 progressCallback)
- public Task LoadLibraryPrefabs(LevelState levelState, CancellationToken cancelToken, Action`1 progressCallback)
- private Task LoadPropLibraryReferences(LevelState levelState, CancellationToken cancelToken, Action`1 loadingProgressCallback)
- private Task LoadTerrainLibraryPrefabs(LevelState levelState, CancellationToken cancelToken, Action`1 loadingProgressCallback)
- public Void LoadTilesetByIndex(Int32 tilesetIndex)
- public Void PrepareForLevelChange(SerializableGuid levelToChangeTo)
- public Void PropDestroyed(SerializableGuid instanceId)
- public Void PropFailedToLoad(PropEntry propEntry)
- private Void PurgeCachedLevels()
- private Void PurgeOfflineStages()
- public Void RegisterStage(Stage stage)
- public Void RemoveOfflineStage(AssetReference reference)
- public Task RepopulateAudioLibrary(CancellationToken cancellationToken)
- public Void SavePropState(SerializableGuid instanceId, String componentName, Object newState)
- public Void SetJoinLevelId(SerializableGuid levelId)
- public Boolean SignaturesMatch(Int32[] signatureOne, Int32[] signatureTwo)
- public Void Start()
- public Boolean TryGetCachedLevel(SerializableGuid levelId, String versionNumber, LevelState& levelState)
- public Boolean TryGetComponentDefinition(Type type, ComponentDefinition& definition)
- public Boolean TryGetDataTypeSignature(List`1 eventInfos, String memberName, Int32[]& signature)
- public Boolean TryGetOfflineStage(AssetReference reference, OfflineStage& offlineStage)
- public Void UnloadAll()
- public Void UpdateStageVersion(AssetReference oldReference, AssetReference newReference)

---

## PropLibrary
Namespace: Endless.Gameplay.LevelEditing
Base: Object

### Fields
- private Dictionary`2 _referenceFilterMap
- private EndlessProp basePropPrefab
- private ReferenceFilter[] dynamicFilters
- private HashSet`1 inflightLoadRequests
- private List`1 injectedPropIds
- private Dictionary`2 loadedPropMap
- private EndlessProp loadingObjectProp
- private EndlessProp missingObjectPrefab
- private Transform prefabSpawnRoot

### Properties
- RuntimePropInfo Item { get: True; set: False }
- RuntimePropInfo Item { get: True; set: False }
- Dictionary`2 ReferenceFilterMap { get: True; set: False }

### Methods
- public Void AddMissingObjectRuntimePropInfo(RuntimePropInfo missingObjectInfo)
- public Task FetchAndSpawnPropPrefab(AssetReference assetReference, CancellationToken cancelToken, EndlessProp propPrefab, EndlessProp missingObjectPrefab, List`1 modifiedIds)
- public RuntimePropInfo get_Item(SerializableGuid assetId)
- public RuntimePropInfo get_Item(AssetReference assetReference)
- private Dictionary`2 get_ReferenceFilterMap()
- public RuntimePropInfo[] GetAllRuntimeProps()
- public AssetReference[] GetAssetReferences()
- public List`1 GetBaseTypeList(SerializableGuid[] propDataBaseTypeId)
- public List`1 GetBaseTypeList(String propDataBaseTypeId)
- public IReadOnlyList`1 GetReferenceFilteredDefinitionList(ReferenceFilter filter)
- public RuntimePropInfo GetRuntimePropInfo(SerializableGuid assetId)
- public RuntimePropInfo GetRuntimePropInfo(AssetReference assetReference)
- public Void InjectProp(Prop prop, GameObject testPrefab, Script testScript, Sprite icon, Transform prefabSpawnTransform, EndlessProp propPrefab)
- public Boolean IsInjectedProp(String propDataAssetID)
- public Boolean IsRepopulateRequired(Game newGame, Game oldGame)
- private Task LoadPropPrefab(EndlessProp propPrefab, EndlessProp missingObjectPrefab, CancellationToken cancelToken, Action`1 progressCallback, Prop prop, List`1 modifiedIds)
- public Task`1 LoadPropPrefabs(LevelState levelState, Transform prefabSpawnTransform, EndlessProp propPrefab, EndlessProp missingObjectPrefab, CancellationToken cancelToken, Action`2 progressCallback)
- private Void PopulateReferenceFilterMap()
- public Task PreloadData(CancellationToken cancelToken, List`1 previouslyMissingProps, Action`2 propLoadingUpdate)
- public Task`1 Repopulate(Stage activeStage, CancellationToken cancellationToken)
- private Boolean SanitizeMemberChanges(PropEntry propEntry)
- private Boolean SanitizeWireBundles(LevelState levelState, PropEntry propEntry)
- private Task SpawnPropPrefab(AssetReference assetReference, CancellationToken cancelToken, EndlessProp propPrefab, EndlessProp missingObjectPrefab, List`1 modifiedIds, Prop prop)
- public Boolean TryGetRuntimePropInfo(SerializableGuid assetId, RuntimePropInfo& metadata)
- public Boolean TryGetRuntimePropInfo(AssetReference assetReference, RuntimePropInfo& metadata)
- public Void UnloadAll()
- private Void UnloadProp(RuntimePropInfo info, Boolean dataUnload)
- public List`1 UnloadPropsNotInGameLibrary()

---

## RuntimePropInfo
### Endless.Gameplay.LevelEditing.PropLibrary+RuntimePropInfo
IsNested: True
IsClass: True
IsValueType: False

#### Fields
- public Prop PropData
- public Sprite Icon
- public EndlessProp EndlessProp
- private Boolean <IsLoading>k__BackingField
- private Boolean <IsMissingObject>k__BackingField

#### Properties
- Boolean IsLoading
- Boolean IsMissingObject

---

## InjectedProps
Namespace: Endless.Gameplay.LevelEditing.Level
IsValueType (struct): False

### Fields
- public Prop Prop
- public GameObject TestPrefab
- public Script TestScript
- public Sprite Icon

---

## EndlessProp
Namespace: Endless.Gameplay.Scripting
Base: EndlessBehaviour

### Fields (first 30)
- private EndlessScriptComponent scriptComponent
- private  worldObject
- private EndlessVisuals endlessVisuals
- public UnityEvent`1 OnInspectionStateChanged
- private Boolean isNetworked
- private ReferenceFilter <ReferenceFilter>k__BackingField
- private NavType <NavValue>k__BackingField
- private Prop <Prop>k__BackingField
- private  scriptInjectors
- private Dictionary`2 transformMap

### Properties (first 20)
- EndlessScriptComponent ScriptComponent
- ReferenceFilter ReferenceFilter
- NavType NavValue
- Prop Prop
- Boolean IsNetworked
-  ScriptInjectors
-  WorldObject
- Dictionary`2 TransformMap
- CancellationToken destroyCancellationToken
- Boolean useGUILayout
- Boolean enabled
- Boolean isActiveAndEnabled
- Transform transform
- GameObject gameObject
- String tag
- String name
- HideFlags hideFlags
