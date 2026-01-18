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

namespace Endless.Gameplay
{
	// Token: 0x020000BF RID: 191
	public class GameEndManager : NetworkBehaviourSingleton<GameEndManager>
	{
		// Token: 0x14000001 RID: 1
		// (add) Token: 0x06000381 RID: 897 RVA: 0x00012F2C File Offset: 0x0001112C
		// (remove) Token: 0x06000382 RID: 898 RVA: 0x00012F64 File Offset: 0x00011164
		public event Action OnGameEndScreenTriggered;

		// Token: 0x17000097 RID: 151
		// (get) Token: 0x06000383 RID: 899 RVA: 0x00012F99 File Offset: 0x00011199
		public IReadOnlyList<int> NextLevelVotes
		{
			get
			{
				return this.nextLevelVotes;
			}
		}

		// Token: 0x17000098 RID: 152
		// (get) Token: 0x06000384 RID: 900 RVA: 0x00012FA1 File Offset: 0x000111A1
		public IReadOnlyList<int> EndMatchVotes
		{
			get
			{
				return this.endMatchVotes;
			}
		}

		// Token: 0x17000099 RID: 153
		// (get) Token: 0x06000385 RID: 901 RVA: 0x00012FA9 File Offset: 0x000111A9
		public IReadOnlyList<int> ReplayVotes
		{
			get
			{
				return this.replayVotes;
			}
		}

		// Token: 0x06000386 RID: 902 RVA: 0x00012FB1 File Offset: 0x000111B1
		private void Start()
		{
			NetworkBehaviourSingleton<UserIdManager>.Instance.OnUserIdRemoved.AddListener(new UnityAction<int>(this.HandleUserIdLeft));
			MonoBehaviourSingleton<GameplayManager>.Instance.OnGameplayCleanup.AddListener(new UnityAction(this.HandleGameplayCleanup));
		}

		// Token: 0x06000387 RID: 903 RVA: 0x00012FE9 File Offset: 0x000111E9
		private void HandleGameplayCleanup()
		{
			this.isActive = false;
			this.HideWindow();
			this.nextLevelVotes.Clear();
			this.endMatchVotes.Clear();
			this.replayVotes.Clear();
			this.ResumeTime();
			this.gameEndBlock = null;
		}

		// Token: 0x06000388 RID: 904 RVA: 0x00013028 File Offset: 0x00011228
		private void HandleUserIdLeft(int userId)
		{
			if (EndlessServices.Instance == null || userId == EndlessServices.Instance.CloudService.ActiveUserId)
			{
				return;
			}
			GameEndManager.VoteType andRemoveCurrentVote = this.GetAndRemoveCurrentVote(userId);
			if (andRemoveCurrentVote != GameEndManager.VoteType.None)
			{
				this.OnVoteMoved.Invoke(userId, andRemoveCurrentVote, GameEndManager.VoteType.None);
			}
			if (base.IsServer && this.isActive)
			{
				this.CheckVoteCount();
			}
		}

