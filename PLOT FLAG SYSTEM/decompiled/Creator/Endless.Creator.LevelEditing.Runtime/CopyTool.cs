using System;
using System.Collections.Generic;
using Endless.Assets;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using Unity.Services.Analytics;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.LevelEditing.Runtime;

public class CopyTool : PropBasedTool
{
	public UnityEvent<ClientCopyHistoryEntry> CopyHistoryEntryInserted = new UnityEvent<ClientCopyHistoryEntry>();

	public UnityEvent<int> CopyHistoryTrimmed = new UnityEvent<int>();

	public UnityEvent CopyHistoryCleared = new UnityEvent();

	public UnityEvent<int> OnSelectedCopyIndexSet = new UnityEvent<int>();

	private const int SELECTING_PROP_TO_COPY = -1;

	[SerializeField]
	private int maximumHistoryEntries = 9;

	private SerializableGuid copiedInstanceId = SerializableGuid.Empty;

	private SerializableGuid hoveredPropId = SerializableGuid.Empty;

	private int selectedIndex = -1;

	public override ToolType ToolType => ToolType.Copy;

	public List<ClientCopyHistoryEntry> ClientCopyHistoryEntries { get; } = new List<ClientCopyHistoryEntry>();

	public Dictionary<ulong, List<ServerCopyHistoryEntry>> ServerCopyHistoryPerPlayer { get; } = new Dictionary<ulong, List<ServerCopyHistoryEntry>>();

	public int SelectedIndex => selectedIndex;

	public override void SessionEnded()
	{
		CopyHistoryCleared.Invoke();
		ClientCopyHistoryEntries.Clear();
		ResetToolState();
	}

	public override void HandleSelected()
	{
		base.HandleSelected();
		Set3DCursorUsesIntersection(val: true);
		base.UIToolPrompter.Hide();
	}

	public override void HandleDeselected()
	{
		base.HandleDeselected();
		ResetToolState();
	}

	public override void CreatorExited()
	{
		ResetToolState();
	}

	public override void Reset()
	{
		base.Reset();
		ResetToolState();
	}

	public override void ToolPressed()
	{
		if (selectedIndex != -1)
		{
			base.ToolPressed();
		}
	}

	private void ResetToolState()
	{
		Set3DCursorUsesIntersection(val: true);
		selectedIndex = -1;
		if (hoveredPropId != SerializableGuid.Empty)
		{
			MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(hoveredPropId, PropLocationType.Selected);
			hoveredPropId = SerializableGuid.Empty;
		}
		if (copiedInstanceId != SerializableGuid.Empty)
		{
			MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(copiedInstanceId, PropLocationType.Selected);
			copiedInstanceId = SerializableGuid.Empty;
		}
		DestroyPreview();
	}

	public void ClearCopyHistory()
	{
		ClientCopyHistoryEntries.Clear();
		ClearCopyHistoryForPlayer_ServerRpc();
	}

