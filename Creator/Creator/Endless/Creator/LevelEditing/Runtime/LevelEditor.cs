using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Endless.Assets;
using Endless.Data;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Matchmaking;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Runtime.Shared.Matchmaking;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Creator.LevelEditing.Runtime
{
	// Token: 0x0200035E RID: 862
	public class LevelEditor : NetworkBehaviour
	{
		// Token: 0x0600105E RID: 4190 RVA: 0x0004D768 File Offset: 0x0004B968
		[ServerRpc(RequireOwnership = false)]
		public void UpdateLevelName_ServerRpc(string newLevelName, ServerRpcParams serverRpcParams = default(ServerRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(97940106U, serverRpcParams, RpcDelivery.Reliable);
				bool flag = newLevelName != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag, default(FastBufferWriter.ForPrimitives));
				if (flag)
				{
					fastBufferWriter.WriteValueSafe(newLevelName, false);
				}
				base.__endSendServerRpc(ref fastBufferWriter, 97940106U, serverRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.UpdateLevelName(newLevelName, serverRpcParams);
		}

		// Token: 0x0600105F RID: 4191 RVA: 0x0004D884 File Offset: 0x0004BA84
		private async Task UpdateLevelName(string newLevelName, ServerRpcParams serverRpcParams)
		{
			if (base.IsServer)
			{
				int userId;
				if (!NetworkBehaviourSingleton<UserIdManager>.Instance.TryGetUserId(serverRpcParams.Receive.SenderClientId, out userId))
				{
					Debug.LogException(new Exception(string.Format("Unable to determine User Id for Client Id: {0}", serverRpcParams.Receive.SenderClientId)));
				}
				else
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
						MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.RevisionTimestamp = DateTime.UtcNow.Ticks;
						MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.Changes.Add(new ChangeData
						{
							ChangeType = ChangeType.LevelNameUpdated,
							UserId = userId
						});
						this.UpdateLevelName_ClientRpc(newLevelName);
						if (!base.IsClient)
						{
							this.UpdateLevelName(newLevelName);
						}
						NetworkBehaviourSingleton<CreatorManager>.Instance.SaveLoadManager.SaveLevel();
					}
				}
			}
		}

		// Token: 0x06001060 RID: 4192 RVA: 0x0004D8D8 File Offset: 0x0004BAD8
		[ClientRpc]
		private void UpdateLevelName_ClientRpc(string newLevelName)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(1717172957U, clientRpcParams, RpcDelivery.Reliable);
				bool flag = newLevelName != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag, default(FastBufferWriter.ForPrimitives));
				if (flag)
				{
					fastBufferWriter.WriteValueSafe(newLevelName, false);
				}
				base.__endSendClientRpc(ref fastBufferWriter, 1717172957U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.UpdateLevelName(newLevelName);
		}

		// Token: 0x06001061 RID: 4193 RVA: 0x0004D9F0 File Offset: 0x0004BBF0
		private void UpdateLevelName(string newLevelName)
		{
			if (NetworkBehaviourSingleton<CreatorManager>.Instance.RpcReceiveState == RpcReceiveState.Allow)
			{
				MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.Name = newLevelName;
				return;
			}
			NetworkBehaviourSingleton<CreatorManager>.Instance.AddCachedRpc(delegate
			{
				MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.Name = newLevelName;
			});
		}

		// Token: 0x06001062 RID: 4194 RVA: 0x0004DA48 File Offset: 0x0004BC48
		[ServerRpc(RequireOwnership = false)]
		public void UpdateLevelDescription_ServerRpc(string newLevelDescription, ServerRpcParams serverRpcParams = default(ServerRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(3198977598U, serverRpcParams, RpcDelivery.Reliable);
				bool flag = newLevelDescription != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag, default(FastBufferWriter.ForPrimitives));
				if (flag)
				{
					fastBufferWriter.WriteValueSafe(newLevelDescription, false);
				}
				base.__endSendServerRpc(ref fastBufferWriter, 3198977598U, serverRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.UpdateLevelDescription(newLevelDescription, serverRpcParams);
		}

		// Token: 0x06001063 RID: 4195 RVA: 0x0004DB64 File Offset: 0x0004BD64
		private async Task UpdateLevelDescription(string newLevelDescription, ServerRpcParams serverRpcParams)
		{
			if (base.IsServer)
			{
				int userId;
				if (!NetworkBehaviourSingleton<UserIdManager>.Instance.TryGetUserId(serverRpcParams.Receive.SenderClientId, out userId))
				{
					Debug.LogException(new Exception(string.Format("Unable to determine User Id for Client Id: {0}", serverRpcParams.Receive.SenderClientId)));
				}
				else
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
						MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.RevisionTimestamp = DateTime.UtcNow.Ticks;
						MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.Changes.Add(new ChangeData
						{
							ChangeType = ChangeType.LevelDescriptionUpdated,
							UserId = userId
						});
						this.UpdateLevelDescription_ClientRpc(newLevelDescription);
						if (!base.IsClient)
						{
							this.UpdateLevelDescription(newLevelDescription);
						}
						NetworkBehaviourSingleton<CreatorManager>.Instance.SaveLoadManager.SaveLevel();
					}
				}
			}
		}

		// Token: 0x06001064 RID: 4196 RVA: 0x0004DBB8 File Offset: 0x0004BDB8
		[ClientRpc]
		private void UpdateLevelDescription_ClientRpc(string newLevelName)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(339205854U, clientRpcParams, RpcDelivery.Reliable);
				bool flag = newLevelName != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag, default(FastBufferWriter.ForPrimitives));
				if (flag)
				{
					fastBufferWriter.WriteValueSafe(newLevelName, false);
				}
				base.__endSendClientRpc(ref fastBufferWriter, 339205854U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.UpdateLevelDescription(newLevelName);
		}

		// Token: 0x06001065 RID: 4197 RVA: 0x0004DCD0 File Offset: 0x0004BED0
		private void UpdateLevelDescription(string newLevelName)
		{
			if (NetworkBehaviourSingleton<CreatorManager>.Instance.RpcReceiveState == RpcReceiveState.Allow)
			{
				MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.Description = newLevelName;
				return;
			}
			NetworkBehaviourSingleton<CreatorManager>.Instance.AddCachedRpc(delegate
			{
				MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.Description = newLevelName;
			});
		}

		// Token: 0x06001066 RID: 4198 RVA: 0x0004DD28 File Offset: 0x0004BF28
		[ServerRpc(RequireOwnership = false)]
		public void UpdateLevelArchivedState_ServerRpc(bool archiveState, ServerRpcParams serverRpcParams = default(ServerRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(1442060746U, serverRpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<bool>(in archiveState, default(FastBufferWriter.ForPrimitives));
				base.__endSendServerRpc(ref fastBufferWriter, 1442060746U, serverRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.UpdateLevelArchivedState(archiveState, serverRpcParams);
		}

		// Token: 0x06001067 RID: 4199 RVA: 0x0004DE20 File Offset: 0x0004C020
		private async Task UpdateLevelArchivedState(bool archiveState, ServerRpcParams serverRpcParams)
		{
			if (base.IsServer)
			{
				int userId;
				if (!NetworkBehaviourSingleton<UserIdManager>.Instance.TryGetUserId(serverRpcParams.Receive.SenderClientId, out userId))
				{
					Debug.LogException(new Exception(string.Format("Unable to determine User Id for Client Id: {0}", serverRpcParams.Receive.SenderClientId)));
				}
				else
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
						MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.RevisionTimestamp = DateTime.UtcNow.Ticks;
						MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.Changes.Add(new ChangeData
						{
							ChangeType = ChangeType.LevelArchived,
							UserId = userId
						});
						this.UpdateLevelArchivedState_ClientRpc(archiveState);
						if (!base.IsClient)
						{
							this.UpdateLevelArchivedState(archiveState);
						}
						NetworkBehaviourSingleton<CreatorManager>.Instance.SaveLoadManager.SaveLevel();
					}
				}
			}
		}

		// Token: 0x06001068 RID: 4200 RVA: 0x0004DE74 File Offset: 0x0004C074
		[ClientRpc]
		private void UpdateLevelArchivedState_ClientRpc(bool archiveState)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(2710954523U, clientRpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<bool>(in archiveState, default(FastBufferWriter.ForPrimitives));
				base.__endSendClientRpc(ref fastBufferWriter, 2710954523U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.UpdateLevelArchivedState(archiveState);
		}

		// Token: 0x06001069 RID: 4201 RVA: 0x0004DF6C File Offset: 0x0004C16C
		private void UpdateLevelArchivedState(bool archiveState)
		{
			if (NetworkBehaviourSingleton<CreatorManager>.Instance.RpcReceiveState == RpcReceiveState.Allow)
			{
				MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.Archived = archiveState;
				return;
			}
			NetworkBehaviourSingleton<CreatorManager>.Instance.AddCachedRpc(delegate
			{
				MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.Archived = archiveState;
			});
		}

		// Token: 0x0600106A RID: 4202 RVA: 0x0004DFC4 File Offset: 0x0004C1C4
		[ServerRpc(RequireOwnership = false)]
		public void UpdateLevelSpawnPointOrder_ServerRpc(SerializableGuid[] spawnPointIdOrder, SerializableGuid[] selectedSpawnPoints, ServerRpcParams serverRpcParams = default(ServerRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(3804119171U, serverRpcParams, RpcDelivery.Reliable);
				bool flag = spawnPointIdOrder != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag, default(FastBufferWriter.ForPrimitives));
				if (flag)
				{
					fastBufferWriter.WriteValueSafe<SerializableGuid>(spawnPointIdOrder, default(FastBufferWriter.ForNetworkSerializable));
				}
				bool flag2 = selectedSpawnPoints != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag2, default(FastBufferWriter.ForPrimitives));
				if (flag2)
				{
					fastBufferWriter.WriteValueSafe<SerializableGuid>(selectedSpawnPoints, default(FastBufferWriter.ForNetworkSerializable));
				}
				base.__endSendServerRpc(ref fastBufferWriter, 3804119171U, serverRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.UpdateLevelSpawnPointOrder(spawnPointIdOrder, selectedSpawnPoints, serverRpcParams);
		}

		// Token: 0x0600106B RID: 4203 RVA: 0x0004E138 File Offset: 0x0004C338
		private async Task UpdateLevelSpawnPointOrder(SerializableGuid[] spawnPointIdOrder, SerializableGuid[] selectedSpawnPoints, ServerRpcParams serverRpcParams)
		{
			if (base.IsServer)
			{
				int userId;
				if (!NetworkBehaviourSingleton<UserIdManager>.Instance.TryGetUserId(serverRpcParams.Receive.SenderClientId, out userId))
				{
					Debug.LogException(new Exception(string.Format("Unable to determine User Id for Client Id: {0}", serverRpcParams.Receive.SenderClientId)));
				}
				else
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
						if (!MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.UpdateSpawnPointOrder(spawnPointIdOrder))
						{
							Debug.LogWarning("Unable to update spawn point order");
						}
						else if (!MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.UpdateSelectedSpawnPoints(selectedSpawnPoints))
						{
							Debug.LogWarning("Unable to update selected spawn points");
						}
						else
						{
							MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.RevisionTimestamp = DateTime.UtcNow.Ticks;
							MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.Changes.Add(new ChangeData
							{
								ChangeType = ChangeType.LevelUpdatedSpawnPositionOrder,
								UserId = userId
							});
							this.UpdateLevelSpawnPointOrder_ClientRpc(spawnPointIdOrder, selectedSpawnPoints);
							if (!base.IsClient)
							{
								this.UpdateLevelSpawnPointOrder(spawnPointIdOrder, selectedSpawnPoints);
							}
							NetworkBehaviourSingleton<CreatorManager>.Instance.SaveLoadManager.SaveLevel();
						}
					}
				}
			}
		}

		// Token: 0x0600106C RID: 4204 RVA: 0x0004E194 File Offset: 0x0004C394
		[ClientRpc]
		private void UpdateLevelSpawnPointOrder_ClientRpc(SerializableGuid[] spawnPointIdOrder, SerializableGuid[] selectedSpawnPointIds)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(1546701494U, clientRpcParams, RpcDelivery.Reliable);
				bool flag = spawnPointIdOrder != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag, default(FastBufferWriter.ForPrimitives));
				if (flag)
				{
					fastBufferWriter.WriteValueSafe<SerializableGuid>(spawnPointIdOrder, default(FastBufferWriter.ForNetworkSerializable));
				}
				bool flag2 = selectedSpawnPointIds != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag2, default(FastBufferWriter.ForPrimitives));
				if (flag2)
				{
					fastBufferWriter.WriteValueSafe<SerializableGuid>(selectedSpawnPointIds, default(FastBufferWriter.ForNetworkSerializable));
				}
				base.__endSendClientRpc(ref fastBufferWriter, 1546701494U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			if (base.IsServer)
			{
				return;
			}
			this.UpdateLevelSpawnPointOrder(spawnPointIdOrder, selectedSpawnPointIds);
		}

		// Token: 0x0600106D RID: 4205 RVA: 0x0004E310 File Offset: 0x0004C510
		private void UpdateLevelSpawnPointOrder(SerializableGuid[] spawnPointIdOrder, SerializableGuid[] selectedSpawnPointIds)
		{
			if (NetworkBehaviourSingleton<CreatorManager>.Instance.RpcReceiveState == RpcReceiveState.Allow)
			{
				if (!MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.UpdateSpawnPointOrder(spawnPointIdOrder))
				{
					Debug.LogException(new Exception("Server requested through RPC to update spawn point order but client was unable to update!"));
				}
				if (!MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.UpdateSelectedSpawnPoints(selectedSpawnPointIds))
				{
					Debug.LogException(new Exception("Server requested through RPC to update spawn point order but client was unable to update!"));
					return;
				}
			}
			else
			{
				NetworkBehaviourSingleton<CreatorManager>.Instance.AddCachedRpc(delegate
				{
					if (!MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.UpdateSpawnPointOrder(spawnPointIdOrder))
					{
						Debug.LogException(new Exception("Server requested through RPC to update spawn point order but client was unable to update!"));
					}
					if (!MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.UpdateSelectedSpawnPoints(selectedSpawnPointIds))
					{
						Debug.LogException(new Exception("Server requested through RPC to update spawn point order but client was unable to update!"));
					}
				});
			}
		}

		// Token: 0x0600106E RID: 4206 RVA: 0x0004E3AC File Offset: 0x0004C5AC
		[ServerRpc(RequireOwnership = false)]
		public void RevertLevelToVersion_ServerRpc(string versionId, ServerRpcParams serverRpcParams = default(ServerRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(3966572557U, serverRpcParams, RpcDelivery.Reliable);
				bool flag = versionId != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag, default(FastBufferWriter.ForPrimitives));
				if (flag)
				{
					fastBufferWriter.WriteValueSafe(versionId, false);
				}
				base.__endSendServerRpc(ref fastBufferWriter, 3966572557U, serverRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.RevertLevelToVersion(versionId, serverRpcParams);
		}

		// Token: 0x0600106F RID: 4207 RVA: 0x0004E4C8 File Offset: 0x0004C6C8
		private async Task RevertLevelToVersion(string versionId, ServerRpcParams serverRpcParams)
		{
			if (base.IsServer)
			{
				int userId;
				if (!NetworkBehaviourSingleton<UserIdManager>.Instance.TryGetUserId(serverRpcParams.Receive.SenderClientId, out userId))
				{
					Debug.LogException(new Exception(string.Format("Unable to determine User Id for Client Id: {0}", serverRpcParams.Receive.SenderClientId)));
				}
				else
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
						this.RevertLevel(versionId, userId);
					}
				}
			}
		}

		// Token: 0x06001070 RID: 4208 RVA: 0x0004E51C File Offset: 0x0004C71C
		private async void RevertLevel(string versionId, int userId)
		{
			try
			{
				Debug.Log("Forcing save if needed");
				if (NetworkBehaviourSingleton<CreatorManager>.Instance.SaveLoadManager.PendingSave)
				{
					Debug.Log("Save forced");
					await NetworkBehaviourSingleton<CreatorManager>.Instance.SaveLoadManager.ForceSaveIfNeededAsync();
				}
				Debug.Log("Retrieving old version");
				LevelState levelState = LevelStateLoader.Load((await EndlessServices.Instance.CloudService.GetAssetAsync(MonoBehaviourSingleton<StageManager>.Instance.ActiveLevelGuid, versionId, null, false, 10)).GetDataMember().ToString());
				levelState.RevisionMetaData.Changes.Clear();
				levelState.RevisionMetaData.RevisionTimestamp = DateTime.UtcNow.Ticks;
				levelState.RevisionMetaData.Changes.Add(new ChangeData
				{
					ChangeType = ChangeType.AssetVersionUpdated,
					UserId = userId,
					Metadata = versionId
				});
				Debug.Log("Saving old version to force update");
				await EndlessServices.Instance.CloudService.UpdateAssetAsync(levelState.GetAnonymousObjectForUpload(), MatchmakingClientController.Instance.ActiveGameId, false, false);
				Debug.Log("Save complete, requesting server to reload");
				NetworkBehaviourSingleton<CreatorManager>.Instance.ForcePlayersToReload_ServerRpc();
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
				ErrorHandler.HandleError(ErrorCodes.LevelEditor_RevertLevel, new Exception(ex.Message), true, false);
				MatchmakingClientController.Instance.EndMatch(null);
			}
		}

		// Token: 0x06001072 RID: 4210 RVA: 0x0004E564 File Offset: 0x0004C764
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x06001073 RID: 4211 RVA: 0x0004E57C File Offset: 0x0004C77C
		protected override void __initializeRpcs()
		{
			base.__registerRpc(97940106U, new NetworkBehaviour.RpcReceiveHandler(LevelEditor.__rpc_handler_97940106), "UpdateLevelName_ServerRpc");
			base.__registerRpc(1717172957U, new NetworkBehaviour.RpcReceiveHandler(LevelEditor.__rpc_handler_1717172957), "UpdateLevelName_ClientRpc");
			base.__registerRpc(3198977598U, new NetworkBehaviour.RpcReceiveHandler(LevelEditor.__rpc_handler_3198977598), "UpdateLevelDescription_ServerRpc");
			base.__registerRpc(339205854U, new NetworkBehaviour.RpcReceiveHandler(LevelEditor.__rpc_handler_339205854), "UpdateLevelDescription_ClientRpc");
			base.__registerRpc(1442060746U, new NetworkBehaviour.RpcReceiveHandler(LevelEditor.__rpc_handler_1442060746), "UpdateLevelArchivedState_ServerRpc");
			base.__registerRpc(2710954523U, new NetworkBehaviour.RpcReceiveHandler(LevelEditor.__rpc_handler_2710954523), "UpdateLevelArchivedState_ClientRpc");
			base.__registerRpc(3804119171U, new NetworkBehaviour.RpcReceiveHandler(LevelEditor.__rpc_handler_3804119171), "UpdateLevelSpawnPointOrder_ServerRpc");
			base.__registerRpc(1546701494U, new NetworkBehaviour.RpcReceiveHandler(LevelEditor.__rpc_handler_1546701494), "UpdateLevelSpawnPointOrder_ClientRpc");
			base.__registerRpc(3966572557U, new NetworkBehaviour.RpcReceiveHandler(LevelEditor.__rpc_handler_3966572557), "RevertLevelToVersion_ServerRpc");
			base.__initializeRpcs();
		}

		// Token: 0x06001074 RID: 4212 RVA: 0x0004E690 File Offset: 0x0004C890
		private static void __rpc_handler_97940106(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
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
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((LevelEditor)target).UpdateLevelName_ServerRpc(text, server);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06001075 RID: 4213 RVA: 0x0004E72C File Offset: 0x0004C92C
		private static void __rpc_handler_1717172957(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
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
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((LevelEditor)target).UpdateLevelName_ClientRpc(text);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06001076 RID: 4214 RVA: 0x0004E7BC File Offset: 0x0004C9BC
		private static void __rpc_handler_3198977598(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
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
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((LevelEditor)target).UpdateLevelDescription_ServerRpc(text, server);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06001077 RID: 4215 RVA: 0x0004E858 File Offset: 0x0004CA58
		private static void __rpc_handler_339205854(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
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
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((LevelEditor)target).UpdateLevelDescription_ClientRpc(text);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06001078 RID: 4216 RVA: 0x0004E8E8 File Offset: 0x0004CAE8
		private static void __rpc_handler_1442060746(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			bool flag;
			reader.ReadValueSafe<bool>(out flag, default(FastBufferWriter.ForPrimitives));
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((LevelEditor)target).UpdateLevelArchivedState_ServerRpc(flag, server);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06001079 RID: 4217 RVA: 0x0004E968 File Offset: 0x0004CB68
		private static void __rpc_handler_2710954523(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			bool flag;
			reader.ReadValueSafe<bool>(out flag, default(FastBufferWriter.ForPrimitives));
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((LevelEditor)target).UpdateLevelArchivedState_ClientRpc(flag);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x0600107A RID: 4218 RVA: 0x0004E9D8 File Offset: 0x0004CBD8
		private static void __rpc_handler_3804119171(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
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
			bool flag2;
			reader.ReadValueSafe<bool>(out flag2, default(FastBufferWriter.ForPrimitives));
			SerializableGuid[] array2 = null;
			if (flag2)
			{
				reader.ReadValueSafe<SerializableGuid>(out array2, default(FastBufferWriter.ForNetworkSerializable));
			}
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((LevelEditor)target).UpdateLevelSpawnPointOrder_ServerRpc(array, array2, server);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x0600107B RID: 4219 RVA: 0x0004EACC File Offset: 0x0004CCCC
		private static void __rpc_handler_1546701494(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
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
			bool flag2;
			reader.ReadValueSafe<bool>(out flag2, default(FastBufferWriter.ForPrimitives));
			SerializableGuid[] array2 = null;
			if (flag2)
			{
				reader.ReadValueSafe<SerializableGuid>(out array2, default(FastBufferWriter.ForNetworkSerializable));
			}
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((LevelEditor)target).UpdateLevelSpawnPointOrder_ClientRpc(array, array2);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x0600107C RID: 4220 RVA: 0x0004EBB0 File Offset: 0x0004CDB0
		private static void __rpc_handler_3966572557(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
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
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((LevelEditor)target).RevertLevelToVersion_ServerRpc(text, server);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x0600107D RID: 4221 RVA: 0x0004EC4B File Offset: 0x0004CE4B
		protected internal override string __getTypeName()
		{
			return "LevelEditor";
		}
	}
}
