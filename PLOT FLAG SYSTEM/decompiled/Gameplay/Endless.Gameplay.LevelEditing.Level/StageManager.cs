using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Endless.Assets;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Gameplay.LevelEditing.Tilesets;
using Endless.Gameplay.Scripting;
using Endless.Props.Assets;
using Endless.Props.Scripting;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Runtime.Gameplay.LevelEditing;
using Runtime.Gameplay.LevelEditing.Levels;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay.LevelEditing.Level;

public class StageManager : MonoBehaviourSingleton<StageManager>
{
	[SerializeField]
	private Stage stageTemplate;

	[SerializeField]
	private OfflineStage offlineStagePrefab;

	[SerializeField]
	private List<PropRequirement> propRequirements = new List<PropRequirement>();

	[SerializeField]
	private List<BaseTypeRequirement> baseTypeRequirements = new List<BaseTypeRequirement>();

	[SerializeField]
	private BaseTypeList baseTypeList;

	[SerializeField]
	private ComponentList componentList;

	[SerializeField]
	private Transform prefabSpawnTransform;

	[SerializeField]
	private EndlessProp basePropPrefab;

	[Header("Terrain")]
	[SerializeField]
	private CollisionLibrary collisionLibrary;

	[SerializeField]
	private GameObject fallbackTerrain;

	[SerializeField]
	private Sprite fallbackDisplayIcon;

	[SerializeField]
	private GameObject loadingTerrain;

	[Header("Props")]
	[SerializeField]
	private EndlessProp loadingPropObject;

	[SerializeField]
	private EndlessProp missingObjectPrefab;

	private readonly Dictionary<SerializableGuid, Stage> spawnedLevels = new Dictionary<SerializableGuid, Stage>();

	private SerializableGuid lastGameGuid = SerializableGuid.Empty;

	private readonly Dictionary<AssetReference, OfflineStage> spawnedOfflineStages = new Dictionary<AssetReference, OfflineStage>();

	private readonly Dictionary<AssetReference, LevelState> cachedLevelStates = new Dictionary<AssetReference, LevelState>();

	private List<InjectedProps> injectedProps = new List<InjectedProps>();

	private PropLibrary activePropLibrary;

	private RuntimePalette activeTerrainPalette;

	private AudioLibrary activeAudioLibrary;

	private Dictionary<SerializableGuid, Dictionary<SerializableGuid, Dictionary<string, object>>> persistantPropStateMap = new Dictionary<SerializableGuid, Dictionary<SerializableGuid, Dictionary<string, object>>>();

	private Dictionary<SerializableGuid, List<SerializableGuid>> destroyedObjectMapByStage = new Dictionary<SerializableGuid, List<SerializableGuid>>();

	private SerializableGuid activeLevelGuid = SerializableGuid.Empty;

	private Dictionary<SerializableGuid, PropRequirement> propRequirementLookup;

	private Dictionary<SerializableGuid, BaseTypeRequirement> baseTypeRequirementLookup;

	public UnityEvent<Stage> OnActiveStageChanged = new UnityEvent<Stage>();

	public UnityEvent<Stage> TerrainAndPropsLoaded;

	public UnityEvent<SerializableGuid> OnLevelLoaded = new UnityEvent<SerializableGuid>();

	public PropLibrary ActivePropLibrary => activePropLibrary;

	public RuntimePalette ActiveTerrainPalette => activeTerrainPalette;

	public AudioLibrary ActiveAudioLibrary => activeAudioLibrary;

	public IReadOnlyDictionary<SerializableGuid, PropRequirement> PropRequirementLookup
	{
		get
		{
			if (propRequirementLookup == null)
			{
				BuildPropRequirementsLookup();
			}
			return propRequirementLookup;
		}
	}

	public IReadOnlyDictionary<SerializableGuid, BaseTypeRequirement> BaseTypeRequirementLookup
	{
		get
		{
			if (baseTypeRequirementLookup == null)
			{
				BuildBaseTypeRequirementsLookup();
			}
			return baseTypeRequirementLookup;
		}
	}

	public Stage ActiveStage
	{
		get
		{
			if (spawnedLevels.ContainsKey(activeLevelGuid))
			{
				return spawnedLevels[activeLevelGuid];
			}
			return null;
		}
	}