		// Token: 0x06000389 RID: 905 RVA: 0x00013084 File Offset: 0x00011284
		public void TriggerGameEndScreen(GameEndBlock gameEnd)
		{
			GameEndManager.StopTime();
			try
			{
				if (this.isActive)
				{
					Debug.LogError("Trying to trigger game end while in game end");
				}
				else
				{
					this.gameEndBlock = gameEnd;
					bool flag = gameEnd.ShowEndMatchButton || (!gameEnd.ShowReplayButton && !gameEnd.ShowNextLevelButton);
					BasicStat[] array = gameEnd.GatherGlobalStats();
					PerPlayerStat[] array2 = gameEnd.GatherPerPlayerStats();
					Debug.Log(string.Format("Gathered stats global: {0} perPlayer: {1}", array.Length, array2.Length));
					UIGameOverWindowModel uigameOverWindowModel = new UIGameOverWindowModel(gameEnd.Title, gameEnd.Description, gameEnd.ShowReplayButton, flag, gameEnd.ShowNextLevelButton, array, array2);
					this.isActive = true;
					this.ShowGameEnd_ClientRpc(uigameOverWindowModel);
				}
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
			Action onGameEndScreenTriggered = this.OnGameEndScreenTriggered;
			if (onGameEndScreenTriggered == null)
			{
				return;
			}
			onGameEndScreenTriggered();
		}

		// Token: 0x0600038A RID: 906 RVA: 0x0001315C File Offset: 0x0001135C
		private static void StopTime()
		{
			Time.timeScale = 0f;
			NetworkBehaviourSingleton<NetClock>.Instance.Suspend();
			MonoBehaviourSingleton<EndlessLoop>.Instance.SuspendPlay();
		}

		// Token: 0x0600038B RID: 907 RVA: 0x0001317C File Offset: 0x0001137C
		[ClientRpc]
		public void ShowGameEnd_ClientRpc(UIGameOverWindowModel gameOverWindowModel)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(72546861U, clientRpcParams, RpcDelivery.Reliable);
				bool flag = gameOverWindowModel != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag, default(FastBufferWriter.ForPrimitives));
				if (flag)
				{
					fastBufferWriter.WriteValueSafe<UIGameOverWindowModel>(in gameOverWindowModel, default(FastBufferWriter.ForNetworkSerializable));
				}
				base.__endSendClientRpc(ref fastBufferWriter, 72546861U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			GameEndManager.StopTime();
			this.currentUiWindow = UIGameOverWindow.Open(this.gameOverWindowPrefab, gameOverWindowModel, MonoBehaviourSingleton<UIGameplayReferenceManager>.Instance.GameplayWindowContainer);
		}

		// Token: 0x0600038C RID: 908 RVA: 0x000132BC File Offset: 0x000114BC
		public void EndGameVote()
		{
			this.EndGameVote_ServerRpc(default(ServerRpcParams));
		}

