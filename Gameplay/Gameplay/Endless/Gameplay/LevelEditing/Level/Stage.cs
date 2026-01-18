using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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

namespace Endless.Gameplay.LevelEditing.Level
{
	// Token: 0x02000567 RID: 1383
	public class Stage : NetworkBehaviour, ISerializationCallbackReceiver
	{
		// Token: 0x17000658 RID: 1624
		// (get) Token: 0x06002135 RID: 8501 RVA: 0x000954CD File Offset: 0x000936CD
		// (set) Token: 0x06002136 RID: 8502 RVA: 0x000954D5 File Offset: 0x000936D5
		public bool IsLoading { get; private set; }

		// Token: 0x17000659 RID: 1625
		// (get) Token: 0x06002137 RID: 8503 RVA: 0x000954DE File Offset: 0x000936DE
		// (set) Token: 0x06002138 RID: 8504 RVA: 0x000954E6 File Offset: 0x000936E6
		public bool HasLoaded { get; private set; }

		// Token: 0x1700065A RID: 1626
		// (get) Token: 0x06002139 RID: 8505 RVA: 0x000954EF File Offset: 0x000936EF
		public DepthPlane DepthPlane
		{
			get
			{
				return this.depthPlane;
			}
		}

		// Token: 0x1700065B RID: 1627
		// (get) Token: 0x0600213A RID: 8506 RVA: 0x000954F7 File Offset: 0x000936F7
		public Vector3Int MinimumExtents
		{
			get
			{
				return this.minimumExtents;
			}
		}

		// Token: 0x1700065C RID: 1628
		// (get) Token: 0x0600213B RID: 8507 RVA: 0x000954FF File Offset: 0x000936FF
		public Vector3Int MaximumExtents
		{
			get
			{
				return this.maximumExtents;
			}
		}

		// Token: 0x1700065D RID: 1629
		// (get) Token: 0x0600213C RID: 8508 RVA: 0x00095508 File Offset: 0x00093708
		public float StageFallOffHeight
		{
			get
			{
				if (MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying && this.depthPlane != null && this.depthPlane.OverrideFallOffHeight)
				{
					return this.depthPlane.GetFallOffHeight();
				}
				return (float)(this.MinimumExtents.y - 5);
			}
		}

		// Token: 0x1700065E RID: 1630
		// (get) Token: 0x0600213D RID: 8509 RVA: 0x0009555C File Offset: 0x0009375C
		public float StageResetHeight
		{
			get
			{
				return (float)(this.MinimumExtents.y - 5);
			}
		}

		// Token: 0x1700065F RID: 1631
		// (get) Token: 0x0600213E RID: 8510 RVA: 0x0009557A File Offset: 0x0009377A
		public LevelState LevelState
		{
			get
			{
				return this.levelState;
			}
		}

		// Token: 0x17000660 RID: 1632
		// (get) Token: 0x0600213F RID: 8511 RVA: 0x00095582 File Offset: 0x00093782
		public IReadOnlyList<Cell> Cells
		{
			get
			{
				return this.cellLookup.Values.Concat(this.offlineStage.GetCells()).ToList<Cell>();
			}
		}

		// Token: 0x17000661 RID: 1633
		// (get) Token: 0x06002140 RID: 8512 RVA: 0x000955A4 File Offset: 0x000937A4
		public SerializableGuid MapId
		{
			get
			{
				return this.mapId.Value;
			}
		}

		// Token: 0x17000662 RID: 1634
		// (get) Token: 0x06002141 RID: 8513 RVA: 0x000955B1 File Offset: 0x000937B1
		public List<PhysicalWire> PhysicalWires
		{
			get
			{
				return this.physicalWires;
			}
		}

		// Token: 0x17000663 RID: 1635
		// (get) Token: 0x06002142 RID: 8514 RVA: 0x000955B9 File Offset: 0x000937B9
		public ChunkManager ChunkManager
		{
			get
			{
				return this.offlineStage.ChunkManager;
			}
		}

		// Token: 0x17000664 RID: 1636
		// (get) Token: 0x06002143 RID: 8515 RVA: 0x000955C6 File Offset: 0x000937C6
		// (set) Token: 0x06002144 RID: 8516 RVA: 0x000955CE File Offset: 0x000937CE
		public bool PropsAreLoaded { get; private set; }

		// Token: 0x17000665 RID: 1637
		// (get) Token: 0x06002145 RID: 8517 RVA: 0x000955D7 File Offset: 0x000937D7
		// (set) Token: 0x06002146 RID: 8518 RVA: 0x000955DF File Offset: 0x000937DF
		public bool TerrainLoaded { get; private set; }

		// Token: 0x17000666 RID: 1638
		// (get) Token: 0x06002147 RID: 8519 RVA: 0x000955E8 File Offset: 0x000937E8
		// (set) Token: 0x06002148 RID: 8520 RVA: 0x000955F0 File Offset: 0x000937F0
		public RuntimePalette RuntimePalette { get; set; }

		// Token: 0x17000667 RID: 1639
		// (get) Token: 0x06002149 RID: 8521 RVA: 0x000955F9 File Offset: 0x000937F9
		// (set) Token: 0x0600214A RID: 8522 RVA: 0x00095601 File Offset: 0x00093801
		public BlockTokenCollection BlockTokenCollection { get; private set; }

		// Token: 0x0600214B RID: 8523 RVA: 0x0009560C File Offset: 0x0009380C
		public SerializableGuid GetInstanceIdFromGameObject(GameObject targetObject)
		{
			SerializableGuid serializableGuid;
			if (this.reverseGameObjectMap.TryGetValue(targetObject, out serializableGuid))
			{
				return serializableGuid;
			}
			return SerializableGuid.Empty;
		}

		// Token: 0x0600214C RID: 8524 RVA: 0x00095630 File Offset: 0x00093830
		public bool CanAddProp(PropLibrary.RuntimePropInfo metadata)
		{
			if (MonoBehaviourSingleton<StageManager>.Instance.PropRequirementLookup.ContainsKey(metadata.PropData.AssetID))
			{
				PropRequirement propRequirement = MonoBehaviourSingleton<StageManager>.Instance.PropRequirementLookup[metadata.PropData.AssetID];
				return this.assetDefinitionMap.Values.Count((SerializableGuid guid) => propRequirement.Guids.Contains(guid)) < propRequirement.MaxCount;
			}
			if (MonoBehaviourSingleton<StageManager>.Instance.BaseTypeRequirementLookup.ContainsKey(metadata.PropData.BaseTypeId))
			{
				BaseTypeRequirement baseTypeRequirement = MonoBehaviourSingleton<StageManager>.Instance.BaseTypeRequirementLookup[metadata.PropData.BaseTypeId];
				List<SerializableGuid> assetIds = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetBaseTypeList(baseTypeRequirement.Guids.ToArray());
				return this.assetDefinitionMap.Values.Count((SerializableGuid guid) => assetIds.Contains(guid)) < baseTypeRequirement.MaxCount;
			}
			return true;
		}

		// Token: 0x0600214D RID: 8525 RVA: 0x00095740 File Offset: 0x00093940
		public bool CanRemoveProp(SerializableGuid assetId)
		{
			PropLibrary.RuntimePropInfo runtimePropInfo;
			return !MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(assetId, out runtimePropInfo) || this.CanRemoveProp(runtimePropInfo);
		}

		// Token: 0x0600214E RID: 8526 RVA: 0x0009576C File Offset: 0x0009396C
		private bool CanRemoveProp(PropLibrary.RuntimePropInfo metadata)
		{
			if (MonoBehaviourSingleton<StageManager>.Instance.PropRequirementLookup.ContainsKey(metadata.PropData.AssetID))
			{
				PropRequirement propRequirement = MonoBehaviourSingleton<StageManager>.Instance.PropRequirementLookup[metadata.PropData.AssetID];
				return this.assetDefinitionMap.Values.Count((SerializableGuid guid) => propRequirement.Guids.Contains(guid)) > propRequirement.MinCount;
			}
			if (MonoBehaviourSingleton<StageManager>.Instance.BaseTypeRequirementLookup.ContainsKey(metadata.PropData.BaseTypeId))
			{
				BaseTypeRequirement baseTypeRequirement = MonoBehaviourSingleton<StageManager>.Instance.BaseTypeRequirementLookup[metadata.PropData.BaseTypeId];
				List<SerializableGuid> assetIds = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetBaseTypeList(baseTypeRequirement.Guids.ToArray());
				return this.assetDefinitionMap.Values.Count((SerializableGuid guid) => assetIds.Contains(guid)) > baseTypeRequirement.MinCount;
			}
			return true;
		}

		// Token: 0x0600214F RID: 8527 RVA: 0x0009587C File Offset: 0x00093A7C
		public void SetMapId(SerializableGuid id)
		{
			this.mapId = new NetworkVariable<SerializableGuid>(id, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
		}

		// Token: 0x06002150 RID: 8528 RVA: 0x0009588C File Offset: 0x00093A8C
		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();
			this.BlockTokenCollection = new BlockTokenCollection();
			if (!base.IsServer)
			{
				MonoBehaviourSingleton<StageManager>.Instance.RegisterStage(this);
				return;
			}
			NetworkManager.Singleton.OnClientConnectedCallback += this.HandleNewPlayer;
		}

		// Token: 0x06002151 RID: 8529 RVA: 0x000958CC File Offset: 0x00093ACC
		public void HandleNewPlayer(ulong clientId)
		{
			if (NetworkManager.Singleton.LocalClientId != clientId)
			{
				ulong[] array = this.pendingNetworkIds.Keys.ToArray<ulong>();
				SerializableGuid[] array2 = this.pendingNetworkIds.Values.Select((Stage.PendingNetworkIdInfo p) => p.InstanceId).ToArray<SerializableGuid>();
				SerializableGuid[] array3 = new SerializableGuid[array2.Length];
				for (int i = 0; i < array2.Length; i++)
				{
					array3[i] = this.assetDefinitionMap[array2[i]];
				}
				ClientRpcParams clientRpcParams = new ClientRpcParams
				{
					Send = new ClientRpcSendParams
					{
						TargetClientIds = new List<ulong> { clientId }
					}
				};
				this.SendNetworkIds_ClientRpc(array, array2, array3, clientRpcParams);
			}
		}