	public BaseTypeList BaseTypeList => baseTypeList;

	public ComponentList ComponentList => componentList;

	public SerializableGuid ActiveLevelGuid => activeLevelGuid;

	public bool PreloadContent => false;

	protected override void Awake()
	{
		base.Awake();
		activeTerrainPalette = new RuntimePalette(collisionLibrary, fallbackTerrain, fallbackDisplayIcon, loadingTerrain);
		activePropLibrary = new PropLibrary(prefabSpawnTransform, loadingPropObject, basePropPrefab, missingObjectPrefab);
		activeAudioLibrary = new AudioLibrary(base.gameObject);
	}

	public void Start()
	{
		MonoBehaviourSingleton<GameplayManager>.Instance.OnGameplayCleanup.AddListener(HandleGameplayCleanup);
		base.transform.SetParent(null);
	}

	public bool TryGetOfflineStage(AssetReference reference, out OfflineStage offlineStage)
	{
		bool flag = spawnedOfflineStages.TryGetValue(reference, out offlineStage);
		if (!flag)
		{
			AssetReference[] array = spawnedOfflineStages.Keys.ToArray();
			foreach (AssetReference assetReference in array)
			{
				if (assetReference.AssetID == reference.AssetID)
				{
					UnityEngine.Object.Destroy(spawnedOfflineStages[assetReference].gameObject);
					spawnedOfflineStages.Remove(assetReference);
				}
			}
		}
		return flag;
	}

	public void RemoveOfflineStage(AssetReference reference)
	{
		if (spawnedOfflineStages.TryGetValue(reference, out var value))
		{
			UnityEngine.Object.Destroy(value.gameObject);
			spawnedOfflineStages.Remove(reference);
		}
	}

	public OfflineStage GetNewOfflineStage(AssetReference reference, string stageName)
	{
		OfflineStage offlineStage = UnityEngine.Object.Instantiate(offlineStagePrefab, base.transform);
		offlineStage.gameObject.name = "OfflineStage: " + base.name + " - " + reference.AssetID + " " + reference.AssetVersion;
		spawnedOfflineStages.Add(reference, offlineStage);
		return offlineStage;
	}

	public bool TryGetCachedLevel(SerializableGuid levelId, string versionNumber, out LevelState levelState)
	{
		AssetReference key = new AssetReference
		{
			AssetID = levelId,
			AssetVersion = versionNumber,
			AssetType = "level"
		};
		return cachedLevelStates.TryGetValue(key, out levelState);
	}

	public void UpdateStageVersion(AssetReference oldReference, AssetReference newReference)
	{
		OfflineStage value = spawnedOfflineStages[oldReference];
		spawnedOfflineStages.Remove(oldReference);
		spawnedOfflineStages[newReference] = value;
		LevelState value2 = cachedLevelStates[oldReference];
		cachedLevelStates[newReference] = value2;
		cachedLevelStates.Remove(oldReference);
	}

	private void PurgeOfflineStages()
	{
		AssetReference[] array = spawnedOfflineStages.Keys.ToArray();
		foreach (AssetReference key in array)
		{
			UnityEngine.Object.Destroy(spawnedOfflineStages[key].gameObject);
			spawnedOfflineStages.Remove(key);
		}
		spawnedOfflineStages.Clear();
	}

	private void PurgeCachedLevels()
	{
		cachedLevelStates.Clear();
	}

	public void SetJoinLevelId(SerializableGuid levelId)
	{
		activeLevelGuid = levelId;
	}

	private void HandleGameplayCleanup()
	{
		ClearPersistantPropStates();
		ClearDestroyedObjectMap();
	}

	public object GetPropState(SerializableGuid instanceId, string componentName)
	{
		if (!persistantPropStateMap.ContainsKey(activeLevelGuid))
		{
			return null;
		}
		if (!persistantPropStateMap[activeLevelGuid].ContainsKey(instanceId))
		{
			return null;
		}
		if (!persistantPropStateMap[activeLevelGuid][instanceId].ContainsKey(componentName))
		{
			return null;
		}
		return persistantPropStateMap[activeLevelGuid][instanceId][componentName];
	}

