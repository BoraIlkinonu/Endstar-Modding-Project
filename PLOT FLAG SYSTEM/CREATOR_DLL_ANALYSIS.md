# CREATOR.DLL - COMPLETE ANALYSIS
Generated: 01/06/2026 17:26:36

## CreatorManager
Namespace: Endless.Creator
Base: NetworkBehaviourSingleton`1

### Fields
- protected __RpcExecStage __rpc_exec_stage
- private RpcReceiveState <RpcReceiveState>k__BackingField
- private List`1 activeLevelSendCoroutines
- private CancellationTokenSource assetVersionCancellationSource
- private CancellationTokenSource audioLibraryCancelTokenSource
- private CancellationTokenSource autoRefreshUserRoleCancelTokenSource
- private List`1 cachedRpcs
- private Boolean canSendClientLevelDetails
- private List`1 clientsAwaitingLevelDetails
- private LevelState cloudLevelState
- private static Int32 FRAGMENT_BYTES
- private static Single FRAGMENT_SEND_DELAY
- private Game gamePreRepopulate
- private CancellationTokenSource gameUpdatedCancelTokenSource
- private List`1 levelDataFragments
- private LevelEditor levelEditor
- public UnityEvent LevelReverted
- private String loadedLevelData
- public UnityEvent`1 LocalClientRoleChanged
- protected UInt16 NetworkBehaviourIdCache
- protected List`1 NetworkVariableFields
- protected List`1 NetworkVariableIndexesToReset
- protected HashSet`1 NetworkVariableIndexesToResetSet
- public UnityEvent OnCreatorEnded
- public UnityEvent OnCreatorStarted
- public UnityEvent OnLeavingSession
- private Action OnPropsRepopulated
- private Action OnRepopulate
- private Action OnTerrainRepopulated
- private SerializableGuid originalSubscribedAssetId
- private Patch[] patches
- private SerializableGuid previousProjectId
- private CancellationTokenSource propLibraryCancelTokenSource
- private Coroutine refreshUserRolesCoroutine
- private SaveLoadManager saveLoadManager
- private CancellationTokenSource serverLoadLevelCancellationSource
- private SerializableGuid targetLevelId
- private CancellationTokenSource terrainRepopulationCancelTokenSource
- private List`1 userRolesForLevel
- private Boolean waitingForLevel
- private Boolean waitingForTargetLevelData

### Properties
- CancellationToken destroyCancellationToken { get: True; set: False }
- Boolean enabled { get: True; set: True }
- GameObject gameObject { get: True; set: False }
- Boolean HasNetworkObject { get: True; set: False }
- HideFlags hideFlags { get: True; set: True }
- Boolean IsClient { get: True; set: False }
- Boolean IsHost { get: True; set: False }
- Boolean IsLocalPlayer { get: True; set: False }
- Boolean IsOwnedByServer { get: True; set: True }
- Boolean IsOwner { get: True; set: True }
- Boolean IsServer { get: True; set: False }
- Boolean IsSpawned { get: True; set: True }
- Boolean isActiveAndEnabled { get: True; set: False }
- LevelEditor LevelEditor { get: True; set: False }
- UInt64 m_TargetIdBeingSynchronized { get: True; set: False }
- String name { get: True; set: True }
- UInt16 NetworkBehaviourId { get: True; set: True }
- NetworkManager NetworkManager { get: True; set: False }
- NetworkObject NetworkObject { get: True; set: False }
- UInt64 NetworkObjectId { get: True; set: True }
- UInt64 OwnerClientId { get: True; set: True }
- RpcReceiveState RpcReceiveState { get: True; set: True }
- RpcTarget RpcTarget { get: True; set: False }
- SaveLoadManager SaveLoadManager { get: True; set: False }
- Boolean ServerIsHost { get: True; set: False }
- String tag { get: True; set: True }
- Transform transform { get: True; set: False }
- Boolean useGUILayout { get: True; set: True }

