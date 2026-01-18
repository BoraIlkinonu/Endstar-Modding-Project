using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Endless.Assets;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Gameplay.LevelEditing.Tilesets;
using Endless.Gameplay.LevelEditing.Wires;
using Endless.Gameplay.Scripting;
using Endless.Gameplay.Serialization;
using Endless.Props.Assets;
using Endless.Props.Scripting;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.TerrainCosmetics;
using HackAnythingAnywhere.Core;
using JetBrains.Annotations;
using Unity.Netcode;
using Unity.Profiling;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Level;

public class Stage : NetworkBehaviour, ISerializationCallbackReceiver
{
	private class PendingNetworkIdInfo
	{
		public readonly ulong NetworkId;

		public readonly SerializableGuid InstanceId;

		public readonly bool UpdateLevelState;

		public PendingNetworkIdInfo(ulong networkId, SerializableGuid instanceId, bool updateLevelState = true)
		{
			NetworkId = networkId;
			InstanceId = instanceId;
			UpdateLevelState = updateLevelState;
		}
	}

	private class ClaimCellRequest
	{
		public PropLibrary.RuntimePropInfo Definition { get; }

		public SerializableGuid InstanceId { get; }

		public ulong NetworkObjectId { get; }

		public ClaimCellRequest(PropLibrary.RuntimePropInfo definition, SerializableGuid instanceId, ulong networkObjectId)
		{
			Definition = definition;
			InstanceId = instanceId;
			NetworkObjectId = networkObjectId;
		}
	}

	private class PropRegisteredRequest
	{
		public SerializableGuid InstanceId { get; }

		public PropRegisteredRequest(SerializableGuid instanceId)
		{
			InstanceId = instanceId;
		}
	}

	private const int STAGE_FALLOFF_HEIGHT_BUFFER = 5;

	public const int LOAD_LIMIT_MS = 64;

	public const int PLAY_LIMIT_MS = 4;

	private static UnityEngine.Vector3 topFrontLeft = new UnityEngine.Vector3(-0.5f, 0.5f, 0.5f);

	private static UnityEngine.Vector3 topFrontRight = new UnityEngine.Vector3(0.5f, 0.5f, 0.5f);

	private static UnityEngine.Vector3 bottomFrontLeft = new UnityEngine.Vector3(-0.5f, -0.5f, 0.5f);

	private static UnityEngine.Vector3 bottomFrontRight = new UnityEngine.Vector3(0.5f, -0.5f, 0.5f);

	private static UnityEngine.Vector3 topBackLeft = new UnityEngine.Vector3(-0.5f, 0.5f, -0.5f);

	private static UnityEngine.Vector3 topBackRight = new UnityEngine.Vector3(0.5f, 0.5f, -0.5f);

	private static UnityEngine.Vector3 bottomBackLeft = new UnityEngine.Vector3(-0.5f, -0.5f, -0.5f);

	private static UnityEngine.Vector3 bottomBackRight = new UnityEngine.Vector3(0.5f, -0.5f, -0.5f);

	public static readonly QuadPlane[] CellFaces = new QuadPlane[6]
	{
		new QuadPlane(topFrontLeft, topFrontRight, bottomFrontRight, bottomFrontLeft),
		new QuadPlane(topBackLeft, topBackRight, bottomBackRight, bottomBackLeft),
		new QuadPlane(topFrontLeft, topBackLeft, bottomBackLeft, bottomFrontLeft),
		new QuadPlane(topFrontRight, topBackRight, bottomBackRight, bottomFrontRight),
		new QuadPlane(bottomFrontLeft, bottomFrontRight, bottomBackRight, bottomBackLeft),
		new QuadPlane(topFrontLeft, topFrontRight, topBackRight, topBackLeft)
	};

	[SerializeField]
	public Transform PropRoot;

	[SerializeField]
	private Vector3Int maxExtents = new Vector3Int(100, 20, 100);

	[SerializeField]
	private PhysicalWire physicalWireTemplate;

	[SerializeField]
	private WireColorDictionary wireColorDictionary;

	private readonly List<PhysicalWire> physicalWires = new List<PhysicalWire>();

	private readonly Dictionary<SerializableGuid, Transform[]> propToWireConnectionPoints = new Dictionary<SerializableGuid, Transform[]>();

