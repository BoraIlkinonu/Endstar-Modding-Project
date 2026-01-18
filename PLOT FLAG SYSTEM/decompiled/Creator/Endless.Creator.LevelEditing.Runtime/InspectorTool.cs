using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

namespace Endless.Creator.LevelEditing.Runtime;

public class InspectorTool : EndlessTool
{
	public enum States
	{
		Inspect,
		EyeDropper
	}

	public UnityEvent<States> OnStateChanged = new UnityEvent<States>();

	public UnityEvent<SerializableGuid, SerializableGuid, PropEntry> OnItemInspected = new UnityEvent<SerializableGuid, SerializableGuid, PropEntry>();

	public UnityEvent<SerializableGuid, SerializableGuid, PropEntry> OnItemEyeDropped = new UnityEvent<SerializableGuid, SerializableGuid, PropEntry>();

	public UnityEvent<Vector3Int, float?> OnCellEyedropped = new UnityEvent<Vector3Int, float?>();

	[SerializeField]
	private string initialSelectionPrompt = "Select an object to view/edit it's properties";

	[SerializeField]
	private Transform hoverIndicator;

	[SerializeField]
	private Transform selectedIndicator;

	[SerializeField]
	private Transform invalidIndicator;

	[SerializeField]
	private float toolDragDeadZone = 0.01f;

	[Header("Eye Drop Cursor")]
	[SerializeField]
	private Texture2D validEyeDropperTexture2D;

	[SerializeField]
	private Texture2D invalidEyeDropperTexture2D;

	[SerializeField]
	private Texture2D neutralEyeDropperTexture2D;

	[SerializeField]
	private Vector2 eyeDropperHotSpot = Vector2.zero;

	[SerializeField]
	private UIConsoleMessageIndicatorAnchor consoleMessageIndicatorAnchorSource;

	[Header("Error Management")]
	[SerializeField]
	private InspectorToolErrorIndicator errorIndicatorTemplate;

	private List<InspectorToolErrorIndicator> activeInstances = new List<InspectorToolErrorIndicator>();

	private Texture2D activeCursor;

	private UnityEngine.Vector3 initialRayOrigin;

	private SerializableGuid hoveredPropId = SerializableGuid.Empty;

	private SerializableGuid inspectedInstanceId = SerializableGuid.Empty;

	private Vector3Int cellReferenceRotationOrigin;

	private bool rotating;

	private float? cellReferenceRotation;

	private List<UIConsoleMessageIndicatorAnchor> messageIndicatorAnchors = new List<UIConsoleMessageIndicatorAnchor>();

	private bool configureIndicator = true;

	public override ToolType ToolType => ToolType.Inspector;

	public States State { get; private set; }

	public ReferenceFilter EyeDropperReferenceFilter { get; private set; }

	public override void HandleSelected()
	{
		base.UIToolPrompter.Display(initialSelectionPrompt);
		UIToolPrompterManager.OnCancel = (Action)Delegate.Combine(UIToolPrompterManager.OnCancel, new Action(SetStateToInspect));
		Set3DCursorUsesIntersection(val: true);
		PopulatePropInstanceErrorDisplays();
	}

	private void PopulatePropInstanceErrorDisplays()
	{
		foreach (PropEntry propEntry in MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.PropEntries)
		{
			if (((IReadOnlyCollection<ConsoleMessage>)NetworkBehaviourSingleton<UserScriptingConsole>.Instance.GetConsoleMessagesForInstanceId(propEntry.InstanceId).Where(delegate(ConsoleMessage message)
			{
				LogType logType = message.LogType;
				return logType == LogType.Error || logType == LogType.Exception || logType == LogType.Warning;
			}).ToList()).Count != 0)
			{
				GameObject gameObjectFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(propEntry.InstanceId);
				if (!(gameObjectFromInstanceId == null))
				{
					UIConsoleMessageIndicatorAnchor item = UIConsoleMessageIndicatorAnchor.CreateInstance(consoleMessageIndicatorAnchorSource, gameObjectFromInstanceId.transform, MonoBehaviourSingleton<UICreatorReferenceManager>.Instance.AnchorContainer, propEntry.InstanceId, GetAnchorPositionOffsetForMessageIndicator(propEntry));
					messageIndicatorAnchors.Add(item);
				}
			}
		}
	}

