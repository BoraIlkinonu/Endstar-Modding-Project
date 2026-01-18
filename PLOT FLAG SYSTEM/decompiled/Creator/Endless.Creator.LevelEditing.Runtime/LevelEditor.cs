using System;
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

namespace Endless.Creator.LevelEditing.Runtime;

public class LevelEditor : NetworkBehaviour
{
	[ServerRpc(RequireOwnership = false)]
	public void UpdateLevelName_ServerRpc(string newLevelName, ServerRpcParams serverRpcParams = default(ServerRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			FastBufferWriter bufferWriter = __beginSendServerRpc(97940106u, serverRpcParams, RpcDelivery.Reliable);
			bool value = newLevelName != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(newLevelName);
			}
			__endSendServerRpc(ref bufferWriter, 97940106u, serverRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			UpdateLevelName(newLevelName, serverRpcParams);
		}
	}

	private async Task UpdateLevelName(string newLevelName, ServerRpcParams serverRpcParams)
	{
		if (!base.IsServer)
		{
			return;
		}
		if (!NetworkBehaviourSingleton<UserIdManager>.Instance.TryGetUserId(serverRpcParams.Receive.SenderClientId, out var userId))
		{
			Debug.LogException(new Exception($"Unable to determine User Id for Client Id: {serverRpcParams.Receive.SenderClientId}"));
		}
		else if (await NetworkBehaviourSingleton<CreatorManager>.Instance.UserCanEditLevel(userId))
		{
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.RevisionTimestamp = DateTime.UtcNow.Ticks;
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.Changes.Add(new ChangeData
			{
				ChangeType = ChangeType.LevelNameUpdated,
				UserId = userId
			});
			UpdateLevelName_ClientRpc(newLevelName);
			if (!base.IsClient)
			{
				UpdateLevelName(newLevelName);
			}
			NetworkBehaviourSingleton<CreatorManager>.Instance.SaveLoadManager.SaveLevel();
		}
	}

