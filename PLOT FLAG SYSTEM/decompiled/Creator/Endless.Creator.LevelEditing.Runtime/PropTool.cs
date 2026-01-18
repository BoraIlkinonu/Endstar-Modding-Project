using System;
using Endless.Assets;
using Endless.Creator.UI;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Props.Assets;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Endless.Creator.LevelEditing.Runtime;

public class PropTool : PropBasedTool
{
	private UIScriptWindowView scriptWindow;

	public UnityEvent<SerializableGuid> OnSelectedAssetChanged = new UnityEvent<SerializableGuid>();

	public override ToolType ToolType => ToolType.Prop;

	public SerializableGuid SelectedAssetId => base.ActiveAssetId;

	public SerializableGuid PreviousSelectedAssetId => base.PreviousAssetId;

	public override void HandleSelected()
	{
		base.HandleSelected();
		if (MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(SelectedAssetId, out var metadata))
		{
			GeneratePropPreview(metadata);
		}
		else
		{
			ClearActiveAssetId();
			OnSelectedAssetChanged?.Invoke(SerializableGuid.Empty);
		}
		Set3DCursorUsesIntersection(val: false);
		base.UIToolPrompter.Hide();
	}

	public override void HandleDeselected()
	{
		base.HandleDeselected();
		Set3DCursorUsesIntersection(val: false);
		if ((bool)scriptWindow)
		{
			scriptWindow.Close();
		}
	}

	public override void ToolPressed()
	{
		if (EndlessInput.GetKey(UnityEngine.InputSystem.Key.LeftAlt))
		{
			if (!(base.PropGhostTransform == null))
			{
				return;
			}
			LineCastHit activeLineCastResult = base.ActiveLineCastResult;
			if (!activeLineCastResult.IntersectionOccured)
			{
				return;
			}
			PropCell cellFromCoordinateAs = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetCellFromCoordinateAs<PropCell>(activeLineCastResult.IntersectedObjectPosition);
			if (cellFromCoordinateAs != null)
			{
				SerializableGuid assetIdFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetAssetIdFromInstanceId(cellFromCoordinateAs.InstanceId);
				if (MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(assetIdFromInstanceId, out var metadata))
				{
					GeneratePropPreview(metadata);
				}
			}
		}
		else
		{
			base.ToolPressed();
		}
	}

	public override void ToolReleased()
	{
		if (!base.IsLoadingProp && base.ToolIsPressed)
		{
			base.ToolReleased();
			if ((bool)base.PropGhostTransform && NoInvalidOverlapsExist())
			{
				AttemptPlaceProp_ServerRPC(base.PropGhostTransform.position, base.PropGhostTransform.rotation.eulerAngles, base.ActiveAssetId);
			}
		}
	}