	private UnityEngine.Vector3 GetAnchorPositionOffsetForMessageIndicator(PropEntry propEntry)
	{
		if (MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(propEntry.AssetId, out var metadata))
		{
			return new UnityEngine.Vector3(0f, (float)metadata.PropData.GetBoundingSize().y + 0.25f, 0f);
		}
		return new UnityEngine.Vector3(0f, 0.25f, 0f);
	}

	public override void HandleDeselected()
	{
		if (hoveredPropId != SerializableGuid.Empty)
		{
			MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(hoveredPropId, PropLocationType.Default);
			MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(hoveredPropId, PropLocationType.NoAction);
		}
		if (inspectedInstanceId != SerializableGuid.Empty)
		{
			MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(inspectedInstanceId, PropLocationType.Selected);
			MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(inspectedInstanceId, PropLocationType.NoAction);
		}
		hoverIndicator.gameObject.SetActive(value: false);
		selectedIndicator.gameObject.SetActive(value: false);
		invalidIndicator.gameObject.SetActive(value: false);
		InspectNewProp(SerializableGuid.Empty, SerializableGuid.Empty, null);
		UIToolPrompterManager.OnCancel = (Action)Delegate.Remove(UIToolPrompterManager.OnCancel, new Action(SetStateToInspect));
		SetStateToInspect();
		foreach (InspectorToolErrorIndicator activeInstance in activeInstances)
		{
			MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn(activeInstance);
		}
		foreach (UIConsoleMessageIndicatorAnchor messageIndicatorAnchor in messageIndicatorAnchors)
		{
			messageIndicatorAnchor.Close();
		}
		messageIndicatorAnchors.Clear();
		activeInstances.Clear();
		MonoBehaviourSingleton<MarkerManager>.Instance.MarkCoordinates(Array.Empty<Vector3Int>(), MarkerType.CellHighlight);
	}

	public override void CreatorExited()
	{
		InspectNewProp(SerializableGuid.Empty, SerializableGuid.Empty, null);
		hoverIndicator.gameObject.SetActive(value: false);
		selectedIndicator.gameObject.SetActive(value: false);
		invalidIndicator.gameObject.SetActive(value: false);
		SetStateToInspect();
	}

	public override void ToolPressed()
	{
		base.ToolPressed();
		if (State == States.EyeDropper && EyeDropperReferenceFilter == ReferenceFilter.None)
		{
			initialRayOrigin = base.ActiveRay.origin;
			LineCastHit activeLineCastResult = base.ActiveLineCastResult;
			Cell cellFromCoordinate = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetCellFromCoordinate(activeLineCastResult.IntersectedObjectPosition);
			if (cellFromCoordinate is PropCell)
			{
				cellReferenceRotationOrigin = activeLineCastResult.IntersectedObjectPosition;
			}
			else if (cellFromCoordinate is TerrainCell)
			{
				cellReferenceRotationOrigin = activeLineCastResult.NearestPositionToObject;
			}
		}
	}

	public override void ToolHeld()
	{
		base.ToolHeld();
		if (State != States.EyeDropper || EyeDropperReferenceFilter != ReferenceFilter.None || (initialRayOrigin - base.ActiveRay.origin).magnitude < toolDragDeadZone)
		{
			return;
		}
		UnityEngine.Vector3 vector = new UnityEngine.Vector3(cellReferenceRotationOrigin.x, (float)cellReferenceRotationOrigin.y - 0.5f, cellReferenceRotationOrigin.z);
		if (new Plane(UnityEngine.Vector3.up, vector).Raycast(base.ActiveRay, out var enter))
		{
			UnityEngine.Vector3 point = base.ActiveRay.GetPoint(enter);
			_ = base.ActiveLineCastResult;
			if (!rotating)
			{
				MonoBehaviourSingleton<MarkerManager>.Instance.MarkCoordinates(Array.Empty<Vector3Int>(), MarkerType.CellHighlight);
				rotating = true;
			}
			cellReferenceRotation = Quaternion.LookRotation(point - vector, UnityEngine.Vector3.up).eulerAngles.y;
			MonoBehaviourSingleton<MarkerManager>.Instance.MarkCoordinate(cellReferenceRotationOrigin, MarkerType.CellHighlight, cellReferenceRotation);
		}
	}