	[ClientRpc]
	private void UpdateLevelName_ClientRpc(string newLevelName)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(1717172957u, clientRpcParams, RpcDelivery.Reliable);
			bool value = newLevelName != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(newLevelName);
			}
			__endSendClientRpc(ref bufferWriter, 1717172957u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			UpdateLevelName(newLevelName);
		}
	}

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

	[ServerRpc(RequireOwnership = false)]
	public void UpdateLevelDescription_ServerRpc(string newLevelDescription, ServerRpcParams serverRpcParams = default(ServerRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			FastBufferWriter bufferWriter = __beginSendServerRpc(3198977598u, serverRpcParams, RpcDelivery.Reliable);
			bool value = newLevelDescription != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(newLevelDescription);
			}
			__endSendServerRpc(ref bufferWriter, 3198977598u, serverRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			UpdateLevelDescription(newLevelDescription, serverRpcParams);
		}
	}

	private async Task UpdateLevelDescription(string newLevelDescription, ServerRpcParams serverRpcParams)
	{
		if (!base.IsServer)
		{
			return;
		}
		if (!NetworkBehaviourSingleton<UserIdManager>.Instance.TryGetUserId(serverRpcParams.Receive.SenderClientId, out var userId))
		{
			Debug.LogException(new Exception($"Unable to determine User Id for Client Id: {serverRpcParams.Receive.SenderClientId}"));
		}
		else if (await NetworkBehaviourSingleton<CreatorManager>.Instance.UserCanEditLevel(userId))
		{
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.RevisionTimestamp = DateTime.UtcNow.Ticks;
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.Changes.Add(new ChangeData
			{
				ChangeType = ChangeType.LevelDescriptionUpdated,
				UserId = userId
			});
			UpdateLevelDescription_ClientRpc(newLevelDescription);
			if (!base.IsClient)
			{
				UpdateLevelDescription(newLevelDescription);
			}
			NetworkBehaviourSingleton<CreatorManager>.Instance.SaveLoadManager.SaveLevel();
		}
	}

	[ClientRpc]
	private void UpdateLevelDescription_ClientRpc(string newLevelName)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(339205854u, clientRpcParams, RpcDelivery.Reliable);
			bool value = newLevelName != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(newLevelName);
			}
			__endSendClientRpc(ref bufferWriter, 339205854u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			UpdateLevelDescription(newLevelName);
		}
	}

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

	[ServerRpc(RequireOwnership = false)]
	public void UpdateLevelArchivedState_ServerRpc(bool archiveState, ServerRpcParams serverRpcParams = default(ServerRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter bufferWriter = __beginSendServerRpc(1442060746u, serverRpcParams, RpcDelivery.Reliable);
				bufferWriter.WriteValueSafe(in archiveState, default(FastBufferWriter.ForPrimitives));
				__endSendServerRpc(ref bufferWriter, 1442060746u, serverRpcParams, RpcDelivery.Reliable);
			}
			if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				__rpc_exec_stage = __RpcExecStage.Send;
				UpdateLevelArchivedState(archiveState, serverRpcParams);
			}
		}
	}

	private async Task UpdateLevelArchivedState(bool archiveState, ServerRpcParams serverRpcParams)
	{
		if (!base.IsServer)
		{
			return;
		}
		if (!NetworkBehaviourSingleton<UserIdManager>.Instance.TryGetUserId(serverRpcParams.Receive.SenderClientId, out var userId))
		{
			Debug.LogException(new Exception($"Unable to determine User Id for Client Id: {serverRpcParams.Receive.SenderClientId}"));
		}
		else if (await NetworkBehaviourSingleton<CreatorManager>.Instance.UserCanEditLevel(userId))
		{
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.RevisionTimestamp = DateTime.UtcNow.Ticks;
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.Changes.Add(new ChangeData
			{
				ChangeType = ChangeType.LevelArchived,
				UserId = userId
			});
			UpdateLevelArchivedState_ClientRpc(archiveState);
			if (!base.IsClient)
			{
				UpdateLevelArchivedState(archiveState);
			}
			NetworkBehaviourSingleton<CreatorManager>.Instance.SaveLoadManager.SaveLevel();
		}
	}

	[ClientRpc]
	private void UpdateLevelArchivedState_ClientRpc(bool archiveState)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams = default(ClientRpcParams);
				FastBufferWriter bufferWriter = __beginSendClientRpc(2710954523u, clientRpcParams, RpcDelivery.Reliable);
				bufferWriter.WriteValueSafe(in archiveState, default(FastBufferWriter.ForPrimitives));
				__endSendClientRpc(ref bufferWriter, 2710954523u, clientRpcParams, RpcDelivery.Reliable);
			}
			if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				__rpc_exec_stage = __RpcExecStage.Send;
				UpdateLevelArchivedState(archiveState);
			}
		}
	}

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

	[ServerRpc(RequireOwnership = false)]
	public void UpdateLevelSpawnPointOrder_ServerRpc(SerializableGuid[] spawnPointIdOrder, SerializableGuid[] selectedSpawnPoints, ServerRpcParams serverRpcParams = default(ServerRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			FastBufferWriter bufferWriter = __beginSendServerRpc(3804119171u, serverRpcParams, RpcDelivery.Reliable);
			bool value = spawnPointIdOrder != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(spawnPointIdOrder, default(FastBufferWriter.ForNetworkSerializable));
			}
			bool value2 = selectedSpawnPoints != null;
			bufferWriter.WriteValueSafe(in value2, default(FastBufferWriter.ForPrimitives));
			if (value2)
			{
				bufferWriter.WriteValueSafe(selectedSpawnPoints, default(FastBufferWriter.ForNetworkSerializable));
			}
			__endSendServerRpc(ref bufferWriter, 3804119171u, serverRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			UpdateLevelSpawnPointOrder(spawnPointIdOrder, selectedSpawnPoints, serverRpcParams);
		}
	}

	private async Task UpdateLevelSpawnPointOrder(SerializableGuid[] spawnPointIdOrder, SerializableGuid[] selectedSpawnPoints, ServerRpcParams serverRpcParams)
	{
		if (!base.IsServer)
		{
			return;
		}
		if (!NetworkBehaviourSingleton<UserIdManager>.Instance.TryGetUserId(serverRpcParams.Receive.SenderClientId, out var userId))
		{
			Debug.LogException(new Exception($"Unable to determine User Id for Client Id: {serverRpcParams.Receive.SenderClientId}"));
		}
		else
		{
			if (!(await NetworkBehaviourSingleton<CreatorManager>.Instance.UserCanEditLevel(userId)))
			{
				return;
			}
			if (!MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.UpdateSpawnPointOrder(spawnPointIdOrder))
			{
				Debug.LogWarning("Unable to update spawn point order");
				return;
			}
			if (!MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.UpdateSelectedSpawnPoints(selectedSpawnPoints))
			{
				Debug.LogWarning("Unable to update selected spawn points");
				return;
			}
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.RevisionTimestamp = DateTime.UtcNow.Ticks;
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.Changes.Add(new ChangeData
			{
				ChangeType = ChangeType.LevelUpdatedSpawnPositionOrder,
				UserId = userId
			});
			UpdateLevelSpawnPointOrder_ClientRpc(spawnPointIdOrder, selectedSpawnPoints);
			if (!base.IsClient)
			{
				UpdateLevelSpawnPointOrder(spawnPointIdOrder, selectedSpawnPoints);
			}
			NetworkBehaviourSingleton<CreatorManager>.Instance.SaveLoadManager.SaveLevel();
		}
	}

	[ClientRpc]
	private void UpdateLevelSpawnPointOrder_ClientRpc(SerializableGuid[] spawnPointIdOrder, SerializableGuid[] selectedSpawnPointIds)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(1546701494u, clientRpcParams, RpcDelivery.Reliable);
			bool value = spawnPointIdOrder != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(spawnPointIdOrder, default(FastBufferWriter.ForNetworkSerializable));
			}
			bool value2 = selectedSpawnPointIds != null;
			bufferWriter.WriteValueSafe(in value2, default(FastBufferWriter.ForPrimitives));
			if (value2)
			{
				bufferWriter.WriteValueSafe(selectedSpawnPointIds, default(FastBufferWriter.ForNetworkSerializable));
			}
			__endSendClientRpc(ref bufferWriter, 1546701494u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if (!base.IsServer)
			{
				UpdateLevelSpawnPointOrder(spawnPointIdOrder, selectedSpawnPointIds);
			}
		}
	}

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
			}
			return;
		}
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

	[ServerRpc(RequireOwnership = false)]
	public void RevertLevelToVersion_ServerRpc(string versionId, ServerRpcParams serverRpcParams = default(ServerRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			FastBufferWriter bufferWriter = __beginSendServerRpc(3966572557u, serverRpcParams, RpcDelivery.Reliable);
			bool value = versionId != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(versionId);
			}
			__endSendServerRpc(ref bufferWriter, 3966572557u, serverRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			RevertLevelToVersion(versionId, serverRpcParams);
		}
	}

	private async Task RevertLevelToVersion(string versionId, ServerRpcParams serverRpcParams)
	{
		if (base.IsServer)
		{
			if (!NetworkBehaviourSingleton<UserIdManager>.Instance.TryGetUserId(serverRpcParams.Receive.SenderClientId, out var userId))
			{
				Debug.LogException(new Exception($"Unable to determine User Id for Client Id: {serverRpcParams.Receive.SenderClientId}"));
			}
			else if (await NetworkBehaviourSingleton<CreatorManager>.Instance.UserCanEditLevel(userId))
			{
				RevertLevel(versionId, userId);
			}
		}
	}

	private async void RevertLevel(string versionId, int userId)
	{
		_ = 2;
		try
		{
			Debug.Log("Forcing save if needed");
			if (NetworkBehaviourSingleton<CreatorManager>.Instance.SaveLoadManager.PendingSave)
			{
				Debug.Log("Save forced");
				await NetworkBehaviourSingleton<CreatorManager>.Instance.SaveLoadManager.ForceSaveIfNeededAsync();
			}
			Debug.Log("Retrieving old version");
			LevelState levelState = LevelStateLoader.Load((await EndlessServices.Instance.CloudService.GetAssetAsync(MonoBehaviourSingleton<StageManager>.Instance.ActiveLevelGuid, versionId)).GetDataMember().ToString());
			levelState.RevisionMetaData.Changes.Clear();
			levelState.RevisionMetaData.RevisionTimestamp = DateTime.UtcNow.Ticks;
			levelState.RevisionMetaData.Changes.Add(new ChangeData
			{
				ChangeType = ChangeType.AssetVersionUpdated,
				UserId = userId,
				Metadata = versionId
			});
			Debug.Log("Saving old version to force update");
			await EndlessServices.Instance.CloudService.UpdateAssetAsync(levelState.GetAnonymousObjectForUpload(), MatchmakingClientController.Instance.ActiveGameId);
			Debug.Log("Save complete, requesting server to reload");
			NetworkBehaviourSingleton<CreatorManager>.Instance.ForcePlayersToReload_ServerRpc();
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
			ErrorHandler.HandleError(ErrorCodes.LevelEditor_RevertLevel, new Exception(ex.Message));
			MatchmakingClientController.Instance.EndMatch();
		}
	}

	protected override void __initializeVariables()
	{
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		__registerRpc(97940106u, __rpc_handler_97940106, "UpdateLevelName_ServerRpc");
		__registerRpc(1717172957u, __rpc_handler_1717172957, "UpdateLevelName_ClientRpc");
		__registerRpc(3198977598u, __rpc_handler_3198977598, "UpdateLevelDescription_ServerRpc");
		__registerRpc(339205854u, __rpc_handler_339205854, "UpdateLevelDescription_ClientRpc");
		__registerRpc(1442060746u, __rpc_handler_1442060746, "UpdateLevelArchivedState_ServerRpc");
		__registerRpc(2710954523u, __rpc_handler_2710954523, "UpdateLevelArchivedState_ClientRpc");
		__registerRpc(3804119171u, __rpc_handler_3804119171, "UpdateLevelSpawnPointOrder_ServerRpc");
		__registerRpc(1546701494u, __rpc_handler_1546701494, "UpdateLevelSpawnPointOrder_ClientRpc");
		__registerRpc(3966572557u, __rpc_handler_3966572557, "RevertLevelToVersion_ServerRpc");
		base.__initializeRpcs();
	}

	private static void __rpc_handler_97940106(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
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
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((LevelEditor)target).UpdateLevelName_ServerRpc(s, server);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_1717172957(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
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
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((LevelEditor)target).UpdateLevelName_ClientRpc(s);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_3198977598(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
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
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((LevelEditor)target).UpdateLevelDescription_ServerRpc(s, server);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_339205854(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
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
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((LevelEditor)target).UpdateLevelDescription_ClientRpc(s);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_1442060746(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((LevelEditor)target).UpdateLevelArchivedState_ServerRpc(value, server);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_2710954523(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((LevelEditor)target).UpdateLevelArchivedState_ClientRpc(value);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_3804119171(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
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
			reader.ReadValueSafe(out bool value3, default(FastBufferWriter.ForPrimitives));
			SerializableGuid[] value4 = null;
			if (value3)
			{
				reader.ReadValueSafe(out value4, default(FastBufferWriter.ForNetworkSerializable));
			}
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((LevelEditor)target).UpdateLevelSpawnPointOrder_ServerRpc(value2, value4, server);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_1546701494(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
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
			reader.ReadValueSafe(out bool value3, default(FastBufferWriter.ForPrimitives));
			SerializableGuid[] value4 = null;
			if (value3)
			{
				reader.ReadValueSafe(out value4, default(FastBufferWriter.ForNetworkSerializable));
			}
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((LevelEditor)target).UpdateLevelSpawnPointOrder_ClientRpc(value2, value4);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_3966572557(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
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
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((LevelEditor)target).RevertLevelToVersion_ServerRpc(s, server);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	protected internal override string __getTypeName()
	{
		return "LevelEditor";
	}
}