	public void SavePropState(SerializableGuid instanceId, string componentName, object newState)
	{
		if (!persistantPropStateMap.ContainsKey(activeLevelGuid))
		{
			persistantPropStateMap.Add(activeLevelGuid, new Dictionary<SerializableGuid, Dictionary<string, object>>());
		}
		if (!persistantPropStateMap[activeLevelGuid].ContainsKey(instanceId))
		{
			persistantPropStateMap[activeLevelGuid].Add(instanceId, new Dictionary<string, object>());
		}
		if (!persistantPropStateMap[activeLevelGuid][instanceId].ContainsKey(componentName))
		{
			persistantPropStateMap[activeLevelGuid][instanceId].Add(componentName, newState);
		}
		else
		{
			persistantPropStateMap[activeLevelGuid][instanceId][componentName] = newState;
		}
	}

	public void ClearPersistantPropStates()
	{
		persistantPropStateMap.Clear();
	}

	public void ClearDestroyedObjectMap()
	{
		destroyedObjectMapByStage.Clear();
	}

	public void PropDestroyed(SerializableGuid instanceId)
	{
		SerializableGuid mapId = ActiveStage.MapId;
		if (destroyedObjectMapByStage.ContainsKey(mapId))
		{
			destroyedObjectMapByStage[mapId].Add(instanceId);
		}
		else
		{
			destroyedObjectMapByStage.Add(mapId, new List<SerializableGuid> { instanceId });
		}
		ActiveStage.PropDestroyed(instanceId);
	}

	public bool IsPropDestroyed(SerializableGuid instanceId)
	{
		SerializableGuid mapId = ActiveStage.MapId;
		if (!destroyedObjectMapByStage.ContainsKey(mapId))
		{
			return false;
		}
		return destroyedObjectMapByStage[mapId].Contains(instanceId);
	}

	private void BuildPropRequirementsLookup()
	{
		propRequirementLookup = new Dictionary<SerializableGuid, PropRequirement>();
		foreach (PropRequirement propRequirement in propRequirements)
		{
			foreach (SerializableGuid guid in propRequirement.Guids)
			{
				propRequirementLookup.Add(guid, propRequirement);
			}
		}
	}

	private void BuildBaseTypeRequirementsLookup()
	{
		baseTypeRequirementLookup = new Dictionary<SerializableGuid, BaseTypeRequirement>();
		foreach (BaseTypeRequirement baseTypeRequirement in baseTypeRequirements)
		{
			foreach (SerializableGuid guid in baseTypeRequirement.Guids)
			{
				baseTypeRequirementLookup.Add(guid, baseTypeRequirement);
			}
		}
	}

	public void InjectProp(Prop prop, GameObject testPrefab, Script testScript, Sprite icon)
	{
		injectedProps.Add(new InjectedProps
		{
			Prop = prop,
			TestPrefab = testPrefab,
			TestScript = testScript,
			Icon = icon
		});
	}

	public async Task RepopulateAudioLibrary(CancellationToken cancellationToken)
	{
		activeAudioLibrary.UnloadAssetsNotInGameLibrary();
		await activeAudioLibrary.PreloadData(cancellationToken);
	}

	public void PrepareForLevelChange(SerializableGuid levelToChangeTo)
	{
		if (!(levelToChangeTo == activeLevelGuid) && !(levelToChangeTo == SerializableGuid.Empty))
		{
			if ((bool)MonoBehaviourSingleton<StageManager>.Instance.ActiveStage && MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.MapId != levelToChangeTo)
			{
				MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.CleanupProps();
				MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.gameObject.SetActive(value: false);
			}
			activeLevelGuid = levelToChangeTo;
			if ((bool)MonoBehaviourSingleton<StageManager>.Instance.ActiveStage)
			{
				MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.gameObject.SetActive(value: true);
			}
		}
	}

