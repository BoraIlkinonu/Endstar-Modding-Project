using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Endless.Assets;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.Scripting;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using Unity.Services.Analytics;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.LevelEditing.Runtime
{
	// Token: 0x02000347 RID: 839
	public class CopyTool : PropBasedTool
	{
		// Token: 0x17000260 RID: 608
		// (get) Token: 0x06000F94 RID: 3988 RVA: 0x00047D07 File Offset: 0x00045F07
		public override ToolType ToolType
		{
			get
			{
				return ToolType.Copy;
			}
		}

		// Token: 0x17000261 RID: 609
		// (get) Token: 0x06000F95 RID: 3989 RVA: 0x00047D0A File Offset: 0x00045F0A
		public List<ClientCopyHistoryEntry> ClientCopyHistoryEntries { get; } = new List<ClientCopyHistoryEntry>();

		// Token: 0x17000262 RID: 610
		// (get) Token: 0x06000F96 RID: 3990 RVA: 0x00047D12 File Offset: 0x00045F12
		public Dictionary<ulong, List<ServerCopyHistoryEntry>> ServerCopyHistoryPerPlayer { get; } = new Dictionary<ulong, List<ServerCopyHistoryEntry>>();

		// Token: 0x17000263 RID: 611
		// (get) Token: 0x06000F97 RID: 3991 RVA: 0x00047D1A File Offset: 0x00045F1A
		public int SelectedIndex
		{
			get
			{
				return this.selectedIndex;
			}
		}

		// Token: 0x06000F98 RID: 3992 RVA: 0x00047D22 File Offset: 0x00045F22
		public override void SessionEnded()
		{
			this.CopyHistoryCleared.Invoke();
			this.ClientCopyHistoryEntries.Clear();
			this.ResetToolState();
		}

		// Token: 0x06000F99 RID: 3993 RVA: 0x00047D40 File Offset: 0x00045F40
		public override void HandleSelected()
		{
			base.HandleSelected();
			base.Set3DCursorUsesIntersection(true);
			base.UIToolPrompter.Hide();
		}

		// Token: 0x06000F9A RID: 3994 RVA: 0x00047D5A File Offset: 0x00045F5A
		public override void HandleDeselected()
		{
			base.HandleDeselected();
			this.ResetToolState();
		}

		// Token: 0x06000F9B RID: 3995 RVA: 0x00047D68 File Offset: 0x00045F68
		public override void CreatorExited()
		{
			this.ResetToolState();
		}

		// Token: 0x06000F9C RID: 3996 RVA: 0x00047D70 File Offset: 0x00045F70
		public override void Reset()
		{
			base.Reset();
			this.ResetToolState();
		}

		// Token: 0x06000F9D RID: 3997 RVA: 0x00047D7E File Offset: 0x00045F7E
		public override void ToolPressed()
		{
			if (this.selectedIndex != -1)
			{
				base.ToolPressed();
			}
		}

		// Token: 0x06000F9E RID: 3998 RVA: 0x00047D90 File Offset: 0x00045F90
		private void ResetToolState()
		{
			base.Set3DCursorUsesIntersection(true);
			this.selectedIndex = -1;
			if (this.hoveredPropId != SerializableGuid.Empty)
			{
				MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(this.hoveredPropId, PropLocationType.Selected);
				this.hoveredPropId = SerializableGuid.Empty;
			}
			if (this.copiedInstanceId != SerializableGuid.Empty)
			{
				MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(this.copiedInstanceId, PropLocationType.Selected);
				this.copiedInstanceId = SerializableGuid.Empty;
			}
			base.DestroyPreview();
		}

		// Token: 0x06000F9F RID: 3999 RVA: 0x00047E10 File Offset: 0x00046010
		public void ClearCopyHistory()
		{
			this.ClientCopyHistoryEntries.Clear();
			this.ClearCopyHistoryForPlayer_ServerRpc(default(ServerRpcParams));
		}

		// Token: 0x06000FA0 RID: 4000 RVA: 0x00047E38 File Offset: 0x00046038
		[ServerRpc(RequireOwnership = false)]
		private void ClearCopyHistoryForPlayer_ServerRpc(ServerRpcParams rpcParams = default(ServerRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(3657533409U, rpcParams, RpcDelivery.Reliable);
				base.__endSendServerRpc(ref fastBufferWriter, 3657533409U, rpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			ulong senderClientId = rpcParams.Receive.SenderClientId;
			if (!this.ServerCopyHistoryPerPlayer.ContainsKey(senderClientId))
			{
				Debug.LogException(new Exception(string.Format("Attempting to clear copy history for player {0}, but that player has no copy history on server!", senderClientId)));
				return;
			}
			this.ServerCopyHistoryPerPlayer[senderClientId].Clear();
		}

		// Token: 0x06000FA1 RID: 4001 RVA: 0x00047F54 File Offset: 0x00046154
		public void SetSelectedCopyIndex(int index)
		{
			this.selectedIndex = index;
			if (base.PropGhostTransform != null)
			{
				MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(this.copiedInstanceId, PropLocationType.Selected);
				this.copiedInstanceId = SerializableGuid.Empty;
				base.DestroyPreview();
			}
			if (this.selectedIndex < 0 || this.selectedIndex >= this.maximumHistoryEntries)
			{
				base.Set3DCursorUsesIntersection(true);
				if (this.selectedIndex >= this.maximumHistoryEntries)
				{
					Debug.LogException(new Exception(string.Format("Somehow we attempted to set the selected index to a number greater than the maximum history stored! Requested Selected Index: {0}, Maximum History: {1}", this.selectedIndex, this.maximumHistoryEntries)));
				}
				if (base.PropGhostTransform != null)
				{
					MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(this.copiedInstanceId, PropLocationType.Selected);
					this.copiedInstanceId = SerializableGuid.Empty;
					base.DestroyPreview();
				}
				return;
			}
			base.Set3DCursorUsesIntersection(false);
			ClientCopyHistoryEntry clientCopyHistoryEntry = this.ClientCopyHistoryEntries[index];
			this.copiedInstanceId = clientCopyHistoryEntry.InstanceId;
			MonoBehaviourSingleton<PropLocationMarker>.Instance.MarkCellsForProp(this.copiedInstanceId, PropLocationType.Selected);
			PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(clientCopyHistoryEntry.AssetId);
			base.GeneratePropPreview(runtimePropInfo, default(SerializableGuid));
			base.PropGhostTransform.rotation = clientCopyHistoryEntry.Rotation;
			this.OnSelectedCopyIndexSet.Invoke(index);
		}

		// Token: 0x06000FA2 RID: 4002 RVA: 0x00048094 File Offset: 0x00046294
		public void RemoveIndex(int index)
		{
			if (index < 0 || index >= this.ClientCopyHistoryEntries.Count)
			{
				return;
			}
			this.ClientCopyHistoryEntries.RemoveAt(index);
			this.RemoveCopyHistoryAtIndex_ServerRpc(index, default(ServerRpcParams));
			if (this.ClientCopyHistoryEntries.Count == 0)
			{
				this.ResetToolState();
				return;
			}
			if (index == this.selectedIndex)
			{
				this.SetSelectedCopyIndex((index - 1 >= 0) ? (index - 1) : index);
			}
		}

		// Token: 0x06000FA3 RID: 4003 RVA: 0x00048100 File Offset: 0x00046300
		[ServerRpc(RequireOwnership = false)]
		private void RemoveCopyHistoryAtIndex_ServerRpc(int index, ServerRpcParams rpcParams = default(ServerRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(2768657413U, rpcParams, RpcDelivery.Reliable);
				BytePacker.WriteValueBitPacked(fastBufferWriter, index);
				base.__endSendServerRpc(ref fastBufferWriter, 2768657413U, rpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			ulong senderClientId = rpcParams.Receive.SenderClientId;
			if (index < 0)
			{
				Debug.LogException(new Exception(string.Format("Attempted to remove copy history at a less than zero index, which should mean the player (player id: {0}) is in prop selection state, not paste state.", senderClientId)));
				return;
			}
			if (!this.ServerCopyHistoryPerPlayer.ContainsKey(senderClientId))
			{
				Debug.LogException(new Exception(string.Format("Attempting to remove copy history for player {0} (index {1}), but that player has no copy history on server!", senderClientId, index)));
				return;
			}
			List<ServerCopyHistoryEntry> list = this.ServerCopyHistoryPerPlayer[senderClientId];
			if (list.Count < index)
			{
				Debug.LogException(new Exception(string.Format("Attempted to remove copy history for an index ({0}) that didn't exist in the players (player id: {1}) copy history. Attempted to paste index {2} but only {3} entries exist.", new object[] { index, senderClientId, index, list.Count })));
				return;
			}
			list.RemoveAt(index);
		}

		// Token: 0x06000FA4 RID: 4004 RVA: 0x0004829C File Offset: 0x0004649C
		public void UpdateCopyLabel(string newLabel, int index)
		{
			if (index < 0)
			{
				Debug.LogWarning(string.Format("Attempted to set a new label for an index less than zero. New Label: {0}, Index: {1}", newLabel, index));
				return;
			}
			if (index >= this.maximumHistoryEntries)
			{
				Debug.LogWarning(string.Format("Attempted to set a new label for an index exceeding the maximum history. New Label: {0}, Index: {1}", newLabel, index));
				return;
			}
			if (index > this.ClientCopyHistoryEntries.Count)
			{
				Debug.LogWarning(string.Format("Attempted to set a new label for an index exceeding the current history count. New Label: {0}, Desired Index: {1}, Current History Count: {2}", newLabel, index, this.ClientCopyHistoryEntries.Count));
				return;
			}
			this.ClientCopyHistoryEntries[index].Label = newLabel;
			this.SetCopyHistoryLabel_ServerRpc(newLabel, index, default(ServerRpcParams));
		}

		// Token: 0x06000FA5 RID: 4005 RVA: 0x0004833C File Offset: 0x0004653C
		[ServerRpc(RequireOwnership = false)]
		private void SetCopyHistoryLabel_ServerRpc(string newLabel, int index, ServerRpcParams rpcParams = default(ServerRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(1537653066U, rpcParams, RpcDelivery.Reliable);
				bool flag = newLabel != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag, default(FastBufferWriter.ForPrimitives));
				if (flag)
				{
					fastBufferWriter.WriteValueSafe(newLabel, false);
				}
				BytePacker.WriteValueBitPacked(fastBufferWriter, index);
				base.__endSendServerRpc(ref fastBufferWriter, 1537653066U, rpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			ulong senderClientId = rpcParams.Receive.SenderClientId;
			if (index < 0)
			{
				Debug.LogException(new Exception(string.Format("Attempted to rename copy history at a less than zero index, which should mean the player (player id: {0}) is in prop selection state, not paste state.", senderClientId)));
				return;
			}
			if (!this.ServerCopyHistoryPerPlayer.ContainsKey(senderClientId))
			{
				Debug.LogException(new Exception(string.Format("Attempting to rename copy history for player {0} (index {1}), but that player has no copy history on server!", senderClientId, index)));
				return;
			}
			List<ServerCopyHistoryEntry> list = this.ServerCopyHistoryPerPlayer[senderClientId];
			if (list.Count < index)
			{
				Debug.LogException(new Exception(string.Format("Attempted to rename copy history for an index ({0}) that didn't exist in the players (player id: {1}) copy history. Attempted to paste index {2} but only {3} entries exist.", new object[] { index, senderClientId, index, list.Count })));
				return;
			}
			list[index].Prop.Label = newLabel;
		}

		// Token: 0x06000FA6 RID: 4006 RVA: 0x00048520 File Offset: 0x00046720
		public override void ToolReleased()
		{
			base.ToolReleased();
			if (this.copiedInstanceId == SerializableGuid.Empty)
			{
				this.AttemptSelectPropToCopy();
				return;
			}
			this.AttemptPasteCopyOfPropInstance();
		}

		// Token: 0x06000FA7 RID: 4007 RVA: 0x00048548 File Offset: 0x00046748
		private void AttemptSelectPropToCopy()
		{
			LineCastHit activeLineCastResult = base.ActiveLineCastResult;
			if (!activeLineCastResult.IntersectionOccured)
			{
				return;
			}
			PropCell cellFromCoordinateAs = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetCellFromCoordinateAs<PropCell>(activeLineCastResult.IntersectedObjectPosition);
			if (cellFromCoordinateAs == null)
			{
				return;
			}
			this.copiedInstanceId = cellFromCoordinateAs.InstanceId;
			MonoBehaviourSingleton<PropLocationMarker>.Instance.MarkCellsForProp(this.copiedInstanceId, PropLocationType.Selected);
			SerializableGuid assetIdFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetAssetIdFromInstanceId(this.copiedInstanceId);
			PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(assetIdFromInstanceId);
			base.GeneratePropPreview(runtimePropInfo, default(SerializableGuid));
			this.selectedIndex = 0;
			base.Set3DCursorUsesIntersection(false);
			GameObject gameObjectFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(this.copiedInstanceId);
			base.PropGhostTransform.rotation = gameObjectFromInstanceId.transform.rotation;
			this.ClientCopyHistoryEntries.Insert(0, new ClientCopyHistoryEntry
			{
				Label = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.GetPropEntry(this.copiedInstanceId).Label,
				InstanceId = this.copiedInstanceId,
				AssetId = assetIdFromInstanceId,
				Rotation = (gameObjectFromInstanceId ? gameObjectFromInstanceId.transform.rotation : Quaternion.identity)
			});
			this.CopyHistoryEntryInserted.Invoke(this.ClientCopyHistoryEntries[this.selectedIndex]);
			this.TrimHistoryIfNecessary<ClientCopyHistoryEntry>(this.ClientCopyHistoryEntries, true);
			this.AddCopyHistoryFromObject_ServerRpc(this.copiedInstanceId, default(ServerRpcParams));
		}

		// Token: 0x06000FA8 RID: 4008 RVA: 0x000486BC File Offset: 0x000468BC
		[ServerRpc(RequireOwnership = false)]
		private void AddCopyHistoryFromObject_ServerRpc(SerializableGuid instanceId, ServerRpcParams rpcParams = default(ServerRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(2516555771U, rpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<SerializableGuid>(in instanceId, default(FastBufferWriter.ForNetworkSerializable));
				base.__endSendServerRpc(ref fastBufferWriter, 2516555771U, rpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			PropEntry propEntry = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.CopyProp(instanceId);
			if (propEntry == null)
			{
				return;
			}
			ServerCopyHistoryEntry serverCopyHistoryEntry = new ServerCopyHistoryEntry();
			serverCopyHistoryEntry.Prop = propEntry;
			ValueTuple<List<WireBundle>, List<WireBundle>> bundlesWiredToInstance = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.GetBundlesWiredToInstance(instanceId);
			List<WireBundle> item = bundlesWiredToInstance.Item1;
			List<WireBundle> item2 = bundlesWiredToInstance.Item2;
			serverCopyHistoryEntry.EmitterWireBundles = item;
			serverCopyHistoryEntry.ReceiverWireBundles = item2;
			ulong senderClientId = rpcParams.Receive.SenderClientId;
			if (!this.ServerCopyHistoryPerPlayer.ContainsKey(senderClientId))
			{
				this.ServerCopyHistoryPerPlayer.Add(senderClientId, new List<ServerCopyHistoryEntry>());
			}
			try
			{
				PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(serverCopyHistoryEntry.Prop.AssetId);
				base.FirePropEvent("propCopied", runtimePropInfo.PropData.AssetID, runtimePropInfo.PropData.Name);
			}
			catch (Exception ex)
			{
				Debug.LogException(new Exception("Error with copy tool analyitcs", ex));
			}
			this.ServerCopyHistoryPerPlayer[senderClientId].Insert(0, serverCopyHistoryEntry);
			this.TrimHistoryIfNecessary<ServerCopyHistoryEntry>(this.ServerCopyHistoryPerPlayer[senderClientId], false);
		}

		// Token: 0x06000FA9 RID: 4009 RVA: 0x000488C4 File Offset: 0x00046AC4
		private void TrimHistoryIfNecessary<T>(List<T> history, bool emitTrimEvent = true)
		{
			if (history.Count < this.maximumHistoryEntries)
			{
				return;
			}
			int num = 0;
			for (int i = history.Count - 1; i >= this.maximumHistoryEntries; i--)
			{
				num++;
				history.RemoveAt(i);
			}
			if (emitTrimEvent && base.IsClient)
			{
				this.CopyHistoryTrimmed.Invoke(num);
			}
		}

		// Token: 0x06000FAA RID: 4010 RVA: 0x0004891C File Offset: 0x00046B1C
		private void AttemptPasteCopyOfPropInstance()
		{
			if (base.PropGhostTransform && base.NoInvalidOverlapsExist())
			{
				this.AttemptCopyProp_ServerRpc(this.selectedIndex, base.PropGhostTransform.position, base.PropGhostTransform.rotation.eulerAngles, default(ServerRpcParams));
			}
		}

		// Token: 0x06000FAB RID: 4011 RVA: 0x00048974 File Offset: 0x00046B74
		[ServerRpc(RequireOwnership = false)]
		private void AttemptCopyProp_ServerRpc(int index, global::UnityEngine.Vector3 position, global::UnityEngine.Vector3 eulerAngles, ServerRpcParams rpcParams = default(ServerRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(3110554693U, rpcParams, RpcDelivery.Reliable);
				BytePacker.WriteValueBitPacked(fastBufferWriter, index);
				fastBufferWriter.WriteValueSafe(in position);
				fastBufferWriter.WriteValueSafe(in eulerAngles);
				base.__endSendServerRpc(ref fastBufferWriter, 3110554693U, rpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			ulong senderClientId = rpcParams.Receive.SenderClientId;
			int num;
			if (!NetworkBehaviourSingleton<UserIdManager>.Instance.TryGetUserId(senderClientId, out num))
			{
				Debug.LogException(new Exception(string.Format("Unable to determine User Id for Client Id: {0}", senderClientId)));
			}
			this.AttemptCopyProp_Execute(index, position, eulerAngles, num, senderClientId);
		}

		// Token: 0x06000FAC RID: 4012 RVA: 0x00048AB0 File Offset: 0x00046CB0
		private async void AttemptCopyProp_Execute(int index, global::UnityEngine.Vector3 position, global::UnityEngine.Vector3 eulerAngles, int userId, ulong playerId)
		{
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
				if (index < 0)
				{
					Debug.LogException(new Exception(string.Format("Attempted to place a less than zero index, which should mean the player (player id: {0}) is in prop selection state, not paste state.", playerId)));
				}
				else if (!this.ServerCopyHistoryPerPlayer.ContainsKey(playerId))
				{
					Debug.LogException(new Exception(string.Format("Attempting to paste a prop for player {0}, but that player has no copy history on server!", playerId)));
				}
				else
				{
					List<ServerCopyHistoryEntry> list = this.ServerCopyHistoryPerPlayer[playerId];
					if (list.Count < index)
					{
						Debug.LogException(new Exception(string.Format("Attempted to paste an index that didn't exist in the players (player id: {0}) copy history. Attempted to paste index {1} but only {2} entries exist.", playerId, index, list.Count)));
					}
					else
					{
						ServerCopyHistoryEntry serverCopyHistoryEntry = list[index];
						PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(serverCopyHistoryEntry.Prop.AssetId);
						if (runtimePropInfo == null)
						{
							Debug.LogException(new Exception("Server was told to spawn invalid asset ID!"));
						}
						else
						{
							bool flag = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.PlacementIsValid(runtimePropInfo.PropData, position, Quaternion.Euler(eulerAngles));
							bool flag2 = true;
							bool flag3 = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.CanAddProp(runtimePropInfo);
							if (flag && flag2 && flag3)
							{
								base.FirePropPlacedAnalyticEvent(runtimePropInfo.PropData.AssetID, runtimePropInfo.PropData.Name);
								GameObject gameObject = global::UnityEngine.Object.Instantiate<EndlessProp>(runtimePropInfo.EndlessProp, position, Quaternion.Euler(eulerAngles)).gameObject;
								SerializableGuid serializableGuid = SerializableGuid.NewGuid();
								ulong num = 0UL;
								if (runtimePropInfo.EndlessProp.IsNetworked)
								{
									NetworkObject component = gameObject.GetComponent<NetworkObject>();
									if (component != null)
									{
										component.Spawn(false);
										num = component.NetworkObjectId;
									}
									MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.TrackPendingNetworkId(num, serializableGuid, true, null);
								}
								MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.TrackNonNetworkedObject(serverCopyHistoryEntry.Prop.AssetId, serializableGuid, gameObject, true);
								PropEntry propEntry = serverCopyHistoryEntry.Prop.CopyWithNewInstanceId(serializableGuid);
								propEntry.Position = position;
								propEntry.Rotation = Quaternion.Euler(eulerAngles);
								MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.AddOrUpdatePropWithInstance(propEntry);
								WireBundle[] array = serverCopyHistoryEntry.CopyEmitterBundles(serializableGuid);
								WireBundle[] array2 = serverCopyHistoryEntry.CopyReceiverBundles(serializableGuid);
								MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.InsertWireBundles(array);
								MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.InsertWireBundles(array2);
								bool flag4 = false;
								foreach (WireBundle wireBundle in MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.WireBundles)
								{
									if (wireBundle.EmitterInstanceId == serializableGuid || wireBundle.ReceiverInstanceId == serializableGuid)
									{
										MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.SpawnPhysicalWire(wireBundle.BundleId, wireBundle.EmitterInstanceId, wireBundle.ReceiverInstanceId, wireBundle.RerouteNodeIds, wireBundle.WireColor, false);
										try
										{
											SerializableGuid assetIdFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetAssetIdFromInstanceId(wireBundle.EmitterInstanceId);
											PropLibrary.RuntimePropInfo runtimePropInfo2 = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(assetIdFromInstanceId);
											SerializableGuid assetIdFromInstanceId2 = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetAssetIdFromInstanceId(wireBundle.ReceiverInstanceId);
											PropLibrary.RuntimePropInfo runtimePropInfo3 = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(assetIdFromInstanceId2);
											CustomEvent customEvent = new CustomEvent("wireDuplicated");
											customEvent.Add("propName1", runtimePropInfo2.PropData.Name);
											customEvent.Add("propId1", assetIdFromInstanceId.ToString());
											customEvent.Add("propName2", runtimePropInfo3.PropData.Name);
											customEvent.Add("propId2", assetIdFromInstanceId2.ToString());
											AnalyticsService.Instance.RecordEvent(customEvent);
										}
										catch (Exception ex)
										{
											Debug.LogException(new Exception("Wire copy analytics event error", ex));
										}
									}
								}
								CustomEvent customEvent2 = new CustomEvent("copyToolPropPasted");
								customEvent2.Add("propName1", runtimePropInfo.PropData.Name);
								customEvent2.Add("propId1", runtimePropInfo.PropData.AssetID);
								customEvent2.Add("pastedWires", flag4);
								AnalyticsService.Instance.RecordEvent(customEvent2);
								Stage.ApplyMemberChanges(propEntry);
								MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LoadExistingWires(false);
								this.CopyProp_ClientRpc(position, eulerAngles, propEntry, array, array2, num);
								MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.RevisionTimestamp = DateTime.UtcNow.Ticks;
								MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.Changes.Add(new ChangeData
								{
									ChangeType = ChangeType.PropPasted,
									UserId = userId
								});
								IPropPlacedSubscriber[] componentsInChildren = gameObject.GetComponentsInChildren<IPropPlacedSubscriber>();
								for (int i = 0; i < componentsInChildren.Length; i++)
								{
									componentsInChildren[i].PropPlaced(serializableGuid, true);
								}
								NetworkBehaviourSingleton<CreatorManager>.Instance.SaveLoadManager.SaveLevel();
							}
						}
					}
				}
			}
		}

		// Token: 0x06000FAD RID: 4013 RVA: 0x00048B14 File Offset: 0x00046D14
		[ClientRpc]
		private void CopyProp_ClientRpc(global::UnityEngine.Vector3 position, global::UnityEngine.Vector3 eulerAngles, PropEntry copy, WireBundle[] emitterWireBundles, WireBundle[] receiverWireBundles, ulong networkObjectId)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			global::UnityEngine.Vector3 position;
			global::UnityEngine.Vector3 eulerAngles;
			PropEntry copy;
			WireBundle[] emitterWireBundles;
			WireBundle[] receiverWireBundles;
			ulong networkObjectId;
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(3984099148U, clientRpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe(in position);
				fastBufferWriter.WriteValueSafe(in eulerAngles);
				bool flag = copy != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag, default(FastBufferWriter.ForPrimitives));
				if (flag)
				{
					fastBufferWriter.WriteValueSafe<PropEntry>(in copy, default(FastBufferWriter.ForNetworkSerializable));
				}
				bool flag2 = emitterWireBundles != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag2, default(FastBufferWriter.ForPrimitives));
				if (flag2)
				{
					fastBufferWriter.WriteValueSafe<WireBundle>(emitterWireBundles, default(FastBufferWriter.ForNetworkSerializable));
				}
				bool flag3 = receiverWireBundles != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag3, default(FastBufferWriter.ForPrimitives));
				if (flag3)
				{
					fastBufferWriter.WriteValueSafe<WireBundle>(receiverWireBundles, default(FastBufferWriter.ForNetworkSerializable));
				}
				BytePacker.WriteValueBitPacked(fastBufferWriter, networkObjectId);
				base.__endSendClientRpc(ref fastBufferWriter, 3984099148U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			position = position;
			eulerAngles = eulerAngles;
			copy = copy;
			emitterWireBundles = emitterWireBundles;
			receiverWireBundles = receiverWireBundles;
			networkObjectId = networkObjectId;
			if (base.IsServer)
			{
				return;
			}
			if (NetworkBehaviourSingleton<CreatorManager>.Instance.RpcReceiveState == RpcReceiveState.Allow)
			{
				this.CopyProp(position, eulerAngles, copy, emitterWireBundles, receiverWireBundles, networkObjectId);
				return;
			}
			NetworkBehaviourSingleton<CreatorManager>.Instance.AddCachedRpc(delegate
			{
				this.CopyProp(position, eulerAngles, copy, emitterWireBundles, receiverWireBundles, networkObjectId);
			});
		}

		// Token: 0x06000FAE RID: 4014 RVA: 0x00048D80 File Offset: 0x00046F80
		private void CopyProp(global::UnityEngine.Vector3 position, global::UnityEngine.Vector3 eulerAngles, PropEntry copy, WireBundle[] emitterWireBundles, WireBundle[] receiverWireBundles, ulong networkObjectId)
		{
			PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(copy.AssetId);
			bool wireToolIsActive = MonoBehaviourSingleton<ToolManager>.Instance.ActiveTool is WiringTool;
			if (networkObjectId == 0UL)
			{
				GameObject gameObject = global::UnityEngine.Object.Instantiate<EndlessProp>(runtimePropInfo.EndlessProp).gameObject;
				gameObject.transform.position = position;
				gameObject.transform.rotation = Quaternion.Euler(eulerAngles);
				MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.TrackNonNetworkedObject(copy.AssetId, copy.InstanceId, gameObject, true);
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

		// Token: 0x06000FAF RID: 4015 RVA: 0x00048FA0 File Offset: 0x000471A0
		public override void UpdateTool()
		{
			base.UpdateTool();
			if (this.selectedIndex == -1)
			{
				this.UpdateHighlightState();
				return;
			}
			if (!base.IsMobile)
			{
				base.UpdatePropPlacement();
			}
		}

		// Token: 0x06000FB0 RID: 4016 RVA: 0x00048FC8 File Offset: 0x000471C8
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
					if (this.hoveredPropId != this.copiedInstanceId)
					{
						MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(this.hoveredPropId, PropLocationType.Selected);
					}
					this.hoveredPropId = SerializableGuid.Empty;
					return;
				}
			}
			else if (!this.hoveredPropId.IsEmpty)
			{
				if (this.hoveredPropId != this.copiedInstanceId)
				{
					MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(this.hoveredPropId, PropLocationType.Selected);
				}
				this.hoveredPropId = SerializableGuid.Empty;
			}
		}

		// Token: 0x06000FB1 RID: 4017 RVA: 0x000490E3 File Offset: 0x000472E3
		public override void ToolSecondaryPressed()
		{
			base.ToolSecondaryPressed();
			this.selectedIndex = -1;
			this.ResetToolState();
		}

		// Token: 0x06000FB3 RID: 4019 RVA: 0x00049174 File Offset: 0x00047374
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x06000FB4 RID: 4020 RVA: 0x0004918C File Offset: 0x0004738C
		protected override void __initializeRpcs()
		{
			base.__registerRpc(3657533409U, new NetworkBehaviour.RpcReceiveHandler(CopyTool.__rpc_handler_3657533409), "ClearCopyHistoryForPlayer_ServerRpc");
			base.__registerRpc(2768657413U, new NetworkBehaviour.RpcReceiveHandler(CopyTool.__rpc_handler_2768657413), "RemoveCopyHistoryAtIndex_ServerRpc");
			base.__registerRpc(1537653066U, new NetworkBehaviour.RpcReceiveHandler(CopyTool.__rpc_handler_1537653066), "SetCopyHistoryLabel_ServerRpc");
			base.__registerRpc(2516555771U, new NetworkBehaviour.RpcReceiveHandler(CopyTool.__rpc_handler_2516555771), "AddCopyHistoryFromObject_ServerRpc");
			base.__registerRpc(3110554693U, new NetworkBehaviour.RpcReceiveHandler(CopyTool.__rpc_handler_3110554693), "AttemptCopyProp_ServerRpc");
			base.__registerRpc(3984099148U, new NetworkBehaviour.RpcReceiveHandler(CopyTool.__rpc_handler_3984099148), "CopyProp_ClientRpc");
			base.__initializeRpcs();
		}

		// Token: 0x06000FB5 RID: 4021 RVA: 0x0004924C File Offset: 0x0004744C
		private static void __rpc_handler_3657533409(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((CopyTool)target).ClearCopyHistoryForPlayer_ServerRpc(server);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06000FB6 RID: 4022 RVA: 0x000492AC File Offset: 0x000474AC
		private static void __rpc_handler_2768657413(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			int num;
			ByteUnpacker.ReadValueBitPacked(reader, out num);
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((CopyTool)target).RemoveCopyHistoryAtIndex_ServerRpc(num, server);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06000FB7 RID: 4023 RVA: 0x0004931C File Offset: 0x0004751C
		private static void __rpc_handler_1537653066(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
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
			int num;
			ByteUnpacker.ReadValueBitPacked(reader, out num);
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((CopyTool)target).SetCopyHistoryLabel_ServerRpc(text, num, server);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06000FB8 RID: 4024 RVA: 0x000493C8 File Offset: 0x000475C8
		private static void __rpc_handler_2516555771(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
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
			((CopyTool)target).AddCopyHistoryFromObject_ServerRpc(serializableGuid, server);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06000FB9 RID: 4025 RVA: 0x00049448 File Offset: 0x00047648
		private static void __rpc_handler_3110554693(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			int num;
			ByteUnpacker.ReadValueBitPacked(reader, out num);
			global::UnityEngine.Vector3 vector;
			reader.ReadValueSafe(out vector);
			global::UnityEngine.Vector3 vector2;
			reader.ReadValueSafe(out vector2);
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((CopyTool)target).AttemptCopyProp_ServerRpc(num, vector, vector2, server);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06000FBA RID: 4026 RVA: 0x000494DC File Offset: 0x000476DC
		private static void __rpc_handler_3984099148(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			global::UnityEngine.Vector3 vector;
			reader.ReadValueSafe(out vector);
			global::UnityEngine.Vector3 vector2;
			reader.ReadValueSafe(out vector2);
			bool flag;
			reader.ReadValueSafe<bool>(out flag, default(FastBufferWriter.ForPrimitives));
			PropEntry propEntry = null;
			if (flag)
			{
				reader.ReadValueSafe<PropEntry>(out propEntry, default(FastBufferWriter.ForNetworkSerializable));
			}
			bool flag2;
			reader.ReadValueSafe<bool>(out flag2, default(FastBufferWriter.ForPrimitives));
			WireBundle[] array = null;
			if (flag2)
			{
				reader.ReadValueSafe<WireBundle>(out array, default(FastBufferWriter.ForNetworkSerializable));
			}
			bool flag3;
			reader.ReadValueSafe<bool>(out flag3, default(FastBufferWriter.ForPrimitives));
			WireBundle[] array2 = null;
			if (flag3)
			{
				reader.ReadValueSafe<WireBundle>(out array2, default(FastBufferWriter.ForNetworkSerializable));
			}
			ulong num;
			ByteUnpacker.ReadValueBitPacked(reader, out num);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((CopyTool)target).CopyProp_ClientRpc(vector, vector2, propEntry, array, array2, num);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06000FBB RID: 4027 RVA: 0x0004963B File Offset: 0x0004783B
		protected internal override string __getTypeName()
		{
			return "CopyTool";
		}

		// Token: 0x04000CF1 RID: 3313
		public UnityEvent<ClientCopyHistoryEntry> CopyHistoryEntryInserted = new UnityEvent<ClientCopyHistoryEntry>();

		// Token: 0x04000CF2 RID: 3314
		public UnityEvent<int> CopyHistoryTrimmed = new UnityEvent<int>();

		// Token: 0x04000CF3 RID: 3315
		public UnityEvent CopyHistoryCleared = new UnityEvent();

		// Token: 0x04000CF4 RID: 3316
		public UnityEvent<int> OnSelectedCopyIndexSet = new UnityEvent<int>();

		// Token: 0x04000CF5 RID: 3317
		private const int SELECTING_PROP_TO_COPY = -1;

		// Token: 0x04000CF6 RID: 3318
		[SerializeField]
		private int maximumHistoryEntries = 9;

		// Token: 0x04000CF7 RID: 3319
		private SerializableGuid copiedInstanceId = SerializableGuid.Empty;

		// Token: 0x04000CF8 RID: 3320
		private SerializableGuid hoveredPropId = SerializableGuid.Empty;

		// Token: 0x04000CF9 RID: 3321
		private int selectedIndex = -1;
	}
}
