using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Endless.Assets;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Creator.LevelEditing.Runtime
{
	// Token: 0x0200036C RID: 876
	public class MoveTool : PropBasedTool, IBackable
	{
		// Token: 0x1700027E RID: 638
		// (get) Token: 0x060010A1 RID: 4257 RVA: 0x0004FCC2 File Offset: 0x0004DEC2
		public override ToolType ToolType
		{
			get
			{
				return ToolType.Move;
			}
		}

		// Token: 0x060010A2 RID: 4258 RVA: 0x00047D40 File Offset: 0x00045F40
		public override void HandleSelected()
		{
			base.HandleSelected();
			base.Set3DCursorUsesIntersection(true);
			base.UIToolPrompter.Hide();
		}

		// Token: 0x060010A3 RID: 4259 RVA: 0x0004FCC8 File Offset: 0x0004DEC8
		public override void HandleDeselected()
		{
			base.HandleDeselected();
			if (this.hoveredPropId != SerializableGuid.Empty)
			{
				MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(this.hoveredPropId, PropLocationType.Selected);
				this.hoveredPropId = SerializableGuid.Empty;
			}
			base.Set3DCursorUsesIntersection(true);
			base.ClearLineCastExclusionId();
			if (this.movedObjectInstanceId.IsEmpty)
			{
				return;
			}
			MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(this.movedObjectInstanceId, PropLocationType.Selected);
			this.CancelPropMovement_ServerRpc(this.movedObjectInstanceId, default(ServerRpcParams));
		}

		// Token: 0x060010A4 RID: 4260 RVA: 0x0004FD4C File Offset: 0x0004DF4C
		public override void CreatorExited()
		{
			this.hoveredPropId = SerializableGuid.Empty;
			if (this.movedObjectInstanceId.IsEmpty)
			{
				return;
			}
			this.CancelPropMovement_ServerRpc(this.movedObjectInstanceId, default(ServerRpcParams));
			base.DestroyPreview();
		}

		// Token: 0x060010A5 RID: 4261 RVA: 0x0004FD8D File Offset: 0x0004DF8D
		public void OnBack()
		{
			this.HandleDeselected();
		}

		// Token: 0x060010A6 RID: 4262 RVA: 0x0004FD98 File Offset: 0x0004DF98
		private void HandlePropSelectionAttempt()
		{
			SerializableGuid propUnderCursor = base.GetPropUnderCursor();
			if (propUnderCursor.IsEmpty || this.propsBeingMoved.Contains(propUnderCursor))
			{
				return;
			}
			base.Set3DCursorUsesIntersection(false);
			if (this.movedObjectInstanceId != SerializableGuid.Empty)
			{
				MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(this.movedObjectInstanceId, PropLocationType.Selected);
			}
			this.movedObjectInstanceId = propUnderCursor;
			MonoBehaviourSingleton<PropLocationMarker>.Instance.MarkCellsForProp(this.movedObjectInstanceId, PropLocationType.Selected);
			this.SetPropAsMoving_ServerRpc(propUnderCursor, default(ServerRpcParams));
		}

		// Token: 0x060010A7 RID: 4263 RVA: 0x0004FE18 File Offset: 0x0004E018
		[ServerRpc(RequireOwnership = false)]
		private void SetPropAsMoving_ServerRpc(SerializableGuid instanceId, ServerRpcParams rpcParams = default(ServerRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(2231083241U, rpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<SerializableGuid>(in instanceId, default(FastBufferWriter.ForNetworkSerializable));
				base.__endSendServerRpc(ref fastBufferWriter, 2231083241U, rpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			if (this.propsBeingMoved.Contains(instanceId))
			{
				ClientRpcParams clientRpcParams = new ClientRpcParams
				{
					Send = new ClientRpcSendParams
					{
						TargetClientIds = new ulong[] { rpcParams.Receive.SenderClientId }
					}
				};
				this.RejectPropSelection_ClientRpc(instanceId, clientRpcParams);
				return;
			}
			this.SetPropAsMoving_ClientRpc(instanceId, rpcParams.Receive.SenderClientId);
			if (!base.IsClient)
			{
				this.SetPropAsMoving(instanceId, rpcParams.Receive.SenderClientId);
			}
		}

		// Token: 0x060010A8 RID: 4264 RVA: 0x0004FF80 File Offset: 0x0004E180
		[ClientRpc]
		private void SetPropAsMoving_ClientRpc(SerializableGuid instanceId, ulong playerId)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			SerializableGuid instanceId;
			ulong playerId;
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(40283873U, clientRpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<SerializableGuid>(in instanceId, default(FastBufferWriter.ForNetworkSerializable));
				BytePacker.WriteValueBitPacked(fastBufferWriter, playerId);
				base.__endSendClientRpc(ref fastBufferWriter, 40283873U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			instanceId = instanceId;
			playerId = playerId;
			if (NetworkBehaviourSingleton<CreatorManager>.Instance.RpcReceiveState == RpcReceiveState.Allow)
			{
				this.SetPropAsMoving(instanceId, playerId);
				return;
			}
			NetworkBehaviourSingleton<CreatorManager>.Instance.AddCachedRpc(delegate
			{
				this.SetPropAsMoving(instanceId, playerId);
			});
		}

		// Token: 0x060010A9 RID: 4265 RVA: 0x000500CC File Offset: 0x0004E2CC
		private void SetPropAsMoving(SerializableGuid instanceId, ulong playerId)
		{
			this.propsBeingMoved.Add(instanceId);
			if (playerId == NetworkManager.Singleton.LocalClientId)
			{
				SerializableGuid assetIdFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetAssetIdFromInstanceId(instanceId);
				PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(assetIdFromInstanceId);
				base.GeneratePropPreview(runtimePropInfo, this.movedObjectInstanceId);
				GameObject gameObjectFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(instanceId);
				base.PropGhostTransform.rotation = gameObjectFromInstanceId.transform.rotation;
				this.moveState = MoveTool.MoveState.Placing;
				MonoBehaviourSingleton<BackManager>.Instance.ClaimContext(this);
			}
		}

		// Token: 0x060010AA RID: 4266 RVA: 0x0005015C File Offset: 0x0004E35C
		[ClientRpc]
		private void RejectPropSelection_ClientRpc(SerializableGuid instanceId, ClientRpcParams rpcParams = default(ClientRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(1449843910U, rpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<SerializableGuid>(in instanceId, default(FastBufferWriter.ForNetworkSerializable));
				base.__endSendClientRpc(ref fastBufferWriter, 1449843910U, rpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			if (NetworkBehaviourSingleton<CreatorManager>.Instance.RpcReceiveState == RpcReceiveState.Allow)
			{
				SerializableGuid assetIdFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetAssetIdFromInstanceId(instanceId);
				PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(assetIdFromInstanceId);
				Debug.Log(string.Format("Unable to select Prop Id: {0}, Asset Name: {1}", instanceId, runtimePropInfo.PropData.Name));
				base.ClearLineCastExclusionId();
				if (!this.movedObjectInstanceId.IsEmpty)
				{
					MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(this.movedObjectInstanceId, PropLocationType.Selected);
					this.movedObjectInstanceId = SerializableGuid.Empty;
				}
			}
		}

		// Token: 0x060010AB RID: 4267 RVA: 0x000502C8 File Offset: 0x0004E4C8
		public override void ToolReleased()
		{
			base.ToolReleased();
			MoveTool.MoveState moveState = this.moveState;
			if (moveState == MoveTool.MoveState.None)
			{
				this.HandlePropSelectionAttempt();
				return;
			}
			if (moveState != MoveTool.MoveState.Placing)
			{
				return;
			}
			this.HandlePropPlacementAttempt();
		}

		// Token: 0x060010AC RID: 4268 RVA: 0x000502F8 File Offset: 0x0004E4F8
		private void HandlePropPlacementAttempt()
		{
			if (base.PropGhostTransform != null && base.NoInvalidOverlapsExist())
			{
				PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(base.ActiveAssetId);
				if (runtimePropInfo == null)
				{
					Debug.LogException(new Exception("Server was told to spawn invalid asset ID!"));
					return;
				}
				if (MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.PlacementIsValidAllowingSelfOverlap(runtimePropInfo.PropData, base.PropGhostTransform.position, Quaternion.Euler(base.PropGhostTransform.rotation.eulerAngles), this.movedObjectInstanceId))
				{
					base.Set3DCursorUsesIntersection(true);
					base.ClearLineCastExclusionId();
					MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(base.ActiveAssetId, PropLocationType.Selected);
					base.PropGhostTransform.gameObject.SetActive(false);
					this.AttemptMoveProp_ServerRpc(base.PropGhostTransform.position, base.PropGhostTransform.rotation.eulerAngles, base.ActiveAssetId, this.movedObjectInstanceId, default(ServerRpcParams));
				}
			}
		}

		// Token: 0x060010AD RID: 4269 RVA: 0x000503F4 File Offset: 0x0004E5F4
		[ServerRpc(RequireOwnership = false)]
		private void AttemptMoveProp_ServerRpc(Vector3 position, Vector3 eulerAngles, SerializableGuid assetId, SerializableGuid instanceId, ServerRpcParams rpcParams = default(ServerRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(1804559521U, rpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe(in position);
				fastBufferWriter.WriteValueSafe(in eulerAngles);
				fastBufferWriter.WriteValueSafe<SerializableGuid>(in assetId, default(FastBufferWriter.ForNetworkSerializable));
				fastBufferWriter.WriteValueSafe<SerializableGuid>(in instanceId, default(FastBufferWriter.ForNetworkSerializable));
				base.__endSendServerRpc(ref fastBufferWriter, 1804559521U, rpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.AttemptMoveProp(position, eulerAngles, assetId, instanceId, rpcParams);
		}

		// Token: 0x060010AE RID: 4270 RVA: 0x00050528 File Offset: 0x0004E728
		private async Task AttemptMoveProp(Vector3 position, Vector3 eulerAngles, SerializableGuid assetId, SerializableGuid instanceId, ServerRpcParams rpcParams)
		{
			if (base.IsServer)
			{
				int userId;
				if (!NetworkBehaviourSingleton<UserIdManager>.Instance.TryGetUserId(rpcParams.Receive.SenderClientId, out userId))
				{
					Debug.LogException(new Exception(string.Format("Unable to determine User Id for Client Id: {0}", rpcParams.Receive.SenderClientId)));
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
					PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(assetId);
					if (runtimePropInfo == null)
					{
						Debug.LogException(new Exception("Server was told to spawn invalid asset ID!"));
					}
					else if (MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.PlacementIsValidAllowingSelfOverlap(runtimePropInfo.PropData, position, Quaternion.Euler(eulerAngles), instanceId))
					{
						base.FirePropEvent("propMoved", assetId, runtimePropInfo.PropData.Name);
						this.MoveProp_ClientRpc(instanceId, assetId, position, eulerAngles, rpcParams.Receive.SenderClientId);
						if (!base.IsClient)
						{
							this.MoveProp(instanceId, assetId, position, eulerAngles, rpcParams.Receive.SenderClientId);
						}
						MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.RevisionTimestamp = DateTime.UtcNow.Ticks;
						MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.Changes.Add(new ChangeData
						{
							ChangeType = ChangeType.PropMoved,
							UserId = userId
						});
						NetworkBehaviourSingleton<CreatorManager>.Instance.SaveLoadManager.SaveLevel();
					}
				}
			}
		}

		// Token: 0x060010AF RID: 4271 RVA: 0x00050598 File Offset: 0x0004E798
		[ClientRpc]
		private void MoveProp_ClientRpc(SerializableGuid instanceId, SerializableGuid assetId, Vector3 position, Vector3 eulerAngles, ulong playerId)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			SerializableGuid instanceId;
			SerializableGuid assetId;
			Vector3 position;
			Vector3 eulerAngles;
			ulong playerId;
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(1737830098U, clientRpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<SerializableGuid>(in instanceId, default(FastBufferWriter.ForNetworkSerializable));
				fastBufferWriter.WriteValueSafe<SerializableGuid>(in assetId, default(FastBufferWriter.ForNetworkSerializable));
				fastBufferWriter.WriteValueSafe(in position);
				fastBufferWriter.WriteValueSafe(in eulerAngles);
				BytePacker.WriteValueBitPacked(fastBufferWriter, playerId);
				base.__endSendClientRpc(ref fastBufferWriter, 1737830098U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			instanceId = instanceId;
			assetId = assetId;
			position = position;
			eulerAngles = eulerAngles;
			playerId = playerId;
			if (NetworkBehaviourSingleton<CreatorManager>.Instance.RpcReceiveState == RpcReceiveState.Allow)
			{
				this.MoveProp(instanceId, assetId, position, eulerAngles, playerId);
				return;
			}
			NetworkBehaviourSingleton<CreatorManager>.Instance.AddCachedRpc(delegate
			{
				this.MoveProp(instanceId, assetId, position, eulerAngles, playerId);
			});
		}

		// Token: 0x060010B0 RID: 4272 RVA: 0x00050744 File Offset: 0x0004E944
		private void MoveProp(SerializableGuid instanceId, SerializableGuid assetId, Vector3 position, Vector3 eulerAngles, ulong playerId)
		{
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.MoveProp(assetId, instanceId, position, eulerAngles);
			GameObject gameObjectFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(instanceId);
			gameObjectFromInstanceId.transform.position = position;
			gameObjectFromInstanceId.transform.rotation = Quaternion.Euler(eulerAngles);
			this.propsBeingMoved.Remove(instanceId);
			if (playerId != NetworkManager.Singleton.LocalClientId)
			{
				return;
			}
			base.DestroyPreview();
			this.moveState = MoveTool.MoveState.None;
			MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(this.movedObjectInstanceId, PropLocationType.Selected);
			this.movedObjectInstanceId = SerializableGuid.Empty;
			this.hoveredPropId = SerializableGuid.Empty;
			MonoBehaviourSingleton<BackManager>.Instance.UnclaimContext(this);
		}

		// Token: 0x060010B1 RID: 4273 RVA: 0x000507F0 File Offset: 0x0004E9F0
		[ServerRpc(RequireOwnership = false)]
		private void CancelPropMovement_ServerRpc(SerializableGuid instanceId, ServerRpcParams rpcParams = default(ServerRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(2218965149U, rpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<SerializableGuid>(in instanceId, default(FastBufferWriter.ForNetworkSerializable));
				base.__endSendServerRpc(ref fastBufferWriter, 2218965149U, rpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.CancelPropMovement_ClientRpc(instanceId, rpcParams.Receive.SenderClientId);
			if (!base.IsClient)
			{
				this.CancelPropMovement(instanceId, rpcParams.Receive.SenderClientId);
			}
		}

		// Token: 0x060010B2 RID: 4274 RVA: 0x0005090C File Offset: 0x0004EB0C
		[ClientRpc]
		private void CancelPropMovement_ClientRpc(SerializableGuid instanceId, ulong playerId)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			SerializableGuid instanceId;
			ulong playerId;
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(149448483U, clientRpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<SerializableGuid>(in instanceId, default(FastBufferWriter.ForNetworkSerializable));
				BytePacker.WriteValueBitPacked(fastBufferWriter, playerId);
				base.__endSendClientRpc(ref fastBufferWriter, 149448483U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			instanceId = instanceId;
			playerId = playerId;
			if (NetworkBehaviourSingleton<CreatorManager>.Instance.RpcReceiveState == RpcReceiveState.Allow)
			{
				this.CancelPropMovement(instanceId, playerId);
				return;
			}
			NetworkBehaviourSingleton<CreatorManager>.Instance.AddCachedRpc(delegate
			{
				this.CancelPropMovement(instanceId, playerId);
			});
		}

		// Token: 0x060010B3 RID: 4275 RVA: 0x00050A58 File Offset: 0x0004EC58
		private void CancelPropMovement(SerializableGuid instanceId, ulong playerId)
		{
			this.propsBeingMoved.Remove(instanceId);
			if (playerId != NetworkManager.Singleton.LocalClientId)
			{
				return;
			}
			base.DestroyPreview();
			base.Set3DCursorUsesIntersection(true);
			this.moveState = MoveTool.MoveState.None;
			this.movedObjectInstanceId = SerializableGuid.Empty;
			MonoBehaviourSingleton<BackManager>.Instance.UnclaimContext(this);
		}

		// Token: 0x060010B4 RID: 4276 RVA: 0x00050AAC File Offset: 0x0004ECAC
		public override void UpdateTool()
		{
			base.UpdateTool();
			MoveTool.MoveState moveState = this.moveState;
			if (moveState == MoveTool.MoveState.None)
			{
				this.UpdateHighlightState();
				return;
			}
			if (moveState != MoveTool.MoveState.Placing)
			{
				return;
			}
			if (!base.IsMobile)
			{
				base.UpdatePropPlacement();
			}
		}

		// Token: 0x060010B5 RID: 4277 RVA: 0x00050AE4 File Offset: 0x0004ECE4
		private void UpdateHighlightState()
		{
			LineCastHit activeLineCastResult = base.ActiveLineCastResult;
			if (activeLineCastResult.IntersectionOccured)
			{
				PropCell cellFromCoordinateAs = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetCellFromCoordinateAs<PropCell>(activeLineCastResult.IntersectedObjectPosition);
				if (cellFromCoordinateAs != null)
				{
					if (cellFromCoordinateAs.InstanceId == this.hoveredPropId)
					{
						return;
					}
					if (this.hoveredPropId != SerializableGuid.Empty && this.hoveredPropId != cellFromCoordinateAs.InstanceId)
					{
						MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(this.hoveredPropId, PropLocationType.Selected);
					}
					this.hoveredPropId = cellFromCoordinateAs.InstanceId;
					MonoBehaviourSingleton<PropLocationMarker>.Instance.MarkCellsForProp(this.hoveredPropId, PropLocationType.Selected);
					return;
				}
				else if (!this.hoveredPropId.IsEmpty)
				{
					if (this.hoveredPropId != this.movedObjectInstanceId)
					{
						MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(this.hoveredPropId, PropLocationType.Selected);
					}
					this.hoveredPropId = SerializableGuid.Empty;
					return;
				}
			}
			else if (!this.hoveredPropId.IsEmpty)
			{
				if (this.hoveredPropId != this.movedObjectInstanceId)
				{
					MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(this.hoveredPropId, PropLocationType.Selected);
				}
				this.hoveredPropId = SerializableGuid.Empty;
			}
		}

		// Token: 0x060010B6 RID: 4278 RVA: 0x00050BFF File Offset: 0x0004EDFF
		public bool PropIsBeingMoved(SerializableGuid instanceId)
		{
			return this.propsBeingMoved.Contains(instanceId);
		}

		// Token: 0x060010B7 RID: 4279 RVA: 0x00050C10 File Offset: 0x0004EE10
		public override void ToolSecondaryPressed()
		{
			base.ToolSecondaryPressed();
			base.ClearLineCastExclusionId();
			this.CancelPropMovement_ServerRpc(this.movedObjectInstanceId, default(ServerRpcParams));
		}

		// Token: 0x060010B9 RID: 4281 RVA: 0x00050C5C File Offset: 0x0004EE5C
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x060010BA RID: 4282 RVA: 0x00050C74 File Offset: 0x0004EE74
		protected override void __initializeRpcs()
		{
			base.__registerRpc(2231083241U, new NetworkBehaviour.RpcReceiveHandler(MoveTool.__rpc_handler_2231083241), "SetPropAsMoving_ServerRpc");
			base.__registerRpc(40283873U, new NetworkBehaviour.RpcReceiveHandler(MoveTool.__rpc_handler_40283873), "SetPropAsMoving_ClientRpc");
			base.__registerRpc(1449843910U, new NetworkBehaviour.RpcReceiveHandler(MoveTool.__rpc_handler_1449843910), "RejectPropSelection_ClientRpc");
			base.__registerRpc(1804559521U, new NetworkBehaviour.RpcReceiveHandler(MoveTool.__rpc_handler_1804559521), "AttemptMoveProp_ServerRpc");
			base.__registerRpc(1737830098U, new NetworkBehaviour.RpcReceiveHandler(MoveTool.__rpc_handler_1737830098), "MoveProp_ClientRpc");
			base.__registerRpc(2218965149U, new NetworkBehaviour.RpcReceiveHandler(MoveTool.__rpc_handler_2218965149), "CancelPropMovement_ServerRpc");
			base.__registerRpc(149448483U, new NetworkBehaviour.RpcReceiveHandler(MoveTool.__rpc_handler_149448483), "CancelPropMovement_ClientRpc");
			base.__initializeRpcs();
		}

		// Token: 0x060010BB RID: 4283 RVA: 0x00050D50 File Offset: 0x0004EF50
		private static void __rpc_handler_2231083241(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			SerializableGuid serializableGuid;
			reader.ReadValueSafe<SerializableGuid>(out serializableGuid, default(FastBufferWriter.ForNetworkSerializable));
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((MoveTool)target).SetPropAsMoving_ServerRpc(serializableGuid, server);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060010BC RID: 4284 RVA: 0x00050DD0 File Offset: 0x0004EFD0
		private static void __rpc_handler_40283873(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
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
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((MoveTool)target).SetPropAsMoving_ClientRpc(serializableGuid, num);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060010BD RID: 4285 RVA: 0x00050E54 File Offset: 0x0004F054
		private static void __rpc_handler_1449843910(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			SerializableGuid serializableGuid;
			reader.ReadValueSafe<SerializableGuid>(out serializableGuid, default(FastBufferWriter.ForNetworkSerializable));
			ClientRpcParams client = rpcParams.Client;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((MoveTool)target).RejectPropSelection_ClientRpc(serializableGuid, client);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060010BE RID: 4286 RVA: 0x00050ED4 File Offset: 0x0004F0D4
		private static void __rpc_handler_1804559521(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			Vector3 vector;
			reader.ReadValueSafe(out vector);
			Vector3 vector2;
			reader.ReadValueSafe(out vector2);
			SerializableGuid serializableGuid;
			reader.ReadValueSafe<SerializableGuid>(out serializableGuid, default(FastBufferWriter.ForNetworkSerializable));
			SerializableGuid serializableGuid2;
			reader.ReadValueSafe<SerializableGuid>(out serializableGuid2, default(FastBufferWriter.ForNetworkSerializable));
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((MoveTool)target).AttemptMoveProp_ServerRpc(vector, vector2, serializableGuid, serializableGuid2, server);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060010BF RID: 4287 RVA: 0x00050F94 File Offset: 0x0004F194
		private static void __rpc_handler_1737830098(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
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
			Vector3 vector;
			reader.ReadValueSafe(out vector);
			Vector3 vector2;
			reader.ReadValueSafe(out vector2);
			ulong num;
			ByteUnpacker.ReadValueBitPacked(reader, out num);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((MoveTool)target).MoveProp_ClientRpc(serializableGuid, serializableGuid2, vector, vector2, num);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060010C0 RID: 4288 RVA: 0x00051058 File Offset: 0x0004F258
		private static void __rpc_handler_2218965149(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			SerializableGuid serializableGuid;
			reader.ReadValueSafe<SerializableGuid>(out serializableGuid, default(FastBufferWriter.ForNetworkSerializable));
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((MoveTool)target).CancelPropMovement_ServerRpc(serializableGuid, server);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060010C1 RID: 4289 RVA: 0x000510D8 File Offset: 0x0004F2D8
		private static void __rpc_handler_149448483(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
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
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((MoveTool)target).CancelPropMovement_ClientRpc(serializableGuid, num);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060010C2 RID: 4290 RVA: 0x00051159 File Offset: 0x0004F359
		protected internal override string __getTypeName()
		{
			return "MoveTool";
		}

		// Token: 0x04000DC3 RID: 3523
		private MoveTool.MoveState moveState;

		// Token: 0x04000DC4 RID: 3524
		private SerializableGuid movedObjectInstanceId = SerializableGuid.Empty;

		// Token: 0x04000DC5 RID: 3525
		private List<SerializableGuid> propsBeingMoved = new List<SerializableGuid>();

		// Token: 0x04000DC6 RID: 3526
		private SerializableGuid hoveredPropId;

		// Token: 0x0200036D RID: 877
		private enum MoveState
		{
			// Token: 0x04000DC8 RID: 3528
			None,
			// Token: 0x04000DC9 RID: 3529
			Placing
		}
	}
}