	public async Task LoadLevel(LevelState levelState, bool loadLibraryPrefabs, CancellationToken cancelToken, Action<string> progressCallback = null)
	{
		MonoBehaviourSingleton<LoadTimeTester>.Instance.StartTracking("StageManager");
		SerializableGuid serializableGuid = MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.AssetID;
		if (lastGameGuid != serializableGuid)
		{
			lastGameGuid = serializableGuid;
			PurgeOfflineStages();
			PurgeCachedLevels();
		}
		AssetReference key = levelState.ToAssetReference();
		if (!cachedLevelStates.TryAdd(key, levelState))
		{
			cachedLevelStates[key] = levelState;
		}
		Debug.Log("StageManager.LoadLevel - Loading Level: " + levelState.AssetID);
		if (loadLibraryPrefabs)
		{
			await LoadLibraryPrefabs(levelState, cancelToken, progressCallback);
			MonoBehaviourSingleton<LoadTimeTester>.Instance.LogTimeDelta("LoadLibraryPrefabs", "StageManager");
			if (cancelToken.IsCancellationRequested)
			{
				return;
			}
		}
		if (NetworkManager.Singleton.IsServer)
		{
			if (!spawnedLevels.TryGetValue(levelState.AssetID, out var value) || value == null)
			{
				Stage stage = UnityEngine.Object.Instantiate(stageTemplate);
				stage.SetMapId(levelState.AssetID);
				stage.RuntimePalette = activeTerrainPalette;
				stage.GetComponent<NetworkObject>().Spawn();
				RegisterStage(stage);
				activeLevelGuid = levelState.AssetID;
				await stage.LoadLevelIfNecessary(levelState, cancelToken, progressCallback);
				Debug.Log("Adding MapId: " + levelState.AssetID + ", to Loaded Levels.");
				foreach (ulong connectedClientsId in NetworkManager.Singleton.ConnectedClientsIds)
				{
					if (connectedClientsId != 0L)
					{
						stage.HandleNewPlayer(connectedClientsId);
					}
				}
				HandleLevelLoaded(levelState.AssetID);
			}
			else
			{
				HandleLevelLoaded(levelState.AssetID);
			}
			if (spawnedLevels.Keys.Count == 1)
			{
				activeLevelGuid = levelState.AssetID;
				OnActiveStageChanged.Invoke(ActiveStage);
			}
			return;
		}
		Stage value2;
		while (!spawnedLevels.TryGetValue(levelState.AssetID, out value2))
		{
			await Task.Yield();
			if (cancelToken.IsCancellationRequested)
			{
				return;
			}
		}
		Debug.Log("Client -- StageManager.LoadLevel " + levelState.AssetID);
		Stage stage2 = value2;
		if (stage2.RuntimePalette == null)
		{
			stage2.RuntimePalette = activeTerrainPalette;
		}
		if (!value2.IsLoading)
		{
			await spawnedLevels[levelState.AssetID].LoadLevelIfNecessary(levelState, cancelToken, progressCallback);
			Debug.Log("Adding MapId: " + levelState.AssetID + ", to Loaded Levels.");
			if (!cancelToken.IsCancellationRequested)
			{
				OnActiveStageChanged.Invoke(ActiveStage);
				HandleLevelLoaded(levelState.AssetID);
			}
		}
	}

	public async Task LoadLibraryPrefabs(LevelState levelState, CancellationToken cancelToken, Action<string> progressCallback)
	{
		MonoBehaviourSingleton<LoadTimeTester>.Instance.StartTracking("StageManager - LoadLibraryPrefabs");
		progressCallback("Loading terrain for level...");
		await LoadTerrainLibraryPrefabs(levelState, cancelToken, progressCallback);
		if (!cancelToken.IsCancellationRequested)
		{
			MonoBehaviourSingleton<LoadTimeTester>.Instance.LogTimeDelta("Load Terrain", "StageManager - LoadLibraryPrefabs");
			progressCallback("Loading props for level...");
			await LoadPropLibraryReferences(levelState, cancelToken, progressCallback);
			if (!cancelToken.IsCancellationRequested)
			{
				MonoBehaviourSingleton<LoadTimeTester>.Instance.LogTimeDelta("Load props", "StageManager - LoadLibraryPrefabs");
				await Resources.UnloadUnusedAssets();
				MonoBehaviourSingleton<LoadTimeTester>.Instance.StopTracking("StageManager - LoadLibraryPrefabs");
			}
		}
	}

