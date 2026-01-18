using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Endless.Assets;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Props.Assets;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using Unity.Services.Analytics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Endless.Creator.LevelEditing.Runtime
{
	// Token: 0x02000351 RID: 849
	public class EraseTool : EndlessTool
	{
		// Token: 0x17000275 RID: 629
		// (get) Token: 0x06000FFA RID: 4090 RVA: 0x0004A296 File Offset: 0x00048496
		public override ToolType ToolType
		{
			get
			{
				return ToolType.Erase;
			}
		}

		// Token: 0x17000276 RID: 630
		// (get) Token: 0x06000FFB RID: 4091 RVA: 0x0004A299 File Offset: 0x00048499
		public EraseToolFunction CurrentFunction
		{
			get
			{
				return this.currentFunction;
			}
		}

		// Token: 0x06000FFC RID: 4092 RVA: 0x0004A2A1 File Offset: 0x000484A1
		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();
			if (base.IsClient)
			{
				this.wiringTool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<WiringTool>();
			}
		}

		// Token: 0x06000FFD RID: 4093 RVA: 0x0004A2C4 File Offset: 0x000484C4
		public override void UpdateTool()
		{
			base.UpdateTool();
			if (EndlessInput.GetKeyDown(Key.E))
			{
				this.erasingModeIndex = (this.erasingModeIndex + 1) % this.planeErasingModes.Length;
				Debug.Log("Switched to Painting Mode: " + this.GetActiveEraseMode().GetType().Name);
				this.UpdateToolPrompter();
			}
			else if (EndlessInput.GetKeyDown(Key.Q))
			{
				this.erasingModeIndex--;
				if (this.erasingModeIndex < 0)
				{
					this.erasingModeIndex = this.planeErasingModes.Length - 1;
				}
				Debug.Log("Switched to Painting Mode: " + this.GetActiveEraseMode().GetType().Name);
				this.UpdateToolPrompter();
			}
			if (this.isErasing)
			{
				return;
			}
			LineCastHit activeLineCastResult = base.ActiveLineCastResult;
			this.minimumErasedCells.Clear();
			this.minimumIgnoredCells.Clear();
			if (!activeLineCastResult.IntersectionOccured)
			{
				MonoBehaviourSingleton<MarkerManager>.Instance.ReleaseAllMarkers();
				return;
			}
			PropCell propCell = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetCellFromCoordinate(activeLineCastResult.IntersectedObjectPosition) as PropCell;
			if (propCell != null)
			{
				bool flag = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.PropIsWired(propCell.InstanceId);
				if (!MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.CanRemoveProp(MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetAssetIdFromInstanceId(propCell.InstanceId)))
				{
					this.minimumIgnoredCells.Add(activeLineCastResult.IntersectedObjectPosition);
				}
				else if (((this.currentFunction & EraseToolFunction.WiredProps) == EraseToolFunction.WiredProps && flag) || ((this.currentFunction & EraseToolFunction.UnwiredProps) == EraseToolFunction.UnwiredProps && !flag))
				{
					GameObject gameObjectFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(propCell.InstanceId);
					PropLibrary.RuntimePropInfo runtimePropInfo;
					if (gameObjectFromInstanceId && MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetAssetIdFromInstanceId(propCell.InstanceId), out runtimePropInfo))
					{
						PropLocationOffset[] rotatedPropLocations = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetRotatedPropLocations(runtimePropInfo.PropData, gameObjectFromInstanceId.transform.position, gameObjectFromInstanceId.transform.rotation);
						this.minimumErasedCells.AddRange(rotatedPropLocations.Select((PropLocationOffset x) => x.Offset));
					}
				}
				else
				{
					this.minimumIgnoredCells.Add(activeLineCastResult.IntersectedObjectPosition);
				}
			}
			else if (MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetCellFromCoordinate(activeLineCastResult.IntersectedObjectPosition) is TerrainCell)
			{
				if ((this.currentFunction & EraseToolFunction.Terrain) == EraseToolFunction.Terrain)
				{
					this.minimumErasedCells.Add(activeLineCastResult.IntersectedObjectPosition);
				}
				else
				{
					this.minimumIgnoredCells.Add(activeLineCastResult.IntersectedObjectPosition);
				}
			}
			MonoBehaviourSingleton<MarkerManager>.Instance.MarkCoordinates(this.minimumIgnoredCells, MarkerType.Ignore);
			MonoBehaviourSingleton<MarkerManager>.Instance.MarkCoordinates(this.minimumErasedCells, MarkerType.Erase);
		}

		// Token: 0x06000FFE RID: 4094 RVA: 0x0004A57C File Offset: 0x0004877C
		private void UpdateToolPrompter()
		{
			string displayName = this.planeErasingModes[this.erasingModeIndex].DisplayName;
			base.UIToolPrompter.Display("Erasing " + displayName + " (Q/E)", false);
		}

		// Token: 0x06000FFF RID: 4095 RVA: 0x0004A5B8 File Offset: 0x000487B8
		public override void HandleSelected()
		{
			base.Set3DCursorUsesIntersection(true);
			this.UpdateToolPrompter();
		}

		// Token: 0x06001000 RID: 4096 RVA: 0x0004A5C7 File Offset: 0x000487C7
		public override void HandleDeselected()
		{
			MonoBehaviourSingleton<MarkerManager>.Instance.ReleaseAllMarkers();
			this.isErasing = false;
		}

		// Token: 0x06001001 RID: 4097 RVA: 0x0004A5DC File Offset: 0x000487DC
		public override void ToolPressed()
		{
			LineCastHit activeLineCastResult = base.ActiveLineCastResult;
			if (!activeLineCastResult.IntersectionOccured)
			{
				return;
			}
			base.AutoPlace3DCursor = false;
			this.initialCoordinate = activeLineCastResult.IntersectedObjectPosition;
			this.previousRayOrigin = base.ActiveRay.origin;
			this.isErasing = true;
			for (int i = 0; i < this.planeErasingModes.Length; i++)
			{
				this.planeErasingModes[i].InputDown(this.initialCoordinate);
			}
		}

		// Token: 0x06001002 RID: 4098 RVA: 0x0004A64F File Offset: 0x0004884F
		private CellCollectionMode GetActiveEraseMode()
		{
			return this.planeErasingModes[this.erasingModeIndex];
		}

		// Token: 0x06001003 RID: 4099 RVA: 0x0004A660 File Offset: 0x00048860
		public override void ToolHeld()
		{
			if (!this.isErasing)
			{
				return;
			}
			Vector3 origin = base.ActiveRay.origin;
			HashSet<SerializableGuid> hashSet = new HashSet<SerializableGuid>();
			List<Vector3Int> list;
			if ((origin - this.previousRayOrigin).magnitude >= this.toolDragDeadZone)
			{
				MonoBehaviourSingleton<CellMarker>.Instance.MoveTo(this.GetActiveEraseMode().CurrentEndCoordinate);
				list = this.GetActiveEraseMode().CollectPaintedPositions(base.ActiveRay, this.maximumPositions);
			}
			else
			{
				MonoBehaviourSingleton<CellMarker>.Instance.MoveTo(this.initialCoordinate);
				list = new List<Vector3Int> { this.initialCoordinate };
			}
			this.erasedPositions.Clear();
			this.ignoredPositions.Clear();
			this.allErasedPropGuids.Clear();
			for (int i = 0; i < list.Count; i++)
			{
				Vector3Int vector3Int = list[i];
				Cell cellFromCoordinate = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetCellFromCoordinate(vector3Int);
				if (cellFromCoordinate == null)
				{
					this.ignoredPositions.Add(vector3Int);
				}
				else if (cellFromCoordinate is TerrainCell)
				{
					if ((this.currentFunction & EraseToolFunction.Terrain) == EraseToolFunction.Terrain)
					{
						this.erasedPositions.Add(vector3Int);
					}
					else
					{
						this.ignoredPositions.Add(vector3Int);
					}
				}
				else
				{
					PropCell propCell = cellFromCoordinate as PropCell;
					if (propCell != null && !hashSet.Contains(propCell.InstanceId))
					{
						hashSet.Add(propCell.InstanceId);
						bool flag = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.PropIsWired(propCell.InstanceId);
						if (!MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.CanRemoveProp(MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetAssetIdFromInstanceId(propCell.InstanceId)))
						{
							this.ignoredPositions.Add(vector3Int);
						}
						else if (((this.currentFunction & EraseToolFunction.WiredProps) == EraseToolFunction.WiredProps && flag) || ((this.currentFunction & EraseToolFunction.UnwiredProps) == EraseToolFunction.UnwiredProps && !flag))
						{
							this.allErasedPropGuids.Add(propCell.InstanceId);
							GameObject gameObjectFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(propCell.InstanceId);
							PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetAssetIdFromInstanceId(propCell.InstanceId));
							PropLocationOffset[] rotatedPropLocations = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetRotatedPropLocations(runtimePropInfo.PropData, gameObjectFromInstanceId.transform.position, gameObjectFromInstanceId.transform.rotation);
							for (int j = 0; j < rotatedPropLocations.Length; j++)
							{
								this.erasedPositions.Add(rotatedPropLocations[j].Offset);
							}
						}
						else
						{
							this.ignoredPositions.Add(vector3Int);
						}
					}
				}
			}
			MonoBehaviourSingleton<MarkerManager>.Instance.MarkCoordinates(this.erasedPositions, MarkerType.Erase);
			MonoBehaviourSingleton<MarkerManager>.Instance.MarkCoordinates(this.ignoredPositions, MarkerType.Ignore);
		}

		// Token: 0x06001004 RID: 4100 RVA: 0x0004A92C File Offset: 0x00048B2C
		public override void ToolReleased()
		{
			if (!this.isErasing)
			{
				return;
			}
			this.GetActiveEraseMode().InputReleased();
			if (this.GetActiveEraseMode().IsComplete())
			{
				base.AutoPlace3DCursor = true;
				this.AttemptErasing_ServerRPC(this.erasedPositions.ToArray<Vector3Int>(), this.allErasedPropGuids.ToArray<SerializableGuid>(), default(ServerRpcParams));
			}
			MonoBehaviourSingleton<MarkerManager>.Instance.ReleaseAllMarkers();
			this.isErasing = false;
		}

		// Token: 0x06001005 RID: 4101 RVA: 0x0004A998 File Offset: 0x00048B98
		public void ToggleCurrentFunction(EraseToolFunction function, bool state)
		{
			EraseToolFunction eraseToolFunction = this.currentFunction;
			if (state)
			{
				eraseToolFunction |= function;
			}
			else
			{
				eraseToolFunction &= ~function;
			}
			if (this.currentFunction != eraseToolFunction)
			{
				this.currentFunction = eraseToolFunction;
				this.OnFunctionChange.Invoke(this.currentFunction);
			}
		}

		// Token: 0x06001006 RID: 4102 RVA: 0x0004A9E0 File Offset: 0x00048BE0
		[ServerRpc(RequireOwnership = false)]
		private void AttemptErasing_ServerRPC(Vector3Int[] locations, SerializableGuid[] propsToErase, ServerRpcParams serverRpcParams = default(ServerRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(2785434172U, serverRpcParams, RpcDelivery.Reliable);
				bool flag = locations != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag, default(FastBufferWriter.ForPrimitives));
				if (flag)
				{
					fastBufferWriter.WriteValueSafe(locations);
				}
				bool flag2 = propsToErase != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag2, default(FastBufferWriter.ForPrimitives));
				if (flag2)
				{
					fastBufferWriter.WriteValueSafe<SerializableGuid>(propsToErase, default(FastBufferWriter.ForNetworkSerializable));
				}
				base.__endSendServerRpc(ref fastBufferWriter, 2785434172U, serverRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.AttemptErasing(locations, propsToErase, serverRpcParams);
		}

		// Token: 0x06001007 RID: 4103 RVA: 0x0004AB48 File Offset: 0x00048D48
		private async Task AttemptErasing(Vector3Int[] locations, SerializableGuid[] propsToErase, ServerRpcParams serverRpcParams)
		{
			if (base.IsServer)
			{
				int userId;
				if (!NetworkBehaviourSingleton<UserIdManager>.Instance.TryGetUserId(serverRpcParams.Receive.SenderClientId, out userId))
				{
					Debug.LogException(new Exception(string.Format("Unable to determine User Id for Client Id: {0}", serverRpcParams.Receive.SenderClientId)));
				}
				TaskAwaiter<bool> taskAwaiter = NetworkBehaviourSingleton<CreatorManager>.Instance.UserCanEditLevel(userId).GetAwaiter();
				if (!taskAwaiter.IsCompleted)
				{
					await taskAwaiter;
					TaskAwaiter<bool> taskAwaiter2;
					taskAwaiter = taskAwaiter2;
					taskAwaiter2 = default(TaskAwaiter<bool>);
				}
				if (taskAwaiter.GetResult())
				{
					List<Vector3Int> list = new List<Vector3Int>();
					foreach (Vector3Int vector3Int in locations)
					{
						if (MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.CanEraseTerrainCell(vector3Int))
						{
							list.Add(vector3Int);
						}
					}
					if (list.Count > 0 || propsToErase.Length != 0)
					{
						Vector3Int[] array = list.ToArray();
						List<SerializableGuid> list2 = this.wiringTool.FilterRerouteNodeGuids(propsToErase);
						HashSet<SerializableGuid> hashSet = propsToErase.ToHashSet<SerializableGuid>();
						HashSet<SerializableGuid> wiresUsingProps = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetWiresUsingProps(propsToErase);
						HashSet<SerializableGuid> hashSet2 = new HashSet<SerializableGuid>();
						foreach (SerializableGuid serializableGuid in wiresUsingProps)
						{
							foreach (SerializableGuid serializableGuid2 in MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.GetRerouteNodesFromWire(serializableGuid))
							{
								hashSet2.Add(serializableGuid2);
								list2.Remove(serializableGuid2);
							}
						}
						foreach (SerializableGuid serializableGuid3 in list2.Concat(hashSet2))
						{
							hashSet2.Add(serializableGuid3);
							hashSet.Remove(serializableGuid3);
						}
						for (int j = hashSet.Count - 1; j >= 0; j--)
						{
							SerializableGuid serializableGuid4 = hashSet.ElementAt(j);
							if (!MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.CanRemoveProp(MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetAssetIdFromInstanceId(serializableGuid4)))
							{
								hashSet.Remove(hashSet.ElementAt(j));
							}
						}
						PropEraseData[] array2 = new PropEraseData[hashSet.Count];
						for (int k = 0; k < array2.Length; k++)
						{
							GameObject gameObjectFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(hashSet.ElementAt(k));
							SerializableGuid assetIdFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetAssetIdFromInstanceId(hashSet.ElementAt(k));
							array2[k] = new PropEraseData
							{
								Position = gameObjectFromInstanceId.transform.position,
								Rotation = gameObjectFromInstanceId.transform.rotation,
								InstanceId = hashSet.ElementAt(k),
								AssetId = assetIdFromInstanceId
							};
							PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(assetIdFromInstanceId);
							CustomEvent customEvent = new CustomEvent("propErased");
							customEvent.Add("propName1", runtimePropInfo.PropData.Name);
							customEvent.Add("propId1", assetIdFromInstanceId.ToString());
							AnalyticsService.Instance.RecordEvent(customEvent);
						}
						if (array.Length != 0)
						{
							CustomEvent customEvent2 = new CustomEvent("terrainErased");
							customEvent2.Add("cellChangedCount", array.Length);
							AnalyticsService.Instance.RecordEvent(customEvent2);
						}
						this.AttemptErasing_ClientRPC(array, array2);
						this.wiringTool.EraseRerouteNodes_ServerRpc(list2.ToArray());
						if (!base.IsClient)
						{
							this.ApplyErasingResult(array, array2);
						}
						if (list.Count > 0)
						{
							MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.Changes.Add(new ChangeData
							{
								ChangeType = ChangeType.TerrainErased,
								UserId = userId
							});
						}
						if (propsToErase.Count<SerializableGuid>() > 0)
						{
							MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.Changes.Add(new ChangeData
							{
								ChangeType = ChangeType.PropErase,
								UserId = userId
							});
						}
						if (wiresUsingProps.Count<SerializableGuid>() > 0)
						{
							MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.Changes.Add(new ChangeData
							{
								ChangeType = ChangeType.WireDeleted,
								UserId = userId
							});
						}
						if (hashSet2.Count<SerializableGuid>() > 0)
						{
							MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.Changes.Add(new ChangeData
							{
								ChangeType = ChangeType.WireRerouteDeleted,
								UserId = userId
							});
						}
						MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.RevisionTimestamp = DateTime.UtcNow.Ticks;
						NetworkBehaviourSingleton<CreatorManager>.Instance.SaveLoadManager.SaveLevel();
					}
				}
			}
		}

		// Token: 0x06001008 RID: 4104 RVA: 0x0004ABA4 File Offset: 0x00048DA4
		[ClientRpc]
		private void AttemptErasing_ClientRPC(Vector3Int[] affectedCellPositions, PropEraseData[] propsToErase)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			Vector3Int[] affectedCellPositions;
			PropEraseData[] propsToErase;
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(2160773166U, clientRpcParams, RpcDelivery.Reliable);
				bool flag = affectedCellPositions != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag, default(FastBufferWriter.ForPrimitives));
				if (flag)
				{
					fastBufferWriter.WriteValueSafe(affectedCellPositions);
				}
				bool flag2 = propsToErase != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag2, default(FastBufferWriter.ForPrimitives));
				if (flag2)
				{
					fastBufferWriter.WriteValueSafe(in propsToErase);
				}
				base.__endSendClientRpc(ref fastBufferWriter, 2160773166U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			affectedCellPositions = affectedCellPositions;
			propsToErase = propsToErase;
			if (NetworkBehaviourSingleton<CreatorManager>.Instance.RpcReceiveState == RpcReceiveState.Allow)
			{
				this.ApplyErasingResult(affectedCellPositions, propsToErase);
				return;
			}
			NetworkBehaviourSingleton<CreatorManager>.Instance.AddCachedRpc(delegate
			{
				this.ApplyErasingResult(affectedCellPositions, propsToErase);
			});
		}

		// Token: 0x06001009 RID: 4105 RVA: 0x0004AD44 File Offset: 0x00048F44
		private void ApplyErasingResult(Vector3Int[] affectedTerrainCellPositions, PropEraseData[] propsToErase)
		{
			Debug.Log(string.Format("{0} - props to erase: {1}", "ApplyErasingResult", propsToErase.Length));
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.EraseCells(affectedTerrainCellPositions.ToList<Vector3Int>());
			this.wiringTool.DestroyWires(MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetWiresUsingProps(propsToErase.Select((PropEraseData propEraseData) => propEraseData.InstanceId)));
			foreach (PropEraseData propEraseData2 in propsToErase)
			{
				MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.RemoveProp(propEraseData2.InstanceId, propEraseData2.AssetId);
			}
		}

		// Token: 0x0600100A RID: 4106 RVA: 0x0004ADF0 File Offset: 0x00048FF0
		public override void Reset()
		{
			base.Reset();
			MonoBehaviourSingleton<MarkerManager>.Instance.ReleaseAllMarkers();
			this.isErasing = false;
		}

		// Token: 0x0600100C RID: 4108 RVA: 0x0004AEA0 File Offset: 0x000490A0
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x0600100D RID: 4109 RVA: 0x0004AEB8 File Offset: 0x000490B8
		protected override void __initializeRpcs()
		{
			base.__registerRpc(2785434172U, new NetworkBehaviour.RpcReceiveHandler(EraseTool.__rpc_handler_2785434172), "AttemptErasing_ServerRPC");
			base.__registerRpc(2160773166U, new NetworkBehaviour.RpcReceiveHandler(EraseTool.__rpc_handler_2160773166), "AttemptErasing_ClientRPC");
			base.__initializeRpcs();
		}

		// Token: 0x0600100E RID: 4110 RVA: 0x0004AF08 File Offset: 0x00049108
		private static void __rpc_handler_2785434172(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			bool flag;
			reader.ReadValueSafe<bool>(out flag, default(FastBufferWriter.ForPrimitives));
			Vector3Int[] array = null;
			if (flag)
			{
				reader.ReadValueSafe(out array);
			}
			bool flag2;
			reader.ReadValueSafe<bool>(out flag2, default(FastBufferWriter.ForPrimitives));
			SerializableGuid[] array2 = null;
			if (flag2)
			{
				reader.ReadValueSafe<SerializableGuid>(out array2, default(FastBufferWriter.ForNetworkSerializable));
			}
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((EraseTool)target).AttemptErasing_ServerRPC(array, array2, server);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x0600100F RID: 4111 RVA: 0x0004AFEC File Offset: 0x000491EC
		private static void __rpc_handler_2160773166(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			bool flag;
			reader.ReadValueSafe<bool>(out flag, default(FastBufferWriter.ForPrimitives));
			Vector3Int[] array = null;
			if (flag)
			{
				reader.ReadValueSafe(out array);
			}
			bool flag2;
			reader.ReadValueSafe<bool>(out flag2, default(FastBufferWriter.ForPrimitives));
			PropEraseData[] array2 = null;
			if (flag2)
			{
				reader.ReadValueSafe(out array2);
			}
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((EraseTool)target).AttemptErasing_ClientRPC(array, array2);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06001010 RID: 4112 RVA: 0x0004B0B3 File Offset: 0x000492B3
		protected internal override string __getTypeName()
		{
			return "EraseTool";
		}

		// Token: 0x04000D26 RID: 3366
		public UnityEvent<EraseToolFunction> OnFunctionChange = new UnityEvent<EraseToolFunction>();

		// Token: 0x04000D27 RID: 3367
		[SerializeField]
		private float toolDragDeadZone = 0.01f;

		// Token: 0x04000D28 RID: 3368
		[SerializeField]
		private int maximumPositions = 100;

		// Token: 0x04000D29 RID: 3369
		private WiringTool wiringTool;

		// Token: 0x04000D2A RID: 3370
		private Vector3Int initialCoordinate;

		// Token: 0x04000D2B RID: 3371
		private Vector3 previousRayOrigin;

		// Token: 0x04000D2C RID: 3372
		private bool isErasing;

		// Token: 0x04000D2D RID: 3373
		private EraseToolFunction currentFunction = (EraseToolFunction)3;

		// Token: 0x04000D2E RID: 3374
		private List<Vector3Int> minimumErasedCells = new List<Vector3Int>();

		// Token: 0x04000D2F RID: 3375
		private List<Vector3Int> minimumIgnoredCells = new List<Vector3Int>();

		// Token: 0x04000D30 RID: 3376
		private HashSet<SerializableGuid> allErasedPropGuids = new HashSet<SerializableGuid>();

		// Token: 0x04000D31 RID: 3377
		private HashSet<Vector3Int> erasedPositions = new HashSet<Vector3Int>();

		// Token: 0x04000D32 RID: 3378
		private HashSet<Vector3Int> ignoredPositions = new HashSet<Vector3Int>();

		// Token: 0x04000D33 RID: 3379
		private int erasingModeIndex;

		// Token: 0x04000D34 RID: 3380
		private CellCollectionMode[] planeErasingModes = new CellCollectionMode[]
		{
			new HorizontalPlaneCellCollectionMode(),
			new FacingPlaneCellCollectionMode(),
			new SidePlaneCellCollectionMode()
		};
	}
}
