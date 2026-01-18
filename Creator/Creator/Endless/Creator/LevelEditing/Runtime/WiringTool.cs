using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Endless.Assets;
using Endless.Creator.UI;
using Endless.Data;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.LevelEditing.Wires;
using Endless.Props.Assets;
using Endless.Props.Scripting;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Runtime.Shared.Matchmaking;
using Unity.Netcode;
using Unity.Services.Analytics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Endless.Creator.LevelEditing.Runtime
{
	// Token: 0x02000397 RID: 919
	public class WiringTool : EndlessTool
	{
		// Token: 0x170002A0 RID: 672
		// (get) Token: 0x060011CF RID: 4559 RVA: 0x0002B5D1 File Offset: 0x000297D1
		public override ToolType ToolType
		{
			get
			{
				return ToolType.Wiring;
			}
		}

		// Token: 0x170002A1 RID: 673
		// (get) Token: 0x060011D0 RID: 4560 RVA: 0x00057ABF File Offset: 0x00055CBF
		public new WiringTool.WiringToolState ToolState
		{
			get
			{
				return this.toolState;
			}
		}

		// Token: 0x060011D1 RID: 4561 RVA: 0x00057AC7 File Offset: 0x00055CC7
		private void Awake()
		{
			this.playerInputActions = new PlayerInputActions();
		}

		// Token: 0x060011D2 RID: 4562 RVA: 0x00057AD4 File Offset: 0x00055CD4
		private void Start()
		{
			NetworkBehaviourSingleton<UserIdManager>.Instance.OnUserIdAdded.AddListener(new UnityAction<int>(this.HandleUserIdAdded));
			NetworkBehaviourSingleton<UserIdManager>.Instance.OnUserIdRemoved.AddListener(new UnityAction<int>(this.HandleUserIdRemoved));
			MonoBehaviourSingleton<UIWiringManager>.Instance.OnWiringStateChanged.AddListener(new UnityAction<UIWiringStates>(this.OnUIWiringStateChange));
		}

		// Token: 0x060011D3 RID: 4563 RVA: 0x00057B34 File Offset: 0x00055D34
		private void OnEnable()
		{
			this.playerInputActions.Player.PopLastRerouteNode.performed += this.RequestPopLastRerouteNode;
			this.playerInputActions.Player.PopLastRerouteNode.Enable();
		}

		// Token: 0x060011D4 RID: 4564 RVA: 0x00057B80 File Offset: 0x00055D80
		private void OnDisable()
		{
			this.playerInputActions.Player.PopLastRerouteNode.performed -= this.RequestPopLastRerouteNode;
			this.playerInputActions.Player.PopLastRerouteNode.Disable();
		}

		// Token: 0x060011D5 RID: 4565 RVA: 0x00057BCC File Offset: 0x00055DCC
		private void OnUIWiringStateChange(UIWiringStates arg0)
		{
			int userId = NetworkBehaviourSingleton<UserIdManager>.Instance.GetUserId(NetworkManager.Singleton.LocalClientId);
			switch (arg0)
			{
			case UIWiringStates.Nothing:
			case UIWiringStates.CreateNew:
				break;
			case UIWiringStates.EditExisting:
				if (this.temporaryWires[userId] && this.temporaryWires[userId].BundleId == MonoBehaviourSingleton<UIWiringManager>.Instance.WireEditorController.WireToEdit.WireId)
				{
					return;
				}
				this.SetWireEditedByPlayer_ServerRpc(MonoBehaviourSingleton<UIWiringManager>.Instance.WireEditorController.WireToEdit.WireId, default(ServerRpcParams));
				break;
			default:
				return;
			}
		}

		// Token: 0x060011D6 RID: 4566 RVA: 0x00057C64 File Offset: 0x00055E64
		private void HandleUserIdAdded(int userId)
		{
			if (!this.playerToTempReroutes.ContainsKey(userId))
			{
				this.playerToTempReroutes.Add(userId, new List<SerializableGuid>());
			}
			if (!this.temporaryWires.ContainsKey(userId))
			{
				this.temporaryWires.Add(userId, null);
			}
		}

		// Token: 0x060011D7 RID: 4567 RVA: 0x00057CA0 File Offset: 0x00055EA0
		private void HandleUserIdRemoved(int userId)
		{
			if (this.temporaryWires.ContainsKey(userId))
			{
				PhysicalWire physicalWire = this.temporaryWires[userId];
				if (physicalWire)
				{
					if (physicalWire.IsBeingEdited)
					{
						physicalWire.SetPlaced();
					}
					if (this.temporaryWires[userId] && !this.temporaryWires[userId].IsPlaced)
					{
						global::UnityEngine.Object.Destroy(this.temporaryWires[userId].gameObject);
						this.temporaryWires[userId] = null;
					}
				}
				this.temporaryWires.Remove(userId);
			}
			if (!this.playerToTempReroutes.ContainsKey(userId))
			{
				return;
			}
			int count = this.playerToTempReroutes[userId].Count;
			this.playerToTempReroutes.Remove(userId);
		}

		// Token: 0x060011D8 RID: 4568 RVA: 0x00057D64 File Offset: 0x00055F64
		public override void CreatorExited()
		{
			this.DisableRerouteUndo.Invoke();
			MonoBehaviourSingleton<MarkerManager>.Instance.ReleaseAllMarkers();
			this.EmitterSelectionCancelled();
			this.TogglePhysicalWires(false);
			if (this.reroutePreview)
			{
				global::UnityEngine.Object.Destroy(this.reroutePreview.gameObject);
			}
			this.SetToolState(WiringTool.WiringToolState.Wiring);
		}

		// Token: 0x060011D9 RID: 4569 RVA: 0x00057DB8 File Offset: 0x00055FB8
		public override void HandleDeselected()
		{
			if (this.eventObjectInstanceId != SerializableGuid.Empty)
			{
				MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(this.eventObjectInstanceId, PropLocationType.Emitter);
			}
			if (this.receiverObjectInstanceId != SerializableGuid.Empty)
			{
				MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(this.receiverObjectInstanceId, PropLocationType.Receiver);
			}
			if (this.hoveredTarget != SerializableGuid.Empty)
			{
				MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(this.hoveredTarget, PropLocationType.Default);
				MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(this.hoveredTarget, PropLocationType.Selected);
				MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(this.hoveredTarget, PropLocationType.Receiver);
				MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(this.hoveredTarget, PropLocationType.Emitter);
			}
			this.DisableRerouteUndo.Invoke();
			MonoBehaviourSingleton<MarkerManager>.Instance.ReleaseAllMarkers();
			int activeUserId = EndlessServices.Instance.CloudService.ActiveUserId;
			PhysicalWire physicalWire;
			if (this.temporaryWires.TryGetValue(activeUserId, out physicalWire) && physicalWire)
			{
				if (physicalWire.IsBeingEdited)
				{
					physicalWire.SetPlaced();
				}
				if (this.temporaryWires[activeUserId] && !this.temporaryWires[activeUserId].IsPlaced)
				{
					global::UnityEngine.Object.Destroy(this.temporaryWires[activeUserId].gameObject);
					this.temporaryWires[activeUserId] = null;
				}
			}
			this.EmitterSelectionCancelled();
			this.TogglePhysicalWires(false);
			this.WiringToolExited_ServerRpc(this.pendingRerouteNodes.ToArray(), default(ServerRpcParams));
			if (this.reroutePreview)
			{
				global::UnityEngine.Object.Destroy(this.reroutePreview.gameObject);
			}
			this.SetToolState(WiringTool.WiringToolState.Wiring);
		}

		// Token: 0x060011DA RID: 4570 RVA: 0x00057F40 File Offset: 0x00056140
		[ServerRpc(RequireOwnership = false)]
		private void ClearPendingRerouteNodesForPlayer_ServerRpc(SerializableGuid[] rerouteNodes, ServerRpcParams rpcParams = default(ServerRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(405623707U, rpcParams, RpcDelivery.Reliable);
				bool flag = rerouteNodes != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag, default(FastBufferWriter.ForPrimitives));
				if (flag)
				{
					fastBufferWriter.WriteValueSafe<SerializableGuid>(rerouteNodes, default(FastBufferWriter.ForNetworkSerializable));
				}
				base.__endSendServerRpc(ref fastBufferWriter, 405623707U, rpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage;
		}

		// Token: 0x060011DB RID: 4571 RVA: 0x00058070 File Offset: 0x00056270
		[ServerRpc(RequireOwnership = false)]
		private void WiringToolExited_ServerRpc(SerializableGuid[] rerouteNodes, ServerRpcParams rpcParams = default(ServerRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(4103524712U, rpcParams, RpcDelivery.Reliable);
				bool flag = rerouteNodes != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag, default(FastBufferWriter.ForPrimitives));
				if (flag)
				{
					fastBufferWriter.WriteValueSafe<SerializableGuid>(rerouteNodes, default(FastBufferWriter.ForNetworkSerializable));
				}
				base.__endSendServerRpc(ref fastBufferWriter, 4103524712U, rpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			int userId = NetworkBehaviourSingleton<UserIdManager>.Instance.GetUserId(rpcParams.Receive.SenderClientId);
			this.WiringToolExited_ClientRpc(userId);
			if (!base.IsClient)
			{
				this.ClearTemporaryWireForPlayer(userId);
			}
		}

		// Token: 0x060011DC RID: 4572 RVA: 0x000581BC File Offset: 0x000563BC
		[ClientRpc]
		private void WiringToolExited_ClientRpc(int userId)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(1791207916U, clientRpcParams, RpcDelivery.Reliable);
				BytePacker.WriteValueBitPacked(fastBufferWriter, userId);
				base.__endSendClientRpc(ref fastBufferWriter, 1791207916U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.ClearTemporaryWireForPlayer(userId);
		}

		// Token: 0x060011DD RID: 4573 RVA: 0x000582A3 File Offset: 0x000564A3
		public override void HandleSelected()
		{
			this.EmitterSelectionCancelled();
			base.Set3DCursorUsesIntersection(true);
			this.TogglePhysicalWires(true);
			this.SetCellMarkerToEmitterColor();
		}

		// Token: 0x060011DE RID: 4574 RVA: 0x000582C0 File Offset: 0x000564C0
		private void SetCellMarkerToEmitterColor()
		{
			base.AutoPlace3DCursor = true;
			MonoBehaviourSingleton<CellMarker>.Instance.MoveTo(base.ActiveLineCastResult.NearestPositionToObject);
			MonoBehaviourSingleton<CellMarker>.Instance.SetColor(new Color(0.7f, 0.4f, 0.2f, 1f));
		}

		// Token: 0x060011DF RID: 4575 RVA: 0x00058310 File Offset: 0x00056510
		private void SetCellMarkerToReceiverColor()
		{
			base.AutoPlace3DCursor = true;
			MonoBehaviourSingleton<CellMarker>.Instance.MoveTo(base.ActiveLineCastResult.NearestPositionToObject);
			MonoBehaviourSingleton<CellMarker>.Instance.SetColor(new Color(0.2f, 0.4f, 0.7f, 1f));
		}

		// Token: 0x060011E0 RID: 4576 RVA: 0x00058360 File Offset: 0x00056560
		private void TogglePhysicalWires(bool active)
		{
			if (MonoBehaviourSingleton<StageManager>.Instance.ActiveStage)
			{
				foreach (PhysicalWire physicalWire in MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.PhysicalWires)
				{
					physicalWire.gameObject.SetActive(active);
				}
			}
			foreach (PhysicalWire physicalWire2 in this.temporaryWires.Values)
			{
				if (physicalWire2)
				{
					physicalWire2.gameObject.SetActive(active);
				}
			}
		}

		// Token: 0x060011E1 RID: 4577 RVA: 0x00058424 File Offset: 0x00056624
		public override void UpdateTool()
		{
			base.UpdateTool();
			if (this.toolState == WiringTool.WiringToolState.Rerouting)
			{
				LineCastHit activeLineCastResult = base.ActiveLineCastResult;
				this.reroutePreview.transform.position = activeLineCastResult.NearestPositionToObject;
				return;
			}
			WiringTool.WiringStage wiringStage = this.currentWiringStage;
			if (wiringStage == WiringTool.WiringStage.SelectingEventObject || wiringStage == WiringTool.WiringStage.SelectingReceiverObject)
			{
				this.CalculateTarget();
				if (this.wiringTarget == WiringTool.WiringTarget.Prop)
				{
					PropCell propCell = this.wiringTargetPropCell;
					if (propCell.InstanceId == this.hoveredTarget)
					{
						return;
					}
					Color white = Color.white;
					bool flag = false;
					if (this.hoveredTarget == SerializableGuid.Empty || this.hoveredTarget != propCell.InstanceId)
					{
						if (this.hoveredTarget != SerializableGuid.Empty && this.hoveredTarget != propCell.InstanceId)
						{
							MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(this.hoveredTarget, PropLocationType.NoAction);
							MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(this.hoveredTarget, PropLocationType.Default);
							if (this.eventObjectInstanceId != this.hoveredTarget)
							{
								MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(this.hoveredTarget, PropLocationType.Emitter);
							}
							if (this.receiverObjectInstanceId != this.hoveredTarget)
							{
								MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(this.hoveredTarget, PropLocationType.Receiver);
							}
						}
						SerializableGuid assetIdFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetAssetIdFromInstanceId(propCell.InstanceId);
						PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(assetIdFromInstanceId);
						wiringStage = this.currentWiringStage;
						if (wiringStage != WiringTool.WiringStage.SelectingEventObject)
						{
							if (wiringStage != WiringTool.WiringStage.SelectingReceiverObject)
							{
								Color gray = Color.gray;
							}
							else
							{
								Script script = runtimePropInfo.EndlessProp.ScriptComponent.Script;
								List<EndlessEventInfo> list = ((script != null) ? script.Receivers : null) ?? new List<EndlessEventInfo>();
								IEnumerable<EndlessEventInfo> enumerable = runtimePropInfo.GetAllDefinitions().SelectMany((ComponentDefinition definition) => definition.AvailableReceivers);
								flag = list.Any<EndlessEventInfo>() || enumerable.Any<EndlessEventInfo>();
							}
						}
						else
						{
							Script script2 = runtimePropInfo.EndlessProp.ScriptComponent.Script;
							List<EndlessEventInfo> list2 = ((script2 != null) ? script2.Events : null) ?? new List<EndlessEventInfo>();
							IEnumerable<EndlessEventInfo> enumerable2 = runtimePropInfo.GetAllDefinitions().SelectMany((ComponentDefinition definition) => definition.AvailableEvents);
							flag = list2.Any<EndlessEventInfo>() || enumerable2.Any<EndlessEventInfo>();
						}
					}
					this.hoveredTarget = propCell.InstanceId;
					wiringStage = this.currentWiringStage;
					PropLocationType propLocationType;
					if (wiringStage != WiringTool.WiringStage.SelectingEventObject)
					{
						if (wiringStage != WiringTool.WiringStage.SelectingReceiverObject)
						{
							propLocationType = (flag ? PropLocationType.Default : PropLocationType.NoAction);
						}
						else
						{
							propLocationType = (flag ? PropLocationType.Receiver : PropLocationType.NoAction);
						}
					}
					else
					{
						propLocationType = (flag ? PropLocationType.Emitter : PropLocationType.NoAction);
					}
					MonoBehaviourSingleton<PropLocationMarker>.Instance.MarkCellsForProp(this.hoveredTarget, propLocationType);
					if (this.hoveredWire)
					{
						this.hoveredWire.Unhovered();
						this.hoveredWire = null;
						return;
					}
				}
				else if (this.wiringTarget == WiringTool.WiringTarget.PhysicalWire)
				{
					PhysicalWire physicalWire = this.wiringTargetPhysicalWire;
					if (physicalWire != this.hoveredWire)
					{
						if (this.hoveredWire)
						{
							this.hoveredWire.Unhovered();
						}
						this.hoveredWire = physicalWire;
						this.hoveredWire.Hover();
						base.AutoPlace3DCursor = false;
						MonoBehaviourSingleton<CellMarker>.Instance.MoveTo(new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue));
					}
					else if (this.hoveredWire != null && physicalWire != this.hoveredWire)
					{
						Debug.Log("hoveredWire test");
						base.AutoPlace3DCursor = true;
						MonoBehaviourSingleton<CellMarker>.Instance.MoveTo(base.ActiveLineCastResult.NearestPositionToObject);
						this.hoveredWire.Unhovered();
						this.hoveredWire = null;
					}
					if (this.hoveredTarget != SerializableGuid.Empty)
					{
						MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(this.hoveredTarget, PropLocationType.NoAction);
						MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(this.hoveredTarget, PropLocationType.Default);
						if (this.eventObjectInstanceId != this.hoveredTarget)
						{
							MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(this.hoveredTarget, PropLocationType.Emitter);
						}
						if (this.receiverObjectInstanceId != this.hoveredTarget)
						{
							MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(this.hoveredTarget, PropLocationType.Receiver);
						}
						this.hoveredTarget = SerializableGuid.Empty;
						return;
					}
				}
				else
				{
					if (this.hoveredTarget != SerializableGuid.Empty)
					{
						MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(this.hoveredTarget, PropLocationType.NoAction);
						MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(this.hoveredTarget, PropLocationType.Default);
						if (this.eventObjectInstanceId != this.hoveredTarget)
						{
							MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(this.hoveredTarget, PropLocationType.Emitter);
						}
						if (this.receiverObjectInstanceId != this.hoveredTarget)
						{
							MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(this.hoveredTarget, PropLocationType.Receiver);
						}
						this.hoveredTarget = SerializableGuid.Empty;
					}
					if (this.hoveredWire != null)
					{
						base.AutoPlace3DCursor = true;
						MonoBehaviourSingleton<CellMarker>.Instance.MoveTo(base.ActiveLineCastResult.NearestPositionToObject);
						this.hoveredWire.Unhovered();
						this.hoveredWire = null;
					}
				}
			}
		}

		// Token: 0x060011E2 RID: 4578 RVA: 0x0005890C File Offset: 0x00056B0C
		private void AttemptHighlightWire()
		{
			RaycastHit raycastHit;
			if (base.PerformRaycast(out raycastHit, LayerMask.GetMask(new string[] { "Wires" })))
			{
				PhysicalWire physicalWire;
				if (raycastHit.transform.TryGetComponent<PhysicalWire>(out physicalWire))
				{
					if (physicalWire != this.hoveredWire)
					{
						if (this.hoveredWire)
						{
							this.hoveredWire.Unhovered();
						}
						this.hoveredWire = physicalWire;
						this.hoveredWire.Hover();
						return;
					}
				}
				else if (this.hoveredWire)
				{
					this.hoveredWire.Unhovered();
					this.hoveredWire = null;
					return;
				}
			}
			else if (this.hoveredWire)
			{
				this.hoveredWire.Unhovered();
				this.hoveredWire = null;
			}
		}

		// Token: 0x060011E3 RID: 4579 RVA: 0x000589C0 File Offset: 0x00056BC0
		public void SetToolState(WiringTool.WiringToolState wiringToolState)
		{
			this.toolState = wiringToolState;
			WiringTool.WiringToolState wiringToolState2 = this.toolState;
			if (wiringToolState2 != WiringTool.WiringToolState.Wiring)
			{
				if (wiringToolState2 != WiringTool.WiringToolState.Rerouting)
				{
					Debug.LogWarningFormat(this, "WiringTool's SetWiringStage method does not support a WiringToolState value of '{0}'", new object[] { this.toolState });
				}
				else if (!this.reroutePreview)
				{
				}
			}
			else if (this.reroutePreview)
			{
				global::UnityEngine.Object.Destroy(this.reroutePreview.gameObject);
			}
			this.OnToolStateChanged.Invoke(wiringToolState);
		}

		// Token: 0x060011E4 RID: 4580 RVA: 0x00058A40 File Offset: 0x00056C40
		public override void ToolReleased()
		{
			Debug.LogWarning(string.Format("Tool released: {0} - {1}", this.toolState, this.currentWiringStage));
			if (this.toolState == WiringTool.WiringToolState.Wiring)
			{
				WiringTool.WiringStage wiringStage = this.currentWiringStage;
				RaycastHit raycastHit;
				if (wiringStage == WiringTool.WiringStage.SelectingEventObject || wiringStage == WiringTool.WiringStage.SelectingReceiverObject)
				{
					if (this.wiringTarget == WiringTool.WiringTarget.PhysicalWire)
					{
						this.SelectPhysicalWire(this.wiringTargetPhysicalWire);
						return;
					}
					if (this.wiringTarget == WiringTool.WiringTarget.Prop)
					{
						PropCell propCell = this.wiringTargetPropCell;
						SerializableGuid assetIdFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetAssetIdFromInstanceId(propCell.InstanceId);
						MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(assetIdFromInstanceId);
						wiringStage = this.currentWiringStage;
						if (wiringStage != WiringTool.WiringStage.SelectingEventObject)
						{
							if (wiringStage != WiringTool.WiringStage.SelectingReceiverObject)
							{
								return;
							}
							if (propCell.InstanceId != this.eventObjectInstanceId)
							{
								this.currentWiringStage = WiringTool.WiringStage.AwaitingConfirmation;
								this.receiverObjectInstanceId = propCell.InstanceId;
								this.SetTemporaryWireTarget_ServerRpc(this.receiverObjectInstanceId, default(ServerRpcParams));
								this.CollectReceiverEventInfos(assetIdFromInstanceId);
								this.OnReceiverObjectSelected.Invoke(propCell.CellBase, this.receiverObjectInstanceId, this.GetUiReceiverLists(assetIdFromInstanceId));
								base.AutoPlace3DCursor = false;
								MonoBehaviourSingleton<CellMarker>.Instance.MoveTo(new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue));
								MonoBehaviourSingleton<PropLocationMarker>.Instance.MarkCellsForProp(this.receiverObjectInstanceId, PropLocationType.Receiver);
								return;
							}
						}
						else
						{
							this.eventObjectInstanceId = propCell.InstanceId;
							if (this.eventObjectInstanceId.IsEmpty)
							{
								Debug.LogWarning("Attempted to send empty event object instance id to server, and this is not allowed!");
								return;
							}
							this.currentWiringStage = WiringTool.WiringStage.SelectingReceiverObject;
							this.CollectEmitterEventInfos(assetIdFromInstanceId);
							Debug.LogWarning("Firing event for UI");
							this.OnEventObjectSelected.Invoke(propCell.CellBase, this.eventObjectInstanceId, this.GetUiEmitterLists(assetIdFromInstanceId));
							this.hoveredTarget = SerializableGuid.Empty;
							this.SpawnTemporaryWire_ServerRpc(propCell.InstanceId, this.receiverObjectInstanceId, default(ServerRpcParams));
							MonoBehaviourSingleton<PropLocationMarker>.Instance.MarkCellsForProp(this.eventObjectInstanceId, PropLocationType.Emitter);
							this.SetCellMarkerToReceiverColor();
							return;
						}
					}
				}
				else if (base.PerformRaycast(out raycastHit, LayerMask.GetMask(new string[] { "Wires" })))
				{
					PhysicalWire physicalWire;
					if (raycastHit.transform.TryGetComponent<PhysicalWire>(out physicalWire))
					{
						this.EmitterSelectionCancelled();
						this.SelectPhysicalWire(physicalWire);
						return;
					}
					Debug.LogException(new Exception("Mouse click detected a wire, but was unable to retrieve the physical wire instance."), raycastHit.transform.gameObject);
				}
			}
		}

		// Token: 0x060011E5 RID: 4581 RVA: 0x00058C7C File Offset: 0x00056E7C
		private void CalculateTarget()
		{
			bool flag = false;
			float num = float.MaxValue;
			float num2 = float.MaxValue;
			LineCastHit activeLineCastResult = base.ActiveLineCastResult;
			RaycastHit raycastHit;
			if (base.PerformRaycast(out raycastHit, LayerMask.GetMask(new string[] { "Wires" })))
			{
				flag = true;
				num = raycastHit.distance;
			}
			if (activeLineCastResult.IntersectionOccured)
			{
				num2 = activeLineCastResult.Distance;
			}
			if (num < num2 && flag)
			{
				PhysicalWire physicalWire;
				if (raycastHit.transform.TryGetComponent<PhysicalWire>(out physicalWire))
				{
					this.wiringTarget = WiringTool.WiringTarget.PhysicalWire;
					this.wiringTargetPhysicalWire = physicalWire;
					this.wiringTargetPropCell = null;
					return;
				}
				this.wiringTarget = WiringTool.WiringTarget.None;
				this.wiringTargetPhysicalWire = null;
				this.wiringTargetPropCell = null;
				return;
			}
			else
			{
				if (num2 >= num || !activeLineCastResult.IntersectionOccured)
				{
					this.wiringTarget = WiringTool.WiringTarget.None;
					this.wiringTargetPhysicalWire = null;
					this.wiringTargetPropCell = null;
					return;
				}
				PropCell propCell = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetCellFromCoordinate(activeLineCastResult.IntersectedObjectPosition) as PropCell;
				if (propCell != null)
				{
					this.wiringTarget = WiringTool.WiringTarget.Prop;
					this.wiringTargetPropCell = propCell;
					this.wiringTargetPhysicalWire = null;
					return;
				}
				this.wiringTarget = WiringTool.WiringTarget.None;
				this.wiringTargetPhysicalWire = null;
				this.wiringTargetPropCell = null;
				return;
			}
		}

		// Token: 0x060011E6 RID: 4582 RVA: 0x00058D8C File Offset: 0x00056F8C
		private List<UIEndlessEventList> GetUiReceiverLists(SerializableGuid assetId)
		{
			List<UIEndlessEventList> list = new List<UIEndlessEventList>();
			PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(assetId);
			list.Add(new UIEndlessEventList
			{
				DisplayName = "Lua Events",
				EndlessEventInfos = runtimePropInfo.EndlessProp.ScriptComponent.Script.Receivers,
				AssemblyQualifiedTypeName = null
			});
			foreach (ComponentDefinition componentDefinition in runtimePropInfo.GetAllDefinitions())
			{
				Type type = componentDefinition.ComponentBase.GetType();
				list.Add(new UIEndlessEventList
				{
					DisplayName = type.Name,
					EndlessEventInfos = componentDefinition.AvailableReceivers,
					AssemblyQualifiedTypeName = type.AssemblyQualifiedName
				});
			}
			return list;
		}

		// Token: 0x060011E7 RID: 4583 RVA: 0x00058E68 File Offset: 0x00057068
		private List<UIEndlessEventList> GetUiEmitterLists(SerializableGuid assetId)
		{
			List<UIEndlessEventList> list = new List<UIEndlessEventList>();
			PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(assetId);
			list.Add(new UIEndlessEventList
			{
				DisplayName = "Lua Events",
				EndlessEventInfos = runtimePropInfo.EndlessProp.ScriptComponent.Script.Events,
				AssemblyQualifiedTypeName = null
			});
			foreach (ComponentDefinition componentDefinition in runtimePropInfo.GetAllDefinitions())
			{
				Type type = componentDefinition.ComponentBase.GetType();
				list.Add(new UIEndlessEventList
				{
					DisplayName = type.Name,
					EndlessEventInfos = componentDefinition.AvailableEvents,
					AssemblyQualifiedTypeName = type.AssemblyQualifiedName
				});
			}
			return list;
		}

		// Token: 0x060011E8 RID: 4584 RVA: 0x00058F44 File Offset: 0x00057144
		private void SelectPhysicalWire(PhysicalWire wire)
		{
			this.eventObjectInstanceId = wire.EmitterId;
			GameObject gameObjectFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(this.eventObjectInstanceId);
			SerializableGuid assetIdFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetAssetIdFromInstanceId(this.eventObjectInstanceId);
			this.CollectEmitterEventInfos(assetIdFromInstanceId);
			this.receiverObjectInstanceId = wire.ReceiverId;
			GameObject gameObjectFromInstanceId2 = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(this.receiverObjectInstanceId);
			SerializableGuid assetIdFromInstanceId2 = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetAssetIdFromInstanceId(this.receiverObjectInstanceId);
			this.CollectReceiverEventInfos(assetIdFromInstanceId2);
			this.currentWiringStage = WiringTool.WiringStage.AwaitingConfirmation;
			int activeUserId = EndlessServices.Instance.CloudService.ActiveUserId;
			if (this.temporaryWires[activeUserId] && this.temporaryWires[activeUserId].IsLive)
			{
				global::UnityEngine.Object.Destroy(this.temporaryWires[activeUserId].gameObject);
			}
			this.temporaryWires[activeUserId] = this.GetPhysicalWireFromId(wire.BundleId);
			this.SetWireEditedByPlayer_ServerRpc(wire.BundleId, default(ServerRpcParams));
			this.OnEventObjectSelected.Invoke(gameObjectFromInstanceId.transform, this.eventObjectInstanceId, this.GetUiEmitterLists(assetIdFromInstanceId));
			this.OnReceiverObjectSelected.Invoke(gameObjectFromInstanceId2.transform, this.receiverObjectInstanceId, this.GetUiReceiverLists(assetIdFromInstanceId2));
			MonoBehaviourSingleton<PropLocationMarker>.Instance.MarkCellsForProp(this.eventObjectInstanceId, PropLocationType.Emitter);
			MonoBehaviourSingleton<PropLocationMarker>.Instance.MarkCellsForProp(this.receiverObjectInstanceId, PropLocationType.Receiver);
		}

		// Token: 0x060011E9 RID: 4585 RVA: 0x000590B4 File Offset: 0x000572B4
		private PhysicalWire GetPhysicalWireFromId(SerializableGuid wiringId)
		{
			PhysicalWire physicalWire = null;
			List<PhysicalWire> physicalWires = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.PhysicalWires;
			for (int i = 0; i < physicalWires.Count; i++)
			{
				if (physicalWires[i].BundleId == wiringId)
				{
					physicalWire = physicalWires[i];
					break;
				}
			}
			return physicalWire;
		}

		// Token: 0x060011EA RID: 4586 RVA: 0x00059104 File Offset: 0x00057304
		[ServerRpc(RequireOwnership = false)]
		private void SetTemporaryWireTargetToPlayer_ServerRpc(ServerRpcParams serverParams = default(ServerRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(354023136U, serverParams, RpcDelivery.Reliable);
				base.__endSendServerRpc(ref fastBufferWriter, 354023136U, serverParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			NetworkObject networkObject2 = NetworkManager.Singleton.ConnectedClients[serverParams.Receive.SenderClientId].OwnedObjects.FirstOrDefault((NetworkObject networkObject) => networkObject.GetComponent<PlayerController>() != null);
			ulong? num = ((networkObject2 != null) ? new ulong?(networkObject2.NetworkObjectId) : null);
			if (num != null)
			{
				int userId = NetworkBehaviourSingleton<UserIdManager>.Instance.GetUserId(serverParams.Receive.SenderClientId);
				this.SetTemporaryWireTargetToPlayer_ClientRpc(userId, num.Value);
				if (!base.IsClient)
				{
					this.SetTemporaryWireTargetToPlayer(userId, num.Value);
				}
			}
		}

		// Token: 0x060011EB RID: 4587 RVA: 0x00059278 File Offset: 0x00057478
		[ClientRpc]
		private void SetTemporaryWireTargetToPlayer_ClientRpc(int userId, ulong networkObjectId)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(1104155854U, clientRpcParams, RpcDelivery.Reliable);
				BytePacker.WriteValueBitPacked(fastBufferWriter, userId);
				BytePacker.WriteValueBitPacked(fastBufferWriter, networkObjectId);
				base.__endSendClientRpc(ref fastBufferWriter, 1104155854U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			if (NetworkBehaviourSingleton<CreatorManager>.Instance.RpcReceiveState == RpcReceiveState.Allow)
			{
				this.SetTemporaryWireTargetToPlayer(userId, networkObjectId);
			}
		}

		// Token: 0x060011EC RID: 4588 RVA: 0x0005937C File Offset: 0x0005757C
		private void SetTemporaryWireTargetToPlayer(int userId, ulong networkObjectId)
		{
			NetworkObject networkObject = base.NetworkManager.SpawnManager.SpawnedObjects[networkObjectId];
			Transform transform = ((networkObject != null) ? networkObject.transform : null);
			if (transform == null)
			{
				return;
			}
			transform = transform.GetComponent<PlayerReferenceManager>().ApperanceController.transform;
			if (this.temporaryWires.ContainsKey(userId) && this.temporaryWires[userId])
			{
				this.temporaryWires[userId].SetReceiverToPlayer(transform);
			}
		}

		// Token: 0x060011ED RID: 4589 RVA: 0x000593FC File Offset: 0x000575FC
		[ServerRpc(RequireOwnership = false)]
		private void SetTemporaryWireTarget_ServerRpc(SerializableGuid target, ServerRpcParams serverParams = default(ServerRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(3716040516U, serverParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<SerializableGuid>(in target, default(FastBufferWriter.ForNetworkSerializable));
				base.__endSendServerRpc(ref fastBufferWriter, 3716040516U, serverParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			int userId = NetworkBehaviourSingleton<UserIdManager>.Instance.GetUserId(serverParams.Receive.SenderClientId);
			this.SetTemporaryWireTarget_ClientRpc(target, userId);
			if (!base.IsClient)
			{
				this.SetTemporaryWireTarget(target, userId);
			}
		}

		// Token: 0x060011EE RID: 4590 RVA: 0x00059518 File Offset: 0x00057718
		[ClientRpc]
		private void SetTemporaryWireTarget_ClientRpc(SerializableGuid target, int userId)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(4269480805U, clientRpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<SerializableGuid>(in target, default(FastBufferWriter.ForNetworkSerializable));
				BytePacker.WriteValueBitPacked(fastBufferWriter, userId);
				base.__endSendClientRpc(ref fastBufferWriter, 4269480805U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			if (NetworkBehaviourSingleton<CreatorManager>.Instance.RpcReceiveState == RpcReceiveState.Allow)
			{
				this.SetTemporaryWireTarget(target, userId);
			}
		}

		// Token: 0x060011EF RID: 4591 RVA: 0x00059628 File Offset: 0x00057828
		private void SetTemporaryWireTarget(SerializableGuid target, int userId)
		{
			GameObject gameObjectFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(target);
			if (this.temporaryWires.ContainsKey(userId))
			{
				if (!this.temporaryWires[userId])
				{
					Debug.LogWarning(string.Format("Temporary wire for Player Id: {0} is null, but shouldn't be because we're in {1}", userId, "SetTemporaryWireTarget"));
				}
				if (!gameObjectFromInstanceId)
				{
					Debug.LogWarning(string.Format("Target Object for wire target for Player Id: {0} is null, but shouldn't be because we're in {1}", userId, "SetTemporaryWireTarget"));
				}
				if (!this.temporaryWires[userId])
				{
					Debug.LogWarning(string.Format("Temporary wire for Player Id: {0} is null, this is possible if they joined late and never got the temp wire spawn.", userId));
					return;
				}
				this.temporaryWires[userId].SetTemporaryTarget(gameObjectFromInstanceId.transform, target, this.temporaryWires[userId].ReceiverTargetPosition(), delegate
				{
					foreach (WireBundle wireBundle in MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.WireBundles)
					{
						if (wireBundle.EmitterInstanceId == this.temporaryWires[userId].EmitterId && wireBundle.ReceiverInstanceId == this.temporaryWires[userId].ReceiverId)
						{
							this.ClearTemporaryWireForPlayer_ServerRpc(default(ServerRpcParams));
							this.SetWireEditedByPlayer_ServerRpc(wireBundle.BundleId, default(ServerRpcParams));
							break;
						}
					}
				}, MonoBehaviourSingleton<ToolManager>.Instance.ActiveTool is WiringTool);
			}
		}

		// Token: 0x060011F0 RID: 4592 RVA: 0x00059754 File Offset: 0x00057954
		[ServerRpc(RequireOwnership = false)]
		internal void SetWireEditedByPlayer_ServerRpc(SerializableGuid wireId, ServerRpcParams serverParams = default(ServerRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(3772252676U, serverParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<SerializableGuid>(in wireId, default(FastBufferWriter.ForNetworkSerializable));
				base.__endSendServerRpc(ref fastBufferWriter, 3772252676U, serverParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			int userId = NetworkBehaviourSingleton<UserIdManager>.Instance.GetUserId(serverParams.Receive.SenderClientId);
			this.SetWireEditedByPlayer_ClientRpc(wireId, userId, default(ClientRpcParams));
			if (!base.IsClient)
			{
				this.SetWireEditedByPlayer(wireId, userId);
			}
		}

		// Token: 0x060011F1 RID: 4593 RVA: 0x0005987C File Offset: 0x00057A7C
		[ClientRpc]
		private void SetWireEditedByPlayer_ClientRpc(SerializableGuid wireId, int userId, ClientRpcParams rpcParams = default(ClientRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			SerializableGuid wireId;
			int userId;
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(1404899374U, rpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<SerializableGuid>(in wireId, default(FastBufferWriter.ForNetworkSerializable));
				BytePacker.WriteValueBitPacked(fastBufferWriter, userId);
				base.__endSendClientRpc(ref fastBufferWriter, 1404899374U, rpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			wireId = wireId;
			userId = userId;
			if (NetworkBehaviourSingleton<CreatorManager>.Instance.RpcReceiveState == RpcReceiveState.Allow)
			{
				this.SetWireEditedByPlayer(wireId, userId);
				return;
			}
			NetworkBehaviourSingleton<CreatorManager>.Instance.AddCachedRpc(delegate
			{
				this.SetWireEditedByPlayer(wireId, userId);
			});
		}

		// Token: 0x060011F2 RID: 4594 RVA: 0x000599C8 File Offset: 0x00057BC8
		private void SetWireEditedByPlayer(SerializableGuid wireId, int userId)
		{
			PhysicalWire physicalWire = null;
			List<PhysicalWire> physicalWires = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.PhysicalWires;
			for (int i = 0; i < physicalWires.Count; i++)
			{
				if (physicalWires[i].BundleId == wireId)
				{
					physicalWire = physicalWires[i];
					break;
				}
			}
			if (!physicalWire)
			{
				return;
			}
			physicalWire.SetEditing();
			if (!this.temporaryWires.ContainsKey(userId))
			{
				this.temporaryWires.Add(userId, null);
			}
			if (this.temporaryWires[userId] && this.temporaryWires[userId].IsLive)
			{
				global::UnityEngine.Object.Destroy(this.temporaryWires[userId].gameObject);
			}
			this.temporaryWires[userId] = physicalWire;
			int activeUserId = EndlessServices.Instance.CloudService.ActiveUserId;
			if (userId == activeUserId && physicalWire.RerouteNodeIds.Count > 0)
			{
				this.EnableRerouteUndo.Invoke();
			}
		}

		// Token: 0x060011F3 RID: 4595 RVA: 0x00059AB8 File Offset: 0x00057CB8
		[ServerRpc(RequireOwnership = false)]
		private void SpawnTemporaryWire_ServerRpc(SerializableGuid source, SerializableGuid target, ServerRpcParams serverParams = default(ServerRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(90801435U, serverParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<SerializableGuid>(in source, default(FastBufferWriter.ForNetworkSerializable));
				fastBufferWriter.WriteValueSafe<SerializableGuid>(in target, default(FastBufferWriter.ForNetworkSerializable));
				base.__endSendServerRpc(ref fastBufferWriter, 90801435U, serverParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			NetworkObject networkObject2 = NetworkManager.Singleton.ConnectedClients[serverParams.Receive.SenderClientId].OwnedObjects.FirstOrDefault((NetworkObject networkObject) => networkObject.GetComponent<PlayerController>() != null);
			ulong? num = ((networkObject2 != null) ? new ulong?(networkObject2.NetworkObjectId) : null);
			if (num != null)
			{
				int userId = NetworkBehaviourSingleton<UserIdManager>.Instance.GetUserId(serverParams.Receive.SenderClientId);
				this.SpawnTemporaryWire_ClientRpc(source, userId, num.Value, target, default(ClientRpcParams));
				if (!base.IsClient)
				{
					this.SpawnTemporaryWire(source, userId, num.Value, target);
					return;
				}
			}
			else
			{
				Debug.LogException(new Exception("Attempted to get the network object id for player controller but it was unavailable."));
			}
		}

		// Token: 0x060011F4 RID: 4596 RVA: 0x00059C80 File Offset: 0x00057E80
		[ClientRpc]
		private void SpawnTemporaryWire_ClientRpc(SerializableGuid source, int targetUserId, ulong networkObjectId, SerializableGuid target, ClientRpcParams clientRpcParams = default(ClientRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			SerializableGuid source;
			int targetUserId;
			ulong networkObjectId;
			SerializableGuid target;
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(3985273964U, clientRpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<SerializableGuid>(in source, default(FastBufferWriter.ForNetworkSerializable));
				BytePacker.WriteValueBitPacked(fastBufferWriter, targetUserId);
				BytePacker.WriteValueBitPacked(fastBufferWriter, networkObjectId);
				fastBufferWriter.WriteValueSafe<SerializableGuid>(in target, default(FastBufferWriter.ForNetworkSerializable));
				base.__endSendClientRpc(ref fastBufferWriter, 3985273964U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			source = source;
			targetUserId = targetUserId;
			networkObjectId = networkObjectId;
			target = target;
			if (NetworkBehaviourSingleton<CreatorManager>.Instance.RpcReceiveState == RpcReceiveState.Allow)
			{
				this.SpawnTemporaryWire(source, targetUserId, networkObjectId, target);
				return;
			}
			NetworkBehaviourSingleton<CreatorManager>.Instance.AddCachedRpc(delegate
			{
				this.SpawnTemporaryWire(source, targetUserId, networkObjectId, target);
			});
		}

		// Token: 0x060011F5 RID: 4597 RVA: 0x00059E10 File Offset: 0x00058010
		private void SpawnTemporaryWire(SerializableGuid source, int targetUserId, ulong networkObjectId, SerializableGuid target)
		{
			if (!this.temporaryWires.ContainsKey(targetUserId))
			{
				this.temporaryWires.Add(targetUserId, null);
			}
			if (this.temporaryWires[targetUserId])
			{
				this.ClearTemporaryWireForPlayer(targetUserId);
			}
			Transform connectionPointFromObject = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetConnectionPointFromObject(MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(source), source, ConnectionPoint.Emitter);
			Transform transform;
			if (target.IsEmpty)
			{
				NetworkObject networkObject = base.NetworkManager.SpawnManager.SpawnedObjects[networkObjectId];
				transform = ((networkObject != null) ? networkObject.transform : null);
			}
			else
			{
				transform = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(target).transform;
			}
			if (transform == null)
			{
				return;
			}
			NetworkObject networkObject2 = base.NetworkManager.SpawnManager.SpawnedObjects[networkObjectId];
			Transform transform2 = ((networkObject2 != null) ? networkObject2.transform : null);
			PhysicalWire physicalWire = global::UnityEngine.Object.Instantiate<PhysicalWire>(this.physicalWireTemplate);
			physicalWire.SetupLiveMode(source, connectionPointFromObject, transform2.position, transform, MonoBehaviourSingleton<ToolManager>.Instance.ActiveTool is WiringTool);
			physicalWire.gameObject.SetActive(MonoBehaviourSingleton<ToolManager>.Instance.ActiveTool is WiringTool);
			this.temporaryWires[targetUserId] = physicalWire;
		}

		// Token: 0x060011F6 RID: 4598 RVA: 0x00059F38 File Offset: 0x00058138
		[ServerRpc(RequireOwnership = false)]
		private void AddRerouteNodeToPosition_ServerRpc(Vector3Int coordinate, ServerRpcParams serverParams = default(ServerRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(359986616U, serverParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe(in coordinate);
				base.__endSendServerRpc(ref fastBufferWriter, 359986616U, serverParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060011F7 RID: 4599 RVA: 0x0005A018 File Offset: 0x00058218
		private async void AddRerouteNodeToPosition(Vector3Int coordinate, ServerRpcParams serverParams)
		{
		}

		// Token: 0x060011F8 RID: 4600 RVA: 0x0005A048 File Offset: 0x00058248
		[ClientRpc]
		private void PlaceRerouteNode_ClientRPC(Vector3Int coordinate, SerializableGuid assetId, SerializableGuid instanceId, ulong networkObjectId, ulong targetPlayerId)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(101785308U, clientRpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe(in coordinate);
				fastBufferWriter.WriteValueSafe<SerializableGuid>(in assetId, default(FastBufferWriter.ForNetworkSerializable));
				fastBufferWriter.WriteValueSafe<SerializableGuid>(in instanceId, default(FastBufferWriter.ForNetworkSerializable));
				BytePacker.WriteValueBitPacked(fastBufferWriter, networkObjectId);
				BytePacker.WriteValueBitPacked(fastBufferWriter, targetPlayerId);
				base.__endSendClientRpc(ref fastBufferWriter, 101785308U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060011F9 RID: 4601 RVA: 0x000056F3 File Offset: 0x000038F3
		private void PlaceRerouteNode(Vector3Int coordinate, SerializableGuid assetId, SerializableGuid instanceId, ulong networkObjectId, ulong targetPlayerId)
		{
		}

		// Token: 0x060011FA RID: 4602 RVA: 0x0005A178 File Offset: 0x00058378
		private void CollectReceiverEventInfos(SerializableGuid assetId)
		{
			this.receiverEventInfos.Clear();
			PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(assetId);
			Script script = runtimePropInfo.EndlessProp.ScriptComponent.Script;
			List<EndlessEventInfo> list = ((script != null) ? script.Receivers : null) ?? new List<EndlessEventInfo>();
			IEnumerable<EndlessEventInfo> enumerable = runtimePropInfo.GetAllDefinitions().SelectMany((ComponentDefinition definition) => definition.AvailableReceivers);
			this.emitterEventInfos = list.Concat(enumerable).ToList<EndlessEventInfo>();
		}

		// Token: 0x060011FB RID: 4603 RVA: 0x0005A204 File Offset: 0x00058404
		private void CollectEmitterEventInfos(SerializableGuid assetId)
		{
			this.emitterEventInfos.Clear();
			PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(assetId);
			Script script = runtimePropInfo.EndlessProp.ScriptComponent.Script;
			List<EndlessEventInfo> list = ((script != null) ? script.Events : null) ?? new List<EndlessEventInfo>();
			IEnumerable<EndlessEventInfo> enumerable = runtimePropInfo.GetAllDefinitions().SelectMany((ComponentDefinition definition) => definition.AvailableEvents);
			this.emitterEventInfos = list.Concat(enumerable).ToList<EndlessEventInfo>();
		}

		// Token: 0x060011FC RID: 4604 RVA: 0x0005A290 File Offset: 0x00058490
		public void ReceiverSelectionCancelled(bool closing = false)
		{
			this.SetCellMarkerToReceiverColor();
			MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(this.receiverObjectInstanceId, PropLocationType.Receiver);
			this.receiverObjectInstanceId = SerializableGuid.Empty;
			this.currentWiringStage = WiringTool.WiringStage.SelectingReceiverObject;
			int num;
			if (!NetworkBehaviourSingleton<UserIdManager>.Instance.TryGetUserId(NetworkManager.Singleton.LocalClientId, out num))
			{
				return;
			}
			if (!this.temporaryWires.ContainsKey(num))
			{
				return;
			}
			if (this.temporaryWires[num] && this.temporaryWires[num].IsLive)
			{
				this.SetTemporaryWireTargetToPlayer_ServerRpc(default(ServerRpcParams));
				return;
			}
			if ((!this.temporaryWires[num] || !this.temporaryWires[num].IsLive) && !closing)
			{
				if (this.eventObjectInstanceId.IsEmpty)
				{
					Debug.LogWarning("Attempted to call server RPC with empty emitter instance id. This shouldn't be possible (but has occurred in cases where the game is in an errored state)");
					return;
				}
				this.SpawnTemporaryWire_ServerRpc(this.eventObjectInstanceId, SerializableGuid.Empty, default(ServerRpcParams));
			}
		}

		// Token: 0x060011FD RID: 4605 RVA: 0x0005A380 File Offset: 0x00058580
		public void EmitterSelectionCancelled()
		{
			this.currentWiringStage = WiringTool.WiringStage.SelectingEventObject;
			this.SetCellMarkerToEmitterColor();
			MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(this.eventObjectInstanceId, PropLocationType.Emitter);
			if (this.eventObjectInstanceId == SerializableGuid.Empty)
			{
				return;
			}
			this.eventObjectInstanceId = SerializableGuid.Empty;
			this.receiverObjectInstanceId = SerializableGuid.Empty;
			this.ClearTemporaryWireForPlayer_ServerRpc(default(ServerRpcParams));
			this.pendingRerouteNodes.Clear();
		}

		// Token: 0x060011FE RID: 4606 RVA: 0x0005A3F0 File Offset: 0x000585F0
		[ServerRpc(RequireOwnership = false)]
		private void ClearTemporaryWireForPlayer_ServerRpc(ServerRpcParams serverParams = default(ServerRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(3636364046U, serverParams, RpcDelivery.Reliable);
				base.__endSendServerRpc(ref fastBufferWriter, 3636364046U, serverParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			int userId = NetworkBehaviourSingleton<UserIdManager>.Instance.GetUserId(serverParams.Receive.SenderClientId);
			this.ClearTemporaryWireForPlayer_ClientRpc(userId);
			if (!base.IsClient)
			{
				this.ClearTemporaryWireForPlayer(userId);
			}
		}

		// Token: 0x060011FF RID: 4607 RVA: 0x0005A4F0 File Offset: 0x000586F0
		[ClientRpc]
		private void ClearTemporaryWireForPlayer_ClientRpc(int userId)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			int userId;
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(3434663878U, clientRpcParams, RpcDelivery.Reliable);
				BytePacker.WriteValueBitPacked(fastBufferWriter, userId);
				base.__endSendClientRpc(ref fastBufferWriter, 3434663878U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			userId = userId;
			if (NetworkBehaviourSingleton<CreatorManager>.Instance.RpcReceiveState == RpcReceiveState.Allow)
			{
				this.ClearTemporaryWireForPlayer(userId);
				return;
			}
			NetworkBehaviourSingleton<CreatorManager>.Instance.AddCachedRpc(delegate
			{
				this.ClearTemporaryWireForPlayer(userId);
			});
		}

		// Token: 0x06001200 RID: 4608 RVA: 0x0005A614 File Offset: 0x00058814
		private void ClearTemporaryWireForPlayer(int userId)
		{
			if (!this.temporaryWires.ContainsKey(userId))
			{
				return;
			}
			if (!this.temporaryWires[userId])
			{
				return;
			}
			if (!this.temporaryWires[userId].IsLive)
			{
				this.temporaryWires[userId].SetPlaced();
				this.temporaryWires[userId] = null;
				return;
			}
			global::UnityEngine.Object.Destroy(this.temporaryWires[userId].gameObject);
			this.temporaryWires[userId] = null;
		}

		// Token: 0x06001201 RID: 4609 RVA: 0x0005A69C File Offset: 0x0005889C
		public void EventConfirmed(EndlessEventInfo emitterEventInfo, string emitterTypeName, EndlessEventInfo receiverInfo, string receiverTypeName, string[] storedParameterValues, WireColor wireColor)
		{
			for (int i = 0; i < storedParameterValues.Length; i++)
			{
				Debug.Log(storedParameterValues[i]);
			}
			this.AttemptWireTogether_ServerRpc(this.eventObjectInstanceId, emitterEventInfo.MemberName, emitterTypeName, this.receiverObjectInstanceId, receiverInfo.MemberName, receiverTypeName, storedParameterValues, this.pendingRerouteNodes.ToArray(), wireColor, default(ServerRpcParams));
		}

		// Token: 0x06001202 RID: 4610 RVA: 0x0005A700 File Offset: 0x00058900
		public void UpdateWire(SerializableGuid wireId, SerializableGuid emitterInstanceId, string emitterMemberName, string emitterTypeName, SerializableGuid receiverInstanceId, string receiverMemberName, string receiverTypeName, StringArraySerialized storedParameterValues, SerializableGuid[] pendingRerouteNodes, WireColor wireColor)
		{
			SerializableGuid serializableGuid = SerializableGuid.Empty;
			int num = 0;
			while (serializableGuid.IsEmpty && num < MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.WireBundles.Count)
			{
				using (List<WireEntry>.Enumerator enumerator = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.WireBundles[num].Wires.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current.WireId == wireId)
						{
							serializableGuid = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.WireBundles[num].BundleId;
							break;
						}
					}
				}
				num++;
			}
			if (serializableGuid.IsEmpty)
			{
				return;
			}
			this.UpdateWireDetails_ServerRpc(serializableGuid, wireId, emitterInstanceId, emitterMemberName, emitterTypeName, receiverInstanceId, receiverMemberName, receiverTypeName, storedParameterValues, pendingRerouteNodes, wireColor, default(ServerRpcParams));
		}

		// Token: 0x06001203 RID: 4611 RVA: 0x0005A7F4 File Offset: 0x000589F4
		[ServerRpc(RequireOwnership = false)]
		private void UpdateWireDetails_ServerRpc(SerializableGuid bundleId, SerializableGuid wireId, SerializableGuid emitterInstanceId, string emitterMemberName, string emitterTypeName, SerializableGuid receiverInstanceId, string receiverMemberName, string receiverTypeName, StringArraySerialized storedParameterValues, SerializableGuid[] pendingRerouteNodes, WireColor wireColor, ServerRpcParams serverRpcParams = default(ServerRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(328729018U, serverRpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<SerializableGuid>(in bundleId, default(FastBufferWriter.ForNetworkSerializable));
				fastBufferWriter.WriteValueSafe<SerializableGuid>(in wireId, default(FastBufferWriter.ForNetworkSerializable));
				fastBufferWriter.WriteValueSafe<SerializableGuid>(in emitterInstanceId, default(FastBufferWriter.ForNetworkSerializable));
				bool flag = emitterMemberName != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag, default(FastBufferWriter.ForPrimitives));
				if (flag)
				{
					fastBufferWriter.WriteValueSafe(emitterMemberName, false);
				}
				bool flag2 = emitterTypeName != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag2, default(FastBufferWriter.ForPrimitives));
				if (flag2)
				{
					fastBufferWriter.WriteValueSafe(emitterTypeName, false);
				}
				fastBufferWriter.WriteValueSafe<SerializableGuid>(in receiverInstanceId, default(FastBufferWriter.ForNetworkSerializable));
				bool flag3 = receiverMemberName != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag3, default(FastBufferWriter.ForPrimitives));
				if (flag3)
				{
					fastBufferWriter.WriteValueSafe(receiverMemberName, false);
				}
				bool flag4 = receiverTypeName != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag4, default(FastBufferWriter.ForPrimitives));
				if (flag4)
				{
					fastBufferWriter.WriteValueSafe(receiverTypeName, false);
				}
				fastBufferWriter.WriteValueSafe<StringArraySerialized>(in storedParameterValues, default(FastBufferWriter.ForNetworkSerializable));
				bool flag5 = pendingRerouteNodes != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag5, default(FastBufferWriter.ForPrimitives));
				if (flag5)
				{
					fastBufferWriter.WriteValueSafe<SerializableGuid>(pendingRerouteNodes, default(FastBufferWriter.ForNetworkSerializable));
				}
				fastBufferWriter.WriteValueSafe<WireColor>(in wireColor, default(FastBufferWriter.ForEnums));
				base.__endSendServerRpc(ref fastBufferWriter, 328729018U, serverRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.UpdateWireDetails(bundleId, wireId, emitterInstanceId, emitterMemberName, emitterTypeName, receiverInstanceId, receiverMemberName, receiverTypeName, storedParameterValues, pendingRerouteNodes, wireColor, serverRpcParams);
		}

		// Token: 0x06001204 RID: 4612 RVA: 0x0005AAC8 File Offset: 0x00058CC8
		private async void UpdateWireDetails(SerializableGuid bundleId, SerializableGuid wireId, SerializableGuid emitterInstanceId, string emitterMemberName, string emitterTypeName, SerializableGuid receiverInstanceId, string receiverMemberName, string receiverTypeName, StringArraySerialized storedParameterValues, SerializableGuid[] pendingRerouteNodes, WireColor wireColor, ServerRpcParams serverRpcParams)
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
					this.UpdateWireDetails_ClientRpc(bundleId, wireId, emitterInstanceId, emitterMemberName, emitterTypeName, receiverInstanceId, receiverMemberName, receiverTypeName, storedParameterValues, pendingRerouteNodes, wireColor);
					if (!base.IsClient)
					{
						this.UpdateWireDetails(bundleId, wireId, emitterInstanceId, emitterMemberName, emitterTypeName, receiverInstanceId, receiverMemberName, receiverTypeName, storedParameterValues, pendingRerouteNodes, wireColor);
					}
					MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.RevisionTimestamp = DateTime.UtcNow.Ticks;
					MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.Changes.Add(new ChangeData
					{
						ChangeType = ChangeType.WireUpdated,
						UserId = userId
					});
					NetworkBehaviourSingleton<CreatorManager>.Instance.SaveLoadManager.SaveLevel();
				}
			}
		}

		// Token: 0x06001205 RID: 4613 RVA: 0x0005AB68 File Offset: 0x00058D68
		[ClientRpc]
		private void UpdateWireDetails_ClientRpc(SerializableGuid bundleId, SerializableGuid wireId, SerializableGuid emitterInstanceId, string emitterMemberName, string emitterTypeName, SerializableGuid receiverInstanceId, string receiverMemberName, string receiverTypeName, StringArraySerialized storedParameterValues, SerializableGuid[] pendingRerouteNodes, WireColor wireColor)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			SerializableGuid bundleId;
			SerializableGuid wireId;
			SerializableGuid emitterInstanceId;
			string emitterMemberName;
			string emitterTypeName;
			SerializableGuid receiverInstanceId;
			string receiverMemberName;
			string receiverTypeName;
			StringArraySerialized storedParameterValues;
			SerializableGuid[] pendingRerouteNodes;
			WireColor wireColor;
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(1095343870U, clientRpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<SerializableGuid>(in bundleId, default(FastBufferWriter.ForNetworkSerializable));
				fastBufferWriter.WriteValueSafe<SerializableGuid>(in wireId, default(FastBufferWriter.ForNetworkSerializable));
				fastBufferWriter.WriteValueSafe<SerializableGuid>(in emitterInstanceId, default(FastBufferWriter.ForNetworkSerializable));
				bool flag = emitterMemberName != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag, default(FastBufferWriter.ForPrimitives));
				if (flag)
				{
					fastBufferWriter.WriteValueSafe(emitterMemberName, false);
				}
				bool flag2 = emitterTypeName != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag2, default(FastBufferWriter.ForPrimitives));
				if (flag2)
				{
					fastBufferWriter.WriteValueSafe(emitterTypeName, false);
				}
				fastBufferWriter.WriteValueSafe<SerializableGuid>(in receiverInstanceId, default(FastBufferWriter.ForNetworkSerializable));
				bool flag3 = receiverMemberName != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag3, default(FastBufferWriter.ForPrimitives));
				if (flag3)
				{
					fastBufferWriter.WriteValueSafe(receiverMemberName, false);
				}
				bool flag4 = receiverTypeName != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag4, default(FastBufferWriter.ForPrimitives));
				if (flag4)
				{
					fastBufferWriter.WriteValueSafe(receiverTypeName, false);
				}
				fastBufferWriter.WriteValueSafe<StringArraySerialized>(in storedParameterValues, default(FastBufferWriter.ForNetworkSerializable));
				bool flag5 = pendingRerouteNodes != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag5, default(FastBufferWriter.ForPrimitives));
				if (flag5)
				{
					fastBufferWriter.WriteValueSafe<SerializableGuid>(pendingRerouteNodes, default(FastBufferWriter.ForNetworkSerializable));
				}
				fastBufferWriter.WriteValueSafe<WireColor>(in wireColor, default(FastBufferWriter.ForEnums));
				base.__endSendClientRpc(ref fastBufferWriter, 1095343870U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			bundleId = bundleId;
			wireId = wireId;
			emitterInstanceId = emitterInstanceId;
			emitterMemberName = emitterMemberName;
			emitterTypeName = emitterTypeName;
			receiverInstanceId = receiverInstanceId;
			receiverMemberName = receiverMemberName;
			receiverTypeName = receiverTypeName;
			storedParameterValues = storedParameterValues;
			pendingRerouteNodes = pendingRerouteNodes;
			wireColor = wireColor;
			if (NetworkBehaviourSingleton<CreatorManager>.Instance.RpcReceiveState == RpcReceiveState.Allow)
			{
				this.UpdateWireDetails(bundleId, wireId, emitterInstanceId, emitterMemberName, emitterTypeName, receiverInstanceId, receiverMemberName, receiverTypeName, storedParameterValues, pendingRerouteNodes, wireColor);
				return;
			}
			NetworkBehaviourSingleton<CreatorManager>.Instance.AddCachedRpc(delegate
			{
				this.UpdateWireDetails(bundleId, wireId, emitterInstanceId, emitterMemberName, emitterTypeName, receiverInstanceId, receiverMemberName, receiverTypeName, storedParameterValues, pendingRerouteNodes, wireColor);
			});
		}

		// Token: 0x06001206 RID: 4614 RVA: 0x0005AEF4 File Offset: 0x000590F4
		private void UpdateWireDetails(SerializableGuid bundleId, SerializableGuid wireId, SerializableGuid emitterInstanceId, string emitterMemberName, string emitterTypeName, SerializableGuid receiverInstanceId, string receiverMemberName, string receiverTypeName, string[] storedParameterValues, SerializableGuid[] pendingRerouteNodes, WireColor wireColor)
		{
			PhysicalWire physicalWireFromId = this.GetPhysicalWireFromId(bundleId);
			this.ApplyWiring(bundleId, wireId, emitterInstanceId, emitterMemberName, emitterTypeName, receiverInstanceId, receiverMemberName, receiverTypeName, storedParameterValues, physicalWireFromId.RerouteNodeIds, wireColor);
		}

		// Token: 0x06001207 RID: 4615 RVA: 0x0005AF28 File Offset: 0x00059128
		private EndlessEventInfo GetEventInfo(SerializableGuid instanceId, string typeName, string memberName, bool emitter)
		{
			EndlessEventInfo endlessEventInfo = null;
			SerializableGuid assetIdFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetAssetIdFromInstanceId(instanceId);
			PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(assetIdFromInstanceId);
			if (typeName == null)
			{
				if (runtimePropInfo.EndlessProp.ScriptComponent.Script == null)
				{
					return null;
				}
				if (emitter)
				{
					endlessEventInfo = runtimePropInfo.EndlessProp.ScriptComponent.Script.Events.FirstOrDefault((EndlessEventInfo eventInfo) => eventInfo.MemberName == memberName);
				}
				else
				{
					endlessEventInfo = runtimePropInfo.EndlessProp.ScriptComponent.Script.Receivers.FirstOrDefault((EndlessEventInfo eventInfo) => eventInfo.MemberName == memberName);
				}
			}
			else
			{
				ComponentDefinition componentDefinition = runtimePropInfo.GetAllDefinitions().FirstOrDefault((ComponentDefinition defintion) => defintion.ComponentBase.GetType().AssemblyQualifiedName == typeName);
				if (componentDefinition)
				{
					if (emitter)
					{
						endlessEventInfo = componentDefinition.AvailableEvents.FirstOrDefault((EndlessEventInfo eventInfo) => eventInfo.MemberName == memberName);
					}
					else
					{
						endlessEventInfo = componentDefinition.AvailableReceivers.FirstOrDefault((EndlessEventInfo eventInfo) => eventInfo.MemberName == memberName);
					}
				}
			}
			if (endlessEventInfo == null)
			{
				Debug.LogError(string.Format("Failed to find {0} {1} ({2})", emitter, memberName, typeName));
			}
			return endlessEventInfo;
		}

		// Token: 0x06001208 RID: 4616 RVA: 0x0005B068 File Offset: 0x00059268
		[ServerRpc(RequireOwnership = false)]
		private void AttemptWireTogether_ServerRpc(SerializableGuid emitterInstanceId, string emitterMemberName, string emitterTypeName, SerializableGuid receiverInstanceId, string receiverMemberName, string receiverTypeName, StringArraySerialized storedParameterValues, SerializableGuid[] pendingRerouteNodes, WireColor wireColor, ServerRpcParams rpcParams = default(ServerRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(1366945158U, rpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<SerializableGuid>(in emitterInstanceId, default(FastBufferWriter.ForNetworkSerializable));
				bool flag = emitterMemberName != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag, default(FastBufferWriter.ForPrimitives));
				if (flag)
				{
					fastBufferWriter.WriteValueSafe(emitterMemberName, false);
				}
				bool flag2 = emitterTypeName != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag2, default(FastBufferWriter.ForPrimitives));
				if (flag2)
				{
					fastBufferWriter.WriteValueSafe(emitterTypeName, false);
				}
				fastBufferWriter.WriteValueSafe<SerializableGuid>(in receiverInstanceId, default(FastBufferWriter.ForNetworkSerializable));
				bool flag3 = receiverMemberName != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag3, default(FastBufferWriter.ForPrimitives));
				if (flag3)
				{
					fastBufferWriter.WriteValueSafe(receiverMemberName, false);
				}
				bool flag4 = receiverTypeName != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag4, default(FastBufferWriter.ForPrimitives));
				if (flag4)
				{
					fastBufferWriter.WriteValueSafe(receiverTypeName, false);
				}
				fastBufferWriter.WriteValueSafe<StringArraySerialized>(in storedParameterValues, default(FastBufferWriter.ForNetworkSerializable));
				bool flag5 = pendingRerouteNodes != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag5, default(FastBufferWriter.ForPrimitives));
				if (flag5)
				{
					fastBufferWriter.WriteValueSafe<SerializableGuid>(pendingRerouteNodes, default(FastBufferWriter.ForNetworkSerializable));
				}
				fastBufferWriter.WriteValueSafe<WireColor>(in wireColor, default(FastBufferWriter.ForEnums));
				base.__endSendServerRpc(ref fastBufferWriter, 1366945158U, rpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.AttemptWireTogether(emitterInstanceId, emitterMemberName, emitterTypeName, receiverInstanceId, receiverMemberName, receiverTypeName, storedParameterValues, pendingRerouteNodes, wireColor, rpcParams);
		}

		// Token: 0x06001209 RID: 4617 RVA: 0x0005B304 File Offset: 0x00059504
		private async void AttemptWireTogether(SerializableGuid emitterInstanceId, string emitterMemberName, string emitterTypeName, SerializableGuid receiverInstanceId, string receiverMemberName, string receiverTypeName, StringArraySerialized storedParameterValues, SerializableGuid[] pendingRerouteNodes, WireColor wireColor, ServerRpcParams rpcParams)
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
					EndlessEventInfo eventInfo = this.GetEventInfo(emitterInstanceId, emitterTypeName, emitterMemberName, true);
					EndlessEventInfo eventInfo2 = this.GetEventInfo(receiverInstanceId, receiverTypeName, receiverMemberName, false);
					if (eventInfo != null && eventInfo2 != null)
					{
						int count = eventInfo2.ParamList.Count;
						string[] value = storedParameterValues.value;
						int? num = ((value != null) ? new int?(value.Length) : null);
						if (!((count == num.GetValueOrDefault()) & (num != null)))
						{
							string[] value2 = storedParameterValues.value;
							if (value2 == null || value2.Length != 0)
							{
								goto IL_016F;
							}
						}
						SerializableGuid serializableGuid = SerializableGuid.Empty;
						WireBundle bundleUsingInstances = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.GetBundleUsingInstances(emitterInstanceId, receiverInstanceId);
						SerializableGuid serializableGuid2 = ((bundleUsingInstances != null) ? bundleUsingInstances.BundleId : SerializableGuid.Empty);
						if (serializableGuid2.IsEmpty)
						{
							if (bundleUsingInstances != null)
							{
								serializableGuid2 = bundleUsingInstances.BundleId;
							}
							else
							{
								serializableGuid2 = SerializableGuid.NewGuid();
								serializableGuid = SerializableGuid.NewGuid();
							}
						}
						else
						{
							foreach (WireEntry wireEntry in bundleUsingInstances.Wires)
							{
								if (wireEntry.EmitterComponentAssemblyQualifiedTypeName == emitterTypeName && wireEntry.ReceiverComponentAssemblyQualifiedTypeName == receiverTypeName && wireEntry.EmitterMemberName == emitterMemberName && wireEntry.ReceiverMemberName == receiverMemberName)
								{
									serializableGuid = wireEntry.WireId;
									break;
								}
							}
							if (serializableGuid.IsEmpty)
							{
								serializableGuid = SerializableGuid.NewGuid();
							}
						}
						try
						{
							SerializableGuid assetIdFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetAssetIdFromInstanceId(emitterInstanceId);
							PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(assetIdFromInstanceId);
							SerializableGuid assetIdFromInstanceId2 = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetAssetIdFromInstanceId(receiverInstanceId);
							PropLibrary.RuntimePropInfo runtimePropInfo2 = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(assetIdFromInstanceId2);
							CustomEvent customEvent = new CustomEvent("wireCreated");
							customEvent.Add("propName1", runtimePropInfo.PropData.Name);
							customEvent.Add("propId1", assetIdFromInstanceId.ToString());
							customEvent.Add("propName2", runtimePropInfo2.PropData.Name);
							customEvent.Add("assetId2", assetIdFromInstanceId2.ToString());
							AnalyticsService.Instance.RecordEvent(customEvent);
						}
						catch (Exception ex)
						{
							Debug.LogException(new Exception("Analytics error with wiring", ex));
						}
						int userId2 = NetworkBehaviourSingleton<UserIdManager>.Instance.GetUserId(rpcParams.Receive.SenderClientId);
						this.WireTogether_ClientRpc(userId2, serializableGuid2, serializableGuid, emitterInstanceId, emitterMemberName, emitterTypeName, receiverInstanceId, receiverMemberName, receiverTypeName, storedParameterValues, pendingRerouteNodes, wireColor);
						if (!base.IsClient)
						{
							this.ApplyWiring(serializableGuid2, serializableGuid, emitterInstanceId, emitterMemberName, emitterTypeName, receiverInstanceId, receiverMemberName, receiverTypeName, storedParameterValues, pendingRerouteNodes, wireColor);
						}
						MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.RevisionTimestamp = DateTime.UtcNow.Ticks;
						if (pendingRerouteNodes.Length != 0)
						{
							MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.Changes.Add(new ChangeData
							{
								ChangeType = ChangeType.WireRerouteAdded,
								UserId = userId
							});
						}
						MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.Changes.Add(new ChangeData
						{
							ChangeType = ChangeType.WireCreated,
							UserId = userId
						});
						NetworkBehaviourSingleton<CreatorManager>.Instance.SaveLoadManager.SaveLevel();
						return;
					}
					IL_016F:
					Debug.LogError(string.Format("emitter: {0}, receiver: {1}", eventInfo, eventInfo2));
					string text = "receiver params {0} stored params {1}";
					object obj = eventInfo2.ParamList.Count;
					string[] value3 = storedParameterValues.value;
					Debug.LogError(string.Format(text, obj, (value3 != null) ? new int?(value3.Length) : null));
				}
			}
		}

		// Token: 0x0600120A RID: 4618 RVA: 0x0005B394 File Offset: 0x00059594
		[ClientRpc]
		private void WireTogether_ClientRpc(int userId, SerializableGuid bundleId, SerializableGuid wireId, SerializableGuid emitterInstanceId, string emitterMemberName, string emitterTypeName, SerializableGuid receiverInstanceId, string receiverMemberName, string receiverTypeName, StringArraySerialized storedParameterValues, SerializableGuid[] rerouteNodes, WireColor wireColor)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			int userId;
			SerializableGuid bundleId;
			SerializableGuid wireId;
			SerializableGuid emitterInstanceId;
			string emitterMemberName;
			string emitterTypeName;
			SerializableGuid receiverInstanceId;
			string receiverMemberName;
			string receiverTypeName;
			StringArraySerialized storedParameterValues;
			SerializableGuid[] rerouteNodes;
			WireColor wireColor;
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(1485607664U, clientRpcParams, RpcDelivery.Reliable);
				BytePacker.WriteValueBitPacked(fastBufferWriter, userId);
				fastBufferWriter.WriteValueSafe<SerializableGuid>(in bundleId, default(FastBufferWriter.ForNetworkSerializable));
				fastBufferWriter.WriteValueSafe<SerializableGuid>(in wireId, default(FastBufferWriter.ForNetworkSerializable));
				fastBufferWriter.WriteValueSafe<SerializableGuid>(in emitterInstanceId, default(FastBufferWriter.ForNetworkSerializable));
				bool flag = emitterMemberName != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag, default(FastBufferWriter.ForPrimitives));
				if (flag)
				{
					fastBufferWriter.WriteValueSafe(emitterMemberName, false);
				}
				bool flag2 = emitterTypeName != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag2, default(FastBufferWriter.ForPrimitives));
				if (flag2)
				{
					fastBufferWriter.WriteValueSafe(emitterTypeName, false);
				}
				fastBufferWriter.WriteValueSafe<SerializableGuid>(in receiverInstanceId, default(FastBufferWriter.ForNetworkSerializable));
				bool flag3 = receiverMemberName != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag3, default(FastBufferWriter.ForPrimitives));
				if (flag3)
				{
					fastBufferWriter.WriteValueSafe(receiverMemberName, false);
				}
				bool flag4 = receiverTypeName != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag4, default(FastBufferWriter.ForPrimitives));
				if (flag4)
				{
					fastBufferWriter.WriteValueSafe(receiverTypeName, false);
				}
				fastBufferWriter.WriteValueSafe<StringArraySerialized>(in storedParameterValues, default(FastBufferWriter.ForNetworkSerializable));
				bool flag5 = rerouteNodes != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag5, default(FastBufferWriter.ForPrimitives));
				if (flag5)
				{
					fastBufferWriter.WriteValueSafe<SerializableGuid>(rerouteNodes, default(FastBufferWriter.ForNetworkSerializable));
				}
				fastBufferWriter.WriteValueSafe<WireColor>(in wireColor, default(FastBufferWriter.ForEnums));
				base.__endSendClientRpc(ref fastBufferWriter, 1485607664U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			userId = userId;
			bundleId = bundleId;
			wireId = wireId;
			emitterInstanceId = emitterInstanceId;
			emitterMemberName = emitterMemberName;
			emitterTypeName = emitterTypeName;
			receiverInstanceId = receiverInstanceId;
			receiverMemberName = receiverMemberName;
			receiverTypeName = receiverTypeName;
			storedParameterValues = storedParameterValues;
			rerouteNodes = rerouteNodes;
			wireColor = wireColor;
			if (NetworkBehaviourSingleton<CreatorManager>.Instance.RpcReceiveState == RpcReceiveState.Allow)
			{
				this.WireTogether(userId, bundleId, wireId, emitterInstanceId, emitterMemberName, emitterTypeName, receiverInstanceId, receiverMemberName, receiverTypeName, storedParameterValues, rerouteNodes, wireColor);
				return;
			}
			NetworkBehaviourSingleton<CreatorManager>.Instance.AddCachedRpc(delegate
			{
				this.WireTogether(userId, bundleId, wireId, emitterInstanceId, emitterMemberName, emitterTypeName, receiverInstanceId, receiverMemberName, receiverTypeName, storedParameterValues, rerouteNodes, wireColor);
			});
		}

		// Token: 0x0600120B RID: 4619 RVA: 0x0005B738 File Offset: 0x00059938
		private void WireTogether(int userId, SerializableGuid bundleId, SerializableGuid wireId, SerializableGuid emitterInstanceId, string emitterMemberName, string emitterTypeName, SerializableGuid receiverInstanceId, string receiverMemberName, string receiverTypeName, StringArraySerialized storedParameterValues, SerializableGuid[] rerouteNodes, WireColor wireColor)
		{
			this.ApplyWiring(bundleId, wireId, emitterInstanceId, emitterMemberName, emitterTypeName, receiverInstanceId, receiverMemberName, receiverTypeName, storedParameterValues, rerouteNodes, wireColor);
			this.ConvertTemporaryWireIntoPermanent(bundleId, emitterInstanceId, receiverInstanceId, rerouteNodes, userId, wireColor);
			if (userId == EndlessServices.Instance.CloudService.ActiveUserId)
			{
				this.OnWireConfirmed.Invoke();
				this.pendingRerouteNodes.Clear();
				if (rerouteNodes.Length != 0)
				{
					MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.AddInstanceIdsToLevelState_ServerRpc(rerouteNodes);
				}
			}
		}

		// Token: 0x0600120C RID: 4620 RVA: 0x0005B7B4 File Offset: 0x000599B4
		private void ConvertTemporaryWireIntoPermanent(SerializableGuid bundleId, SerializableGuid emitterInstanceId, SerializableGuid receiverInstanceId, IEnumerable<SerializableGuid> rerouteNodeIds, int userId, WireColor wireColor)
		{
			Color color = this.wireColorDictionary[wireColor].Color;
			PhysicalWire physicalWire;
			if (!this.temporaryWires.TryGetValue(userId, out physicalWire) || physicalWire == null)
			{
				physicalWire = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.SpawnPhysicalWire(bundleId, emitterInstanceId, receiverInstanceId, rerouteNodeIds, wireColor, true);
				this.temporaryWires.TryAdd(userId, physicalWire);
			}
			if (!physicalWire.IsLive)
			{
				return;
			}
			physicalWire.ConvertLiveToPlaced(bundleId, emitterInstanceId, receiverInstanceId, rerouteNodeIds, color);
			physicalWire.SetEditing();
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.PhysicalWires.Add(physicalWire);
		}

		// Token: 0x0600120D RID: 4621 RVA: 0x0005B848 File Offset: 0x00059A48
		private void ApplyWiring(SerializableGuid bundleId, SerializableGuid wireId, SerializableGuid emitterInstanceId, string emitterMemberName, string emitterTypeName, SerializableGuid receiverInstanceId, string receiverMemberName, string receiverTypeName, string[] storedParameterValues, IEnumerable<SerializableGuid> rerouteNodes, WireColor wireColor)
		{
			EndlessEventInfo eventInfo = this.GetEventInfo(emitterInstanceId, emitterTypeName, emitterMemberName, true);
			EndlessEventInfo eventInfo2 = this.GetEventInfo(receiverInstanceId, receiverTypeName, receiverMemberName, false);
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.AddWiring(bundleId, wireId, emitterInstanceId, eventInfo, emitterTypeName, receiverInstanceId, eventInfo2, receiverTypeName, storedParameterValues ?? new string[0], rerouteNodes, wireColor);
		}

		// Token: 0x0600120E RID: 4622 RVA: 0x0005B8A0 File Offset: 0x00059AA0
		public void DeleteWire(SerializableGuid wiringId)
		{
			this.AttemptDeleteWire_ServerRpc(wiringId, default(ServerRpcParams));
		}

		// Token: 0x0600120F RID: 4623 RVA: 0x0005B8C0 File Offset: 0x00059AC0
		[ServerRpc(RequireOwnership = false)]
		private void AttemptDeleteWire_ServerRpc(SerializableGuid wiringId, ServerRpcParams serverRpcParams = default(ServerRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(1188419855U, serverRpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<SerializableGuid>(in wiringId, default(FastBufferWriter.ForNetworkSerializable));
				base.__endSendServerRpc(ref fastBufferWriter, 1188419855U, serverRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.AttemptDeleteWire(wiringId, serverRpcParams);
		}

		// Token: 0x06001210 RID: 4624 RVA: 0x0005B9B8 File Offset: 0x00059BB8
		private async void AttemptDeleteWire(SerializableGuid wiringId, ServerRpcParams serverRpcParams)
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
					this.DeleteWire_ClientRpc(wiringId);
					if (!base.IsClient)
					{
						this.RemoveWiring(wiringId);
					}
					MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.RevisionTimestamp = DateTime.UtcNow.Ticks;
					MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.Changes.Add(new ChangeData
					{
						ChangeType = ChangeType.WireDeleted,
						UserId = userId
					});
					NetworkBehaviourSingleton<CreatorManager>.Instance.SaveLoadManager.SaveLevel();
				}
			}
		}

		// Token: 0x06001211 RID: 4625 RVA: 0x0005BA00 File Offset: 0x00059C00
		[ClientRpc]
		private void DeleteWire_ClientRpc(SerializableGuid wiringId)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			SerializableGuid wiringId;
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(494307063U, clientRpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<SerializableGuid>(in wiringId, default(FastBufferWriter.ForNetworkSerializable));
				base.__endSendClientRpc(ref fastBufferWriter, 494307063U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			wiringId = wiringId;
			if (NetworkBehaviourSingleton<CreatorManager>.Instance.RpcReceiveState == RpcReceiveState.Allow)
			{
				this.RemoveWiring(wiringId);
				return;
			}
			NetworkBehaviourSingleton<CreatorManager>.Instance.AddCachedRpc(delegate
			{
				this.RemoveWiring(wiringId);
			});
		}

		// Token: 0x06001212 RID: 4626 RVA: 0x0005BB34 File Offset: 0x00059D34
		private void RemoveWiring(SerializableGuid wiringId)
		{
			SerializableGuid serializableGuid = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RemoveWiring(wiringId);
			if (!serializableGuid.IsEmpty)
			{
				int i = 0;
				while (i < MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.PhysicalWires.Count)
				{
					PhysicalWire physicalWire = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.PhysicalWires[i];
					if (physicalWire.BundleId == serializableGuid)
					{
						for (int j = 0; j < physicalWire.RerouteNodeIds.Count; j++)
						{
							SerializableGuid serializableGuid2 = physicalWire.RerouteNodeIds[j];
							MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(serializableGuid2);
							MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.EraseReroute(serializableGuid2);
							physicalWire.RerouteNodeIds.Remove(serializableGuid2);
							this.pendingRerouteNodes.Remove(serializableGuid2);
						}
						MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.PhysicalWires.RemoveAt(i);
						if (this.temporaryWires.Values.Any((PhysicalWire valueWire) => valueWire == physicalWire))
						{
							physicalWire.ResetToLive();
							break;
						}
						global::UnityEngine.Object.Destroy(physicalWire.gameObject);
						break;
					}
					else
					{
						i++;
					}
				}
			}
			for (int k = 0; k < this.temporaryWires.Keys.Count; k++)
			{
				int num = this.temporaryWires.Keys.ElementAt(k);
				PhysicalWire physicalWire2 = this.temporaryWires[num];
				if (physicalWire2 && physicalWire2.BundleId == wiringId)
				{
					this.temporaryWires[num] = null;
					break;
				}
			}
			this.OnWireRemoved.Invoke();
		}

		// Token: 0x06001213 RID: 4627 RVA: 0x0005BD00 File Offset: 0x00059F00
		public void DestroyWires(IEnumerable<SerializableGuid> removedWires)
		{
			foreach (SerializableGuid serializableGuid in removedWires)
			{
				this.RemoveWiring(serializableGuid);
			}
		}

		// Token: 0x06001214 RID: 4628 RVA: 0x0005BD48 File Offset: 0x00059F48
		public void UpdateColorForWire(SerializableGuid wireId, WireColor newColor)
		{
			this.UpdateWireColor_ServerRpc(wireId, newColor, default(ServerRpcParams));
		}

		// Token: 0x06001215 RID: 4629 RVA: 0x0005BD68 File Offset: 0x00059F68
		[ServerRpc(RequireOwnership = false)]
		public void UpdateWireColor_ServerRpc(SerializableGuid wireId, WireColor newColor, ServerRpcParams serverRpcParams = default(ServerRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(1994188595U, serverRpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<SerializableGuid>(in wireId, default(FastBufferWriter.ForNetworkSerializable));
				fastBufferWriter.WriteValueSafe<WireColor>(in newColor, default(FastBufferWriter.ForEnums));
				base.__endSendServerRpc(ref fastBufferWriter, 1994188595U, serverRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.UpdateWireColor(wireId, newColor, serverRpcParams);
		}

		// Token: 0x06001216 RID: 4630 RVA: 0x0005BE7C File Offset: 0x0005A07C
		private async void UpdateWireColor(SerializableGuid wireId, WireColor newColor, ServerRpcParams serverRpcParams)
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
					this.UpdateWireColor_ClientRpc(wireId, newColor);
					if (!base.IsClient)
					{
						this.ApplyWireColorUpdate(wireId, newColor);
					}
					MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.RevisionTimestamp = DateTime.UtcNow.Ticks;
					MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.Changes.Add(new ChangeData
					{
						ChangeType = ChangeType.WireColorUpdated,
						UserId = userId
					});
					NetworkBehaviourSingleton<CreatorManager>.Instance.SaveLoadManager.SaveLevel();
				}
			}
		}

		// Token: 0x06001217 RID: 4631 RVA: 0x0005BECC File Offset: 0x0005A0CC
		[ClientRpc]
		public void UpdateWireColor_ClientRpc(SerializableGuid wireId, WireColor newColor)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			SerializableGuid wireId;
			WireColor newColor;
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(3360193517U, clientRpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<SerializableGuid>(in wireId, default(FastBufferWriter.ForNetworkSerializable));
				fastBufferWriter.WriteValueSafe<WireColor>(in newColor, default(FastBufferWriter.ForEnums));
				base.__endSendClientRpc(ref fastBufferWriter, 3360193517U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			wireId = wireId;
			newColor = newColor;
			if (NetworkBehaviourSingleton<CreatorManager>.Instance.RpcReceiveState == RpcReceiveState.Allow)
			{
				this.ApplyWireColorUpdate(wireId, newColor);
				return;
			}
			NetworkBehaviourSingleton<CreatorManager>.Instance.AddCachedRpc(delegate
			{
				this.ApplyWireColorUpdate(wireId, newColor);
			});
		}

		// Token: 0x06001218 RID: 4632 RVA: 0x0005C026 File Offset: 0x0005A226
		private void ApplyWireColorUpdate(SerializableGuid wireId, WireColor newColor)
		{
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.UpdateWireColor(wireId, newColor);
		}

		// Token: 0x06001219 RID: 4633 RVA: 0x0005C040 File Offset: 0x0005A240
		internal List<SerializableGuid> FilterRerouteNodeGuids(SerializableGuid[] allGuids)
		{
			List<SerializableGuid> list = new List<SerializableGuid>();
			foreach (SerializableGuid serializableGuid in allGuids)
			{
				for (int j = 0; j < MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.PhysicalWires.Count; j++)
				{
					if (MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.PhysicalWires[j].ContainsRerouteNodeId(serializableGuid))
					{
						list.Add(serializableGuid);
						break;
					}
				}
			}
			return list;
		}

		// Token: 0x0600121A RID: 4634 RVA: 0x0005C0B8 File Offset: 0x0005A2B8
		[ServerRpc(RequireOwnership = false)]
		internal void EraseRerouteNodes_ServerRpc(SerializableGuid[] instanceIds)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				ServerRpcParams serverRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(2949353187U, serverRpcParams, RpcDelivery.Reliable);
				bool flag = instanceIds != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag, default(FastBufferWriter.ForPrimitives));
				if (flag)
				{
					fastBufferWriter.WriteValueSafe<SerializableGuid>(instanceIds, default(FastBufferWriter.ForNetworkSerializable));
				}
				base.__endSendServerRpc(ref fastBufferWriter, 2949353187U, serverRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.EraseRerouteNodes_ClientRpc(instanceIds);
			if (!base.IsClient)
			{
				this.EraseRerouteNodes(instanceIds);
			}
		}

		// Token: 0x0600121B RID: 4635 RVA: 0x0005C1EC File Offset: 0x0005A3EC
		[ClientRpc]
		private void EraseRerouteNodes_ClientRpc(SerializableGuid[] instanceIds)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			SerializableGuid[] instanceIds;
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(1441364666U, clientRpcParams, RpcDelivery.Reliable);
				bool flag = instanceIds != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag, default(FastBufferWriter.ForPrimitives));
				if (flag)
				{
					fastBufferWriter.WriteValueSafe<SerializableGuid>(instanceIds, default(FastBufferWriter.ForNetworkSerializable));
				}
				base.__endSendClientRpc(ref fastBufferWriter, 1441364666U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			instanceIds = instanceIds;
			if (NetworkBehaviourSingleton<CreatorManager>.Instance.RpcReceiveState == RpcReceiveState.Allow)
			{
				this.EraseRerouteNodes(instanceIds);
				return;
			}
			NetworkBehaviourSingleton<CreatorManager>.Instance.AddCachedRpc(delegate
			{
				this.EraseRerouteNodes(instanceIds);
			});
		}

		// Token: 0x0600121C RID: 4636 RVA: 0x0005C350 File Offset: 0x0005A550
		private void EraseRerouteNodes(SerializableGuid[] instanceIds)
		{
			List<PhysicalWire> list = new List<PhysicalWire>();
			foreach (SerializableGuid serializableGuid in instanceIds)
			{
				MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(serializableGuid);
				MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.EraseReroute(serializableGuid);
				if (this.pendingRerouteNodes.Remove(serializableGuid) && this.pendingRerouteNodes.Count == 0)
				{
					this.DisableRerouteUndo.Invoke();
				}
				foreach (PhysicalWire physicalWire in MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.PhysicalWires)
				{
					if (physicalWire.RerouteNodeIds.Remove(serializableGuid))
					{
						list.Add(physicalWire);
						break;
					}
				}
			}
			foreach (PhysicalWire physicalWire2 in list)
			{
				physicalWire2.GenerateLineRendererPositions();
			}
			foreach (int num in this.temporaryWires.Keys)
			{
				PhysicalWire physicalWire3 = this.temporaryWires[num];
				if (physicalWire3 && list.Contains(physicalWire3) && physicalWire3.RerouteNodeIds.Count == 0)
				{
					this.DisableRerouteUndo.Invoke();
				}
			}
		}

		// Token: 0x0600121D RID: 4637 RVA: 0x0005C4E4 File Offset: 0x0005A6E4
		public void RequestPopLastRerouteNode(InputAction.CallbackContext callbackContext)
		{
			int activeUserId = EndlessServices.Instance.CloudService.ActiveUserId;
			if (!this.temporaryWires[activeUserId])
			{
				return;
			}
			this.PopLastRerouteNode_ServerRPC(default(ServerRpcParams));
		}

		// Token: 0x0600121E RID: 4638 RVA: 0x0005C524 File Offset: 0x0005A724
		[ServerRpc(RequireOwnership = false)]
		public void PopLastRerouteNode_ServerRPC(ServerRpcParams serverRpc = default(ServerRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(416093770U, serverRpc, RpcDelivery.Reliable);
				base.__endSendServerRpc(ref fastBufferWriter, 416093770U, serverRpc, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.PopLastRerouteNode(serverRpc);
		}

		// Token: 0x0600121F RID: 4639 RVA: 0x0005C600 File Offset: 0x0005A800
		private async void PopLastRerouteNode(ServerRpcParams serverRpc)
		{
			if (base.IsServer)
			{
				ulong senderClientId = serverRpc.Receive.SenderClientId;
				int userId;
				if (!NetworkBehaviourSingleton<UserIdManager>.Instance.TryGetUserId(senderClientId, out userId))
				{
					Debug.LogException(new Exception(string.Format("Unable to determine User Id for Client Id: {0}", senderClientId)));
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
					SerializableGuid serializableGuid = SerializableGuid.Empty;
					if (this.playerToTempReroutes.ContainsKey(userId) && this.playerToTempReroutes[userId].Count > 0)
					{
						serializableGuid = this.playerToTempReroutes[userId][this.playerToTempReroutes[userId].Count - 1];
						this.playerToTempReroutes[userId].RemoveAt(this.playerToTempReroutes[userId].Count - 1);
					}
					else if (this.temporaryWires.ContainsKey(userId) && this.temporaryWires[userId].RerouteNodeIds.Count > 0)
					{
						serializableGuid = this.temporaryWires[userId].RerouteNodeIds[this.temporaryWires[userId].RerouteNodeIds.Count - 1];
					}
					if (!serializableGuid.IsEmpty)
					{
						this.EraseRerouteNodes_ServerRpc(new SerializableGuid[] { serializableGuid });
						MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.RevisionTimestamp = DateTime.UtcNow.Ticks;
						MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.Changes.Add(new ChangeData
						{
							ChangeType = ChangeType.WireRerouteDeleted,
							UserId = userId
						});
						NetworkBehaviourSingleton<CreatorManager>.Instance.SaveLoadManager.SaveLevel();
					}
				}
			}
		}

		// Token: 0x06001220 RID: 4640 RVA: 0x0005C640 File Offset: 0x0005A840
		public void LateUpdate()
		{
			if (!base.IsActive)
			{
				return;
			}
			if (MonoBehaviourSingleton<StageManager>.Instance.ActiveStage == null)
			{
				return;
			}
			if (MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.PhysicalWires.Count == 0)
			{
				return;
			}
			if (this.currentWireIndex > MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.PhysicalWires.Count - 1)
			{
				this.currentWireIndex = 0;
			}
			PhysicalWire physicalWire = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.PhysicalWires[this.currentWireIndex];
			if (physicalWire != null)
			{
				physicalWire.BakeCollision();
			}
			this.currentWireIndex++;
		}

		// Token: 0x06001222 RID: 4642 RVA: 0x0005C790 File Offset: 0x0005A990
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x06001223 RID: 4643 RVA: 0x0005C7A8 File Offset: 0x0005A9A8
		protected override void __initializeRpcs()
		{
			base.__registerRpc(405623707U, new NetworkBehaviour.RpcReceiveHandler(WiringTool.__rpc_handler_405623707), "ClearPendingRerouteNodesForPlayer_ServerRpc");
			base.__registerRpc(4103524712U, new NetworkBehaviour.RpcReceiveHandler(WiringTool.__rpc_handler_4103524712), "WiringToolExited_ServerRpc");
			base.__registerRpc(1791207916U, new NetworkBehaviour.RpcReceiveHandler(WiringTool.__rpc_handler_1791207916), "WiringToolExited_ClientRpc");
			base.__registerRpc(354023136U, new NetworkBehaviour.RpcReceiveHandler(WiringTool.__rpc_handler_354023136), "SetTemporaryWireTargetToPlayer_ServerRpc");
			base.__registerRpc(1104155854U, new NetworkBehaviour.RpcReceiveHandler(WiringTool.__rpc_handler_1104155854), "SetTemporaryWireTargetToPlayer_ClientRpc");
			base.__registerRpc(3716040516U, new NetworkBehaviour.RpcReceiveHandler(WiringTool.__rpc_handler_3716040516), "SetTemporaryWireTarget_ServerRpc");
			base.__registerRpc(4269480805U, new NetworkBehaviour.RpcReceiveHandler(WiringTool.__rpc_handler_4269480805), "SetTemporaryWireTarget_ClientRpc");
			base.__registerRpc(3772252676U, new NetworkBehaviour.RpcReceiveHandler(WiringTool.__rpc_handler_3772252676), "SetWireEditedByPlayer_ServerRpc");
			base.__registerRpc(1404899374U, new NetworkBehaviour.RpcReceiveHandler(WiringTool.__rpc_handler_1404899374), "SetWireEditedByPlayer_ClientRpc");
			base.__registerRpc(90801435U, new NetworkBehaviour.RpcReceiveHandler(WiringTool.__rpc_handler_90801435), "SpawnTemporaryWire_ServerRpc");
			base.__registerRpc(3985273964U, new NetworkBehaviour.RpcReceiveHandler(WiringTool.__rpc_handler_3985273964), "SpawnTemporaryWire_ClientRpc");
			base.__registerRpc(359986616U, new NetworkBehaviour.RpcReceiveHandler(WiringTool.__rpc_handler_359986616), "AddRerouteNodeToPosition_ServerRpc");
			base.__registerRpc(101785308U, new NetworkBehaviour.RpcReceiveHandler(WiringTool.__rpc_handler_101785308), "PlaceRerouteNode_ClientRPC");
			base.__registerRpc(3636364046U, new NetworkBehaviour.RpcReceiveHandler(WiringTool.__rpc_handler_3636364046), "ClearTemporaryWireForPlayer_ServerRpc");
			base.__registerRpc(3434663878U, new NetworkBehaviour.RpcReceiveHandler(WiringTool.__rpc_handler_3434663878), "ClearTemporaryWireForPlayer_ClientRpc");
			base.__registerRpc(328729018U, new NetworkBehaviour.RpcReceiveHandler(WiringTool.__rpc_handler_328729018), "UpdateWireDetails_ServerRpc");
			base.__registerRpc(1095343870U, new NetworkBehaviour.RpcReceiveHandler(WiringTool.__rpc_handler_1095343870), "UpdateWireDetails_ClientRpc");
			base.__registerRpc(1366945158U, new NetworkBehaviour.RpcReceiveHandler(WiringTool.__rpc_handler_1366945158), "AttemptWireTogether_ServerRpc");
			base.__registerRpc(1485607664U, new NetworkBehaviour.RpcReceiveHandler(WiringTool.__rpc_handler_1485607664), "WireTogether_ClientRpc");
			base.__registerRpc(1188419855U, new NetworkBehaviour.RpcReceiveHandler(WiringTool.__rpc_handler_1188419855), "AttemptDeleteWire_ServerRpc");
			base.__registerRpc(494307063U, new NetworkBehaviour.RpcReceiveHandler(WiringTool.__rpc_handler_494307063), "DeleteWire_ClientRpc");
			base.__registerRpc(1994188595U, new NetworkBehaviour.RpcReceiveHandler(WiringTool.__rpc_handler_1994188595), "UpdateWireColor_ServerRpc");
			base.__registerRpc(3360193517U, new NetworkBehaviour.RpcReceiveHandler(WiringTool.__rpc_handler_3360193517), "UpdateWireColor_ClientRpc");
			base.__registerRpc(2949353187U, new NetworkBehaviour.RpcReceiveHandler(WiringTool.__rpc_handler_2949353187), "EraseRerouteNodes_ServerRpc");
			base.__registerRpc(1441364666U, new NetworkBehaviour.RpcReceiveHandler(WiringTool.__rpc_handler_1441364666), "EraseRerouteNodes_ClientRpc");
			base.__registerRpc(416093770U, new NetworkBehaviour.RpcReceiveHandler(WiringTool.__rpc_handler_416093770), "PopLastRerouteNode_ServerRPC");
			base.__initializeRpcs();
		}

		// Token: 0x06001224 RID: 4644 RVA: 0x0005CA98 File Offset: 0x0005AC98
		private static void __rpc_handler_405623707(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
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
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((WiringTool)target).ClearPendingRerouteNodesForPlayer_ServerRpc(array, server);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06001225 RID: 4645 RVA: 0x0005CB40 File Offset: 0x0005AD40
		private static void __rpc_handler_4103524712(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
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
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((WiringTool)target).WiringToolExited_ServerRpc(array, server);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06001226 RID: 4646 RVA: 0x0005CBE8 File Offset: 0x0005ADE8
		private static void __rpc_handler_1791207916(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			int num;
			ByteUnpacker.ReadValueBitPacked(reader, out num);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((WiringTool)target).WiringToolExited_ClientRpc(num);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06001227 RID: 4647 RVA: 0x0005CC4C File Offset: 0x0005AE4C
		private static void __rpc_handler_354023136(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((WiringTool)target).SetTemporaryWireTargetToPlayer_ServerRpc(server);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06001228 RID: 4648 RVA: 0x0005CCAC File Offset: 0x0005AEAC
		private static void __rpc_handler_1104155854(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			int num;
			ByteUnpacker.ReadValueBitPacked(reader, out num);
			ulong num2;
			ByteUnpacker.ReadValueBitPacked(reader, out num2);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((WiringTool)target).SetTemporaryWireTargetToPlayer_ClientRpc(num, num2);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06001229 RID: 4649 RVA: 0x0005CD20 File Offset: 0x0005AF20
		private static void __rpc_handler_3716040516(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
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
			((WiringTool)target).SetTemporaryWireTarget_ServerRpc(serializableGuid, server);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x0600122A RID: 4650 RVA: 0x0005CDA0 File Offset: 0x0005AFA0
		private static void __rpc_handler_4269480805(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			SerializableGuid serializableGuid;
			reader.ReadValueSafe<SerializableGuid>(out serializableGuid, default(FastBufferWriter.ForNetworkSerializable));
			int num;
			ByteUnpacker.ReadValueBitPacked(reader, out num);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((WiringTool)target).SetTemporaryWireTarget_ClientRpc(serializableGuid, num);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x0600122B RID: 4651 RVA: 0x0005CE24 File Offset: 0x0005B024
		private static void __rpc_handler_3772252676(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
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
			((WiringTool)target).SetWireEditedByPlayer_ServerRpc(serializableGuid, server);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x0600122C RID: 4652 RVA: 0x0005CEA4 File Offset: 0x0005B0A4
		private static void __rpc_handler_1404899374(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			SerializableGuid serializableGuid;
			reader.ReadValueSafe<SerializableGuid>(out serializableGuid, default(FastBufferWriter.ForNetworkSerializable));
			int num;
			ByteUnpacker.ReadValueBitPacked(reader, out num);
			ClientRpcParams client = rpcParams.Client;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((WiringTool)target).SetWireEditedByPlayer_ClientRpc(serializableGuid, num, client);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x0600122D RID: 4653 RVA: 0x0005CF34 File Offset: 0x0005B134
		private static void __rpc_handler_90801435(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
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
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((WiringTool)target).SpawnTemporaryWire_ServerRpc(serializableGuid, serializableGuid2, server);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x0600122E RID: 4654 RVA: 0x0005CFD4 File Offset: 0x0005B1D4
		private static void __rpc_handler_3985273964(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			SerializableGuid serializableGuid;
			reader.ReadValueSafe<SerializableGuid>(out serializableGuid, default(FastBufferWriter.ForNetworkSerializable));
			int num;
			ByteUnpacker.ReadValueBitPacked(reader, out num);
			ulong num2;
			ByteUnpacker.ReadValueBitPacked(reader, out num2);
			SerializableGuid serializableGuid2;
			reader.ReadValueSafe<SerializableGuid>(out serializableGuid2, default(FastBufferWriter.ForNetworkSerializable));
			ClientRpcParams client = rpcParams.Client;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((WiringTool)target).SpawnTemporaryWire_ClientRpc(serializableGuid, num, num2, serializableGuid2, client);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x0600122F RID: 4655 RVA: 0x0005D094 File Offset: 0x0005B294
		private static void __rpc_handler_359986616(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			Vector3Int vector3Int;
			reader.ReadValueSafe(out vector3Int);
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((WiringTool)target).AddRerouteNodeToPosition_ServerRpc(vector3Int, server);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06001230 RID: 4656 RVA: 0x0005D104 File Offset: 0x0005B304
		private static void __rpc_handler_101785308(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			Vector3Int vector3Int;
			reader.ReadValueSafe(out vector3Int);
			SerializableGuid serializableGuid;
			reader.ReadValueSafe<SerializableGuid>(out serializableGuid, default(FastBufferWriter.ForNetworkSerializable));
			SerializableGuid serializableGuid2;
			reader.ReadValueSafe<SerializableGuid>(out serializableGuid2, default(FastBufferWriter.ForNetworkSerializable));
			ulong num;
			ByteUnpacker.ReadValueBitPacked(reader, out num);
			ulong num2;
			ByteUnpacker.ReadValueBitPacked(reader, out num2);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((WiringTool)target).PlaceRerouteNode_ClientRPC(vector3Int, serializableGuid, serializableGuid2, num, num2);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06001231 RID: 4657 RVA: 0x0005D1C8 File Offset: 0x0005B3C8
		private static void __rpc_handler_3636364046(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((WiringTool)target).ClearTemporaryWireForPlayer_ServerRpc(server);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06001232 RID: 4658 RVA: 0x0005D228 File Offset: 0x0005B428
		private static void __rpc_handler_3434663878(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			int num;
			ByteUnpacker.ReadValueBitPacked(reader, out num);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((WiringTool)target).ClearTemporaryWireForPlayer_ClientRpc(num);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06001233 RID: 4659 RVA: 0x0005D28C File Offset: 0x0005B48C
		private static void __rpc_handler_328729018(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
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
			SerializableGuid serializableGuid3;
			reader.ReadValueSafe<SerializableGuid>(out serializableGuid3, default(FastBufferWriter.ForNetworkSerializable));
			bool flag;
			reader.ReadValueSafe<bool>(out flag, default(FastBufferWriter.ForPrimitives));
			string text = null;
			if (flag)
			{
				reader.ReadValueSafe(out text, false);
			}
			bool flag2;
			reader.ReadValueSafe<bool>(out flag2, default(FastBufferWriter.ForPrimitives));
			string text2 = null;
			if (flag2)
			{
				reader.ReadValueSafe(out text2, false);
			}
			SerializableGuid serializableGuid4;
			reader.ReadValueSafe<SerializableGuid>(out serializableGuid4, default(FastBufferWriter.ForNetworkSerializable));
			bool flag3;
			reader.ReadValueSafe<bool>(out flag3, default(FastBufferWriter.ForPrimitives));
			string text3 = null;
			if (flag3)
			{
				reader.ReadValueSafe(out text3, false);
			}
			bool flag4;
			reader.ReadValueSafe<bool>(out flag4, default(FastBufferWriter.ForPrimitives));
			string text4 = null;
			if (flag4)
			{
				reader.ReadValueSafe(out text4, false);
			}
			StringArraySerialized stringArraySerialized;
			reader.ReadValueSafe<StringArraySerialized>(out stringArraySerialized, default(FastBufferWriter.ForNetworkSerializable));
			bool flag5;
			reader.ReadValueSafe<bool>(out flag5, default(FastBufferWriter.ForPrimitives));
			SerializableGuid[] array = null;
			if (flag5)
			{
				reader.ReadValueSafe<SerializableGuid>(out array, default(FastBufferWriter.ForNetworkSerializable));
			}
			WireColor wireColor;
			reader.ReadValueSafe<WireColor>(out wireColor, default(FastBufferWriter.ForEnums));
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((WiringTool)target).UpdateWireDetails_ServerRpc(serializableGuid, serializableGuid2, serializableGuid3, text, text2, serializableGuid4, text3, text4, stringArraySerialized, array, wireColor, server);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06001234 RID: 4660 RVA: 0x0005D4E0 File Offset: 0x0005B6E0
		private static void __rpc_handler_1095343870(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
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
			SerializableGuid serializableGuid3;
			reader.ReadValueSafe<SerializableGuid>(out serializableGuid3, default(FastBufferWriter.ForNetworkSerializable));
			bool flag;
			reader.ReadValueSafe<bool>(out flag, default(FastBufferWriter.ForPrimitives));
			string text = null;
			if (flag)
			{
				reader.ReadValueSafe(out text, false);
			}
			bool flag2;
			reader.ReadValueSafe<bool>(out flag2, default(FastBufferWriter.ForPrimitives));
			string text2 = null;
			if (flag2)
			{
				reader.ReadValueSafe(out text2, false);
			}
			SerializableGuid serializableGuid4;
			reader.ReadValueSafe<SerializableGuid>(out serializableGuid4, default(FastBufferWriter.ForNetworkSerializable));
			bool flag3;
			reader.ReadValueSafe<bool>(out flag3, default(FastBufferWriter.ForPrimitives));
			string text3 = null;
			if (flag3)
			{
				reader.ReadValueSafe(out text3, false);
			}
			bool flag4;
			reader.ReadValueSafe<bool>(out flag4, default(FastBufferWriter.ForPrimitives));
			string text4 = null;
			if (flag4)
			{
				reader.ReadValueSafe(out text4, false);
			}
			StringArraySerialized stringArraySerialized;
			reader.ReadValueSafe<StringArraySerialized>(out stringArraySerialized, default(FastBufferWriter.ForNetworkSerializable));
			bool flag5;
			reader.ReadValueSafe<bool>(out flag5, default(FastBufferWriter.ForPrimitives));
			SerializableGuid[] array = null;
			if (flag5)
			{
				reader.ReadValueSafe<SerializableGuid>(out array, default(FastBufferWriter.ForNetworkSerializable));
			}
			WireColor wireColor;
			reader.ReadValueSafe<WireColor>(out wireColor, default(FastBufferWriter.ForEnums));
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((WiringTool)target).UpdateWireDetails_ClientRpc(serializableGuid, serializableGuid2, serializableGuid3, text, text2, serializableGuid4, text3, text4, stringArraySerialized, array, wireColor);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06001235 RID: 4661 RVA: 0x0005D724 File Offset: 0x0005B924
		private static void __rpc_handler_1366945158(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			SerializableGuid serializableGuid;
			reader.ReadValueSafe<SerializableGuid>(out serializableGuid, default(FastBufferWriter.ForNetworkSerializable));
			bool flag;
			reader.ReadValueSafe<bool>(out flag, default(FastBufferWriter.ForPrimitives));
			string text = null;
			if (flag)
			{
				reader.ReadValueSafe(out text, false);
			}
			bool flag2;
			reader.ReadValueSafe<bool>(out flag2, default(FastBufferWriter.ForPrimitives));
			string text2 = null;
			if (flag2)
			{
				reader.ReadValueSafe(out text2, false);
			}
			SerializableGuid serializableGuid2;
			reader.ReadValueSafe<SerializableGuid>(out serializableGuid2, default(FastBufferWriter.ForNetworkSerializable));
			bool flag3;
			reader.ReadValueSafe<bool>(out flag3, default(FastBufferWriter.ForPrimitives));
			string text3 = null;
			if (flag3)
			{
				reader.ReadValueSafe(out text3, false);
			}
			bool flag4;
			reader.ReadValueSafe<bool>(out flag4, default(FastBufferWriter.ForPrimitives));
			string text4 = null;
			if (flag4)
			{
				reader.ReadValueSafe(out text4, false);
			}
			StringArraySerialized stringArraySerialized;
			reader.ReadValueSafe<StringArraySerialized>(out stringArraySerialized, default(FastBufferWriter.ForNetworkSerializable));
			bool flag5;
			reader.ReadValueSafe<bool>(out flag5, default(FastBufferWriter.ForPrimitives));
			SerializableGuid[] array = null;
			if (flag5)
			{
				reader.ReadValueSafe<SerializableGuid>(out array, default(FastBufferWriter.ForNetworkSerializable));
			}
			WireColor wireColor;
			reader.ReadValueSafe<WireColor>(out wireColor, default(FastBufferWriter.ForEnums));
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((WiringTool)target).AttemptWireTogether_ServerRpc(serializableGuid, text, text2, serializableGuid2, text3, text4, stringArraySerialized, array, wireColor, server);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06001236 RID: 4662 RVA: 0x0005D938 File Offset: 0x0005BB38
		private static void __rpc_handler_1485607664(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			int num;
			ByteUnpacker.ReadValueBitPacked(reader, out num);
			SerializableGuid serializableGuid;
			reader.ReadValueSafe<SerializableGuid>(out serializableGuid, default(FastBufferWriter.ForNetworkSerializable));
			SerializableGuid serializableGuid2;
			reader.ReadValueSafe<SerializableGuid>(out serializableGuid2, default(FastBufferWriter.ForNetworkSerializable));
			SerializableGuid serializableGuid3;
			reader.ReadValueSafe<SerializableGuid>(out serializableGuid3, default(FastBufferWriter.ForNetworkSerializable));
			bool flag;
			reader.ReadValueSafe<bool>(out flag, default(FastBufferWriter.ForPrimitives));
			string text = null;
			if (flag)
			{
				reader.ReadValueSafe(out text, false);
			}
			bool flag2;
			reader.ReadValueSafe<bool>(out flag2, default(FastBufferWriter.ForPrimitives));
			string text2 = null;
			if (flag2)
			{
				reader.ReadValueSafe(out text2, false);
			}
			SerializableGuid serializableGuid4;
			reader.ReadValueSafe<SerializableGuid>(out serializableGuid4, default(FastBufferWriter.ForNetworkSerializable));
			bool flag3;
			reader.ReadValueSafe<bool>(out flag3, default(FastBufferWriter.ForPrimitives));
			string text3 = null;
			if (flag3)
			{
				reader.ReadValueSafe(out text3, false);
			}
			bool flag4;
			reader.ReadValueSafe<bool>(out flag4, default(FastBufferWriter.ForPrimitives));
			string text4 = null;
			if (flag4)
			{
				reader.ReadValueSafe(out text4, false);
			}
			StringArraySerialized stringArraySerialized;
			reader.ReadValueSafe<StringArraySerialized>(out stringArraySerialized, default(FastBufferWriter.ForNetworkSerializable));
			bool flag5;
			reader.ReadValueSafe<bool>(out flag5, default(FastBufferWriter.ForPrimitives));
			SerializableGuid[] array = null;
			if (flag5)
			{
				reader.ReadValueSafe<SerializableGuid>(out array, default(FastBufferWriter.ForNetworkSerializable));
			}
			WireColor wireColor;
			reader.ReadValueSafe<WireColor>(out wireColor, default(FastBufferWriter.ForEnums));
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((WiringTool)target).WireTogether_ClientRpc(num, serializableGuid, serializableGuid2, serializableGuid3, text, text2, serializableGuid4, text3, text4, stringArraySerialized, array, wireColor);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06001237 RID: 4663 RVA: 0x0005DB90 File Offset: 0x0005BD90
		private static void __rpc_handler_1188419855(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
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
			((WiringTool)target).AttemptDeleteWire_ServerRpc(serializableGuid, server);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06001238 RID: 4664 RVA: 0x0005DC10 File Offset: 0x0005BE10
		private static void __rpc_handler_494307063(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			SerializableGuid serializableGuid;
			reader.ReadValueSafe<SerializableGuid>(out serializableGuid, default(FastBufferWriter.ForNetworkSerializable));
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((WiringTool)target).DeleteWire_ClientRpc(serializableGuid);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06001239 RID: 4665 RVA: 0x0005DC80 File Offset: 0x0005BE80
		private static void __rpc_handler_1994188595(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			SerializableGuid serializableGuid;
			reader.ReadValueSafe<SerializableGuid>(out serializableGuid, default(FastBufferWriter.ForNetworkSerializable));
			WireColor wireColor;
			reader.ReadValueSafe<WireColor>(out wireColor, default(FastBufferWriter.ForEnums));
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((WiringTool)target).UpdateWireColor_ServerRpc(serializableGuid, wireColor, server);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x0600123A RID: 4666 RVA: 0x0005DD20 File Offset: 0x0005BF20
		private static void __rpc_handler_3360193517(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			SerializableGuid serializableGuid;
			reader.ReadValueSafe<SerializableGuid>(out serializableGuid, default(FastBufferWriter.ForNetworkSerializable));
			WireColor wireColor;
			reader.ReadValueSafe<WireColor>(out wireColor, default(FastBufferWriter.ForEnums));
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((WiringTool)target).UpdateWireColor_ClientRpc(serializableGuid, wireColor);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x0600123B RID: 4667 RVA: 0x0005DDB0 File Offset: 0x0005BFB0
		private static void __rpc_handler_2949353187(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
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
			((WiringTool)target).EraseRerouteNodes_ServerRpc(array);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x0600123C RID: 4668 RVA: 0x0005DE4C File Offset: 0x0005C04C
		private static void __rpc_handler_1441364666(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
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
			((WiringTool)target).EraseRerouteNodes_ClientRpc(array);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x0600123D RID: 4669 RVA: 0x0005DEE8 File Offset: 0x0005C0E8
		private static void __rpc_handler_416093770(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((WiringTool)target).PopLastRerouteNode_ServerRPC(server);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x0600123E RID: 4670 RVA: 0x0005DF47 File Offset: 0x0005C147
		protected internal override string __getTypeName()
		{
			return "WiringTool";
		}

		// Token: 0x04000E9F RID: 3743
		[SerializeField]
		private PhysicalWire physicalWireTemplate;

		// Token: 0x04000EA0 RID: 3744
		[SerializeField]
		private WireColorDictionary wireColorDictionary;

		// Token: 0x04000EA1 RID: 3745
		public UnityEvent<Transform, SerializableGuid, List<UIEndlessEventList>> OnEventObjectSelected = new UnityEvent<Transform, SerializableGuid, List<UIEndlessEventList>>();

		// Token: 0x04000EA2 RID: 3746
		public UnityEvent<Transform, SerializableGuid, List<UIEndlessEventList>> OnReceiverObjectSelected = new UnityEvent<Transform, SerializableGuid, List<UIEndlessEventList>>();

		// Token: 0x04000EA3 RID: 3747
		public UnityEvent OnWireConfirmed = new UnityEvent();

		// Token: 0x04000EA4 RID: 3748
		public UnityEvent OnWireRemoved = new UnityEvent();

		// Token: 0x04000EA5 RID: 3749
		public UnityEvent<WiringTool.WiringToolState> OnToolStateChanged = new UnityEvent<WiringTool.WiringToolState>();

		// Token: 0x04000EA6 RID: 3750
		public UnityEvent EnableRerouteUndo = new UnityEvent();

		// Token: 0x04000EA7 RID: 3751
		public UnityEvent DisableRerouteUndo = new UnityEvent();

		// Token: 0x04000EA8 RID: 3752
		private SerializableGuid eventObjectInstanceId = SerializableGuid.Empty;

		// Token: 0x04000EA9 RID: 3753
		private SerializableGuid receiverObjectInstanceId = SerializableGuid.Empty;

		// Token: 0x04000EAA RID: 3754
		private List<EndlessEventInfo> emitterEventInfos = new List<EndlessEventInfo>();

		// Token: 0x04000EAB RID: 3755
		private List<EndlessEventInfo> receiverEventInfos = new List<EndlessEventInfo>();

		// Token: 0x04000EAC RID: 3756
		private WiringTool.WiringStage currentWiringStage;

		// Token: 0x04000EAD RID: 3757
		private WiringTool.WiringToolState toolState;

		// Token: 0x04000EAE RID: 3758
		private List<SerializableGuid> pendingRerouteNodes = new List<SerializableGuid>();

		// Token: 0x04000EAF RID: 3759
		private Dictionary<int, List<SerializableGuid>> playerToTempReroutes = new Dictionary<int, List<SerializableGuid>>();

		// Token: 0x04000EB0 RID: 3760
		private Dictionary<int, PhysicalWire> temporaryWires = new Dictionary<int, PhysicalWire>();

		// Token: 0x04000EB1 RID: 3761
		private Transform reroutePreview;

		// Token: 0x04000EB2 RID: 3762
		private SerializableGuid hoveredTarget;

		// Token: 0x04000EB3 RID: 3763
		private PhysicalWire hoveredWire;

		// Token: 0x04000EB4 RID: 3764
		private WiringTool.WiringTarget wiringTarget;

		// Token: 0x04000EB5 RID: 3765
		private PropCell wiringTargetPropCell;

		// Token: 0x04000EB6 RID: 3766
		private PhysicalWire wiringTargetPhysicalWire;

		// Token: 0x04000EB7 RID: 3767
		private PlayerInputActions playerInputActions;

		// Token: 0x04000EB8 RID: 3768
		private int currentWireIndex;

		// Token: 0x02000398 RID: 920
		public enum WiringStage
		{
			// Token: 0x04000EBA RID: 3770
			SelectingEventObject,
			// Token: 0x04000EBB RID: 3771
			SelectingReceiverObject,
			// Token: 0x04000EBC RID: 3772
			AwaitingConfirmation
		}

		// Token: 0x02000399 RID: 921
		public enum WiringToolState
		{
			// Token: 0x04000EBE RID: 3774
			Wiring,
			// Token: 0x04000EBF RID: 3775
			Rerouting
		}

		// Token: 0x0200039A RID: 922
		public enum WiringTarget
		{
			// Token: 0x04000EC1 RID: 3777
			None,
			// Token: 0x04000EC2 RID: 3778
			Prop,
			// Token: 0x04000EC3 RID: 3779
			PhysicalWire
		}
	}
}
