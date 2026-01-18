using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Endless.Assets;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Creator.LevelEditing.Runtime;

public class MoveTool : PropBasedTool, IBackable
{
	private enum MoveState
	{
		None,
		Placing
	}

	private MoveState moveState;

	private SerializableGuid movedObjectInstanceId = SerializableGuid.Empty;

	private List<SerializableGuid> propsBeingMoved = new List<SerializableGuid>();

	private SerializableGuid hoveredPropId;

	public override ToolType ToolType => ToolType.Move;

	public override void HandleSelected()
	{
		base.HandleSelected();
		Set3DCursorUsesIntersection(val: true);
		base.UIToolPrompter.Hide();
	}

	public override void HandleDeselected()
	{
		base.HandleDeselected();
		if (hoveredPropId != SerializableGuid.Empty)
		{
			MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(hoveredPropId, PropLocationType.Selected);
			hoveredPropId = SerializableGuid.Empty;
		}
		Set3DCursorUsesIntersection(val: true);
		ClearLineCastExclusionId();
		if (!movedObjectInstanceId.IsEmpty)
		{
			MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(movedObjectInstanceId, PropLocationType.Selected);
			CancelPropMovement_ServerRpc(movedObjectInstanceId);
		}
	}

	public override void CreatorExited()
	{
		hoveredPropId = SerializableGuid.Empty;
		if (!movedObjectInstanceId.IsEmpty)
		{
			CancelPropMovement_ServerRpc(movedObjectInstanceId);
			DestroyPreview();
		}
	}

	public void OnBack()
	{
		HandleDeselected();
	}

	private void HandlePropSelectionAttempt()
	{
		SerializableGuid propUnderCursor = GetPropUnderCursor();
		if (!propUnderCursor.IsEmpty && !propsBeingMoved.Contains(propUnderCursor))
		{
			Set3DCursorUsesIntersection(val: false);
			if (movedObjectInstanceId != SerializableGuid.Empty)
			{
				MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(movedObjectInstanceId, PropLocationType.Selected);
			}
			movedObjectInstanceId = propUnderCursor;
			MonoBehaviourSingleton<PropLocationMarker>.Instance.MarkCellsForProp(movedObjectInstanceId, PropLocationType.Selected);
			SetPropAsMoving_ServerRpc(propUnderCursor);
		}
	}

