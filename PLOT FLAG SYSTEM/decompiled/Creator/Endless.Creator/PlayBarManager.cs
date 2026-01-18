using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay;
using Endless.Gameplay.RightsManagement;
using Endless.Matchmaking;
using Endless.Networking;
using Endless.Shared;
using Runtime.Shared.Matchmaking;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator;

public class PlayBarManager : EndlessNetworkBehaviourSingleton<PlayBarManager>
{
	public enum PlayerReadyStatus
	{
		None,
		NotReady,
		Ready
	}

	public enum PlayOption
	{
		None,
		PlayWithParty,
		PlaySolo,
		StopSolo,
		StopWithParty
	}

	[HideInInspector]
	public UnityEvent<int, PlayerReadyStatus, PlayerReadyStatus> OnPlayerReadyChanged = new UnityEvent<int, PlayerReadyStatus, PlayerReadyStatus>();

	[HideInInspector]
	public UnityEvent OnMatchTimerUpdated = new UnityEvent();

	[SerializeField]
	private float matchTransitionDuration = 30f;

	[SerializeField]
	private float allReadyDuration = 5f;

	[SerializeField]
	private float soloDuration = 1f;

	private HashSet<int> readyUserIds = new HashSet<int>();

	private Coroutine timerCoroutine;

	private bool IsInParty
	{
		get
		{
			if (MatchmakingClientController.Instance.LocalGroup != null)
			{
				return MatchmakingClientController.Instance.LocalGroup.Members.Count != 1;
			}
			return false;
		}
	}

	public bool IsTimerStarted => MatchTransitionTime != 0f;

	public float MatchTransitionTime { get; private set; }

	public bool IsLocalPlayerReady => readyUserIds.Contains(EndlessServices.Instance.CloudService.ActiveUserId);

	private bool IsPartyHost
	{
		get
		{
			if (!IsInParty)
			{
				return false;
			}
			ClientData value = MatchmakingClientController.Instance.LocalClientData.Value;
			CoreClientData host = MatchmakingClientController.Instance.LocalGroup.Host;
			return value.CoreDataEquals(host);
		}
	}

	public bool IsLocalClientReady => readyUserIds.Contains(EndlessServices.Instance.CloudService.ActiveUserId);

	protected override void Start()
	{
		base.Start();
		MonoBehaviourSingleton<GameplayManager>.Instance.OnGameplayStarted.AddListener(HandleGameplayStarted);
		NetworkBehaviourSingleton<CreatorManager>.Instance.OnCreatorStarted.AddListener(HandleCreatorStarted);
		NetworkBehaviourSingleton<UserIdManager>.Instance.OnUserIdAdded.AddListener(HandleUserAdded);
		NetworkBehaviourSingleton<UserIdManager>.Instance.OnUserIdRemoved.AddListener(HandleUserRemoved);
	}

	private void HandleCreatorStarted()
	{
		if (base.IsServer)
		{
			UnreadyUserIdsServerOnly(readyUserIds.ToArray());
		}
	}

	private void HandleGameplayStarted()
	{
		if (base.IsServer)
		{
			UnreadyUserIdsServerOnly(readyUserIds.ToArray());
		}
	}

	public void PlayHit()
	{
		if (IsLocalPlayerReady)
		{
			if (!IsInParty)
			{
				UnplayHit_ServerRpc();
			}
			else if (IsPartyHost)
			{
				UnplayHit_PartyLeader_ServerRpc(GetPartyUserIds());
			}
		}
		else if (!IsInParty)
		{
			PlayHit_ServerRpc();
		}
		else if (IsPartyHost)
		{
			PlayHit_PartyLeader_ServerRpc(GetPartyUserIds());
		}
	}

	private static int[] GetPartyUserIds()
	{
		return MatchmakingClientController.Instance.LocalGroup.Members.Select((CoreClientData member) => int.Parse(member.PlatformId)).ToArray();
	}