	private Vector3Int minimumExtents = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);

	private Vector3Int maximumExtents = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);

	private readonly Dictionary<Vector3Int, Cell> cellLookup = new Dictionary<Vector3Int, Cell>();

	private LevelState levelState;

	private readonly Dictionary<SerializableGuid, Dictionary<Type, Component>> propComponentMap = new Dictionary<SerializableGuid, Dictionary<Type, Component>>();

	private readonly List<TerrainChange> terrainChanges = new List<TerrainChange>();

	private ProfilerMarker linecastMarker = new ProfilerMarker("linecastMarker");

	private ProfilerMarker respawnTerrainCellsWithTilesetIndexesMarker = new ProfilerMarker("respawnTerrainCellsWithTilesetIndexesMarker");

	private ProfilerMarker respawnTerrainCellsWithTilesetIndexMarker = new ProfilerMarker("respawnTerrainCellsWithTilesetIndexMarker");

	public NavMeshBuildSourceTracker BuildSourceTracker;

	private readonly Dictionary<ulong, PendingNetworkIdInfo> pendingNetworkIds = new Dictionary<ulong, PendingNetworkIdInfo>();

	private readonly Dictionary<ulong, GameObject> pendingNetworkObjects = new Dictionary<ulong, GameObject>();

	private readonly Dictionary<ulong, Action> pendingNetworkCallbacks = new Dictionary<ulong, Action>();

	private DepthPlane depthPlane;

	private readonly Dictionary<SerializableGuid, GameObject> gameObjectMap = new Dictionary<SerializableGuid, GameObject>();

	private readonly Dictionary<GameObject, SerializableGuid> reverseGameObjectMap = new Dictionary<GameObject, SerializableGuid>();

	private readonly Dictionary<SerializableGuid, SerializableGuid> assetDefinitionMap = new Dictionary<SerializableGuid, SerializableGuid>();

	private int activeTilesetIndex;

	private NetworkVariable<SerializableGuid> mapId = new NetworkVariable<SerializableGuid>();

	private readonly NetworkVariable<bool> serverPropLoadFinished = new NetworkVariable<bool>(value: false);

	private readonly List<ClaimCellRequest> claimCellRequests = new List<ClaimCellRequest>();

	private readonly List<PropRegisteredRequest> propRegisteredRequest = new List<PropRegisteredRequest>();

	private OfflineStage offlineStage;

	private ProfilerMarker propLoadProfilerMarker = new ProfilerMarker("Prop loading");

	private ProfilerMarker terrainLoadProfilerMarker = new ProfilerMarker("Stage - Load Level - Terrain Loading");

	private ProfilerMarker terrainSpawnProfilerMarker = new ProfilerMarker("Stage - Load Level - Terrain Spawning");

	private ProfilerMarker stageSpawnTileMarker = new ProfilerMarker("Stage - Stage Spawn Tile");

	private ProfilerMarker stageUpdateNeighborsMarker = new ProfilerMarker("Stage - Stage Update Neighbors");

	public bool IsLoading { get; private set; }

	public bool HasLoaded { get; private set; }

	public DepthPlane DepthPlane => depthPlane;

	public Vector3Int MinimumExtents => minimumExtents;

	public Vector3Int MaximumExtents => maximumExtents;

	public float StageFallOffHeight
	{
		get
		{
			if (MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying && depthPlane != null && depthPlane.OverrideFallOffHeight)
			{
				return depthPlane.GetFallOffHeight();
			}
			return MinimumExtents.y - 5;
		}
	}

	public float StageResetHeight => MinimumExtents.y - 5;

	public LevelState LevelState => levelState;

	public IReadOnlyList<Cell> Cells => cellLookup.Values.Concat(offlineStage.GetCells()).ToList();

	public SerializableGuid MapId => mapId.Value;

	public List<PhysicalWire> PhysicalWires => physicalWires;

	public ChunkManager ChunkManager => offlineStage.ChunkManager;

	public bool PropsAreLoaded { get; private set; }

	public bool TerrainLoaded { get; private set; }

	public RuntimePalette RuntimePalette { get; set; }

	public BlockTokenCollection BlockTokenCollection { get; private set; }

	public SerializableGuid GetInstanceIdFromGameObject(GameObject targetObject)
	{
		if (reverseGameObjectMap.TryGetValue(targetObject, out var value))
		{
			return value;
		}
		return SerializableGuid.Empty;
	}

	public bool CanAddProp(PropLibrary.RuntimePropInfo metadata)
	{
		if (MonoBehaviourSingleton<StageManager>.Instance.PropRequirementLookup.ContainsKey(metadata.PropData.AssetID))
		{
			PropRequirement propRequirement = MonoBehaviourSingleton<StageManager>.Instance.PropRequirementLookup[metadata.PropData.AssetID];
			return assetDefinitionMap.Values.Count((SerializableGuid guid) => propRequirement.Guids.Contains(guid)) < propRequirement.MaxCount;
		}
		if (MonoBehaviourSingleton<StageManager>.Instance.BaseTypeRequirementLookup.ContainsKey(metadata.PropData.BaseTypeId))
		{
			BaseTypeRequirement baseTypeRequirement = MonoBehaviourSingleton<StageManager>.Instance.BaseTypeRequirementLookup[metadata.PropData.BaseTypeId];
			List<SerializableGuid> assetIds = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetBaseTypeList(baseTypeRequirement.Guids.ToArray());
			return assetDefinitionMap.Values.Count((SerializableGuid guid) => assetIds.Contains(guid)) < baseTypeRequirement.MaxCount;
		}
		return true;
	}

	public bool CanRemoveProp(SerializableGuid assetId)
	{
		if (MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(assetId, out var metadata))
		{
			return CanRemoveProp(metadata);
		}
		return true;
	}

	private bool CanRemoveProp(PropLibrary.RuntimePropInfo metadata)
	{
		if (MonoBehaviourSingleton<StageManager>.Instance.PropRequirementLookup.ContainsKey(metadata.PropData.AssetID))
		{
			PropRequirement propRequirement = MonoBehaviourSingleton<StageManager>.Instance.PropRequirementLookup[metadata.PropData.AssetID];
			return assetDefinitionMap.Values.Count((SerializableGuid guid) => propRequirement.Guids.Contains(guid)) > propRequirement.MinCount;
		}
		if (MonoBehaviourSingleton<StageManager>.Instance.BaseTypeRequirementLookup.ContainsKey(metadata.PropData.BaseTypeId))
		{
			BaseTypeRequirement baseTypeRequirement = MonoBehaviourSingleton<StageManager>.Instance.BaseTypeRequirementLookup[metadata.PropData.BaseTypeId];
			List<SerializableGuid> assetIds = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetBaseTypeList(baseTypeRequirement.Guids.ToArray());
			return assetDefinitionMap.Values.Count((SerializableGuid guid) => assetIds.Contains(guid)) > baseTypeRequirement.MinCount;
		}
		return true;
	}

	public void SetMapId(SerializableGuid id)
	{
		mapId = new NetworkVariable<SerializableGuid>(id);
	}

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();
		BlockTokenCollection = new BlockTokenCollection();
		if (!base.IsServer)
		{
			MonoBehaviourSingleton<StageManager>.Instance.RegisterStage(this);
		}
		else
		{
			NetworkManager.Singleton.OnClientConnectedCallback += HandleNewPlayer;
		}
	}

	public void HandleNewPlayer(ulong clientId)
	{
		if (NetworkManager.Singleton.LocalClientId != clientId)
		{
			ulong[] networkIds = pendingNetworkIds.Keys.ToArray();
			SerializableGuid[] array = pendingNetworkIds.Values.Select((PendingNetworkIdInfo p) => p.InstanceId).ToArray();
			SerializableGuid[] array2 = new SerializableGuid[array.Length];
			for (int num = 0; num < array.Length; num++)
			{
				array2[num] = assetDefinitionMap[array[num]];
			}
			ClientRpcParams clientRpcParams = new ClientRpcParams
			{
				Send = new ClientRpcSendParams
				{
					TargetClientIds = new List<ulong> { clientId }
				}
			};
			SendNetworkIds_ClientRpc(networkIds, array, array2, clientRpcParams);
		}
	}

	[ClientRpc]
	private void SendNetworkIds_ClientRpc(ulong[] networkIds, SerializableGuid[] instanceIds, SerializableGuid[] assetDefinitionIds, ClientRpcParams clientRpcParams)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			FastBufferWriter bufferWriter = __beginSendClientRpc(2231079621u, clientRpcParams, RpcDelivery.Reliable);
			bool value = networkIds != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(networkIds, default(FastBufferWriter.ForPrimitives));
			}
			bool value2 = instanceIds != null;
			bufferWriter.WriteValueSafe(in value2, default(FastBufferWriter.ForPrimitives));
			if (value2)
			{
				bufferWriter.WriteValueSafe(instanceIds, default(FastBufferWriter.ForNetworkSerializable));
			}
			bool value3 = assetDefinitionIds != null;
			bufferWriter.WriteValueSafe(in value3, default(FastBufferWriter.ForPrimitives));
			if (value3)
			{
				bufferWriter.WriteValueSafe(assetDefinitionIds, default(FastBufferWriter.ForNetworkSerializable));
			}
			__endSendClientRpc(ref bufferWriter, 2231079621u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			for (int i = 0; i < networkIds.Length; i++)
			{
				TrackPendingNetworkObject(networkIds[i], assetDefinitionIds[i], instanceIds[i], updateLevelState: false);
			}
		}
	}

	private void OnEnable()
	{
		foreach (PhysicalWire physicalWire in physicalWires)
		{
			physicalWire.gameObject.SetActive(value: true);
		}
	}

	public void CleanupProps()
	{
		if (!PropsAreLoaded)
		{
			return;
		}
		if (!MonoBehaviourSingleton<StageManager>.Instance)
		{
			UnityEngine.Debug.LogException(new Exception("Calling Cleanup Props without a valid StageManager.Instance!"));
			return;
		}
		if (assetDefinitionMap.Count == 0)
		{
			UnityEngine.Debug.LogWarning("Cleaning up props but assetDefinition map is empty");
		}
		PropsAreLoaded = false;
		propComponentMap.Clear();
		foreach (PhysicalWire physicalWire in physicalWires)
		{
			if ((bool)physicalWire)
			{
				UnityEngine.Object.Destroy(physicalWire.gameObject);
			}
		}
		physicalWires.Clear();
		pendingNetworkIds.Clear();
		pendingNetworkObjects.Clear();
		pendingNetworkCallbacks.Clear();
		UnityEngine.Debug.Log($"Disabling Stage {MapId}");
		foreach (SerializableGuid key in gameObjectMap.Keys)
		{
			GameObject gameObject = gameObjectMap[key];
			PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(assetDefinitionMap[key]);
			if (gameObject == null)
			{
				UnityEngine.Debug.Log($"Obj was null: {key}, Name: {runtimePropInfo.PropData.Name}");
			}
			else if (runtimePropInfo == null)
			{
				UnityEngine.Debug.LogError($"Attempting to cleanup in OnDisable and release cells for asset: (Id: {assetDefinitionMap[key]}, Name: {runtimePropInfo.PropData.Name}), but it didn't exist?");
			}
			else if ((object)gameObject.GetComponent<NetworkObject>() == null || base.IsServer)
			{
				UnityEngine.Object.Destroy(gameObject);
			}
		}
		List<Vector3Int> list = new List<Vector3Int>();
		foreach (Cell value in cellLookup.Values)
		{
			if (value is PropCell)
			{
				list.Add(value.Coordinate);
			}
		}
		foreach (Vector3Int item in list)
		{
			cellLookup.Remove(item);
		}
		assetDefinitionMap.Clear();
		gameObjectMap.Clear();
		reverseGameObjectMap.Clear();
	}

	private void OnDisable()
	{
		foreach (PhysicalWire physicalWire in physicalWires)
		{
			if ((bool)physicalWire)
			{
				UnityEngine.Object.Destroy(physicalWire.gameObject);
			}
		}
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if (offlineStage != null)
		{
			offlineStage.gameObject.SetActive(value: false);
		}
		foreach (GameObject value in gameObjectMap.Values)
		{
			if ((bool)value)
			{
				NetworkObject component = value.GetComponent<NetworkObject>();
				if (component == null || (component != null && base.IsServer))
				{
					UnityEngine.Object.Destroy(value);
				}
			}
		}
		foreach (PhysicalWire physicalWire in physicalWires)
		{
			if (!(physicalWire == null) && (bool)physicalWire.gameObject)
			{
				UnityEngine.Object.Destroy(physicalWire.gameObject);
			}
		}
		physicalWires.Clear();
		if ((bool)NetworkManager.Singleton)
		{
			NetworkManager.Singleton.OnClientConnectedCallback -= HandleNewPlayer;
		}
	}

	public async Task LoadLevelIfNecessary(LevelState newLevelState, CancellationToken cancelToken, Action<string> progressCallback = null)
	{
		if (levelState == null)
		{
			levelState = newLevelState;
			await LoadLevel(cancelToken, progressCallback);
		}
		else
		{
			await HandleEndOfStageLoading(cancelToken);
		}
	}

	private async Task HandleEndOfStageLoading(CancellationToken cancelToken, Action<string> progressCallback = null)
	{
		ExecutePendingRequests();
		if (base.IsServer)
		{
			IsLoading = false;
			PropsAreLoaded = true;
			return;
		}
		claimCellRequests.Clear();
		propRegisteredRequest.Clear();
		NetworkBehaviourSingleton<GameplayMessagingManager>.Instance.LocalRestartLevel();
		bool scopeRequestSent = false;
		progressCallback?.Invoke("Awaiting network objects...");
		while (!NetworkBehaviourSingleton<GameplayMessagingManager>.Instance.LocalClientDoneLoadingScope)
		{
			if (!scopeRequestSent && serverPropLoadFinished.Value)
			{
				MonoBehaviourSingleton<NetworkScopeManager>.Instance.ClientSendScopeRequest(MapId);
				scopeRequestSent = true;
				PropLibrary.RuntimePropInfo[] allRuntimeProps = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetAllRuntimeProps();
				foreach (PropLibrary.RuntimePropInfo runtimePropInfo in allRuntimeProps)
				{
					if (!runtimePropInfo.IsLoading && !runtimePropInfo.IsMissingObject && runtimePropInfo.EndlessProp.IsNetworked)
					{
						NetworkBehaviourSingleton<GameplayMessagingManager>.Instance.ClientBuiltNetworkProp_ServerRpc(runtimePropInfo.EndlessProp.GetComponent<NetworkObject>().PrefabIdHash, MonoBehaviourSingleton<StageManager>.Instance.ActiveLevelGuid, gameplay: false);
					}
				}
			}
			await Task.Yield();
			if (NetworkBehaviourSingleton<GameplayMessagingManager>.Instance == null || cancelToken.IsCancellationRequested)
			{
				return;
			}
		}
		ExecutePendingRequests();
		PropsAreLoaded = true;
		IsLoading = false;
	}

	private void ExecutePendingRequests()
	{
		foreach (ClaimCellRequest claimCellRequest in claimCellRequests)
		{
			if (levelState.TryGetPropEntry(claimCellRequest.InstanceId, out var propEntry))
			{
				ClaimCellsForProp(claimCellRequest.Definition, propEntry.Position, propEntry.Rotation, propEntry.InstanceId);
			}
		}
		foreach (PropRegisteredRequest item in propRegisteredRequest)
		{
			if (!gameObjectMap.TryGetValue(item.InstanceId, out var value))
			{
				continue;
			}
			NetworkObject component = value.GetComponent<NetworkObject>();
			if ((bool)component)
			{
				IRegisteredSubscriber[] componentsInChildren = component.GetComponentsInChildren<IRegisteredSubscriber>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].EndlessRegistered();
				}
			}
		}
	}

	private async Task LoadLevel(CancellationToken cancelToken, Action<string> progressCallback = null)
	{
		MonoBehaviourSingleton<LoadTimeTester>.Instance.StartTracking("Stage");
		bool hadOfflineStage;
		if (MonoBehaviourSingleton<StageManager>.Instance.TryGetOfflineStage(levelState.ToAssetReference(), out offlineStage))
		{
			hadOfflineStage = true;
			offlineStage.gameObject.SetActive(value: true);
			foreach (Cell cell2 in offlineStage.GetCells())
			{
				UpdateBoundaries(cell2);
			}
		}
		else
		{
			hadOfflineStage = false;
			offlineStage = MonoBehaviourSingleton<StageManager>.Instance.GetNewOfflineStage(levelState.ToAssetReference(), levelState.Name);
		}
		IsLoading = true;
		MonoBehaviourSingleton<LoadTimeTester>.Instance.LogTimeDelta("Load Boundries", "Stage");
		await LoadProps(cancelToken, progressCallback);
		MonoBehaviourSingleton<LoadTimeTester>.Instance.LogTimeDelta("Load Props", "Stage");
		if (cancelToken.IsCancellationRequested)
		{
			if (!hadOfflineStage)
			{
				MonoBehaviourSingleton<StageManager>.Instance.RemoveOfflineStage(levelState.ToAssetReference());
			}
			return;
		}
		Stopwatch frameStopWatch = new Stopwatch();
		frameStopWatch.Start();
		Dictionary<Vector3Int, List<Cell>> cellChunkMap = new Dictionary<Vector3Int, List<Cell>>();
		offlineStage.ChunkManager.SetCollectionModeToLoadTime();
		if (!hadOfflineStage)
		{
			for (int coordinateIndex = 0; coordinateIndex < levelState.TerrainEntries.Count; coordinateIndex++)
			{
				TerrainEntry terrainEntry = levelState.TerrainEntries[coordinateIndex];
				Cell item = LoadCellPosition(terrainEntry.Position, terrainEntry.TilesetId);
				if (cellLookup.ContainsKey(terrainEntry.Position))
				{
					continue;
				}
				Vector3Int chunkPosition = ChunkManager.GetChunkPosition(terrainEntry.Position);
				cellChunkMap.TryAdd(chunkPosition, new List<Cell>());
				cellChunkMap[chunkPosition].Add(item);
				if (frameStopWatch.ElapsedMilliseconds > 64)
				{
					await Task.Yield();
					frameStopWatch.Restart();
					progressCallback?.Invoke($"Loading terrain {coordinateIndex:N0}/{levelState.TerrainEntries.Count:N0}");
					if (cancelToken.IsCancellationRequested)
					{
						MonoBehaviourSingleton<StageManager>.Instance.RemoveOfflineStage(levelState.ToAssetReference());
						return;
					}
				}
			}
			int cellCount = 0;
			int totalTerrainCells = levelState.TerrainEntries.Count;
			progressCallback?.Invoke($"Spawning terrain {0}/{totalTerrainCells:N0}");
			foreach (Vector3Int coords in cellChunkMap.Keys)
			{
				foreach (Cell item2 in cellChunkMap[coords])
				{
					if (cellLookup.ContainsKey(item2.Coordinate))
					{
						continue;
					}
					cellCount++;
					TerrainCell cell = item2 as TerrainCell;
					SpawnTile(GetTilesetFromCell(cell), cell, addToChunkManager: true);
					UpdateBoundaries(item2);
					if (frameStopWatch.ElapsedMilliseconds <= 64)
					{
						continue;
					}
					await Task.Yield();
					frameStopWatch.Restart();
					progressCallback?.Invoke($"Spawning terrain {cellCount:N0}/{totalTerrainCells:N0}");
					if (cancelToken.IsCancellationRequested)
					{
						if (!hadOfflineStage)
						{
							MonoBehaviourSingleton<StageManager>.Instance.RemoveOfflineStage(levelState.ToAssetReference());
						}
						return;
					}
				}
				if ((bool)offlineStage.ChunkManager)
				{
					offlineStage.ChunkManager.MarkChunkAsReadyToMerge(coords);
				}
			}
			MonoBehaviourSingleton<LoadTimeTester>.Instance.LogTimeDelta("Loaded Terrain", "Stage");
		}
		await Task.Yield();
		BuildSourceTracker = new NavMeshBuildSourceTracker();
		Collider[] componentsInChildren = offlineStage.TileRoot.GetComponentsInChildren<Collider>();
		foreach (Collider collider in componentsInChildren)
		{
			BuildSourceTracker.AddTerrainSource(collider);
		}
		foreach (GameObject key in reverseGameObjectMap.Keys)
		{
			BuildSourceTracker.AddPropToSources(key);
		}
		if (minimumExtents == new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue))
		{
			minimumExtents = Vector3Int.zero;
		}
		if (maximumExtents == new Vector3Int(int.MinValue, int.MinValue, int.MinValue))
		{
			maximumExtents = Vector3Int.zero;
		}
		if (base.IsServer)
		{
			serverPropLoadFinished.Value = true;
		}
		for (int awaitingChunkCount = offlineStage.ChunkManager.AwaitingChunkCount; awaitingChunkCount > 0; awaitingChunkCount = offlineStage.ChunkManager.AwaitingChunkCount)
		{
			progressCallback?.Invoke($"Optimizing terrain {awaitingChunkCount} chunks remain...");
			await Task.Yield();
			if (cancelToken.IsCancellationRequested)
			{
				if (!hadOfflineStage)
				{
					MonoBehaviourSingleton<StageManager>.Instance.RemoveOfflineStage(levelState.ToAssetReference());
				}
				return;
			}
		}
		MonoBehaviourSingleton<LoadTimeTester>.Instance.LogTimeDelta("Chunks Merged", "Stage");
		offlineStage.ChunkManager.SetCollectionModeToRuntime();
		TerrainLoaded = true;
		progressCallback?.Invoke("Awaiting load finalization...");
		MonoBehaviourSingleton<StageManager>.Instance.TerrainAndPropsLoaded.Invoke(this);
		while (!BlockTokenCollection.IsPoolEmpty)
		{
			await Task.Yield();
			if (cancelToken.IsCancellationRequested)
			{
				if (!hadOfflineStage)
				{
					MonoBehaviourSingleton<StageManager>.Instance.RemoveOfflineStage(levelState.ToAssetReference());
				}
				return;
			}
		}
		MonoBehaviourSingleton<LoadTimeTester>.Instance.LogTimeDelta("Blocking Token Collection", "Stage");
		progressCallback?.Invoke("Waiting for host to load...");
		await HandleEndOfStageLoading(cancelToken, progressCallback);
		MonoBehaviourSingleton<LoadTimeTester>.Instance.StopTracking("Stage");
		if (cancelToken.IsCancellationRequested)
		{
			if (!hadOfflineStage)
			{
				MonoBehaviourSingleton<StageManager>.Instance.RemoveOfflineStage(levelState.ToAssetReference());
			}
		}
		else
		{
			HasLoaded = true;
		}
	}

	private async Task LoadProps(CancellationToken cancelToken, Action<string> progressCallback = null)
	{
		Stopwatch frameStopWatch = new Stopwatch();
		int propEntryCount = levelState.PropEntries.Count();
		for (int index = 0; index < propEntryCount; index++)
		{
			PropEntry propEntry = levelState.PropEntries[index];
			if (MonoBehaviourSingleton<StageManager>.Instance.IsPropDestroyed(propEntry.InstanceId))
			{
				continue;
			}
			if (MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(propEntry.AssetId, out var metadata))
			{
				if (metadata.IsMissingObject)
				{
					MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.ReplacePropWithMissingObject(propEntry.InstanceId, metadata, metadata);
				}
				else if (metadata.IsLoading)
				{
					UnityEngine.Debug.LogError("LoadingProp: " + metadata.PropData.Name + " Loading props, but the meta data is still loading? Not expected, please report. Prop: " + metadata.PropData.Name + ", AssetId: " + metadata.PropData.AssetID);
				}
				else if (metadata.EndlessProp.IsNetworked)
				{
					if (base.IsServer)
					{
						GameObject gameObject = UnityEngine.Object.Instantiate(metadata.EndlessProp, propEntry.Position, propEntry.Rotation).gameObject;
						NetworkObject component = gameObject.GetComponent<NetworkObject>();
						if (component != null)
						{
							component.Spawn();
							TrackPendingNetworkId(component.NetworkObjectId, propEntry.InstanceId, updateLevelState: false);
						}
						TrackNonNetworkedObject(propEntry.AssetId, propEntry.InstanceId, gameObject.gameObject, updateLevelState: false);
					}
				}
				else
				{
					GameObject newObject = UnityEngine.Object.Instantiate(metadata.EndlessProp, propEntry.Position, propEntry.Rotation).gameObject;
					TrackNonNetworkedObject(propEntry.AssetId, propEntry.InstanceId, newObject, updateLevelState: false);
				}
			}
			else
			{
				MonoBehaviourSingleton<StageManager>.Instance.PropFailedToLoad(propEntry);
			}
			if (frameStopWatch.ElapsedMilliseconds > 64)
			{
				await Task.Yield();
				if (cancelToken.IsCancellationRequested)
				{
					break;
				}
				frameStopWatch.Restart();
				progressCallback?.Invoke($"Spawning prop {index:N0}/{propEntryCount:N0}");
			}
		}
	}

	public async Task LoadPropsInStage(CancellationToken cancelToken, Action<string> progressCallback = null)
	{
		await LoadProps(cancelToken, progressCallback);
		if (!cancelToken.IsCancellationRequested)
		{
			await HandleEndOfStageLoading(cancelToken, progressCallback);
		}
	}

	public void LoadExistingWires(bool setDisplayed = false)
	{
		if (!base.IsClient || !(MonoBehaviourSingleton<StageManager>.Instance.ActiveStage != null))
		{
			return;
		}
		Stage activeStage = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage;
		foreach (WireBundle wireBundle in MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.WireBundles)
		{
			if (!activeStage.PhysicalWires.Any((PhysicalWire wire) => wire.BundleId == wireBundle.BundleId))
			{
				SpawnPhysicalWire(wireBundle.BundleId, wireBundle.EmitterInstanceId, wireBundle.ReceiverInstanceId, wireBundle.RerouteNodeIds, wireBundle.WireColor, setDisplayed);
			}
		}
	}

	public PhysicalWire SpawnPhysicalWire(SerializableGuid bundleId, SerializableGuid emitterInstanceId, SerializableGuid receiverInstanceId, IEnumerable<SerializableGuid> rerouteNodeIds, WireColor wireColor = WireColor.NoColor, bool startActive = false)
	{
		GameObject gameObjectFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(emitterInstanceId);
		GameObject gameObjectFromInstanceId2 = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(receiverInstanceId);
		if (gameObjectFromInstanceId == null || gameObjectFromInstanceId2 == null)
		{
			return null;
		}
		PhysicalWire physicalWire = UnityEngine.Object.Instantiate(physicalWireTemplate);
		Transform connectionPointFromObject = GetConnectionPointFromObject(gameObjectFromInstanceId, emitterInstanceId, ConnectionPoint.Emitter);
		Transform connectionPointFromObject2 = GetConnectionPointFromObject(gameObjectFromInstanceId2, receiverInstanceId, ConnectionPoint.Receiver);
		UnityEngine.Color color = wireColorDictionary[wireColor].Color;
		physicalWire.Setup(bundleId, emitterInstanceId, connectionPointFromObject, receiverInstanceId, connectionPointFromObject2, rerouteNodeIds, color);
		physicalWire.gameObject.SetActive(startActive);
		MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.PhysicalWires.Add(physicalWire);
		return physicalWire;
	}

	public Transform GetConnectionPointFromObject(GameObject instanceObject, SerializableGuid instanceId, ConnectionPoint connectionPoint)
	{
		if (!propToWireConnectionPoints.ContainsKey(instanceId))
		{
			propToWireConnectionPoints.Add(instanceId, new Transform[2]);
		}
		if (propToWireConnectionPoints[instanceId][(int)connectionPoint] == null)
		{
			propToWireConnectionPoints[instanceId][(int)connectionPoint] = GenerateConnectionPoint(instanceId, connectionPoint, instanceObject);
		}
		return propToWireConnectionPoints[instanceId][(int)connectionPoint];
	}

	private Transform GenerateConnectionPoint(SerializableGuid instanceId, ConnectionPoint connectionPoint, GameObject parent)
	{
		Vector3Int[] array = new Vector3Int[1] { WorldSpacePointToGridCoordinate(parent.transform.position) };
		UnityEngine.Vector3 vector = array[0];
		UnityEngine.Vector3 vector2 = array[^1];
		UnityEngine.Vector3 vector3 = (vector2 - vector) * 0.5f;
		float num = 1f;
		UnityEngine.Vector3 vector4 = new UnityEngine.Vector3(0f, num * 0.25f, 0f);
		if (connectionPoint == ConnectionPoint.Receiver)
		{
			vector4 = -vector4;
		}
		UnityEngine.Debug.DrawLine(vector, vector2, UnityEngine.Color.red, 100f);
		Transform obj = new GameObject($"Physical Wire Connection Point ({instanceId})").transform;
		obj.SetParent(parent.transform, worldPositionStays: false);
		obj.position = array[0] + vector3 + vector4;
		return obj;
	}

	public bool CanEraseTerrainCell(Vector3Int coordinate)
	{
		Cell cellFromCoordinate = GetCellFromCoordinate(coordinate);
		if (cellFromCoordinate != null)
		{
			return cellFromCoordinate is TerrainCell;
		}
		return false;
	}

	public bool CanPaintCellPosition(Vector3Int coordinate)
	{
		if (IsPositionInBounds(coordinate))
		{
			return GetCellFromCoordinate(coordinate) == null;
		}
		return false;
	}

	public Cell GetCellFromCoordinate(Vector3Int coordinate)
	{
		if (!cellLookup.TryGetValue(coordinate, out var value))
		{
			offlineStage.TryGetCellFromCoordinate(coordinate, out value);
		}
		return value;
	}

	public T GetCellFromCoordinateAs<T>(Vector3Int coordinate) where T : Cell
	{
		Cell cellFromCoordinate = GetCellFromCoordinate(coordinate);
		if (cellFromCoordinate == null)
		{
			return null;
		}
		return cellFromCoordinate as T;
	}

	public static Vector3Int WorldSpacePointToGridCoordinate(UnityEngine.Vector3 worldSpacePoint)
	{
		return new Vector3Int(Mathf.RoundToInt(worldSpacePoint.x), Mathf.RoundToInt(worldSpacePoint.y), Mathf.RoundToInt(worldSpacePoint.z));
	}

	public void PaintCellPositions(IEnumerable<Vector3Int> coordinates, int tilesetIndex, bool updateLevelState = true, bool updateNeighbors = true, bool generateChanges = false)
	{
		List<TerrainCell> list = new List<TerrainCell>();
		List<Vector3Int> list2 = new List<Vector3Int>();
		foreach (Vector3Int coordinate in coordinates)
		{
			Cell cellFromCoordinate = GetCellFromCoordinate(coordinate);
			if (!IsPositionInBounds(coordinate) || cellFromCoordinate != null)
			{
				if (cellFromCoordinate != null)
				{
					list2.Add(coordinate);
				}
				UnityEngine.Debug.LogWarning("Invalid cell");
				continue;
			}
			Transform transform = new GameObject($"Cell ({coordinate.x}, {coordinate.y}, {coordinate.z})").transform;
			transform.SetParent(offlineStage.TileRoot);
			transform.position = coordinate;
			TerrainCell terrainCell = new TerrainCell(coordinate, transform);
			terrainCell.SetTilesetDetails(tilesetIndex);
			list.Add(terrainCell);
			offlineStage.AddCell(coordinate, terrainCell);
			if (updateLevelState)
			{
				levelState.AddTerrainCell(terrainCell);
			}
		}
		if (generateChanges)
		{
			terrainChanges.Add(new TerrainChange
			{
				Coordinates = list.Select((TerrainCell cell) => cell.Coordinate).ToArray(),
				TilesetIndex = tilesetIndex,
				Erased = false
			});
		}
		if (!updateNeighbors)
		{
			return;
		}
		HashSet<Vector3Int> excludedPositions = coordinates.Except(list2).ToHashSet();
		foreach (TerrainCell item in list)
		{
			SpawnTile(TilesetAtIndex(tilesetIndex), item, addToChunkManager: true);
			UpdateNeighbors(item.Coordinate, TilesetAtIndex(tilesetIndex), excludedPositions);
			UpdateNearbyFringe(item.Coordinate);
			UpdateBoundaries(item);
		}
	}

	private Cell LoadCellPosition(Vector3Int coordinate, int tilesetIndex)
	{
		Transform transform = new GameObject($"Tile - Coordinate: {coordinate.x},{coordinate.y},{coordinate.z}").transform;
		transform.SetParent(offlineStage.TileRoot);
		transform.position = coordinate;
		TerrainCell terrainCell = new TerrainCell(coordinate, transform);
		terrainCell.SetTilesetDetails(tilesetIndex);
		offlineStage.AddCell(coordinate, terrainCell);
		return terrainCell;
	}

	public void TrackPendingNetworkId(ulong networkObjectId, SerializableGuid instanceId, bool updateLevelState = true, Action callback = null)
	{
		if (pendingNetworkIds.ContainsKey(networkObjectId))
		{
			UnityEngine.Debug.LogWarning($"We reused a pending network Id. Are we okay with this? {networkObjectId}");
			pendingNetworkIds[networkObjectId] = new PendingNetworkIdInfo(networkObjectId, instanceId, updateLevelState);
		}
		else
		{
			pendingNetworkIds.Add(networkObjectId, new PendingNetworkIdInfo(networkObjectId, instanceId, updateLevelState));
		}
		pendingNetworkCallbacks[networkObjectId] = callback;
	}

	[ClientRpc]
	private void TrackPendingNetworkObject_ClientRpc(SerializableGuid assetId, SerializableGuid instanceId, ulong networkObjectId, bool updateLevelState = true, ClientRpcParams rpcParams = default(ClientRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				FastBufferWriter bufferWriter = __beginSendClientRpc(667828874u, rpcParams, RpcDelivery.Reliable);
				bufferWriter.WriteValueSafe(in assetId, default(FastBufferWriter.ForNetworkSerializable));
				bufferWriter.WriteValueSafe(in instanceId, default(FastBufferWriter.ForNetworkSerializable));
				BytePacker.WriteValueBitPacked(bufferWriter, networkObjectId);
				bufferWriter.WriteValueSafe(in updateLevelState, default(FastBufferWriter.ForPrimitives));
				__endSendClientRpc(ref bufferWriter, 667828874u, rpcParams, RpcDelivery.Reliable);
			}
			if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				__rpc_exec_stage = __RpcExecStage.Send;
				TrackPendingNetworkObject(networkObjectId, assetId, instanceId, updateLevelState);
			}
		}
	}

	public void TrackPendingNetworkObject(ulong networkObjectId, SerializableGuid assetId, SerializableGuid instanceId, bool updateLevelState = true, Action callback = null)
	{
		assetDefinitionMap.TryAdd(instanceId, assetId);
		if (pendingNetworkObjects.ContainsKey(networkObjectId) && pendingNetworkObjects[networkObjectId] != null)
		{
			UnityEngine.Debug.Log(string.Format("{0}::{1}: Adding Instance Id: {2}", "Stage", "TrackPendingNetworkObject", instanceId));
			gameObjectMap.Add(instanceId, pendingNetworkObjects[networkObjectId]);
			reverseGameObjectMap.Add(pendingNetworkObjects[networkObjectId], instanceId);
			PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary[assetId];
			if (updateLevelState)
			{
				AddObjectToLevelState(pendingNetworkObjects[networkObjectId], runtimePropInfo, instanceId);
			}
			if (PropsAreLoaded)
			{
				IRegisteredSubscriber[] componentsInChildren = pendingNetworkObjects[networkObjectId].GetComponentsInChildren<IRegisteredSubscriber>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].EndlessRegistered();
				}
				PropEntry propEntry = levelState.GetPropEntry(instanceId);
				ClaimCellsForProp(runtimePropInfo, propEntry.Position, propEntry.Rotation, instanceId);
				pendingNetworkObjects.Remove(networkObjectId);
			}
			else
			{
				claimCellRequests.Add(new ClaimCellRequest(runtimePropInfo, instanceId, networkObjectId));
				propRegisteredRequest.Add(new PropRegisteredRequest(instanceId));
			}
			if (pendingNetworkCallbacks.TryGetValue(networkObjectId, out var value))
			{
				value?.Invoke();
				pendingNetworkCallbacks.Remove(networkObjectId);
			}
			else if (callback != null)
			{
				callback?.Invoke();
			}
		}
		else
		{
			TrackPendingNetworkId(networkObjectId, instanceId, updateLevelState, callback);
		}
	}

	public void TrackNonNetworkedObject(SerializableGuid assetId, SerializableGuid instanceId, GameObject newObject, bool updateLevelState = true)
	{
		assetDefinitionMap.Add(instanceId, assetId);
		gameObjectMap.Add(instanceId, newObject);
		reverseGameObjectMap.Add(newObject, instanceId);
		MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(assetId, out var metadata);
		if (updateLevelState)
		{
			AddObjectToLevelState(newObject, metadata, instanceId);
		}
		PropEntry propEntry = levelState.GetPropEntry(instanceId);
		ClaimCellsForProp(metadata, propEntry.Position, propEntry.Rotation, instanceId);
		IRegisteredSubscriber[] componentsInChildren = newObject.GetComponentsInChildren<IRegisteredSubscriber>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].EndlessRegistered();
		}
	}

	public void TrackNetworkObject(ulong networkObjectId, GameObject networkObject, Action callback = null)
	{
		if (pendingNetworkIds.ContainsKey(networkObjectId))
		{
			SerializableGuid instanceId = pendingNetworkIds[networkObjectId].InstanceId;
			if (!gameObjectMap.ContainsKey(instanceId))
			{
				gameObjectMap.Add(instanceId, networkObject);
			}
			if (!reverseGameObjectMap.ContainsKey(networkObject))
			{
				reverseGameObjectMap.Add(networkObject, instanceId);
			}
			if (!MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(assetDefinitionMap[instanceId], out var metadata))
			{
				return;
			}
			if (pendingNetworkIds[networkObjectId].UpdateLevelState)
			{
				AddObjectToLevelState(networkObject, metadata, instanceId);
			}
			if (PropsAreLoaded)
			{
				PropEntry propEntry = levelState.GetPropEntry(instanceId);
				ClaimCellsForProp(metadata, propEntry.Position, propEntry.Rotation, instanceId);
			}
			else
			{
				claimCellRequests.Add(new ClaimCellRequest(metadata, instanceId, networkObjectId));
			}
			pendingNetworkIds.Remove(networkObjectId);
			if (pendingNetworkCallbacks.TryGetValue(networkObjectId, out var value))
			{
				value?.Invoke();
				pendingNetworkCallbacks.Remove(networkObjectId);
			}
			else if (callback != null)
			{
				callback?.Invoke();
			}
			if (PropsAreLoaded)
			{
				IRegisteredSubscriber[] componentsInChildren = networkObject.GetComponentsInChildren<IRegisteredSubscriber>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].EndlessRegistered();
				}
			}
			else
			{
				propRegisteredRequest.Add(new PropRegisteredRequest(instanceId));
			}
		}
		else
		{
			pendingNetworkObjects.Add(networkObjectId, networkObject);
			if (callback != null)
			{
				pendingNetworkCallbacks.Add(networkObjectId, callback);
			}
		}
	}

	public void UntrackNetworkObject(GameObject gameObject, ulong networkInstanceId)
	{
		if (reverseGameObjectMap.TryGetValue(gameObject, out var value) && gameObjectMap[value] == gameObject)
		{
			reverseGameObjectMap.Remove(gameObject);
			gameObjectMap.Remove(value);
			assetDefinitionMap.Remove(value);
			pendingNetworkIds.Remove(networkInstanceId);
		}
	}

	private void UpdateNearbyFringe(Vector3Int sourceCoordinate)
	{
		Vector3Int[] array = new Vector3Int[13]
		{
			Vector3Int.up + Vector3Int.forward,
			Vector3Int.up + Vector3Int.right,
			Vector3Int.up + Vector3Int.left,
			Vector3Int.up + Vector3Int.back,
			Vector3Int.down,
			Vector3Int.down + Vector3Int.forward,
			Vector3Int.down + Vector3Int.right,
			Vector3Int.down + Vector3Int.left,
			Vector3Int.down + Vector3Int.back,
			Vector3Int.forward + Vector3Int.right,
			Vector3Int.right + Vector3Int.back,
			Vector3Int.back + Vector3Int.left,
			Vector3Int.left + Vector3Int.forward
		};
		for (int i = 0; i < array.Length; i++)
		{
			Vector3Int coordinate = sourceCoordinate + array[i];
			Cell cellFromCoordinate = GetCellFromCoordinate(coordinate);
			if (cellFromCoordinate is TerrainCell)
			{
				Tileset tilesetFromCell = GetTilesetFromCell(cellFromCoordinate);
				if (tilesetFromCell != null && tilesetFromCell.HasFringe)
				{
					SpawnTile(tilesetFromCell, (TerrainCell)cellFromCoordinate, addToChunkManager: false);
				}
			}
		}
	}

	private void SpawnTile(Tileset tileset, TerrainCell cell, bool addToChunkManager)
	{
		TileSpawnContext validVisualForCellPosition = tileset.GetValidVisualForCellPosition(this, cell.Coordinate);
		validVisualForCellPosition.Tile.Spawn(cell, validVisualForCellPosition, this, UnityEngine.Object.DestroyImmediate);
		if ((bool)offlineStage.ChunkManager && addToChunkManager)
		{
			offlineStage.ChunkManager.CellAdded(cell.Coordinate, cell.CellBase.gameObject);
		}
	}

	private void UpdateNeighbors(Vector3Int coordinates, Tileset tileset, HashSet<Vector3Int> excludedPositions = null)
	{
		if (excludedPositions == null)
		{
			excludedPositions = new HashSet<Vector3Int>();
		}
		for (int i = -1; i < 2; i++)
		{
			for (int j = -1; j < 2; j++)
			{
				for (int k = -1; k < 2; k++)
				{
					Vector3Int vector3Int = new Vector3Int(i, j, k);
					if (!(vector3Int == Vector3Int.zero))
					{
						Vector3Int vector3Int2 = coordinates + vector3Int;
						if (!excludedPositions.Contains(vector3Int2))
						{
							UpdateCellAtPosition(coordinates, vector3Int2, tileset);
						}
					}
				}
			}
		}
	}

	private void UpdateCellAtPosition(Vector3Int originCoordinate, Vector3Int coordinate, Tileset originCellTileset)
	{
		Cell cellFromCoordinate = GetCellFromCoordinate(coordinate);
		Tileset tilesetFromCell = GetTilesetFromCell(cellFromCoordinate);
		if (cellFromCoordinate != null && tilesetFromCell != null && ((originCellTileset != null && originCellTileset.TilesetType == TilesetType.Slope) || (originCoordinate.y <= coordinate.y && tilesetFromCell.HasFringe) || originCellTileset == null || originCellTileset.ConsidersTileset(tilesetFromCell) || tilesetFromCell.ConsidersTileset(originCellTileset) || tilesetFromCell.TilesetType == TilesetType.Slope || originCoordinate.y < coordinate.y || tilesetFromCell.HasFringe))
		{
			SpawnTile(tilesetFromCell, cellFromCoordinate as TerrainCell, addToChunkManager: false);
			if ((bool)offlineStage.ChunkManager)
			{
				offlineStage.ChunkManager.CellUpdated(coordinate);
			}
		}
	}

	public Tileset GetTilesetFromCell(Cell cell)
	{
		if (cell is TerrainCell terrainCell)
		{
			return TilesetAtIndex(terrainCell.TilesetIndex);
		}
		return null;
	}

	[CanBeNull]
	public string GetTilesetIdFromCell(Cell cell)
	{
		if (cell is TerrainCell terrainCell)
		{
			return TilesetAtIndex(terrainCell.TilesetIndex)?.Asset.AssetID;
		}
		return null;
	}

	private bool CheckValidity(Vector3Int cellCoordinates)
	{
		if (GetCellFromCoordinate(cellCoordinates) != null)
		{
			return false;
		}
		return true;
	}

	private void AddObjectToLevelState(GameObject instance, PropLibrary.RuntimePropInfo metadata, SerializableGuid instanceId)
	{
		levelState.AddProp(instance, metadata, instanceId);
	}

	private void ClaimCellsForProp(PropLibrary.RuntimePropInfo runtimePropInfo, UnityEngine.Vector3 propPosition, Quaternion rotation, SerializableGuid instanceId)
	{
		PropLocationOffset[] array = ((runtimePropInfo != null) ? GetRotatedPropLocations(runtimePropInfo.PropData, propPosition, rotation) : new PropLocationOffset[1]
		{
			new PropLocationOffset
			{
				Offset = new Vector3Int(Mathf.FloorToInt(propPosition.x), Mathf.FloorToInt(propPosition.y), Mathf.FloorToInt(propPosition.z))
			}
		});
		PropLocationOffset[] array2 = array;
		foreach (PropLocationOffset propLocationOffset in array2)
		{
			GameObject gameObject = new GameObject($"Cell ({propLocationOffset.Offset.x}, {propLocationOffset.Offset.y}, {propLocationOffset.Offset.z})");
			gameObject.transform.SetParent(PropRoot);
			gameObject.transform.position = propLocationOffset.Offset;
			PropCell propCell = new PropCell(propLocationOffset, gameObject.transform)
			{
				InstanceId = instanceId
			};
			UpdateBoundaries(propCell);
			if (!cellLookup.TryGetValue(propLocationOffset.Offset, out var value))
			{
				offlineStage.TryGetCellFromCoordinate(propLocationOffset.Offset, out value);
			}
			if (value != null)
			{
				if (value is TerrainCell)
				{
					UnityEngine.Debug.LogWarning("Attempted to claim cell for a prop that is actually terrain?");
				}
				else if (value is PropCell propCell2 && propCell2.InstanceId != instanceId)
				{
					UnityEngine.Debug.LogWarning($"Attempting to claim a cell for instance id: {instanceId} but it is already claimed by instance id: {propCell2.InstanceId}");
				}
			}
			else
			{
				cellLookup.Add(propLocationOffset.Offset, propCell);
			}
		}
		array2 = array;
		foreach (PropLocationOffset propLocationOffset2 in array2)
		{
			UpdateNeighbors(propLocationOffset2.Offset, null);
		}
	}

	private void CheckStageBoundary(Cell cell)
	{
		if (!ResetStageBoundaryIfNecessary(cell))
		{
			return;
		}
		foreach (Cell value in cellLookup.Values)
		{
			UpdateBoundaries(value);
		}
		foreach (Cell cell2 in offlineStage.GetCells())
		{
			UpdateBoundaries(cell2);
		}
	}

	public void EraseCells(IEnumerable<Vector3Int> coordinates)
	{
		List<Cell> list = new List<Cell>();
		foreach (Vector3Int coordinate in coordinates)
		{
			Cell cellFromCoordinate = GetCellFromCoordinate(coordinate);
			if (cellLookup.Remove(coordinate) || offlineStage.RemoveCell(coordinate))
			{
				levelState.RemoveTerrainCell(cellFromCoordinate);
				if (cellFromCoordinate is TerrainCell && (bool)offlineStage.ChunkManager)
				{
					offlineStage.ChunkManager.CellRemoved(cellFromCoordinate.Coordinate, cellFromCoordinate.CellBase.gameObject);
				}
				if (cellFromCoordinate.CellBase != null)
				{
					UnityEngine.Object.Destroy(cellFromCoordinate.CellBase.gameObject);
				}
				list.Add(cellFromCoordinate);
			}
		}
		foreach (Cell item in list)
		{
			CheckStageBoundary(item);
			UpdateNeighbors(item.Coordinate, (item is TerrainCell terrainCell) ? TilesetAtIndex(terrainCell.TilesetIndex) : null);
			UpdateNearbyFringe(item.Coordinate);
		}
	}

	public HashSet<SerializableGuid> GetWiresUsingProps(IEnumerable<SerializableGuid> propIds)
	{
		HashSet<SerializableGuid> hashSet = new HashSet<SerializableGuid>();
		foreach (SerializableGuid propId in propIds)
		{
			foreach (WireBundle wireBundle in levelState.WireBundles)
			{
				if (wireBundle.EmitterInstanceId == propId || wireBundle.ReceiverInstanceId == propId)
				{
					hashSet.Add(wireBundle.BundleId);
				}
			}
		}
		return hashSet;
	}

	public void RemoveProp(SerializableGuid instanceId, SerializableGuid assetId)
	{
		UnityEngine.Debug.Log("Stage.RemoveProps - Removing Props");
		if (MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(assetId, out var metadata))
		{
			DestroyStageObject(instanceId, metadata, wasMissingObject: false);
		}
		PendingNetworkIdInfo pendingNetworkIdInfo = pendingNetworkIds.Values.FirstOrDefault((PendingNetworkIdInfo item) => item.InstanceId == instanceId);
		if (pendingNetworkIdInfo != null)
		{
			pendingNetworkIds.Remove(pendingNetworkIdInfo.NetworkId);
		}
		LevelState.RemoveProp(instanceId);
	}

	public void ReplacePropWithMissingObject(SerializableGuid instanceId, PropLibrary.RuntimePropInfo propInfo, PropLibrary.RuntimePropInfo missingPropInfo)
	{
		UnityEngine.Debug.Log($"ReplacePropWithMissingObject:: Replacing {instanceId} with missing object");
		DestroyStageObject(instanceId, propInfo, wasMissingObject: false);
		PendingNetworkIdInfo pendingNetworkIdInfo = pendingNetworkIds.Values.FirstOrDefault((PendingNetworkIdInfo item) => item.InstanceId == instanceId);
		if (pendingNetworkIdInfo != null)
		{
			pendingNetworkIds.Remove(pendingNetworkIdInfo.NetworkId);
		}
		UnityEngine.Debug.Log($"ReplacePropWithMissingObject:: Spawning the missing object, Instance Id: {instanceId}, EndlessProp.GameObject Name {missingPropInfo.EndlessProp.gameObject.name}");
		EndlessProp endlessProp = UnityEngine.Object.Instantiate(missingPropInfo.EndlessProp);
		assetDefinitionMap.Add(instanceId, missingPropInfo.PropData.AssetID);
		gameObjectMap.Add(instanceId, endlessProp.gameObject);
		reverseGameObjectMap.Add(endlessProp.gameObject, instanceId);
		PropEntry propEntry = levelState.GetPropEntry(instanceId);
		endlessProp.transform.position = WorldSpacePointToGridCoordinate(propEntry.Position);
		ClaimCellsForProp(missingPropInfo, propEntry.Position, propEntry.Rotation, instanceId);
	}

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
	}

	public LineCastHit Linecast(Ray ray, float length, float scalar, SerializableGuid ignoredInstanceId)
	{
		return Linecast(ray, length, scalar, length, ignoredInstanceId);
	}

	public LineCastHit Linecast(Ray ray, float length, float scalar, float maxFallbackDistance, SerializableGuid ignoredInstanceId)
	{
		LineCastHit result = default(LineCastHit);
		LinecastEnumerator linecastEnumerator = new LinecastEnumerator(ray, length, scalar);
		Vector3Int current = linecastEnumerator.Current;
		Vector3Int nearestPositionToObject = Vector3Int.zero;
		float num = maxFallbackDistance * maxFallbackDistance;
		while (!IsCoordinateInPotentialBounds(WorldSpacePointToGridCoordinate(ray.origin)) || IsCoordinateInPotentialBounds(linecastEnumerator.Current))
		{
			Cell cellFromCoordinate = GetCellFromCoordinate(linecastEnumerator.Current);
			if (cellFromCoordinate != null && (!(cellFromCoordinate is PropCell propCell) || propCell.InstanceId != ignoredInstanceId))
			{
				result.IntersectionOccured = true;
				result.Distance = (linecastEnumerator.PositionAtStep - ray.origin).magnitude;
				result.IntersectedObjectPosition = linecastEnumerator.Current;
				result.NearestPositionToObject = current;
				break;
			}
			if ((linecastEnumerator.PositionAtStep - ray.origin).sqrMagnitude < num)
			{
				nearestPositionToObject = linecastEnumerator.Current;
			}
			current = linecastEnumerator.Current;
			if (!linecastEnumerator.MoveNext())
			{
				break;
			}
		}
		if (!result.IntersectionOccured)
		{
			result.NearestPositionToObject = nearestPositionToObject;
		}
		return result;
		bool IsCoordinateInPotentialBounds(Vector3Int coordinate)
		{
			Vector3Int vector3Int = maximumExtents - minimumExtents;
			Vector3Int vector3Int2 = new Vector3Int(100, 100, 100) - vector3Int;
			Vector3Int vector3Int3 = minimumExtents - vector3Int2;
			Vector3Int vector3Int4 = maximumExtents + vector3Int2;
			if (coordinate.x < vector3Int3.x || coordinate.y < vector3Int3.y || coordinate.z < vector3Int3.z)
			{
				return false;
			}
			if (coordinate.x > vector3Int4.x || coordinate.y > vector3Int4.y || coordinate.z > vector3Int4.z)
			{
				return false;
			}
			return true;
		}
	}

	public GameObject GetGameObjectFromInstanceId(SerializableGuid instanceId)
	{
		if (!gameObjectMap.ContainsKey(instanceId))
		{
			bool flag = false;
			for (int i = 0; i < levelState.PropEntries.Count; i++)
			{
				if (levelState.PropEntries[i].InstanceId == instanceId)
				{
					flag = true;
					UnityEngine.Debug.LogWarning($"Instance Id: {instanceId} was not in GameObjectMap, but exists in level state. Label: {levelState.PropEntries[i].Label}, Asset Id: {levelState.PropEntries[i].AssetId}");
					break;
				}
			}
			if (!flag)
			{
				UnityEngine.Debug.LogWarning($"Attempted to find Instance Id: {instanceId} in the GameObjectMap, and after searching level state, the object was not found.");
			}
			return null;
		}
		return gameObjectMap[instanceId];
	}

	public SerializableGuid GetAssetIdFromInstanceId(SerializableGuid instanceId)
	{
		return assetDefinitionMap[instanceId];
	}

	public bool TryGetAssetDefinitionFromInstanceId(SerializableGuid instanceId, out SerializableGuid assetId)
	{
		return assetDefinitionMap.TryGetValue(instanceId, out assetId);
	}

	private bool IsPositionInBounds(Vector3Int coordinate)
	{
		if (maximumExtents.x != int.MinValue && minimumExtents.x != int.MaxValue && (Mathf.Abs(maximumExtents.x - coordinate.x) > maxExtents.x || Mathf.Abs(coordinate.x - minimumExtents.x) > maxExtents.x))
		{
			return false;
		}
		if (maximumExtents.z != int.MinValue && minimumExtents.z != int.MaxValue && (Mathf.Abs(maximumExtents.z - coordinate.z) > maxExtents.z || Mathf.Abs(coordinate.z - minimumExtents.z) > maxExtents.z))
		{
			return false;
		}
		if (maximumExtents.y != int.MinValue && minimumExtents.y != int.MaxValue && (Mathf.Abs(maximumExtents.y - coordinate.y) > maxExtents.y || Mathf.Abs(coordinate.y - minimumExtents.y) > maxExtents.y))
		{
			return false;
		}
		return true;
	}

	private void UpdateBoundaries(Cell newCell)
	{
		if (minimumExtents.x == int.MaxValue || minimumExtents.x > newCell.Coordinate.x)
		{
			minimumExtents.x = newCell.Coordinate.x;
		}
		if (maximumExtents.x == int.MinValue || maximumExtents.x < newCell.Coordinate.x)
		{
			maximumExtents.x = newCell.Coordinate.x;
		}
		if (maximumExtents.z == int.MinValue || maximumExtents.z < newCell.Coordinate.z)
		{
			maximumExtents.z = newCell.Coordinate.z;
		}
		if (minimumExtents.z == int.MaxValue || minimumExtents.z > newCell.Coordinate.z)
		{
			minimumExtents.z = newCell.Coordinate.z;
		}
		if (maximumExtents.y == int.MinValue || maximumExtents.y < newCell.Coordinate.y)
		{
			maximumExtents.y = newCell.Coordinate.y;
		}
		if (minimumExtents.y == int.MaxValue || minimumExtents.y > newCell.Coordinate.y)
		{
			minimumExtents.y = newCell.Coordinate.y;
		}
	}

	private bool ResetStageBoundaryIfNecessary(Cell cell)
	{
		bool result = false;
		if (minimumExtents.x == cell.Coordinate.x)
		{
			minimumExtents.x = int.MaxValue;
			result = true;
		}
		if (maximumExtents.x == cell.Coordinate.x)
		{
			maximumExtents.x = int.MinValue;
			result = true;
		}
		if (maximumExtents.z == cell.Coordinate.z)
		{
			maximumExtents.z = int.MinValue;
			result = true;
		}
		if (minimumExtents.z == cell.Coordinate.z)
		{
			minimumExtents.z = int.MaxValue;
			result = true;
		}
		if (maximumExtents.y == cell.Coordinate.y)
		{
			maximumExtents.y = int.MinValue;
			result = true;
		}
		if (minimumExtents.y == cell.Coordinate.y)
		{
			minimumExtents.y = int.MaxValue;
			result = true;
		}
		return result;
	}

	[ServerRpc(RequireOwnership = false)]
	public void AddInstanceIdsToLevelState_ServerRpc(SerializableGuid[] instanceIds)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			ServerRpcParams serverRpcParams = default(ServerRpcParams);
			FastBufferWriter bufferWriter = __beginSendServerRpc(1781947305u, serverRpcParams, RpcDelivery.Reliable);
			bool value = instanceIds != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(instanceIds, default(FastBufferWriter.ForNetworkSerializable));
			}
			__endSendServerRpc(ref bufferWriter, 1781947305u, serverRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			AddInstanceIdsToLevelState_ClientRpc(instanceIds);
		}
	}

	[ClientRpc]
	private void AddInstanceIdsToLevelState_ClientRpc(SerializableGuid[] instanceIds)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(1366621623u, clientRpcParams, RpcDelivery.Reliable);
			bool value = instanceIds != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(instanceIds, default(FastBufferWriter.ForNetworkSerializable));
			}
			__endSendClientRpc(ref bufferWriter, 1366621623u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			foreach (SerializableGuid instanceId in instanceIds)
			{
				AddObjectToLevelState(GetGameObjectFromInstanceId(instanceId), MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary[GetAssetIdFromInstanceId(instanceId)], instanceId);
			}
		}
	}

	public void RegisterDepthPlane(DepthPlane newDepthPlane)
	{
		if (depthPlane == null)
		{
			depthPlane = newDepthPlane;
		}
		else
		{
			UnityEngine.Debug.LogWarning("Registering a depth plane when one already exists!");
		}
	}

	public void UnregisterDepthPlane(DepthPlane targetDepthPlane)
	{
		if (depthPlane == targetDepthPlane)
		{
			depthPlane = null;
		}
	}

	public void SetDefaultEnvironment(SerializableGuid instanceId)
	{
		LevelState.SetDefaultEnvironment(instanceId);
	}

	public void ClearDefaultEnvironment()
	{
		levelState.ClearDefaultEnvironment();
	}

	public void MoveProp(SerializableGuid assetId, SerializableGuid instanceId, UnityEngine.Vector3 position, UnityEngine.Vector3 eulerAngles)
	{
		ReleaseCellsForProp(instanceId, assetId);
		LevelState.UpdatePropPositionAndRotation(instanceId, position, Quaternion.Euler(eulerAngles));
		AdjustCellsForObject(assetId, instanceId, position, eulerAngles);
	}

	private void AdjustCellsForObject(SerializableGuid movedObjectAssetId, SerializableGuid instanceId, UnityEngine.Vector3 position, UnityEngine.Vector3 eulerAngles)
	{
		PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary[movedObjectAssetId];
		PropEntry propEntry = levelState.GetPropEntry(instanceId);
		ClaimCellsForProp(runtimePropInfo, propEntry.Position, propEntry.Rotation, instanceId);
	}

	public void ReleaseCellsForProp(SerializableGuid instanceId, SerializableGuid assetId)
	{
		UnityEngine.Debug.Log(string.Format("{0}: Releasing Id: {1}, Asset Id: {2}", "ReleaseCellsForProp", instanceId, assetId));
		GameObject gameObjectFromInstanceId = GetGameObjectFromInstanceId(instanceId);
		if (!gameObjectFromInstanceId)
		{
			UnityEngine.Debug.LogException(new Exception($"Releasing cells for Prop: {instanceId}, Asset Id: {assetId} but the objectInstance was invalid"));
		}
		if (!MonoBehaviourSingleton<StageManager>.Instance)
		{
			UnityEngine.Debug.LogException(new Exception($"Releasing cells for Prop: {instanceId}, Asset Id: {assetId} but the StageManager.Instance is invalid"));
		}
		PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(assetId);
		if (runtimePropInfo == null)
		{
			UnityEngine.Debug.LogException(new Exception($"Releasing cells for Prop: {instanceId}, Asset Id: {assetId} but the definition was invalid"));
			return;
		}
		Vector3Int[] coordinates = (from x in GetRotatedPropLocations(runtimePropInfo.PropData, gameObjectFromInstanceId.transform.position, gameObjectFromInstanceId.transform.rotation)
			select x.Offset).ToArray();
		EraseCells(coordinates);
	}

	public void PrepForGameplay()
	{
		ApplyMemberChanges();
		if (base.IsServer)
		{
			ApplyWiring();
		}
	}

	public void ApplyMemberChanges()
	{
		foreach (PropEntry propEntry in MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.PropEntries)
		{
			if (!MonoBehaviourSingleton<StageManager>.Instance.IsPropDestroyed(propEntry.InstanceId) && MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(propEntry.AssetId, out var metadata) && !metadata.IsMissingObject)
			{
				try
				{
					ApplyMemberChanges(propEntry);
				}
				catch (Exception exception)
				{
					UnityEngine.Debug.LogException(exception);
				}
			}
		}
	}

	public static void ApplyMemberChanges(PropEntry propEntry)
	{
		GameObject gameObjectFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(propEntry.InstanceId);
		if (gameObjectFromInstanceId != null)
		{
			foreach (ComponentEntry componentEntry in propEntry.ComponentEntries)
			{
				Type type = Type.GetType(componentEntry.AssemblyQualifiedName);
				if ((object)type != null)
				{
					Component componentInChildren = gameObjectFromInstanceId.GetComponentInChildren(type);
					if ((bool)componentInChildren)
					{
						foreach (MemberChange change in componentEntry.Changes)
						{
							try
							{
								MemberInfo[] member = componentInChildren.GetType().GetMember(change.MemberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
								if (member.Length != 0)
								{
									if (member[0].GetCustomAttribute<EndlessNonSerializedAttribute>() == null)
									{
										member[0].SetValue(componentInChildren, change.ToObject());
									}
								}
								else
								{
									UnityEngine.Debug.LogException(new Exception("Member " + change.MemberName + " not found on type " + componentInChildren.GetType().Name));
								}
							}
							catch (Exception innerException)
							{
								UnityEngine.Debug.LogException(new Exception("Error applying Member " + change.MemberName + " on type " + componentInChildren.GetType().Name, innerException));
							}
						}
					}
				}
			}
			return;
		}
		UnityEngine.Debug.LogWarning("Applying member changes to a non-destroyed, but still missing prop. Known Issue.");
	}

	private void ApplyWiring()
	{
		List<WireBundle> list = new List<WireBundle>();
		foreach (WireBundle wireBundle in MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.WireBundles)
		{
			if (MonoBehaviourSingleton<StageManager>.Instance.IsPropDestroyed(wireBundle.EmitterInstanceId) || MonoBehaviourSingleton<StageManager>.Instance.IsPropDestroyed(wireBundle.ReceiverInstanceId))
			{
				continue;
			}
			SerializableGuid value2;
			if (!assetDefinitionMap.TryGetValue(wireBundle.EmitterInstanceId, out var value))
			{
				UnityEngine.Debug.LogError($"While applying wiring, we could not find the asset definition for emitter asset id: {wireBundle.EmitterInstanceId}");
			}
			else if (!assetDefinitionMap.TryGetValue(wireBundle.ReceiverInstanceId, out value2))
			{
				UnityEngine.Debug.LogError($"While applying wiring, we could not find the asset definition for receiver asset id: {wireBundle.EmitterInstanceId}");
			}
			else
			{
				if (!MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(value, out var metadata) || !MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(value2, out var metadata2) || metadata.IsMissingObject || metadata2.IsMissingObject)
				{
					continue;
				}
				GameObject gameObjectFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(wireBundle.EmitterInstanceId);
				GameObject gameObjectFromInstanceId2 = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(wireBundle.ReceiverInstanceId);
				if (gameObjectFromInstanceId == null)
				{
					UnityEngine.Debug.LogException(new Exception("Attempting to apply wiring from a non-existent object."));
					continue;
				}
				if (gameObjectFromInstanceId2 == null)
				{
					UnityEngine.Debug.LogException(new Exception("Attempting to apply wiring to a non-existent object."));
					continue;
				}
				list.Add(wireBundle);
				foreach (WireEntry wire in wireBundle.Wires)
				{
					bool flag = string.IsNullOrEmpty(wire.EmitterComponentAssemblyQualifiedTypeName);
					Type type;
					if (flag)
					{
						type = typeof(EndlessScriptComponent);
					}
					else
					{
						type = Type.GetType(wire.EmitterComponentAssemblyQualifiedTypeName);
						if ((object)type == null)
						{
							continue;
						}
					}
					Component componentFromMap = GetComponentFromMap(wireBundle.EmitterInstanceId, type, gameObjectFromInstanceId);
					if (componentFromMap == null)
					{
						UnityEngine.Debug.LogException(new Exception("Attempting to apply wiring from a non-existent component. Known issue."));
						continue;
					}
					if (flag)
					{
						(componentFromMap as EndlessScriptComponent).ClearEndlessEventObjects();
						continue;
					}
					MemberInfo[] member = type.GetMember(wire.EmitterMemberName, BindingFlags.Instance | BindingFlags.Public);
					if (member.Length != 0)
					{
						object value3 = Activator.CreateInstance((member[0] as FieldInfo).FieldType);
						member[0].SetValue(componentFromMap, value3);
					}
				}
			}
		}
		foreach (WireBundle item in list)
		{
			GameObject gameObjectFromInstanceId3 = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(item.EmitterInstanceId);
			GameObject gameObjectFromInstanceId4 = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(item.ReceiverInstanceId);
			foreach (WireEntry wire2 in item.Wires)
			{
				try
				{
					ApplyGameEditorEvents(item, wire2, gameObjectFromInstanceId3, gameObjectFromInstanceId4);
				}
				catch (Exception innerException)
				{
					UnityEngine.Debug.LogException(new Exception($"Failed to apply wire. {wire2}", innerException));
				}
			}
		}
	}

	private void ApplyGameEditorEvents(WireBundle wireBundle, WireEntry wireEntry, GameObject emitterObject, GameObject receiverObject)
	{
		bool num = string.IsNullOrEmpty(wireEntry.EmitterComponentAssemblyQualifiedTypeName);
		Type type = ((!num) ? Type.GetType(wireEntry.EmitterComponentAssemblyQualifiedTypeName) : typeof(EndlessScriptComponent));
		bool flag = string.IsNullOrEmpty(wireEntry.ReceiverComponentAssemblyQualifiedTypeName);
		Type type2 = ((!flag) ? Type.GetType(wireEntry.ReceiverComponentAssemblyQualifiedTypeName) : typeof(EndlessScriptComponent));
		Component componentFromMap = GetComponentFromMap(wireBundle.EmitterInstanceId, type, emitterObject);
		Component componentFromMap2 = GetComponentFromMap(wireBundle.ReceiverInstanceId, type2, receiverObject);
		object obj;
		Type[] array;
		if (num)
		{
			EndlessScriptComponent endlessScriptComponent = componentFromMap as EndlessScriptComponent;
			obj = endlessScriptComponent.GetEventObject(wireEntry.EmitterMemberName);
			array = endlessScriptComponent.GetEventInfo(wireEntry.EmitterMemberName).ParamList.Select((EndlessParameterInfo a) => EndlessTypeMapping.Instance.GetTypeFromId(a.DataType)).ToArray();
			if (obj == null)
			{
				MethodInfo[] methods = typeof(WireBindings).GetMethods(BindingFlags.Static | BindingFlags.NonPublic);
				foreach (MethodInfo methodInfo in methods)
				{
					if (methodInfo.Name == WireBindings.MAKE_ENDLESS_EVENT_NAME)
					{
						Type[] genericArguments = methodInfo.GetGenericArguments();
						if (array.Length == genericArguments.Length)
						{
							obj = ((array.Length != 0) ? methodInfo.MakeGenericMethod(array.ToArray()).Invoke(null, null) : methodInfo.Invoke(null, null));
							break;
						}
					}
				}
			}
			if (obj == null)
			{
				UnityEngine.Debug.LogError("Failed to create endless event for a lua event");
				return;
			}
			endlessScriptComponent.AddEndlessEventObject(obj, wireEntry.EmitterMemberName);
		}
		else
		{
			MemberInfo[] member = type.GetMember(wireEntry.EmitterMemberName, BindingFlags.Instance | BindingFlags.Public);
			array = (member[0] as FieldInfo).FieldType.GenericTypeArguments;
			obj = member[0].GetValue(componentFromMap);
		}
		array = ((array != null && array.Length != 0) ? new Type[1] { typeof(Context) }.Concat(array).ToArray() : new Type[1] { typeof(Context) });
		bool flag2 = false;
		if (flag)
		{
			EndlessScriptComponent endlessScriptComponent2 = componentFromMap2 as EndlessScriptComponent;
			int count = endlessScriptComponent2.Script.Receivers.First((EndlessEventInfo r) => r.MemberName == wireEntry.ReceiverMemberName).ParamList.Count;
			MethodInfo[] methods = typeof(WireBindings).GetMethods(BindingFlags.Static | BindingFlags.NonPublic);
			foreach (MethodInfo methodInfo2 in methods)
			{
				if (!(methodInfo2.Name == WireBindings.BIND_LUA_NAME))
				{
					continue;
				}
				Type[] genericArguments2 = methodInfo2.GetGenericArguments();
				if (array.Length == genericArguments2.Length)
				{
					if (array.Length == 0)
					{
						methodInfo2.Invoke(null, new object[5] { obj, wireEntry.ReceiverMemberName, endlessScriptComponent2, count, wireEntry.StaticParameters });
						flag2 = true;
					}
					else
					{
						methodInfo2.MakeGenericMethod(array).Invoke(null, new object[5] { obj, wireEntry.ReceiverMemberName, endlessScriptComponent2, count, wireEntry.StaticParameters });
						flag2 = true;
					}
					break;
				}
			}
		}
		else
		{
			MethodInfo methodInfo3 = null;
			try
			{
				if (wireEntry.StaticParameters != null && wireEntry.StaticParameters.Length != 0)
				{
					Type[] types = new Type[1] { typeof(Context) }.Concat(wireEntry.StaticParameters.Select((StoredParameter param) => EndlessTypeMapping.Instance.GetTypeFromId(param.DataType))).ToArray();
					methodInfo3 = type2.GetMethod(wireEntry.ReceiverMemberName, BindingFlags.Instance | BindingFlags.Public, null, types, null);
				}
				else
				{
					if (array.Length != 0)
					{
						methodInfo3 = type2.GetMethod(wireEntry.ReceiverMemberName, BindingFlags.Instance | BindingFlags.Public, null, array, null);
					}
					if (methodInfo3 == null)
					{
						methodInfo3 = type2.GetMethod(wireEntry.ReceiverMemberName, BindingFlags.Instance | BindingFlags.Public, null, new Type[1] { typeof(Context) }, null);
					}
				}
				if (methodInfo3 == null)
				{
					throw new NullReferenceException();
				}
			}
			catch (Exception innerException)
			{
				UnityEngine.Debug.LogException(new Exception("Something went wrong targeting " + wireEntry.ReceiverComponentAssemblyQualifiedTypeName + ", " + wireEntry.ReceiverMemberName, innerException));
				return;
			}
			MethodInfo[] methods = typeof(WireBindings).GetMethods(BindingFlags.Static | BindingFlags.NonPublic);
			foreach (MethodInfo methodInfo4 in methods)
			{
				if (!(methodInfo4.Name == WireBindings.PROCESS_NAME))
				{
					continue;
				}
				Type[] genericArguments3 = methodInfo4.GetGenericArguments();
				if (array.Length == genericArguments3.Length)
				{
					if (array.Length == 0)
					{
						methodInfo4.Invoke(null, new object[4] { obj, methodInfo3, componentFromMap2, wireEntry.StaticParameters });
						flag2 = true;
					}
					else
					{
						methodInfo4.MakeGenericMethod(array).Invoke(null, new object[4] { obj, methodInfo3, componentFromMap2, wireEntry.StaticParameters });
						flag2 = true;
					}
					break;
				}
			}
		}
		if (!flag2)
		{
			UnityEngine.Debug.LogError("Failed to find event to process wire");
		}
	}

	private Component GetComponentFromMap(SerializableGuid propInstanceId, Type componentType, GameObject sourceObject)
	{
		if (!propComponentMap.ContainsKey(propInstanceId))
		{
			propComponentMap.Add(propInstanceId, new Dictionary<Type, Component>());
		}
		if (!propComponentMap[propInstanceId].ContainsKey(componentType))
		{
			propComponentMap[propInstanceId].Add(componentType, sourceObject.GetComponentInChildren(componentType));
		}
		return propComponentMap[propInstanceId][componentType];
	}

	public Tileset TilesetAtIndex(int tilesetIndex)
	{
		return RuntimePalette.GetTileset(tilesetIndex);
	}

	public async Task RespawnTerrainCellsWithTilesetIndexes(List<int> tilesetsToRespawn, CancellationToken cancelToken)
	{
		Stopwatch frameStopWatch = new Stopwatch();
		frameStopWatch.Start();
		Vector3Int[] array = offlineStage.GetCellCoordinates().ToArray();
		Vector3Int[] array2 = array;
		foreach (Vector3Int coordinate in array2)
		{
			if (offlineStage.TryGetCellFromCoordinate(coordinate, out var result))
			{
				if (tilesetsToRespawn.Contains(result.TilesetIndex))
				{
					SpawnTile(RuntimePalette.GetTileset(result.TilesetIndex), result as TerrainCell, addToChunkManager: false);
					offlineStage.ChunkManager.CellUpdated(result.Coordinate);
				}
				if (frameStopWatch.ElapsedMilliseconds > 4)
				{
					await Task.Yield();
					frameStopWatch.Restart();
				}
			}
		}
	}

	public IEnumerator RespawnTerrainCellsWithTilesetIndex(int tilesetIndexToRespawn, Action callback)
	{
		Tileset tileset = RuntimePalette.GetTileset(tilesetIndexToRespawn);
		Stopwatch frameStopWatch = new Stopwatch();
		frameStopWatch.Start();
		Vector3Int[] array = offlineStage.GetCellCoordinates().ToArray();
		Vector3Int[] array2 = array;
		foreach (Vector3Int coordinate in array2)
		{
			if (offlineStage.TryGetCellFromCoordinate(coordinate, out var result))
			{
				Cell cell = result;
				if (cell.TilesetIndex == tilesetIndexToRespawn)
				{
					SpawnTile(tileset, cell as TerrainCell, addToChunkManager: false);
					offlineStage.ChunkManager.CellUpdated(cell.Coordinate);
				}
				if (frameStopWatch.ElapsedMilliseconds > 4)
				{
					yield return null;
					frameStopWatch.Restart();
				}
			}
		}
		callback?.Invoke();
	}

	public void PropDestroyed(SerializableGuid instanceId)
	{
	}

	public List<PropEntry> GetReferenceFilteredPropEntries(ReferenceFilter filter)
	{
		List<PropEntry> list = new List<PropEntry>();
		List<string> list2 = (from d in MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetReferenceFilteredDefinitionList(filter)
			select d.PropData.AssetID).ToList();
		foreach (PropEntry propEntry in LevelState.PropEntries)
		{
			if (list2.Contains(propEntry.AssetId))
			{
				list.Add(propEntry);
			}
		}
		return list;
	}

	public void RespawnPropsWithAssetId(SerializableGuid assetId, bool wasMissingObject)
	{
		if (!MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(assetId, out var metadata))
		{
			UnityEngine.Debug.LogError($"We expected to find a metadata object for asset id ({assetId}) but didn't find it!");
			return;
		}
		foreach (PropEntry propEntry in levelState.PropEntries)
		{
			if (propEntry.AssetId != assetId)
			{
				continue;
			}
			SerializableGuid instanceId = propEntry.InstanceId;
			GameObject gameObjectFromInstanceId = GetGameObjectFromInstanceId(instanceId);
			ulong num = 0uL;
			if (base.IsServer && metadata.EndlessProp.IsNetworked && !wasMissingObject)
			{
				num = gameObjectFromInstanceId.GetComponent<NetworkObject>().NetworkObjectId;
				reverseGameObjectMap.Remove(gameObjectFromInstanceId);
			}
			DestroyStageObject(instanceId, metadata, wasMissingObject);
			if (metadata.EndlessProp.IsNetworked && !wasMissingObject)
			{
				if (base.IsServer)
				{
					pendingNetworkIds.Remove(num);
					gameObjectMap.Remove(instanceId);
					assetDefinitionMap.Remove(instanceId);
					ForceUntrackNetworkObject_ClientRpc(instanceId, num, wasMissingObject);
				}
				else if (gameObjectFromInstanceId != null)
				{
					PendingNetworkIdInfo pendingNetworkIdInfo = pendingNetworkIds.Values.FirstOrDefault((PendingNetworkIdInfo pending) => pending.InstanceId == instanceId);
					if (pendingNetworkIdInfo != null)
					{
						UnityEngine.Debug.Log("RespawnPropsWithAssetId:: Pending Network Info Existed. Calling TrackNetworkObject");
						TrackNetworkObject(pendingNetworkIdInfo.NetworkId, gameObjectFromInstanceId);
					}
				}
			}
			try
			{
				if (metadata.EndlessProp.IsNetworked)
				{
					if (base.IsServer)
					{
						GameObject gameObject = UnityEngine.Object.Instantiate(metadata.EndlessProp, propEntry.Position, propEntry.Rotation).gameObject;
						NetworkObject component = gameObject.GetComponent<NetworkObject>();
						if (component != null)
						{
							component.Spawn();
							TrackPendingNetworkId(component.NetworkObjectId, propEntry.InstanceId, updateLevelState: false);
						}
						TrackNonNetworkedObject(propEntry.AssetId, propEntry.InstanceId, gameObject.gameObject, updateLevelState: false);
						TrackPendingNetworkObject_ClientRpc(assetId, instanceId, component.NetworkObjectId, updateLevelState: false);
					}
				}
				else
				{
					GameObject newObject = UnityEngine.Object.Instantiate(metadata.EndlessProp, propEntry.Position, propEntry.Rotation).gameObject;
					TrackNonNetworkedObject(propEntry.AssetId, propEntry.InstanceId, newObject, updateLevelState: false);
				}
			}
			catch (MissingReferenceException)
			{
				MonoBehaviourSingleton<StageManager>.Instance.PropFailedToLoad(propEntry);
			}
		}
	}

	[ClientRpc]
	private void ForceUntrackNetworkObject_ClientRpc(SerializableGuid instanceId, ulong networkObjectId, bool wasMissingObject)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(2409622067u, clientRpcParams, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in instanceId, default(FastBufferWriter.ForNetworkSerializable));
			BytePacker.WriteValueBitPacked(bufferWriter, networkObjectId);
			bufferWriter.WriteValueSafe(in wasMissingObject, default(FastBufferWriter.ForPrimitives));
			__endSendClientRpc(ref bufferWriter, 2409622067u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
		{
			return;
		}
		__rpc_exec_stage = __RpcExecStage.Send;
		GameObject gameObjectFromInstanceId = GetGameObjectFromInstanceId(instanceId);
		if (wasMissingObject || !(gameObjectFromInstanceId != null) || !gameObjectFromInstanceId.TryGetComponent<NetworkObject>(out var component) || component.NetworkObjectId == networkObjectId)
		{
			pendingNetworkIds.Remove(networkObjectId);
			gameObjectMap.Remove(instanceId);
			assetDefinitionMap.Remove(instanceId);
			if (gameObjectFromInstanceId != null)
			{
				reverseGameObjectMap.Remove(gameObjectFromInstanceId);
			}
		}
	}

	public void ValidatePhysicalWiresStillAlive()
	{
		for (int num = physicalWires.Count - 1; num >= 0; num--)
		{
			bool flag = false;
			for (int i = 0; i < levelState.WireBundles.Count; i++)
			{
				if (!(levelState.WireBundles[i].BundleId != physicalWires[num].BundleId))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				UnityEngine.Object.Destroy(physicalWires[num].gameObject);
				physicalWires.RemoveAt(num);
			}
		}
	}

	private void DestroyStageObject(SerializableGuid instanceId, PropLibrary.RuntimePropInfo propInfo, bool wasMissingObject)
	{
		PropEntry propEntry = LevelState.GetPropEntry(instanceId);
		UnityEngine.Vector3 position = propEntry.Position;
		Quaternion rotation = propEntry.Rotation;
		Vector3Int[] coordinates = (from x in GetRotatedPropLocations(propInfo.PropData, position, rotation)
			select x.Offset).ToArray();
		for (int num = physicalWires.Count - 1; num >= 0; num--)
		{
			PhysicalWire physicalWire = physicalWires[num];
			if (physicalWire.EmitterId == instanceId || physicalWire.ReceiverId == instanceId)
			{
				UnityEngine.Object.Destroy(physicalWire.gameObject);
				physicalWires.RemoveAt(num);
			}
		}
		if (propToWireConnectionPoints.ContainsKey(instanceId))
		{
			Transform[] array = propToWireConnectionPoints[instanceId];
			foreach (Transform transform in array)
			{
				if ((bool)transform)
				{
					UnityEngine.Object.Destroy(transform.gameObject);
				}
			}
			propToWireConnectionPoints.Remove(instanceId);
		}
		GameObject gameObjectFromInstanceId = GetGameObjectFromInstanceId(instanceId);
		if (!propInfo.EndlessProp.IsNetworked || wasMissingObject || base.IsServer)
		{
			gameObjectMap.Remove(instanceId);
			assetDefinitionMap.Remove(instanceId);
			if (gameObjectFromInstanceId != null)
			{
				reverseGameObjectMap.Remove(gameObjectFromInstanceId);
				UnityEngine.Object.Destroy(gameObjectFromInstanceId);
			}
		}
		EraseCells(coordinates);
	}

	public bool PlacementIsValid(Prop prop, UnityEngine.Vector3 position, Quaternion rotation)
	{
		Vector3Int[] array = (from x in GetRotatedPropLocations(prop, position, rotation)
			select x.Offset).ToArray();
		bool result = true;
		for (int num = 0; num < array.Length; num++)
		{
			if (!CheckValidity(array[num]))
			{
				result = false;
				break;
			}
			if (!IsPositionInBounds(array[num]))
			{
				result = false;
				break;
			}
		}
		return result;
	}

	public bool PlacementIsValidAllowingSelfOverlap(Prop prop, UnityEngine.Vector3 position, Quaternion rotation, SerializableGuid selfInstanceId)
	{
		Vector3Int[] array = (from x in GetRotatedPropLocations(prop, position, rotation)
			select x.Offset).ToArray();
		bool result = true;
		for (int num = 0; num < array.Length; num++)
		{
			Cell cellFromCoordinate = GetCellFromCoordinate(array[num]);
			if (cellFromCoordinate != null && (!(cellFromCoordinate is PropCell propCell) || propCell.InstanceId != selfInstanceId))
			{
				result = false;
				break;
			}
			if (!IsPositionInBounds(array[num]))
			{
				result = false;
				break;
			}
		}
		return result;
	}

	public PropLocationOffset[] GetRotatedPropLocations(Prop prop, UnityEngine.Vector3 position, Quaternion rotation)
	{
		if (prop.PropLocationOffsets == null)
		{
			return new PropLocationOffset[1]
			{
				new PropLocationOffset
				{
					Offset = Vector3Int.zero
				}
			};
		}
		PropLocationOffset[] array = new PropLocationOffset[prop.PropLocationOffsets.Count];
		UnityEngine.Vector3 vector = new UnityEngine.Vector3(prop.ApplyXOffset ? 0.5f : 0f, 0f, prop.ApplyZOffset ? 0.5f : 0f);
		for (int i = 0; i < array.Length; i++)
		{
			UnityEngine.Vector3 vector2 = prop.PropLocationOffsets.ElementAt(i).Offset;
			vector2.x += vector.x;
			vector2.y += vector.y;
			vector2.z += vector.z;
			UnityEngine.Vector3 worldSpacePoint = rotation * vector2 + position;
			array[i] = new PropLocationOffset
			{
				Offset = WorldSpacePointToGridCoordinate(worldSpacePoint)
			};
		}
		return array;
	}

	public void ConfigureIndicatorTransformToProp(Prop prop, Transform targetIndicator, SerializableGuid propInstanceId)
	{
		if (propInstanceId == SerializableGuid.Empty)
		{
			targetIndicator.rotation = Quaternion.identity;
			targetIndicator.transform.position = UnityEngine.Vector3.zero;
			targetIndicator.GetChild(0).localScale = UnityEngine.Vector3.zero;
			return;
		}
		MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetAssetIdFromInstanceId(propInstanceId);
		MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(propInstanceId).transform.GetPositionAndRotation(out var position, out var rotation);
		Vector3Int boundingSize = prop.GetBoundingSize();
		Vector3Int[] cells = (from x in GetRotatedPropLocations(prop, position, rotation)
			select x.Offset).ToArray();
		UnityEngine.Vector3 vector = GetMinimumPosition(cells);
		UnityEngine.Vector3 vector2 = GetMaximumPosition(cells);
		UnityEngine.Vector3 position2 = (vector + vector2) * 0.5f;
		targetIndicator.gameObject.SetActive(value: true);
		targetIndicator.rotation = rotation;
		targetIndicator.transform.position = position2;
		targetIndicator.GetChild(0).localScale = boundingSize;
		static UnityEngine.Vector3 GetMaximumPosition(Vector3Int[] array)
		{
			Vector3Int vector3Int = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);
			for (int i = 0; i < array.Length; i++)
			{
				Vector3Int vector3Int2 = array[i];
				if (vector3Int2.x > vector3Int.x || vector3Int2.y > vector3Int.y || vector3Int2.z > vector3Int.z)
				{
					vector3Int = vector3Int2;
				}
			}
			return vector3Int;
		}
		static UnityEngine.Vector3 GetMinimumPosition(Vector3Int[] array)
		{
			Vector3Int vector3Int = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);
			for (int i = 0; i < array.Length; i++)
			{
				Vector3Int vector3Int2 = array[i];
				if (vector3Int2.x < vector3Int.x || vector3Int2.y < vector3Int.y || vector3Int2.z < vector3Int.z)
				{
					vector3Int = vector3Int2;
				}
			}
			return vector3Int;
		}
	}

	public bool TryGetCellFromCoordinate<T>(Vector3Int intersectedObjectPosition, out T propCell) where T : Cell
	{
		propCell = GetCellFromCoordinateAs<T>(intersectedObjectPosition);
		return propCell != null;
	}

	public void NetworkSetDefaultEnvironment(SerializableGuid instanceId)
	{
		SetDefaultEnvironment_ClientRpc(instanceId);
	}

	[ClientRpc]
	private void SetDefaultEnvironment_ClientRpc(SerializableGuid instanceId)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams = default(ClientRpcParams);
				FastBufferWriter bufferWriter = __beginSendClientRpc(2206152673u, clientRpcParams, RpcDelivery.Reliable);
				bufferWriter.WriteValueSafe(in instanceId, default(FastBufferWriter.ForNetworkSerializable));
				__endSendClientRpc(ref bufferWriter, 2206152673u, clientRpcParams, RpcDelivery.Reliable);
			}
			if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				__rpc_exec_stage = __RpcExecStage.Send;
				MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.SetDefaultEnvironment(instanceId);
			}
		}
	}

	public void UpdateVersion(string assetVersion)
	{
		UpdateVersionLocal(assetVersion);
		UpdateVersion_ClientRpc(assetVersion);
	}

	[ClientRpc]
	private void UpdateVersion_ClientRpc(string assetVersion)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(687058185u, clientRpcParams, RpcDelivery.Reliable);
			bool value = assetVersion != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(assetVersion);
			}
			__endSendClientRpc(ref bufferWriter, 687058185u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if (!base.IsServer)
			{
				UpdateVersionLocal(assetVersion);
			}
		}
	}

	private void UpdateVersionLocal(string assetVersion)
	{
		if (levelState != null)
		{
			AssetReference oldReference = levelState.ToAssetReference();
			levelState.AssetVersion = assetVersion;
			MonoBehaviourSingleton<StageManager>.Instance.UpdateStageVersion(oldReference, levelState.ToAssetReference());
		}
	}

	public static void DebugDrawCell(Vector3Int cellPosition, UnityEngine.Color color, float duration)
	{
		QuadPlane[] cellFaces = CellFaces;
		for (int i = 0; i < cellFaces.Length; i++)
		{
			cellFaces[i].DebugDraw(cellPosition, color, duration);
		}
	}

	protected override void __initializeVariables()
	{
		if (mapId == null)
		{
			throw new Exception("Stage.mapId cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		mapId.Initialize(this);
		__nameNetworkVariable(mapId, "mapId");
		NetworkVariableFields.Add(mapId);
		if (serverPropLoadFinished == null)
		{
			throw new Exception("Stage.serverPropLoadFinished cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		serverPropLoadFinished.Initialize(this);
		__nameNetworkVariable(serverPropLoadFinished, "serverPropLoadFinished");
		NetworkVariableFields.Add(serverPropLoadFinished);
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		__registerRpc(2231079621u, __rpc_handler_2231079621, "SendNetworkIds_ClientRpc");
		__registerRpc(667828874u, __rpc_handler_667828874, "TrackPendingNetworkObject_ClientRpc");
		__registerRpc(1781947305u, __rpc_handler_1781947305, "AddInstanceIdsToLevelState_ServerRpc");
		__registerRpc(1366621623u, __rpc_handler_1366621623, "AddInstanceIdsToLevelState_ClientRpc");
		__registerRpc(2409622067u, __rpc_handler_2409622067, "ForceUntrackNetworkObject_ClientRpc");
		__registerRpc(2206152673u, __rpc_handler_2206152673, "SetDefaultEnvironment_ClientRpc");
		__registerRpc(687058185u, __rpc_handler_687058185, "UpdateVersion_ClientRpc");
		base.__initializeRpcs();
	}

	private static void __rpc_handler_2231079621(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			ulong[] value2 = null;
			if (value)
			{
				reader.ReadValueSafe(out value2, default(FastBufferWriter.ForPrimitives));
			}
			reader.ReadValueSafe(out bool value3, default(FastBufferWriter.ForPrimitives));
			SerializableGuid[] value4 = null;
			if (value3)
			{
				reader.ReadValueSafe(out value4, default(FastBufferWriter.ForNetworkSerializable));
			}
			reader.ReadValueSafe(out bool value5, default(FastBufferWriter.ForPrimitives));
			SerializableGuid[] value6 = null;
			if (value5)
			{
				reader.ReadValueSafe(out value6, default(FastBufferWriter.ForNetworkSerializable));
			}
			ClientRpcParams client = rpcParams.Client;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((Stage)target).SendNetworkIds_ClientRpc(value2, value4, value6, client);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_667828874(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out SerializableGuid value, default(FastBufferWriter.ForNetworkSerializable));
			reader.ReadValueSafe(out SerializableGuid value2, default(FastBufferWriter.ForNetworkSerializable));
			ByteUnpacker.ReadValueBitPacked(reader, out ulong value3);
			reader.ReadValueSafe(out bool value4, default(FastBufferWriter.ForPrimitives));
			ClientRpcParams client = rpcParams.Client;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((Stage)target).TrackPendingNetworkObject_ClientRpc(value, value2, value3, value4, client);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_1781947305(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			SerializableGuid[] value2 = null;
			if (value)
			{
				reader.ReadValueSafe(out value2, default(FastBufferWriter.ForNetworkSerializable));
			}
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((Stage)target).AddInstanceIdsToLevelState_ServerRpc(value2);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_1366621623(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			SerializableGuid[] value2 = null;
			if (value)
			{
				reader.ReadValueSafe(out value2, default(FastBufferWriter.ForNetworkSerializable));
			}
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((Stage)target).AddInstanceIdsToLevelState_ClientRpc(value2);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_2409622067(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out SerializableGuid value, default(FastBufferWriter.ForNetworkSerializable));
			ByteUnpacker.ReadValueBitPacked(reader, out ulong value2);
			reader.ReadValueSafe(out bool value3, default(FastBufferWriter.ForPrimitives));
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((Stage)target).ForceUntrackNetworkObject_ClientRpc(value, value2, value3);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_2206152673(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out SerializableGuid value, default(FastBufferWriter.ForNetworkSerializable));
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((Stage)target).SetDefaultEnvironment_ClientRpc(value);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_687058185(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			string s = null;
			if (value)
			{
				reader.ReadValueSafe(out s, oneByteChars: false);
			}
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((Stage)target).UpdateVersion_ClientRpc(s);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	protected internal override string __getTypeName()
	{
		return "Stage";
	}
}