	public override void ToolReleased()
	{
		LineCastHit activeLineCastResult = base.ActiveLineCastResult;
		Cell cellFromCoordinate = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetCellFromCoordinate(activeLineCastResult.IntersectedObjectPosition);
		if (activeLineCastResult.IntersectionOccured)
		{
			if (cellFromCoordinate is PropCell { InstanceId: var instanceId } propCell)
			{
				SerializableGuid assetIdFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetAssetIdFromInstanceId(instanceId);
				PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(assetIdFromInstanceId);
				PropEntry propEntry = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.GetPropEntry(instanceId);
				switch (State)
				{
				case States.Inspect:
					InspectNewProp(assetIdFromInstanceId, instanceId, propEntry);
					break;
				case States.EyeDropper:
				{
					if (EyeDropperReferenceFilter == ReferenceFilter.None)
					{
						OnCellEyedropped.Invoke(cellReferenceRotationOrigin, cellReferenceRotation);
						SetStateToInspect();
						break;
					}
					bool flag = true;
					if (EyeDropperReferenceFilter != ReferenceFilter.None)
					{
						flag = runtimePropInfo.EndlessProp.ReferenceFilter.HasFlag(EyeDropperReferenceFilter);
					}
					if (!flag)
					{
						return;
					}
					OnItemEyeDropped.Invoke(assetIdFromInstanceId, propCell.InstanceId, propEntry);
					SetStateToInspect();
					break;
				}
				default:
					DebugUtility.LogNoEnumSupportError(this, "ToolReleased", State);
					break;
				}
			}
			else if (cellFromCoordinate is TerrainCell)
			{
				States state = State;
				if (state != States.Inspect && state == States.EyeDropper)
				{
					OnCellEyedropped.Invoke(cellReferenceRotationOrigin, cellReferenceRotation);
					SetStateToInspect();
				}
			}
		}
		else if (inspectedInstanceId != SerializableGuid.Empty)
		{
			InspectNewProp(SerializableGuid.Empty, SerializableGuid.Empty, null);
		}
		if (rotating)
		{
			rotating = false;
			cellReferenceRotation = null;
		}
	}

	public void DeselectProp()
	{
		InspectNewProp(SerializableGuid.Empty, SerializableGuid.Empty, null);
	}

	private void InspectNewProp(SerializableGuid propId, SerializableGuid instanceId, PropEntry propEntry)
	{
		if (inspectedInstanceId != SerializableGuid.Empty)
		{
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(inspectedInstanceId)?.GetComponent<EndlessProp>()?.HandleInspectionStateChanged(isInspected: false);
			MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(inspectedInstanceId, PropLocationType.Selected);
			MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(inspectedInstanceId, PropLocationType.NoAction);
		}
		inspectedInstanceId = instanceId;
		OnItemInspected.Invoke(propId, instanceId, propEntry);
		if (inspectedInstanceId == SerializableGuid.Empty)
		{
			base.UIToolPrompter.Display(initialSelectionPrompt);
			return;
		}
		SerializableGuid assetIdFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetAssetIdFromInstanceId(instanceId);
		MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(instanceId)?.GetComponent<EndlessProp>()?.HandleInspectionStateChanged(isInspected: true);
		if (MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(assetIdFromInstanceId, out var metadata))
		{
			bool flag = false;
			if ((bool)metadata.EndlessProp && (bool)metadata.EndlessProp.ScriptComponent && metadata.EndlessProp.ScriptComponent.Script != null)
			{
				flag = metadata.EndlessProp.ScriptComponent.Script.InspectorValues.Count > 0;
			}
			if (!flag)
			{
				flag = (from definition in metadata.GetAllDefinitions()
					where (object)definition != null
					select definition).Any((ComponentDefinition definition) => definition.InspectableMembers.Count > 0);
			}
			MonoBehaviourSingleton<PropLocationMarker>.Instance.MarkCellsForProp(inspectedInstanceId, flag ? PropLocationType.Selected : PropLocationType.NoAction);
		}
		base.UIToolPrompter.Hide();
	}

