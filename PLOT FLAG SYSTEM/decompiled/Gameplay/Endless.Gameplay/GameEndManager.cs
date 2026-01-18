using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay.Stats;
using Endless.Gameplay.UI;
using Endless.Matchmaking;
using Endless.Shared;
using Runtime.Shared;
using Runtime.Shared.Matchmaking;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay;

public class GameEndManager : NetworkBehaviourSingleton<GameEndManager>
{
	public enum VoteType
	{
		None,
		NextLevel,
		EndMatch,
		Replay
	}

	[SerializeField]
	private UIGameOverWindow gameOverWindowPrefab;

	private List<int> nextLevelVotes = new List<int>();

	private List<int> endMatchVotes = new List<int>();

	private List<int> replayVotes = new List<int>();

	private GameEndBlock gameEndBlock;

	private UIGameOverWindow currentUiWindow;

	private bool isActive;

	public UnityEvent<int, VoteType, VoteType> OnVoteMoved = new UnityEvent<int, VoteType, VoteType>();

	public IReadOnlyList<int> NextLevelVotes => nextLevelVotes;

	public IReadOnlyList<int> EndMatchVotes => endMatchVotes;

	public IReadOnlyList<int> ReplayVotes => replayVotes;

	public event Action OnGameEndScreenTriggered;

	private void Start()
	{
		NetworkBehaviourSingleton<UserIdManager>.Instance.OnUserIdRemoved.AddListener(HandleUserIdLeft);
		MonoBehaviourSingleton<GameplayManager>.Instance.OnGameplayCleanup.AddListener(HandleGameplayCleanup);
	}

	private void HandleGameplayCleanup()
	{
		isActive = false;
		HideWindow();
		nextLevelVotes.Clear();
		endMatchVotes.Clear();
		replayVotes.Clear();
		ResumeTime();
		gameEndBlock = null;
	}

	private void HandleUserIdLeft(int userId)
	{
		if (!(EndlessServices.Instance == null) && userId != EndlessServices.Instance.CloudService.ActiveUserId)
		{
			VoteType andRemoveCurrentVote = GetAndRemoveCurrentVote(userId);
			if (andRemoveCurrentVote != VoteType.None)
			{
				OnVoteMoved.Invoke(userId, andRemoveCurrentVote, VoteType.None);
			}
			if (base.IsServer && isActive)
			{
				CheckVoteCount();
			}
		}
	}

	public void TriggerGameEndScreen(GameEndBlock gameEnd)
	{
		StopTime();
		try
		{
			if (isActive)
			{
				Debug.LogError("Trying to trigger game end while in game end");
			}
			else
			{
				gameEndBlock = gameEnd;
				bool showEndMatch = gameEnd.ShowEndMatchButton || (!gameEnd.ShowReplayButton && !gameEnd.ShowNextLevelButton);
				BasicStat[] array = gameEnd.GatherGlobalStats();
				PerPlayerStat[] array2 = gameEnd.GatherPerPlayerStats();
				Debug.Log($"Gathered stats global: {array.Length} perPlayer: {array2.Length}");
				UIGameOverWindowModel gameOverWindowModel = new UIGameOverWindowModel(gameEnd.Title, gameEnd.Description, gameEnd.ShowReplayButton, showEndMatch, gameEnd.ShowNextLevelButton, array, array2);
				isActive = true;
				ShowGameEnd_ClientRpc(gameOverWindowModel);
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		this.OnGameEndScreenTriggered?.Invoke();
	}

	private static void StopTime()
	{
		Time.timeScale = 0f;
		NetworkBehaviourSingleton<NetClock>.Instance.Suspend();
		MonoBehaviourSingleton<EndlessLoop>.Instance.SuspendPlay();
	}

	[ClientRpc]
	public void ShowGameEnd_ClientRpc(UIGameOverWindowModel gameOverWindowModel)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(72546861u, clientRpcParams, RpcDelivery.Reliable);
			bool value = gameOverWindowModel != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(in gameOverWindowModel, default(FastBufferWriter.ForNetworkSerializable));
			}
			__endSendClientRpc(ref bufferWriter, 72546861u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			StopTime();
			currentUiWindow = UIGameOverWindow.Open(gameOverWindowPrefab, gameOverWindowModel, MonoBehaviourSingleton<UIGameplayReferenceManager>.Instance.GameplayWindowContainer);
		}
	}

	public void EndGameVote()
	{
		EndGameVote_ServerRpc();
	}