### Methods (first 50)
- public Void add_OnPropsRepopulated(Action value)
- public Void add_OnRepopulate(Action value)
- public Void add_OnTerrainRepopulated(Action value)
- public Void AddCachedRpc(Action cachedRpc)
- public Task ApplyCachedRpcs(CancellationToken cancelToken)
- public Void AssetUpdated(AssetUpdatedMetaData assetUpdatedMetaData)
- private Void AutoRefreshUserRoles(CancellationToken cancelToken)
- public Void ClearCachedRPCs()
- public Void CreatorLoaded()
- private Void EndingLevelFragments_ClientRpc(Boolean complete, ClientRpcParams rpcParams)
- private Void EnterCachingState_ClientRpc(ClientRpcParams rpcParams)
- public Void EnteringCreator()
- private Void ForcePlayersToReload_ClientRpc()
- public Void ForcePlayersToReload_ServerRpc()
- public LevelEditor get_LevelEditor()
- public RpcReceiveState get_RpcReceiveState()
- public SaveLoadManager get_SaveLoadManager()
- public CancellationToken GetAssetVersionCancellationToken()
- public Task HandleGameUpdated(Game newGame, Game oldGame)
- private Void HandleNewPlayer(UInt64 clientId)
- public Void LeavingCreator(Boolean forceSave)
- public Void LeavingSession()
- private Void LevelFragmentSend_ClientRpc(LevelDataFragment data, ClientRpcParams rpcParams)
- private Void OnLevelRightsChanged(IReadOnlyList`1 roles)
- private Void OnMatchmakingStarted()
- public Void OnNetworkSpawn()
- private Void OnWebsocketReconnected()
- public Task PerformInitialLevelLoad(SerializableGuid levelId, Action`1 progressCallback, CancellationToken cancelToken)
- public Void ReceiveCachedLevelDetails_ClientRpc(SerializableGuid levelId, String version, ClientRpcParams rpcParams)
- public Void remove_OnPropsRepopulated(Action value)
- public Void remove_OnRepopulate(Action value)
- public Void remove_OnTerrainRepopulated(Action value)
- private Void RequestLevelDetails_ServerRpc(ServerRpcParams rpcParams)
- public Task RetrieveAndLoadServerLevel(CancellationToken cancelToken, Action`1 progressCallback)
- private Void RetrieveLevelStateOnClient(SerializableGuid levelId, String version)
- private ValueTuple`2 Sanitize(LevelState levelState)
- private Boolean SanitizeMemberChanges(LevelState levelState)
- private Boolean SanitizeWireBundles(LevelState levelState)
- private Void SendDetailsToClient(ClientRpcParams rpcParams)
- private Void SendDetailsToClients(List`1 awaitingClients)
- public IEnumerator SendLevelData(Byte[] bytes, ClientRpcParams rpcParams)
- private Void set_RpcReceiveState(RpcReceiveState value)
- public Void SetRPCReceiveState(RpcReceiveState rpcReceiveState)
- private Void Start()
- private Void StartingLevelFragment_ClientRpc(ClientRpcParams rpcParams)
- private Void UnHookOnMatchStartEventsAndHideScreenCover()
- private Void Update()
- public Task`1 UserCanEditLevel(UInt64 clientId)
- public Task`1 UserCanEditLevel(Int32 userId)
- private Void ValidateConnectedUsers()

---

## PropTool
Namespace: Endless.Creator.LevelEditing.Runtime
Base: PropBasedTool

### Fields
- protected __RpcExecStage __rpc_exec_stage
- protected UInt16 NetworkBehaviourIdCache
- protected List`1 NetworkVariableFields
- protected List`1 NetworkVariableIndexesToReset
- protected HashSet`1 NetworkVariableIndexesToResetSet
- public UnityEvent`1 OnSelectedAssetChanged
- private  scriptWindow

### Methods
- protected String __getTypeName()
- protected Void __initializeRpcs()
- protected Void __initializeVariables()
- private Void AttemptPlaceProp(Vector3 position, Vector3 eulerRotation, SerializableGuid assetId, ServerRpcParams serverRpcParams)
- public Void AttemptPlaceProp_ServerRPC(Vector3 position, Vector3 eulerRotation, SerializableGuid assetId, ServerRpcParams serverRpcParams)
- public Void EditScript(Boolean readOnly)
- public SerializableGuid get_PreviousSelectedAssetId()
- public SerializableGuid get_SelectedAssetId()
- public ToolType get_ToolType()
- public Void HandleDeselected()
- public Void HandleSelected()
- private Void LoadPropPrefab(RuntimePropInfo runtimePropInfo)
- private Void OnScriptWindowClosed(SerializableGuid propAssetId)
- private Void PlaceProp(Vector3 position, Vector3 eulerRotation, SerializableGuid assetId, SerializableGuid instanceId, UInt64 networkObjectId)
- public Void PlaceProp_ClientRPC(Vector3 position, Vector3 eulerRotation, SerializableGuid assetId, SerializableGuid instanceId, UInt64 networkObjectId)
- public Void ToolPressed()
- public Void ToolReleased()
- public Void ToolSecondaryPressed()
- public Void UpdateSelectedAssetId(SerializableGuid selectedAssetId)
- public Void UpdateTool()

---

## UI Classes Related to Props

### UIPropCreationDataView
Namespace: Endless.Creator
Base: UIGameObject
#### Fields
- private TextMeshProUGUI displayNameText
- private Image iconImage
- private RawImage iconRawImage
- private Image subMenuIconImage
- private Boolean verboseLogging
#### Methods
- Void View(PropCreationData model)

### UIPropCreationDataListView
Namespace: Endless.Creator.UI
Base: UIBaseListView`1
#### Fields
- protected  ActiveCellSource
- protected Boolean IgnoreValidation
#### Methods

### UIPropEntryListView
Namespace: Endless.Creator.UI
Base: UIBaseListView`1
#### Fields
- protected  ActiveCellSource
- protected Boolean IgnoreValidation
#### Methods

### UIRuntimePropInfoListModel
Namespace: Endless.Creator.UI
Base: UIBaseLocalFilterableListModel`1
#### Fields
- private Contexts <Context>k__BackingField
- protected List`1 List
#### Methods
- Contexts get_Context()
- Void set_Context(Contexts value)
- Comparison`1 get_DefaultSort()
- Void Synchronize(ReferenceFilter referenceFilter, IReadOnlyList`1 propsToIgnore)

### UIRuntimePropInfoListView
Namespace: Endless.Creator.UI
Base: UIBaseListView`1
#### Fields
- private UnityEvent`1 <OnCellSelected>k__BackingField
- protected  ActiveCellSource
- protected Boolean IgnoreValidation
#### Methods
- UnityEvent`1 get_OnCellSelected()
- Void set_OnCellSelected(UnityEvent`1 value)

### IUIBasePropertyViewable
Namespace: Endless.Creator.UI
Base: 
#### Fields
#### Methods
- Void add_OnUserChangedModel(Action`1 value)
- Void remove_OnUserChangedModel(Action`1 value)

### UIRuntimePropInfoDetailView
Namespace: Endless.Creator.UI
Base: UIGameObject
#### Fields
- private Image iconImage
- private TextMeshProUGUI nameText
- private TextMeshProUGUI descriptionText
- private UIButton editScriptButton
- private TextMeshProUGUI editScriptButtonText
- private GameObject cantEditScriptTooltip
- private TextMeshProUGUI versionText
- private Boolean verboseLogging
- private Boolean isInjected
- private Boolean propIsOpenSource
- private Boolean scriptIsOpenSource
- private Boolean canEditProp
- private Boolean canEditScript
- private SerializableGuid propId
- private SerializableGuid scriptId
- private Boolean isSubscribedToRightsManager
- private Modes <Mode>k__BackingField
- private UnityEvent <OnLoadingStarted>k__BackingField
- private UnityEvent <OnLoadingEnded>k__BackingField
- private RuntimePropInfo <Model>k__BackingField
#### Methods
- Modes get_Mode()
- Void set_Mode(Modes value)
- UnityEvent get_OnLoadingStarted()
- UnityEvent get_OnLoadingEnded()
- RuntimePropInfo get_Model()
- Void set_Model(RuntimePropInfo value)
- Int32 get_LocalClientUserId()
- Void View(RuntimePropInfo model)
- Void Clear()
- Void OnPropRolesUpdated(IReadOnlyList`1 userRoles)
- Void OnScriptRolesUpdated(IReadOnlyList`1 userRoles)
- Void HandleEditScriptButtonInteractabilityAndText()

### UIPropToolPanelController
Namespace: Endless.Creator.UI
Base: UIItemSelectionToolPanelController`2
#### Fields
- protected UIBaseToolPanelView`1 View
- protected PropTool Tool
#### Methods
- Void Deselect()

### UIPropToolPanelView
Namespace: Endless.Creator.UI
Base: UIItemSelectionToolPanelView`2
#### Fields
- private UIRuntimePropInfoListModel runtimePropInfoListModel
- private SerializableGuid selectedAssetId
- private Boolean inCreatorMode
- private  scriptWindow
- protected Boolean IsMobile
- protected Single minHeight
- protected PropTool Tool
#### Methods
- Boolean get_HasSelectedItem()
- Boolean get_CanViewDetail()
- Void Start()
- Void OnToolChange(EndlessTool activeTool)
- Void OnLibraryRepopulated()
- Void OnSelectedAssetChanged(SerializableGuid selectedAssetId)
- Void OnCreatorStarted()
- Void OnCreatorEnded()
-  OnWindowDisplayed()
- Void OnScriptWindowClosed()

### UIWirePropertyModifierView
Namespace: Endless.Creator.UI
Base: UIGameObject
#### Fields
- private RectTransform container
- private Boolean verboseLogging
- private Boolean superVerboseLogging
- private  presenters
- private List`1 storedParameterValues
- private List`1 eventUnsubscribers
#### Methods
- String[] get_StoredParameterValues()
-  DisplayExistingWire()
-  DisplayDefaultParameters()
- Void Clean()
-  UpdateExistingWire()

---

## Types that reference PropLibrary
