using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Assets;
using Endless.Creator.UI;
using Endless.Data;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.LevelEditing.Wires;
using Endless.Props.Scripting;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Runtime.Shared.Matchmaking;
using Unity.Netcode;
using Unity.Services.Analytics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Endless.Creator.LevelEditing.Runtime;

public class WiringTool : EndlessTool
{
	public enum WiringStage
	{
		SelectingEventObject,
		SelectingReceiverObject,
		AwaitingConfirmation
	}

	public enum WiringToolState
	{
		Wiring,
		Rerouting
	}

	public enum WiringTarget
	{
		None,
		Prop,
		PhysicalWire
	}

	[SerializeField]
	private PhysicalWire physicalWireTemplate;

	[SerializeField]
	private WireColorDictionary wireColorDictionary;

	public UnityEvent<Transform, SerializableGuid, List<UIEndlessEventList>> OnEventObjectSelected = new UnityEvent<Transform, SerializableGuid, List<UIEndlessEventList>>();

	public UnityEvent<Transform, SerializableGuid, List<UIEndlessEventList>> OnReceiverObjectSelected = new UnityEvent<Transform, SerializableGuid, List<UIEndlessEventList>>();

	public UnityEvent OnWireConfirmed = new UnityEvent();

	public UnityEvent OnWireRemoved = new UnityEvent();

	public UnityEvent<WiringToolState> OnToolStateChanged = new UnityEvent<WiringToolState>();

	public UnityEvent EnableRerouteUndo = new UnityEvent();

	public UnityEvent DisableRerouteUndo = new UnityEvent();

	private SerializableGuid eventObjectInstanceId = SerializableGuid.Empty;

	private SerializableGuid receiverObjectInstanceId = SerializableGuid.Empty;

	private List<EndlessEventInfo> emitterEventInfos = new List<EndlessEventInfo>();

	private List<EndlessEventInfo> receiverEventInfos = new List<EndlessEventInfo>();

	private WiringStage currentWiringStage;

	private WiringToolState toolState;

	private List<SerializableGuid> pendingRerouteNodes = new List<SerializableGuid>();

	private Dictionary<int, List<SerializableGuid>> playerToTempReroutes = new Dictionary<int, List<SerializableGuid>>();

	private Dictionary<int, PhysicalWire> temporaryWires = new Dictionary<int, PhysicalWire>();

	private Transform reroutePreview;

	private SerializableGuid hoveredTarget;

	private PhysicalWire hoveredWire;

	private WiringTarget wiringTarget;

	private PropCell wiringTargetPropCell;

	private PhysicalWire wiringTargetPhysicalWire;

	private PlayerInputActions playerInputActions;

	private int currentWireIndex;

	public override ToolType ToolType => ToolType.Wiring;

	public new WiringToolState ToolState => toolState;

	private void Awake()
	{
		playerInputActions = new PlayerInputActions();
	}

	private void Start()
	{
		NetworkBehaviourSingleton<UserIdManager>.Instance.OnUserIdAdded.AddListener(HandleUserIdAdded);
		NetworkBehaviourSingleton<UserIdManager>.Instance.OnUserIdRemoved.AddListener(HandleUserIdRemoved);
		MonoBehaviourSingleton<UIWiringManager>.Instance.OnWiringStateChanged.AddListener(OnUIWiringStateChange);
	}

	private void OnEnable()
	{
		playerInputActions.Player.PopLastRerouteNode.performed += RequestPopLastRerouteNode;
		playerInputActions.Player.PopLastRerouteNode.Enable();
	}

	private void OnDisable()
	{
		playerInputActions.Player.PopLastRerouteNode.performed -= RequestPopLastRerouteNode;
		playerInputActions.Player.PopLastRerouteNode.Disable();
	}

	private void OnUIWiringStateChange(UIWiringStates arg0)
	{
		int userId = NetworkBehaviourSingleton<UserIdManager>.Instance.GetUserId(NetworkManager.Singleton.LocalClientId);
		switch (arg0)
		{
		case UIWiringStates.EditExisting:
			if (!temporaryWires[userId] || !(temporaryWires[userId].BundleId == MonoBehaviourSingleton<UIWiringManager>.Instance.WireEditorController.WireToEdit.WireId))
			{
				SetWireEditedByPlayer_ServerRpc(MonoBehaviourSingleton<UIWiringManager>.Instance.WireEditorController.WireToEdit.WireId);
			}
			break;
		case UIWiringStates.Nothing:
		case UIWiringStates.CreateNew:
			break;
		}
	}

	private void HandleUserIdAdded(int userId)
	{
		if (!playerToTempReroutes.ContainsKey(userId))
		{
			playerToTempReroutes.Add(userId, new List<SerializableGuid>());
		}
		if (!temporaryWires.ContainsKey(userId))
		{
			temporaryWires.Add(userId, null);
		}
	}

	private void HandleUserIdRemoved(int userId)
	{
		if (temporaryWires.ContainsKey(userId))
		{
			PhysicalWire physicalWire = temporaryWires[userId];
			if ((bool)physicalWire)
			{
				if (physicalWire.IsBeingEdited)
				{
					physicalWire.SetPlaced();
				}
				if ((bool)temporaryWires[userId] && !temporaryWires[userId].IsPlaced)
				{
					UnityEngine.Object.Destroy(temporaryWires[userId].gameObject);
					temporaryWires[userId] = null;
				}
			}
			temporaryWires.Remove(userId);
		}
		if (playerToTempReroutes.ContainsKey(userId))
		{
			_ = playerToTempReroutes[userId].Count;
			playerToTempReroutes.Remove(userId);
		}
	}

	public override void CreatorExited()
	{
		DisableRerouteUndo.Invoke();
		MonoBehaviourSingleton<MarkerManager>.Instance.ReleaseAllMarkers();
		EmitterSelectionCancelled();
		TogglePhysicalWires(active: false);
		if ((bool)reroutePreview)
		{
			UnityEngine.Object.Destroy(reroutePreview.gameObject);
		}
		SetToolState(WiringToolState.Wiring);
	}

	public override void HandleDeselected()
	{
		if (eventObjectInstanceId != SerializableGuid.Empty)
		{
			MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(eventObjectInstanceId, PropLocationType.Emitter);
		}
		if (receiverObjectInstanceId != SerializableGuid.Empty)
		{
			MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(receiverObjectInstanceId, PropLocationType.Receiver);
		}
		if (hoveredTarget != SerializableGuid.Empty)
		{
			MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(hoveredTarget, PropLocationType.Default);
			MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(hoveredTarget, PropLocationType.Selected);
			MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(hoveredTarget, PropLocationType.Receiver);
			MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(hoveredTarget, PropLocationType.Emitter);
		}
		DisableRerouteUndo.Invoke();
		MonoBehaviourSingleton<MarkerManager>.Instance.ReleaseAllMarkers();
		int activeUserId = EndlessServices.Instance.CloudService.ActiveUserId;
		if (temporaryWires.TryGetValue(activeUserId, out var value) && (bool)value)
		{
			if (value.IsBeingEdited)
			{
				value.SetPlaced();
			}
			if ((bool)temporaryWires[activeUserId] && !temporaryWires[activeUserId].IsPlaced)
			{
				UnityEngine.Object.Destroy(temporaryWires[activeUserId].gameObject);
				temporaryWires[activeUserId] = null;
			}
		}
		EmitterSelectionCancelled();
		TogglePhysicalWires(active: false);
		WiringToolExited_ServerRpc(pendingRerouteNodes.ToArray());
		if ((bool)reroutePreview)
		{
			UnityEngine.Object.Destroy(reroutePreview.gameObject);
		}
		SetToolState(WiringToolState.Wiring);
	}

	[ServerRpc(RequireOwnership = false)]
	private void ClearPendingRerouteNodesForPlayer_ServerRpc(SerializableGuid[] rerouteNodes, ServerRpcParams rpcParams = default(ServerRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			FastBufferWriter bufferWriter = __beginSendServerRpc(405623707u, rpcParams, RpcDelivery.Reliable);
			bool value = rerouteNodes != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(rerouteNodes, default(FastBufferWriter.ForNetworkSerializable));
			}
			__endSendServerRpc(ref bufferWriter, 405623707u, rpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			_ = (bool)MonoBehaviourSingleton<StageManager>.Instance.ActiveStage;
		}
	}

	[ServerRpc(RequireOwnership = false)]
	private void WiringToolExited_ServerRpc(SerializableGuid[] rerouteNodes, ServerRpcParams rpcParams = default(ServerRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			FastBufferWriter bufferWriter = __beginSendServerRpc(4103524712u, rpcParams, RpcDelivery.Reliable);
			bool value = rerouteNodes != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(rerouteNodes, default(FastBufferWriter.ForNetworkSerializable));
			}
			__endSendServerRpc(ref bufferWriter, 4103524712u, rpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			int userId = NetworkBehaviourSingleton<UserIdManager>.Instance.GetUserId(rpcParams.Receive.SenderClientId);
			WiringToolExited_ClientRpc(userId);
			if (!base.IsClient)
			{
				ClearTemporaryWireForPlayer(userId);
			}
		}
	}

