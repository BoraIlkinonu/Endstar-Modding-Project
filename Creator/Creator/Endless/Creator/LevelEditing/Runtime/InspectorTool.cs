using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Endless.Assets;
using Endless.Creator.UI;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.Scripting;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using HackAnythingAnywhere.Core;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.LevelEditing.Runtime
{
	// Token: 0x02000356 RID: 854
	public class InspectorTool : EndlessTool
	{
		// Token: 0x17000279 RID: 633
		// (get) Token: 0x06001021 RID: 4129 RVA: 0x0004B779 File Offset: 0x00049979
		public override ToolType ToolType
		{
			get
			{
				return ToolType.Inspector;
			}
		}

		// Token: 0x1700027A RID: 634
		// (get) Token: 0x06001022 RID: 4130 RVA: 0x0004B77C File Offset: 0x0004997C
		// (set) Token: 0x06001023 RID: 4131 RVA: 0x0004B784 File Offset: 0x00049984
		public InspectorTool.States State { get; private set; }

		// Token: 0x1700027B RID: 635
		// (get) Token: 0x06001024 RID: 4132 RVA: 0x0004B78D File Offset: 0x0004998D
		// (set) Token: 0x06001025 RID: 4133 RVA: 0x0004B795 File Offset: 0x00049995
		public ReferenceFilter EyeDropperReferenceFilter { get; private set; }

		// Token: 0x06001026 RID: 4134 RVA: 0x0004B7A0 File Offset: 0x000499A0
		public override void HandleSelected()
		{
			base.UIToolPrompter.Display(this.initialSelectionPrompt, false);
			UIToolPrompterManager.OnCancel = (Action)Delegate.Combine(UIToolPrompterManager.OnCancel, new Action(this.SetStateToInspect));
			base.Set3DCursorUsesIntersection(true);
			this.PopulatePropInstanceErrorDisplays();
		}

		// Token: 0x06001027 RID: 4135 RVA: 0x0004B7EC File Offset: 0x000499EC
		private void PopulatePropInstanceErrorDisplays()
		{
			foreach (PropEntry propEntry in MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.PropEntries)
			{
				if (((IReadOnlyCollection<ConsoleMessage>)NetworkBehaviourSingleton<UserScriptingConsole>.Instance.GetConsoleMessagesForInstanceId(propEntry.InstanceId).Where(delegate(ConsoleMessage message)
				{
					LogType logType = message.LogType;
					return logType == LogType.Error || logType == LogType.Exception || logType == LogType.Warning;
				}).ToList<ConsoleMessage>()).Count != 0)
				{
					GameObject gameObjectFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(propEntry.InstanceId);
					if (!(gameObjectFromInstanceId == null))
					{
						UIConsoleMessageIndicatorAnchor uiconsoleMessageIndicatorAnchor = UIConsoleMessageIndicatorAnchor.CreateInstance(this.consoleMessageIndicatorAnchorSource, gameObjectFromInstanceId.transform, MonoBehaviourSingleton<UICreatorReferenceManager>.Instance.AnchorContainer, propEntry.InstanceId, new global::UnityEngine.Vector3?(this.GetAnchorPositionOffsetForMessageIndicator(propEntry)));
						this.messageIndicatorAnchors.Add(uiconsoleMessageIndicatorAnchor);
					}
				}
			}
		}

		// Token: 0x06001028 RID: 4136 RVA: 0x0004B8E0 File Offset: 0x00049AE0
		private global::UnityEngine.Vector3 GetAnchorPositionOffsetForMessageIndicator(PropEntry propEntry)
		{
			PropLibrary.RuntimePropInfo runtimePropInfo;
			if (MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(propEntry.AssetId, out runtimePropInfo))
			{
				return new global::UnityEngine.Vector3(0f, (float)runtimePropInfo.PropData.GetBoundingSize().y + 0.25f, 0f);
			}
			return new global::UnityEngine.Vector3(0f, 0.25f, 0f);
		}

		// Token: 0x06001029 RID: 4137 RVA: 0x0004B944 File Offset: 0x00049B44
		public override void HandleDeselected()
		{
			if (this.hoveredPropId != SerializableGuid.Empty)
			{
				MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(this.hoveredPropId, PropLocationType.Default);
				MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(this.hoveredPropId, PropLocationType.NoAction);
			}
			if (this.inspectedInstanceId != SerializableGuid.Empty)
			{
				MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(this.inspectedInstanceId, PropLocationType.Selected);
				MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(this.inspectedInstanceId, PropLocationType.NoAction);
			}
			this.hoverIndicator.gameObject.SetActive(false);
			this.selectedIndicator.gameObject.SetActive(false);
			this.invalidIndicator.gameObject.SetActive(false);
			this.InspectNewProp(SerializableGuid.Empty, SerializableGuid.Empty, null);
			UIToolPrompterManager.OnCancel = (Action)Delegate.Remove(UIToolPrompterManager.OnCancel, new Action(this.SetStateToInspect));
			this.SetStateToInspect();
			foreach (InspectorToolErrorIndicator inspectorToolErrorIndicator in this.activeInstances)
			{
				MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn<InspectorToolErrorIndicator>(inspectorToolErrorIndicator);
			}
			foreach (UIConsoleMessageIndicatorAnchor uiconsoleMessageIndicatorAnchor in this.messageIndicatorAnchors)
			{
				uiconsoleMessageIndicatorAnchor.Close();
			}
			this.messageIndicatorAnchors.Clear();
			this.activeInstances.Clear();
			MonoBehaviourSingleton<MarkerManager>.Instance.MarkCoordinates(Array.Empty<Vector3Int>(), MarkerType.CellHighlight);
		}

		// Token: 0x0600102A RID: 4138 RVA: 0x0004BAD4 File Offset: 0x00049CD4
		public override void CreatorExited()
		{
			this.InspectNewProp(SerializableGuid.Empty, SerializableGuid.Empty, null);
			this.hoverIndicator.gameObject.SetActive(false);
			this.selectedIndicator.gameObject.SetActive(false);
			this.invalidIndicator.gameObject.SetActive(false);
			this.SetStateToInspect();
		}

		// Token: 0x0600102B RID: 4139 RVA: 0x0004BB2C File Offset: 0x00049D2C
		public override void ToolPressed()
		{
			base.ToolPressed();
			if (this.State == InspectorTool.States.EyeDropper && this.EyeDropperReferenceFilter == ReferenceFilter.None)
			{
				this.initialRayOrigin = base.ActiveRay.origin;
				LineCastHit activeLineCastResult = base.ActiveLineCastResult;
				Cell cellFromCoordinate = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetCellFromCoordinate(activeLineCastResult.IntersectedObjectPosition);
				if (cellFromCoordinate is PropCell)
				{
					this.cellReferenceRotationOrigin = activeLineCastResult.IntersectedObjectPosition;
					return;
				}
				if (cellFromCoordinate is TerrainCell)
				{
					this.cellReferenceRotationOrigin = activeLineCastResult.NearestPositionToObject;
				}
			}
		}

		// Token: 0x0600102C RID: 4140 RVA: 0x0004BBB0 File Offset: 0x00049DB0
		public override void ToolHeld()
		{
			base.ToolHeld();
			if (this.State == InspectorTool.States.EyeDropper && this.EyeDropperReferenceFilter == ReferenceFilter.None)
			{
				if ((this.initialRayOrigin - base.ActiveRay.origin).magnitude < this.toolDragDeadZone)
				{
					return;
				}
				global::UnityEngine.Vector3 vector = new global::UnityEngine.Vector3((float)this.cellReferenceRotationOrigin.x, (float)this.cellReferenceRotationOrigin.y - 0.5f, (float)this.cellReferenceRotationOrigin.z);
				Plane plane = new Plane(global::UnityEngine.Vector3.up, vector);
				float num;
				if (plane.Raycast(base.ActiveRay, out num))
				{
					global::UnityEngine.Vector3 point = base.ActiveRay.GetPoint(num);
					LineCastHit activeLineCastResult = base.ActiveLineCastResult;
					if (!this.rotating)
					{
						MonoBehaviourSingleton<MarkerManager>.Instance.MarkCoordinates(Array.Empty<Vector3Int>(), MarkerType.CellHighlight);
						this.rotating = true;
					}
					global::UnityEngine.Vector3 eulerAngles = Quaternion.LookRotation(point - vector, global::UnityEngine.Vector3.up).eulerAngles;
					this.cellReferenceRotation = new float?(eulerAngles.y);
					MonoBehaviourSingleton<MarkerManager>.Instance.MarkCoordinate(this.cellReferenceRotationOrigin, MarkerType.CellHighlight, this.cellReferenceRotation);
				}
			}
		}

		// Token: 0x0600102D RID: 4141 RVA: 0x0004BCD0 File Offset: 0x00049ED0
		public override void ToolReleased()
		{
			LineCastHit activeLineCastResult = base.ActiveLineCastResult;
			Cell cellFromCoordinate = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetCellFromCoordinate(activeLineCastResult.IntersectedObjectPosition);
			if (activeLineCastResult.IntersectionOccured)
			{
				PropCell propCell = cellFromCoordinate as PropCell;
				if (propCell != null)
				{
					SerializableGuid instanceId = propCell.InstanceId;
					SerializableGuid assetIdFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetAssetIdFromInstanceId(instanceId);
					PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(assetIdFromInstanceId);
					PropEntry propEntry = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.GetPropEntry(instanceId);
					InspectorTool.States states = this.State;
					if (states != InspectorTool.States.Inspect)
					{
						if (states != InspectorTool.States.EyeDropper)
						{
							DebugUtility.LogNoEnumSupportError<InspectorTool.States>(this, "ToolReleased", this.State, Array.Empty<object>());
						}
						else if (this.EyeDropperReferenceFilter == ReferenceFilter.None)
						{
							this.OnCellEyedropped.Invoke(this.cellReferenceRotationOrigin, this.cellReferenceRotation);
							this.SetStateToInspect();
						}
						else
						{
							bool flag = true;
							if (this.EyeDropperReferenceFilter != ReferenceFilter.None)
							{
								flag = runtimePropInfo.EndlessProp.ReferenceFilter.HasFlag(this.EyeDropperReferenceFilter);
							}
							if (!flag)
							{
								return;
							}
							this.OnItemEyeDropped.Invoke(assetIdFromInstanceId, propCell.InstanceId, propEntry);
							this.SetStateToInspect();
						}
					}
					else
					{
						this.InspectNewProp(assetIdFromInstanceId, instanceId, propEntry);
					}
				}
				else if (cellFromCoordinate is TerrainCell)
				{
					InspectorTool.States states = this.State;
					if (states != InspectorTool.States.Inspect && states == InspectorTool.States.EyeDropper)
					{
						this.OnCellEyedropped.Invoke(this.cellReferenceRotationOrigin, this.cellReferenceRotation);
						this.SetStateToInspect();
					}
				}
			}
			else if (this.inspectedInstanceId != SerializableGuid.Empty)
			{
				this.InspectNewProp(SerializableGuid.Empty, SerializableGuid.Empty, null);
			}
			if (this.rotating)
			{
				this.rotating = false;
				this.cellReferenceRotation = null;
			}
		}

		// Token: 0x0600102E RID: 4142 RVA: 0x0004BE85 File Offset: 0x0004A085
		public void DeselectProp()
		{
			this.InspectNewProp(SerializableGuid.Empty, SerializableGuid.Empty, null);
		}

		// Token: 0x0600102F RID: 4143 RVA: 0x0004BE98 File Offset: 0x0004A098
		private void InspectNewProp(SerializableGuid propId, SerializableGuid instanceId, PropEntry propEntry)
		{
			if (this.inspectedInstanceId != SerializableGuid.Empty)
			{
				GameObject gameObjectFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(this.inspectedInstanceId);
				if (gameObjectFromInstanceId != null)
				{
					EndlessProp component = gameObjectFromInstanceId.GetComponent<EndlessProp>();
					if (component != null)
					{
						component.HandleInspectionStateChanged(false);
					}
				}
				MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(this.inspectedInstanceId, PropLocationType.Selected);
				MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(this.inspectedInstanceId, PropLocationType.NoAction);
			}
			this.inspectedInstanceId = instanceId;
			this.OnItemInspected.Invoke(propId, instanceId, propEntry);
			if (this.inspectedInstanceId == SerializableGuid.Empty)
			{
				base.UIToolPrompter.Display(this.initialSelectionPrompt, false);
				return;
			}
			SerializableGuid assetIdFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetAssetIdFromInstanceId(instanceId);
			GameObject gameObjectFromInstanceId2 = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(instanceId);
			if (gameObjectFromInstanceId2 != null)
			{
				EndlessProp component2 = gameObjectFromInstanceId2.GetComponent<EndlessProp>();
				if (component2 != null)
				{
					component2.HandleInspectionStateChanged(true);
				}
			}
			PropLibrary.RuntimePropInfo runtimePropInfo;
			if (MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(assetIdFromInstanceId, out runtimePropInfo))
			{
				bool flag = false;
				if (runtimePropInfo.EndlessProp && runtimePropInfo.EndlessProp.ScriptComponent && runtimePropInfo.EndlessProp.ScriptComponent.Script != null)
				{
					flag = runtimePropInfo.EndlessProp.ScriptComponent.Script.InspectorValues.Count > 0;
				}
				if (!flag)
				{
					flag = (from definition in runtimePropInfo.GetAllDefinitions()
						where definition != null
						select definition).Any((ComponentDefinition definition) => definition.InspectableMembers.Count > 0);
				}
				MonoBehaviourSingleton<PropLocationMarker>.Instance.MarkCellsForProp(this.inspectedInstanceId, flag ? PropLocationType.Selected : PropLocationType.NoAction);
			}
			base.UIToolPrompter.Hide();
		}

		// Token: 0x06001030 RID: 4144 RVA: 0x0004C054 File Offset: 0x0004A254
		internal void HandleLabelChange(SerializableGuid instanceId, string newLabel)
		{
			this.HandleLabelChange_ServerRpc(instanceId, newLabel, default(ServerRpcParams));
		}

		// Token: 0x06001031 RID: 4145 RVA: 0x0004C074 File Offset: 0x0004A274
		[ServerRpc(RequireOwnership = false)]
		private void HandleLabelChange_ServerRpc(SerializableGuid instanceId, string newLabel, ServerRpcParams serverRpcParams = default(ServerRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(2505368387U, serverRpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<SerializableGuid>(in instanceId, default(FastBufferWriter.ForNetworkSerializable));
				bool flag = newLabel != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag, default(FastBufferWriter.ForPrimitives));
				if (flag)
				{
					fastBufferWriter.WriteValueSafe(newLabel, false);
				}
				base.__endSendServerRpc(ref fastBufferWriter, 2505368387U, serverRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.HandleLabelChange(instanceId, newLabel, serverRpcParams);
		}

		// Token: 0x06001032 RID: 4146 RVA: 0x0004C1AC File Offset: 0x0004A3AC
		private async Task HandleLabelChange(SerializableGuid instanceId, string newLabel, ServerRpcParams serverRpcParams)
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
					MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.RevisionTimestamp = DateTime.UtcNow.Ticks;
					MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.Changes.Add(new ChangeData
					{
						ChangeType = ChangeType.PropLabelChanged,
						UserId = userId
					});
					this.HandleLabelChange_ClientRpc(instanceId, newLabel);
					NetworkBehaviourSingleton<CreatorManager>.Instance.SaveLoadManager.SaveLevel();
				}
			}
		}

		// Token: 0x06001033 RID: 4147 RVA: 0x0004C208 File Offset: 0x0004A408
		[ClientRpc]
		private void HandleLabelChange_ClientRpc(SerializableGuid instanceId, string newLabel)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			SerializableGuid instanceId;
			string newLabel;
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(99799061U, clientRpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<SerializableGuid>(in instanceId, default(FastBufferWriter.ForNetworkSerializable));
				bool flag = newLabel != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag, default(FastBufferWriter.ForPrimitives));
				if (flag)
				{
					fastBufferWriter.WriteValueSafe(newLabel, false);
				}
				base.__endSendClientRpc(ref fastBufferWriter, 99799061U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			instanceId = instanceId;
			newLabel = newLabel;
			if (NetworkBehaviourSingleton<CreatorManager>.Instance.RpcReceiveState == RpcReceiveState.Allow)
			{
				MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.ApplyLabelChange(instanceId, newLabel);
				return;
			}
			NetworkBehaviourSingleton<CreatorManager>.Instance.AddCachedRpc(delegate
			{
				MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.ApplyLabelChange(instanceId, newLabel);
			});
		}

		// Token: 0x06001034 RID: 4148 RVA: 0x0004C38C File Offset: 0x0004A58C
		internal void HandleMemberChange(SerializableGuid instanceId, MemberChange newChange, string componentTypeName)
		{
			this.HandleMemberChanged_ServerRpc(instanceId, newChange, componentTypeName, default(ServerRpcParams));
		}

		// Token: 0x06001035 RID: 4149 RVA: 0x0004C3AC File Offset: 0x0004A5AC
		[ServerRpc(RequireOwnership = false)]
		private void HandleMemberChanged_ServerRpc(SerializableGuid instanceId, MemberChange newChange, string componentTypeName, ServerRpcParams serverRpcParams = default(ServerRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(2408856282U, serverRpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<SerializableGuid>(in instanceId, default(FastBufferWriter.ForNetworkSerializable));
				bool flag = newChange != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag, default(FastBufferWriter.ForPrimitives));
				if (flag)
				{
					fastBufferWriter.WriteValueSafe<MemberChange>(in newChange, default(FastBufferWriter.ForNetworkSerializable));
				}
				bool flag2 = componentTypeName != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag2, default(FastBufferWriter.ForPrimitives));
				if (flag2)
				{
					fastBufferWriter.WriteValueSafe(componentTypeName, false);
				}
				base.__endSendServerRpc(ref fastBufferWriter, 2408856282U, serverRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.HandleMemberChanged(instanceId, newChange, componentTypeName, serverRpcParams);
		}

		// Token: 0x06001036 RID: 4150 RVA: 0x0004C530 File Offset: 0x0004A730
		private async Task HandleMemberChanged(SerializableGuid instanceId, MemberChange newChange, string componentTypeName, ServerRpcParams serverRpcParams)
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
					this.HandleMemberChanged_ClientRpc(instanceId, newChange, componentTypeName);
					if (!base.IsClient)
					{
						this.ApplyChangeToStage(instanceId, newChange, componentTypeName);
					}
					MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.RevisionTimestamp = DateTime.UtcNow.Ticks;
					MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.Changes.Add(new ChangeData
					{
						ChangeType = ChangeType.PropMemberChanged,
						UserId = userId
					});
					NetworkBehaviourSingleton<CreatorManager>.Instance.SaveLoadManager.SaveLevel();
				}
			}
		}

		// Token: 0x06001037 RID: 4151 RVA: 0x0004C594 File Offset: 0x0004A794
		[ClientRpc]
		private void HandleMemberChanged_ClientRpc(SerializableGuid instanceId, MemberChange newChange, string assemblyQualifiedName)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			SerializableGuid instanceId;
			MemberChange newChange;
			string assemblyQualifiedName;
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(995148298U, clientRpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<SerializableGuid>(in instanceId, default(FastBufferWriter.ForNetworkSerializable));
				bool flag = newChange != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag, default(FastBufferWriter.ForPrimitives));
				if (flag)
				{
					fastBufferWriter.WriteValueSafe<MemberChange>(in newChange, default(FastBufferWriter.ForNetworkSerializable));
				}
				bool flag2 = assemblyQualifiedName != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag2, default(FastBufferWriter.ForPrimitives));
				if (flag2)
				{
					fastBufferWriter.WriteValueSafe(assemblyQualifiedName, false);
				}
				base.__endSendClientRpc(ref fastBufferWriter, 995148298U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			instanceId = instanceId;
			newChange = newChange;
			assemblyQualifiedName = assemblyQualifiedName;
			if (NetworkBehaviourSingleton<CreatorManager>.Instance.RpcReceiveState == RpcReceiveState.Allow)
			{
				this.ApplyChangeToStage(instanceId, newChange, assemblyQualifiedName);
				return;
			}
			NetworkBehaviourSingleton<CreatorManager>.Instance.AddCachedRpc(delegate
			{
				this.ApplyChangeToStage(instanceId, newChange, assemblyQualifiedName);
			});
		}

		// Token: 0x06001038 RID: 4152 RVA: 0x0004C76C File Offset: 0x0004A96C
		private void ApplyChangeToStage(SerializableGuid instanceId, MemberChange newChange, string componentTypeName)
		{
			GameObject gameObjectFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(instanceId);
			if (gameObjectFromInstanceId == null)
			{
				Debug.LogWarning("InspectorTool.ApplyChangeToStage: propGameObject is null and it shouldn't be!");
				return;
			}
			if (string.IsNullOrEmpty(componentTypeName))
			{
				MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.ApplyMemberChange(instanceId, newChange, componentTypeName);
				return;
			}
			Type type = Type.GetType(componentTypeName);
			Component componentInChildren = gameObjectFromInstanceId.GetComponentInChildren(type);
			MemberInfo[] member = componentInChildren.GetType().GetMember(newChange.MemberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (member[0].GetCustomAttribute<EndlessNonSerializedAttribute>() == null)
			{
				MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.ApplyMemberChange(instanceId, newChange, componentTypeName);
			}
			member[0].SetValue(componentInChildren, newChange.ToObject());
		}

		// Token: 0x06001039 RID: 4153 RVA: 0x0004C814 File Offset: 0x0004AA14
		public override void UpdateTool()
		{
			base.UpdateTool();
			LineCastHit activeLineCastResult = base.ActiveLineCastResult;
			InspectorTool.States state = this.State;
			if (state != InspectorTool.States.Inspect)
			{
				if (state != InspectorTool.States.EyeDropper)
				{
					return;
				}
				if (!activeLineCastResult.IntersectionOccured)
				{
					MonoBehaviourSingleton<MarkerManager>.Instance.MarkCoordinates(Array.Empty<Vector3Int>(), MarkerType.CellHighlight);
					return;
				}
				Cell cellFromCoordinate = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetCellFromCoordinate(activeLineCastResult.IntersectedObjectPosition);
				PropCell propCell = cellFromCoordinate as PropCell;
				if (propCell != null)
				{
					SerializableGuid instanceId = propCell.InstanceId;
					SerializableGuid assetIdFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetAssetIdFromInstanceId(instanceId);
					MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(assetIdFromInstanceId);
					if (!this.rotating)
					{
						MonoBehaviourSingleton<MarkerManager>.Instance.MarkCoordinates(Array.Empty<Vector3Int>(), MarkerType.CellHighlight);
						if (this.EyeDropperReferenceFilter == ReferenceFilter.None)
						{
							MonoBehaviourSingleton<MarkerManager>.Instance.MarkCoordinate(activeLineCastResult.IntersectedObjectPosition, MarkerType.CellHighlight, null);
							return;
						}
					}
				}
				else if (cellFromCoordinate is TerrainCell)
				{
					if (this.EyeDropperReferenceFilter != ReferenceFilter.None)
					{
						return;
					}
					if (!this.rotating)
					{
						MonoBehaviourSingleton<MarkerManager>.Instance.MarkCoordinates(Array.Empty<Vector3Int>(), MarkerType.CellHighlight);
						MonoBehaviourSingleton<MarkerManager>.Instance.MarkCoordinate(activeLineCastResult.NearestPositionToObject, MarkerType.CellHighlight, null);
					}
				}
			}
			else
			{
				if (activeLineCastResult.IntersectionOccured)
				{
					PropCell cellFromCoordinateAs = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetCellFromCoordinateAs<PropCell>(activeLineCastResult.IntersectedObjectPosition);
					if (cellFromCoordinateAs != null)
					{
						SerializableGuid assetIdFromInstanceId2 = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetAssetIdFromInstanceId(cellFromCoordinateAs.InstanceId);
						PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(assetIdFromInstanceId2);
						if (cellFromCoordinateAs.InstanceId == this.hoveredPropId)
						{
							return;
						}
						bool flag = false;
						if (this.hoveredPropId == SerializableGuid.Empty || this.hoveredPropId != cellFromCoordinateAs.InstanceId)
						{
							if (this.hoveredPropId != cellFromCoordinateAs.InstanceId && this.hoveredPropId != SerializableGuid.Empty)
							{
								MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(this.hoveredPropId, PropLocationType.Default);
								MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(this.hoveredPropId, PropLocationType.NoAction);
							}
							if (runtimePropInfo.EndlessProp && runtimePropInfo.EndlessProp.ScriptComponent && runtimePropInfo.EndlessProp.ScriptComponent.Script != null)
							{
								flag = runtimePropInfo.EndlessProp.ScriptComponent.Script.InspectorValues.Count > 0;
							}
							if (!flag)
							{
								flag = (from definition in runtimePropInfo.GetAllDefinitions()
									where definition != null
									select definition).Any((ComponentDefinition definition) => definition.InspectableMembers.Count > 0);
							}
						}
						if (cellFromCoordinateAs.InstanceId != this.inspectedInstanceId)
						{
							MonoBehaviourSingleton<PropLocationMarker>.Instance.MarkCellsForProp(cellFromCoordinateAs.InstanceId, flag ? PropLocationType.Default : PropLocationType.NoAction);
						}
						this.hoveredPropId = cellFromCoordinateAs.InstanceId;
					}
					else
					{
						MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(this.hoveredPropId, PropLocationType.Default);
						if (this.hoveredPropId != SerializableGuid.Empty && this.hoveredPropId != this.inspectedInstanceId)
						{
							MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(this.hoveredPropId, PropLocationType.NoAction);
						}
						this.hoveredPropId = SerializableGuid.Empty;
					}
				}
				else if (!this.hoveredPropId.IsEmpty)
				{
					MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(this.hoveredPropId, PropLocationType.Default);
					if (this.hoveredPropId != this.inspectedInstanceId)
					{
						MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(this.hoveredPropId, PropLocationType.NoAction);
					}
					this.hoveredPropId = SerializableGuid.Empty;
				}
				if (this.hoveredPropId == SerializableGuid.Empty && this.State == InspectorTool.States.EyeDropper)
				{
					this.HandleEyeDropperCursor(null);
					return;
				}
			}
		}

		// Token: 0x0600103A RID: 4154 RVA: 0x0004CBBF File Offset: 0x0004ADBF
		public void CancelEyeDrop()
		{
			MonoBehaviourSingleton<MarkerManager>.Instance.MarkCoordinates(Array.Empty<Vector3Int>(), MarkerType.CellHighlight);
		}

		// Token: 0x0600103B RID: 4155 RVA: 0x0004CBD1 File Offset: 0x0004ADD1
		public void SetStateToInspect()
		{
			this.SetState(InspectorTool.States.Inspect);
		}

		// Token: 0x0600103C RID: 4156 RVA: 0x0004CBDA File Offset: 0x0004ADDA
		public void SetStateToEyeDropper(ReferenceFilter newEyeDropperReferenceFilter)
		{
			this.EyeDropperReferenceFilter = newEyeDropperReferenceFilter;
			this.SetState(InspectorTool.States.EyeDropper);
		}

		// Token: 0x0600103D RID: 4157 RVA: 0x0004CBEC File Offset: 0x0004ADEC
		private void SetState(InspectorTool.States newState)
		{
			if (this.State == newState)
			{
				return;
			}
			this.State = newState;
			InspectorTool.States state = this.State;
			if (state != InspectorTool.States.Inspect)
			{
				if (state != InspectorTool.States.EyeDropper)
				{
					DebugUtility.LogNoEnumSupportError<InspectorTool.States>(this, "SetState", this.State, Array.Empty<object>());
				}
				else
				{
					this.SetCursor(this.neutralEyeDropperTexture2D, this.eyeDropperHotSpot);
				}
			}
			else
			{
				if (this.inspectedInstanceId == SerializableGuid.Empty)
				{
					base.UIToolPrompter.Display(this.initialSelectionPrompt, false);
				}
				this.SetCursor(null, Vector2.zero);
			}
			this.OnStateChanged.Invoke(this.State);
		}

		// Token: 0x0600103E RID: 4158 RVA: 0x0004CC88 File Offset: 0x0004AE88
		private void HandleEyeDropperCursor(ReferenceFilter? filter)
		{
			Texture2D texture2D = this.validEyeDropperTexture2D;
			if (filter == null)
			{
				texture2D = this.neutralEyeDropperTexture2D;
			}
			else if (this.EyeDropperReferenceFilter != ReferenceFilter.None && !this.EyeDropperReferenceFilter.HasFlag(filter.Value))
			{
				texture2D = this.invalidEyeDropperTexture2D;
			}
			if (this.activeCursor != texture2D)
			{
				this.SetCursor(texture2D, this.eyeDropperHotSpot);
			}
		}

		// Token: 0x0600103F RID: 4159 RVA: 0x0004CCFC File Offset: 0x0004AEFC
		private void SetCursor(Texture2D texture2D, Vector2 hotspot)
		{
			Cursor.SetCursor(texture2D, hotspot, CursorMode.Auto);
			this.activeCursor = texture2D;
		}

		// Token: 0x06001040 RID: 4160 RVA: 0x0004CD0D File Offset: 0x0004AF0D
		public void ShowConsoleLogFor(SerializableGuid instanceId)
		{
			Debug.Log(string.Format("{0}: {1}", "ShowConsoleLogFor", instanceId));
			UIConsoleWindowView.Display(NetworkBehaviourSingleton<UserScriptingConsole>.Instance.GetConsoleMessagesForInstanceId(instanceId), ConsoleScope.Instance, instanceId, null);
		}

		// Token: 0x06001041 RID: 4161 RVA: 0x0004CD3D File Offset: 0x0004AF3D
		public void SetInspectedId(SerializableGuid instanceId)
		{
			this.inspectedInstanceId = instanceId;
		}

		// Token: 0x06001043 RID: 4163 RVA: 0x0004CDDC File Offset: 0x0004AFDC
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x06001044 RID: 4164 RVA: 0x0004CDF4 File Offset: 0x0004AFF4
		protected override void __initializeRpcs()
		{
			base.__registerRpc(2505368387U, new NetworkBehaviour.RpcReceiveHandler(InspectorTool.__rpc_handler_2505368387), "HandleLabelChange_ServerRpc");
			base.__registerRpc(99799061U, new NetworkBehaviour.RpcReceiveHandler(InspectorTool.__rpc_handler_99799061), "HandleLabelChange_ClientRpc");
			base.__registerRpc(2408856282U, new NetworkBehaviour.RpcReceiveHandler(InspectorTool.__rpc_handler_2408856282), "HandleMemberChanged_ServerRpc");
			base.__registerRpc(995148298U, new NetworkBehaviour.RpcReceiveHandler(InspectorTool.__rpc_handler_995148298), "HandleMemberChanged_ClientRpc");
			base.__initializeRpcs();
		}

		// Token: 0x06001045 RID: 4165 RVA: 0x0004CE7C File Offset: 0x0004B07C
		private static void __rpc_handler_2505368387(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
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
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((InspectorTool)target).HandleLabelChange_ServerRpc(serializableGuid, text, server);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06001046 RID: 4166 RVA: 0x0004CF38 File Offset: 0x0004B138
		private static void __rpc_handler_99799061(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
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
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((InspectorTool)target).HandleLabelChange_ClientRpc(serializableGuid, text);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06001047 RID: 4167 RVA: 0x0004CFE4 File Offset: 0x0004B1E4
		private static void __rpc_handler_2408856282(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
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
			MemberChange memberChange = null;
			if (flag)
			{
				reader.ReadValueSafe<MemberChange>(out memberChange, default(FastBufferWriter.ForNetworkSerializable));
			}
			bool flag2;
			reader.ReadValueSafe<bool>(out flag2, default(FastBufferWriter.ForPrimitives));
			string text = null;
			if (flag2)
			{
				reader.ReadValueSafe(out text, false);
			}
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((InspectorTool)target).HandleMemberChanged_ServerRpc(serializableGuid, memberChange, text, server);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06001048 RID: 4168 RVA: 0x0004D0E8 File Offset: 0x0004B2E8
		private static void __rpc_handler_995148298(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
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
			MemberChange memberChange = null;
			if (flag)
			{
				reader.ReadValueSafe<MemberChange>(out memberChange, default(FastBufferWriter.ForNetworkSerializable));
			}
			bool flag2;
			reader.ReadValueSafe<bool>(out flag2, default(FastBufferWriter.ForPrimitives));
			string text = null;
			if (flag2)
			{
				reader.ReadValueSafe(out text, false);
			}
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((InspectorTool)target).HandleMemberChanged_ClientRpc(serializableGuid, memberChange, text);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06001049 RID: 4169 RVA: 0x0004D1DD File Offset: 0x0004B3DD
		protected internal override string __getTypeName()
		{
			return "InspectorTool";
		}

		// Token: 0x04000D44 RID: 3396
		public UnityEvent<InspectorTool.States> OnStateChanged = new UnityEvent<InspectorTool.States>();

		// Token: 0x04000D45 RID: 3397
		public UnityEvent<SerializableGuid, SerializableGuid, PropEntry> OnItemInspected = new UnityEvent<SerializableGuid, SerializableGuid, PropEntry>();

		// Token: 0x04000D46 RID: 3398
		public UnityEvent<SerializableGuid, SerializableGuid, PropEntry> OnItemEyeDropped = new UnityEvent<SerializableGuid, SerializableGuid, PropEntry>();

		// Token: 0x04000D47 RID: 3399
		public UnityEvent<Vector3Int, float?> OnCellEyedropped = new UnityEvent<Vector3Int, float?>();

		// Token: 0x04000D48 RID: 3400
		[SerializeField]
		private string initialSelectionPrompt = "Select an object to view/edit it's properties";

		// Token: 0x04000D49 RID: 3401
		[SerializeField]
		private Transform hoverIndicator;

		// Token: 0x04000D4A RID: 3402
		[SerializeField]
		private Transform selectedIndicator;

		// Token: 0x04000D4B RID: 3403
		[SerializeField]
		private Transform invalidIndicator;

		// Token: 0x04000D4C RID: 3404
		[SerializeField]
		private float toolDragDeadZone = 0.01f;

		// Token: 0x04000D4D RID: 3405
		[Header("Eye Drop Cursor")]
		[SerializeField]
		private Texture2D validEyeDropperTexture2D;

		// Token: 0x04000D4E RID: 3406
		[SerializeField]
		private Texture2D invalidEyeDropperTexture2D;

		// Token: 0x04000D4F RID: 3407
		[SerializeField]
		private Texture2D neutralEyeDropperTexture2D;

		// Token: 0x04000D50 RID: 3408
		[SerializeField]
		private Vector2 eyeDropperHotSpot = Vector2.zero;

		// Token: 0x04000D51 RID: 3409
		[SerializeField]
		private UIConsoleMessageIndicatorAnchor consoleMessageIndicatorAnchorSource;

		// Token: 0x04000D52 RID: 3410
		[Header("Error Management")]
		[SerializeField]
		private InspectorToolErrorIndicator errorIndicatorTemplate;

		// Token: 0x04000D53 RID: 3411
		private List<InspectorToolErrorIndicator> activeInstances = new List<InspectorToolErrorIndicator>();

		// Token: 0x04000D54 RID: 3412
		private Texture2D activeCursor;

		// Token: 0x04000D55 RID: 3413
		private global::UnityEngine.Vector3 initialRayOrigin;

		// Token: 0x04000D56 RID: 3414
		private SerializableGuid hoveredPropId = SerializableGuid.Empty;

		// Token: 0x04000D57 RID: 3415
		private SerializableGuid inspectedInstanceId = SerializableGuid.Empty;

		// Token: 0x04000D58 RID: 3416
		private Vector3Int cellReferenceRotationOrigin;

		// Token: 0x04000D59 RID: 3417
		private bool rotating;

		// Token: 0x04000D5A RID: 3418
		private float? cellReferenceRotation;

		// Token: 0x04000D5B RID: 3419
		private List<UIConsoleMessageIndicatorAnchor> messageIndicatorAnchors = new List<UIConsoleMessageIndicatorAnchor>();

		// Token: 0x04000D5E RID: 3422
		private bool configureIndicator = true;

		// Token: 0x02000357 RID: 855
		public enum States
		{
			// Token: 0x04000D60 RID: 3424
			Inspect,
			// Token: 0x04000D61 RID: 3425
			EyeDropper
		}
	}
}