	private async Task LoadPropLibraryReferences(LevelState levelState, CancellationToken cancelToken, Action<string> loadingProgressCallback)
	{
		foreach (InjectedProps injectedProp in injectedProps)
		{
			activePropLibrary.InjectProp(injectedProp.Prop, injectedProp.TestPrefab, injectedProp.TestScript, injectedProp.Icon, prefabSpawnTransform, basePropPrefab);
		}
		await activePropLibrary.LoadPropPrefabs(levelState, prefabSpawnTransform, basePropPrefab, missingObjectPrefab, cancelToken, ProgressCallback);
		void ProgressCallback(int loaded, int total)
		{
			loadingProgressCallback?.Invoke($"Loading Prop: {loaded:N0}/{total:N0}");
		}
	}

	private async Task LoadTerrainLibraryPrefabs(LevelState levelState, CancellationToken cancelToken, Action<string> loadingProgressCallback)
	{
		HashSet<int> usedTileSetIds = levelState.GetUsedTileSetIds(MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.GameLibrary);
		IReadOnlyList<TerrainUsage> terrainUsagesInLevel = MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.GameLibrary.GetTerrainUsagesInLevel(usedTileSetIds);
		await activeTerrainPalette.Populate(terrainUsagesInLevel, MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.GameLibrary, cancelToken, ProgressCallback);
		void ProgressCallback(int loaded, int total)
		{
			loadingProgressCallback?.Invoke($"Loading Terrain: {loaded:N0}/{total:N0}");
		}
	}

	public bool LevelIsLoaded(SerializableGuid levelId)
	{
		if (!spawnedLevels.TryGetValue(levelId, out var value))
		{
			return false;
		}
		return value.TerrainLoaded;
	}

	private void HandleLevelLoaded(SerializableGuid mapId)
	{
		MonoBehaviourSingleton<LoadTimeTester>.Instance.StopTracking("StageManager");
		OnLevelLoaded.Invoke(mapId);
	}

	public void RegisterStage(Stage stage)
	{
		spawnedLevels[stage.MapId] = stage;
	}

	public void FlushLoadedAndSpawnedStages(bool destroyStageObjects)
	{
		if (destroyStageObjects)
		{
			foreach (Stage value in spawnedLevels.Values)
			{
				UnityEngine.Object.Destroy(value.gameObject);
			}
		}
		foreach (OfflineStage value2 in spawnedOfflineStages.Values)
		{
			value2.gameObject.SetActive(value: false);
		}
		spawnedLevels.Clear();
	}

	public void LeavingSession()
	{
		if ((bool)ActiveStage)
		{
			ActiveStage.CleanupProps();
		}
		spawnedLevels.Clear();
		activeLevelGuid = SerializableGuid.Empty;
	}

	public void PropFailedToLoad(PropEntry propEntry)
	{
		Prop prop = new Prop
		{
			Name = "Missing Object",
			AssetID = propEntry.AssetId,
			AssetVersion = "0.0.0"
		};
		prop.AddLocationOffset(new PropLocationOffset
		{
			Offset = Vector3Int.zero
		});
		if (!activePropLibrary.TryGetRuntimePropInfo(prop.ToAssetReference(), out var metadata))
		{
			metadata = new PropLibrary.RuntimePropInfo
			{
				EndlessProp = UnityEngine.Object.Instantiate(missingObjectPrefab, prefabSpawnTransform),
				Icon = MonoBehaviourSingleton<DefaultContentManager>.Instance.MissingPropDisplayIcon,
				PropData = prop,
				IsMissingObject = true,
				IsLoading = false
			};
			metadata.EndlessProp.gameObject.name = prop.Name + " (Missing)";
			activePropLibrary.AddMissingObjectRuntimePropInfo(metadata);
		}
		Debug.Log(string.Format("{0} spawning missing prop for {1}", "PropFailedToLoad", propEntry.AssetId));
		ActiveStage.ReplacePropWithMissingObject(propEntry.InstanceId, metadata, metadata);
	}

	public BaseTypeDefinition GetBaseTypeDefinition(Type type)
	{
		if (!baseTypeList.TryGetDefinition(type, out var componentDefinition))
		{
			return null;
		}
		return componentDefinition;
	}

	public ComponentDefinition GetComponentDefinition(Type type)
	{
		if (!componentList.TryGetDefinition(type, out var componentDefinition))
		{
			return null;
		}
		return componentDefinition;
	}

	public async Task FetchAndSpawnPropPrefab(AssetReference assetReference)
	{
		await activePropLibrary.FetchAndSpawnPropPrefab(assetReference, CancellationToken.None, basePropPrefab, missingObjectPrefab);
	}