	[ClientRpc]
	private void WiringToolExited_ClientRpc(int userId)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams = default(ClientRpcParams);
				FastBufferWriter bufferWriter = __beginSendClientRpc(1791207916u, clientRpcParams, RpcDelivery.Reliable);
				BytePacker.WriteValueBitPacked(bufferWriter, userId);
				__endSendClientRpc(ref bufferWriter, 1791207916u, clientRpcParams, RpcDelivery.Reliable);
			}
			if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				__rpc_exec_stage = __RpcExecStage.Send;
				ClearTemporaryWireForPlayer(userId);
			}
		}
	}

	public override void HandleSelected()
	{
		EmitterSelectionCancelled();
		Set3DCursorUsesIntersection(val: true);
		TogglePhysicalWires(active: true);
		SetCellMarkerToEmitterColor();
	}

	private void SetCellMarkerToEmitterColor()
	{
		base.AutoPlace3DCursor = true;
		MonoBehaviourSingleton<CellMarker>.Instance.MoveTo(base.ActiveLineCastResult.NearestPositionToObject);
		MonoBehaviourSingleton<CellMarker>.Instance.SetColor(new Color(0.7f, 0.4f, 0.2f, 1f));
	}

	private void SetCellMarkerToReceiverColor()
	{
		base.AutoPlace3DCursor = true;
		MonoBehaviourSingleton<CellMarker>.Instance.MoveTo(base.ActiveLineCastResult.NearestPositionToObject);
		MonoBehaviourSingleton<CellMarker>.Instance.SetColor(new Color(0.2f, 0.4f, 0.7f, 1f));
	}

	private void TogglePhysicalWires(bool active)
	{
		if ((bool)MonoBehaviourSingleton<StageManager>.Instance.ActiveStage)
		{
			foreach (PhysicalWire physicalWire in MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.PhysicalWires)
			{
				physicalWire.gameObject.SetActive(active);
			}
		}
		foreach (PhysicalWire value in temporaryWires.Values)
		{
			if ((bool)value)
			{
				value.gameObject.SetActive(active);
			}
		}
	}

	public override void UpdateTool()
	{
		base.UpdateTool();
		if (toolState == WiringToolState.Rerouting)
		{
			LineCastHit activeLineCastResult = base.ActiveLineCastResult;
			reroutePreview.transform.position = activeLineCastResult.NearestPositionToObject;
			return;
		}
		WiringStage wiringStage = currentWiringStage;
		if (wiringStage != WiringStage.SelectingEventObject && wiringStage != WiringStage.SelectingReceiverObject)
		{
			return;
		}
		CalculateTarget();
		if (wiringTarget == WiringTarget.Prop)
		{
			PropCell propCell = wiringTargetPropCell;
			if (propCell.InstanceId == hoveredTarget)
			{
				return;
			}
			_ = Color.white;
			bool flag = false;
			if (hoveredTarget == SerializableGuid.Empty || hoveredTarget != propCell.InstanceId)
			{
				if (hoveredTarget != SerializableGuid.Empty && hoveredTarget != propCell.InstanceId)
				{
					MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(hoveredTarget, PropLocationType.NoAction);
					MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(hoveredTarget, PropLocationType.Default);
					if (eventObjectInstanceId != hoveredTarget)
					{
						MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(hoveredTarget, PropLocationType.Emitter);
					}
					if (receiverObjectInstanceId != hoveredTarget)
					{
						MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(hoveredTarget, PropLocationType.Receiver);
					}
				}
				SerializableGuid assetIdFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetAssetIdFromInstanceId(propCell.InstanceId);
				PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(assetIdFromInstanceId);
				switch (currentWiringStage)
				{
				case WiringStage.SelectingEventObject:
				{
					List<EndlessEventInfo> source3 = runtimePropInfo.EndlessProp.ScriptComponent.Script?.Events ?? new List<EndlessEventInfo>();
					IEnumerable<EndlessEventInfo> source4 = runtimePropInfo.GetAllDefinitions().SelectMany((ComponentDefinition definition) => definition.AvailableEvents);
					flag = source3.Any() || source4.Any();
					break;
				}
				case WiringStage.SelectingReceiverObject:
				{
					List<EndlessEventInfo> source = runtimePropInfo.EndlessProp.ScriptComponent.Script?.Receivers ?? new List<EndlessEventInfo>();
					IEnumerable<EndlessEventInfo> source2 = runtimePropInfo.GetAllDefinitions().SelectMany((ComponentDefinition definition) => definition.AvailableReceivers);
					flag = source.Any() || source2.Any();
					break;
				}
				default:
					_ = Color.gray;
					break;
				}
			}
			hoveredTarget = propCell.InstanceId;
			wiringStage = currentWiringStage;
			MonoBehaviourSingleton<PropLocationMarker>.Instance.MarkCellsForProp(hoveredTarget, wiringStage switch
			{
				WiringStage.SelectingEventObject => flag ? PropLocationType.Emitter : PropLocationType.NoAction, 
				WiringStage.SelectingReceiverObject => flag ? PropLocationType.Receiver : PropLocationType.NoAction, 
				_ => (!flag) ? PropLocationType.NoAction : PropLocationType.Default, 
			});
			if ((bool)hoveredWire)
			{
				hoveredWire.Unhovered();
				hoveredWire = null;
			}
			return;
		}
		if (wiringTarget == WiringTarget.PhysicalWire)
		{
			PhysicalWire physicalWire = wiringTargetPhysicalWire;
			if (physicalWire != hoveredWire)
			{
				if ((bool)hoveredWire)
				{
					hoveredWire.Unhovered();
				}
				hoveredWire = physicalWire;
				hoveredWire.Hover();
				base.AutoPlace3DCursor = false;
				MonoBehaviourSingleton<CellMarker>.Instance.MoveTo(new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue));
			}
			else if (hoveredWire != null && physicalWire != hoveredWire)
			{
				Debug.Log("hoveredWire test");
				base.AutoPlace3DCursor = true;
				MonoBehaviourSingleton<CellMarker>.Instance.MoveTo(base.ActiveLineCastResult.NearestPositionToObject);
				hoveredWire.Unhovered();
				hoveredWire = null;
			}
			if (hoveredTarget != SerializableGuid.Empty)
			{
				MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(hoveredTarget, PropLocationType.NoAction);
				MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(hoveredTarget, PropLocationType.Default);
				if (eventObjectInstanceId != hoveredTarget)
				{
					MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(hoveredTarget, PropLocationType.Emitter);
				}
				if (receiverObjectInstanceId != hoveredTarget)
				{
					MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(hoveredTarget, PropLocationType.Receiver);
				}
				hoveredTarget = SerializableGuid.Empty;
			}
			return;
		}
		if (hoveredTarget != SerializableGuid.Empty)
		{
			MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(hoveredTarget, PropLocationType.NoAction);
			MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(hoveredTarget, PropLocationType.Default);
			if (eventObjectInstanceId != hoveredTarget)
			{
				MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(hoveredTarget, PropLocationType.Emitter);
			}
			if (receiverObjectInstanceId != hoveredTarget)
			{
				MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(hoveredTarget, PropLocationType.Receiver);
			}
			hoveredTarget = SerializableGuid.Empty;
		}
		if (hoveredWire != null)
		{
			base.AutoPlace3DCursor = true;
			MonoBehaviourSingleton<CellMarker>.Instance.MoveTo(base.ActiveLineCastResult.NearestPositionToObject);
			hoveredWire.Unhovered();
			hoveredWire = null;
		}
	}

	private void AttemptHighlightWire()
	{
		if (PerformRaycast(out var hit, LayerMask.GetMask("Wires")))
		{
			if (hit.transform.TryGetComponent<PhysicalWire>(out var component))
			{
				if (component != hoveredWire)
				{
					if ((bool)hoveredWire)
					{
						hoveredWire.Unhovered();
					}
					hoveredWire = component;
					hoveredWire.Hover();
				}
			}
			else if ((bool)hoveredWire)
			{
				hoveredWire.Unhovered();
				hoveredWire = null;
			}
		}
		else if ((bool)hoveredWire)
		{
			hoveredWire.Unhovered();
			hoveredWire = null;
		}
	}

	public void SetToolState(WiringToolState wiringToolState)
	{
		toolState = wiringToolState;
		switch (toolState)
		{
		case WiringToolState.Wiring:
			if ((bool)reroutePreview)
			{
				UnityEngine.Object.Destroy(reroutePreview.gameObject);
			}
			break;
		case WiringToolState.Rerouting:
			if ((bool)reroutePreview)
			{
			}
			break;
		default:
			Debug.LogWarningFormat(this, "WiringTool's SetWiringStage method does not support a WiringToolState value of '{0}'", toolState);
			break;
		}
		OnToolStateChanged.Invoke(wiringToolState);
	}

	public override void ToolReleased()
	{
		Debug.LogWarning($"Tool released: {toolState} - {currentWiringStage}");
		if (toolState != WiringToolState.Wiring)
		{
			return;
		}
		WiringStage wiringStage = currentWiringStage;
		RaycastHit hit;
		if (wiringStage == WiringStage.SelectingEventObject || wiringStage == WiringStage.SelectingReceiverObject)
		{
			if (wiringTarget == WiringTarget.PhysicalWire)
			{
				SelectPhysicalWire(wiringTargetPhysicalWire);
			}
			else
			{
				if (wiringTarget != WiringTarget.Prop)
				{
					return;
				}
				PropCell propCell = wiringTargetPropCell;
				SerializableGuid assetIdFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetAssetIdFromInstanceId(propCell.InstanceId);
				MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(assetIdFromInstanceId);
				switch (currentWiringStage)
				{
				case WiringStage.SelectingEventObject:
					eventObjectInstanceId = propCell.InstanceId;
					if (eventObjectInstanceId.IsEmpty)
					{
						Debug.LogWarning("Attempted to send empty event object instance id to server, and this is not allowed!");
						break;
					}
					currentWiringStage = WiringStage.SelectingReceiverObject;
					CollectEmitterEventInfos(assetIdFromInstanceId);
					Debug.LogWarning("Firing event for UI");
					OnEventObjectSelected.Invoke(propCell.CellBase, eventObjectInstanceId, GetUiEmitterLists(assetIdFromInstanceId));
					hoveredTarget = SerializableGuid.Empty;
					SpawnTemporaryWire_ServerRpc(propCell.InstanceId, receiverObjectInstanceId);
					MonoBehaviourSingleton<PropLocationMarker>.Instance.MarkCellsForProp(eventObjectInstanceId, PropLocationType.Emitter);
					SetCellMarkerToReceiverColor();
					break;
				case WiringStage.SelectingReceiverObject:
					if (propCell.InstanceId != eventObjectInstanceId)
					{
						currentWiringStage = WiringStage.AwaitingConfirmation;
						receiverObjectInstanceId = propCell.InstanceId;
						SetTemporaryWireTarget_ServerRpc(receiverObjectInstanceId);
						CollectReceiverEventInfos(assetIdFromInstanceId);
						OnReceiverObjectSelected.Invoke(propCell.CellBase, receiverObjectInstanceId, GetUiReceiverLists(assetIdFromInstanceId));
						base.AutoPlace3DCursor = false;
						MonoBehaviourSingleton<CellMarker>.Instance.MoveTo(new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue));
						MonoBehaviourSingleton<PropLocationMarker>.Instance.MarkCellsForProp(receiverObjectInstanceId, PropLocationType.Receiver);
					}
					break;
				}
			}
		}
		else if (PerformRaycast(out hit, LayerMask.GetMask("Wires")))
		{
			if (hit.transform.TryGetComponent<PhysicalWire>(out var component))
			{
				EmitterSelectionCancelled();
				SelectPhysicalWire(component);
			}
			else
			{
				Debug.LogException(new Exception("Mouse click detected a wire, but was unable to retrieve the physical wire instance."), hit.transform.gameObject);
			}
		}
	}

	private void CalculateTarget()
	{
		bool flag = false;
		float num = float.MaxValue;
		float num2 = float.MaxValue;
		LineCastHit activeLineCastResult = base.ActiveLineCastResult;
		if (PerformRaycast(out var hit, LayerMask.GetMask("Wires")))
		{
			flag = true;
			num = hit.distance;
		}
		if (activeLineCastResult.IntersectionOccured)
		{
			num2 = activeLineCastResult.Distance;
		}
		if (num < num2 && flag)
		{
			if (hit.transform.TryGetComponent<PhysicalWire>(out var component))
			{
				wiringTarget = WiringTarget.PhysicalWire;
				wiringTargetPhysicalWire = component;
				wiringTargetPropCell = null;
			}
			else
			{
				wiringTarget = WiringTarget.None;
				wiringTargetPhysicalWire = null;
				wiringTargetPropCell = null;
			}
		}
		else if (num2 < num && activeLineCastResult.IntersectionOccured)
		{
			if (MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetCellFromCoordinate(activeLineCastResult.IntersectedObjectPosition) is PropCell propCell)
			{
				wiringTarget = WiringTarget.Prop;
				wiringTargetPropCell = propCell;
				wiringTargetPhysicalWire = null;
			}
			else
			{
				wiringTarget = WiringTarget.None;
				wiringTargetPhysicalWire = null;
				wiringTargetPropCell = null;
			}
		}
		else
		{
			wiringTarget = WiringTarget.None;
			wiringTargetPhysicalWire = null;
			wiringTargetPropCell = null;
		}
	}

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
		foreach (ComponentDefinition allDefinition in runtimePropInfo.GetAllDefinitions())
		{
			Type type = allDefinition.ComponentBase.GetType();
			list.Add(new UIEndlessEventList
			{
				DisplayName = type.Name,
				EndlessEventInfos = allDefinition.AvailableReceivers,
				AssemblyQualifiedTypeName = type.AssemblyQualifiedName
			});
		}
		return list;
	}

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
		foreach (ComponentDefinition allDefinition in runtimePropInfo.GetAllDefinitions())
		{
			Type type = allDefinition.ComponentBase.GetType();
			list.Add(new UIEndlessEventList
			{
				DisplayName = type.Name,
				EndlessEventInfos = allDefinition.AvailableEvents,
				AssemblyQualifiedTypeName = type.AssemblyQualifiedName
			});
		}
		return list;
	}

	private void SelectPhysicalWire(PhysicalWire wire)
	{
		eventObjectInstanceId = wire.EmitterId;
		GameObject gameObjectFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(eventObjectInstanceId);
		SerializableGuid assetIdFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetAssetIdFromInstanceId(eventObjectInstanceId);
		CollectEmitterEventInfos(assetIdFromInstanceId);
		receiverObjectInstanceId = wire.ReceiverId;
		GameObject gameObjectFromInstanceId2 = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(receiverObjectInstanceId);
		SerializableGuid assetIdFromInstanceId2 = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetAssetIdFromInstanceId(receiverObjectInstanceId);
		CollectReceiverEventInfos(assetIdFromInstanceId2);
		currentWiringStage = WiringStage.AwaitingConfirmation;
		int activeUserId = EndlessServices.Instance.CloudService.ActiveUserId;
		if ((bool)temporaryWires[activeUserId] && temporaryWires[activeUserId].IsLive)
		{
			UnityEngine.Object.Destroy(temporaryWires[activeUserId].gameObject);
		}
		temporaryWires[activeUserId] = GetPhysicalWireFromId(wire.BundleId);
		SetWireEditedByPlayer_ServerRpc(wire.BundleId);
		OnEventObjectSelected.Invoke(gameObjectFromInstanceId.transform, eventObjectInstanceId, GetUiEmitterLists(assetIdFromInstanceId));
		OnReceiverObjectSelected.Invoke(gameObjectFromInstanceId2.transform, receiverObjectInstanceId, GetUiReceiverLists(assetIdFromInstanceId2));
		MonoBehaviourSingleton<PropLocationMarker>.Instance.MarkCellsForProp(eventObjectInstanceId, PropLocationType.Emitter);
		MonoBehaviourSingleton<PropLocationMarker>.Instance.MarkCellsForProp(receiverObjectInstanceId, PropLocationType.Receiver);
	}

	private PhysicalWire GetPhysicalWireFromId(SerializableGuid wiringId)
	{
		PhysicalWire result = null;
		List<PhysicalWire> physicalWires = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.PhysicalWires;
		for (int i = 0; i < physicalWires.Count; i++)
		{
			if (physicalWires[i].BundleId == wiringId)
			{
				result = physicalWires[i];
				break;
			}
		}
		return result;
	}

	[ServerRpc(RequireOwnership = false)]
	private void SetTemporaryWireTargetToPlayer_ServerRpc(ServerRpcParams serverParams = default(ServerRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			FastBufferWriter bufferWriter = __beginSendServerRpc(354023136u, serverParams, RpcDelivery.Reliable);
			__endSendServerRpc(ref bufferWriter, 354023136u, serverParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
		{
			return;
		}
		__rpc_exec_stage = __RpcExecStage.Send;
		ulong? num = NetworkManager.Singleton.ConnectedClients[serverParams.Receive.SenderClientId].OwnedObjects.FirstOrDefault((NetworkObject networkObject) => networkObject.GetComponent<PlayerController>() != null)?.NetworkObjectId;
		if (num.HasValue)
		{
			int userId = NetworkBehaviourSingleton<UserIdManager>.Instance.GetUserId(serverParams.Receive.SenderClientId);
			SetTemporaryWireTargetToPlayer_ClientRpc(userId, num.Value);
			if (!base.IsClient)
			{
				SetTemporaryWireTargetToPlayer(userId, num.Value);
			}
		}
	}

	[ClientRpc]
	private void SetTemporaryWireTargetToPlayer_ClientRpc(int userId, ulong networkObjectId)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(1104155854u, clientRpcParams, RpcDelivery.Reliable);
			BytePacker.WriteValueBitPacked(bufferWriter, userId);
			BytePacker.WriteValueBitPacked(bufferWriter, networkObjectId);
			__endSendClientRpc(ref bufferWriter, 1104155854u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if (NetworkBehaviourSingleton<CreatorManager>.Instance.RpcReceiveState == RpcReceiveState.Allow)
			{
				SetTemporaryWireTargetToPlayer(userId, networkObjectId);
			}
		}
	}

	private void SetTemporaryWireTargetToPlayer(int userId, ulong networkObjectId)
	{
		Transform transform = base.NetworkManager.SpawnManager.SpawnedObjects[networkObjectId]?.transform;
		if (!(transform == null))
		{
			transform = transform.GetComponent<PlayerReferenceManager>().ApperanceController.transform;
			if (temporaryWires.ContainsKey(userId) && (bool)temporaryWires[userId])
			{
				temporaryWires[userId].SetReceiverToPlayer(transform);
			}
		}
	}

	[ServerRpc(RequireOwnership = false)]
	private void SetTemporaryWireTarget_ServerRpc(SerializableGuid target, ServerRpcParams serverParams = default(ServerRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			FastBufferWriter bufferWriter = __beginSendServerRpc(3716040516u, serverParams, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in target, default(FastBufferWriter.ForNetworkSerializable));
			__endSendServerRpc(ref bufferWriter, 3716040516u, serverParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			int userId = NetworkBehaviourSingleton<UserIdManager>.Instance.GetUserId(serverParams.Receive.SenderClientId);
			SetTemporaryWireTarget_ClientRpc(target, userId);
			if (!base.IsClient)
			{
				SetTemporaryWireTarget(target, userId);
			}
		}
	}

	[ClientRpc]
	private void SetTemporaryWireTarget_ClientRpc(SerializableGuid target, int userId)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(4269480805u, clientRpcParams, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in target, default(FastBufferWriter.ForNetworkSerializable));
			BytePacker.WriteValueBitPacked(bufferWriter, userId);
			__endSendClientRpc(ref bufferWriter, 4269480805u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if (NetworkBehaviourSingleton<CreatorManager>.Instance.RpcReceiveState == RpcReceiveState.Allow)
			{
				SetTemporaryWireTarget(target, userId);
			}
		}
	}

	private void SetTemporaryWireTarget(SerializableGuid target, int userId)
	{
		GameObject gameObjectFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(target);
		if (!temporaryWires.ContainsKey(userId))
		{
			return;
		}
		if (!temporaryWires[userId])
		{
			Debug.LogWarning(string.Format("Temporary wire for Player Id: {0} is null, but shouldn't be because we're in {1}", userId, "SetTemporaryWireTarget"));
		}
		if (!gameObjectFromInstanceId)
		{
			Debug.LogWarning(string.Format("Target Object for wire target for Player Id: {0} is null, but shouldn't be because we're in {1}", userId, "SetTemporaryWireTarget"));
		}
		if (!temporaryWires[userId])
		{
			Debug.LogWarning($"Temporary wire for Player Id: {userId} is null, this is possible if they joined late and never got the temp wire spawn.");
			return;
		}
		temporaryWires[userId].SetTemporaryTarget(gameObjectFromInstanceId.transform, target, temporaryWires[userId].ReceiverTargetPosition(), delegate
		{
			foreach (WireBundle wireBundle in MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.WireBundles)
			{
				if (wireBundle.EmitterInstanceId == temporaryWires[userId].EmitterId && wireBundle.ReceiverInstanceId == temporaryWires[userId].ReceiverId)
				{
					ClearTemporaryWireForPlayer_ServerRpc();
					SetWireEditedByPlayer_ServerRpc(wireBundle.BundleId);
					break;
				}
			}
		}, MonoBehaviourSingleton<ToolManager>.Instance.ActiveTool is WiringTool);
	}

	[ServerRpc(RequireOwnership = false)]
	internal void SetWireEditedByPlayer_ServerRpc(SerializableGuid wireId, ServerRpcParams serverParams = default(ServerRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			FastBufferWriter bufferWriter = __beginSendServerRpc(3772252676u, serverParams, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in wireId, default(FastBufferWriter.ForNetworkSerializable));
			__endSendServerRpc(ref bufferWriter, 3772252676u, serverParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			int userId = NetworkBehaviourSingleton<UserIdManager>.Instance.GetUserId(serverParams.Receive.SenderClientId);
			SetWireEditedByPlayer_ClientRpc(wireId, userId);
			if (!base.IsClient)
			{
				SetWireEditedByPlayer(wireId, userId);
			}
		}
	}

	[ClientRpc]
	private void SetWireEditedByPlayer_ClientRpc(SerializableGuid wireId, int userId, ClientRpcParams rpcParams = default(ClientRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			FastBufferWriter bufferWriter = __beginSendClientRpc(1404899374u, rpcParams, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in wireId, default(FastBufferWriter.ForNetworkSerializable));
			BytePacker.WriteValueBitPacked(bufferWriter, userId);
			__endSendClientRpc(ref bufferWriter, 1404899374u, rpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
		{
			return;
		}
		__rpc_exec_stage = __RpcExecStage.Send;
		SerializableGuid wireId2 = wireId;
		int userId2 = userId;
		if (NetworkBehaviourSingleton<CreatorManager>.Instance.RpcReceiveState == RpcReceiveState.Allow)
		{
			SetWireEditedByPlayer(wireId2, userId2);
			return;
		}
		NetworkBehaviourSingleton<CreatorManager>.Instance.AddCachedRpc(delegate
		{
			SetWireEditedByPlayer(wireId2, userId2);
		});
	}

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
		if ((bool)physicalWire)
		{
			physicalWire.SetEditing();
			if (!temporaryWires.ContainsKey(userId))
			{
				temporaryWires.Add(userId, null);
			}
			if ((bool)temporaryWires[userId] && temporaryWires[userId].IsLive)
			{
				UnityEngine.Object.Destroy(temporaryWires[userId].gameObject);
			}
			temporaryWires[userId] = physicalWire;
			int activeUserId = EndlessServices.Instance.CloudService.ActiveUserId;
			if (userId == activeUserId && physicalWire.RerouteNodeIds.Count > 0)
			{
				EnableRerouteUndo.Invoke();
			}
		}
	}

	[ServerRpc(RequireOwnership = false)]
	private void SpawnTemporaryWire_ServerRpc(SerializableGuid source, SerializableGuid target, ServerRpcParams serverParams = default(ServerRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			FastBufferWriter bufferWriter = __beginSendServerRpc(90801435u, serverParams, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in source, default(FastBufferWriter.ForNetworkSerializable));
			bufferWriter.WriteValueSafe(in target, default(FastBufferWriter.ForNetworkSerializable));
			__endSendServerRpc(ref bufferWriter, 90801435u, serverParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
		{
			return;
		}
		__rpc_exec_stage = __RpcExecStage.Send;
		ulong? num = NetworkManager.Singleton.ConnectedClients[serverParams.Receive.SenderClientId].OwnedObjects.FirstOrDefault((NetworkObject networkObject) => networkObject.GetComponent<PlayerController>() != null)?.NetworkObjectId;
		if (num.HasValue)
		{
			int userId = NetworkBehaviourSingleton<UserIdManager>.Instance.GetUserId(serverParams.Receive.SenderClientId);
			SpawnTemporaryWire_ClientRpc(source, userId, num.Value, target);
			if (!base.IsClient)
			{
				SpawnTemporaryWire(source, userId, num.Value, target);
			}
		}
		else
		{
			Debug.LogException(new Exception("Attempted to get the network object id for player controller but it was unavailable."));
		}
	}

	[ClientRpc]
	private void SpawnTemporaryWire_ClientRpc(SerializableGuid source, int targetUserId, ulong networkObjectId, SerializableGuid target, ClientRpcParams clientRpcParams = default(ClientRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			FastBufferWriter bufferWriter = __beginSendClientRpc(3985273964u, clientRpcParams, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in source, default(FastBufferWriter.ForNetworkSerializable));
			BytePacker.WriteValueBitPacked(bufferWriter, targetUserId);
			BytePacker.WriteValueBitPacked(bufferWriter, networkObjectId);
			bufferWriter.WriteValueSafe(in target, default(FastBufferWriter.ForNetworkSerializable));
			__endSendClientRpc(ref bufferWriter, 3985273964u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
		{
			return;
		}
		__rpc_exec_stage = __RpcExecStage.Send;
		SerializableGuid source2 = source;
		int targetUserId2 = targetUserId;
		ulong networkObjectId2 = networkObjectId;
		SerializableGuid target2 = target;
		if (NetworkBehaviourSingleton<CreatorManager>.Instance.RpcReceiveState == RpcReceiveState.Allow)
		{
			SpawnTemporaryWire(source2, targetUserId2, networkObjectId2, target2);
			return;
		}
		NetworkBehaviourSingleton<CreatorManager>.Instance.AddCachedRpc(delegate
		{
			SpawnTemporaryWire(source2, targetUserId2, networkObjectId2, target2);
		});
	}

	private void SpawnTemporaryWire(SerializableGuid source, int targetUserId, ulong networkObjectId, SerializableGuid target)
	{
		if (!temporaryWires.ContainsKey(targetUserId))
		{
			temporaryWires.Add(targetUserId, null);
		}
		if ((bool)temporaryWires[targetUserId])
		{
			ClearTemporaryWireForPlayer(targetUserId);
		}
		Transform connectionPointFromObject = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetConnectionPointFromObject(MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(source), source, ConnectionPoint.Emitter);
		Transform transform = ((!target.IsEmpty) ? MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(target).transform : base.NetworkManager.SpawnManager.SpawnedObjects[networkObjectId]?.transform);
		if ((object)transform != null)
		{
			Transform transform2 = base.NetworkManager.SpawnManager.SpawnedObjects[networkObjectId]?.transform;
			PhysicalWire physicalWire = UnityEngine.Object.Instantiate(physicalWireTemplate);
			physicalWire.SetupLiveMode(source, connectionPointFromObject, transform2.position, transform, MonoBehaviourSingleton<ToolManager>.Instance.ActiveTool is WiringTool);
			physicalWire.gameObject.SetActive(MonoBehaviourSingleton<ToolManager>.Instance.ActiveTool is WiringTool);
			temporaryWires[targetUserId] = physicalWire;
		}
	}

	[ServerRpc(RequireOwnership = false)]
	private void AddRerouteNodeToPosition_ServerRpc(Vector3Int coordinate, ServerRpcParams serverParams = default(ServerRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter bufferWriter = __beginSendServerRpc(359986616u, serverParams, RpcDelivery.Reliable);
				bufferWriter.WriteValueSafe(in coordinate);
				__endSendServerRpc(ref bufferWriter, 359986616u, serverParams, RpcDelivery.Reliable);
			}
			if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				__rpc_exec_stage = __RpcExecStage.Send;
			}
		}
	}

	private async void AddRerouteNodeToPosition(Vector3Int coordinate, ServerRpcParams serverParams)
	{
	}

	[ClientRpc]
	private void PlaceRerouteNode_ClientRPC(Vector3Int coordinate, SerializableGuid assetId, SerializableGuid instanceId, ulong networkObjectId, ulong targetPlayerId)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams = default(ClientRpcParams);
				FastBufferWriter bufferWriter = __beginSendClientRpc(101785308u, clientRpcParams, RpcDelivery.Reliable);
				bufferWriter.WriteValueSafe(in coordinate);
				bufferWriter.WriteValueSafe(in assetId, default(FastBufferWriter.ForNetworkSerializable));
				bufferWriter.WriteValueSafe(in instanceId, default(FastBufferWriter.ForNetworkSerializable));
				BytePacker.WriteValueBitPacked(bufferWriter, networkObjectId);
				BytePacker.WriteValueBitPacked(bufferWriter, targetPlayerId);
				__endSendClientRpc(ref bufferWriter, 101785308u, clientRpcParams, RpcDelivery.Reliable);
			}
			if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				__rpc_exec_stage = __RpcExecStage.Send;
			}
		}
	}

	private void PlaceRerouteNode(Vector3Int coordinate, SerializableGuid assetId, SerializableGuid instanceId, ulong networkObjectId, ulong targetPlayerId)
	{
	}

	private void CollectReceiverEventInfos(SerializableGuid assetId)
	{
		receiverEventInfos.Clear();
		PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(assetId);
		List<EndlessEventInfo> first = runtimePropInfo.EndlessProp.ScriptComponent.Script?.Receivers ?? new List<EndlessEventInfo>();
		IEnumerable<EndlessEventInfo> second = runtimePropInfo.GetAllDefinitions().SelectMany((ComponentDefinition definition) => definition.AvailableReceivers);
		emitterEventInfos = first.Concat(second).ToList();
	}

	private void CollectEmitterEventInfos(SerializableGuid assetId)
	{
		emitterEventInfos.Clear();
		PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(assetId);
		List<EndlessEventInfo> first = runtimePropInfo.EndlessProp.ScriptComponent.Script?.Events ?? new List<EndlessEventInfo>();
		IEnumerable<EndlessEventInfo> second = runtimePropInfo.GetAllDefinitions().SelectMany((ComponentDefinition definition) => definition.AvailableEvents);
		emitterEventInfos = first.Concat(second).ToList();
	}

	public void ReceiverSelectionCancelled(bool closing = false)
	{
		SetCellMarkerToReceiverColor();
		MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(receiverObjectInstanceId, PropLocationType.Receiver);
		receiverObjectInstanceId = SerializableGuid.Empty;
		currentWiringStage = WiringStage.SelectingReceiverObject;
		if (!NetworkBehaviourSingleton<UserIdManager>.Instance.TryGetUserId(NetworkManager.Singleton.LocalClientId, out var userId) || !temporaryWires.ContainsKey(userId))
		{
			return;
		}
		if ((bool)temporaryWires[userId] && temporaryWires[userId].IsLive)
		{
			SetTemporaryWireTargetToPlayer_ServerRpc();
		}
		else if ((!temporaryWires[userId] || !temporaryWires[userId].IsLive) && !closing)
		{
			if (eventObjectInstanceId.IsEmpty)
			{
				Debug.LogWarning("Attempted to call server RPC with empty emitter instance id. This shouldn't be possible (but has occurred in cases where the game is in an errored state)");
			}
			else
			{
				SpawnTemporaryWire_ServerRpc(eventObjectInstanceId, SerializableGuid.Empty);
			}
		}
	}

	public void EmitterSelectionCancelled()
	{
		currentWiringStage = WiringStage.SelectingEventObject;
		SetCellMarkerToEmitterColor();
		MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(eventObjectInstanceId, PropLocationType.Emitter);
		if (!(eventObjectInstanceId == SerializableGuid.Empty))
		{
			eventObjectInstanceId = SerializableGuid.Empty;
			receiverObjectInstanceId = SerializableGuid.Empty;
			ClearTemporaryWireForPlayer_ServerRpc();
			pendingRerouteNodes.Clear();
		}
	}

	[ServerRpc(RequireOwnership = false)]
	private void ClearTemporaryWireForPlayer_ServerRpc(ServerRpcParams serverParams = default(ServerRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			FastBufferWriter bufferWriter = __beginSendServerRpc(3636364046u, serverParams, RpcDelivery.Reliable);
			__endSendServerRpc(ref bufferWriter, 3636364046u, serverParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			int userId = NetworkBehaviourSingleton<UserIdManager>.Instance.GetUserId(serverParams.Receive.SenderClientId);
			ClearTemporaryWireForPlayer_ClientRpc(userId);
			if (!base.IsClient)
			{
				ClearTemporaryWireForPlayer(userId);
			}
		}
	}

	[ClientRpc]
	private void ClearTemporaryWireForPlayer_ClientRpc(int userId)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(3434663878u, clientRpcParams, RpcDelivery.Reliable);
			BytePacker.WriteValueBitPacked(bufferWriter, userId);
			__endSendClientRpc(ref bufferWriter, 3434663878u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
		{
			return;
		}
		__rpc_exec_stage = __RpcExecStage.Send;
		int userId2 = userId;
		if (NetworkBehaviourSingleton<CreatorManager>.Instance.RpcReceiveState == RpcReceiveState.Allow)
		{
			ClearTemporaryWireForPlayer(userId2);
			return;
		}
		NetworkBehaviourSingleton<CreatorManager>.Instance.AddCachedRpc(delegate
		{
			ClearTemporaryWireForPlayer(userId2);
		});
	}

	private void ClearTemporaryWireForPlayer(int userId)
	{
		if (temporaryWires.ContainsKey(userId) && (bool)temporaryWires[userId])
		{
			if (!temporaryWires[userId].IsLive)
			{
				temporaryWires[userId].SetPlaced();
				temporaryWires[userId] = null;
			}
			else
			{
				UnityEngine.Object.Destroy(temporaryWires[userId].gameObject);
				temporaryWires[userId] = null;
			}
		}
	}

	public void EventConfirmed(EndlessEventInfo emitterEventInfo, string emitterTypeName, EndlessEventInfo receiverInfo, string receiverTypeName, string[] storedParameterValues, WireColor wireColor)
	{
		for (int i = 0; i < storedParameterValues.Length; i++)
		{
			Debug.Log(storedParameterValues[i]);
		}
		AttemptWireTogether_ServerRpc(eventObjectInstanceId, emitterEventInfo.MemberName, emitterTypeName, receiverObjectInstanceId, receiverInfo.MemberName, receiverTypeName, storedParameterValues, pendingRerouteNodes.ToArray(), wireColor);
	}

	public void UpdateWire(SerializableGuid wireId, SerializableGuid emitterInstanceId, string emitterMemberName, string emitterTypeName, SerializableGuid receiverInstanceId, string receiverMemberName, string receiverTypeName, StringArraySerialized storedParameterValues, SerializableGuid[] pendingRerouteNodes, WireColor wireColor)
	{
		SerializableGuid bundleId = SerializableGuid.Empty;
		int num = 0;
		while (bundleId.IsEmpty && num < MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.WireBundles.Count)
		{
			foreach (WireEntry wire in MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.WireBundles[num].Wires)
			{
				if (wire.WireId == wireId)
				{
					bundleId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.WireBundles[num].BundleId;
					break;
				}
			}
			num++;
		}
		if (!bundleId.IsEmpty)
		{
			UpdateWireDetails_ServerRpc(bundleId, wireId, emitterInstanceId, emitterMemberName, emitterTypeName, receiverInstanceId, receiverMemberName, receiverTypeName, storedParameterValues, pendingRerouteNodes, wireColor);
		}
	}

	[ServerRpc(RequireOwnership = false)]
	private void UpdateWireDetails_ServerRpc(SerializableGuid bundleId, SerializableGuid wireId, SerializableGuid emitterInstanceId, string emitterMemberName, string emitterTypeName, SerializableGuid receiverInstanceId, string receiverMemberName, string receiverTypeName, StringArraySerialized storedParameterValues, SerializableGuid[] pendingRerouteNodes, WireColor wireColor, ServerRpcParams serverRpcParams = default(ServerRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			FastBufferWriter bufferWriter = __beginSendServerRpc(328729018u, serverRpcParams, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in bundleId, default(FastBufferWriter.ForNetworkSerializable));
			bufferWriter.WriteValueSafe(in wireId, default(FastBufferWriter.ForNetworkSerializable));
			bufferWriter.WriteValueSafe(in emitterInstanceId, default(FastBufferWriter.ForNetworkSerializable));
			bool value = emitterMemberName != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(emitterMemberName);
			}
			bool value2 = emitterTypeName != null;
			bufferWriter.WriteValueSafe(in value2, default(FastBufferWriter.ForPrimitives));
			if (value2)
			{
				bufferWriter.WriteValueSafe(emitterTypeName);
			}
			bufferWriter.WriteValueSafe(in receiverInstanceId, default(FastBufferWriter.ForNetworkSerializable));
			bool value3 = receiverMemberName != null;
			bufferWriter.WriteValueSafe(in value3, default(FastBufferWriter.ForPrimitives));
			if (value3)
			{
				bufferWriter.WriteValueSafe(receiverMemberName);
			}
			bool value4 = receiverTypeName != null;
			bufferWriter.WriteValueSafe(in value4, default(FastBufferWriter.ForPrimitives));
			if (value4)
			{
				bufferWriter.WriteValueSafe(receiverTypeName);
			}
			bufferWriter.WriteValueSafe(in storedParameterValues, default(FastBufferWriter.ForNetworkSerializable));
			bool value5 = pendingRerouteNodes != null;
			bufferWriter.WriteValueSafe(in value5, default(FastBufferWriter.ForPrimitives));
			if (value5)
			{
				bufferWriter.WriteValueSafe(pendingRerouteNodes, default(FastBufferWriter.ForNetworkSerializable));
			}
			bufferWriter.WriteValueSafe(in wireColor, default(FastBufferWriter.ForEnums));
			__endSendServerRpc(ref bufferWriter, 328729018u, serverRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			UpdateWireDetails(bundleId, wireId, emitterInstanceId, emitterMemberName, emitterTypeName, receiverInstanceId, receiverMemberName, receiverTypeName, storedParameterValues, pendingRerouteNodes, wireColor, serverRpcParams);
		}
	}

	private async void UpdateWireDetails(SerializableGuid bundleId, SerializableGuid wireId, SerializableGuid emitterInstanceId, string emitterMemberName, string emitterTypeName, SerializableGuid receiverInstanceId, string receiverMemberName, string receiverTypeName, StringArraySerialized storedParameterValues, SerializableGuid[] pendingRerouteNodes, WireColor wireColor, ServerRpcParams serverRpcParams)
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
			UpdateWireDetails_ClientRpc(bundleId, wireId, emitterInstanceId, emitterMemberName, emitterTypeName, receiverInstanceId, receiverMemberName, receiverTypeName, storedParameterValues, pendingRerouteNodes, wireColor);
			if (!base.IsClient)
			{
				UpdateWireDetails(bundleId, wireId, emitterInstanceId, emitterMemberName, emitterTypeName, receiverInstanceId, receiverMemberName, receiverTypeName, storedParameterValues, pendingRerouteNodes, wireColor);
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

	[ClientRpc]
	private void UpdateWireDetails_ClientRpc(SerializableGuid bundleId, SerializableGuid wireId, SerializableGuid emitterInstanceId, string emitterMemberName, string emitterTypeName, SerializableGuid receiverInstanceId, string receiverMemberName, string receiverTypeName, StringArraySerialized storedParameterValues, SerializableGuid[] pendingRerouteNodes, WireColor wireColor)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(1095343870u, clientRpcParams, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in bundleId, default(FastBufferWriter.ForNetworkSerializable));
			bufferWriter.WriteValueSafe(in wireId, default(FastBufferWriter.ForNetworkSerializable));
			bufferWriter.WriteValueSafe(in emitterInstanceId, default(FastBufferWriter.ForNetworkSerializable));
			bool value = emitterMemberName != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(emitterMemberName);
			}
			bool value2 = emitterTypeName != null;
			bufferWriter.WriteValueSafe(in value2, default(FastBufferWriter.ForPrimitives));
			if (value2)
			{
				bufferWriter.WriteValueSafe(emitterTypeName);
			}
			bufferWriter.WriteValueSafe(in receiverInstanceId, default(FastBufferWriter.ForNetworkSerializable));
			bool value3 = receiverMemberName != null;
			bufferWriter.WriteValueSafe(in value3, default(FastBufferWriter.ForPrimitives));
			if (value3)
			{
				bufferWriter.WriteValueSafe(receiverMemberName);
			}
			bool value4 = receiverTypeName != null;
			bufferWriter.WriteValueSafe(in value4, default(FastBufferWriter.ForPrimitives));
			if (value4)
			{
				bufferWriter.WriteValueSafe(receiverTypeName);
			}
			bufferWriter.WriteValueSafe(in storedParameterValues, default(FastBufferWriter.ForNetworkSerializable));
			bool value5 = pendingRerouteNodes != null;
			bufferWriter.WriteValueSafe(in value5, default(FastBufferWriter.ForPrimitives));
			if (value5)
			{
				bufferWriter.WriteValueSafe(pendingRerouteNodes, default(FastBufferWriter.ForNetworkSerializable));
			}
			bufferWriter.WriteValueSafe(in wireColor, default(FastBufferWriter.ForEnums));
			__endSendClientRpc(ref bufferWriter, 1095343870u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
		{
			return;
		}
		__rpc_exec_stage = __RpcExecStage.Send;
		SerializableGuid bundleId2 = bundleId;
		SerializableGuid wireId2 = wireId;
		SerializableGuid emitterInstanceId2 = emitterInstanceId;
		string emitterMemberName2 = emitterMemberName;
		string emitterTypeName2 = emitterTypeName;
		SerializableGuid receiverInstanceId2 = receiverInstanceId;
		string receiverMemberName2 = receiverMemberName;
		string receiverTypeName2 = receiverTypeName;
		StringArraySerialized storedParameterValues2 = storedParameterValues;
		SerializableGuid[] pendingRerouteNodes2 = pendingRerouteNodes;
		WireColor wireColor2 = wireColor;
		if (NetworkBehaviourSingleton<CreatorManager>.Instance.RpcReceiveState == RpcReceiveState.Allow)
		{
			UpdateWireDetails(bundleId2, wireId2, emitterInstanceId2, emitterMemberName2, emitterTypeName2, receiverInstanceId2, receiverMemberName2, receiverTypeName2, storedParameterValues2, pendingRerouteNodes2, wireColor2);
			return;
		}
		NetworkBehaviourSingleton<CreatorManager>.Instance.AddCachedRpc(delegate
		{
			UpdateWireDetails(bundleId2, wireId2, emitterInstanceId2, emitterMemberName2, emitterTypeName2, receiverInstanceId2, receiverMemberName2, receiverTypeName2, storedParameterValues2, pendingRerouteNodes2, wireColor2);
		});
	}

	private void UpdateWireDetails(SerializableGuid bundleId, SerializableGuid wireId, SerializableGuid emitterInstanceId, string emitterMemberName, string emitterTypeName, SerializableGuid receiverInstanceId, string receiverMemberName, string receiverTypeName, string[] storedParameterValues, SerializableGuid[] pendingRerouteNodes, WireColor wireColor)
	{
		PhysicalWire physicalWireFromId = GetPhysicalWireFromId(bundleId);
		ApplyWiring(bundleId, wireId, emitterInstanceId, emitterMemberName, emitterTypeName, receiverInstanceId, receiverMemberName, receiverTypeName, storedParameterValues, physicalWireFromId.RerouteNodeIds, wireColor);
	}

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
			endlessEventInfo = ((!emitter) ? runtimePropInfo.EndlessProp.ScriptComponent.Script.Receivers.FirstOrDefault((EndlessEventInfo eventInfo) => eventInfo.MemberName == memberName) : runtimePropInfo.EndlessProp.ScriptComponent.Script.Events.FirstOrDefault((EndlessEventInfo eventInfo) => eventInfo.MemberName == memberName));
		}
		else
		{
			ComponentDefinition componentDefinition = runtimePropInfo.GetAllDefinitions().FirstOrDefault((ComponentDefinition defintion) => defintion.ComponentBase.GetType().AssemblyQualifiedName == typeName);
			if ((bool)componentDefinition)
			{
				endlessEventInfo = ((!emitter) ? componentDefinition.AvailableReceivers.FirstOrDefault((EndlessEventInfo eventInfo) => eventInfo.MemberName == memberName) : componentDefinition.AvailableEvents.FirstOrDefault((EndlessEventInfo eventInfo) => eventInfo.MemberName == memberName));
			}
		}
		if (endlessEventInfo == null)
		{
			Debug.LogError($"Failed to find {emitter} {memberName} ({typeName})");
		}
		return endlessEventInfo;
	}

	[ServerRpc(RequireOwnership = false)]
	private void AttemptWireTogether_ServerRpc(SerializableGuid emitterInstanceId, string emitterMemberName, string emitterTypeName, SerializableGuid receiverInstanceId, string receiverMemberName, string receiverTypeName, StringArraySerialized storedParameterValues, SerializableGuid[] pendingRerouteNodes, WireColor wireColor, ServerRpcParams rpcParams = default(ServerRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			FastBufferWriter bufferWriter = __beginSendServerRpc(1366945158u, rpcParams, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in emitterInstanceId, default(FastBufferWriter.ForNetworkSerializable));
			bool value = emitterMemberName != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(emitterMemberName);
			}
			bool value2 = emitterTypeName != null;
			bufferWriter.WriteValueSafe(in value2, default(FastBufferWriter.ForPrimitives));
			if (value2)
			{
				bufferWriter.WriteValueSafe(emitterTypeName);
			}
			bufferWriter.WriteValueSafe(in receiverInstanceId, default(FastBufferWriter.ForNetworkSerializable));
			bool value3 = receiverMemberName != null;
			bufferWriter.WriteValueSafe(in value3, default(FastBufferWriter.ForPrimitives));
			if (value3)
			{
				bufferWriter.WriteValueSafe(receiverMemberName);
			}
			bool value4 = receiverTypeName != null;
			bufferWriter.WriteValueSafe(in value4, default(FastBufferWriter.ForPrimitives));
			if (value4)
			{
				bufferWriter.WriteValueSafe(receiverTypeName);
			}
			bufferWriter.WriteValueSafe(in storedParameterValues, default(FastBufferWriter.ForNetworkSerializable));
			bool value5 = pendingRerouteNodes != null;
			bufferWriter.WriteValueSafe(in value5, default(FastBufferWriter.ForPrimitives));
			if (value5)
			{
				bufferWriter.WriteValueSafe(pendingRerouteNodes, default(FastBufferWriter.ForNetworkSerializable));
			}
			bufferWriter.WriteValueSafe(in wireColor, default(FastBufferWriter.ForEnums));
			__endSendServerRpc(ref bufferWriter, 1366945158u, rpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			AttemptWireTogether(emitterInstanceId, emitterMemberName, emitterTypeName, receiverInstanceId, receiverMemberName, receiverTypeName, storedParameterValues, pendingRerouteNodes, wireColor, rpcParams);
		}
	}

	private async void AttemptWireTogether(SerializableGuid emitterInstanceId, string emitterMemberName, string emitterTypeName, SerializableGuid receiverInstanceId, string receiverMemberName, string receiverTypeName, StringArraySerialized storedParameterValues, SerializableGuid[] pendingRerouteNodes, WireColor wireColor, ServerRpcParams rpcParams)
	{
		if (!base.IsServer)
		{
			return;
		}
		if (!NetworkBehaviourSingleton<UserIdManager>.Instance.TryGetUserId(rpcParams.Receive.SenderClientId, out var userId))
		{
			Debug.LogException(new Exception($"Unable to determine User Id for Client Id: {rpcParams.Receive.SenderClientId}"));
		}
		if (!(await NetworkBehaviourSingleton<CreatorManager>.Instance.UserCanEditLevel(userId)))
		{
			return;
		}
		EndlessEventInfo eventInfo = GetEventInfo(emitterInstanceId, emitterTypeName, emitterMemberName, emitter: true);
		EndlessEventInfo eventInfo2 = GetEventInfo(receiverInstanceId, receiverTypeName, receiverMemberName, emitter: false);
		if (eventInfo != null && eventInfo2 != null)
		{
			if (eventInfo2.ParamList.Count != storedParameterValues.value?.Length)
			{
				string[] value = storedParameterValues.value;
				if (value == null || value.Length != 0)
				{
					goto IL_016f;
				}
			}
			SerializableGuid wireId = SerializableGuid.Empty;
			WireBundle bundleUsingInstances = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.GetBundleUsingInstances(emitterInstanceId, receiverInstanceId);
			SerializableGuid bundleId = bundleUsingInstances?.BundleId ?? SerializableGuid.Empty;
			if (bundleId.IsEmpty)
			{
				if (bundleUsingInstances != null)
				{
					bundleId = bundleUsingInstances.BundleId;
				}
				else
				{
					bundleId = SerializableGuid.NewGuid();
					wireId = SerializableGuid.NewGuid();
				}
			}
			else
			{
				foreach (WireEntry wire in bundleUsingInstances.Wires)
				{
					if (wire.EmitterComponentAssemblyQualifiedTypeName == emitterTypeName && wire.ReceiverComponentAssemblyQualifiedTypeName == receiverTypeName && wire.EmitterMemberName == emitterMemberName && wire.ReceiverMemberName == receiverMemberName)
					{
						wireId = wire.WireId;
						break;
					}
				}
				if (wireId.IsEmpty)
				{
					wireId = SerializableGuid.NewGuid();
				}
			}
			try
			{
				SerializableGuid assetIdFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetAssetIdFromInstanceId(emitterInstanceId);
				PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(assetIdFromInstanceId);
				SerializableGuid assetIdFromInstanceId2 = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetAssetIdFromInstanceId(receiverInstanceId);
				PropLibrary.RuntimePropInfo runtimePropInfo2 = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(assetIdFromInstanceId2);
				CustomEvent e = new CustomEvent("wireCreated")
				{
					{
						"propName1",
						runtimePropInfo.PropData.Name
					},
					{
						"propId1",
						assetIdFromInstanceId.ToString()
					},
					{
						"propName2",
						runtimePropInfo2.PropData.Name
					},
					{
						"assetId2",
						assetIdFromInstanceId2.ToString()
					}
				};
				AnalyticsService.Instance.RecordEvent(e);
			}
			catch (Exception innerException)
			{
				Debug.LogException(new Exception("Analytics error with wiring", innerException));
			}
			int userId2 = NetworkBehaviourSingleton<UserIdManager>.Instance.GetUserId(rpcParams.Receive.SenderClientId);
			WireTogether_ClientRpc(userId2, bundleId, wireId, emitterInstanceId, emitterMemberName, emitterTypeName, receiverInstanceId, receiverMemberName, receiverTypeName, storedParameterValues, pendingRerouteNodes, wireColor);
			if (!base.IsClient)
			{
				ApplyWiring(bundleId, wireId, emitterInstanceId, emitterMemberName, emitterTypeName, receiverInstanceId, receiverMemberName, receiverTypeName, storedParameterValues, pendingRerouteNodes, wireColor);
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
		goto IL_016f;
		IL_016f:
		Debug.LogError($"emitter: {eventInfo}, receiver: {eventInfo2}");
		Debug.LogError($"receiver params {eventInfo2.ParamList.Count} stored params {storedParameterValues.value?.Length}");
	}

	[ClientRpc]
	private void WireTogether_ClientRpc(int userId, SerializableGuid bundleId, SerializableGuid wireId, SerializableGuid emitterInstanceId, string emitterMemberName, string emitterTypeName, SerializableGuid receiverInstanceId, string receiverMemberName, string receiverTypeName, StringArraySerialized storedParameterValues, SerializableGuid[] rerouteNodes, WireColor wireColor)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(1485607664u, clientRpcParams, RpcDelivery.Reliable);
			BytePacker.WriteValueBitPacked(bufferWriter, userId);
			bufferWriter.WriteValueSafe(in bundleId, default(FastBufferWriter.ForNetworkSerializable));
			bufferWriter.WriteValueSafe(in wireId, default(FastBufferWriter.ForNetworkSerializable));
			bufferWriter.WriteValueSafe(in emitterInstanceId, default(FastBufferWriter.ForNetworkSerializable));
			bool value = emitterMemberName != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(emitterMemberName);
			}
			bool value2 = emitterTypeName != null;
			bufferWriter.WriteValueSafe(in value2, default(FastBufferWriter.ForPrimitives));
			if (value2)
			{
				bufferWriter.WriteValueSafe(emitterTypeName);
			}
			bufferWriter.WriteValueSafe(in receiverInstanceId, default(FastBufferWriter.ForNetworkSerializable));
			bool value3 = receiverMemberName != null;
			bufferWriter.WriteValueSafe(in value3, default(FastBufferWriter.ForPrimitives));
			if (value3)
			{
				bufferWriter.WriteValueSafe(receiverMemberName);
			}
			bool value4 = receiverTypeName != null;
			bufferWriter.WriteValueSafe(in value4, default(FastBufferWriter.ForPrimitives));
			if (value4)
			{
				bufferWriter.WriteValueSafe(receiverTypeName);
			}
			bufferWriter.WriteValueSafe(in storedParameterValues, default(FastBufferWriter.ForNetworkSerializable));
			bool value5 = rerouteNodes != null;
			bufferWriter.WriteValueSafe(in value5, default(FastBufferWriter.ForPrimitives));
			if (value5)
			{
				bufferWriter.WriteValueSafe(rerouteNodes, default(FastBufferWriter.ForNetworkSerializable));
			}
			bufferWriter.WriteValueSafe(in wireColor, default(FastBufferWriter.ForEnums));
			__endSendClientRpc(ref bufferWriter, 1485607664u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
		{
			return;
		}
		__rpc_exec_stage = __RpcExecStage.Send;
		int userId2 = userId;
		SerializableGuid bundleId2 = bundleId;
		SerializableGuid wireId2 = wireId;
		SerializableGuid emitterInstanceId2 = emitterInstanceId;
		string emitterMemberName2 = emitterMemberName;
		string emitterTypeName2 = emitterTypeName;
		SerializableGuid receiverInstanceId2 = receiverInstanceId;
		string receiverMemberName2 = receiverMemberName;
		string receiverTypeName2 = receiverTypeName;
		StringArraySerialized storedParameterValues2 = storedParameterValues;
		SerializableGuid[] rerouteNodes2 = rerouteNodes;
		WireColor wireColor2 = wireColor;
		if (NetworkBehaviourSingleton<CreatorManager>.Instance.RpcReceiveState == RpcReceiveState.Allow)
		{
			WireTogether(userId2, bundleId2, wireId2, emitterInstanceId2, emitterMemberName2, emitterTypeName2, receiverInstanceId2, receiverMemberName2, receiverTypeName2, storedParameterValues2, rerouteNodes2, wireColor2);
			return;
		}
		NetworkBehaviourSingleton<CreatorManager>.Instance.AddCachedRpc(delegate
		{
			WireTogether(userId2, bundleId2, wireId2, emitterInstanceId2, emitterMemberName2, emitterTypeName2, receiverInstanceId2, receiverMemberName2, receiverTypeName2, storedParameterValues2, rerouteNodes2, wireColor2);
		});
	}

	private void WireTogether(int userId, SerializableGuid bundleId, SerializableGuid wireId, SerializableGuid emitterInstanceId, string emitterMemberName, string emitterTypeName, SerializableGuid receiverInstanceId, string receiverMemberName, string receiverTypeName, StringArraySerialized storedParameterValues, SerializableGuid[] rerouteNodes, WireColor wireColor)
	{
		ApplyWiring(bundleId, wireId, emitterInstanceId, emitterMemberName, emitterTypeName, receiverInstanceId, receiverMemberName, receiverTypeName, storedParameterValues, rerouteNodes, wireColor);
		ConvertTemporaryWireIntoPermanent(bundleId, emitterInstanceId, receiverInstanceId, rerouteNodes, userId, wireColor);
		if (userId == EndlessServices.Instance.CloudService.ActiveUserId)
		{
			OnWireConfirmed.Invoke();
			pendingRerouteNodes.Clear();
			if (rerouteNodes.Length != 0)
			{
				MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.AddInstanceIdsToLevelState_ServerRpc(rerouteNodes);
			}
		}
	}

	private void ConvertTemporaryWireIntoPermanent(SerializableGuid bundleId, SerializableGuid emitterInstanceId, SerializableGuid receiverInstanceId, IEnumerable<SerializableGuid> rerouteNodeIds, int userId, WireColor wireColor)
	{
		Color color = wireColorDictionary[wireColor].Color;
		if (!temporaryWires.TryGetValue(userId, out var value) || value == null)
		{
			value = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.SpawnPhysicalWire(bundleId, emitterInstanceId, receiverInstanceId, rerouteNodeIds, wireColor, startActive: true);
			temporaryWires.TryAdd(userId, value);
		}
		if (value.IsLive)
		{
			value.ConvertLiveToPlaced(bundleId, emitterInstanceId, receiverInstanceId, rerouteNodeIds, color);
			value.SetEditing();
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.PhysicalWires.Add(value);
		}
	}

	private void ApplyWiring(SerializableGuid bundleId, SerializableGuid wireId, SerializableGuid emitterInstanceId, string emitterMemberName, string emitterTypeName, SerializableGuid receiverInstanceId, string receiverMemberName, string receiverTypeName, string[] storedParameterValues, IEnumerable<SerializableGuid> rerouteNodes, WireColor wireColor)
	{
		EndlessEventInfo eventInfo = GetEventInfo(emitterInstanceId, emitterTypeName, emitterMemberName, emitter: true);
		EndlessEventInfo eventInfo2 = GetEventInfo(receiverInstanceId, receiverTypeName, receiverMemberName, emitter: false);
		MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.AddWiring(bundleId, wireId, emitterInstanceId, eventInfo, emitterTypeName, receiverInstanceId, eventInfo2, receiverTypeName, storedParameterValues ?? new string[0], rerouteNodes, wireColor);
	}

	public void DeleteWire(SerializableGuid wiringId)
	{
		AttemptDeleteWire_ServerRpc(wiringId);
	}

	[ServerRpc(RequireOwnership = false)]
	private void AttemptDeleteWire_ServerRpc(SerializableGuid wiringId, ServerRpcParams serverRpcParams = default(ServerRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter bufferWriter = __beginSendServerRpc(1188419855u, serverRpcParams, RpcDelivery.Reliable);
				bufferWriter.WriteValueSafe(in wiringId, default(FastBufferWriter.ForNetworkSerializable));
				__endSendServerRpc(ref bufferWriter, 1188419855u, serverRpcParams, RpcDelivery.Reliable);
			}
			if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				__rpc_exec_stage = __RpcExecStage.Send;
				AttemptDeleteWire(wiringId, serverRpcParams);
			}
		}
	}

	private async void AttemptDeleteWire(SerializableGuid wiringId, ServerRpcParams serverRpcParams)
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
			DeleteWire_ClientRpc(wiringId);
			if (!base.IsClient)
			{
				RemoveWiring(wiringId);
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

	[ClientRpc]
	private void DeleteWire_ClientRpc(SerializableGuid wiringId)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(494307063u, clientRpcParams, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in wiringId, default(FastBufferWriter.ForNetworkSerializable));
			__endSendClientRpc(ref bufferWriter, 494307063u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
		{
			return;
		}
		__rpc_exec_stage = __RpcExecStage.Send;
		SerializableGuid wiringId2 = wiringId;
		if (NetworkBehaviourSingleton<CreatorManager>.Instance.RpcReceiveState == RpcReceiveState.Allow)
		{
			RemoveWiring(wiringId2);
			return;
		}
		NetworkBehaviourSingleton<CreatorManager>.Instance.AddCachedRpc(delegate
		{
			RemoveWiring(wiringId2);
		});
	}

	private void RemoveWiring(SerializableGuid wiringId)
	{
		SerializableGuid serializableGuid = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RemoveWiring(wiringId);
		if (!serializableGuid.IsEmpty)
		{
			for (int i = 0; i < MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.PhysicalWires.Count; i++)
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
						pendingRerouteNodes.Remove(serializableGuid2);
					}
					MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.PhysicalWires.RemoveAt(i);
					if (temporaryWires.Values.Any((PhysicalWire valueWire) => valueWire == physicalWire))
					{
						physicalWire.ResetToLive();
					}
					else
					{
						UnityEngine.Object.Destroy(physicalWire.gameObject);
					}
					break;
				}
			}
		}
		for (int num = 0; num < temporaryWires.Keys.Count; num++)
		{
			int key = temporaryWires.Keys.ElementAt(num);
			PhysicalWire physicalWire2 = temporaryWires[key];
			if ((bool)physicalWire2 && physicalWire2.BundleId == wiringId)
			{
				temporaryWires[key] = null;
				break;
			}
		}
		OnWireRemoved.Invoke();
	}

	public void DestroyWires(IEnumerable<SerializableGuid> removedWires)
	{
		foreach (SerializableGuid removedWire in removedWires)
		{
			RemoveWiring(removedWire);
		}
	}

	public void UpdateColorForWire(SerializableGuid wireId, WireColor newColor)
	{
		UpdateWireColor_ServerRpc(wireId, newColor);
	}

	[ServerRpc(RequireOwnership = false)]
	public void UpdateWireColor_ServerRpc(SerializableGuid wireId, WireColor newColor, ServerRpcParams serverRpcParams = default(ServerRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter bufferWriter = __beginSendServerRpc(1994188595u, serverRpcParams, RpcDelivery.Reliable);
				bufferWriter.WriteValueSafe(in wireId, default(FastBufferWriter.ForNetworkSerializable));
				bufferWriter.WriteValueSafe(in newColor, default(FastBufferWriter.ForEnums));
				__endSendServerRpc(ref bufferWriter, 1994188595u, serverRpcParams, RpcDelivery.Reliable);
			}
			if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				__rpc_exec_stage = __RpcExecStage.Send;
				UpdateWireColor(wireId, newColor, serverRpcParams);
			}
		}
	}

	private async void UpdateWireColor(SerializableGuid wireId, WireColor newColor, ServerRpcParams serverRpcParams)
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
			UpdateWireColor_ClientRpc(wireId, newColor);
			if (!base.IsClient)
			{
				ApplyWireColorUpdate(wireId, newColor);
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

	[ClientRpc]
	public void UpdateWireColor_ClientRpc(SerializableGuid wireId, WireColor newColor)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(3360193517u, clientRpcParams, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in wireId, default(FastBufferWriter.ForNetworkSerializable));
			bufferWriter.WriteValueSafe(in newColor, default(FastBufferWriter.ForEnums));
			__endSendClientRpc(ref bufferWriter, 3360193517u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
		{
			return;
		}
		__rpc_exec_stage = __RpcExecStage.Send;
		SerializableGuid wireId2 = wireId;
		WireColor newColor2 = newColor;
		if (NetworkBehaviourSingleton<CreatorManager>.Instance.RpcReceiveState == RpcReceiveState.Allow)
		{
			ApplyWireColorUpdate(wireId2, newColor2);
			return;
		}
		NetworkBehaviourSingleton<CreatorManager>.Instance.AddCachedRpc(delegate
		{
			ApplyWireColorUpdate(wireId2, newColor2);
		});
	}

	private void ApplyWireColorUpdate(SerializableGuid wireId, WireColor newColor)
	{
		MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.UpdateWireColor(wireId, newColor);
	}

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

	[ServerRpc(RequireOwnership = false)]
	internal void EraseRerouteNodes_ServerRpc(SerializableGuid[] instanceIds)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			ServerRpcParams serverRpcParams = default(ServerRpcParams);
			FastBufferWriter bufferWriter = __beginSendServerRpc(2949353187u, serverRpcParams, RpcDelivery.Reliable);
			bool value = instanceIds != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(instanceIds, default(FastBufferWriter.ForNetworkSerializable));
			}
			__endSendServerRpc(ref bufferWriter, 2949353187u, serverRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			EraseRerouteNodes_ClientRpc(instanceIds);
			if (!base.IsClient)
			{
				EraseRerouteNodes(instanceIds);
			}
		}
	}

	[ClientRpc]
	private void EraseRerouteNodes_ClientRpc(SerializableGuid[] instanceIds)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(1441364666u, clientRpcParams, RpcDelivery.Reliable);
			bool value = instanceIds != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(instanceIds, default(FastBufferWriter.ForNetworkSerializable));
			}
			__endSendClientRpc(ref bufferWriter, 1441364666u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
		{
			return;
		}
		__rpc_exec_stage = __RpcExecStage.Send;
		SerializableGuid[] instanceIds2 = instanceIds;
		if (NetworkBehaviourSingleton<CreatorManager>.Instance.RpcReceiveState == RpcReceiveState.Allow)
		{
			EraseRerouteNodes(instanceIds2);
			return;
		}
		NetworkBehaviourSingleton<CreatorManager>.Instance.AddCachedRpc(delegate
		{
			EraseRerouteNodes(instanceIds2);
		});
	}

	private void EraseRerouteNodes(SerializableGuid[] instanceIds)
	{
		List<PhysicalWire> list = new List<PhysicalWire>();
		foreach (SerializableGuid serializableGuid in instanceIds)
		{
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(serializableGuid);
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.EraseReroute(serializableGuid);
			if (pendingRerouteNodes.Remove(serializableGuid) && pendingRerouteNodes.Count == 0)
			{
				DisableRerouteUndo.Invoke();
			}
			foreach (PhysicalWire physicalWire2 in MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.PhysicalWires)
			{
				if (physicalWire2.RerouteNodeIds.Remove(serializableGuid))
				{
					list.Add(physicalWire2);
					break;
				}
			}
		}
		foreach (PhysicalWire item in list)
		{
			item.GenerateLineRendererPositions();
		}
		foreach (int key in temporaryWires.Keys)
		{
			PhysicalWire physicalWire = temporaryWires[key];
			if ((bool)physicalWire && list.Contains(physicalWire) && physicalWire.RerouteNodeIds.Count == 0)
			{
				DisableRerouteUndo.Invoke();
			}
		}
	}

	public void RequestPopLastRerouteNode(InputAction.CallbackContext callbackContext)
	{
		int activeUserId = EndlessServices.Instance.CloudService.ActiveUserId;
		if ((bool)temporaryWires[activeUserId])
		{
			PopLastRerouteNode_ServerRPC();
		}
	}

	[ServerRpc(RequireOwnership = false)]
	public void PopLastRerouteNode_ServerRPC(ServerRpcParams serverRpc = default(ServerRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter bufferWriter = __beginSendServerRpc(416093770u, serverRpc, RpcDelivery.Reliable);
				__endSendServerRpc(ref bufferWriter, 416093770u, serverRpc, RpcDelivery.Reliable);
			}
			if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				__rpc_exec_stage = __RpcExecStage.Send;
				PopLastRerouteNode(serverRpc);
			}
		}
	}

	private async void PopLastRerouteNode(ServerRpcParams serverRpc)
	{
		if (!base.IsServer)
		{
			return;
		}
		ulong senderClientId = serverRpc.Receive.SenderClientId;
		if (!NetworkBehaviourSingleton<UserIdManager>.Instance.TryGetUserId(senderClientId, out var userId))
		{
			Debug.LogException(new Exception($"Unable to determine User Id for Client Id: {senderClientId}"));
		}
		if (await NetworkBehaviourSingleton<CreatorManager>.Instance.UserCanEditLevel(userId))
		{
			SerializableGuid serializableGuid = SerializableGuid.Empty;
			if (playerToTempReroutes.ContainsKey(userId) && playerToTempReroutes[userId].Count > 0)
			{
				serializableGuid = playerToTempReroutes[userId][playerToTempReroutes[userId].Count - 1];
				playerToTempReroutes[userId].RemoveAt(playerToTempReroutes[userId].Count - 1);
			}
			else if (temporaryWires.ContainsKey(userId) && temporaryWires[userId].RerouteNodeIds.Count > 0)
			{
				serializableGuid = temporaryWires[userId].RerouteNodeIds[temporaryWires[userId].RerouteNodeIds.Count - 1];
			}
			if (!serializableGuid.IsEmpty)
			{
				EraseRerouteNodes_ServerRpc(new SerializableGuid[1] { serializableGuid });
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

	public void LateUpdate()
	{
		if (base.IsActive && !(MonoBehaviourSingleton<StageManager>.Instance.ActiveStage == null) && MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.PhysicalWires.Count != 0)
		{
			if (currentWireIndex > MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.PhysicalWires.Count - 1)
			{
				currentWireIndex = 0;
			}
			PhysicalWire physicalWire = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.PhysicalWires[currentWireIndex];
			if (physicalWire != null)
			{
				physicalWire.BakeCollision();
			}
			currentWireIndex++;
		}
	}

	protected override void __initializeVariables()
	{
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		__registerRpc(405623707u, __rpc_handler_405623707, "ClearPendingRerouteNodesForPlayer_ServerRpc");
		__registerRpc(4103524712u, __rpc_handler_4103524712, "WiringToolExited_ServerRpc");
		__registerRpc(1791207916u, __rpc_handler_1791207916, "WiringToolExited_ClientRpc");
		__registerRpc(354023136u, __rpc_handler_354023136, "SetTemporaryWireTargetToPlayer_ServerRpc");
		__registerRpc(1104155854u, __rpc_handler_1104155854, "SetTemporaryWireTargetToPlayer_ClientRpc");
		__registerRpc(3716040516u, __rpc_handler_3716040516, "SetTemporaryWireTarget_ServerRpc");
		__registerRpc(4269480805u, __rpc_handler_4269480805, "SetTemporaryWireTarget_ClientRpc");
		__registerRpc(3772252676u, __rpc_handler_3772252676, "SetWireEditedByPlayer_ServerRpc");
		__registerRpc(1404899374u, __rpc_handler_1404899374, "SetWireEditedByPlayer_ClientRpc");
		__registerRpc(90801435u, __rpc_handler_90801435, "SpawnTemporaryWire_ServerRpc");
		__registerRpc(3985273964u, __rpc_handler_3985273964, "SpawnTemporaryWire_ClientRpc");
		__registerRpc(359986616u, __rpc_handler_359986616, "AddRerouteNodeToPosition_ServerRpc");
		__registerRpc(101785308u, __rpc_handler_101785308, "PlaceRerouteNode_ClientRPC");
		__registerRpc(3636364046u, __rpc_handler_3636364046, "ClearTemporaryWireForPlayer_ServerRpc");
		__registerRpc(3434663878u, __rpc_handler_3434663878, "ClearTemporaryWireForPlayer_ClientRpc");
		__registerRpc(328729018u, __rpc_handler_328729018, "UpdateWireDetails_ServerRpc");
		__registerRpc(1095343870u, __rpc_handler_1095343870, "UpdateWireDetails_ClientRpc");
		__registerRpc(1366945158u, __rpc_handler_1366945158, "AttemptWireTogether_ServerRpc");
		__registerRpc(1485607664u, __rpc_handler_1485607664, "WireTogether_ClientRpc");
		__registerRpc(1188419855u, __rpc_handler_1188419855, "AttemptDeleteWire_ServerRpc");
		__registerRpc(494307063u, __rpc_handler_494307063, "DeleteWire_ClientRpc");
		__registerRpc(1994188595u, __rpc_handler_1994188595, "UpdateWireColor_ServerRpc");
		__registerRpc(3360193517u, __rpc_handler_3360193517, "UpdateWireColor_ClientRpc");
		__registerRpc(2949353187u, __rpc_handler_2949353187, "EraseRerouteNodes_ServerRpc");
		__registerRpc(1441364666u, __rpc_handler_1441364666, "EraseRerouteNodes_ClientRpc");
		__registerRpc(416093770u, __rpc_handler_416093770, "PopLastRerouteNode_ServerRPC");
		base.__initializeRpcs();
	}

	private static void __rpc_handler_405623707(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
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
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((WiringTool)target).ClearPendingRerouteNodesForPlayer_ServerRpc(value2, server);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_4103524712(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
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
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((WiringTool)target).WiringToolExited_ServerRpc(value2, server);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_1791207916(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			ByteUnpacker.ReadValueBitPacked(reader, out int value);
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((WiringTool)target).WiringToolExited_ClientRpc(value);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_354023136(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((WiringTool)target).SetTemporaryWireTargetToPlayer_ServerRpc(server);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_1104155854(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			ByteUnpacker.ReadValueBitPacked(reader, out int value);
			ByteUnpacker.ReadValueBitPacked(reader, out ulong value2);
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((WiringTool)target).SetTemporaryWireTargetToPlayer_ClientRpc(value, value2);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_3716040516(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out SerializableGuid value, default(FastBufferWriter.ForNetworkSerializable));
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((WiringTool)target).SetTemporaryWireTarget_ServerRpc(value, server);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_4269480805(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out SerializableGuid value, default(FastBufferWriter.ForNetworkSerializable));
			ByteUnpacker.ReadValueBitPacked(reader, out int value2);
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((WiringTool)target).SetTemporaryWireTarget_ClientRpc(value, value2);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_3772252676(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out SerializableGuid value, default(FastBufferWriter.ForNetworkSerializable));
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((WiringTool)target).SetWireEditedByPlayer_ServerRpc(value, server);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_1404899374(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out SerializableGuid value, default(FastBufferWriter.ForNetworkSerializable));
			ByteUnpacker.ReadValueBitPacked(reader, out int value2);
			ClientRpcParams client = rpcParams.Client;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((WiringTool)target).SetWireEditedByPlayer_ClientRpc(value, value2, client);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_90801435(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out SerializableGuid value, default(FastBufferWriter.ForNetworkSerializable));
			reader.ReadValueSafe(out SerializableGuid value2, default(FastBufferWriter.ForNetworkSerializable));
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((WiringTool)target).SpawnTemporaryWire_ServerRpc(value, value2, server);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_3985273964(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out SerializableGuid value, default(FastBufferWriter.ForNetworkSerializable));
			ByteUnpacker.ReadValueBitPacked(reader, out int value2);
			ByteUnpacker.ReadValueBitPacked(reader, out ulong value3);
			reader.ReadValueSafe(out SerializableGuid value4, default(FastBufferWriter.ForNetworkSerializable));
			ClientRpcParams client = rpcParams.Client;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((WiringTool)target).SpawnTemporaryWire_ClientRpc(value, value2, value3, value4, client);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_359986616(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out Vector3Int value);
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((WiringTool)target).AddRerouteNodeToPosition_ServerRpc(value, server);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_101785308(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out Vector3Int value);
			reader.ReadValueSafe(out SerializableGuid value2, default(FastBufferWriter.ForNetworkSerializable));
			reader.ReadValueSafe(out SerializableGuid value3, default(FastBufferWriter.ForNetworkSerializable));
			ByteUnpacker.ReadValueBitPacked(reader, out ulong value4);
			ByteUnpacker.ReadValueBitPacked(reader, out ulong value5);
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((WiringTool)target).PlaceRerouteNode_ClientRPC(value, value2, value3, value4, value5);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_3636364046(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((WiringTool)target).ClearTemporaryWireForPlayer_ServerRpc(server);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_3434663878(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			ByteUnpacker.ReadValueBitPacked(reader, out int value);
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((WiringTool)target).ClearTemporaryWireForPlayer_ClientRpc(value);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_328729018(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out SerializableGuid value, default(FastBufferWriter.ForNetworkSerializable));
			reader.ReadValueSafe(out SerializableGuid value2, default(FastBufferWriter.ForNetworkSerializable));
			reader.ReadValueSafe(out SerializableGuid value3, default(FastBufferWriter.ForNetworkSerializable));
			reader.ReadValueSafe(out bool value4, default(FastBufferWriter.ForPrimitives));
			string s = null;
			if (value4)
			{
				reader.ReadValueSafe(out s, oneByteChars: false);
			}
			reader.ReadValueSafe(out bool value5, default(FastBufferWriter.ForPrimitives));
			string s2 = null;
			if (value5)
			{
				reader.ReadValueSafe(out s2, oneByteChars: false);
			}
			reader.ReadValueSafe(out SerializableGuid value6, default(FastBufferWriter.ForNetworkSerializable));
			reader.ReadValueSafe(out bool value7, default(FastBufferWriter.ForPrimitives));
			string s3 = null;
			if (value7)
			{
				reader.ReadValueSafe(out s3, oneByteChars: false);
			}
			reader.ReadValueSafe(out bool value8, default(FastBufferWriter.ForPrimitives));
			string s4 = null;
			if (value8)
			{
				reader.ReadValueSafe(out s4, oneByteChars: false);
			}
			reader.ReadValueSafe(out StringArraySerialized value9, default(FastBufferWriter.ForNetworkSerializable));
			reader.ReadValueSafe(out bool value10, default(FastBufferWriter.ForPrimitives));
			SerializableGuid[] value11 = null;
			if (value10)
			{
				reader.ReadValueSafe(out value11, default(FastBufferWriter.ForNetworkSerializable));
			}
			reader.ReadValueSafe(out WireColor value12, default(FastBufferWriter.ForEnums));
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((WiringTool)target).UpdateWireDetails_ServerRpc(value, value2, value3, s, s2, value6, s3, s4, value9, value11, value12, server);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_1095343870(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out SerializableGuid value, default(FastBufferWriter.ForNetworkSerializable));
			reader.ReadValueSafe(out SerializableGuid value2, default(FastBufferWriter.ForNetworkSerializable));
			reader.ReadValueSafe(out SerializableGuid value3, default(FastBufferWriter.ForNetworkSerializable));
			reader.ReadValueSafe(out bool value4, default(FastBufferWriter.ForPrimitives));
			string s = null;
			if (value4)
			{
				reader.ReadValueSafe(out s, oneByteChars: false);
			}
			reader.ReadValueSafe(out bool value5, default(FastBufferWriter.ForPrimitives));
			string s2 = null;
			if (value5)
			{
				reader.ReadValueSafe(out s2, oneByteChars: false);
			}
			reader.ReadValueSafe(out SerializableGuid value6, default(FastBufferWriter.ForNetworkSerializable));
			reader.ReadValueSafe(out bool value7, default(FastBufferWriter.ForPrimitives));
			string s3 = null;
			if (value7)
			{
				reader.ReadValueSafe(out s3, oneByteChars: false);
			}
			reader.ReadValueSafe(out bool value8, default(FastBufferWriter.ForPrimitives));
			string s4 = null;
			if (value8)
			{
				reader.ReadValueSafe(out s4, oneByteChars: false);
			}
			reader.ReadValueSafe(out StringArraySerialized value9, default(FastBufferWriter.ForNetworkSerializable));
			reader.ReadValueSafe(out bool value10, default(FastBufferWriter.ForPrimitives));
			SerializableGuid[] value11 = null;
			if (value10)
			{
				reader.ReadValueSafe(out value11, default(FastBufferWriter.ForNetworkSerializable));
			}
			reader.ReadValueSafe(out WireColor value12, default(FastBufferWriter.ForEnums));
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((WiringTool)target).UpdateWireDetails_ClientRpc(value, value2, value3, s, s2, value6, s3, s4, value9, value11, value12);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_1366945158(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
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
			reader.ReadValueSafe(out bool value3, default(FastBufferWriter.ForPrimitives));
			string s2 = null;
			if (value3)
			{
				reader.ReadValueSafe(out s2, oneByteChars: false);
			}
			reader.ReadValueSafe(out SerializableGuid value4, default(FastBufferWriter.ForNetworkSerializable));
			reader.ReadValueSafe(out bool value5, default(FastBufferWriter.ForPrimitives));
			string s3 = null;
			if (value5)
			{
				reader.ReadValueSafe(out s3, oneByteChars: false);
			}
			reader.ReadValueSafe(out bool value6, default(FastBufferWriter.ForPrimitives));
			string s4 = null;
			if (value6)
			{
				reader.ReadValueSafe(out s4, oneByteChars: false);
			}
			reader.ReadValueSafe(out StringArraySerialized value7, default(FastBufferWriter.ForNetworkSerializable));
			reader.ReadValueSafe(out bool value8, default(FastBufferWriter.ForPrimitives));
			SerializableGuid[] value9 = null;
			if (value8)
			{
				reader.ReadValueSafe(out value9, default(FastBufferWriter.ForNetworkSerializable));
			}
			reader.ReadValueSafe(out WireColor value10, default(FastBufferWriter.ForEnums));
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((WiringTool)target).AttemptWireTogether_ServerRpc(value, s, s2, value4, s3, s4, value7, value9, value10, server);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_1485607664(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			ByteUnpacker.ReadValueBitPacked(reader, out int value);
			reader.ReadValueSafe(out SerializableGuid value2, default(FastBufferWriter.ForNetworkSerializable));
			reader.ReadValueSafe(out SerializableGuid value3, default(FastBufferWriter.ForNetworkSerializable));
			reader.ReadValueSafe(out SerializableGuid value4, default(FastBufferWriter.ForNetworkSerializable));
			reader.ReadValueSafe(out bool value5, default(FastBufferWriter.ForPrimitives));
			string s = null;
			if (value5)
			{
				reader.ReadValueSafe(out s, oneByteChars: false);
			}
			reader.ReadValueSafe(out bool value6, default(FastBufferWriter.ForPrimitives));
			string s2 = null;
			if (value6)
			{
				reader.ReadValueSafe(out s2, oneByteChars: false);
			}
			reader.ReadValueSafe(out SerializableGuid value7, default(FastBufferWriter.ForNetworkSerializable));
			reader.ReadValueSafe(out bool value8, default(FastBufferWriter.ForPrimitives));
			string s3 = null;
			if (value8)
			{
				reader.ReadValueSafe(out s3, oneByteChars: false);
			}
			reader.ReadValueSafe(out bool value9, default(FastBufferWriter.ForPrimitives));
			string s4 = null;
			if (value9)
			{
				reader.ReadValueSafe(out s4, oneByteChars: false);
			}
			reader.ReadValueSafe(out StringArraySerialized value10, default(FastBufferWriter.ForNetworkSerializable));
			reader.ReadValueSafe(out bool value11, default(FastBufferWriter.ForPrimitives));
			SerializableGuid[] value12 = null;
			if (value11)
			{
				reader.ReadValueSafe(out value12, default(FastBufferWriter.ForNetworkSerializable));
			}
			reader.ReadValueSafe(out WireColor value13, default(FastBufferWriter.ForEnums));
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((WiringTool)target).WireTogether_ClientRpc(value, value2, value3, value4, s, s2, value7, s3, s4, value10, value12, value13);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_1188419855(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out SerializableGuid value, default(FastBufferWriter.ForNetworkSerializable));
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((WiringTool)target).AttemptDeleteWire_ServerRpc(value, server);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_494307063(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out SerializableGuid value, default(FastBufferWriter.ForNetworkSerializable));
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((WiringTool)target).DeleteWire_ClientRpc(value);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_1994188595(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out SerializableGuid value, default(FastBufferWriter.ForNetworkSerializable));
			reader.ReadValueSafe(out WireColor value2, default(FastBufferWriter.ForEnums));
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((WiringTool)target).UpdateWireColor_ServerRpc(value, value2, server);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_3360193517(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out SerializableGuid value, default(FastBufferWriter.ForNetworkSerializable));
			reader.ReadValueSafe(out WireColor value2, default(FastBufferWriter.ForEnums));
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((WiringTool)target).UpdateWireColor_ClientRpc(value, value2);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_2949353187(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
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
			((WiringTool)target).EraseRerouteNodes_ServerRpc(value2);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_1441364666(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
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
			((WiringTool)target).EraseRerouteNodes_ClientRpc(value2);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_416093770(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((WiringTool)target).PopLastRerouteNode_ServerRPC(server);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	protected internal override string __getTypeName()
	{
		return "WiringTool";
	}
}