	internal void HandleLabelChange(SerializableGuid instanceId, string newLabel)
	{
		HandleLabelChange_ServerRpc(instanceId, newLabel);
	}

	[ServerRpc(RequireOwnership = false)]
	private void HandleLabelChange_ServerRpc(SerializableGuid instanceId, string newLabel, ServerRpcParams serverRpcParams = default(ServerRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			FastBufferWriter bufferWriter = __beginSendServerRpc(2505368387u, serverRpcParams, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in instanceId, default(FastBufferWriter.ForNetworkSerializable));
			bool value = newLabel != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(newLabel);
			}
			__endSendServerRpc(ref bufferWriter, 2505368387u, serverRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			HandleLabelChange(instanceId, newLabel, serverRpcParams);
		}
	}

	private async Task HandleLabelChange(SerializableGuid instanceId, string newLabel, ServerRpcParams serverRpcParams)
	{
		if (base.IsServer)
		{
			if (!NetworkBehaviourSingleton<UserIdManager>.Instance.TryGetUserId(serverRpcParams.Receive.SenderClientId, out var userId))
			{
				Debug.LogException(new Exception($"Unable to determine User Id for Client Id: {serverRpcParams.Receive.SenderClientId}"));
			}
			if (await NetworkBehaviourSingleton<CreatorManager>.Instance.UserCanEditLevel(userId))
			{
				MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.RevisionTimestamp = DateTime.UtcNow.Ticks;
				MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.Changes.Add(new ChangeData
				{
					ChangeType = ChangeType.PropLabelChanged,
					UserId = userId
				});
				HandleLabelChange_ClientRpc(instanceId, newLabel);
				NetworkBehaviourSingleton<CreatorManager>.Instance.SaveLoadManager.SaveLevel();
			}
		}
	}