	public async void LoadTilesetByIndex(int tilesetIndex)
	{
		if (activeTerrainPalette.IsTilesetIndexInFlight(tilesetIndex))
		{
			Debug.Log($"Request for Index ({tilesetIndex}) is already in flight. Aborting");
			return;
		}
		GameLibrary gameLibrary = MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.GameLibrary;
		HashSet<int> hashSet = new HashSet<int>();
		TerrainUsage terrainUsage = gameLibrary.TerrainEntries[tilesetIndex];
		do
		{
			int num = terrainUsage.RedirectIndex;
			if (num > gameLibrary.TerrainEntries.Count)
			{
				num = 0;
				Debug.LogException(new Exception("Had improper index in terrain redirects"));
			}
			else if (hashSet.Contains(num))
			{
				Debug.LogException(new Exception($"Redirect Indexes looped. Can't validate terrain usage at index: {tilesetIndex}"));
				return;
			}
			hashSet.Add(num);
			terrainUsage = gameLibrary.TerrainEntries[num];
		}
		while (!terrainUsage.IsActive);
		await ActiveTerrainPalette.LoadTerrain(terrainUsage, tilesetIndex, CancellationToken.None);
		await Resources.UnloadUnusedAssets();
		StartCoroutine(ActiveStage.RespawnTerrainCellsWithTilesetIndex(tilesetIndex, null));
	}

	public PropLibrary.RuntimePropInfo GetMissingObjectInfo(Prop propData)
	{
		Prop prop = new Prop
		{
			Name = propData.Name + " (Missing)",
			AssetType = "prop",
			AssetID = propData.AssetID,
			AssetVersion = propData.AssetVersion,
			InternalVersion = Prop.INTERNAL_VERSION.ToString()
		};
		prop.AddLocationOffset(new PropLocationOffset
		{
			Offset = Vector3Int.zero
		});
		PropLibrary.RuntimePropInfo runtimePropInfo = new PropLibrary.RuntimePropInfo();
		runtimePropInfo.EndlessProp = UnityEngine.Object.Instantiate(missingObjectPrefab, prefabSpawnTransform);
		runtimePropInfo.Icon = MonoBehaviourSingleton<DefaultContentManager>.Instance.MissingPropDisplayIcon;
		runtimePropInfo.PropData = prop;
		runtimePropInfo.IsMissingObject = true;
		runtimePropInfo.IsLoading = false;
		runtimePropInfo.EndlessProp.gameObject.name = propData.Name + " (Missing)";
		return runtimePropInfo;
	}

	public bool TryGetComponentDefinition(Type type, out ComponentDefinition definition)
	{
		definition = null;
		if (typeof(IBaseType).IsAssignableFrom(type))
		{
			if (!MonoBehaviourSingleton<StageManager>.Instance.BaseTypeList.TryGetDefinition(type, out var componentDefinition))
			{
				return false;
			}
			definition = componentDefinition;
			return true;
		}
		if (typeof(IComponentBase).IsAssignableFrom(type) && MonoBehaviourSingleton<StageManager>.Instance.ComponentList.TryGetDefinition(type, out definition))
		{
			return true;
		}
		return false;
	}

	public bool TryGetDataTypeSignature(List<EndlessEventInfo> eventInfos, string memberName, out int[] signature)
	{
		EndlessEventInfo endlessEventInfo = eventInfos.FirstOrDefault((EndlessEventInfo eventInfo) => eventInfo.MemberName == memberName);
		if (endlessEventInfo == null)
		{
			signature = Array.Empty<int>();
			return false;
		}
		signature = endlessEventInfo.ParamList.Select((EndlessParameterInfo param) => param.DataType).ToArray();
		return true;
	}

	public bool SignaturesMatch(int[] signatureOne, int[] signatureTwo)
	{
		if (signatureOne.Length != signatureTwo.Length)
		{
			return false;
		}
		return !signatureOne.Where((int t, int index) => t != signatureTwo[index]).Any();
	}

	public void UnloadAll()
	{
		activeTerrainPalette.UnloadAll();
		activePropLibrary.UnloadAll();
		activeAudioLibrary.UnloadAll();
	}
}