		// Token: 0x0600038D RID: 909 RVA: 0x000132D8 File Offset: 0x000114D8
		[ServerRpc(RequireOwnership = false)]
		public void EndGameVote_ServerRpc(ServerRpcParams serverRpcParams = default(ServerRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(2843382700U, serverRpcParams, RpcDelivery.Reliable);
				base.__endSendServerRpc(ref fastBufferWriter, 2843382700U, serverRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.ChangeVote(serverRpcParams.Receive.SenderClientId, GameEndManager.VoteType.EndMatch);
		}

		// Token: 0x0600038E RID: 910 RVA: 0x000133C0 File Offset: 0x000115C0
		public void NextLevelVote()
		{
			this.NextLevelVote_ServerRpc(default(ServerRpcParams));
		}

		// Token: 0x0600038F RID: 911 RVA: 0x000133DC File Offset: 0x000115DC
		[ServerRpc(RequireOwnership = false)]
		public void NextLevelVote_ServerRpc(ServerRpcParams serverRpcParams = default(ServerRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(2552639600U, serverRpcParams, RpcDelivery.Reliable);
				base.__endSendServerRpc(ref fastBufferWriter, 2552639600U, serverRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.ChangeVote(serverRpcParams.Receive.SenderClientId, GameEndManager.VoteType.NextLevel);
		}

		// Token: 0x06000390 RID: 912 RVA: 0x000134C4 File Offset: 0x000116C4
		public void ReplayVote()
		{
			this.ReplayVote_ServerRpc(default(ServerRpcParams));
		}

		// Token: 0x06000391 RID: 913 RVA: 0x000134E0 File Offset: 0x000116E0
		[ServerRpc(RequireOwnership = false)]
		public void ReplayVote_ServerRpc(ServerRpcParams serverRpcParams = default(ServerRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(1522087727U, serverRpcParams, RpcDelivery.Reliable);
				base.__endSendServerRpc(ref fastBufferWriter, 1522087727U, serverRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.ChangeVote(serverRpcParams.Receive.SenderClientId, GameEndManager.VoteType.Replay);
		}

		// Token: 0x06000392 RID: 914 RVA: 0x000135C8 File Offset: 0x000117C8
		private void ChangeVote(ulong clientId, GameEndManager.VoteType newVote)
		{
			int num;
			if (NetworkBehaviourSingleton<UserIdManager>.Instance.TryGetUserId(clientId, out num))
			{
				GameEndManager.VoteType andRemoveCurrentVote = this.GetAndRemoveCurrentVote(num);
				this.AddVote(num, newVote);
				this.VoteChanged_ClientRpc(num, andRemoveCurrentVote, newVote);
			}
			this.CheckVoteCount();
		}

		// Token: 0x06000393 RID: 915 RVA: 0x00013604 File Offset: 0x00011804
		[ClientRpc]
		private void VoteChanged_ClientRpc(int userId, GameEndManager.VoteType previousVote, GameEndManager.VoteType newVote)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(926248860U, clientRpcParams, RpcDelivery.Reliable);
				BytePacker.WriteValueBitPacked(fastBufferWriter, userId);
				fastBufferWriter.WriteValueSafe<GameEndManager.VoteType>(in previousVote, default(FastBufferWriter.ForEnums));
				fastBufferWriter.WriteValueSafe<GameEndManager.VoteType>(in newVote, default(FastBufferWriter.ForEnums));
				base.__endSendClientRpc(ref fastBufferWriter, 926248860U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			if (!base.IsServer)
			{
				previousVote = this.GetAndRemoveCurrentVote(userId);
				this.AddVote(userId, newVote);
			}
			this.OnVoteMoved.Invoke(userId, previousVote, newVote);
		}

		// Token: 0x06000394 RID: 916 RVA: 0x00013744 File Offset: 0x00011944
		private void AddVote(int userId, GameEndManager.VoteType newVote)
		{
			switch (newVote)
			{
			case GameEndManager.VoteType.NextLevel:
				this.nextLevelVotes.Add(userId);
				return;
			case GameEndManager.VoteType.EndMatch:
				this.endMatchVotes.Add(userId);
				return;
			case GameEndManager.VoteType.Replay:
				this.replayVotes.Add(userId);
				return;
			default:
				throw new ArgumentOutOfRangeException("newVote", newVote, null);
			}
		}

		// Token: 0x06000395 RID: 917 RVA: 0x000137A0 File Offset: 0x000119A0
		private void CheckVoteCount()
		{
			int userCount = NetworkBehaviourSingleton<UserIdManager>.Instance.UserCount;
			int num = this.nextLevelVotes.Count + this.endMatchVotes.Count + this.replayVotes.Count;
			Debug.Log(string.Format("Checking vote count: {0}/{1}", num, userCount));
			if (num == userCount)
			{
				this.ExecuteHighestVote();
			}
		}

		// Token: 0x06000396 RID: 918 RVA: 0x00013804 File Offset: 0x00011A04
		[ClientRpc]
		private void HideUiWindow_ClientRpc()
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(3053430080U, clientRpcParams, RpcDelivery.Reliable);
				base.__endSendClientRpc(ref fastBufferWriter, 3053430080U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.HideWindow();
			if (!base.IsServer)
			{
				this.ClearVotes();
				this.ResumeTime();
			}
		}

		// Token: 0x06000397 RID: 919 RVA: 0x000138F1 File Offset: 0x00011AF1
		private void ClearVotes()
		{
			this.replayVotes.Clear();
			this.nextLevelVotes.Clear();
			this.endMatchVotes.Clear();
		}

		// Token: 0x06000398 RID: 920 RVA: 0x00013914 File Offset: 0x00011B14
		private void HideWindow()
		{
			if (this.currentUiWindow != null)
			{
				this.currentUiWindow.Close();
				this.currentUiWindow = null;
			}
		}

		// Token: 0x06000399 RID: 921 RVA: 0x00013938 File Offset: 0x00011B38
		private void ExecuteHighestVote()
		{
			int num = -1;
			GameEndManager.VoteType voteType = GameEndManager.VoteType.EndMatch;
			if (this.gameEndBlock.ShowReplayButton && this.replayVotes.Count >= num)
			{
				num = this.replayVotes.Count;
				voteType = GameEndManager.VoteType.Replay;
			}
			if (this.gameEndBlock.ShowNextLevelButton && this.nextLevelVotes.Count >= num)
			{
				num = this.nextLevelVotes.Count;
				voteType = GameEndManager.VoteType.NextLevel;
			}
			if (this.endMatchVotes.Count > num)
			{
				voteType = GameEndManager.VoteType.EndMatch;
			}
			switch (voteType)
			{
			case GameEndManager.VoteType.NextLevel:
				this.gameEndBlock.TriggerNextLevel();
				break;
			case GameEndManager.VoteType.EndMatch:
				MonoBehaviourSingleton<ConnectionActions>.Instance.EndMatch(null);
				break;
			case GameEndManager.VoteType.Replay:
			{
				string[] array = MatchmakingClientController.FormatUserIdsForChangeMatch(NetworkBehaviourSingleton<UserIdManager>.Instance.ConnectedUserIds.ToList<int>());
				MatchData matchData = MatchSession.Instance.MatchData;
				string projectId = matchData.ProjectId;
				string levelId = matchData.LevelId;
				string customData = matchData.CustomData;
				MatchmakingClientController.Instance.ChangeMatch(projectId, levelId, false, MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.AssetVersion, null, customData, false, null, array);
				break;
			}
			default:
				throw new ArgumentOutOfRangeException();
			}
			this.isActive = false;
			this.ClearVotes();
			this.ResumeTime();
			this.HideUiWindow_ClientRpc();
		}

		// Token: 0x0600039A RID: 922 RVA: 0x00013A54 File Offset: 0x00011C54
		private void ResumeTime()
		{
			Time.timeScale = 1f;
			if (NetworkBehaviourSingleton<NetClock>.Instance)
			{
				NetworkBehaviourSingleton<NetClock>.Instance.Unsuspend();
			}
			if (MonoBehaviourSingleton<EndlessLoop>.Instance)
			{
				MonoBehaviourSingleton<EndlessLoop>.Instance.UnsuspendPlay();
			}
		}

		// Token: 0x0600039B RID: 923 RVA: 0x00013A8C File Offset: 0x00011C8C
		private GameEndManager.VoteType GetAndRemoveCurrentVote(int userId)
		{
			if (this.replayVotes.Remove(userId))
			{
				return GameEndManager.VoteType.Replay;
			}
			if (this.nextLevelVotes.Remove(userId))
			{
				return GameEndManager.VoteType.NextLevel;
			}
			if (this.endMatchVotes.Remove(userId))
			{
				return GameEndManager.VoteType.EndMatch;
			}
			return GameEndManager.VoteType.None;
		}

		// Token: 0x0600039D RID: 925 RVA: 0x00013AF4 File Offset: 0x00011CF4
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x0600039E RID: 926 RVA: 0x00013B0C File Offset: 0x00011D0C
		protected override void __initializeRpcs()
		{
			base.__registerRpc(72546861U, new NetworkBehaviour.RpcReceiveHandler(GameEndManager.__rpc_handler_72546861), "ShowGameEnd_ClientRpc");
			base.__registerRpc(2843382700U, new NetworkBehaviour.RpcReceiveHandler(GameEndManager.__rpc_handler_2843382700), "EndGameVote_ServerRpc");
			base.__registerRpc(2552639600U, new NetworkBehaviour.RpcReceiveHandler(GameEndManager.__rpc_handler_2552639600), "NextLevelVote_ServerRpc");
			base.__registerRpc(1522087727U, new NetworkBehaviour.RpcReceiveHandler(GameEndManager.__rpc_handler_1522087727), "ReplayVote_ServerRpc");
			base.__registerRpc(926248860U, new NetworkBehaviour.RpcReceiveHandler(GameEndManager.__rpc_handler_926248860), "VoteChanged_ClientRpc");
			base.__registerRpc(3053430080U, new NetworkBehaviour.RpcReceiveHandler(GameEndManager.__rpc_handler_3053430080), "HideUiWindow_ClientRpc");
			base.__initializeRpcs();
		}

		// Token: 0x0600039F RID: 927 RVA: 0x00013BCC File Offset: 0x00011DCC
		private static void __rpc_handler_72546861(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			bool flag;
			reader.ReadValueSafe<bool>(out flag, default(FastBufferWriter.ForPrimitives));
			UIGameOverWindowModel uigameOverWindowModel = null;
			if (flag)
			{
				reader.ReadValueSafe<UIGameOverWindowModel>(out uigameOverWindowModel, default(FastBufferWriter.ForNetworkSerializable));
			}
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((GameEndManager)target).ShowGameEnd_ClientRpc(uigameOverWindowModel);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060003A0 RID: 928 RVA: 0x00013C68 File Offset: 0x00011E68
		private static void __rpc_handler_2843382700(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((GameEndManager)target).EndGameVote_ServerRpc(server);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060003A1 RID: 929 RVA: 0x00013CC8 File Offset: 0x00011EC8
		private static void __rpc_handler_2552639600(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((GameEndManager)target).NextLevelVote_ServerRpc(server);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060003A2 RID: 930 RVA: 0x00013D28 File Offset: 0x00011F28
		private static void __rpc_handler_1522087727(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((GameEndManager)target).ReplayVote_ServerRpc(server);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060003A3 RID: 931 RVA: 0x00013D88 File Offset: 0x00011F88
		private static void __rpc_handler_926248860(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			int num;
			ByteUnpacker.ReadValueBitPacked(reader, out num);
			GameEndManager.VoteType voteType;
			reader.ReadValueSafe<GameEndManager.VoteType>(out voteType, default(FastBufferWriter.ForEnums));
			GameEndManager.VoteType voteType2;
			reader.ReadValueSafe<GameEndManager.VoteType>(out voteType2, default(FastBufferWriter.ForEnums));
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((GameEndManager)target).VoteChanged_ClientRpc(num, voteType, voteType2);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060003A4 RID: 932 RVA: 0x00013E28 File Offset: 0x00012028
		private static void __rpc_handler_3053430080(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((GameEndManager)target).HideUiWindow_ClientRpc();
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060003A5 RID: 933 RVA: 0x00013E79 File Offset: 0x00012079
		protected internal override string __getTypeName()
		{
			return "GameEndManager";
		}

		// Token: 0x04000342 RID: 834
		[SerializeField]
		private UIGameOverWindow gameOverWindowPrefab;

		// Token: 0x04000343 RID: 835
		private List<int> nextLevelVotes = new List<int>();

		// Token: 0x04000344 RID: 836
		private List<int> endMatchVotes = new List<int>();

		// Token: 0x04000345 RID: 837
		private List<int> replayVotes = new List<int>();

		// Token: 0x04000346 RID: 838
		private GameEndBlock gameEndBlock;

		// Token: 0x04000347 RID: 839
		private UIGameOverWindow currentUiWindow;

		// Token: 0x04000348 RID: 840
		private bool isActive;

		// Token: 0x04000349 RID: 841
		public UnityEvent<int, GameEndManager.VoteType, GameEndManager.VoteType> OnVoteMoved = new UnityEvent<int, GameEndManager.VoteType, GameEndManager.VoteType>();

		// Token: 0x020000C0 RID: 192
		public enum VoteType
		{
			// Token: 0x0400034B RID: 843
			None,
			// Token: 0x0400034C RID: 844
			NextLevel,
			// Token: 0x0400034D RID: 845
			EndMatch,
			// Token: 0x0400034E RID: 846
			Replay
		}
	}
}