	public PlayOption GetLocalPlayOption()
	{
		if (MatchmakingClientController.Instance.LocalGroup != null)
		{
			_ = MatchmakingClientController.Instance.LocalGroup.Members.Count;
		}
		PlayOption result = PlayOption.None;
		if (NetworkBehaviourSingleton<UserIdManager>.Instance.UserCount == 1)
		{
			return result;
		}
		if (MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying)
		{
			if (base.IsHost)
			{
				return result;
			}
			if (!IsInParty)
			{
				return PlayOption.StopSolo;
			}
			if (IsPartyHost)
			{
				return PlayOption.StopWithParty;
			}
		}
		else
		{
			if (!IsInParty)
			{
				return PlayOption.PlaySolo;
			}
			if (IsPartyHost)
			{
				return PlayOption.PlayWithParty;
			}
		}
		return result;
	}

	public void ExecuteLocalPlayOption()
	{
		switch (GetLocalPlayOption())
		{
		case PlayOption.PlaySolo:
		case PlayOption.StopSolo:
			ChangeMatchImmediateSolo_ServerRpc();
			break;
		case PlayOption.PlayWithParty:
		case PlayOption.StopWithParty:
			ChangeMatchImmediateParty_ServerRpc(GetPartyUserIds());
			break;
		}
	}