	[ServerRpc(RequireOwnership = false)]
	public void AttemptPlaceProp_ServerRPC(Vector3 position, Vector3 eulerRotation, SerializableGuid assetId, ServerRpcParams serverRpcParams = default(ServerRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter bufferWriter = __beginSendServerRpc(2880201906u, serverRpcParams, RpcDelivery.Reliable);
				bufferWriter.WriteValueSafe(in position);
				bufferWriter.WriteValueSafe(in eulerRotation);
				bufferWriter.WriteValueSafe(in assetId, default(FastBufferWriter.ForNetworkSerializable));
				__endSendServerRpc(ref bufferWriter, 2880201906u, serverRpcParams, RpcDelivery.Reliable);
			}
			if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				__rpc_exec_stage = __RpcExecStage.Send;
				AttemptPlaceProp(position, eulerRotation, assetId, serverRpcParams);
			}
		}
	}

	private async void AttemptPlaceProp(Vector3 position, Vector3 eulerRotation, SerializableGuid assetId, ServerRpcParams serverRpcParams)
	{
		if (!base.IsServer)
		{
			return;
		}
		if (!NetworkBehaviourSingleton<UserIdManager>.Instance.TryGetUserId(serverRpcParams.Receive.SenderClientId, out var userId))
		{
			Debug.LogException(new Exception($"Unable to determine User Id for Client Id: {serverRpcParams.Receive.SenderClientId}"));
		}
		if (!(await NetworkBehaviourSingleton<CreatorManager>.Instance.UserCanEditLevel(userId)))
		{
			return;
		}
		PropLibrary.RuntimePropInfo metadata = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary[assetId];
		if (metadata == null)
		{
			Debug.LogException(new Exception("Server was told to spawn invalid asset ID!"));
			return;
		}
		if (metadata.IsLoading)
		{
			await MonoBehaviourSingleton<StageManager>.Instance.FetchAndSpawnPropPrefab(metadata.PropData.ToAssetReference());
		}
		if (metadata.IsMissingObject)
		{
			return;
		}
		bool num = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.PlacementIsValid(metadata.PropData, position, Quaternion.Euler(eulerRotation));
		bool flag = true;
		bool flag2 = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.CanAddProp(metadata);
		if (!(num && flag && flag2))
		{
			return;
		}
		FirePropPlacedAnalyticEvent(metadata.PropData.AssetID, metadata.PropData.Name);
		FirePropEvent("propToolPropPlaced", metadata.PropData.AssetID, metadata.PropData.Name);
		GameObject gameObject = UnityEngine.Object.Instantiate(metadata.EndlessProp, position, Quaternion.Euler(eulerRotation)).gameObject;
		SerializableGuid instanceId = SerializableGuid.NewGuid();
		ulong networkObjectId = 0uL;
		if (metadata.EndlessProp.IsNetworked)
		{
			NetworkObject component = gameObject.GetComponent<NetworkObject>();
			if (component != null)
			{
				component.Spawn();
				networkObjectId = component.NetworkObjectId;
			}
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.TrackPendingNetworkId(networkObjectId, instanceId);
		}
		MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.TrackNonNetworkedObject(assetId, instanceId, gameObject);
		PlaceProp_ClientRPC(position, eulerRotation, assetId, instanceId, networkObjectId);
		MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.RevisionTimestamp = DateTime.UtcNow.Ticks;
		MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.Changes.Add(new ChangeData
		{
			ChangeType = ChangeType.PropPainted,
			UserId = userId
		});
		IPropPlacedSubscriber[] componentsInChildren = gameObject.GetComponentsInChildren<IPropPlacedSubscriber>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].PropPlaced(instanceId, isCopy: false);
		}
		NetworkBehaviourSingleton<CreatorManager>.Instance.SaveLoadManager.SaveLevel();
	}

	[ClientRpc]
	public void PlaceProp_ClientRPC(Vector3 position, Vector3 eulerRotation, SerializableGuid assetId, SerializableGuid instanceId, ulong networkObjectId)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(47490796u, clientRpcParams, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in position);
			bufferWriter.WriteValueSafe(in eulerRotation);
			bufferWriter.WriteValueSafe(in assetId, default(FastBufferWriter.ForNetworkSerializable));
			bufferWriter.WriteValueSafe(in instanceId, default(FastBufferWriter.ForNetworkSerializable));
			BytePacker.WriteValueBitPacked(bufferWriter, networkObjectId);
			__endSendClientRpc(ref bufferWriter, 47490796u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
		{
			return;
		}
		__rpc_exec_stage = __RpcExecStage.Send;
		Vector3 position2 = position;
		Vector3 eulerRotation2 = eulerRotation;
		SerializableGuid assetId2 = assetId;
		SerializableGuid instanceId2 = instanceId;
		ulong networkObjectId2 = networkObjectId;
		if (base.IsServer)
		{
			return;
		}
		if (NetworkBehaviourSingleton<CreatorManager>.Instance.RpcReceiveState == RpcReceiveState.Allow)
		{
			PlaceProp(position2, eulerRotation2, assetId2, instanceId2, networkObjectId2);
			return;
		}
		NetworkBehaviourSingleton<CreatorManager>.Instance.AddCachedRpc(delegate
		{
			PlaceProp(position2, eulerRotation2, assetId2, instanceId2, networkObjectId2);
		});
	}

	private async void PlaceProp(Vector3 position, Vector3 eulerRotation, SerializableGuid assetId, SerializableGuid instanceId, ulong networkObjectId)
	{
		PropLibrary.RuntimePropInfo propInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary[assetId];
		if (propInfo.IsLoading)
		{
			await MonoBehaviourSingleton<StageManager>.Instance.FetchAndSpawnPropPrefab(propInfo.PropData.ToAssetReference());
		}
		if (networkObjectId == 0L)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(propInfo.EndlessProp).gameObject;
			gameObject.transform.position = position;
			gameObject.transform.rotation = Quaternion.Euler(eulerRotation);
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.TrackNonNetworkedObject(assetId, instanceId, gameObject);
		}
		else
		{
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.TrackPendingNetworkObject(networkObjectId, assetId, instanceId);
		}
	}

	public async void UpdateSelectedAssetId(SerializableGuid selectedAssetId)
	{
		if (base.Rotating)
		{
			EndRotating();
		}
		MonoBehaviourSingleton<MarkerManager>.Instance.ReleaseAllMarkers();
		if (selectedAssetId.IsEmpty || !MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(selectedAssetId, out var metadata))
		{
			if (!base.ActiveAssetId.IsEmpty)
			{
				DestroyPreview();
				ClearPreviousAssetId();
				OnSelectedAssetChanged.Invoke(selectedAssetId);
			}
			Set3DCursorUsesIntersection(val: true);
			return;
		}
		if ((bool)scriptWindow && base.ActiveAssetId != selectedAssetId && PreviousSelectedAssetId != selectedAssetId)
		{
			scriptWindow.Close();
		}
		SetPropDataFromPropInfo(metadata);
		if (metadata.IsLoading)
		{
			LoadPropPrefab(metadata);
		}
		if (!scriptWindow)
		{
			SpawnPreview(metadata);
		}
		OnSelectedAssetChanged.Invoke(selectedAssetId);
	}

	private async void LoadPropPrefab(PropLibrary.RuntimePropInfo runtimePropInfo)
	{
		if (!base.InFlightLoads.Add(runtimePropInfo))
		{
			return;
		}
		await MonoBehaviourSingleton<StageManager>.Instance.FetchAndSpawnPropPrefab(runtimePropInfo.PropData.ToAssetReference());
		base.InFlightLoads.Remove(runtimePropInfo);
		if (!scriptWindow && MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(runtimePropInfo.PropData.AssetID, out var metadata) && (SerializableGuid)metadata.PropData.AssetID == SelectedAssetId)
		{
			if (!metadata.IsMissingObject)
			{
				SpawnPreview(metadata);
			}
			else
			{
				SpawnPreview(runtimePropInfo);
			}
			OnSelectedAssetChanged.Invoke(runtimePropInfo.PropData.AssetID);
		}
	}

	public override void UpdateTool()
	{
		base.UpdateTool();
		if (base.PropGhostTransform == null && EndlessInput.GetKeyDown(UnityEngine.InputSystem.Key.LeftAlt))
		{
			Set3DCursorUsesIntersection(val: true);
		}
		if (EndlessInput.GetKeyUp(UnityEngine.InputSystem.Key.LeftAlt))
		{
			Set3DCursorUsesIntersection(val: false);
		}
		if (!base.IsMobile && base.ToolState == ToolState.None)
		{
			UpdatePropPlacement();
		}
	}

	public override void ToolSecondaryPressed()
	{
		base.ToolSecondaryPressed();
		OnSelectedAssetChanged?.Invoke(SerializableGuid.Empty);
	}

	public void EditScript(bool readOnly)
	{
		PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(base.ActiveAssetId);
		Script script = runtimePropInfo.EndlessProp.ScriptComponent.Script;
		DestroyPreview();
		scriptWindow = UIScriptWindowView.Display(script, runtimePropInfo, readOnly);
		scriptWindow.OnClosedUnityEvent.AddListener(OnScriptWindowClosed);
	}

	private void OnScriptWindowClosed(SerializableGuid propAssetId)
	{
		scriptWindow.OnClosedUnityEvent.RemoveListener(OnScriptWindowClosed);
		scriptWindow = null;
		if (base.IsActive && propAssetId == base.PreviousAssetId && (base.ActiveAssetId.IsEmpty || base.PreviousAssetId == base.ActiveAssetId))
		{
			PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(propAssetId);
			GeneratePropPreview(runtimePropInfo, propAssetId);
		}
	}

	protected override void __initializeVariables()
	{
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		__registerRpc(2880201906u, __rpc_handler_2880201906, "AttemptPlaceProp_ServerRPC");
		__registerRpc(47490796u, __rpc_handler_47490796, "PlaceProp_ClientRPC");
		base.__initializeRpcs();
	}

	private static void __rpc_handler_2880201906(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out Vector3 value);
			reader.ReadValueSafe(out Vector3 value2);
			reader.ReadValueSafe(out SerializableGuid value3, default(FastBufferWriter.ForNetworkSerializable));
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((PropTool)target).AttemptPlaceProp_ServerRPC(value, value2, value3, server);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_47490796(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out Vector3 value);
			reader.ReadValueSafe(out Vector3 value2);
			reader.ReadValueSafe(out SerializableGuid value3, default(FastBufferWriter.ForNetworkSerializable));
			reader.ReadValueSafe(out SerializableGuid value4, default(FastBufferWriter.ForNetworkSerializable));
			ByteUnpacker.ReadValueBitPacked(reader, out ulong value5);
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((PropTool)target).PlaceProp_ClientRPC(value, value2, value3, value4, value5);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	protected internal override string __getTypeName()
	{
		return "PropTool";
	}
}