	[ClientRpc]
	private void HandleLabelChange_ClientRpc(SerializableGuid instanceId, string newLabel)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(99799061u, clientRpcParams, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in instanceId, default(FastBufferWriter.ForNetworkSerializable));
			bool value = newLabel != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(newLabel);
			}
			__endSendClientRpc(ref bufferWriter, 99799061u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
		{
			return;
		}
		__rpc_exec_stage = __RpcExecStage.Send;
		SerializableGuid instanceId2 = instanceId;
		string newLabel2 = newLabel;
		if (NetworkBehaviourSingleton<CreatorManager>.Instance.RpcReceiveState == RpcReceiveState.Allow)
		{
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.ApplyLabelChange(instanceId2, newLabel2);
			return;
		}
		NetworkBehaviourSingleton<CreatorManager>.Instance.AddCachedRpc(delegate
		{
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.ApplyLabelChange(instanceId2, newLabel2);
		});
	}

	internal void HandleMemberChange(SerializableGuid instanceId, MemberChange newChange, string componentTypeName)
	{
		HandleMemberChanged_ServerRpc(instanceId, newChange, componentTypeName);
	}

	[ServerRpc(RequireOwnership = false)]
	private void HandleMemberChanged_ServerRpc(SerializableGuid instanceId, MemberChange newChange, string componentTypeName, ServerRpcParams serverRpcParams = default(ServerRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			FastBufferWriter bufferWriter = __beginSendServerRpc(2408856282u, serverRpcParams, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in instanceId, default(FastBufferWriter.ForNetworkSerializable));
			bool value = newChange != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(in newChange, default(FastBufferWriter.ForNetworkSerializable));
			}
			bool value2 = componentTypeName != null;
			bufferWriter.WriteValueSafe(in value2, default(FastBufferWriter.ForPrimitives));
			if (value2)
			{
				bufferWriter.WriteValueSafe(componentTypeName);
			}
			__endSendServerRpc(ref bufferWriter, 2408856282u, serverRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			HandleMemberChanged(instanceId, newChange, componentTypeName, serverRpcParams);
		}
	}

	private async Task HandleMemberChanged(SerializableGuid instanceId, MemberChange newChange, string componentTypeName, ServerRpcParams serverRpcParams)
	{
		if (!base.IsServer)
		{
			return;
		}
		if (!NetworkBehaviourSingleton<UserIdManager>.Instance.TryGetUserId(serverRpcParams.Receive.SenderClientId, out var userId))
		{
			Debug.LogException(new Exception($"Unable to determine User Id for Client Id: {serverRpcParams.Receive.SenderClientId}"));
		}
		if (await NetworkBehaviourSingleton<CreatorManager>.Instance.UserCanEditLevel(userId))
		{
			HandleMemberChanged_ClientRpc(instanceId, newChange, componentTypeName);
			if (!base.IsClient)
			{
				ApplyChangeToStage(instanceId, newChange, componentTypeName);
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

	[ClientRpc]
	private void HandleMemberChanged_ClientRpc(SerializableGuid instanceId, MemberChange newChange, string assemblyQualifiedName)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(995148298u, clientRpcParams, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in instanceId, default(FastBufferWriter.ForNetworkSerializable));
			bool value = newChange != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(in newChange, default(FastBufferWriter.ForNetworkSerializable));
			}
			bool value2 = assemblyQualifiedName != null;
			bufferWriter.WriteValueSafe(in value2, default(FastBufferWriter.ForPrimitives));
			if (value2)
			{
				bufferWriter.WriteValueSafe(assemblyQualifiedName);
			}
			__endSendClientRpc(ref bufferWriter, 995148298u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
		{
			return;
		}
		__rpc_exec_stage = __RpcExecStage.Send;
		SerializableGuid instanceId2 = instanceId;
		MemberChange newChange2 = newChange;
		string assemblyQualifiedName2 = assemblyQualifiedName;
		if (NetworkBehaviourSingleton<CreatorManager>.Instance.RpcReceiveState == RpcReceiveState.Allow)
		{
			ApplyChangeToStage(instanceId2, newChange2, assemblyQualifiedName2);
			return;
		}
		NetworkBehaviourSingleton<CreatorManager>.Instance.AddCachedRpc(delegate
		{
			ApplyChangeToStage(instanceId2, newChange2, assemblyQualifiedName2);
		});
	}

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

	public override void UpdateTool()
	{
		base.UpdateTool();
		LineCastHit activeLineCastResult = base.ActiveLineCastResult;
		switch (State)
		{
		case States.Inspect:
			if (activeLineCastResult.IntersectionOccured)
			{
				PropCell cellFromCoordinateAs = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetCellFromCoordinateAs<PropCell>(activeLineCastResult.IntersectedObjectPosition);
				if (cellFromCoordinateAs != null)
				{
					SerializableGuid assetIdFromInstanceId2 = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetAssetIdFromInstanceId(cellFromCoordinateAs.InstanceId);
					PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(assetIdFromInstanceId2);
					if (cellFromCoordinateAs.InstanceId == hoveredPropId)
					{
						break;
					}
					bool flag = false;
					if (hoveredPropId == SerializableGuid.Empty || hoveredPropId != cellFromCoordinateAs.InstanceId)
					{
						if (hoveredPropId != cellFromCoordinateAs.InstanceId && hoveredPropId != SerializableGuid.Empty)
						{
							MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(hoveredPropId, PropLocationType.Default);
							MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(hoveredPropId, PropLocationType.NoAction);
						}
						if ((bool)runtimePropInfo.EndlessProp && (bool)runtimePropInfo.EndlessProp.ScriptComponent && runtimePropInfo.EndlessProp.ScriptComponent.Script != null)
						{
							flag = runtimePropInfo.EndlessProp.ScriptComponent.Script.InspectorValues.Count > 0;
						}
						if (!flag)
						{
							flag = (from definition in runtimePropInfo.GetAllDefinitions()
								where (object)definition != null
								select definition).Any((ComponentDefinition definition) => definition.InspectableMembers.Count > 0);
						}
					}
					if (cellFromCoordinateAs.InstanceId != inspectedInstanceId)
					{
						MonoBehaviourSingleton<PropLocationMarker>.Instance.MarkCellsForProp(cellFromCoordinateAs.InstanceId, (!flag) ? PropLocationType.NoAction : PropLocationType.Default);
					}
					hoveredPropId = cellFromCoordinateAs.InstanceId;
				}
				else
				{
					MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(hoveredPropId, PropLocationType.Default);
					if (hoveredPropId != SerializableGuid.Empty && hoveredPropId != inspectedInstanceId)
					{
						MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(hoveredPropId, PropLocationType.NoAction);
					}
					hoveredPropId = SerializableGuid.Empty;
				}
			}
			else if (!hoveredPropId.IsEmpty)
			{
				MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(hoveredPropId, PropLocationType.Default);
				if (hoveredPropId != inspectedInstanceId)
				{
					MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(hoveredPropId, PropLocationType.NoAction);
				}
				hoveredPropId = SerializableGuid.Empty;
			}
			if (hoveredPropId == SerializableGuid.Empty && State == States.EyeDropper)
			{
				HandleEyeDropperCursor(null);
			}
			break;
		case States.EyeDropper:
		{
			if (!activeLineCastResult.IntersectionOccured)
			{
				MonoBehaviourSingleton<MarkerManager>.Instance.MarkCoordinates(Array.Empty<Vector3Int>(), MarkerType.CellHighlight);
				break;
			}
			Cell cellFromCoordinate = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetCellFromCoordinate(activeLineCastResult.IntersectedObjectPosition);
			if (cellFromCoordinate is PropCell { InstanceId: var instanceId })
			{
				SerializableGuid assetIdFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetAssetIdFromInstanceId(instanceId);
				MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(assetIdFromInstanceId);
				if (!rotating)
				{
					MonoBehaviourSingleton<MarkerManager>.Instance.MarkCoordinates(Array.Empty<Vector3Int>(), MarkerType.CellHighlight);
					if (EyeDropperReferenceFilter == ReferenceFilter.None)
					{
						MonoBehaviourSingleton<MarkerManager>.Instance.MarkCoordinate(activeLineCastResult.IntersectedObjectPosition, MarkerType.CellHighlight);
					}
				}
			}
			else if (cellFromCoordinate is TerrainCell && EyeDropperReferenceFilter == ReferenceFilter.None && !rotating)
			{
				MonoBehaviourSingleton<MarkerManager>.Instance.MarkCoordinates(Array.Empty<Vector3Int>(), MarkerType.CellHighlight);
				MonoBehaviourSingleton<MarkerManager>.Instance.MarkCoordinate(activeLineCastResult.NearestPositionToObject, MarkerType.CellHighlight);
			}
			break;
		}
		}
	}

	public void CancelEyeDrop()
	{
		MonoBehaviourSingleton<MarkerManager>.Instance.MarkCoordinates(Array.Empty<Vector3Int>(), MarkerType.CellHighlight);
	}

	public void SetStateToInspect()
	{
		SetState(States.Inspect);
	}

	public void SetStateToEyeDropper(ReferenceFilter newEyeDropperReferenceFilter)
	{
		EyeDropperReferenceFilter = newEyeDropperReferenceFilter;
		SetState(States.EyeDropper);
	}

	private void SetState(States newState)
	{
		if (State == newState)
		{
			return;
		}
		State = newState;
		switch (State)
		{
		case States.Inspect:
			if (inspectedInstanceId == SerializableGuid.Empty)
			{
				base.UIToolPrompter.Display(initialSelectionPrompt);
			}
			SetCursor(null, Vector2.zero);
			break;
		case States.EyeDropper:
			SetCursor(neutralEyeDropperTexture2D, eyeDropperHotSpot);
			break;
		default:
			DebugUtility.LogNoEnumSupportError(this, "SetState", State);
			break;
		}
		OnStateChanged.Invoke(State);
	}

	private void HandleEyeDropperCursor(ReferenceFilter? filter)
	{
		Texture2D texture2D = validEyeDropperTexture2D;
		if (!filter.HasValue)
		{
			texture2D = neutralEyeDropperTexture2D;
		}
		else if (EyeDropperReferenceFilter != ReferenceFilter.None && !EyeDropperReferenceFilter.HasFlag(filter.Value))
		{
			texture2D = invalidEyeDropperTexture2D;
		}
		if (activeCursor != texture2D)
		{
			SetCursor(texture2D, eyeDropperHotSpot);
		}
	}

	private void SetCursor(Texture2D texture2D, Vector2 hotspot)
	{
		Cursor.SetCursor(texture2D, hotspot, CursorMode.Auto);
		activeCursor = texture2D;
	}

	public void ShowConsoleLogFor(SerializableGuid instanceId)
	{
		Debug.Log(string.Format("{0}: {1}", "ShowConsoleLogFor", instanceId));
		UIConsoleWindowView.Display(NetworkBehaviourSingleton<UserScriptingConsole>.Instance.GetConsoleMessagesForInstanceId(instanceId), ConsoleScope.Instance, instanceId, null);
	}

	public void SetInspectedId(SerializableGuid instanceId)
	{
		inspectedInstanceId = instanceId;
	}

	protected override void __initializeVariables()
	{
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		__registerRpc(2505368387u, __rpc_handler_2505368387, "HandleLabelChange_ServerRpc");
		__registerRpc(99799061u, __rpc_handler_99799061, "HandleLabelChange_ClientRpc");
		__registerRpc(2408856282u, __rpc_handler_2408856282, "HandleMemberChanged_ServerRpc");
		__registerRpc(995148298u, __rpc_handler_995148298, "HandleMemberChanged_ClientRpc");
		base.__initializeRpcs();
	}

	private static void __rpc_handler_2505368387(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out SerializableGuid value, default(FastBufferWriter.ForNetworkSerializable));
			reader.ReadValueSafe(out bool value2, default(FastBufferWriter.ForPrimitives));
			string s = null;
			if (value2)
			{
				reader.ReadValueSafe(out s, oneByteChars: false);
			}
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((InspectorTool)target).HandleLabelChange_ServerRpc(value, s, server);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_99799061(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out SerializableGuid value, default(FastBufferWriter.ForNetworkSerializable));
			reader.ReadValueSafe(out bool value2, default(FastBufferWriter.ForPrimitives));
			string s = null;
			if (value2)
			{
				reader.ReadValueSafe(out s, oneByteChars: false);
			}
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((InspectorTool)target).HandleLabelChange_ClientRpc(value, s);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_2408856282(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out SerializableGuid value, default(FastBufferWriter.ForNetworkSerializable));
			reader.ReadValueSafe(out bool value2, default(FastBufferWriter.ForPrimitives));
			MemberChange value3 = null;
			if (value2)
			{
				reader.ReadValueSafe(out value3, default(FastBufferWriter.ForNetworkSerializable));
			}
			reader.ReadValueSafe(out bool value4, default(FastBufferWriter.ForPrimitives));
			string s = null;
			if (value4)
			{
				reader.ReadValueSafe(out s, oneByteChars: false);
			}
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((InspectorTool)target).HandleMemberChanged_ServerRpc(value, value3, s, server);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_995148298(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out SerializableGuid value, default(FastBufferWriter.ForNetworkSerializable));
			reader.ReadValueSafe(out bool value2, default(FastBufferWriter.ForPrimitives));
			MemberChange value3 = null;
			if (value2)
			{
				reader.ReadValueSafe(out value3, default(FastBufferWriter.ForNetworkSerializable));
			}
			reader.ReadValueSafe(out bool value4, default(FastBufferWriter.ForPrimitives));
			string s = null;
			if (value4)
			{
				reader.ReadValueSafe(out s, oneByteChars: false);
			}
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((InspectorTool)target).HandleMemberChanged_ClientRpc(value, value3, s);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	protected internal override string __getTypeName()
	{
		return "InspectorTool";
	}
}