	[ServerRpc(RequireOwnership = false)]
	private void ClearCopyHistoryForPlayer_ServerRpc(ServerRpcParams rpcParams = default(ServerRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			FastBufferWriter bufferWriter = __beginSendServerRpc(3657533409u, rpcParams, RpcDelivery.Reliable);
			__endSendServerRpc(ref bufferWriter, 3657533409u, rpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			ulong senderClientId = rpcParams.Receive.SenderClientId;
			if (!ServerCopyHistoryPerPlayer.ContainsKey(senderClientId))
			{
				Debug.LogException(new Exception($"Attempting to clear copy history for player {senderClientId}, but that player has no copy history on server!"));
			}
			else
			{
				ServerCopyHistoryPerPlayer[senderClientId].Clear();
			}
		}
	}

	public void SetSelectedCopyIndex(int index)
	{
		selectedIndex = index;
		if (base.PropGhostTransform != null)
		{
			MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(copiedInstanceId, PropLocationType.Selected);
			copiedInstanceId = SerializableGuid.Empty;
			DestroyPreview();
		}
		if (selectedIndex < 0 || selectedIndex >= maximumHistoryEntries)
		{
			Set3DCursorUsesIntersection(val: true);
			if (selectedIndex >= maximumHistoryEntries)
			{
				Debug.LogException(new Exception($"Somehow we attempted to set the selected index to a number greater than the maximum history stored! Requested Selected Index: {selectedIndex}, Maximum History: {maximumHistoryEntries}"));
			}
			if (base.PropGhostTransform != null)
			{
				MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(copiedInstanceId, PropLocationType.Selected);
				copiedInstanceId = SerializableGuid.Empty;
				DestroyPreview();
			}
		}
		else
		{
			Set3DCursorUsesIntersection(val: false);
			ClientCopyHistoryEntry clientCopyHistoryEntry = ClientCopyHistoryEntries[index];
			copiedInstanceId = clientCopyHistoryEntry.InstanceId;
			MonoBehaviourSingleton<PropLocationMarker>.Instance.MarkCellsForProp(copiedInstanceId, PropLocationType.Selected);
			PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(clientCopyHistoryEntry.AssetId);
			GeneratePropPreview(runtimePropInfo);
			base.PropGhostTransform.rotation = clientCopyHistoryEntry.Rotation;
			OnSelectedCopyIndexSet.Invoke(index);
		}
	}

	public void RemoveIndex(int index)
	{
		if (index >= 0 && index < ClientCopyHistoryEntries.Count)
		{
			ClientCopyHistoryEntries.RemoveAt(index);
			RemoveCopyHistoryAtIndex_ServerRpc(index);
			if (ClientCopyHistoryEntries.Count == 0)
			{
				ResetToolState();
			}
			else if (index == selectedIndex)
			{
				SetSelectedCopyIndex((index - 1 >= 0) ? (index - 1) : index);
			}
		}
	}

	[ServerRpc(RequireOwnership = false)]
	private void RemoveCopyHistoryAtIndex_ServerRpc(int index, ServerRpcParams rpcParams = default(ServerRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			FastBufferWriter bufferWriter = __beginSendServerRpc(2768657413u, rpcParams, RpcDelivery.Reliable);
			BytePacker.WriteValueBitPacked(bufferWriter, index);
			__endSendServerRpc(ref bufferWriter, 2768657413u, rpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
		{
			return;
		}
		__rpc_exec_stage = __RpcExecStage.Send;
		ulong senderClientId = rpcParams.Receive.SenderClientId;
		if (index < 0)
		{
			Debug.LogException(new Exception($"Attempted to remove copy history at a less than zero index, which should mean the player (player id: {senderClientId}) is in prop selection state, not paste state."));
			return;
		}
		if (!ServerCopyHistoryPerPlayer.ContainsKey(senderClientId))
		{
			Debug.LogException(new Exception($"Attempting to remove copy history for player {senderClientId} (index {index}), but that player has no copy history on server!"));
			return;
		}
		List<ServerCopyHistoryEntry> list = ServerCopyHistoryPerPlayer[senderClientId];
		if (list.Count < index)
		{
			Debug.LogException(new Exception($"Attempted to remove copy history for an index ({index}) that didn't exist in the players (player id: {senderClientId}) copy history. Attempted to paste index {index} but only {list.Count} entries exist."));
		}
		else
		{
			list.RemoveAt(index);
		}
	}

	public void UpdateCopyLabel(string newLabel, int index)
	{
		if (index < 0)
		{
			Debug.LogWarning($"Attempted to set a new label for an index less than zero. New Label: {newLabel}, Index: {index}");
			return;
		}
		if (index >= maximumHistoryEntries)
		{
			Debug.LogWarning($"Attempted to set a new label for an index exceeding the maximum history. New Label: {newLabel}, Index: {index}");
			return;
		}
		if (index > ClientCopyHistoryEntries.Count)
		{
			Debug.LogWarning($"Attempted to set a new label for an index exceeding the current history count. New Label: {newLabel}, Desired Index: {index}, Current History Count: {ClientCopyHistoryEntries.Count}");
			return;
		}
		ClientCopyHistoryEntries[index].Label = newLabel;
		SetCopyHistoryLabel_ServerRpc(newLabel, index);
	}

	[ServerRpc(RequireOwnership = false)]
	private void SetCopyHistoryLabel_ServerRpc(string newLabel, int index, ServerRpcParams rpcParams = default(ServerRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			FastBufferWriter bufferWriter = __beginSendServerRpc(1537653066u, rpcParams, RpcDelivery.Reliable);
			bool value = newLabel != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(newLabel);
			}
			BytePacker.WriteValueBitPacked(bufferWriter, index);
			__endSendServerRpc(ref bufferWriter, 1537653066u, rpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
		{
			return;
		}
		__rpc_exec_stage = __RpcExecStage.Send;
		ulong senderClientId = rpcParams.Receive.SenderClientId;
		if (index < 0)
		{
			Debug.LogException(new Exception($"Attempted to rename copy history at a less than zero index, which should mean the player (player id: {senderClientId}) is in prop selection state, not paste state."));
			return;
		}
		if (!ServerCopyHistoryPerPlayer.ContainsKey(senderClientId))
		{
			Debug.LogException(new Exception($"Attempting to rename copy history for player {senderClientId} (index {index}), but that player has no copy history on server!"));
			return;
		}
		List<ServerCopyHistoryEntry> list = ServerCopyHistoryPerPlayer[senderClientId];
		if (list.Count < index)
		{
			Debug.LogException(new Exception($"Attempted to rename copy history for an index ({index}) that didn't exist in the players (player id: {senderClientId}) copy history. Attempted to paste index {index} but only {list.Count} entries exist."));
		}
		else
		{
			list[index].Prop.Label = newLabel;
		}
	}

	public override void ToolReleased()
	{
		base.ToolReleased();
		if (copiedInstanceId == SerializableGuid.Empty)
		{
			AttemptSelectPropToCopy();
		}
		else
		{
			AttemptPasteCopyOfPropInstance();
		}
	}

	private void AttemptSelectPropToCopy()
	{
		LineCastHit activeLineCastResult = base.ActiveLineCastResult;
		if (activeLineCastResult.IntersectionOccured)
		{
			PropCell cellFromCoordinateAs = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetCellFromCoordinateAs<PropCell>(activeLineCastResult.IntersectedObjectPosition);
			if (cellFromCoordinateAs != null)
			{
				copiedInstanceId = cellFromCoordinateAs.InstanceId;
				MonoBehaviourSingleton<PropLocationMarker>.Instance.MarkCellsForProp(copiedInstanceId, PropLocationType.Selected);
				SerializableGuid assetIdFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetAssetIdFromInstanceId(copiedInstanceId);
				PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(assetIdFromInstanceId);
				GeneratePropPreview(runtimePropInfo);
				selectedIndex = 0;
				Set3DCursorUsesIntersection(val: false);
				GameObject gameObjectFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(copiedInstanceId);
				base.PropGhostTransform.rotation = gameObjectFromInstanceId.transform.rotation;
				ClientCopyHistoryEntries.Insert(0, new ClientCopyHistoryEntry
				{
					Label = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.GetPropEntry(copiedInstanceId).Label,
					InstanceId = copiedInstanceId,
					AssetId = assetIdFromInstanceId,
					Rotation = (gameObjectFromInstanceId ? gameObjectFromInstanceId.transform.rotation : Quaternion.identity)
				});
				CopyHistoryEntryInserted.Invoke(ClientCopyHistoryEntries[selectedIndex]);
				TrimHistoryIfNecessary(ClientCopyHistoryEntries);
				AddCopyHistoryFromObject_ServerRpc(copiedInstanceId);
			}
		}
	}

	[ServerRpc(RequireOwnership = false)]
	private void AddCopyHistoryFromObject_ServerRpc(SerializableGuid instanceId, ServerRpcParams rpcParams = default(ServerRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			FastBufferWriter bufferWriter = __beginSendServerRpc(2516555771u, rpcParams, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in instanceId, default(FastBufferWriter.ForNetworkSerializable));
			__endSendServerRpc(ref bufferWriter, 2516555771u, rpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
		{
			return;
		}
		__rpc_exec_stage = __RpcExecStage.Send;
		PropEntry propEntry = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.CopyProp(instanceId);
		if (propEntry != null)
		{
			ServerCopyHistoryEntry serverCopyHistoryEntry = new ServerCopyHistoryEntry();
			serverCopyHistoryEntry.Prop = propEntry;
			(List<WireBundle>, List<WireBundle>) bundlesWiredToInstance = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.GetBundlesWiredToInstance(instanceId);
			List<WireBundle> item = bundlesWiredToInstance.Item1;
			List<WireBundle> item2 = bundlesWiredToInstance.Item2;
			serverCopyHistoryEntry.EmitterWireBundles = item;
			serverCopyHistoryEntry.ReceiverWireBundles = item2;
			ulong senderClientId = rpcParams.Receive.SenderClientId;
			if (!ServerCopyHistoryPerPlayer.ContainsKey(senderClientId))
			{
				ServerCopyHistoryPerPlayer.Add(senderClientId, new List<ServerCopyHistoryEntry>());
			}
			try
			{
				PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(serverCopyHistoryEntry.Prop.AssetId);
				FirePropEvent("propCopied", runtimePropInfo.PropData.AssetID, runtimePropInfo.PropData.Name);
			}
			catch (Exception innerException)
			{
				Debug.LogException(new Exception("Error with copy tool analyitcs", innerException));
			}
			ServerCopyHistoryPerPlayer[senderClientId].Insert(0, serverCopyHistoryEntry);
			TrimHistoryIfNecessary(ServerCopyHistoryPerPlayer[senderClientId], emitTrimEvent: false);
		}
	}

	private void TrimHistoryIfNecessary<T>(List<T> history, bool emitTrimEvent = true)
	{
		if (history.Count >= maximumHistoryEntries)
		{
			int num = 0;
			for (int num2 = history.Count - 1; num2 >= maximumHistoryEntries; num2--)
			{
				num++;
				history.RemoveAt(num2);
			}
			if (emitTrimEvent && base.IsClient)
			{
				CopyHistoryTrimmed.Invoke(num);
			}
		}
	}

	private void AttemptPasteCopyOfPropInstance()
	{
		if ((bool)base.PropGhostTransform && NoInvalidOverlapsExist())
		{
			AttemptCopyProp_ServerRpc(selectedIndex, base.PropGhostTransform.position, base.PropGhostTransform.rotation.eulerAngles);
		}
	}

	[ServerRpc(RequireOwnership = false)]
	private void AttemptCopyProp_ServerRpc(int index, Vector3 position, Vector3 eulerAngles, ServerRpcParams rpcParams = default(ServerRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			FastBufferWriter bufferWriter = __beginSendServerRpc(3110554693u, rpcParams, RpcDelivery.Reliable);
			BytePacker.WriteValueBitPacked(bufferWriter, index);
			bufferWriter.WriteValueSafe(in position);
			bufferWriter.WriteValueSafe(in eulerAngles);
			__endSendServerRpc(ref bufferWriter, 3110554693u, rpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			ulong senderClientId = rpcParams.Receive.SenderClientId;
			if (!NetworkBehaviourSingleton<UserIdManager>.Instance.TryGetUserId(senderClientId, out var userId))
			{
				Debug.LogException(new Exception($"Unable to determine User Id for Client Id: {senderClientId}"));
			}
			AttemptCopyProp_Execute(index, position, eulerAngles, userId, senderClientId);
		}
	}

	private async void AttemptCopyProp_Execute(int index, Vector3 position, Vector3 eulerAngles, int userId, ulong playerId)
	{
		if (!(await NetworkBehaviourSingleton<CreatorManager>.Instance.UserCanEditLevel(userId)))
		{
			return;
		}
		if (index < 0)
		{
			Debug.LogException(new Exception($"Attempted to place a less than zero index, which should mean the player (player id: {playerId}) is in prop selection state, not paste state."));
			return;
		}
		if (!ServerCopyHistoryPerPlayer.ContainsKey(playerId))
		{
			Debug.LogException(new Exception($"Attempting to paste a prop for player {playerId}, but that player has no copy history on server!"));
			return;
		}
		List<ServerCopyHistoryEntry> list = ServerCopyHistoryPerPlayer[playerId];
		if (list.Count < index)
		{
			Debug.LogException(new Exception($"Attempted to paste an index that didn't exist in the players (player id: {playerId}) copy history. Attempted to paste index {index} but only {list.Count} entries exist."));
			return;
		}
		ServerCopyHistoryEntry serverCopyHistoryEntry = list[index];
		PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(serverCopyHistoryEntry.Prop.AssetId);
		if (runtimePropInfo == null)
		{
			Debug.LogException(new Exception("Server was told to spawn invalid asset ID!"));
			return;
		}
		bool num = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.PlacementIsValid(runtimePropInfo.PropData, position, Quaternion.Euler(eulerAngles));
		bool flag = true;
		bool flag2 = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.CanAddProp(runtimePropInfo);
		if (!(num && flag && flag2))
		{
			return;
		}
		FirePropPlacedAnalyticEvent(runtimePropInfo.PropData.AssetID, runtimePropInfo.PropData.Name);
		GameObject gameObject = UnityEngine.Object.Instantiate(runtimePropInfo.EndlessProp, position, Quaternion.Euler(eulerAngles)).gameObject;
		SerializableGuid serializableGuid = SerializableGuid.NewGuid();
		ulong networkObjectId = 0uL;
		if (runtimePropInfo.EndlessProp.IsNetworked)
		{
			NetworkObject component = gameObject.GetComponent<NetworkObject>();
			if (component != null)
			{
				component.Spawn();
				networkObjectId = component.NetworkObjectId;
			}
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.TrackPendingNetworkId(networkObjectId, serializableGuid);
		}
		MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.TrackNonNetworkedObject(serverCopyHistoryEntry.Prop.AssetId, serializableGuid, gameObject);
		PropEntry propEntry = serverCopyHistoryEntry.Prop.CopyWithNewInstanceId(serializableGuid);
		propEntry.Position = position;
		propEntry.Rotation = Quaternion.Euler(eulerAngles);
		MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.AddOrUpdatePropWithInstance(propEntry);
		WireBundle[] array = serverCopyHistoryEntry.CopyEmitterBundles(serializableGuid);
		WireBundle[] array2 = serverCopyHistoryEntry.CopyReceiverBundles(serializableGuid);
		MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.InsertWireBundles(array);
		MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.InsertWireBundles(array2);
		bool flag3 = false;
		foreach (WireBundle wireBundle in MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.WireBundles)
		{
			if (wireBundle.EmitterInstanceId == serializableGuid || wireBundle.ReceiverInstanceId == serializableGuid)
			{
				MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.SpawnPhysicalWire(wireBundle.BundleId, wireBundle.EmitterInstanceId, wireBundle.ReceiverInstanceId, wireBundle.RerouteNodeIds, wireBundle.WireColor);
				try
				{
					SerializableGuid assetIdFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetAssetIdFromInstanceId(wireBundle.EmitterInstanceId);
					PropLibrary.RuntimePropInfo runtimePropInfo2 = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(assetIdFromInstanceId);
					SerializableGuid assetIdFromInstanceId2 = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetAssetIdFromInstanceId(wireBundle.ReceiverInstanceId);
					PropLibrary.RuntimePropInfo runtimePropInfo3 = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(assetIdFromInstanceId2);
					CustomEvent e = new CustomEvent("wireDuplicated")
					{
						{
							"propName1",
							runtimePropInfo2.PropData.Name
						},
						{
							"propId1",
							assetIdFromInstanceId.ToString()
						},
						{
							"propName2",
							runtimePropInfo3.PropData.Name
						},
						{
							"propId2",
							assetIdFromInstanceId2.ToString()
						}
					};
					AnalyticsService.Instance.RecordEvent(e);
				}
				catch (Exception innerException)
				{
					Debug.LogException(new Exception("Wire copy analytics event error", innerException));
				}
			}
		}
		CustomEvent e2 = new CustomEvent("copyToolPropPasted")
		{
			{
				"propName1",
				runtimePropInfo.PropData.Name
			},
			{
				"propId1",
				runtimePropInfo.PropData.AssetID
			},
			{ "pastedWires", flag3 }
		};
		AnalyticsService.Instance.RecordEvent(e2);
		Stage.ApplyMemberChanges(propEntry);
		MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LoadExistingWires();
		CopyProp_ClientRpc(position, eulerAngles, propEntry, array, array2, networkObjectId);
		MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.RevisionTimestamp = DateTime.UtcNow.Ticks;
		MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.Changes.Add(new ChangeData
		{
			ChangeType = ChangeType.PropPasted,
			UserId = userId
		});
		IPropPlacedSubscriber[] componentsInChildren = gameObject.GetComponentsInChildren<IPropPlacedSubscriber>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].PropPlaced(serializableGuid, isCopy: true);
		}
		NetworkBehaviourSingleton<CreatorManager>.Instance.SaveLoadManager.SaveLevel();
	}

	[ClientRpc]
	private void CopyProp_ClientRpc(Vector3 position, Vector3 eulerAngles, PropEntry copy, WireBundle[] emitterWireBundles, WireBundle[] receiverWireBundles, ulong networkObjectId)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(3984099148u, clientRpcParams, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in position);
			bufferWriter.WriteValueSafe(in eulerAngles);
			bool value = copy != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(in copy, default(FastBufferWriter.ForNetworkSerializable));
			}
			bool value2 = emitterWireBundles != null;
			bufferWriter.WriteValueSafe(in value2, default(FastBufferWriter.ForPrimitives));
			if (value2)
			{
				bufferWriter.WriteValueSafe(emitterWireBundles, default(FastBufferWriter.ForNetworkSerializable));
			}
			bool value3 = receiverWireBundles != null;
			bufferWriter.WriteValueSafe(in value3, default(FastBufferWriter.ForPrimitives));
			if (value3)
			{
				bufferWriter.WriteValueSafe(receiverWireBundles, default(FastBufferWriter.ForNetworkSerializable));
			}
			BytePacker.WriteValueBitPacked(bufferWriter, networkObjectId);
			__endSendClientRpc(ref bufferWriter, 3984099148u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
		{
			return;
		}
		__rpc_exec_stage = __RpcExecStage.Send;
		Vector3 position2 = position;
		Vector3 eulerAngles2 = eulerAngles;
		PropEntry copy2 = copy;
		WireBundle[] emitterWireBundles2 = emitterWireBundles;
		WireBundle[] receiverWireBundles2 = receiverWireBundles;
		ulong networkObjectId2 = networkObjectId;
		if (base.IsServer)
		{
			return;
		}
		if (NetworkBehaviourSingleton<CreatorManager>.Instance.RpcReceiveState == RpcReceiveState.Allow)
		{
			CopyProp(position2, eulerAngles2, copy2, emitterWireBundles2, receiverWireBundles2, networkObjectId2);
			return;
		}
		NetworkBehaviourSingleton<CreatorManager>.Instance.AddCachedRpc(delegate
		{
			CopyProp(position2, eulerAngles2, copy2, emitterWireBundles2, receiverWireBundles2, networkObjectId2);
		});
	}

	private void CopyProp(Vector3 position, Vector3 eulerAngles, PropEntry copy, WireBundle[] emitterWireBundles, WireBundle[] receiverWireBundles, ulong networkObjectId)
	{
		PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(copy.AssetId);
		bool wireToolIsActive = MonoBehaviourSingleton<ToolManager>.Instance.ActiveTool is WiringTool;
		if (networkObjectId == 0L)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(runtimePropInfo.EndlessProp).gameObject;
			gameObject.transform.position = position;
			gameObject.transform.rotation = Quaternion.Euler(eulerAngles);
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.TrackNonNetworkedObject(copy.AssetId, copy.InstanceId, gameObject);
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.AddOrUpdatePropWithInstance(copy);
			foreach (WireBundle wireBundle in MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.WireBundles)
			{
				if (wireBundle.EmitterInstanceId == copy.InstanceId || wireBundle.ReceiverInstanceId == copy.InstanceId)
				{
					MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.SpawnPhysicalWire(wireBundle.BundleId, wireBundle.EmitterInstanceId, wireBundle.ReceiverInstanceId, wireBundle.RerouteNodeIds, wireBundle.WireColor, wireToolIsActive);
				}
			}
			Stage.ApplyMemberChanges(copy);
		}
		else
		{
			PropEntry propEntry;
			bool flag = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.TryGetPropEntry(copy.InstanceId, out propEntry);
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.TrackPendingNetworkObject(networkObjectId, copy.AssetId, copy.InstanceId, !flag, delegate
			{
				MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.AddOrUpdatePropWithInstance(copy);
				Stage.ApplyMemberChanges(copy);
				foreach (WireBundle wireBundle2 in MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.WireBundles)
				{
					if (wireBundle2.EmitterInstanceId == copy.InstanceId || wireBundle2.ReceiverInstanceId == copy.InstanceId)
					{
						MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.SpawnPhysicalWire(wireBundle2.BundleId, wireBundle2.EmitterInstanceId, wireBundle2.ReceiverInstanceId, wireBundle2.RerouteNodeIds, wireBundle2.WireColor, wireToolIsActive);
					}
				}
			});
		}
		MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.InsertWireBundles(emitterWireBundles);
		MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.InsertWireBundles(receiverWireBundles);
		MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LoadExistingWires(wireToolIsActive);
	}

	public override void UpdateTool()
	{
		base.UpdateTool();
		if (selectedIndex == -1)
		{
			UpdateHighlightState();
		}
		else if (!base.IsMobile)
		{
			UpdatePropPlacement();
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
				if (hoveredPropId != copiedInstanceId)
				{
					MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(hoveredPropId, PropLocationType.Selected);
				}
				hoveredPropId = SerializableGuid.Empty;
			}
		}
		else if (!hoveredPropId.IsEmpty)
		{
			if (hoveredPropId != copiedInstanceId)
			{
				MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(hoveredPropId, PropLocationType.Selected);
			}
			hoveredPropId = SerializableGuid.Empty;
		}
	}

	public override void ToolSecondaryPressed()
	{
		base.ToolSecondaryPressed();
		selectedIndex = -1;
		ResetToolState();
	}

	protected override void __initializeVariables()
	{
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		__registerRpc(3657533409u, __rpc_handler_3657533409, "ClearCopyHistoryForPlayer_ServerRpc");
		__registerRpc(2768657413u, __rpc_handler_2768657413, "RemoveCopyHistoryAtIndex_ServerRpc");
		__registerRpc(1537653066u, __rpc_handler_1537653066, "SetCopyHistoryLabel_ServerRpc");
		__registerRpc(2516555771u, __rpc_handler_2516555771, "AddCopyHistoryFromObject_ServerRpc");
		__registerRpc(3110554693u, __rpc_handler_3110554693, "AttemptCopyProp_ServerRpc");
		__registerRpc(3984099148u, __rpc_handler_3984099148, "CopyProp_ClientRpc");
		base.__initializeRpcs();
	}

	private static void __rpc_handler_3657533409(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((CopyTool)target).ClearCopyHistoryForPlayer_ServerRpc(server);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_2768657413(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			ByteUnpacker.ReadValueBitPacked(reader, out int value);
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((CopyTool)target).RemoveCopyHistoryAtIndex_ServerRpc(value, server);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_1537653066(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
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
			ByteUnpacker.ReadValueBitPacked(reader, out int value2);
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((CopyTool)target).SetCopyHistoryLabel_ServerRpc(s, value2, server);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_2516555771(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out SerializableGuid value, default(FastBufferWriter.ForNetworkSerializable));
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((CopyTool)target).AddCopyHistoryFromObject_ServerRpc(value, server);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_3110554693(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			ByteUnpacker.ReadValueBitPacked(reader, out int value);
			reader.ReadValueSafe(out Vector3 value2);
			reader.ReadValueSafe(out Vector3 value3);
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((CopyTool)target).AttemptCopyProp_ServerRpc(value, value2, value3, server);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_3984099148(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out Vector3 value);
			reader.ReadValueSafe(out Vector3 value2);
			reader.ReadValueSafe(out bool value3, default(FastBufferWriter.ForPrimitives));
			PropEntry value4 = null;
			if (value3)
			{
				reader.ReadValueSafe(out value4, default(FastBufferWriter.ForNetworkSerializable));
			}
			reader.ReadValueSafe(out bool value5, default(FastBufferWriter.ForPrimitives));
			WireBundle[] value6 = null;
			if (value5)
			{
				reader.ReadValueSafe(out value6, default(FastBufferWriter.ForNetworkSerializable));
			}
			reader.ReadValueSafe(out bool value7, default(FastBufferWriter.ForPrimitives));
			WireBundle[] value8 = null;
			if (value7)
			{
				reader.ReadValueSafe(out value8, default(FastBufferWriter.ForNetworkSerializable));
			}
			ByteUnpacker.ReadValueBitPacked(reader, out ulong value9);
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((CopyTool)target).CopyProp_ClientRpc(value, value2, value4, value6, value8, value9);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	protected internal override string __getTypeName()
	{
		return "CopyTool";
	}
}