	[ServerRpc(RequireOwnership = false)]
	private void SetPropAsMoving_ServerRpc(SerializableGuid instanceId, ServerRpcParams rpcParams = default(ServerRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			FastBufferWriter bufferWriter = __beginSendServerRpc(2231083241u, rpcParams, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in instanceId, default(FastBufferWriter.ForNetworkSerializable));
			__endSendServerRpc(ref bufferWriter, 2231083241u, rpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
		{
			return;
		}
		__rpc_exec_stage = __RpcExecStage.Send;
		if (propsBeingMoved.Contains(instanceId))
		{
			ClientRpcParams rpcParams2 = new ClientRpcParams
			{
				Send = new ClientRpcSendParams
				{
					TargetClientIds = new ulong[1] { rpcParams.Receive.SenderClientId }
				}
			};
			RejectPropSelection_ClientRpc(instanceId, rpcParams2);
		}
		else
		{
			SetPropAsMoving_ClientRpc(instanceId, rpcParams.Receive.SenderClientId);
			if (!base.IsClient)
			{
				SetPropAsMoving(instanceId, rpcParams.Receive.SenderClientId);
			}
		}
	}

	[ClientRpc]
	private void SetPropAsMoving_ClientRpc(SerializableGuid instanceId, ulong playerId)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(40283873u, clientRpcParams, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in instanceId, default(FastBufferWriter.ForNetworkSerializable));
			BytePacker.WriteValueBitPacked(bufferWriter, playerId);
			__endSendClientRpc(ref bufferWriter, 40283873u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
		{
			return;
		}
		__rpc_exec_stage = __RpcExecStage.Send;
		SerializableGuid instanceId2 = instanceId;
		ulong playerId2 = playerId;
		if (NetworkBehaviourSingleton<CreatorManager>.Instance.RpcReceiveState == RpcReceiveState.Allow)
		{
			SetPropAsMoving(instanceId2, playerId2);
			return;
		}
		NetworkBehaviourSingleton<CreatorManager>.Instance.AddCachedRpc(delegate
		{
			SetPropAsMoving(instanceId2, playerId2);
		});
	}

	private void SetPropAsMoving(SerializableGuid instanceId, ulong playerId)
	{
		propsBeingMoved.Add(instanceId);
		if (playerId == NetworkManager.Singleton.LocalClientId)
		{
			SerializableGuid assetIdFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetAssetIdFromInstanceId(instanceId);
			PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(assetIdFromInstanceId);
			GeneratePropPreview(runtimePropInfo, movedObjectInstanceId);
			GameObject gameObjectFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(instanceId);
			base.PropGhostTransform.rotation = gameObjectFromInstanceId.transform.rotation;
			moveState = MoveState.Placing;
			MonoBehaviourSingleton<BackManager>.Instance.ClaimContext(this);
		}
	}

	[ClientRpc]
	private void RejectPropSelection_ClientRpc(SerializableGuid instanceId, ClientRpcParams rpcParams = default(ClientRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			FastBufferWriter bufferWriter = __beginSendClientRpc(1449843910u, rpcParams, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in instanceId, default(FastBufferWriter.ForNetworkSerializable));
			__endSendClientRpc(ref bufferWriter, 1449843910u, rpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
		{
			return;
		}
		__rpc_exec_stage = __RpcExecStage.Send;
		if (NetworkBehaviourSingleton<CreatorManager>.Instance.RpcReceiveState == RpcReceiveState.Allow)
		{
			SerializableGuid assetIdFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetAssetIdFromInstanceId(instanceId);
			PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(assetIdFromInstanceId);
			Debug.Log($"Unable to select Prop Id: {instanceId}, Asset Name: {runtimePropInfo.PropData.Name}");
			ClearLineCastExclusionId();
			if (!movedObjectInstanceId.IsEmpty)
			{
				MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(movedObjectInstanceId, PropLocationType.Selected);
				movedObjectInstanceId = SerializableGuid.Empty;
			}
		}
	}

	public override void ToolReleased()
	{
		base.ToolReleased();
		switch (moveState)
		{
		case MoveState.None:
			HandlePropSelectionAttempt();
			break;
		case MoveState.Placing:
			HandlePropPlacementAttempt();
			break;
		}
	}

	private void HandlePropPlacementAttempt()
	{
		if (base.PropGhostTransform != null && NoInvalidOverlapsExist())
		{
			PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(base.ActiveAssetId);
			if (runtimePropInfo == null)
			{
				Debug.LogException(new Exception("Server was told to spawn invalid asset ID!"));
			}
			else if (MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.PlacementIsValidAllowingSelfOverlap(runtimePropInfo.PropData, base.PropGhostTransform.position, Quaternion.Euler(base.PropGhostTransform.rotation.eulerAngles), movedObjectInstanceId))
			{
				Set3DCursorUsesIntersection(val: true);
				ClearLineCastExclusionId();
				MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(base.ActiveAssetId, PropLocationType.Selected);
				base.PropGhostTransform.gameObject.SetActive(value: false);
				AttemptMoveProp_ServerRpc(base.PropGhostTransform.position, base.PropGhostTransform.rotation.eulerAngles, base.ActiveAssetId, movedObjectInstanceId);
			}
		}
	}

	[ServerRpc(RequireOwnership = false)]
	private void AttemptMoveProp_ServerRpc(Vector3 position, Vector3 eulerAngles, SerializableGuid assetId, SerializableGuid instanceId, ServerRpcParams rpcParams = default(ServerRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter bufferWriter = __beginSendServerRpc(1804559521u, rpcParams, RpcDelivery.Reliable);
				bufferWriter.WriteValueSafe(in position);
				bufferWriter.WriteValueSafe(in eulerAngles);
				bufferWriter.WriteValueSafe(in assetId, default(FastBufferWriter.ForNetworkSerializable));
				bufferWriter.WriteValueSafe(in instanceId, default(FastBufferWriter.ForNetworkSerializable));
				__endSendServerRpc(ref bufferWriter, 1804559521u, rpcParams, RpcDelivery.Reliable);
			}
			if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				__rpc_exec_stage = __RpcExecStage.Send;
				AttemptMoveProp(position, eulerAngles, assetId, instanceId, rpcParams);
			}
		}
	}

	private async Task AttemptMoveProp(Vector3 position, Vector3 eulerAngles, SerializableGuid assetId, SerializableGuid instanceId, ServerRpcParams rpcParams)
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
		PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(assetId);
		if (runtimePropInfo == null)
		{
			Debug.LogException(new Exception("Server was told to spawn invalid asset ID!"));
		}
		else if (MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.PlacementIsValidAllowingSelfOverlap(runtimePropInfo.PropData, position, Quaternion.Euler(eulerAngles), instanceId))
		{
			FirePropEvent("propMoved", assetId, runtimePropInfo.PropData.Name);
			MoveProp_ClientRpc(instanceId, assetId, position, eulerAngles, rpcParams.Receive.SenderClientId);
			if (!base.IsClient)
			{
				MoveProp(instanceId, assetId, position, eulerAngles, rpcParams.Receive.SenderClientId);
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

	[ClientRpc]
	private void MoveProp_ClientRpc(SerializableGuid instanceId, SerializableGuid assetId, Vector3 position, Vector3 eulerAngles, ulong playerId)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(1737830098u, clientRpcParams, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in instanceId, default(FastBufferWriter.ForNetworkSerializable));
			bufferWriter.WriteValueSafe(in assetId, default(FastBufferWriter.ForNetworkSerializable));
			bufferWriter.WriteValueSafe(in position);
			bufferWriter.WriteValueSafe(in eulerAngles);
			BytePacker.WriteValueBitPacked(bufferWriter, playerId);
			__endSendClientRpc(ref bufferWriter, 1737830098u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
		{
			return;
		}
		__rpc_exec_stage = __RpcExecStage.Send;
		SerializableGuid instanceId2 = instanceId;
		SerializableGuid assetId2 = assetId;
		Vector3 position2 = position;
		Vector3 eulerAngles2 = eulerAngles;
		ulong playerId2 = playerId;
		if (NetworkBehaviourSingleton<CreatorManager>.Instance.RpcReceiveState == RpcReceiveState.Allow)
		{
			MoveProp(instanceId2, assetId2, position2, eulerAngles2, playerId2);
			return;
		}
		NetworkBehaviourSingleton<CreatorManager>.Instance.AddCachedRpc(delegate
		{
			MoveProp(instanceId2, assetId2, position2, eulerAngles2, playerId2);
		});
	}

	private void MoveProp(SerializableGuid instanceId, SerializableGuid assetId, Vector3 position, Vector3 eulerAngles, ulong playerId)
	{
		MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.MoveProp(assetId, instanceId, position, eulerAngles);
		GameObject gameObjectFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(instanceId);
		gameObjectFromInstanceId.transform.position = position;
		gameObjectFromInstanceId.transform.rotation = Quaternion.Euler(eulerAngles);
		propsBeingMoved.Remove(instanceId);
		if (playerId == NetworkManager.Singleton.LocalClientId)
		{
			DestroyPreview();
			moveState = MoveState.None;
			MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(movedObjectInstanceId, PropLocationType.Selected);
			movedObjectInstanceId = SerializableGuid.Empty;
			hoveredPropId = SerializableGuid.Empty;
			MonoBehaviourSingleton<BackManager>.Instance.UnclaimContext(this);
		}
	}

	[ServerRpc(RequireOwnership = false)]
	private void CancelPropMovement_ServerRpc(SerializableGuid instanceId, ServerRpcParams rpcParams = default(ServerRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			FastBufferWriter bufferWriter = __beginSendServerRpc(2218965149u, rpcParams, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in instanceId, default(FastBufferWriter.ForNetworkSerializable));
			__endSendServerRpc(ref bufferWriter, 2218965149u, rpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			CancelPropMovement_ClientRpc(instanceId, rpcParams.Receive.SenderClientId);
			if (!base.IsClient)
			{
				CancelPropMovement(instanceId, rpcParams.Receive.SenderClientId);
			}
		}
	}

	[ClientRpc]
	private void CancelPropMovement_ClientRpc(SerializableGuid instanceId, ulong playerId)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(149448483u, clientRpcParams, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in instanceId, default(FastBufferWriter.ForNetworkSerializable));
			BytePacker.WriteValueBitPacked(bufferWriter, playerId);
			__endSendClientRpc(ref bufferWriter, 149448483u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
		{
			return;
		}
		__rpc_exec_stage = __RpcExecStage.Send;
		SerializableGuid instanceId2 = instanceId;
		ulong playerId2 = playerId;
		if (NetworkBehaviourSingleton<CreatorManager>.Instance.RpcReceiveState == RpcReceiveState.Allow)
		{
			CancelPropMovement(instanceId2, playerId2);
			return;
		}
		NetworkBehaviourSingleton<CreatorManager>.Instance.AddCachedRpc(delegate
		{
			CancelPropMovement(instanceId2, playerId2);
		});
	}

	private void CancelPropMovement(SerializableGuid instanceId, ulong playerId)
	{
		propsBeingMoved.Remove(instanceId);
		if (playerId == NetworkManager.Singleton.LocalClientId)
		{
			DestroyPreview();
			Set3DCursorUsesIntersection(val: true);
			moveState = MoveState.None;
			movedObjectInstanceId = SerializableGuid.Empty;
			MonoBehaviourSingleton<BackManager>.Instance.UnclaimContext(this);
		}
	}

	public override void UpdateTool()
	{
		base.UpdateTool();
		switch (moveState)
		{
		case MoveState.None:
			UpdateHighlightState();
			break;
		case MoveState.Placing:
			if (!base.IsMobile)
			{
				UpdatePropPlacement();
			}
			break;
		}
	}

	private void UpdateHighlightState()
	{
		LineCastHit activeLineCastResult = base.ActiveLineCastResult;
		if (activeLineCastResult.IntersectionOccured)
		{
			PropCell cellFromCoordinateAs = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetCellFromCoordinateAs<PropCell>(activeLineCastResult.IntersectedObjectPosition);
			if (cellFromCoordinateAs != null)
			{
				if (!(cellFromCoordinateAs.InstanceId == hoveredPropId))
				{
					if (hoveredPropId != SerializableGuid.Empty && hoveredPropId != cellFromCoordinateAs.InstanceId)
					{
						MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(hoveredPropId, PropLocationType.Selected);
					}
					hoveredPropId = cellFromCoordinateAs.InstanceId;
					MonoBehaviourSingleton<PropLocationMarker>.Instance.MarkCellsForProp(hoveredPropId, PropLocationType.Selected);
				}
			}
			else if (!hoveredPropId.IsEmpty)
			{
				if (hoveredPropId != movedObjectInstanceId)
				{
					MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(hoveredPropId, PropLocationType.Selected);
				}
				hoveredPropId = SerializableGuid.Empty;
			}
		}
		else if (!hoveredPropId.IsEmpty)
		{
			if (hoveredPropId != movedObjectInstanceId)
			{
				MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(hoveredPropId, PropLocationType.Selected);
			}
			hoveredPropId = SerializableGuid.Empty;
		}
	}

	public bool PropIsBeingMoved(SerializableGuid instanceId)
	{
		return propsBeingMoved.Contains(instanceId);
	}

	public override void ToolSecondaryPressed()
	{
		base.ToolSecondaryPressed();
		ClearLineCastExclusionId();
		CancelPropMovement_ServerRpc(movedObjectInstanceId);
	}

	protected override void __initializeVariables()
	{
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		__registerRpc(2231083241u, __rpc_handler_2231083241, "SetPropAsMoving_ServerRpc");
		__registerRpc(40283873u, __rpc_handler_40283873, "SetPropAsMoving_ClientRpc");
		__registerRpc(1449843910u, __rpc_handler_1449843910, "RejectPropSelection_ClientRpc");
		__registerRpc(1804559521u, __rpc_handler_1804559521, "AttemptMoveProp_ServerRpc");
		__registerRpc(1737830098u, __rpc_handler_1737830098, "MoveProp_ClientRpc");
		__registerRpc(2218965149u, __rpc_handler_2218965149, "CancelPropMovement_ServerRpc");
		__registerRpc(149448483u, __rpc_handler_149448483, "CancelPropMovement_ClientRpc");
		base.__initializeRpcs();
	}

	private static void __rpc_handler_2231083241(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out SerializableGuid value, default(FastBufferWriter.ForNetworkSerializable));
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((MoveTool)target).SetPropAsMoving_ServerRpc(value, server);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_40283873(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out SerializableGuid value, default(FastBufferWriter.ForNetworkSerializable));
			ByteUnpacker.ReadValueBitPacked(reader, out ulong value2);
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((MoveTool)target).SetPropAsMoving_ClientRpc(value, value2);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_1449843910(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out SerializableGuid value, default(FastBufferWriter.ForNetworkSerializable));
			ClientRpcParams client = rpcParams.Client;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((MoveTool)target).RejectPropSelection_ClientRpc(value, client);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_1804559521(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out Vector3 value);
			reader.ReadValueSafe(out Vector3 value2);
			reader.ReadValueSafe(out SerializableGuid value3, default(FastBufferWriter.ForNetworkSerializable));
			reader.ReadValueSafe(out SerializableGuid value4, default(FastBufferWriter.ForNetworkSerializable));
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((MoveTool)target).AttemptMoveProp_ServerRpc(value, value2, value3, value4, server);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_1737830098(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out SerializableGuid value, default(FastBufferWriter.ForNetworkSerializable));
			reader.ReadValueSafe(out SerializableGuid value2, default(FastBufferWriter.ForNetworkSerializable));
			reader.ReadValueSafe(out Vector3 value3);
			reader.ReadValueSafe(out Vector3 value4);
			ByteUnpacker.ReadValueBitPacked(reader, out ulong value5);
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((MoveTool)target).MoveProp_ClientRpc(value, value2, value3, value4, value5);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_2218965149(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out SerializableGuid value, default(FastBufferWriter.ForNetworkSerializable));
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((MoveTool)target).CancelPropMovement_ServerRpc(value, server);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_149448483(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out SerializableGuid value, default(FastBufferWriter.ForNetworkSerializable));
			ByteUnpacker.ReadValueBitPacked(reader, out ulong value2);
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((MoveTool)target).CancelPropMovement_ClientRpc(value, value2);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	protected internal override string __getTypeName()
	{
		return "MoveTool";
	}
}