	[ServerRpc(RequireOwnership = false)]
	public void EndGameVote_ServerRpc(ServerRpcParams serverRpcParams = default(ServerRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter bufferWriter = __beginSendServerRpc(2843382700u, serverRpcParams, RpcDelivery.Reliable);
				__endSendServerRpc(ref bufferWriter, 2843382700u, serverRpcParams, RpcDelivery.Reliable);
			}
			if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				__rpc_exec_stage = __RpcExecStage.Send;
				ChangeVote(serverRpcParams.Receive.SenderClientId, VoteType.EndMatch);
			}
		}
	}

	public void NextLevelVote()
	{
		NextLevelVote_ServerRpc();
	}

	[ServerRpc(RequireOwnership = false)]
	public void NextLevelVote_ServerRpc(ServerRpcParams serverRpcParams = default(ServerRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter bufferWriter = __beginSendServerRpc(2552639600u, serverRpcParams, RpcDelivery.Reliable);
				__endSendServerRpc(ref bufferWriter, 2552639600u, serverRpcParams, RpcDelivery.Reliable);
			}
			if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				__rpc_exec_stage = __RpcExecStage.Send;
				ChangeVote(serverRpcParams.Receive.SenderClientId, VoteType.NextLevel);
			}
		}
	}

	public void ReplayVote()
	{
		ReplayVote_ServerRpc();
	}

	[ServerRpc(RequireOwnership = false)]
	public void ReplayVote_ServerRpc(ServerRpcParams serverRpcParams = default(ServerRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter bufferWriter = __beginSendServerRpc(1522087727u, serverRpcParams, RpcDelivery.Reliable);
				__endSendServerRpc(ref bufferWriter, 1522087727u, serverRpcParams, RpcDelivery.Reliable);
			}
			if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				__rpc_exec_stage = __RpcExecStage.Send;
				ChangeVote(serverRpcParams.Receive.SenderClientId, VoteType.Replay);
			}
		}
	}

	private void ChangeVote(ulong clientId, VoteType newVote)
	{
		if (NetworkBehaviourSingleton<UserIdManager>.Instance.TryGetUserId(clientId, out var userId))
		{
			VoteType andRemoveCurrentVote = GetAndRemoveCurrentVote(userId);
			AddVote(userId, newVote);
			VoteChanged_ClientRpc(userId, andRemoveCurrentVote, newVote);
		}
		CheckVoteCount();
	}

	[ClientRpc]
	private void VoteChanged_ClientRpc(int userId, VoteType previousVote, VoteType newVote)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(926248860u, clientRpcParams, RpcDelivery.Reliable);
			BytePacker.WriteValueBitPacked(bufferWriter, userId);
			bufferWriter.WriteValueSafe(in previousVote, default(FastBufferWriter.ForEnums));
			bufferWriter.WriteValueSafe(in newVote, default(FastBufferWriter.ForEnums));
			__endSendClientRpc(ref bufferWriter, 926248860u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if (!base.IsServer)
			{
				previousVote = GetAndRemoveCurrentVote(userId);
				AddVote(userId, newVote);
			}
			OnVoteMoved.Invoke(userId, previousVote, newVote);
		}
	}

	private void AddVote(int userId, VoteType newVote)
	{
		switch (newVote)
		{
		case VoteType.NextLevel:
			nextLevelVotes.Add(userId);
			break;
		case VoteType.EndMatch:
			endMatchVotes.Add(userId);
			break;
		case VoteType.Replay:
			replayVotes.Add(userId);
			break;
		default:
			throw new ArgumentOutOfRangeException("newVote", newVote, null);
		}
	}

	private void CheckVoteCount()
	{
		int userCount = NetworkBehaviourSingleton<UserIdManager>.Instance.UserCount;
		int num = nextLevelVotes.Count + endMatchVotes.Count + replayVotes.Count;
		Debug.Log($"Checking vote count: {num}/{userCount}");
		if (num == userCount)
		{
			ExecuteHighestVote();
		}
	}

	[ClientRpc]
	private void HideUiWindow_ClientRpc()
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(3053430080u, clientRpcParams, RpcDelivery.Reliable);
			__endSendClientRpc(ref bufferWriter, 3053430080u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			HideWindow();
			if (!base.IsServer)
			{
				ClearVotes();
				ResumeTime();
			}
		}
	}

	private void ClearVotes()
	{
		replayVotes.Clear();
		nextLevelVotes.Clear();
		endMatchVotes.Clear();
	}

	private void HideWindow()
	{
		if (currentUiWindow != null)
		{
			currentUiWindow.Close();
			currentUiWindow = null;
		}
	}

	private void ExecuteHighestVote()
	{
		int num = -1;
		VoteType voteType = VoteType.EndMatch;
		if (gameEndBlock.ShowReplayButton && replayVotes.Count >= num)
		{
			num = replayVotes.Count;
			voteType = VoteType.Replay;
		}
		if (gameEndBlock.ShowNextLevelButton && nextLevelVotes.Count >= num)
		{
			num = nextLevelVotes.Count;
			voteType = VoteType.NextLevel;
		}
		if (endMatchVotes.Count > num)
		{
			voteType = VoteType.EndMatch;
		}
		switch (voteType)
		{
		case VoteType.NextLevel:
			gameEndBlock.TriggerNextLevel();
			break;
		case VoteType.EndMatch:
			MonoBehaviourSingleton<ConnectionActions>.Instance.EndMatch();
			break;
		case VoteType.Replay:
		{
			string[] userIds = MatchmakingClientController.FormatUserIdsForChangeMatch(NetworkBehaviourSingleton<UserIdManager>.Instance.ConnectedUserIds.ToList());
			MatchData matchData = MatchSession.Instance.MatchData;
			string projectId = matchData.ProjectId;
			string levelId = matchData.LevelId;
			string customData = matchData.CustomData;
			MatchmakingClientController.Instance.ChangeMatch(projectId, levelId, isEditSession: false, MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.AssetVersion, null, customData, endMatch: false, null, userIds);
			break;
		}
		default:
			throw new ArgumentOutOfRangeException();
		}
		isActive = false;
		ClearVotes();
		ResumeTime();
		HideUiWindow_ClientRpc();
	}

	private void ResumeTime()
	{
		Time.timeScale = 1f;
		if ((bool)NetworkBehaviourSingleton<NetClock>.Instance)
		{
			NetworkBehaviourSingleton<NetClock>.Instance.Unsuspend();
		}
		if ((bool)MonoBehaviourSingleton<EndlessLoop>.Instance)
		{
			MonoBehaviourSingleton<EndlessLoop>.Instance.UnsuspendPlay();
		}
	}

	private VoteType GetAndRemoveCurrentVote(int userId)
	{
		if (replayVotes.Remove(userId))
		{
			return VoteType.Replay;
		}
		if (nextLevelVotes.Remove(userId))
		{
			return VoteType.NextLevel;
		}
		if (endMatchVotes.Remove(userId))
		{
			return VoteType.EndMatch;
		}
		return VoteType.None;
	}

	protected override void __initializeVariables()
	{
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		__registerRpc(72546861u, __rpc_handler_72546861, "ShowGameEnd_ClientRpc");
		__registerRpc(2843382700u, __rpc_handler_2843382700, "EndGameVote_ServerRpc");
		__registerRpc(2552639600u, __rpc_handler_2552639600, "NextLevelVote_ServerRpc");
		__registerRpc(1522087727u, __rpc_handler_1522087727, "ReplayVote_ServerRpc");
		__registerRpc(926248860u, __rpc_handler_926248860, "VoteChanged_ClientRpc");
		__registerRpc(3053430080u, __rpc_handler_3053430080, "HideUiWindow_ClientRpc");
		base.__initializeRpcs();
	}

	private static void __rpc_handler_72546861(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			UIGameOverWindowModel value2 = null;
			if (value)
			{
				reader.ReadValueSafe(out value2, default(FastBufferWriter.ForNetworkSerializable));
			}
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((GameEndManager)target).ShowGameEnd_ClientRpc(value2);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_2843382700(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((GameEndManager)target).EndGameVote_ServerRpc(server);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_2552639600(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((GameEndManager)target).NextLevelVote_ServerRpc(server);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_1522087727(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((GameEndManager)target).ReplayVote_ServerRpc(server);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_926248860(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			ByteUnpacker.ReadValueBitPacked(reader, out int value);
			reader.ReadValueSafe(out VoteType value2, default(FastBufferWriter.ForEnums));
			reader.ReadValueSafe(out VoteType value3, default(FastBufferWriter.ForEnums));
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((GameEndManager)target).VoteChanged_ClientRpc(value, value2, value3);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_3053430080(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((GameEndManager)target).HideUiWindow_ClientRpc();
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	protected internal override string __getTypeName()
	{
		return "GameEndManager";
	}
}