	[ServerRpc(RequireOwnership = false)]
	private void ChangeMatchImmediateSolo_ServerRpc(ServerRpcParams serverRpcParams = default(ServerRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter bufferWriter = __beginSendServerRpc(1311848200u, serverRpcParams, RpcDelivery.Reliable);
				__endSendServerRpc(ref bufferWriter, 1311848200u, serverRpcParams, RpcDelivery.Reliable);
			}
			if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				__rpc_exec_stage = __RpcExecStage.Send;
				int userId = NetworkBehaviourSingleton<UserIdManager>.Instance.GetUserId(serverRpcParams.Receive.SenderClientId);
				TriggerChangeMatch(new int[1] { userId }, userId == EndlessServices.Instance.CloudService.ActiveUserId);
			}
		}
	}

	[ServerRpc(RequireOwnership = false)]
	private void ChangeMatchImmediateParty_ServerRpc(int[] partyUserIds, ServerRpcParams serverRpcParams = default(ServerRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			FastBufferWriter bufferWriter = __beginSendServerRpc(2535256494u, serverRpcParams, RpcDelivery.Reliable);
			bool value = partyUserIds != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(partyUserIds, default(FastBufferWriter.ForPrimitives));
			}
			__endSendServerRpc(ref bufferWriter, 2535256494u, serverRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			int userId = NetworkBehaviourSingleton<UserIdManager>.Instance.GetUserId(serverRpcParams.Receive.SenderClientId);
			if (partyUserIds.Contains(userId))
			{
				TriggerChangeMatch(partyUserIds, partyUserIds.Contains(EndlessServices.Instance.CloudService.ActiveUserId));
			}
		}
	}

	private void TimerFinished()
	{
		Debug.Log("Play bar timer finished");
		bool flag = readyUserIds.Contains(EndlessServices.Instance.CloudService.ActiveUserId);
		HashSet<int> source = ((!flag || !base.IsHost || !MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying) ? new HashSet<int>(readyUserIds) : NetworkBehaviourSingleton<UserIdManager>.Instance.ConnectedUserIds.ToHashSet());
		ClearMatchTimerServerOnly();
		TriggerChangeMatch(source.ToArray(), flag);
	}

	private async void TriggerChangeMatch(int[] userIds, bool isPlayerLeaving)
	{
		string gameId = MatchSession.Instance.MatchData.ProjectId;
		string levelId = MatchSession.Instance.MatchData.LevelId;
		if (isPlayerLeaving)
		{
			MonoBehaviourSingleton<LoadTimeTester>.Instance.StartTracking("MatchLoad");
		}
		if (MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying)
		{
			List<int> list = (await MonoBehaviourSingleton<RightsManager>.Instance.GetAllUserRolesForAssetAsync(gameId)).Roles.Select((UserRole role) => role.UserId).ToList();
			List<int> list2 = new List<int>();
			foreach (int num2 in userIds)
			{
				if (list.Contains(num2))
				{
					list2.Add(num2);
					continue;
				}
				Debug.Log($"Kicking player: {num2}");
				ulong clientId = NetworkBehaviourSingleton<UserIdManager>.Instance.GetClientId(num2);
				MatchSession.Instance.KickClient(clientId, "The playtest has ended and you do not have edit rights to the game.");
			}
			if (list2.Count > 0)
			{
				string[] userIds2 = MatchmakingClientController.FormatUserIdsForChangeMatch(list2);
				MatchmakingClientController.Instance.ChangeMatch(gameId, levelId, isEditSession: true, string.Empty, null, null, endMatch: false, OnMatchChangeFailed, userIds2);
			}
		}
		else
		{
			string assetVersion = MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.AssetVersion;
			string[] userIds3 = MatchmakingClientController.FormatUserIdsForChangeMatch(userIds.ToList());
			MatchmakingClientController.Instance.ChangeMatch(gameId, levelId, isEditSession: false, assetVersion, null, MatchDataExtensions.BuildPlaytestSessionData(), endMatch: false, OnMatchChangeFailed, userIds3);
		}
	}

	private void OnMatchChangeFailed(int arg1, string arg2)
	{
		Debug.LogError($"Failed to change match {arg1}: {arg2}");
	}

	[ClientRpc]
	private void ReadyPlayers_ClientRpc(int[] userIds)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(1970652522u, clientRpcParams, RpcDelivery.Reliable);
			bool value = userIds != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(userIds, default(FastBufferWriter.ForPrimitives));
			}
			__endSendClientRpc(ref bufferWriter, 1970652522u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			foreach (int userId in userIds)
			{
				UserChangedState(userId, PlayerReadyStatus.NotReady, PlayerReadyStatus.Ready);
			}
		}
	}

	private void UpdateMatchTimerServerOnly()
	{
		int count = readyUserIds.Count;
		if (count > 0)
		{
			int num = NetworkBehaviourSingleton<UserIdManager>.Instance.ConnectedUserIds.Count();
			if (!IsTimerStarted)
			{
				float num2 = ((num == 1) ? soloDuration : ((count == num) ? allReadyDuration : matchTransitionDuration));
				MatchTransitionTime = NetworkManager.Singleton.ServerTime.TimeAsFloat + num2;
				UpdateMatchTimer_ClientRpc(MatchTransitionTime);
				timerCoroutine = StartCoroutine(TimerRoutine(num2));
			}
			else if (count == num && MatchTransitionTime - NetworkManager.Singleton.ServerTime.TimeAsFloat > allReadyDuration)
			{
				MatchTransitionTime = NetworkManager.Singleton.ServerTime.TimeAsFloat + allReadyDuration;
				UpdateMatchTimer_ClientRpc(MatchTransitionTime);
				ClearTimerCoroutineServerOnly();
				timerCoroutine = StartCoroutine(TimerRoutine(allReadyDuration));
			}
		}
	}

	private IEnumerator TimerRoutine(float duration)
	{
		yield return new WaitForSecondsRealtime(duration);
		TimerFinished();
	}

	private void ClearMatchTimerServerOnly()
	{
		if (IsTimerStarted)
		{
			MatchTransitionTime = 0f;
			UpdateMatchTimer_ClientRpc(MatchTransitionTime);
			ClearTimerCoroutineServerOnly();
		}
	}

	private void ClearTimerCoroutineServerOnly()
	{
		if (timerCoroutine != null)
		{
			StopCoroutine(timerCoroutine);
			timerCoroutine = null;
		}
	}

	[ClientRpc]
	private void UpdateMatchTimer_ClientRpc(float matchTransitionTime)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(844158908u, clientRpcParams, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in matchTransitionTime, default(FastBufferWriter.ForPrimitives));
			__endSendClientRpc(ref bufferWriter, 844158908u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if (!base.IsServer)
			{
				MatchTransitionTime = matchTransitionTime;
			}
			OnMatchTimerUpdated.Invoke();
		}
	}

	[ServerRpc(RequireOwnership = false)]
	private void PlayHit_ServerRpc(ServerRpcParams serverRpcParams = default(ServerRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			FastBufferWriter bufferWriter = __beginSendServerRpc(4248026517u, serverRpcParams, RpcDelivery.Reliable);
			__endSendServerRpc(ref bufferWriter, 4248026517u, serverRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			int userId = NetworkBehaviourSingleton<UserIdManager>.Instance.GetUserId(serverRpcParams.Receive.SenderClientId);
			if (base.IsHost && userId == EndlessServices.Instance.CloudService.ActiveUserId && MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying)
			{
				ForceReadyAll_SeverOnly();
				return;
			}
			readyUserIds.Add(userId);
			HandlePlayerReadyStatusChanged_ClientRpc(userId, PlayerReadyStatus.NotReady, PlayerReadyStatus.Ready);
			UpdateMatchTimerServerOnly();
		}
	}

	private void ForceReadyAll_SeverOnly()
	{
		int[] array = NetworkBehaviourSingleton<UserIdManager>.Instance.ConnectedUserIds.Except(readyUserIds).ToArray();
		int[] array2 = array;
		foreach (int item in array2)
		{
			readyUserIds.Add(item);
		}
		ReadyPlayers_ClientRpc(array);
		UpdateMatchTimerServerOnly();
	}

	[ServerRpc(RequireOwnership = false)]
	private void PlayHit_PartyLeader_ServerRpc(int[] userIds, ServerRpcParams serverRpcParams = default(ServerRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			FastBufferWriter bufferWriter = __beginSendServerRpc(1740879778u, serverRpcParams, RpcDelivery.Reliable);
			bool value = userIds != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(userIds, default(FastBufferWriter.ForPrimitives));
			}
			__endSendServerRpc(ref bufferWriter, 1740879778u, serverRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
		{
			return;
		}
		__rpc_exec_stage = __RpcExecStage.Send;
		int userId = NetworkBehaviourSingleton<UserIdManager>.Instance.GetUserId(serverRpcParams.Receive.SenderClientId);
		if (!userIds.Contains(userId))
		{
			return;
		}
		if (base.IsHost && userId == EndlessServices.Instance.CloudService.ActiveUserId && MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying)
		{
			ForceReadyAll_SeverOnly();
			return;
		}
		foreach (int item in userIds)
		{
			readyUserIds.Add(item);
		}
		ReadyPlayers_ClientRpc(userIds);
		UpdateMatchTimerServerOnly();
	}

	[ServerRpc(RequireOwnership = false)]
	private void UnplayHit_ServerRpc(ServerRpcParams serverRpcParams = default(ServerRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			FastBufferWriter bufferWriter = __beginSendServerRpc(380088622u, serverRpcParams, RpcDelivery.Reliable);
			__endSendServerRpc(ref bufferWriter, 380088622u, serverRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
		{
			return;
		}
		__rpc_exec_stage = __RpcExecStage.Send;
		int userId = NetworkBehaviourSingleton<UserIdManager>.Instance.GetUserId(serverRpcParams.Receive.SenderClientId);
		if (readyUserIds.Remove(userId))
		{
			HandlePlayerReadyStatusChanged_ClientRpc(userId, PlayerReadyStatus.Ready, PlayerReadyStatus.NotReady);
			if (readyUserIds.Count == 0)
			{
				ClearMatchTimerServerOnly();
			}
		}
	}

	[ServerRpc(RequireOwnership = false)]
	private void UnplayHit_PartyLeader_ServerRpc(int[] userIds, ServerRpcParams serverRpcParams = default(ServerRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			FastBufferWriter bufferWriter = __beginSendServerRpc(2524213796u, serverRpcParams, RpcDelivery.Reliable);
			bool value = userIds != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(userIds, default(FastBufferWriter.ForPrimitives));
			}
			__endSendServerRpc(ref bufferWriter, 2524213796u, serverRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			int userId = NetworkBehaviourSingleton<UserIdManager>.Instance.GetUserId(serverRpcParams.Receive.SenderClientId);
			if (userIds.Contains(userId))
			{
				UnreadyUserIdsServerOnly(userIds);
			}
		}
	}

	private void UnreadyUserIdsServerOnly(int[] userIds)
	{
		foreach (int item in userIds)
		{
			readyUserIds.Remove(item);
		}
		UnreadyPlayers_ClientRpc(userIds);
		if (readyUserIds.Count == 0)
		{
			ClearMatchTimerServerOnly();
		}
	}

	[ClientRpc]
	private void UnreadyPlayers_ClientRpc(int[] userIds)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(3062243066u, clientRpcParams, RpcDelivery.Reliable);
			bool value = userIds != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(userIds, default(FastBufferWriter.ForPrimitives));
			}
			__endSendClientRpc(ref bufferWriter, 3062243066u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			foreach (int userId in userIds)
			{
				UserChangedState(userId, PlayerReadyStatus.Ready, PlayerReadyStatus.NotReady);
			}
		}
	}

	protected override void OnSynchronize<T>(ref BufferSerializer<T> serializer)
	{
		base.OnSynchronize(ref serializer);
		float value = MatchTransitionTime;
		serializer.SerializeValue(ref value, default(FastBufferWriter.ForPrimitives));
		if (serializer.IsReader)
		{
			MatchTransitionTime = value;
		}
		int value2 = 0;
		if (serializer.IsWriter)
		{
			value2 = readyUserIds.Count;
		}
		serializer.SerializeValue(ref value2, default(FastBufferWriter.ForPrimitives));
		int[] array = new int[value2];
		if (serializer.IsWriter)
		{
			array = readyUserIds.ToArray();
		}
		for (int i = 0; i < value2; i++)
		{
			serializer.SerializeValue(ref array[i], default(FastBufferWriter.ForPrimitives));
			if (serializer.IsReader)
			{
				if (NetworkBehaviourSingleton<UserIdManager>.Instance.ConnectedUserIds.Contains(array[i]))
				{
					OnPlayerReadyChanged.Invoke(array[i], PlayerReadyStatus.NotReady, PlayerReadyStatus.Ready);
				}
				else
				{
					OnPlayerReadyChanged.Invoke(array[i], PlayerReadyStatus.None, PlayerReadyStatus.Ready);
				}
			}
		}
		if (serializer.IsReader)
		{
			readyUserIds = array.ToHashSet();
		}
	}

	private void HandleUserAdded(int userId)
	{
		if (!readyUserIds.Contains(userId))
		{
			UserChangedState(userId, PlayerReadyStatus.None, PlayerReadyStatus.NotReady);
		}
	}

	private void HandleUserRemoved(int userId)
	{
		if (readyUserIds.Remove(userId))
		{
			UserChangedState(userId, PlayerReadyStatus.Ready, PlayerReadyStatus.None);
			if (base.IsServer)
			{
				if (readyUserIds.Count == 0)
				{
					ClearMatchTimerServerOnly();
				}
				else
				{
					UpdateMatchTimerServerOnly();
				}
			}
		}
		else
		{
			UserChangedState(userId, PlayerReadyStatus.NotReady, PlayerReadyStatus.None);
		}
	}

	[ClientRpc]
	private void HandlePlayerReadyStatusChanged_ClientRpc(int userId, PlayerReadyStatus previousStatus, PlayerReadyStatus newStatus)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams = default(ClientRpcParams);
				FastBufferWriter bufferWriter = __beginSendClientRpc(28542295u, clientRpcParams, RpcDelivery.Reliable);
				BytePacker.WriteValueBitPacked(bufferWriter, userId);
				bufferWriter.WriteValueSafe(in previousStatus, default(FastBufferWriter.ForEnums));
				bufferWriter.WriteValueSafe(in newStatus, default(FastBufferWriter.ForEnums));
				__endSendClientRpc(ref bufferWriter, 28542295u, clientRpcParams, RpcDelivery.Reliable);
			}
			if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				__rpc_exec_stage = __RpcExecStage.Send;
				UserChangedState(userId, previousStatus, newStatus);
			}
		}
	}

	private void UserChangedState(int userId, PlayerReadyStatus previousStatus, PlayerReadyStatus newStatus)
	{
		if (!base.IsServer)
		{
			if (previousStatus == PlayerReadyStatus.Ready)
			{
				readyUserIds.Remove(userId);
			}
			else if (newStatus == PlayerReadyStatus.Ready)
			{
				readyUserIds.Add(userId);
			}
		}
		OnPlayerReadyChanged.Invoke(userId, previousStatus, newStatus);
	}

	protected override void __initializeVariables()
	{
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		__registerRpc(1311848200u, __rpc_handler_1311848200, "ChangeMatchImmediateSolo_ServerRpc");
		__registerRpc(2535256494u, __rpc_handler_2535256494, "ChangeMatchImmediateParty_ServerRpc");
		__registerRpc(1970652522u, __rpc_handler_1970652522, "ReadyPlayers_ClientRpc");
		__registerRpc(844158908u, __rpc_handler_844158908, "UpdateMatchTimer_ClientRpc");
		__registerRpc(4248026517u, __rpc_handler_4248026517, "PlayHit_ServerRpc");
		__registerRpc(1740879778u, __rpc_handler_1740879778, "PlayHit_PartyLeader_ServerRpc");
		__registerRpc(380088622u, __rpc_handler_380088622, "UnplayHit_ServerRpc");
		__registerRpc(2524213796u, __rpc_handler_2524213796, "UnplayHit_PartyLeader_ServerRpc");
		__registerRpc(3062243066u, __rpc_handler_3062243066, "UnreadyPlayers_ClientRpc");
		__registerRpc(28542295u, __rpc_handler_28542295, "HandlePlayerReadyStatusChanged_ClientRpc");
		base.__initializeRpcs();
	}

	private static void __rpc_handler_1311848200(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((PlayBarManager)target).ChangeMatchImmediateSolo_ServerRpc(server);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_2535256494(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			int[] value2 = null;
			if (value)
			{
				reader.ReadValueSafe(out value2, default(FastBufferWriter.ForPrimitives));
			}
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((PlayBarManager)target).ChangeMatchImmediateParty_ServerRpc(value2, server);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_1970652522(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			int[] value2 = null;
			if (value)
			{
				reader.ReadValueSafe(out value2, default(FastBufferWriter.ForPrimitives));
			}
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((PlayBarManager)target).ReadyPlayers_ClientRpc(value2);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_844158908(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out float value, default(FastBufferWriter.ForPrimitives));
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((PlayBarManager)target).UpdateMatchTimer_ClientRpc(value);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_4248026517(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((PlayBarManager)target).PlayHit_ServerRpc(server);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_1740879778(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			int[] value2 = null;
			if (value)
			{
				reader.ReadValueSafe(out value2, default(FastBufferWriter.ForPrimitives));
			}
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((PlayBarManager)target).PlayHit_PartyLeader_ServerRpc(value2, server);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_380088622(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((PlayBarManager)target).UnplayHit_ServerRpc(server);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_2524213796(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			int[] value2 = null;
			if (value)
			{
				reader.ReadValueSafe(out value2, default(FastBufferWriter.ForPrimitives));
			}
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((PlayBarManager)target).UnplayHit_PartyLeader_ServerRpc(value2, server);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_3062243066(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			int[] value2 = null;
			if (value)
			{
				reader.ReadValueSafe(out value2, default(FastBufferWriter.ForPrimitives));
			}
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((PlayBarManager)target).UnreadyPlayers_ClientRpc(value2);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_28542295(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			ByteUnpacker.ReadValueBitPacked(reader, out int value);
			reader.ReadValueSafe(out PlayerReadyStatus value2, default(FastBufferWriter.ForEnums));
			reader.ReadValueSafe(out PlayerReadyStatus value3, default(FastBufferWriter.ForEnums));
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((PlayBarManager)target).HandlePlayerReadyStatusChanged_ClientRpc(value, value2, value3);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	protected internal override string __getTypeName()
	{
		return "PlayBarManager";
	}
}