		// Token: 0x06002152 RID: 8530 RVA: 0x000959A0 File Offset: 0x00093BA0
		[ClientRpc]
		private void SendNetworkIds_ClientRpc(ulong[] networkIds, SerializableGuid[] instanceIds, SerializableGuid[] assetDefinitionIds, ClientRpcParams clientRpcParams)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(2231079621U, clientRpcParams, RpcDelivery.Reliable);
				bool flag = networkIds != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag, default(FastBufferWriter.ForPrimitives));
				if (flag)
				{
					fastBufferWriter.WriteValueSafe<ulong>(networkIds, default(FastBufferWriter.ForPrimitives));
				}
				bool flag2 = instanceIds != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag2, default(FastBufferWriter.ForPrimitives));
				if (flag2)
				{
					fastBufferWriter.WriteValueSafe<SerializableGuid>(instanceIds, default(FastBufferWriter.ForNetworkSerializable));
				}
				bool flag3 = assetDefinitionIds != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag3, default(FastBufferWriter.ForPrimitives));
				if (flag3)
				{
					fastBufferWriter.WriteValueSafe<SerializableGuid>(assetDefinitionIds, default(FastBufferWriter.ForNetworkSerializable));
				}
				base.__endSendClientRpc(ref fastBufferWriter, 2231079621U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			for (int i = 0; i < networkIds.Length; i++)
			{
				this.TrackPendingNetworkObject(networkIds[i], assetDefinitionIds[i], instanceIds[i], false, null);
			}
		}

		// Token: 0x06002153 RID: 8531 RVA: 0x00095B7C File Offset: 0x00093D7C
		private void OnEnable()
		{
			foreach (PhysicalWire physicalWire in this.physicalWires)
			{
				physicalWire.gameObject.SetActive(true);
			}
		}

		// Token: 0x06002154 RID: 8532 RVA: 0x00095BD4 File Offset: 0x00093DD4
		public void CleanupProps()
		{
			if (!this.PropsAreLoaded)
			{
				return;
			}
			if (!MonoBehaviourSingleton<StageManager>.Instance)
			{
				global::UnityEngine.Debug.LogException(new Exception("Calling Cleanup Props without a valid StageManager.Instance!"));
				return;
			}
			if (this.assetDefinitionMap.Count == 0)
			{
				global::UnityEngine.Debug.LogWarning("Cleaning up props but assetDefinition map is empty");
			}
			this.PropsAreLoaded = false;
			this.propComponentMap.Clear();
			foreach (PhysicalWire physicalWire in this.physicalWires)
			{
				if (physicalWire)
				{
					global::UnityEngine.Object.Destroy(physicalWire.gameObject);
				}
			}
			this.physicalWires.Clear();
			this.pendingNetworkIds.Clear();
			this.pendingNetworkObjects.Clear();
			this.pendingNetworkCallbacks.Clear();
			global::UnityEngine.Debug.Log(string.Format("Disabling Stage {0}", this.MapId));
			foreach (SerializableGuid serializableGuid in this.gameObjectMap.Keys)
			{
				GameObject gameObject = this.gameObjectMap[serializableGuid];
				PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(this.assetDefinitionMap[serializableGuid]);
				if (gameObject == null)
				{
					global::UnityEngine.Debug.Log(string.Format("Obj was null: {0}, Name: {1}", serializableGuid, runtimePropInfo.PropData.Name));
				}
				else if (runtimePropInfo == null)
				{
					global::UnityEngine.Debug.LogError(string.Format("Attempting to cleanup in OnDisable and release cells for asset: (Id: {0}, Name: {1}), but it didn't exist?", this.assetDefinitionMap[serializableGuid], runtimePropInfo.PropData.Name));
				}
				else if (gameObject.GetComponent<NetworkObject>() == null || base.IsServer)
				{
					global::UnityEngine.Object.Destroy(gameObject);
				}
			}
			List<Vector3Int> list = new List<Vector3Int>();
			foreach (Cell cell in this.cellLookup.Values)
			{
				if (cell is PropCell)
				{
					list.Add(cell.Coordinate);
				}
			}
			foreach (Vector3Int vector3Int in list)
			{
				this.cellLookup.Remove(vector3Int);
			}
			this.assetDefinitionMap.Clear();
			this.gameObjectMap.Clear();
			this.reverseGameObjectMap.Clear();
		}

		// Token: 0x06002155 RID: 8533 RVA: 0x00095E80 File Offset: 0x00094080
		private void OnDisable()
		{
			foreach (PhysicalWire physicalWire in this.physicalWires)
			{
				if (physicalWire)
				{
					global::UnityEngine.Object.Destroy(physicalWire.gameObject);
				}
			}
		}

		// Token: 0x06002156 RID: 8534 RVA: 0x00095EE0 File Offset: 0x000940E0
		public override void OnDestroy()
		{
			base.OnDestroy();
			if (this.offlineStage != null)
			{
				this.offlineStage.gameObject.SetActive(false);
			}
			foreach (GameObject gameObject in this.gameObjectMap.Values)
			{
				if (gameObject)
				{
					NetworkObject component = gameObject.GetComponent<NetworkObject>();
					if (component == null || (component != null && base.IsServer))
					{
						global::UnityEngine.Object.Destroy(gameObject);
					}
				}
			}
			foreach (PhysicalWire physicalWire in this.physicalWires)
			{
				if (!(physicalWire == null) && physicalWire.gameObject)
				{
					global::UnityEngine.Object.Destroy(physicalWire.gameObject);
				}
			}
			this.physicalWires.Clear();
			if (NetworkManager.Singleton)
			{
				NetworkManager.Singleton.OnClientConnectedCallback -= this.HandleNewPlayer;
			}
		}

		// Token: 0x06002157 RID: 8535 RVA: 0x00096014 File Offset: 0x00094214
		public async Task LoadLevelIfNecessary(LevelState newLevelState, CancellationToken cancelToken, Action<string> progressCallback = null)
		{
			if (this.levelState == null)
			{
				this.levelState = newLevelState;
				await this.LoadLevel(cancelToken, progressCallback);
			}
			else
			{
				await this.HandleEndOfStageLoading(cancelToken, null);
			}
		}

		// Token: 0x06002158 RID: 8536 RVA: 0x00096070 File Offset: 0x00094270
		private async Task HandleEndOfStageLoading(CancellationToken cancelToken, Action<string> progressCallback = null)
		{
			this.ExecutePendingRequests();
			if (base.IsServer)
			{
				this.IsLoading = false;
				this.PropsAreLoaded = true;
			}
			else
			{
				this.claimCellRequests.Clear();
				this.propRegisteredRequest.Clear();
				NetworkBehaviourSingleton<GameplayMessagingManager>.Instance.LocalRestartLevel();
				bool scopeRequestSent = false;
				if (progressCallback != null)
				{
					progressCallback("Awaiting network objects...");
				}
				while (!NetworkBehaviourSingleton<GameplayMessagingManager>.Instance.LocalClientDoneLoadingScope)
				{
					if (!scopeRequestSent && this.serverPropLoadFinished.Value)
					{
						MonoBehaviourSingleton<NetworkScopeManager>.Instance.ClientSendScopeRequest(this.MapId);
						scopeRequestSent = true;
						foreach (PropLibrary.RuntimePropInfo runtimePropInfo in MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetAllRuntimeProps())
						{
							if (!runtimePropInfo.IsLoading && !runtimePropInfo.IsMissingObject && runtimePropInfo.EndlessProp.IsNetworked)
							{
								NetworkBehaviourSingleton<GameplayMessagingManager>.Instance.ClientBuiltNetworkProp_ServerRpc((ulong)runtimePropInfo.EndlessProp.GetComponent<NetworkObject>().PrefabIdHash, MonoBehaviourSingleton<StageManager>.Instance.ActiveLevelGuid, false, default(ServerRpcParams));
							}
						}
					}
					await Task.Yield();
					if (NetworkBehaviourSingleton<GameplayMessagingManager>.Instance == null || cancelToken.IsCancellationRequested)
					{
						return;
					}
				}
				this.ExecutePendingRequests();
				this.PropsAreLoaded = true;
				this.IsLoading = false;
			}
		}

		// Token: 0x06002159 RID: 8537 RVA: 0x000960C4 File Offset: 0x000942C4
		private void ExecutePendingRequests()
		{
			foreach (Stage.ClaimCellRequest claimCellRequest in this.claimCellRequests)
			{
				PropEntry propEntry;
				if (this.levelState.TryGetPropEntry(claimCellRequest.InstanceId, out propEntry))
				{
					this.ClaimCellsForProp(claimCellRequest.Definition, propEntry.Position, propEntry.Rotation, propEntry.InstanceId);
				}
			}
			foreach (Stage.PropRegisteredRequest propRegisteredRequest in this.propRegisteredRequest)
			{
				GameObject gameObject;
				if (this.gameObjectMap.TryGetValue(propRegisteredRequest.InstanceId, out gameObject))
				{
					NetworkObject component = gameObject.GetComponent<NetworkObject>();
					if (component)
					{
						IRegisteredSubscriber[] componentsInChildren = component.GetComponentsInChildren<IRegisteredSubscriber>();
						for (int i = 0; i < componentsInChildren.Length; i++)
						{
							componentsInChildren[i].EndlessRegistered();
						}
					}
				}
			}
		}

		// Token: 0x0600215A RID: 8538 RVA: 0x000961D0 File Offset: 0x000943D0
		private async Task LoadLevel(CancellationToken cancelToken, Action<string> progressCallback = null)
		{
			MonoBehaviourSingleton<LoadTimeTester>.Instance.StartTracking("Stage");
			bool hadOfflineStage;
			if (MonoBehaviourSingleton<StageManager>.Instance.TryGetOfflineStage(this.levelState.ToAssetReference(), out this.offlineStage))
			{
				hadOfflineStage = true;
				this.offlineStage.gameObject.SetActive(true);
				using (IEnumerator<Cell> enumerator = this.offlineStage.GetCells().GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Cell cell = enumerator.Current;
						this.UpdateBoundaries(cell);
					}
					goto IL_00E1;
				}
			}
			hadOfflineStage = false;
			this.offlineStage = MonoBehaviourSingleton<StageManager>.Instance.GetNewOfflineStage(this.levelState.ToAssetReference(), this.levelState.Name);
			IL_00E1:
			this.IsLoading = true;
			MonoBehaviourSingleton<LoadTimeTester>.Instance.LogTimeDelta("Load Boundries", "Stage");
			await this.LoadProps(cancelToken, progressCallback);
			MonoBehaviourSingleton<LoadTimeTester>.Instance.LogTimeDelta("Load Props", "Stage");
			if (cancelToken.IsCancellationRequested)
			{
				if (!hadOfflineStage)
				{
					MonoBehaviourSingleton<StageManager>.Instance.RemoveOfflineStage(this.levelState.ToAssetReference());
				}
			}
			else
			{
				Stopwatch frameStopWatch = new Stopwatch();
				frameStopWatch.Start();
				Dictionary<Vector3Int, List<Cell>> cellChunkMap = new Dictionary<Vector3Int, List<Cell>>();
				this.offlineStage.ChunkManager.SetCollectionModeToLoadTime();
				if (!hadOfflineStage)
				{
					for (int coordinateIndex = 0; coordinateIndex < this.levelState.TerrainEntries.Count; coordinateIndex++)
					{
						TerrainEntry terrainEntry = this.levelState.TerrainEntries[coordinateIndex];
						Cell cell2 = this.LoadCellPosition(terrainEntry.Position, terrainEntry.TilesetId);
						if (!this.cellLookup.ContainsKey(terrainEntry.Position))
						{
							Vector3Int chunkPosition = ChunkManager.GetChunkPosition(terrainEntry.Position);
							cellChunkMap.TryAdd(chunkPosition, new List<Cell>());
							cellChunkMap[chunkPosition].Add(cell2);
							if (frameStopWatch.ElapsedMilliseconds > 64L)
							{
								await Task.Yield();
								frameStopWatch.Restart();
								if (progressCallback != null)
								{
									progressCallback(string.Format("Loading terrain {0:N0}/{1:N0}", coordinateIndex, this.levelState.TerrainEntries.Count));
								}
								if (cancelToken.IsCancellationRequested)
								{
									MonoBehaviourSingleton<StageManager>.Instance.RemoveOfflineStage(this.levelState.ToAssetReference());
									return;
								}
							}
						}
					}
					int cellCount = 0;
					int totalTerrainCells = this.levelState.TerrainEntries.Count;
					if (progressCallback != null)
					{
						progressCallback(string.Format("Spawning terrain {0}/{1:N0}", 0, totalTerrainCells));
					}
					foreach (Vector3Int coords in cellChunkMap.Keys)
					{
						foreach (Cell cell3 in cellChunkMap[coords])
						{
							if (!this.cellLookup.ContainsKey(cell3.Coordinate))
							{
								cellCount++;
								TerrainCell terrainCell = cell3 as TerrainCell;
								this.SpawnTile(this.GetTilesetFromCell(terrainCell), terrainCell, true);
								this.UpdateBoundaries(cell3);
								if (frameStopWatch.ElapsedMilliseconds > 64L)
								{
									await Task.Yield();
									frameStopWatch.Restart();
									if (progressCallback != null)
									{
										progressCallback(string.Format("Spawning terrain {0:N0}/{1:N0}", cellCount, totalTerrainCells));
									}
									if (cancelToken.IsCancellationRequested)
									{
										if (!hadOfflineStage)
										{
											MonoBehaviourSingleton<StageManager>.Instance.RemoveOfflineStage(this.levelState.ToAssetReference());
										}
										return;
									}
								}
							}
						}
						List<Cell>.Enumerator enumerator3 = default(List<Cell>.Enumerator);
						if (this.offlineStage.ChunkManager)
						{
							this.offlineStage.ChunkManager.MarkChunkAsReadyToMerge(coords);
						}
						coords = default(Vector3Int);
					}
					Dictionary<Vector3Int, List<Cell>>.KeyCollection.Enumerator enumerator2 = default(Dictionary<Vector3Int, List<Cell>>.KeyCollection.Enumerator);
					MonoBehaviourSingleton<LoadTimeTester>.Instance.LogTimeDelta("Loaded Terrain", "Stage");
				}
				await Task.Yield();
				this.BuildSourceTracker = new NavMeshBuildSourceTracker();
				foreach (Collider collider in this.offlineStage.TileRoot.GetComponentsInChildren<Collider>())
				{
					this.BuildSourceTracker.AddTerrainSource(collider);
				}
				foreach (GameObject gameObject in this.reverseGameObjectMap.Keys)
				{
					this.BuildSourceTracker.AddPropToSources(gameObject);
				}
				if (this.minimumExtents == new Vector3Int(2147483647, 2147483647, 2147483647))
				{
					this.minimumExtents = Vector3Int.zero;
				}
				if (this.maximumExtents == new Vector3Int(-2147483648, -2147483648, -2147483648))
				{
					this.maximumExtents = Vector3Int.zero;
				}
				if (base.IsServer)
				{
					this.serverPropLoadFinished.Value = true;
				}
				for (int j = this.offlineStage.ChunkManager.AwaitingChunkCount; j > 0; j = this.offlineStage.ChunkManager.AwaitingChunkCount)
				{
					if (progressCallback != null)
					{
						progressCallback(string.Format("Optimizing terrain {0} chunks remain...", j));
					}
					await Task.Yield();
					if (cancelToken.IsCancellationRequested)
					{
						if (!hadOfflineStage)
						{
							MonoBehaviourSingleton<StageManager>.Instance.RemoveOfflineStage(this.levelState.ToAssetReference());
						}
						return;
					}
				}
				MonoBehaviourSingleton<LoadTimeTester>.Instance.LogTimeDelta("Chunks Merged", "Stage");
				this.offlineStage.ChunkManager.SetCollectionModeToRuntime();
				this.TerrainLoaded = true;
				if (progressCallback != null)
				{
					progressCallback("Awaiting load finalization...");
				}
				MonoBehaviourSingleton<StageManager>.Instance.TerrainAndPropsLoaded.Invoke(this);
				while (!this.BlockTokenCollection.IsPoolEmpty)
				{
					await Task.Yield();
					if (cancelToken.IsCancellationRequested)
					{
						if (!hadOfflineStage)
						{
							MonoBehaviourSingleton<StageManager>.Instance.RemoveOfflineStage(this.levelState.ToAssetReference());
						}
						return;
					}
				}
				MonoBehaviourSingleton<LoadTimeTester>.Instance.LogTimeDelta("Blocking Token Collection", "Stage");
				if (progressCallback != null)
				{
					progressCallback("Waiting for host to load...");
				}
				await this.HandleEndOfStageLoading(cancelToken, progressCallback);
				MonoBehaviourSingleton<LoadTimeTester>.Instance.StopTracking("Stage");
				if (cancelToken.IsCancellationRequested)
				{
					if (!hadOfflineStage)
					{
						MonoBehaviourSingleton<StageManager>.Instance.RemoveOfflineStage(this.levelState.ToAssetReference());
					}
				}
				else
				{
					this.HasLoaded = true;
				}
			}
		}

		// Token: 0x0600215B RID: 8539 RVA: 0x00096224 File Offset: 0x00094424
		private async Task LoadProps(CancellationToken cancelToken, Action<string> progressCallback = null)
		{
			Stopwatch frameStopWatch = new Stopwatch();
			int propEntryCount = this.levelState.PropEntries.Count<PropEntry>();
			for (int index = 0; index < propEntryCount; index++)
			{
				PropEntry propEntry = this.levelState.PropEntries[index];
				if (!MonoBehaviourSingleton<StageManager>.Instance.IsPropDestroyed(propEntry.InstanceId))
				{
					PropLibrary.RuntimePropInfo runtimePropInfo;
					if (MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(propEntry.AssetId, out runtimePropInfo))
					{
						if (runtimePropInfo.IsMissingObject)
						{
							MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.ReplacePropWithMissingObject(propEntry.InstanceId, runtimePropInfo, runtimePropInfo);
						}
						else if (runtimePropInfo.IsLoading)
						{
							global::UnityEngine.Debug.LogError(string.Concat(new string[]
							{
								"LoadingProp: ",
								runtimePropInfo.PropData.Name,
								" Loading props, but the meta data is still loading? Not expected, please report. Prop: ",
								runtimePropInfo.PropData.Name,
								", AssetId: ",
								runtimePropInfo.PropData.AssetID
							}));
						}
						else if (runtimePropInfo.EndlessProp.IsNetworked)
						{
							if (base.IsServer)
							{
								GameObject gameObject = global::UnityEngine.Object.Instantiate<EndlessProp>(runtimePropInfo.EndlessProp, propEntry.Position, propEntry.Rotation).gameObject;
								NetworkObject component = gameObject.GetComponent<NetworkObject>();
								if (component != null)
								{
									component.Spawn(false);
									this.TrackPendingNetworkId(component.NetworkObjectId, propEntry.InstanceId, false, null);
								}
								this.TrackNonNetworkedObject(propEntry.AssetId, propEntry.InstanceId, gameObject.gameObject, false);
							}
						}
						else
						{
							GameObject gameObject2 = global::UnityEngine.Object.Instantiate<EndlessProp>(runtimePropInfo.EndlessProp, propEntry.Position, propEntry.Rotation).gameObject;
							this.TrackNonNetworkedObject(propEntry.AssetId, propEntry.InstanceId, gameObject2, false);
						}
					}
					else
					{
						MonoBehaviourSingleton<StageManager>.Instance.PropFailedToLoad(propEntry);
					}
					if (frameStopWatch.ElapsedMilliseconds > 64L)
					{
						await Task.Yield();
						if (cancelToken.IsCancellationRequested)
						{
							break;
						}
						frameStopWatch.Restart();
						if (progressCallback != null)
						{
							progressCallback(string.Format("Spawning prop {0:N0}/{1:N0}", index, propEntryCount));
						}
					}
				}
			}
		}

		// Token: 0x0600215C RID: 8540 RVA: 0x00096278 File Offset: 0x00094478
		public async Task LoadPropsInStage(CancellationToken cancelToken, Action<string> progressCallback = null)
		{
			await this.LoadProps(cancelToken, progressCallback);
			if (!cancelToken.IsCancellationRequested)
			{
				await this.HandleEndOfStageLoading(cancelToken, progressCallback);
			}
		}

		// Token: 0x0600215D RID: 8541 RVA: 0x000962CC File Offset: 0x000944CC
		public void LoadExistingWires(bool setDisplayed = false)
		{
			if (base.IsClient && MonoBehaviourSingleton<StageManager>.Instance.ActiveStage != null)
			{
				Stage activeStage = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage;
				using (IEnumerator<WireBundle> enumerator = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.WireBundles.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						WireBundle wireBundle = enumerator.Current;
						if (!activeStage.PhysicalWires.Any((PhysicalWire wire) => wire.BundleId == wireBundle.BundleId))
						{
							this.SpawnPhysicalWire(wireBundle.BundleId, wireBundle.EmitterInstanceId, wireBundle.ReceiverInstanceId, wireBundle.RerouteNodeIds, wireBundle.WireColor, setDisplayed);
						}
					}
				}
			}
		}

		// Token: 0x0600215E RID: 8542 RVA: 0x000963B0 File Offset: 0x000945B0
		public PhysicalWire SpawnPhysicalWire(SerializableGuid bundleId, SerializableGuid emitterInstanceId, SerializableGuid receiverInstanceId, IEnumerable<SerializableGuid> rerouteNodeIds, WireColor wireColor = WireColor.NoColor, bool startActive = false)
		{
			GameObject gameObjectFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(emitterInstanceId);
			GameObject gameObjectFromInstanceId2 = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(receiverInstanceId);
			if (gameObjectFromInstanceId == null || gameObjectFromInstanceId2 == null)
			{
				return null;
			}
			PhysicalWire physicalWire = global::UnityEngine.Object.Instantiate<PhysicalWire>(this.physicalWireTemplate);
			Transform connectionPointFromObject = this.GetConnectionPointFromObject(gameObjectFromInstanceId, emitterInstanceId, ConnectionPoint.Emitter);
			Transform connectionPointFromObject2 = this.GetConnectionPointFromObject(gameObjectFromInstanceId2, receiverInstanceId, ConnectionPoint.Receiver);
			global::UnityEngine.Color color = this.wireColorDictionary[wireColor].Color;
			physicalWire.Setup(bundleId, emitterInstanceId, connectionPointFromObject, receiverInstanceId, connectionPointFromObject2, rerouteNodeIds, color);
			physicalWire.gameObject.SetActive(startActive);
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.PhysicalWires.Add(physicalWire);
			return physicalWire;
		}

		// Token: 0x0600215F RID: 8543 RVA: 0x0009645C File Offset: 0x0009465C
		public Transform GetConnectionPointFromObject(GameObject instanceObject, SerializableGuid instanceId, ConnectionPoint connectionPoint)
		{
			if (!this.propToWireConnectionPoints.ContainsKey(instanceId))
			{
				this.propToWireConnectionPoints.Add(instanceId, new Transform[2]);
			}
			if (this.propToWireConnectionPoints[instanceId][(int)connectionPoint] == null)
			{
				this.propToWireConnectionPoints[instanceId][(int)connectionPoint] = this.GenerateConnectionPoint(instanceId, connectionPoint, instanceObject);
			}
			return this.propToWireConnectionPoints[instanceId][(int)connectionPoint];
		}

		// Token: 0x06002160 RID: 8544 RVA: 0x000964C4 File Offset: 0x000946C4
		private Transform GenerateConnectionPoint(SerializableGuid instanceId, ConnectionPoint connectionPoint, GameObject parent)
		{
			Vector3Int[] array = new Vector3Int[] { Stage.WorldSpacePointToGridCoordinate(parent.transform.position) };
			global::UnityEngine.Vector3 vector = array[0];
			Vector3Int[] array2 = array;
			global::UnityEngine.Vector3 vector2 = array2[array2.Length - 1];
			global::UnityEngine.Vector3 vector3 = (vector2 - vector) * 0.5f;
			float num = 1f;
			global::UnityEngine.Vector3 vector4 = new global::UnityEngine.Vector3(0f, num * 0.25f, 0f);
			if (connectionPoint == ConnectionPoint.Receiver)
			{
				vector4 = -vector4;
			}
			global::UnityEngine.Debug.DrawLine(vector, vector2, global::UnityEngine.Color.red, 100f);
			Transform transform = new GameObject(string.Format("Physical Wire Connection Point ({0})", instanceId)).transform;
			transform.SetParent(parent.transform, false);
			transform.position = array[0] + vector3 + vector4;
			return transform;
		}

		// Token: 0x06002161 RID: 8545 RVA: 0x000965A4 File Offset: 0x000947A4
		public bool CanEraseTerrainCell(Vector3Int coordinate)
		{
			Cell cellFromCoordinate = this.GetCellFromCoordinate(coordinate);
			return cellFromCoordinate != null && cellFromCoordinate is TerrainCell;
		}

		// Token: 0x06002162 RID: 8546 RVA: 0x000965C7 File Offset: 0x000947C7
		public bool CanPaintCellPosition(Vector3Int coordinate)
		{
			return this.IsPositionInBounds(coordinate) && this.GetCellFromCoordinate(coordinate) == null;
		}

		// Token: 0x06002163 RID: 8547 RVA: 0x000965E0 File Offset: 0x000947E0
		public Cell GetCellFromCoordinate(Vector3Int coordinate)
		{
			Cell cell;
			if (!this.cellLookup.TryGetValue(coordinate, out cell))
			{
				this.offlineStage.TryGetCellFromCoordinate(coordinate, out cell);
			}
			return cell;
		}

		// Token: 0x06002164 RID: 8548 RVA: 0x00096610 File Offset: 0x00094810
		public T GetCellFromCoordinateAs<T>(Vector3Int coordinate) where T : Cell
		{
			Cell cellFromCoordinate = this.GetCellFromCoordinate(coordinate);
			if (cellFromCoordinate == null)
			{
				return default(T);
			}
			return cellFromCoordinate as T;
		}

		// Token: 0x06002165 RID: 8549 RVA: 0x0009663D File Offset: 0x0009483D
		public static Vector3Int WorldSpacePointToGridCoordinate(global::UnityEngine.Vector3 worldSpacePoint)
		{
			return new Vector3Int(Mathf.RoundToInt(worldSpacePoint.x), Mathf.RoundToInt(worldSpacePoint.y), Mathf.RoundToInt(worldSpacePoint.z));
		}

		// Token: 0x06002166 RID: 8550 RVA: 0x00096668 File Offset: 0x00094868
		public void PaintCellPositions(IEnumerable<Vector3Int> coordinates, int tilesetIndex, bool updateLevelState = true, bool updateNeighbors = true, bool generateChanges = false)
		{
			List<TerrainCell> list = new List<TerrainCell>();
			List<Vector3Int> list2 = new List<Vector3Int>();
			foreach (Vector3Int vector3Int in coordinates)
			{
				Cell cellFromCoordinate = this.GetCellFromCoordinate(vector3Int);
				if (!this.IsPositionInBounds(vector3Int) || cellFromCoordinate != null)
				{
					if (cellFromCoordinate != null)
					{
						list2.Add(vector3Int);
					}
					global::UnityEngine.Debug.LogWarning("Invalid cell");
				}
				else
				{
					Transform transform = new GameObject(string.Format("Cell ({0}, {1}, {2})", vector3Int.x, vector3Int.y, vector3Int.z)).transform;
					transform.SetParent(this.offlineStage.TileRoot);
					transform.position = vector3Int;
					TerrainCell terrainCell = new TerrainCell(vector3Int, transform);
					terrainCell.SetTilesetDetails(tilesetIndex);
					list.Add(terrainCell);
					this.offlineStage.AddCell(vector3Int, terrainCell);
					if (updateLevelState)
					{
						this.levelState.AddTerrainCell(terrainCell);
					}
				}
			}
			if (generateChanges)
			{
				List<TerrainChange> list3 = this.terrainChanges;
				TerrainChange terrainChange = new TerrainChange();
				terrainChange.Coordinates = list.Select((TerrainCell cell) => cell.Coordinate).ToArray<Vector3Int>();
				terrainChange.TilesetIndex = tilesetIndex;
				terrainChange.Erased = false;
				list3.Add(terrainChange);
			}
			if (updateNeighbors)
			{
				HashSet<Vector3Int> hashSet = coordinates.Except(list2).ToHashSet<Vector3Int>();
				foreach (TerrainCell terrainCell2 in list)
				{
					this.SpawnTile(this.TilesetAtIndex(tilesetIndex), terrainCell2, true);
					this.UpdateNeighbors(terrainCell2.Coordinate, this.TilesetAtIndex(tilesetIndex), hashSet);
					this.UpdateNearbyFringe(terrainCell2.Coordinate);
					this.UpdateBoundaries(terrainCell2);
				}
			}
		}

		// Token: 0x06002167 RID: 8551 RVA: 0x00096854 File Offset: 0x00094A54
		private Cell LoadCellPosition(Vector3Int coordinate, int tilesetIndex)
		{
			Transform transform = new GameObject(string.Format("Tile - Coordinate: {0},{1},{2}", coordinate.x, coordinate.y, coordinate.z)).transform;
			transform.SetParent(this.offlineStage.TileRoot);
			transform.position = coordinate;
			TerrainCell terrainCell = new TerrainCell(coordinate, transform);
			terrainCell.SetTilesetDetails(tilesetIndex);
			this.offlineStage.AddCell(coordinate, terrainCell);
			return terrainCell;
		}

		// Token: 0x06002168 RID: 8552 RVA: 0x000968D4 File Offset: 0x00094AD4
		public void TrackPendingNetworkId(ulong networkObjectId, SerializableGuid instanceId, bool updateLevelState = true, Action callback = null)
		{
			if (this.pendingNetworkIds.ContainsKey(networkObjectId))
			{
				global::UnityEngine.Debug.LogWarning(string.Format("We reused a pending network Id. Are we okay with this? {0}", networkObjectId));
				this.pendingNetworkIds[networkObjectId] = new Stage.PendingNetworkIdInfo(networkObjectId, instanceId, updateLevelState);
			}
			else
			{
				this.pendingNetworkIds.Add(networkObjectId, new Stage.PendingNetworkIdInfo(networkObjectId, instanceId, updateLevelState));
			}
			this.pendingNetworkCallbacks[networkObjectId] = callback;
		}

		// Token: 0x06002169 RID: 8553 RVA: 0x0009693C File Offset: 0x00094B3C
		[ClientRpc]
		private void TrackPendingNetworkObject_ClientRpc(SerializableGuid assetId, SerializableGuid instanceId, ulong networkObjectId, bool updateLevelState = true, ClientRpcParams rpcParams = default(ClientRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(667828874U, rpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<SerializableGuid>(in assetId, default(FastBufferWriter.ForNetworkSerializable));
				fastBufferWriter.WriteValueSafe<SerializableGuid>(in instanceId, default(FastBufferWriter.ForNetworkSerializable));
				BytePacker.WriteValueBitPacked(fastBufferWriter, networkObjectId);
				fastBufferWriter.WriteValueSafe<bool>(in updateLevelState, default(FastBufferWriter.ForPrimitives));
				base.__endSendClientRpc(ref fastBufferWriter, 667828874U, rpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.TrackPendingNetworkObject(networkObjectId, assetId, instanceId, updateLevelState, null);
		}

		// Token: 0x0600216A RID: 8554 RVA: 0x00096A7C File Offset: 0x00094C7C
		public void TrackPendingNetworkObject(ulong networkObjectId, SerializableGuid assetId, SerializableGuid instanceId, bool updateLevelState = true, Action callback = null)
		{
			this.assetDefinitionMap.TryAdd(instanceId, assetId);
			if (this.pendingNetworkObjects.ContainsKey(networkObjectId) && this.pendingNetworkObjects[networkObjectId] != null)
			{
				global::UnityEngine.Debug.Log(string.Format("{0}::{1}: Adding Instance Id: {2}", "Stage", "TrackPendingNetworkObject", instanceId));
				this.gameObjectMap.Add(instanceId, this.pendingNetworkObjects[networkObjectId]);
				this.reverseGameObjectMap.Add(this.pendingNetworkObjects[networkObjectId], instanceId);
				PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary[assetId];
				if (updateLevelState)
				{
					this.AddObjectToLevelState(this.pendingNetworkObjects[networkObjectId], runtimePropInfo, instanceId);
				}
				if (this.PropsAreLoaded)
				{
					IRegisteredSubscriber[] componentsInChildren = this.pendingNetworkObjects[networkObjectId].GetComponentsInChildren<IRegisteredSubscriber>();
					for (int i = 0; i < componentsInChildren.Length; i++)
					{
						componentsInChildren[i].EndlessRegistered();
					}
					PropEntry propEntry = this.levelState.GetPropEntry(instanceId);
					this.ClaimCellsForProp(runtimePropInfo, propEntry.Position, propEntry.Rotation, instanceId);
					this.pendingNetworkObjects.Remove(networkObjectId);
				}
				else
				{
					this.claimCellRequests.Add(new Stage.ClaimCellRequest(runtimePropInfo, instanceId, networkObjectId));
					this.propRegisteredRequest.Add(new Stage.PropRegisteredRequest(instanceId));
				}
				Action action;
				if (this.pendingNetworkCallbacks.TryGetValue(networkObjectId, out action))
				{
					if (action != null)
					{
						action();
					}
					this.pendingNetworkCallbacks.Remove(networkObjectId);
					return;
				}
				if (callback != null && callback != null)
				{
					callback();
					return;
				}
			}
			else
			{
				this.TrackPendingNetworkId(networkObjectId, instanceId, updateLevelState, callback);
			}
		}

		// Token: 0x0600216B RID: 8555 RVA: 0x00096C04 File Offset: 0x00094E04
		public void TrackNonNetworkedObject(SerializableGuid assetId, SerializableGuid instanceId, GameObject newObject, bool updateLevelState = true)
		{
			this.assetDefinitionMap.Add(instanceId, assetId);
			this.gameObjectMap.Add(instanceId, newObject);
			this.reverseGameObjectMap.Add(newObject, instanceId);
			PropLibrary.RuntimePropInfo runtimePropInfo;
			MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(assetId, out runtimePropInfo);
			if (updateLevelState)
			{
				this.AddObjectToLevelState(newObject, runtimePropInfo, instanceId);
			}
			PropEntry propEntry = this.levelState.GetPropEntry(instanceId);
			this.ClaimCellsForProp(runtimePropInfo, propEntry.Position, propEntry.Rotation, instanceId);
			IRegisteredSubscriber[] componentsInChildren = newObject.GetComponentsInChildren<IRegisteredSubscriber>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].EndlessRegistered();
			}
		}

		// Token: 0x0600216C RID: 8556 RVA: 0x00096C98 File Offset: 0x00094E98
		public void TrackNetworkObject(ulong networkObjectId, GameObject networkObject, Action callback = null)
		{
			if (!this.pendingNetworkIds.ContainsKey(networkObjectId))
			{
				this.pendingNetworkObjects.Add(networkObjectId, networkObject);
				if (callback != null)
				{
					this.pendingNetworkCallbacks.Add(networkObjectId, callback);
				}
				return;
			}
			SerializableGuid instanceId = this.pendingNetworkIds[networkObjectId].InstanceId;
			if (!this.gameObjectMap.ContainsKey(instanceId))
			{
				this.gameObjectMap.Add(instanceId, networkObject);
			}
			if (!this.reverseGameObjectMap.ContainsKey(networkObject))
			{
				this.reverseGameObjectMap.Add(networkObject, instanceId);
			}
			PropLibrary.RuntimePropInfo runtimePropInfo;
			if (!MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(this.assetDefinitionMap[instanceId], out runtimePropInfo))
			{
				return;
			}
			if (this.pendingNetworkIds[networkObjectId].UpdateLevelState)
			{
				this.AddObjectToLevelState(networkObject, runtimePropInfo, instanceId);
			}
			if (this.PropsAreLoaded)
			{
				PropEntry propEntry = this.levelState.GetPropEntry(instanceId);
				this.ClaimCellsForProp(runtimePropInfo, propEntry.Position, propEntry.Rotation, instanceId);
			}
			else
			{
				this.claimCellRequests.Add(new Stage.ClaimCellRequest(runtimePropInfo, instanceId, networkObjectId));
			}
			this.pendingNetworkIds.Remove(networkObjectId);
			Action action;
			if (this.pendingNetworkCallbacks.TryGetValue(networkObjectId, out action))
			{
				if (action != null)
				{
					action();
				}
				this.pendingNetworkCallbacks.Remove(networkObjectId);
			}
			else if (callback != null && callback != null)
			{
				callback();
			}
			if (this.PropsAreLoaded)
			{
				IRegisteredSubscriber[] componentsInChildren = networkObject.GetComponentsInChildren<IRegisteredSubscriber>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].EndlessRegistered();
				}
				return;
			}
			this.propRegisteredRequest.Add(new Stage.PropRegisteredRequest(instanceId));
		}

		// Token: 0x0600216D RID: 8557 RVA: 0x00096E18 File Offset: 0x00095018
		public void UntrackNetworkObject(GameObject gameObject, ulong networkInstanceId)
		{
			SerializableGuid serializableGuid;
			if (!this.reverseGameObjectMap.TryGetValue(gameObject, out serializableGuid))
			{
				return;
			}
			if (this.gameObjectMap[serializableGuid] == gameObject)
			{
				this.reverseGameObjectMap.Remove(gameObject);
				this.gameObjectMap.Remove(serializableGuid);
				this.assetDefinitionMap.Remove(serializableGuid);
				this.pendingNetworkIds.Remove(networkInstanceId);
			}
		}

		// Token: 0x0600216E RID: 8558 RVA: 0x00096E80 File Offset: 0x00095080
		private void UpdateNearbyFringe(Vector3Int sourceCoordinate)
		{
			Vector3Int[] array = new Vector3Int[]
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
				Vector3Int vector3Int = sourceCoordinate + array[i];
				Cell cellFromCoordinate = this.GetCellFromCoordinate(vector3Int);
				if (cellFromCoordinate is TerrainCell)
				{
					Tileset tilesetFromCell = this.GetTilesetFromCell(cellFromCoordinate);
					if (tilesetFromCell != null && tilesetFromCell.HasFringe)
					{
						this.SpawnTile(tilesetFromCell, (TerrainCell)cellFromCoordinate, false);
					}
				}
			}
		}

		// Token: 0x0600216F RID: 8559 RVA: 0x00097000 File Offset: 0x00095200
		private void SpawnTile(Tileset tileset, TerrainCell cell, bool addToChunkManager)
		{
			TileSpawnContext validVisualForCellPosition = tileset.GetValidVisualForCellPosition(this, cell.Coordinate);
			validVisualForCellPosition.Tile.Spawn(cell, validVisualForCellPosition, this, new Action<GameObject>(global::UnityEngine.Object.DestroyImmediate));
			if (this.offlineStage.ChunkManager && addToChunkManager)
			{
				this.offlineStage.ChunkManager.CellAdded(cell.Coordinate, cell.CellBase.gameObject);
			}
		}

		// Token: 0x06002170 RID: 8560 RVA: 0x0009706C File Offset: 0x0009526C
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
								this.UpdateCellAtPosition(coordinates, vector3Int2, tileset);
							}
						}
					}
				}
			}
		}

		// Token: 0x06002171 RID: 8561 RVA: 0x000970DC File Offset: 0x000952DC
		private void UpdateCellAtPosition(Vector3Int originCoordinate, Vector3Int coordinate, Tileset originCellTileset)
		{
			Cell cellFromCoordinate = this.GetCellFromCoordinate(coordinate);
			Tileset tilesetFromCell = this.GetTilesetFromCell(cellFromCoordinate);
			if (cellFromCoordinate == null || tilesetFromCell == null)
			{
				return;
			}
			if ((originCellTileset == null || originCellTileset.TilesetType != TilesetType.Slope) && (originCoordinate.y > coordinate.y || !tilesetFromCell.HasFringe) && originCellTileset != null && !originCellTileset.ConsidersTileset(tilesetFromCell) && !tilesetFromCell.ConsidersTileset(originCellTileset) && tilesetFromCell.TilesetType != TilesetType.Slope && originCoordinate.y >= coordinate.y && !tilesetFromCell.HasFringe)
			{
				return;
			}
			this.SpawnTile(tilesetFromCell, cellFromCoordinate as TerrainCell, false);
			if (this.offlineStage.ChunkManager)
			{
				this.offlineStage.ChunkManager.CellUpdated(coordinate);
			}
		}

		// Token: 0x06002172 RID: 8562 RVA: 0x0009718C File Offset: 0x0009538C
		public Tileset GetTilesetFromCell(Cell cell)
		{
			TerrainCell terrainCell = cell as TerrainCell;
			if (terrainCell != null)
			{
				return this.TilesetAtIndex(terrainCell.TilesetIndex);
			}
			return null;
		}

		// Token: 0x06002173 RID: 8563 RVA: 0x000971B4 File Offset: 0x000953B4
		[CanBeNull]
		public string GetTilesetIdFromCell(Cell cell)
		{
			TerrainCell terrainCell = cell as TerrainCell;
			if (terrainCell == null)
			{
				return null;
			}
			Tileset tileset = this.TilesetAtIndex(terrainCell.TilesetIndex);
			if (tileset == null)
			{
				return null;
			}
			return tileset.Asset.AssetID;
		}

		// Token: 0x06002174 RID: 8564 RVA: 0x000971E9 File Offset: 0x000953E9
		private bool CheckValidity(Vector3Int cellCoordinates)
		{
			return this.GetCellFromCoordinate(cellCoordinates) == null;
		}

		// Token: 0x06002175 RID: 8565 RVA: 0x000971F7 File Offset: 0x000953F7
		private void AddObjectToLevelState(GameObject instance, PropLibrary.RuntimePropInfo metadata, SerializableGuid instanceId)
		{
			this.levelState.AddProp(instance, metadata, instanceId);
		}

		// Token: 0x06002176 RID: 8566 RVA: 0x00097208 File Offset: 0x00095408
		private void ClaimCellsForProp(PropLibrary.RuntimePropInfo runtimePropInfo, global::UnityEngine.Vector3 propPosition, Quaternion rotation, SerializableGuid instanceId)
		{
			PropLocationOffset[] array;
			if (runtimePropInfo == null)
			{
				array = new PropLocationOffset[]
				{
					new PropLocationOffset
					{
						Offset = new Vector3Int(Mathf.FloorToInt(propPosition.x), Mathf.FloorToInt(propPosition.y), Mathf.FloorToInt(propPosition.z))
					}
				};
			}
			else
			{
				array = this.GetRotatedPropLocations(runtimePropInfo.PropData, propPosition, rotation);
			}
			foreach (PropLocationOffset propLocationOffset in array)
			{
				GameObject gameObject = new GameObject(string.Format("Cell ({0}, {1}, {2})", propLocationOffset.Offset.x, propLocationOffset.Offset.y, propLocationOffset.Offset.z));
				gameObject.transform.SetParent(this.PropRoot);
				gameObject.transform.position = propLocationOffset.Offset;
				PropCell propCell = new PropCell(propLocationOffset, gameObject.transform)
				{
					InstanceId = instanceId
				};
				this.UpdateBoundaries(propCell);
				Cell cell;
				if (!this.cellLookup.TryGetValue(propLocationOffset.Offset, out cell))
				{
					this.offlineStage.TryGetCellFromCoordinate(propLocationOffset.Offset, out cell);
				}
				if (cell != null)
				{
					if (cell is TerrainCell)
					{
						global::UnityEngine.Debug.LogWarning("Attempted to claim cell for a prop that is actually terrain?");
					}
					else
					{
						PropCell propCell2 = cell as PropCell;
						if (propCell2 != null && propCell2.InstanceId != instanceId)
						{
							global::UnityEngine.Debug.LogWarning(string.Format("Attempting to claim a cell for instance id: {0} but it is already claimed by instance id: {1}", instanceId, propCell2.InstanceId));
						}
					}
				}
				else
				{
					this.cellLookup.Add(propLocationOffset.Offset, propCell);
				}
			}
			foreach (PropLocationOffset propLocationOffset2 in array)
			{
				this.UpdateNeighbors(propLocationOffset2.Offset, null, null);
			}
		}

		// Token: 0x06002177 RID: 8567 RVA: 0x000973C4 File Offset: 0x000955C4
		private void CheckStageBoundary(Cell cell)
		{
			if (this.ResetStageBoundaryIfNecessary(cell))
			{
				foreach (Cell cell2 in this.cellLookup.Values)
				{
					this.UpdateBoundaries(cell2);
				}
				foreach (Cell cell3 in this.offlineStage.GetCells())
				{
					this.UpdateBoundaries(cell3);
				}
			}
		}

		// Token: 0x06002178 RID: 8568 RVA: 0x00097468 File Offset: 0x00095668
		public void EraseCells(IEnumerable<Vector3Int> coordinates)
		{
			List<Cell> list = new List<Cell>();
			foreach (Vector3Int vector3Int in coordinates)
			{
				Cell cellFromCoordinate = this.GetCellFromCoordinate(vector3Int);
				if (this.cellLookup.Remove(vector3Int) || this.offlineStage.RemoveCell(vector3Int))
				{
					this.levelState.RemoveTerrainCell(cellFromCoordinate);
					if (cellFromCoordinate is TerrainCell && this.offlineStage.ChunkManager)
					{
						this.offlineStage.ChunkManager.CellRemoved(cellFromCoordinate.Coordinate, cellFromCoordinate.CellBase.gameObject);
					}
					if (cellFromCoordinate.CellBase != null)
					{
						global::UnityEngine.Object.Destroy(cellFromCoordinate.CellBase.gameObject);
					}
					list.Add(cellFromCoordinate);
				}
			}
			foreach (Cell cell in list)
			{
				this.CheckStageBoundary(cell);
				Vector3Int coordinate = cell.Coordinate;
				TerrainCell terrainCell = cell as TerrainCell;
				this.UpdateNeighbors(coordinate, (terrainCell != null) ? this.TilesetAtIndex(terrainCell.TilesetIndex) : null, null);
				this.UpdateNearbyFringe(cell.Coordinate);
			}
		}

		// Token: 0x06002179 RID: 8569 RVA: 0x000975C0 File Offset: 0x000957C0
		public HashSet<SerializableGuid> GetWiresUsingProps(IEnumerable<SerializableGuid> propIds)
		{
			HashSet<SerializableGuid> hashSet = new HashSet<SerializableGuid>();
			foreach (SerializableGuid serializableGuid in propIds)
			{
				foreach (WireBundle wireBundle in this.levelState.WireBundles)
				{
					if (wireBundle.EmitterInstanceId == serializableGuid || wireBundle.ReceiverInstanceId == serializableGuid)
					{
						hashSet.Add(wireBundle.BundleId);
					}
				}
			}
			return hashSet;
		}

		// Token: 0x0600217A RID: 8570 RVA: 0x00097670 File Offset: 0x00095870
		public void RemoveProp(SerializableGuid instanceId, SerializableGuid assetId)
		{
			global::UnityEngine.Debug.Log("Stage.RemoveProps - Removing Props");
			PropLibrary.RuntimePropInfo runtimePropInfo;
			if (MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(assetId, out runtimePropInfo))
			{
				this.DestroyStageObject(instanceId, runtimePropInfo, false);
			}
			Stage.PendingNetworkIdInfo pendingNetworkIdInfo = this.pendingNetworkIds.Values.FirstOrDefault((Stage.PendingNetworkIdInfo item) => item.InstanceId == instanceId);
			if (pendingNetworkIdInfo != null)
			{
				this.pendingNetworkIds.Remove(pendingNetworkIdInfo.NetworkId);
			}
			this.LevelState.RemoveProp(instanceId);
		}

		// Token: 0x0600217B RID: 8571 RVA: 0x000976FC File Offset: 0x000958FC
		public void ReplacePropWithMissingObject(SerializableGuid instanceId, PropLibrary.RuntimePropInfo propInfo, PropLibrary.RuntimePropInfo missingPropInfo)
		{
			global::UnityEngine.Debug.Log(string.Format("ReplacePropWithMissingObject:: Replacing {0} with missing object", instanceId));
			this.DestroyStageObject(instanceId, propInfo, false);
			Stage.PendingNetworkIdInfo pendingNetworkIdInfo = this.pendingNetworkIds.Values.FirstOrDefault((Stage.PendingNetworkIdInfo item) => item.InstanceId == instanceId);
			if (pendingNetworkIdInfo != null)
			{
				this.pendingNetworkIds.Remove(pendingNetworkIdInfo.NetworkId);
			}
			global::UnityEngine.Debug.Log(string.Format("ReplacePropWithMissingObject:: Spawning the missing object, Instance Id: {0}, EndlessProp.GameObject Name {1}", instanceId, missingPropInfo.EndlessProp.gameObject.name));
			EndlessProp endlessProp = global::UnityEngine.Object.Instantiate<EndlessProp>(missingPropInfo.EndlessProp);
			this.assetDefinitionMap.Add(instanceId, missingPropInfo.PropData.AssetID);
			this.gameObjectMap.Add(instanceId, endlessProp.gameObject);
			this.reverseGameObjectMap.Add(endlessProp.gameObject, instanceId);
			PropEntry propEntry = this.levelState.GetPropEntry(instanceId);
			endlessProp.transform.position = Stage.WorldSpacePointToGridCoordinate(propEntry.Position);
			this.ClaimCellsForProp(missingPropInfo, propEntry.Position, propEntry.Rotation, instanceId);
		}

		// Token: 0x0600217C RID: 8572 RVA: 0x00002DB0 File Offset: 0x00000FB0
		public void OnBeforeSerialize()
		{
		}

		// Token: 0x0600217D RID: 8573 RVA: 0x00002DB0 File Offset: 0x00000FB0
		public void OnAfterDeserialize()
		{
		}

		// Token: 0x0600217E RID: 8574 RVA: 0x0009783B File Offset: 0x00095A3B
		public LineCastHit Linecast(Ray ray, float length, float scalar, SerializableGuid ignoredInstanceId)
		{
			return this.Linecast(ray, length, scalar, length, ignoredInstanceId);
		}

		// Token: 0x0600217F RID: 8575 RVA: 0x0009784C File Offset: 0x00095A4C
		public LineCastHit Linecast(Ray ray, float length, float scalar, float maxFallbackDistance, SerializableGuid ignoredInstanceId)
		{
			LineCastHit lineCastHit = default(LineCastHit);
			LinecastEnumerator linecastEnumerator = new LinecastEnumerator(ray, length, scalar);
			Vector3Int vector3Int = linecastEnumerator.Current;
			Vector3Int vector3Int2 = Vector3Int.zero;
			float num = maxFallbackDistance * maxFallbackDistance;
			while (!this.<Linecast>g__IsCoordinateInPotentialBounds|145_0(Stage.WorldSpacePointToGridCoordinate(ray.origin)) || this.<Linecast>g__IsCoordinateInPotentialBounds|145_0(linecastEnumerator.Current))
			{
				Cell cellFromCoordinate = this.GetCellFromCoordinate(linecastEnumerator.Current);
				if (cellFromCoordinate != null)
				{
					PropCell propCell = cellFromCoordinate as PropCell;
					if (propCell == null || propCell.InstanceId != ignoredInstanceId)
					{
						lineCastHit.IntersectionOccured = true;
						lineCastHit.Distance = (linecastEnumerator.PositionAtStep - ray.origin).magnitude;
						lineCastHit.IntersectedObjectPosition = linecastEnumerator.Current;
						lineCastHit.NearestPositionToObject = vector3Int;
						break;
					}
				}
				if ((linecastEnumerator.PositionAtStep - ray.origin).sqrMagnitude < num)
				{
					vector3Int2 = linecastEnumerator.Current;
				}
				vector3Int = linecastEnumerator.Current;
				if (!linecastEnumerator.MoveNext())
				{
					break;
				}
			}
			if (!lineCastHit.IntersectionOccured)
			{
				lineCastHit.NearestPositionToObject = vector3Int2;
			}
			return lineCastHit;
		}

		// Token: 0x06002180 RID: 8576 RVA: 0x00097960 File Offset: 0x00095B60
		public GameObject GetGameObjectFromInstanceId(SerializableGuid instanceId)
		{
			if (!this.gameObjectMap.ContainsKey(instanceId))
			{
				bool flag = false;
				for (int i = 0; i < this.levelState.PropEntries.Count; i++)
				{
					if (this.levelState.PropEntries[i].InstanceId == instanceId)
					{
						flag = true;
						global::UnityEngine.Debug.LogWarning(string.Format("Instance Id: {0} was not in GameObjectMap, but exists in level state. Label: {1}, Asset Id: {2}", instanceId, this.levelState.PropEntries[i].Label, this.levelState.PropEntries[i].AssetId));
						break;
					}
				}
				if (!flag)
				{
					global::UnityEngine.Debug.LogWarning(string.Format("Attempted to find Instance Id: {0} in the GameObjectMap, and after searching level state, the object was not found.", instanceId));
				}
				return null;
			}
			return this.gameObjectMap[instanceId];
		}

		// Token: 0x06002181 RID: 8577 RVA: 0x00097A29 File Offset: 0x00095C29
		public SerializableGuid GetAssetIdFromInstanceId(SerializableGuid instanceId)
		{
			return this.assetDefinitionMap[instanceId];
		}

		// Token: 0x06002182 RID: 8578 RVA: 0x00097A37 File Offset: 0x00095C37
		public bool TryGetAssetDefinitionFromInstanceId(SerializableGuid instanceId, out SerializableGuid assetId)
		{
			return this.assetDefinitionMap.TryGetValue(instanceId, out assetId);
		}

		// Token: 0x06002183 RID: 8579 RVA: 0x00097A48 File Offset: 0x00095C48
		private bool IsPositionInBounds(Vector3Int coordinate)
		{
			return (this.maximumExtents.x == int.MinValue || this.minimumExtents.x == int.MaxValue || (Mathf.Abs(this.maximumExtents.x - coordinate.x) <= this.maxExtents.x && Mathf.Abs(coordinate.x - this.minimumExtents.x) <= this.maxExtents.x)) && (this.maximumExtents.z == int.MinValue || this.minimumExtents.z == int.MaxValue || (Mathf.Abs(this.maximumExtents.z - coordinate.z) <= this.maxExtents.z && Mathf.Abs(coordinate.z - this.minimumExtents.z) <= this.maxExtents.z)) && (this.maximumExtents.y == int.MinValue || this.minimumExtents.y == int.MaxValue || (Mathf.Abs(this.maximumExtents.y - coordinate.y) <= this.maxExtents.y && Mathf.Abs(coordinate.y - this.minimumExtents.y) <= this.maxExtents.y));
		}

		// Token: 0x06002184 RID: 8580 RVA: 0x00097BA8 File Offset: 0x00095DA8
		private void UpdateBoundaries(Cell newCell)
		{
			if (this.minimumExtents.x == 2147483647 || this.minimumExtents.x > newCell.Coordinate.x)
			{
				this.minimumExtents.x = newCell.Coordinate.x;
			}
			if (this.maximumExtents.x == -2147483648 || this.maximumExtents.x < newCell.Coordinate.x)
			{
				this.maximumExtents.x = newCell.Coordinate.x;
			}
			if (this.maximumExtents.z == -2147483648 || this.maximumExtents.z < newCell.Coordinate.z)
			{
				this.maximumExtents.z = newCell.Coordinate.z;
			}
			if (this.minimumExtents.z == 2147483647 || this.minimumExtents.z > newCell.Coordinate.z)
			{
				this.minimumExtents.z = newCell.Coordinate.z;
			}
			if (this.maximumExtents.y == -2147483648 || this.maximumExtents.y < newCell.Coordinate.y)
			{
				this.maximumExtents.y = newCell.Coordinate.y;
			}
			if (this.minimumExtents.y == 2147483647 || this.minimumExtents.y > newCell.Coordinate.y)
			{
				this.minimumExtents.y = newCell.Coordinate.y;
			}
		}

		// Token: 0x06002185 RID: 8581 RVA: 0x00097D5C File Offset: 0x00095F5C
		private bool ResetStageBoundaryIfNecessary(Cell cell)
		{
			bool flag = false;
			if (this.minimumExtents.x == cell.Coordinate.x)
			{
				this.minimumExtents.x = int.MaxValue;
				flag = true;
			}
			if (this.maximumExtents.x == cell.Coordinate.x)
			{
				this.maximumExtents.x = int.MinValue;
				flag = true;
			}
			if (this.maximumExtents.z == cell.Coordinate.z)
			{
				this.maximumExtents.z = int.MinValue;
				flag = true;
			}
			if (this.minimumExtents.z == cell.Coordinate.z)
			{
				this.minimumExtents.z = int.MaxValue;
				flag = true;
			}
			if (this.maximumExtents.y == cell.Coordinate.y)
			{
				this.maximumExtents.y = int.MinValue;
				flag = true;
			}
			if (this.minimumExtents.y == cell.Coordinate.y)
			{
				this.minimumExtents.y = int.MaxValue;
				flag = true;
			}
			return flag;
		}

		// Token: 0x06002186 RID: 8582 RVA: 0x00097E7C File Offset: 0x0009607C
		[ServerRpc(RequireOwnership = false)]
		public void AddInstanceIdsToLevelState_ServerRpc(SerializableGuid[] instanceIds)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				ServerRpcParams serverRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(1781947305U, serverRpcParams, RpcDelivery.Reliable);
				bool flag = instanceIds != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag, default(FastBufferWriter.ForPrimitives));
				if (flag)
				{
					fastBufferWriter.WriteValueSafe<SerializableGuid>(instanceIds, default(FastBufferWriter.ForNetworkSerializable));
				}
				base.__endSendServerRpc(ref fastBufferWriter, 1781947305U, serverRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.AddInstanceIdsToLevelState_ClientRpc(instanceIds);
		}

		// Token: 0x06002187 RID: 8583 RVA: 0x00097FA4 File Offset: 0x000961A4
		[ClientRpc]
		private void AddInstanceIdsToLevelState_ClientRpc(SerializableGuid[] instanceIds)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(1366621623U, clientRpcParams, RpcDelivery.Reliable);
				bool flag = instanceIds != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag, default(FastBufferWriter.ForPrimitives));
				if (flag)
				{
					fastBufferWriter.WriteValueSafe<SerializableGuid>(instanceIds, default(FastBufferWriter.ForNetworkSerializable));
				}
				base.__endSendClientRpc(ref fastBufferWriter, 1366621623U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			foreach (SerializableGuid serializableGuid in instanceIds)
			{
				this.AddObjectToLevelState(this.GetGameObjectFromInstanceId(serializableGuid), MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary[this.GetAssetIdFromInstanceId(serializableGuid)], serializableGuid);
			}
		}

		// Token: 0x06002188 RID: 8584 RVA: 0x000980FE File Offset: 0x000962FE
		public void RegisterDepthPlane(DepthPlane newDepthPlane)
		{
			if (this.depthPlane == null)
			{
				this.depthPlane = newDepthPlane;
				return;
			}
			global::UnityEngine.Debug.LogWarning("Registering a depth plane when one already exists!");
		}

		// Token: 0x06002189 RID: 8585 RVA: 0x00098120 File Offset: 0x00096320
		public void UnregisterDepthPlane(DepthPlane targetDepthPlane)
		{
			if (this.depthPlane == targetDepthPlane)
			{
				this.depthPlane = null;
			}
		}

		// Token: 0x0600218A RID: 8586 RVA: 0x00098137 File Offset: 0x00096337
		public void SetDefaultEnvironment(SerializableGuid instanceId)
		{
			this.LevelState.SetDefaultEnvironment(instanceId);
		}

		// Token: 0x0600218B RID: 8587 RVA: 0x00098145 File Offset: 0x00096345
		public void ClearDefaultEnvironment()
		{
			this.levelState.ClearDefaultEnvironment();
		}

		// Token: 0x0600218C RID: 8588 RVA: 0x00098152 File Offset: 0x00096352
		public void MoveProp(SerializableGuid assetId, SerializableGuid instanceId, global::UnityEngine.Vector3 position, global::UnityEngine.Vector3 eulerAngles)
		{
			this.ReleaseCellsForProp(instanceId, assetId);
			this.LevelState.UpdatePropPositionAndRotation(instanceId, position, Quaternion.Euler(eulerAngles));
			this.AdjustCellsForObject(assetId, instanceId, position, eulerAngles);
		}

		// Token: 0x0600218D RID: 8589 RVA: 0x0009817C File Offset: 0x0009637C
		private void AdjustCellsForObject(SerializableGuid movedObjectAssetId, SerializableGuid instanceId, global::UnityEngine.Vector3 position, global::UnityEngine.Vector3 eulerAngles)
		{
			PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary[movedObjectAssetId];
			PropEntry propEntry = this.levelState.GetPropEntry(instanceId);
			this.ClaimCellsForProp(runtimePropInfo, propEntry.Position, propEntry.Rotation, instanceId);
		}

		// Token: 0x0600218E RID: 8590 RVA: 0x000981BC File Offset: 0x000963BC
		public void ReleaseCellsForProp(SerializableGuid instanceId, SerializableGuid assetId)
		{
			global::UnityEngine.Debug.Log(string.Format("{0}: Releasing Id: {1}, Asset Id: {2}", "ReleaseCellsForProp", instanceId, assetId));
			GameObject gameObjectFromInstanceId = this.GetGameObjectFromInstanceId(instanceId);
			if (!gameObjectFromInstanceId)
			{
				global::UnityEngine.Debug.LogException(new Exception(string.Format("Releasing cells for Prop: {0}, Asset Id: {1} but the objectInstance was invalid", instanceId, assetId)));
			}
			if (!MonoBehaviourSingleton<StageManager>.Instance)
			{
				global::UnityEngine.Debug.LogException(new Exception(string.Format("Releasing cells for Prop: {0}, Asset Id: {1} but the StageManager.Instance is invalid", instanceId, assetId)));
			}
			PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(assetId);
			if (runtimePropInfo == null)
			{
				global::UnityEngine.Debug.LogException(new Exception(string.Format("Releasing cells for Prop: {0}, Asset Id: {1} but the definition was invalid", instanceId, assetId)));
				return;
			}
			Vector3Int[] array = (from x in this.GetRotatedPropLocations(runtimePropInfo.PropData, gameObjectFromInstanceId.transform.position, gameObjectFromInstanceId.transform.rotation)
				select x.Offset).ToArray<Vector3Int>();
			this.EraseCells(array);
		}

		// Token: 0x0600218F RID: 8591 RVA: 0x000982CD File Offset: 0x000964CD
		public void PrepForGameplay()
		{
			this.ApplyMemberChanges();
			if (base.IsServer)
			{
				this.ApplyWiring();
			}
		}

		// Token: 0x06002190 RID: 8592 RVA: 0x000982E4 File Offset: 0x000964E4
		public void ApplyMemberChanges()
		{
			foreach (PropEntry propEntry in MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.PropEntries)
			{
				PropLibrary.RuntimePropInfo runtimePropInfo;
				if (!MonoBehaviourSingleton<StageManager>.Instance.IsPropDestroyed(propEntry.InstanceId) && MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(propEntry.AssetId, out runtimePropInfo) && !runtimePropInfo.IsMissingObject)
				{
					try
					{
						Stage.ApplyMemberChanges(propEntry);
					}
					catch (Exception ex)
					{
						global::UnityEngine.Debug.LogException(ex);
					}
				}
			}
		}

		// Token: 0x06002191 RID: 8593 RVA: 0x00098388 File Offset: 0x00096588
		public static void ApplyMemberChanges(PropEntry propEntry)
		{
			GameObject gameObjectFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(propEntry.InstanceId);
			if (gameObjectFromInstanceId != null)
			{
				using (List<ComponentEntry>.Enumerator enumerator = propEntry.ComponentEntries.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						ComponentEntry componentEntry = enumerator.Current;
						Type type = Type.GetType(componentEntry.AssemblyQualifiedName);
						if (type != null)
						{
							Component componentInChildren = gameObjectFromInstanceId.GetComponentInChildren(type);
							if (componentInChildren)
							{
								foreach (MemberChange memberChange in componentEntry.Changes)
								{
									try
									{
										MemberInfo[] member = componentInChildren.GetType().GetMember(memberChange.MemberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
										if (member.Length != 0)
										{
											if (member[0].GetCustomAttribute<EndlessNonSerializedAttribute>() == null)
											{
												member[0].SetValue(componentInChildren, memberChange.ToObject());
											}
										}
										else
										{
											global::UnityEngine.Debug.LogException(new Exception("Member " + memberChange.MemberName + " not found on type " + componentInChildren.GetType().Name));
										}
									}
									catch (Exception ex)
									{
										global::UnityEngine.Debug.LogException(new Exception("Error applying Member " + memberChange.MemberName + " on type " + componentInChildren.GetType().Name, ex));
									}
								}
							}
						}
					}
					return;
				}
			}
			global::UnityEngine.Debug.LogWarning("Applying member changes to a non-destroyed, but still missing prop. Known Issue.");
		}

		// Token: 0x06002192 RID: 8594 RVA: 0x00098540 File Offset: 0x00096740
		private void ApplyWiring()
		{
			List<WireBundle> list = new List<WireBundle>();
			foreach (WireBundle wireBundle in MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.WireBundles)
			{
				if (!MonoBehaviourSingleton<StageManager>.Instance.IsPropDestroyed(wireBundle.EmitterInstanceId) && !MonoBehaviourSingleton<StageManager>.Instance.IsPropDestroyed(wireBundle.ReceiverInstanceId))
				{
					SerializableGuid serializableGuid;
					SerializableGuid serializableGuid2;
					PropLibrary.RuntimePropInfo runtimePropInfo;
					PropLibrary.RuntimePropInfo runtimePropInfo2;
					if (!this.assetDefinitionMap.TryGetValue(wireBundle.EmitterInstanceId, out serializableGuid))
					{
						global::UnityEngine.Debug.LogError(string.Format("While applying wiring, we could not find the asset definition for emitter asset id: {0}", wireBundle.EmitterInstanceId));
					}
					else if (!this.assetDefinitionMap.TryGetValue(wireBundle.ReceiverInstanceId, out serializableGuid2))
					{
						global::UnityEngine.Debug.LogError(string.Format("While applying wiring, we could not find the asset definition for receiver asset id: {0}", wireBundle.EmitterInstanceId));
					}
					else if (MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(serializableGuid, out runtimePropInfo) && MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(serializableGuid2, out runtimePropInfo2) && !runtimePropInfo.IsMissingObject && !runtimePropInfo2.IsMissingObject)
					{
						GameObject gameObjectFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(wireBundle.EmitterInstanceId);
						GameObject gameObjectFromInstanceId2 = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(wireBundle.ReceiverInstanceId);
						if (gameObjectFromInstanceId == null)
						{
							global::UnityEngine.Debug.LogException(new Exception("Attempting to apply wiring from a non-existent object."));
						}
						else if (gameObjectFromInstanceId2 == null)
						{
							global::UnityEngine.Debug.LogException(new Exception("Attempting to apply wiring to a non-existent object."));
						}
						else
						{
							list.Add(wireBundle);
							foreach (WireEntry wireEntry in wireBundle.Wires)
							{
								bool flag = string.IsNullOrEmpty(wireEntry.EmitterComponentAssemblyQualifiedTypeName);
								Type type;
								if (flag)
								{
									type = typeof(EndlessScriptComponent);
								}
								else
								{
									type = Type.GetType(wireEntry.EmitterComponentAssemblyQualifiedTypeName);
									if (type == null)
									{
										continue;
									}
								}
								Component componentFromMap = this.GetComponentFromMap(wireBundle.EmitterInstanceId, type, gameObjectFromInstanceId);
								if (componentFromMap == null)
								{
									global::UnityEngine.Debug.LogException(new Exception("Attempting to apply wiring from a non-existent component. Known issue."));
								}
								else if (flag)
								{
									(componentFromMap as EndlessScriptComponent).ClearEndlessEventObjects();
								}
								else
								{
									MemberInfo[] member = type.GetMember(wireEntry.EmitterMemberName, BindingFlags.Instance | BindingFlags.Public);
									if (member.Length != 0)
									{
										object obj = Activator.CreateInstance((member[0] as FieldInfo).FieldType);
										member[0].SetValue(componentFromMap, obj);
									}
								}
							}
						}
					}
				}
			}
			foreach (WireBundle wireBundle2 in list)
			{
				GameObject gameObjectFromInstanceId3 = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(wireBundle2.EmitterInstanceId);
				GameObject gameObjectFromInstanceId4 = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(wireBundle2.ReceiverInstanceId);
				foreach (WireEntry wireEntry2 in wireBundle2.Wires)
				{
					try
					{
						this.ApplyGameEditorEvents(wireBundle2, wireEntry2, gameObjectFromInstanceId3, gameObjectFromInstanceId4);
					}
					catch (Exception ex)
					{
						global::UnityEngine.Debug.LogException(new Exception(string.Format("Failed to apply wire. {0}", wireEntry2), ex));
					}
				}
			}
		}

		// Token: 0x06002193 RID: 8595 RVA: 0x000988F8 File Offset: 0x00096AF8
		private void ApplyGameEditorEvents(WireBundle wireBundle, WireEntry wireEntry, GameObject emitterObject, GameObject receiverObject)
		{
			bool flag = string.IsNullOrEmpty(wireEntry.EmitterComponentAssemblyQualifiedTypeName);
			Type type;
			if (flag)
			{
				type = typeof(EndlessScriptComponent);
			}
			else
			{
				type = Type.GetType(wireEntry.EmitterComponentAssemblyQualifiedTypeName);
			}
			bool flag2 = string.IsNullOrEmpty(wireEntry.ReceiverComponentAssemblyQualifiedTypeName);
			Type type2;
			if (flag2)
			{
				type2 = typeof(EndlessScriptComponent);
			}
			else
			{
				type2 = Type.GetType(wireEntry.ReceiverComponentAssemblyQualifiedTypeName);
			}
			Component componentFromMap = this.GetComponentFromMap(wireBundle.EmitterInstanceId, type, emitterObject);
			Component componentFromMap2 = this.GetComponentFromMap(wireBundle.ReceiverInstanceId, type2, receiverObject);
			object obj;
			Type[] array;
			if (flag)
			{
				EndlessScriptComponent endlessScriptComponent = componentFromMap as EndlessScriptComponent;
				obj = endlessScriptComponent.GetEventObject(wireEntry.EmitterMemberName);
				array = endlessScriptComponent.GetEventInfo(wireEntry.EmitterMemberName).ParamList.Select((EndlessParameterInfo a) => EndlessTypeMapping.Instance.GetTypeFromId(a.DataType)).ToArray<Type>();
				if (obj == null)
				{
					foreach (MethodInfo methodInfo in typeof(WireBindings).GetMethods(BindingFlags.Static | BindingFlags.NonPublic))
					{
						if (methodInfo.Name == WireBindings.MAKE_ENDLESS_EVENT_NAME)
						{
							Type[] genericArguments = methodInfo.GetGenericArguments();
							if (array.Length == genericArguments.Length)
							{
								if (array.Length == 0)
								{
									obj = methodInfo.Invoke(null, null);
									break;
								}
								obj = methodInfo.MakeGenericMethod(array.ToArray<Type>()).Invoke(null, null);
								break;
							}
						}
					}
				}
				if (obj == null)
				{
					global::UnityEngine.Debug.LogError("Failed to create endless event for a lua event");
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
			if (array == null || array.Length == 0)
			{
				array = new Type[] { typeof(Context) };
			}
			else
			{
				array = new Type[] { typeof(Context) }.Concat(array).ToArray<Type>();
			}
			bool flag3 = false;
			if (flag2)
			{
				EndlessScriptComponent endlessScriptComponent2 = componentFromMap2 as EndlessScriptComponent;
				int count = endlessScriptComponent2.Script.Receivers.First((EndlessEventInfo r) => r.MemberName == wireEntry.ReceiverMemberName).ParamList.Count;
				foreach (MethodInfo methodInfo2 in typeof(WireBindings).GetMethods(BindingFlags.Static | BindingFlags.NonPublic))
				{
					if (methodInfo2.Name == WireBindings.BIND_LUA_NAME)
					{
						Type[] genericArguments2 = methodInfo2.GetGenericArguments();
						if (array.Length == genericArguments2.Length)
						{
							if (array.Length == 0)
							{
								methodInfo2.Invoke(null, new object[] { obj, wireEntry.ReceiverMemberName, endlessScriptComponent2, count, wireEntry.StaticParameters });
								flag3 = true;
								break;
							}
							methodInfo2.MakeGenericMethod(array).Invoke(null, new object[] { obj, wireEntry.ReceiverMemberName, endlessScriptComponent2, count, wireEntry.StaticParameters });
							flag3 = true;
							break;
						}
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
						Type[] array3 = new Type[] { typeof(Context) }.Concat(wireEntry.StaticParameters.Select((StoredParameter param) => EndlessTypeMapping.Instance.GetTypeFromId(param.DataType))).ToArray<Type>();
						methodInfo3 = type2.GetMethod(wireEntry.ReceiverMemberName, BindingFlags.Instance | BindingFlags.Public, null, array3, null);
					}
					else
					{
						if (array.Length != 0)
						{
							methodInfo3 = type2.GetMethod(wireEntry.ReceiverMemberName, BindingFlags.Instance | BindingFlags.Public, null, array, null);
						}
						if (methodInfo3 == null)
						{
							methodInfo3 = type2.GetMethod(wireEntry.ReceiverMemberName, BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(Context) }, null);
						}
					}
					if (methodInfo3 == null)
					{
						throw new NullReferenceException();
					}
				}
				catch (Exception ex)
				{
					global::UnityEngine.Debug.LogException(new Exception("Something went wrong targeting " + wireEntry.ReceiverComponentAssemblyQualifiedTypeName + ", " + wireEntry.ReceiverMemberName, ex));
					return;
				}
				foreach (MethodInfo methodInfo4 in typeof(WireBindings).GetMethods(BindingFlags.Static | BindingFlags.NonPublic))
				{
					if (methodInfo4.Name == WireBindings.PROCESS_NAME)
					{
						Type[] genericArguments3 = methodInfo4.GetGenericArguments();
						if (array.Length == genericArguments3.Length)
						{
							if (array.Length == 0)
							{
								methodInfo4.Invoke(null, new object[] { obj, methodInfo3, componentFromMap2, wireEntry.StaticParameters });
								flag3 = true;
								break;
							}
							methodInfo4.MakeGenericMethod(array).Invoke(null, new object[] { obj, methodInfo3, componentFromMap2, wireEntry.StaticParameters });
							flag3 = true;
							break;
						}
					}
				}
			}
			if (!flag3)
			{
				global::UnityEngine.Debug.LogError("Failed to find event to process wire");
			}
		}

		// Token: 0x06002194 RID: 8596 RVA: 0x00098E58 File Offset: 0x00097058
		private Component GetComponentFromMap(SerializableGuid propInstanceId, Type componentType, GameObject sourceObject)
		{
			if (!this.propComponentMap.ContainsKey(propInstanceId))
			{
				this.propComponentMap.Add(propInstanceId, new Dictionary<Type, Component>());
			}
			if (!this.propComponentMap[propInstanceId].ContainsKey(componentType))
			{
				this.propComponentMap[propInstanceId].Add(componentType, sourceObject.GetComponentInChildren(componentType));
			}
			return this.propComponentMap[propInstanceId][componentType];
		}

		// Token: 0x06002195 RID: 8597 RVA: 0x00098EC3 File Offset: 0x000970C3
		public Tileset TilesetAtIndex(int tilesetIndex)
		{
			return this.RuntimePalette.GetTileset(tilesetIndex);
		}

		// Token: 0x06002196 RID: 8598 RVA: 0x00098ED4 File Offset: 0x000970D4
		public async Task RespawnTerrainCellsWithTilesetIndexes(List<int> tilesetsToRespawn, CancellationToken cancelToken)
		{
			Stopwatch frameStopWatch = new Stopwatch();
			frameStopWatch.Start();
			Vector3Int[] array = this.offlineStage.GetCellCoordinates().ToArray<Vector3Int>();
			foreach (Vector3Int vector3Int in array)
			{
				Cell cell;
				if (this.offlineStage.TryGetCellFromCoordinate(vector3Int, out cell))
				{
					if (tilesetsToRespawn.Contains(cell.TilesetIndex))
					{
						this.SpawnTile(this.RuntimePalette.GetTileset(cell.TilesetIndex), cell as TerrainCell, false);
						this.offlineStage.ChunkManager.CellUpdated(cell.Coordinate);
					}
					if (frameStopWatch.ElapsedMilliseconds > 4L)
					{
						await Task.Yield();
						frameStopWatch.Restart();
					}
				}
			}
			Vector3Int[] array2 = null;
		}

		// Token: 0x06002197 RID: 8599 RVA: 0x00098F1F File Offset: 0x0009711F
		public IEnumerator RespawnTerrainCellsWithTilesetIndex(int tilesetIndexToRespawn, Action callback)
		{
			Tileset tileset = this.RuntimePalette.GetTileset(tilesetIndexToRespawn);
			Stopwatch frameStopWatch = new Stopwatch();
			frameStopWatch.Start();
			Vector3Int[] array = this.offlineStage.GetCellCoordinates().ToArray<Vector3Int>();
			foreach (Vector3Int vector3Int in array)
			{
				Cell cell;
				if (this.offlineStage.TryGetCellFromCoordinate(vector3Int, out cell))
				{
					Cell cell2 = cell;
					if (cell2.TilesetIndex == tilesetIndexToRespawn)
					{
						this.SpawnTile(tileset, cell2 as TerrainCell, false);
						this.offlineStage.ChunkManager.CellUpdated(cell2.Coordinate);
					}
					if (frameStopWatch.ElapsedMilliseconds > 4L)
					{
						yield return null;
						frameStopWatch.Restart();
					}
				}
			}
			Vector3Int[] array2 = null;
			if (callback != null)
			{
				callback();
			}
			yield break;
		}

		// Token: 0x06002198 RID: 8600 RVA: 0x00002DB0 File Offset: 0x00000FB0
		public void PropDestroyed(SerializableGuid instanceId)
		{
		}

		// Token: 0x06002199 RID: 8601 RVA: 0x00098F3C File Offset: 0x0009713C
		public List<PropEntry> GetReferenceFilteredPropEntries(ReferenceFilter filter)
		{
			List<PropEntry> list = new List<PropEntry>();
			List<string> list2 = (from d in MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetReferenceFilteredDefinitionList(filter)
				select d.PropData.AssetID).ToList<string>();
			foreach (PropEntry propEntry in this.LevelState.PropEntries)
			{
				if (list2.Contains(propEntry.AssetId))
				{
					list.Add(propEntry);
				}
			}
			return list;
		}

		// Token: 0x0600219A RID: 8602 RVA: 0x00098FE4 File Offset: 0x000971E4
		public void RespawnPropsWithAssetId(SerializableGuid assetId, bool wasMissingObject)
		{
			PropLibrary.RuntimePropInfo runtimePropInfo;
			if (!MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(assetId, out runtimePropInfo))
			{
				global::UnityEngine.Debug.LogError(string.Format("We expected to find a metadata object for asset id ({0}) but didn't find it!", assetId));
				return;
			}
			foreach (PropEntry propEntry in this.levelState.PropEntries)
			{
				if (!(propEntry.AssetId != assetId))
				{
					SerializableGuid instanceId = propEntry.InstanceId;
					GameObject gameObjectFromInstanceId = this.GetGameObjectFromInstanceId(instanceId);
					ulong num = 0UL;
					if (base.IsServer && runtimePropInfo.EndlessProp.IsNetworked && !wasMissingObject)
					{
						num = gameObjectFromInstanceId.GetComponent<NetworkObject>().NetworkObjectId;
						this.reverseGameObjectMap.Remove(gameObjectFromInstanceId);
					}
					this.DestroyStageObject(instanceId, runtimePropInfo, wasMissingObject);
					if (runtimePropInfo.EndlessProp.IsNetworked && !wasMissingObject)
					{
						if (base.IsServer)
						{
							this.pendingNetworkIds.Remove(num);
							this.gameObjectMap.Remove(instanceId);
							this.assetDefinitionMap.Remove(instanceId);
							this.ForceUntrackNetworkObject_ClientRpc(instanceId, num, wasMissingObject);
						}
						else if (gameObjectFromInstanceId != null)
						{
							Stage.PendingNetworkIdInfo pendingNetworkIdInfo = this.pendingNetworkIds.Values.FirstOrDefault((Stage.PendingNetworkIdInfo pending) => pending.InstanceId == instanceId);
							if (pendingNetworkIdInfo != null)
							{
								global::UnityEngine.Debug.Log("RespawnPropsWithAssetId:: Pending Network Info Existed. Calling TrackNetworkObject");
								this.TrackNetworkObject(pendingNetworkIdInfo.NetworkId, gameObjectFromInstanceId, null);
							}
						}
					}
					try
					{
						if (runtimePropInfo.EndlessProp.IsNetworked)
						{
							if (base.IsServer)
							{
								GameObject gameObject = global::UnityEngine.Object.Instantiate<EndlessProp>(runtimePropInfo.EndlessProp, propEntry.Position, propEntry.Rotation).gameObject;
								NetworkObject component = gameObject.GetComponent<NetworkObject>();
								if (component != null)
								{
									component.Spawn(false);
									this.TrackPendingNetworkId(component.NetworkObjectId, propEntry.InstanceId, false, null);
								}
								this.TrackNonNetworkedObject(propEntry.AssetId, propEntry.InstanceId, gameObject.gameObject, false);
								this.TrackPendingNetworkObject_ClientRpc(assetId, instanceId, component.NetworkObjectId, false, default(ClientRpcParams));
							}
						}
						else
						{
							GameObject gameObject2 = global::UnityEngine.Object.Instantiate<EndlessProp>(runtimePropInfo.EndlessProp, propEntry.Position, propEntry.Rotation).gameObject;
							this.TrackNonNetworkedObject(propEntry.AssetId, propEntry.InstanceId, gameObject2, false);
						}
					}
					catch (MissingReferenceException)
					{
						MonoBehaviourSingleton<StageManager>.Instance.PropFailedToLoad(propEntry);
					}
				}
			}
		}

		// Token: 0x0600219B RID: 8603 RVA: 0x0009928C File Offset: 0x0009748C
		[ClientRpc]
		private void ForceUntrackNetworkObject_ClientRpc(SerializableGuid instanceId, ulong networkObjectId, bool wasMissingObject)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(2409622067U, clientRpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<SerializableGuid>(in instanceId, default(FastBufferWriter.ForNetworkSerializable));
				BytePacker.WriteValueBitPacked(fastBufferWriter, networkObjectId);
				fastBufferWriter.WriteValueSafe<bool>(in wasMissingObject, default(FastBufferWriter.ForPrimitives));
				base.__endSendClientRpc(ref fastBufferWriter, 2409622067U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			GameObject gameObjectFromInstanceId = this.GetGameObjectFromInstanceId(instanceId);
			NetworkObject networkObject;
			if (!wasMissingObject && gameObjectFromInstanceId != null && gameObjectFromInstanceId.TryGetComponent<NetworkObject>(out networkObject) && networkObject.NetworkObjectId != networkObjectId)
			{
				return;
			}
			this.pendingNetworkIds.Remove(networkObjectId);
			this.gameObjectMap.Remove(instanceId);
			this.assetDefinitionMap.Remove(instanceId);
			if (gameObjectFromInstanceId != null)
			{
				this.reverseGameObjectMap.Remove(gameObjectFromInstanceId);
			}
		}

		// Token: 0x0600219C RID: 8604 RVA: 0x00099408 File Offset: 0x00097608
		public void ValidatePhysicalWiresStillAlive()
		{
			for (int i = this.physicalWires.Count - 1; i >= 0; i--)
			{
				bool flag = false;
				for (int j = 0; j < this.levelState.WireBundles.Count; j++)
				{
					if (!(this.levelState.WireBundles[j].BundleId != this.physicalWires[i].BundleId))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					global::UnityEngine.Object.Destroy(this.physicalWires[i].gameObject);
					this.physicalWires.RemoveAt(i);
				}
			}
		}

		// Token: 0x0600219D RID: 8605 RVA: 0x000994A4 File Offset: 0x000976A4
		private void DestroyStageObject(SerializableGuid instanceId, PropLibrary.RuntimePropInfo propInfo, bool wasMissingObject)
		{
			PropEntry propEntry = this.LevelState.GetPropEntry(instanceId);
			global::UnityEngine.Vector3 position = propEntry.Position;
			Quaternion rotation = propEntry.Rotation;
			Vector3Int[] array = (from x in this.GetRotatedPropLocations(propInfo.PropData, position, rotation)
				select x.Offset).ToArray<Vector3Int>();
			for (int i = this.physicalWires.Count - 1; i >= 0; i--)
			{
				PhysicalWire physicalWire = this.physicalWires[i];
				if (physicalWire.EmitterId == instanceId || physicalWire.ReceiverId == instanceId)
				{
					global::UnityEngine.Object.Destroy(physicalWire.gameObject);
					this.physicalWires.RemoveAt(i);
				}
			}
			if (this.propToWireConnectionPoints.ContainsKey(instanceId))
			{
				foreach (Transform transform in this.propToWireConnectionPoints[instanceId])
				{
					if (transform)
					{
						global::UnityEngine.Object.Destroy(transform.gameObject);
					}
				}
				this.propToWireConnectionPoints.Remove(instanceId);
			}
			GameObject gameObjectFromInstanceId = this.GetGameObjectFromInstanceId(instanceId);
			if (!propInfo.EndlessProp.IsNetworked || wasMissingObject || base.IsServer)
			{
				this.gameObjectMap.Remove(instanceId);
				this.assetDefinitionMap.Remove(instanceId);
				if (gameObjectFromInstanceId != null)
				{
					this.reverseGameObjectMap.Remove(gameObjectFromInstanceId);
					global::UnityEngine.Object.Destroy(gameObjectFromInstanceId);
				}
			}
			this.EraseCells(array);
		}

		// Token: 0x0600219E RID: 8606 RVA: 0x0009961C File Offset: 0x0009781C
		public bool PlacementIsValid(Prop prop, global::UnityEngine.Vector3 position, Quaternion rotation)
		{
			Vector3Int[] array = (from x in this.GetRotatedPropLocations(prop, position, rotation)
				select x.Offset).ToArray<Vector3Int>();
			bool flag = true;
			for (int i = 0; i < array.Length; i++)
			{
				if (!this.CheckValidity(array[i]))
				{
					flag = false;
					break;
				}
				if (!this.IsPositionInBounds(array[i]))
				{
					flag = false;
					break;
				}
			}
			return flag;
		}

		// Token: 0x0600219F RID: 8607 RVA: 0x00099694 File Offset: 0x00097894
		public bool PlacementIsValidAllowingSelfOverlap(Prop prop, global::UnityEngine.Vector3 position, Quaternion rotation, SerializableGuid selfInstanceId)
		{
			Vector3Int[] array = (from x in this.GetRotatedPropLocations(prop, position, rotation)
				select x.Offset).ToArray<Vector3Int>();
			bool flag = true;
			for (int i = 0; i < array.Length; i++)
			{
				Cell cellFromCoordinate = this.GetCellFromCoordinate(array[i]);
				if (cellFromCoordinate != null)
				{
					PropCell propCell = cellFromCoordinate as PropCell;
					if (propCell == null || propCell.InstanceId != selfInstanceId)
					{
						flag = false;
						break;
					}
				}
				if (!this.IsPositionInBounds(array[i]))
				{
					flag = false;
					break;
				}
			}
			return flag;
		}

		// Token: 0x060021A0 RID: 8608 RVA: 0x0009972C File Offset: 0x0009792C
		public PropLocationOffset[] GetRotatedPropLocations(Prop prop, global::UnityEngine.Vector3 position, Quaternion rotation)
		{
			if (prop.PropLocationOffsets == null)
			{
				return new PropLocationOffset[]
				{
					new PropLocationOffset
					{
						Offset = Vector3Int.zero
					}
				};
			}
			PropLocationOffset[] array = new PropLocationOffset[prop.PropLocationOffsets.Count];
			global::UnityEngine.Vector3 vector = new global::UnityEngine.Vector3(prop.ApplyXOffset ? 0.5f : 0f, 0f, prop.ApplyZOffset ? 0.5f : 0f);
			for (int i = 0; i < array.Length; i++)
			{
				global::UnityEngine.Vector3 vector2 = prop.PropLocationOffsets.ElementAt(i).Offset;
				vector2.x += vector.x;
				vector2.y += vector.y;
				vector2.z += vector.z;
				global::UnityEngine.Vector3 vector3 = rotation * vector2 + position;
				array[i] = new PropLocationOffset
				{
					Offset = Stage.WorldSpacePointToGridCoordinate(vector3)
				};
			}
			return array;
		}

		// Token: 0x060021A1 RID: 8609 RVA: 0x00099820 File Offset: 0x00097A20
		public void ConfigureIndicatorTransformToProp(Prop prop, Transform targetIndicator, SerializableGuid propInstanceId)
		{
			if (propInstanceId == SerializableGuid.Empty)
			{
				targetIndicator.rotation = Quaternion.identity;
				targetIndicator.transform.position = global::UnityEngine.Vector3.zero;
				targetIndicator.GetChild(0).localScale = global::UnityEngine.Vector3.zero;
				return;
			}
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetAssetIdFromInstanceId(propInstanceId);
			global::UnityEngine.Vector3 vector;
			Quaternion quaternion;
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(propInstanceId).transform.GetPositionAndRotation(out vector, out quaternion);
			Vector3Int boundingSize = prop.GetBoundingSize();
			Vector3Int[] array = (from x in this.GetRotatedPropLocations(prop, vector, quaternion)
				select x.Offset).ToArray<Vector3Int>();
			global::UnityEngine.Vector3 vector2 = Stage.<ConfigureIndicatorTransformToProp>g__GetMinimumPosition|179_1(array);
			global::UnityEngine.Vector3 vector3 = Stage.<ConfigureIndicatorTransformToProp>g__GetMaximumPosition|179_2(array);
			global::UnityEngine.Vector3 vector4 = (vector2 + vector3) * 0.5f;
			targetIndicator.gameObject.SetActive(true);
			targetIndicator.rotation = quaternion;
			targetIndicator.transform.position = vector4;
			targetIndicator.GetChild(0).localScale = boundingSize;
		}

		// Token: 0x060021A2 RID: 8610 RVA: 0x00099923 File Offset: 0x00097B23
		public bool TryGetCellFromCoordinate<T>(Vector3Int intersectedObjectPosition, out T propCell) where T : Cell
		{
			propCell = this.GetCellFromCoordinateAs<T>(intersectedObjectPosition);
			return propCell != null;
		}

		// Token: 0x060021A3 RID: 8611 RVA: 0x00099940 File Offset: 0x00097B40
		public void NetworkSetDefaultEnvironment(SerializableGuid instanceId)
		{
			this.SetDefaultEnvironment_ClientRpc(instanceId);
		}

		// Token: 0x060021A4 RID: 8612 RVA: 0x0009994C File Offset: 0x00097B4C
		[ClientRpc]
		private void SetDefaultEnvironment_ClientRpc(SerializableGuid instanceId)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(2206152673U, clientRpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<SerializableGuid>(in instanceId, default(FastBufferWriter.ForNetworkSerializable));
				base.__endSendClientRpc(ref fastBufferWriter, 2206152673U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.SetDefaultEnvironment(instanceId);
		}

		// Token: 0x060021A5 RID: 8613 RVA: 0x00099A4A File Offset: 0x00097C4A
		public void UpdateVersion(string assetVersion)
		{
			this.UpdateVersionLocal(assetVersion);
			this.UpdateVersion_ClientRpc(assetVersion);
		}

		// Token: 0x060021A6 RID: 8614 RVA: 0x00099A5C File Offset: 0x00097C5C
		[ClientRpc]
		private void UpdateVersion_ClientRpc(string assetVersion)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(687058185U, clientRpcParams, RpcDelivery.Reliable);
				bool flag = assetVersion != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag, default(FastBufferWriter.ForPrimitives));
				if (flag)
				{
					fastBufferWriter.WriteValueSafe(assetVersion, false);
				}
				base.__endSendClientRpc(ref fastBufferWriter, 687058185U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			if (!base.IsServer)
			{
				this.UpdateVersionLocal(assetVersion);
			}
		}

		// Token: 0x060021A7 RID: 8615 RVA: 0x00099B7C File Offset: 0x00097D7C
		private void UpdateVersionLocal(string assetVersion)
		{
			if (this.levelState != null)
			{
				AssetReference assetReference = this.levelState.ToAssetReference();
				this.levelState.AssetVersion = assetVersion;
				MonoBehaviourSingleton<StageManager>.Instance.UpdateStageVersion(assetReference, this.levelState.ToAssetReference());
			}
		}

		// Token: 0x060021A8 RID: 8616 RVA: 0x00099BC0 File Offset: 0x00097DC0
		public static void DebugDrawCell(Vector3Int cellPosition, global::UnityEngine.Color color, float duration)
		{
			QuadPlane[] cellFaces = Stage.CellFaces;
			for (int i = 0; i < cellFaces.Length; i++)
			{
				cellFaces[i].DebugDraw(cellPosition, color, duration);
			}
		}

		// Token: 0x060021AB RID: 8619 RVA: 0x00099F08 File Offset: 0x00098108
		[CompilerGenerated]
		private bool <Linecast>g__IsCoordinateInPotentialBounds|145_0(Vector3Int coordinate)
		{
			Vector3Int vector3Int = this.maximumExtents - this.minimumExtents;
			Vector3Int vector3Int2 = new Vector3Int(100, 100, 100) - vector3Int;
			Vector3Int vector3Int3 = this.minimumExtents - vector3Int2;
			Vector3Int vector3Int4 = this.maximumExtents + vector3Int2;
			return coordinate.x >= vector3Int3.x && coordinate.y >= vector3Int3.y && coordinate.z >= vector3Int3.z && coordinate.x <= vector3Int4.x && coordinate.y <= vector3Int4.y && coordinate.z <= vector3Int4.z;
		}

		// Token: 0x060021AC RID: 8620 RVA: 0x00099FB8 File Offset: 0x000981B8
		[CompilerGenerated]
		internal static global::UnityEngine.Vector3 <ConfigureIndicatorTransformToProp>g__GetMinimumPosition|179_1(Vector3Int[] cells)
		{
			Vector3Int vector3Int = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);
			foreach (Vector3Int vector3Int2 in cells)
			{
				if (vector3Int2.x < vector3Int.x || vector3Int2.y < vector3Int.y || vector3Int2.z < vector3Int.z)
				{
					vector3Int = vector3Int2;
				}
			}
			return vector3Int;
		}

		// Token: 0x060021AD RID: 8621 RVA: 0x0009A02C File Offset: 0x0009822C
		[CompilerGenerated]
		internal static global::UnityEngine.Vector3 <ConfigureIndicatorTransformToProp>g__GetMaximumPosition|179_2(Vector3Int[] cells)
		{
			Vector3Int vector3Int = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);
			foreach (Vector3Int vector3Int2 in cells)
			{
				if (vector3Int2.x > vector3Int.x || vector3Int2.y > vector3Int.y || vector3Int2.z > vector3Int.z)
				{
					vector3Int = vector3Int2;
				}
			}
			return vector3Int;
		}

		// Token: 0x060021AE RID: 8622 RVA: 0x0009A0A0 File Offset: 0x000982A0
		protected override void __initializeVariables()
		{
			bool flag = this.mapId == null;
			if (flag)
			{
				throw new Exception("Stage.mapId cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.mapId.Initialize(this);
			base.__nameNetworkVariable(this.mapId, "mapId");
			this.NetworkVariableFields.Add(this.mapId);
			flag = this.serverPropLoadFinished == null;
			if (flag)
			{
				throw new Exception("Stage.serverPropLoadFinished cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.serverPropLoadFinished.Initialize(this);
			base.__nameNetworkVariable(this.serverPropLoadFinished, "serverPropLoadFinished");
			this.NetworkVariableFields.Add(this.serverPropLoadFinished);
			base.__initializeVariables();
		}

		// Token: 0x060021AF RID: 8623 RVA: 0x0009A150 File Offset: 0x00098350
		protected override void __initializeRpcs()
		{
			base.__registerRpc(2231079621U, new NetworkBehaviour.RpcReceiveHandler(Stage.__rpc_handler_2231079621), "SendNetworkIds_ClientRpc");
			base.__registerRpc(667828874U, new NetworkBehaviour.RpcReceiveHandler(Stage.__rpc_handler_667828874), "TrackPendingNetworkObject_ClientRpc");
			base.__registerRpc(1781947305U, new NetworkBehaviour.RpcReceiveHandler(Stage.__rpc_handler_1781947305), "AddInstanceIdsToLevelState_ServerRpc");
			base.__registerRpc(1366621623U, new NetworkBehaviour.RpcReceiveHandler(Stage.__rpc_handler_1366621623), "AddInstanceIdsToLevelState_ClientRpc");
			base.__registerRpc(2409622067U, new NetworkBehaviour.RpcReceiveHandler(Stage.__rpc_handler_2409622067), "ForceUntrackNetworkObject_ClientRpc");
			base.__registerRpc(2206152673U, new NetworkBehaviour.RpcReceiveHandler(Stage.__rpc_handler_2206152673), "SetDefaultEnvironment_ClientRpc");
			base.__registerRpc(687058185U, new NetworkBehaviour.RpcReceiveHandler(Stage.__rpc_handler_687058185), "UpdateVersion_ClientRpc");
			base.__initializeRpcs();
		}

		// Token: 0x060021B0 RID: 8624 RVA: 0x0009A22C File Offset: 0x0009842C
		private static void __rpc_handler_2231079621(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			bool flag;
			reader.ReadValueSafe<bool>(out flag, default(FastBufferWriter.ForPrimitives));
			ulong[] array = null;
			if (flag)
			{
				reader.ReadValueSafe<ulong>(out array, default(FastBufferWriter.ForPrimitives));
			}
			bool flag2;
			reader.ReadValueSafe<bool>(out flag2, default(FastBufferWriter.ForPrimitives));
			SerializableGuid[] array2 = null;
			if (flag2)
			{
				reader.ReadValueSafe<SerializableGuid>(out array2, default(FastBufferWriter.ForNetworkSerializable));
			}
			bool flag3;
			reader.ReadValueSafe<bool>(out flag3, default(FastBufferWriter.ForPrimitives));
			SerializableGuid[] array3 = null;
			if (flag3)
			{
				reader.ReadValueSafe<SerializableGuid>(out array3, default(FastBufferWriter.ForNetworkSerializable));
			}
			ClientRpcParams client = rpcParams.Client;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((Stage)target).SendNetworkIds_ClientRpc(array, array2, array3, client);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060021B1 RID: 8625 RVA: 0x0009A368 File Offset: 0x00098568
		private static void __rpc_handler_667828874(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			SerializableGuid serializableGuid;
			reader.ReadValueSafe<SerializableGuid>(out serializableGuid, default(FastBufferWriter.ForNetworkSerializable));
			SerializableGuid serializableGuid2;
			reader.ReadValueSafe<SerializableGuid>(out serializableGuid2, default(FastBufferWriter.ForNetworkSerializable));
			ulong num;
			ByteUnpacker.ReadValueBitPacked(reader, out num);
			bool flag;
			reader.ReadValueSafe<bool>(out flag, default(FastBufferWriter.ForPrimitives));
			ClientRpcParams client = rpcParams.Client;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((Stage)target).TrackPendingNetworkObject_ClientRpc(serializableGuid, serializableGuid2, num, flag, client);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060021B2 RID: 8626 RVA: 0x0009A438 File Offset: 0x00098638
		private static void __rpc_handler_1781947305(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			bool flag;
			reader.ReadValueSafe<bool>(out flag, default(FastBufferWriter.ForPrimitives));
			SerializableGuid[] array = null;
			if (flag)
			{
				reader.ReadValueSafe<SerializableGuid>(out array, default(FastBufferWriter.ForNetworkSerializable));
			}
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((Stage)target).AddInstanceIdsToLevelState_ServerRpc(array);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060021B3 RID: 8627 RVA: 0x0009A4D4 File Offset: 0x000986D4
		private static void __rpc_handler_1366621623(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			bool flag;
			reader.ReadValueSafe<bool>(out flag, default(FastBufferWriter.ForPrimitives));
			SerializableGuid[] array = null;
			if (flag)
			{
				reader.ReadValueSafe<SerializableGuid>(out array, default(FastBufferWriter.ForNetworkSerializable));
			}
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((Stage)target).AddInstanceIdsToLevelState_ClientRpc(array);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060021B4 RID: 8628 RVA: 0x0009A570 File Offset: 0x00098770
		private static void __rpc_handler_2409622067(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			SerializableGuid serializableGuid;
			reader.ReadValueSafe<SerializableGuid>(out serializableGuid, default(FastBufferWriter.ForNetworkSerializable));
			ulong num;
			ByteUnpacker.ReadValueBitPacked(reader, out num);
			bool flag;
			reader.ReadValueSafe<bool>(out flag, default(FastBufferWriter.ForPrimitives));
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((Stage)target).ForceUntrackNetworkObject_ClientRpc(serializableGuid, num, flag);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060021B5 RID: 8629 RVA: 0x0009A610 File Offset: 0x00098810
		private static void __rpc_handler_2206152673(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			SerializableGuid serializableGuid;
			reader.ReadValueSafe<SerializableGuid>(out serializableGuid, default(FastBufferWriter.ForNetworkSerializable));
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((Stage)target).SetDefaultEnvironment_ClientRpc(serializableGuid);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060021B6 RID: 8630 RVA: 0x0009A680 File Offset: 0x00098880
		private static void __rpc_handler_687058185(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			bool flag;
			reader.ReadValueSafe<bool>(out flag, default(FastBufferWriter.ForPrimitives));
			string text = null;
			if (flag)
			{
				reader.ReadValueSafe(out text, false);
			}
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((Stage)target).UpdateVersion_ClientRpc(text);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060021B7 RID: 8631 RVA: 0x0009A70D File Offset: 0x0009890D
		protected internal override string __getTypeName()
		{
			return "Stage";
		}

		// Token: 0x04001A67 RID: 6759
		private const int STAGE_FALLOFF_HEIGHT_BUFFER = 5;

		// Token: 0x04001A68 RID: 6760
		public const int LOAD_LIMIT_MS = 64;

		// Token: 0x04001A69 RID: 6761
		public const int PLAY_LIMIT_MS = 4;

		// Token: 0x04001A6A RID: 6762
		private static global::UnityEngine.Vector3 topFrontLeft = new global::UnityEngine.Vector3(-0.5f, 0.5f, 0.5f);

		// Token: 0x04001A6B RID: 6763
		private static global::UnityEngine.Vector3 topFrontRight = new global::UnityEngine.Vector3(0.5f, 0.5f, 0.5f);

		// Token: 0x04001A6C RID: 6764
		private static global::UnityEngine.Vector3 bottomFrontLeft = new global::UnityEngine.Vector3(-0.5f, -0.5f, 0.5f);

		// Token: 0x04001A6D RID: 6765
		private static global::UnityEngine.Vector3 bottomFrontRight = new global::UnityEngine.Vector3(0.5f, -0.5f, 0.5f);

		// Token: 0x04001A6E RID: 6766
		private static global::UnityEngine.Vector3 topBackLeft = new global::UnityEngine.Vector3(-0.5f, 0.5f, -0.5f);

		// Token: 0x04001A6F RID: 6767
		private static global::UnityEngine.Vector3 topBackRight = new global::UnityEngine.Vector3(0.5f, 0.5f, -0.5f);

		// Token: 0x04001A70 RID: 6768
		private static global::UnityEngine.Vector3 bottomBackLeft = new global::UnityEngine.Vector3(-0.5f, -0.5f, -0.5f);

		// Token: 0x04001A71 RID: 6769
		private static global::UnityEngine.Vector3 bottomBackRight = new global::UnityEngine.Vector3(0.5f, -0.5f, -0.5f);

		// Token: 0x04001A72 RID: 6770
		public static readonly QuadPlane[] CellFaces = new QuadPlane[]
		{
			new QuadPlane(Stage.topFrontLeft, Stage.topFrontRight, Stage.bottomFrontRight, Stage.bottomFrontLeft),
			new QuadPlane(Stage.topBackLeft, Stage.topBackRight, Stage.bottomBackRight, Stage.bottomBackLeft),
			new QuadPlane(Stage.topFrontLeft, Stage.topBackLeft, Stage.bottomBackLeft, Stage.bottomFrontLeft),
			new QuadPlane(Stage.topFrontRight, Stage.topBackRight, Stage.bottomBackRight, Stage.bottomFrontRight),
			new QuadPlane(Stage.bottomFrontLeft, Stage.bottomFrontRight, Stage.bottomBackRight, Stage.bottomBackLeft),
			new QuadPlane(Stage.topFrontLeft, Stage.topFrontRight, Stage.topBackRight, Stage.topBackLeft)
		};

		// Token: 0x04001A73 RID: 6771
		[SerializeField]
		public Transform PropRoot;

		// Token: 0x04001A74 RID: 6772
		[SerializeField]
		private Vector3Int maxExtents = new Vector3Int(100, 20, 100);

		// Token: 0x04001A75 RID: 6773
		[SerializeField]
		private PhysicalWire physicalWireTemplate;

		// Token: 0x04001A76 RID: 6774
		[SerializeField]
		private WireColorDictionary wireColorDictionary;

		// Token: 0x04001A77 RID: 6775
		private readonly List<PhysicalWire> physicalWires = new List<PhysicalWire>();

		// Token: 0x04001A78 RID: 6776
		private readonly Dictionary<SerializableGuid, Transform[]> propToWireConnectionPoints = new Dictionary<SerializableGuid, Transform[]>();

		// Token: 0x04001A79 RID: 6777
		private Vector3Int minimumExtents = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);

		// Token: 0x04001A7A RID: 6778
		private Vector3Int maximumExtents = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);

		// Token: 0x04001A7B RID: 6779
		private readonly Dictionary<Vector3Int, Cell> cellLookup = new Dictionary<Vector3Int, Cell>();

		// Token: 0x04001A7C RID: 6780
		private LevelState levelState;

		// Token: 0x04001A7D RID: 6781
		private readonly Dictionary<SerializableGuid, Dictionary<Type, Component>> propComponentMap = new Dictionary<SerializableGuid, Dictionary<Type, Component>>();

		// Token: 0x04001A7E RID: 6782
		private readonly List<TerrainChange> terrainChanges = new List<TerrainChange>();

		// Token: 0x04001A81 RID: 6785
		private ProfilerMarker linecastMarker = new ProfilerMarker("linecastMarker");

		// Token: 0x04001A82 RID: 6786
		private ProfilerMarker respawnTerrainCellsWithTilesetIndexesMarker = new ProfilerMarker("respawnTerrainCellsWithTilesetIndexesMarker");

		// Token: 0x04001A83 RID: 6787
		private ProfilerMarker respawnTerrainCellsWithTilesetIndexMarker = new ProfilerMarker("respawnTerrainCellsWithTilesetIndexMarker");

		// Token: 0x04001A84 RID: 6788
		public NavMeshBuildSourceTracker BuildSourceTracker;

		// Token: 0x04001A85 RID: 6789
		private readonly Dictionary<ulong, Stage.PendingNetworkIdInfo> pendingNetworkIds = new Dictionary<ulong, Stage.PendingNetworkIdInfo>();

		// Token: 0x04001A86 RID: 6790
		private readonly Dictionary<ulong, GameObject> pendingNetworkObjects = new Dictionary<ulong, GameObject>();

		// Token: 0x04001A87 RID: 6791
		private readonly Dictionary<ulong, Action> pendingNetworkCallbacks = new Dictionary<ulong, Action>();

		// Token: 0x04001A88 RID: 6792
		private DepthPlane depthPlane;

		// Token: 0x04001A89 RID: 6793
		private readonly Dictionary<SerializableGuid, GameObject> gameObjectMap = new Dictionary<SerializableGuid, GameObject>();

		// Token: 0x04001A8A RID: 6794
		private readonly Dictionary<GameObject, SerializableGuid> reverseGameObjectMap = new Dictionary<GameObject, SerializableGuid>();

		// Token: 0x04001A8B RID: 6795
		private readonly Dictionary<SerializableGuid, SerializableGuid> assetDefinitionMap = new Dictionary<SerializableGuid, SerializableGuid>();

		// Token: 0x04001A8C RID: 6796
		private int activeTilesetIndex;

		// Token: 0x04001A8D RID: 6797
		private NetworkVariable<SerializableGuid> mapId = new NetworkVariable<SerializableGuid>(default(SerializableGuid), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x04001A8E RID: 6798
		private readonly NetworkVariable<bool> serverPropLoadFinished = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x04001A92 RID: 6802
		private readonly List<Stage.ClaimCellRequest> claimCellRequests = new List<Stage.ClaimCellRequest>();

		// Token: 0x04001A93 RID: 6803
		private readonly List<Stage.PropRegisteredRequest> propRegisteredRequest = new List<Stage.PropRegisteredRequest>();

		// Token: 0x04001A94 RID: 6804
		private OfflineStage offlineStage;

		// Token: 0x04001A95 RID: 6805
		private ProfilerMarker propLoadProfilerMarker = new ProfilerMarker("Prop loading");

		// Token: 0x04001A96 RID: 6806
		private ProfilerMarker terrainLoadProfilerMarker = new ProfilerMarker("Stage - Load Level - Terrain Loading");

		// Token: 0x04001A97 RID: 6807
		private ProfilerMarker terrainSpawnProfilerMarker = new ProfilerMarker("Stage - Load Level - Terrain Spawning");

		// Token: 0x04001A98 RID: 6808
		private ProfilerMarker stageSpawnTileMarker = new ProfilerMarker("Stage - Stage Spawn Tile");

		// Token: 0x04001A99 RID: 6809
		private ProfilerMarker stageUpdateNeighborsMarker = new ProfilerMarker("Stage - Stage Update Neighbors");

		// Token: 0x02000568 RID: 1384
		private class PendingNetworkIdInfo
		{
			// Token: 0x060021B8 RID: 8632 RVA: 0x0009A714 File Offset: 0x00098914
			public PendingNetworkIdInfo(ulong networkId, SerializableGuid instanceId, bool updateLevelState = true)
			{
				this.NetworkId = networkId;
				this.InstanceId = instanceId;
				this.UpdateLevelState = updateLevelState;
			}

			// Token: 0x04001A9B RID: 6811
			public readonly ulong NetworkId;

			// Token: 0x04001A9C RID: 6812
			public readonly SerializableGuid InstanceId;

			// Token: 0x04001A9D RID: 6813
			public readonly bool UpdateLevelState;
		}

		// Token: 0x02000569 RID: 1385
		private class ClaimCellRequest
		{
			// Token: 0x17000668 RID: 1640
			// (get) Token: 0x060021B9 RID: 8633 RVA: 0x0009A731 File Offset: 0x00098931
			public PropLibrary.RuntimePropInfo Definition { get; }

			// Token: 0x17000669 RID: 1641
			// (get) Token: 0x060021BA RID: 8634 RVA: 0x0009A739 File Offset: 0x00098939
			public SerializableGuid InstanceId { get; }

			// Token: 0x1700066A RID: 1642
			// (get) Token: 0x060021BB RID: 8635 RVA: 0x0009A741 File Offset: 0x00098941
			public ulong NetworkObjectId { get; }

			// Token: 0x060021BC RID: 8636 RVA: 0x0009A749 File Offset: 0x00098949
			public ClaimCellRequest(PropLibrary.RuntimePropInfo definition, SerializableGuid instanceId, ulong networkObjectId)
			{
				this.Definition = definition;
				this.InstanceId = instanceId;
				this.NetworkObjectId = networkObjectId;
			}
		}

		// Token: 0x0200056A RID: 1386
		private class PropRegisteredRequest
		{
			// Token: 0x1700066B RID: 1643
			// (get) Token: 0x060021BD RID: 8637 RVA: 0x0009A766 File Offset: 0x00098966
			public SerializableGuid InstanceId { get; }

			// Token: 0x060021BE RID: 8638 RVA: 0x0009A76E File Offset: 0x0009896E
			public PropRegisteredRequest(SerializableGuid instanceId)
			{
				this.InstanceId = instanceId;
			}
		}
	}
}
