using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Endless.Gameplay;
using Endless.Gameplay.RightsManagement;
using Endless.Matchmaking;
using Endless.Networking;
using Endless.Shared;
using Runtime.Shared.Matchmaking;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator
{
	// Token: 0x02000083 RID: 131
	public class PlayBarManager : EndlessNetworkBehaviourSingleton<PlayBarManager>
	{
		// Token: 0x17000015 RID: 21
		// (get) Token: 0x060001C7 RID: 455 RVA: 0x0000DA7E File Offset: 0x0000BC7E
		private bool IsInParty
		{
			get
			{
				return MatchmakingClientController.Instance.LocalGroup != null && MatchmakingClientController.Instance.LocalGroup.Members.Count != 1;
			}
		}

		// Token: 0x17000016 RID: 22
		// (get) Token: 0x060001C8 RID: 456 RVA: 0x0000DAA8 File Offset: 0x0000BCA8
		public bool IsTimerStarted
		{
			get
			{
				return this.MatchTransitionTime != 0f;
			}
		}

		// Token: 0x17000017 RID: 23
		// (get) Token: 0x060001C9 RID: 457 RVA: 0x0000DABA File Offset: 0x0000BCBA
		// (set) Token: 0x060001CA RID: 458 RVA: 0x0000DAC2 File Offset: 0x0000BCC2
		public float MatchTransitionTime { get; private set; }

		// Token: 0x17000018 RID: 24
		// (get) Token: 0x060001CB RID: 459 RVA: 0x0000DACB File Offset: 0x0000BCCB
		public bool IsLocalPlayerReady
		{
			get
			{
				return this.readyUserIds.Contains(EndlessServices.Instance.CloudService.ActiveUserId);
			}
		}

		// Token: 0x17000019 RID: 25
		// (get) Token: 0x060001CC RID: 460 RVA: 0x0000DAE8 File Offset: 0x0000BCE8
		private bool IsPartyHost
		{
			get
			{
				if (!this.IsInParty)
				{
					return false;
				}
				ClientData value = MatchmakingClientController.Instance.LocalClientData.Value;
				CoreClientData host = MatchmakingClientController.Instance.LocalGroup.Host;
				return value.CoreDataEquals(host);
			}
		}

		// Token: 0x1700001A RID: 26
		// (get) Token: 0x060001CD RID: 461 RVA: 0x0000DACB File Offset: 0x0000BCCB
		public bool IsLocalClientReady
		{
			get
			{
				return this.readyUserIds.Contains(EndlessServices.Instance.CloudService.ActiveUserId);
			}
		}

		// Token: 0x060001CE RID: 462 RVA: 0x0000DB28 File Offset: 0x0000BD28
		protected override void Start()
		{
			base.Start();
			MonoBehaviourSingleton<GameplayManager>.Instance.OnGameplayStarted.AddListener(new UnityAction(this.HandleGameplayStarted));
			NetworkBehaviourSingleton<CreatorManager>.Instance.OnCreatorStarted.AddListener(new UnityAction(this.HandleCreatorStarted));
			NetworkBehaviourSingleton<UserIdManager>.Instance.OnUserIdAdded.AddListener(new UnityAction<int>(this.HandleUserAdded));
			NetworkBehaviourSingleton<UserIdManager>.Instance.OnUserIdRemoved.AddListener(new UnityAction<int>(this.HandleUserRemoved));
		}

		// Token: 0x060001CF RID: 463 RVA: 0x0000DBA7 File Offset: 0x0000BDA7
		private void HandleCreatorStarted()
		{
			if (base.IsServer)
			{
				this.UnreadyUserIdsServerOnly(this.readyUserIds.ToArray<int>());
			}
		}

		// Token: 0x060001D0 RID: 464 RVA: 0x0000DBA7 File Offset: 0x0000BDA7
		private void HandleGameplayStarted()
		{
			if (base.IsServer)
			{
				this.UnreadyUserIdsServerOnly(this.readyUserIds.ToArray<int>());
			}
		}

		// Token: 0x060001D1 RID: 465 RVA: 0x0000DBC4 File Offset: 0x0000BDC4
		public void PlayHit()
		{
			if (this.IsLocalPlayerReady)
			{
				if (!this.IsInParty)
				{
					this.UnplayHit_ServerRpc(default(ServerRpcParams));
					return;
				}
				if (this.IsPartyHost)
				{
					this.UnplayHit_PartyLeader_ServerRpc(PlayBarManager.GetPartyUserIds(), default(ServerRpcParams));
					return;
				}
			}
			else
			{
				if (!this.IsInParty)
				{
					this.PlayHit_ServerRpc(default(ServerRpcParams));
					return;
				}
				if (this.IsPartyHost)
				{
					this.PlayHit_PartyLeader_ServerRpc(PlayBarManager.GetPartyUserIds(), default(ServerRpcParams));
				}
			}
		}

		// Token: 0x060001D2 RID: 466 RVA: 0x0000DC42 File Offset: 0x0000BE42
		private static int[] GetPartyUserIds()
		{
			return MatchmakingClientController.Instance.LocalGroup.Members.Select((CoreClientData member) => int.Parse(member.PlatformId)).ToArray<int>();
		}

		// Token: 0x060001D3 RID: 467 RVA: 0x0000DC7C File Offset: 0x0000BE7C
		public PlayBarManager.PlayOption GetLocalPlayOption()
		{
			if (MatchmakingClientController.Instance.LocalGroup != null)
			{
				int count = MatchmakingClientController.Instance.LocalGroup.Members.Count;
			}
			PlayBarManager.PlayOption playOption = PlayBarManager.PlayOption.None;
			if (NetworkBehaviourSingleton<UserIdManager>.Instance.UserCount == 1)
			{
				return playOption;
			}
			if (MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying)
			{
				if (base.IsHost)
				{
					return playOption;
				}
				if (!this.IsInParty)
				{
					return PlayBarManager.PlayOption.StopSolo;
				}
				if (this.IsPartyHost)
				{
					return PlayBarManager.PlayOption.StopWithParty;
				}
			}
			else
			{
				if (!this.IsInParty)
				{
					return PlayBarManager.PlayOption.PlaySolo;
				}
				if (this.IsPartyHost)
				{
					return PlayBarManager.PlayOption.PlayWithParty;
				}
			}
			return playOption;
		}

		// Token: 0x060001D4 RID: 468 RVA: 0x0000DCFC File Offset: 0x0000BEFC
		public void ExecuteLocalPlayOption()
		{
			switch (this.GetLocalPlayOption())
			{
			case PlayBarManager.PlayOption.PlayWithParty:
			case PlayBarManager.PlayOption.StopWithParty:
				this.ChangeMatchImmediateParty_ServerRpc(PlayBarManager.GetPartyUserIds(), default(ServerRpcParams));
				return;
			case PlayBarManager.PlayOption.PlaySolo:
			case PlayBarManager.PlayOption.StopSolo:
				this.ChangeMatchImmediateSolo_ServerRpc(default(ServerRpcParams));
				return;
			default:
				return;
			}
		}

		// Token: 0x060001D5 RID: 469 RVA: 0x0000DD50 File Offset: 0x0000BF50
		[ServerRpc(RequireOwnership = false)]
		private void ChangeMatchImmediateSolo_ServerRpc(ServerRpcParams serverRpcParams = default(ServerRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(1311848200U, serverRpcParams, RpcDelivery.Reliable);
				base.__endSendServerRpc(ref fastBufferWriter, 1311848200U, serverRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			int userId = NetworkBehaviourSingleton<UserIdManager>.Instance.GetUserId(serverRpcParams.Receive.SenderClientId);
			this.TriggerChangeMatch(new int[] { userId }, userId == EndlessServices.Instance.CloudService.ActiveUserId);
		}

		// Token: 0x060001D6 RID: 470 RVA: 0x0000DE5C File Offset: 0x0000C05C
		[ServerRpc(RequireOwnership = false)]
		private void ChangeMatchImmediateParty_ServerRpc(int[] partyUserIds, ServerRpcParams serverRpcParams = default(ServerRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(2535256494U, serverRpcParams, RpcDelivery.Reliable);
				bool flag = partyUserIds != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag, default(FastBufferWriter.ForPrimitives));
				if (flag)
				{
					fastBufferWriter.WriteValueSafe<int>(partyUserIds, default(FastBufferWriter.ForPrimitives));
				}
				base.__endSendServerRpc(ref fastBufferWriter, 2535256494U, serverRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			int userId = NetworkBehaviourSingleton<UserIdManager>.Instance.GetUserId(serverRpcParams.Receive.SenderClientId);
			if (!partyUserIds.Contains(userId))
			{
				return;
			}
			this.TriggerChangeMatch(partyUserIds, partyUserIds.Contains(EndlessServices.Instance.CloudService.ActiveUserId));
		}

		// Token: 0x060001D7 RID: 471 RVA: 0x0000DFB8 File Offset: 0x0000C1B8
		private void TimerFinished()
		{
			Debug.Log("Play bar timer finished");
			bool flag = this.readyUserIds.Contains(EndlessServices.Instance.CloudService.ActiveUserId);
			HashSet<int> hashSet;
			if (flag && base.IsHost && MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying)
			{
				hashSet = NetworkBehaviourSingleton<UserIdManager>.Instance.ConnectedUserIds.ToHashSet<int>();
			}
			else
			{
				hashSet = new HashSet<int>(this.readyUserIds);
			}
			this.ClearMatchTimerServerOnly();
			this.TriggerChangeMatch(hashSet.ToArray<int>(), flag);
		}

		// Token: 0x060001D8 RID: 472 RVA: 0x0000E034 File Offset: 0x0000C234
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
				TaskAwaiter<GetAllRolesResult> taskAwaiter = MonoBehaviourSingleton<RightsManager>.Instance.GetAllUserRolesForAssetAsync(gameId, null, false).GetAwaiter();
				if (!taskAwaiter.IsCompleted)
				{
					await taskAwaiter;
					TaskAwaiter<GetAllRolesResult> taskAwaiter2;
					taskAwaiter = taskAwaiter2;
					taskAwaiter2 = default(TaskAwaiter<GetAllRolesResult>);
				}
				List<int> list = taskAwaiter.GetResult().Roles.Select((UserRole role) => role.UserId).ToList<int>();
				List<int> list2 = new List<int>();
				foreach (int num in userIds)
				{
					if (list.Contains(num))
					{
						list2.Add(num);
					}
					else
					{
						Debug.Log(string.Format("Kicking player: {0}", num));
						ulong clientId = NetworkBehaviourSingleton<UserIdManager>.Instance.GetClientId(num);
						MatchSession.Instance.KickClient(clientId, "The playtest has ended and you do not have edit rights to the game.");
					}
				}
				if (list2.Count > 0)
				{
					string[] array = MatchmakingClientController.FormatUserIdsForChangeMatch(list2);
					MatchmakingClientController.Instance.ChangeMatch(gameId, levelId, true, string.Empty, null, null, false, new Action<int, string>(this.OnMatchChangeFailed), array);
				}
			}
			else
			{
				string assetVersion = MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.AssetVersion;
				string[] array2 = MatchmakingClientController.FormatUserIdsForChangeMatch(userIds.ToList<int>());
				MatchmakingClientController.Instance.ChangeMatch(gameId, levelId, false, assetVersion, null, MatchDataExtensions.BuildPlaytestSessionData(), false, new Action<int, string>(this.OnMatchChangeFailed), array2);
			}
		}

		// Token: 0x060001D9 RID: 473 RVA: 0x0000E07B File Offset: 0x0000C27B
		private void OnMatchChangeFailed(int arg1, string arg2)
		{
			Debug.LogError(string.Format("Failed to change match {0}: {1}", arg1, arg2));
		}

		// Token: 0x060001DA RID: 474 RVA: 0x0000E094 File Offset: 0x0000C294
		[ClientRpc]
		private void ReadyPlayers_ClientRpc(int[] userIds)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(1970652522U, clientRpcParams, RpcDelivery.Reliable);
				bool flag = userIds != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag, default(FastBufferWriter.ForPrimitives));
				if (flag)
				{
					fastBufferWriter.WriteValueSafe<int>(userIds, default(FastBufferWriter.ForPrimitives));
				}
				base.__endSendClientRpc(ref fastBufferWriter, 1970652522U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			foreach (int num in userIds)
			{
				this.UserChangedState(num, PlayBarManager.PlayerReadyStatus.NotReady, PlayBarManager.PlayerReadyStatus.Ready);
			}
		}

		// Token: 0x060001DB RID: 475 RVA: 0x0000E1D0 File Offset: 0x0000C3D0
		private void UpdateMatchTimerServerOnly()
		{
			int count = this.readyUserIds.Count;
			if (count <= 0)
			{
				return;
			}
			int num = NetworkBehaviourSingleton<UserIdManager>.Instance.ConnectedUserIds.Count<int>();
			if (!this.IsTimerStarted)
			{
				float num2 = ((num == 1) ? this.soloDuration : ((count == num) ? this.allReadyDuration : this.matchTransitionDuration));
				this.MatchTransitionTime = NetworkManager.Singleton.ServerTime.TimeAsFloat + num2;
				this.UpdateMatchTimer_ClientRpc(this.MatchTransitionTime);
				this.timerCoroutine = base.StartCoroutine(this.TimerRoutine(num2));
				return;
			}
			if (count == num && this.MatchTransitionTime - NetworkManager.Singleton.ServerTime.TimeAsFloat > this.allReadyDuration)
			{
				this.MatchTransitionTime = NetworkManager.Singleton.ServerTime.TimeAsFloat + this.allReadyDuration;
				this.UpdateMatchTimer_ClientRpc(this.MatchTransitionTime);
				this.ClearTimerCoroutineServerOnly();
				this.timerCoroutine = base.StartCoroutine(this.TimerRoutine(this.allReadyDuration));
			}
		}

		// Token: 0x060001DC RID: 476 RVA: 0x0000E2CD File Offset: 0x0000C4CD
		private IEnumerator TimerRoutine(float duration)
		{
			yield return new WaitForSecondsRealtime(duration);
			this.TimerFinished();
			yield break;
		}

		// Token: 0x060001DD RID: 477 RVA: 0x0000E2E3 File Offset: 0x0000C4E3
		private void ClearMatchTimerServerOnly()
		{
			if (this.IsTimerStarted)
			{
				this.MatchTransitionTime = 0f;
				this.UpdateMatchTimer_ClientRpc(this.MatchTransitionTime);
				this.ClearTimerCoroutineServerOnly();
			}
		}

		// Token: 0x060001DE RID: 478 RVA: 0x0000E30A File Offset: 0x0000C50A
		private void ClearTimerCoroutineServerOnly()
		{
			if (this.timerCoroutine != null)
			{
				base.StopCoroutine(this.timerCoroutine);
				this.timerCoroutine = null;
			}
		}

		// Token: 0x060001DF RID: 479 RVA: 0x0000E328 File Offset: 0x0000C528
		[ClientRpc]
		private void UpdateMatchTimer_ClientRpc(float matchTransitionTime)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(844158908U, clientRpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<float>(in matchTransitionTime, default(FastBufferWriter.ForPrimitives));
				base.__endSendClientRpc(ref fastBufferWriter, 844158908U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			if (!base.IsServer)
			{
				this.MatchTransitionTime = matchTransitionTime;
			}
			this.OnMatchTimerUpdated.Invoke();
		}

		// Token: 0x060001E0 RID: 480 RVA: 0x0000E430 File Offset: 0x0000C630
		[ServerRpc(RequireOwnership = false)]
		private void PlayHit_ServerRpc(ServerRpcParams serverRpcParams = default(ServerRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(4248026517U, serverRpcParams, RpcDelivery.Reliable);
				base.__endSendServerRpc(ref fastBufferWriter, 4248026517U, serverRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			int userId = NetworkBehaviourSingleton<UserIdManager>.Instance.GetUserId(serverRpcParams.Receive.SenderClientId);
			if (base.IsHost && userId == EndlessServices.Instance.CloudService.ActiveUserId && MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying)
			{
				this.ForceReadyAll_SeverOnly();
				return;
			}
			this.readyUserIds.Add(userId);
			this.HandlePlayerReadyStatusChanged_ClientRpc(userId, PlayBarManager.PlayerReadyStatus.NotReady, PlayBarManager.PlayerReadyStatus.Ready);
			this.UpdateMatchTimerServerOnly();
		}

		// Token: 0x060001E1 RID: 481 RVA: 0x0000E564 File Offset: 0x0000C764
		private void ForceReadyAll_SeverOnly()
		{
			int[] array = NetworkBehaviourSingleton<UserIdManager>.Instance.ConnectedUserIds.Except(this.readyUserIds).ToArray<int>();
			foreach (int num in array)
			{
				this.readyUserIds.Add(num);
			}
			this.ReadyPlayers_ClientRpc(array);
			this.UpdateMatchTimerServerOnly();
		}

		// Token: 0x060001E2 RID: 482 RVA: 0x0000E5BC File Offset: 0x0000C7BC
		[ServerRpc(RequireOwnership = false)]
		private void PlayHit_PartyLeader_ServerRpc(int[] userIds, ServerRpcParams serverRpcParams = default(ServerRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(1740879778U, serverRpcParams, RpcDelivery.Reliable);
				bool flag = userIds != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag, default(FastBufferWriter.ForPrimitives));
				if (flag)
				{
					fastBufferWriter.WriteValueSafe<int>(userIds, default(FastBufferWriter.ForPrimitives));
				}
				base.__endSendServerRpc(ref fastBufferWriter, 1740879778U, serverRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			int userId = NetworkBehaviourSingleton<UserIdManager>.Instance.GetUserId(serverRpcParams.Receive.SenderClientId);
			if (!userIds.Contains(userId))
			{
				return;
			}
			if (base.IsHost && userId == EndlessServices.Instance.CloudService.ActiveUserId && MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying)
			{
				this.ForceReadyAll_SeverOnly();
				return;
			}
			foreach (int num in userIds)
			{
				this.readyUserIds.Add(num);
			}
			this.ReadyPlayers_ClientRpc(userIds);
			this.UpdateMatchTimerServerOnly();
		}

		// Token: 0x060001E3 RID: 483 RVA: 0x0000E758 File Offset: 0x0000C958
		[ServerRpc(RequireOwnership = false)]
		private void UnplayHit_ServerRpc(ServerRpcParams serverRpcParams = default(ServerRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(380088622U, serverRpcParams, RpcDelivery.Reliable);
				base.__endSendServerRpc(ref fastBufferWriter, 380088622U, serverRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			int userId = NetworkBehaviourSingleton<UserIdManager>.Instance.GetUserId(serverRpcParams.Receive.SenderClientId);
			if (this.readyUserIds.Remove(userId))
			{
				this.HandlePlayerReadyStatusChanged_ClientRpc(userId, PlayBarManager.PlayerReadyStatus.Ready, PlayBarManager.PlayerReadyStatus.NotReady);
				if (this.readyUserIds.Count == 0)
				{
					this.ClearMatchTimerServerOnly();
				}
			}
		}

		// Token: 0x060001E4 RID: 484 RVA: 0x0000E86C File Offset: 0x0000CA6C
		[ServerRpc(RequireOwnership = false)]
		private void UnplayHit_PartyLeader_ServerRpc(int[] userIds, ServerRpcParams serverRpcParams = default(ServerRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(2524213796U, serverRpcParams, RpcDelivery.Reliable);
				bool flag = userIds != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag, default(FastBufferWriter.ForPrimitives));
				if (flag)
				{
					fastBufferWriter.WriteValueSafe<int>(userIds, default(FastBufferWriter.ForPrimitives));
				}
				base.__endSendServerRpc(ref fastBufferWriter, 2524213796U, serverRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			int userId = NetworkBehaviourSingleton<UserIdManager>.Instance.GetUserId(serverRpcParams.Receive.SenderClientId);
			if (!userIds.Contains(userId))
			{
				return;
			}
			this.UnreadyUserIdsServerOnly(userIds);
		}

		// Token: 0x060001E5 RID: 485 RVA: 0x0000E9B4 File Offset: 0x0000CBB4
		private void UnreadyUserIdsServerOnly(int[] userIds)
		{
			foreach (int num in userIds)
			{
				this.readyUserIds.Remove(num);
			}
			this.UnreadyPlayers_ClientRpc(userIds);
			if (this.readyUserIds.Count == 0)
			{
				this.ClearMatchTimerServerOnly();
			}
		}

		// Token: 0x060001E6 RID: 486 RVA: 0x0000E9FC File Offset: 0x0000CBFC
		[ClientRpc]
		private void UnreadyPlayers_ClientRpc(int[] userIds)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(3062243066U, clientRpcParams, RpcDelivery.Reliable);
				bool flag = userIds != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag, default(FastBufferWriter.ForPrimitives));
				if (flag)
				{
					fastBufferWriter.WriteValueSafe<int>(userIds, default(FastBufferWriter.ForPrimitives));
				}
				base.__endSendClientRpc(ref fastBufferWriter, 3062243066U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			foreach (int num in userIds)
			{
				this.UserChangedState(num, PlayBarManager.PlayerReadyStatus.Ready, PlayBarManager.PlayerReadyStatus.NotReady);
			}
		}

		// Token: 0x060001E7 RID: 487 RVA: 0x0000EB38 File Offset: 0x0000CD38
		protected override void OnSynchronize<T>(ref BufferSerializer<T> serializer)
		{
			base.OnSynchronize<T>(ref serializer);
			float matchTransitionTime = this.MatchTransitionTime;
			serializer.SerializeValue<float>(ref matchTransitionTime, default(FastBufferWriter.ForPrimitives));
			if (serializer.IsReader)
			{
				this.MatchTransitionTime = matchTransitionTime;
			}
			int num = 0;
			if (serializer.IsWriter)
			{
				num = this.readyUserIds.Count;
			}
			serializer.SerializeValue<int>(ref num, default(FastBufferWriter.ForPrimitives));
			int[] array = new int[num];
			if (serializer.IsWriter)
			{
				array = this.readyUserIds.ToArray<int>();
			}
			for (int i = 0; i < num; i++)
			{
				serializer.SerializeValue<int>(ref array[i], default(FastBufferWriter.ForPrimitives));
				if (serializer.IsReader)
				{
					if (NetworkBehaviourSingleton<UserIdManager>.Instance.ConnectedUserIds.Contains(array[i]))
					{
						this.OnPlayerReadyChanged.Invoke(array[i], PlayBarManager.PlayerReadyStatus.NotReady, PlayBarManager.PlayerReadyStatus.Ready);
					}
					else
					{
						this.OnPlayerReadyChanged.Invoke(array[i], PlayBarManager.PlayerReadyStatus.None, PlayBarManager.PlayerReadyStatus.Ready);
					}
				}
			}
			if (serializer.IsReader)
			{
				this.readyUserIds = array.ToHashSet<int>();
			}
		}

		// Token: 0x060001E8 RID: 488 RVA: 0x0000EC31 File Offset: 0x0000CE31
		private void HandleUserAdded(int userId)
		{
			if (!this.readyUserIds.Contains(userId))
			{
				this.UserChangedState(userId, PlayBarManager.PlayerReadyStatus.None, PlayBarManager.PlayerReadyStatus.NotReady);
			}
		}

		// Token: 0x060001E9 RID: 489 RVA: 0x0000EC4C File Offset: 0x0000CE4C
		private void HandleUserRemoved(int userId)
		{
			if (this.readyUserIds.Remove(userId))
			{
				this.UserChangedState(userId, PlayBarManager.PlayerReadyStatus.Ready, PlayBarManager.PlayerReadyStatus.None);
				if (base.IsServer)
				{
					if (this.readyUserIds.Count == 0)
					{
						this.ClearMatchTimerServerOnly();
						return;
					}
					this.UpdateMatchTimerServerOnly();
					return;
				}
			}
			else
			{
				this.UserChangedState(userId, PlayBarManager.PlayerReadyStatus.NotReady, PlayBarManager.PlayerReadyStatus.None);
			}
		}

		// Token: 0x060001EA RID: 490 RVA: 0x0000EC9C File Offset: 0x0000CE9C
		[ClientRpc]
		private void HandlePlayerReadyStatusChanged_ClientRpc(int userId, PlayBarManager.PlayerReadyStatus previousStatus, PlayBarManager.PlayerReadyStatus newStatus)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(28542295U, clientRpcParams, RpcDelivery.Reliable);
				BytePacker.WriteValueBitPacked(fastBufferWriter, userId);
				fastBufferWriter.WriteValueSafe<PlayBarManager.PlayerReadyStatus>(in previousStatus, default(FastBufferWriter.ForEnums));
				fastBufferWriter.WriteValueSafe<PlayBarManager.PlayerReadyStatus>(in newStatus, default(FastBufferWriter.ForEnums));
				base.__endSendClientRpc(ref fastBufferWriter, 28542295U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.UserChangedState(userId, previousStatus, newStatus);
		}

		// Token: 0x060001EB RID: 491 RVA: 0x0000EDBB File Offset: 0x0000CFBB
		private void UserChangedState(int userId, PlayBarManager.PlayerReadyStatus previousStatus, PlayBarManager.PlayerReadyStatus newStatus)
		{
			if (!base.IsServer)
			{
				if (previousStatus == PlayBarManager.PlayerReadyStatus.Ready)
				{
					this.readyUserIds.Remove(userId);
				}
				else if (newStatus == PlayBarManager.PlayerReadyStatus.Ready)
				{
					this.readyUserIds.Add(userId);
				}
			}
			this.OnPlayerReadyChanged.Invoke(userId, previousStatus, newStatus);
		}

		// Token: 0x060001ED RID: 493 RVA: 0x0000EE50 File Offset: 0x0000D050
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x060001EE RID: 494 RVA: 0x0000EE68 File Offset: 0x0000D068
		protected override void __initializeRpcs()
		{
			base.__registerRpc(1311848200U, new NetworkBehaviour.RpcReceiveHandler(PlayBarManager.__rpc_handler_1311848200), "ChangeMatchImmediateSolo_ServerRpc");
			base.__registerRpc(2535256494U, new NetworkBehaviour.RpcReceiveHandler(PlayBarManager.__rpc_handler_2535256494), "ChangeMatchImmediateParty_ServerRpc");
			base.__registerRpc(1970652522U, new NetworkBehaviour.RpcReceiveHandler(PlayBarManager.__rpc_handler_1970652522), "ReadyPlayers_ClientRpc");
			base.__registerRpc(844158908U, new NetworkBehaviour.RpcReceiveHandler(PlayBarManager.__rpc_handler_844158908), "UpdateMatchTimer_ClientRpc");
			base.__registerRpc(4248026517U, new NetworkBehaviour.RpcReceiveHandler(PlayBarManager.__rpc_handler_4248026517), "PlayHit_ServerRpc");
			base.__registerRpc(1740879778U, new NetworkBehaviour.RpcReceiveHandler(PlayBarManager.__rpc_handler_1740879778), "PlayHit_PartyLeader_ServerRpc");
			base.__registerRpc(380088622U, new NetworkBehaviour.RpcReceiveHandler(PlayBarManager.__rpc_handler_380088622), "UnplayHit_ServerRpc");
			base.__registerRpc(2524213796U, new NetworkBehaviour.RpcReceiveHandler(PlayBarManager.__rpc_handler_2524213796), "UnplayHit_PartyLeader_ServerRpc");
			base.__registerRpc(3062243066U, new NetworkBehaviour.RpcReceiveHandler(PlayBarManager.__rpc_handler_3062243066), "UnreadyPlayers_ClientRpc");
			base.__registerRpc(28542295U, new NetworkBehaviour.RpcReceiveHandler(PlayBarManager.__rpc_handler_28542295), "HandlePlayerReadyStatusChanged_ClientRpc");
			base.__initializeRpcs();
		}

		// Token: 0x060001EF RID: 495 RVA: 0x0000EF98 File Offset: 0x0000D198
		private static void __rpc_handler_1311848200(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((PlayBarManager)target).ChangeMatchImmediateSolo_ServerRpc(server);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060001F0 RID: 496 RVA: 0x0000EFF8 File Offset: 0x0000D1F8
		private static void __rpc_handler_2535256494(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			bool flag;
			reader.ReadValueSafe<bool>(out flag, default(FastBufferWriter.ForPrimitives));
			int[] array = null;
			if (flag)
			{
				reader.ReadValueSafe<int>(out array, default(FastBufferWriter.ForPrimitives));
			}
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((PlayBarManager)target).ChangeMatchImmediateParty_ServerRpc(array, server);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060001F1 RID: 497 RVA: 0x0000F0A0 File Offset: 0x0000D2A0
		private static void __rpc_handler_1970652522(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			bool flag;
			reader.ReadValueSafe<bool>(out flag, default(FastBufferWriter.ForPrimitives));
			int[] array = null;
			if (flag)
			{
				reader.ReadValueSafe<int>(out array, default(FastBufferWriter.ForPrimitives));
			}
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((PlayBarManager)target).ReadyPlayers_ClientRpc(array);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060001F2 RID: 498 RVA: 0x0000F13C File Offset: 0x0000D33C
		private static void __rpc_handler_844158908(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			float num;
			reader.ReadValueSafe<float>(out num, default(FastBufferWriter.ForPrimitives));
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((PlayBarManager)target).UpdateMatchTimer_ClientRpc(num);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060001F3 RID: 499 RVA: 0x0000F1AC File Offset: 0x0000D3AC
		private static void __rpc_handler_4248026517(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((PlayBarManager)target).PlayHit_ServerRpc(server);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060001F4 RID: 500 RVA: 0x0000F20C File Offset: 0x0000D40C
		private static void __rpc_handler_1740879778(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			bool flag;
			reader.ReadValueSafe<bool>(out flag, default(FastBufferWriter.ForPrimitives));
			int[] array = null;
			if (flag)
			{
				reader.ReadValueSafe<int>(out array, default(FastBufferWriter.ForPrimitives));
			}
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((PlayBarManager)target).PlayHit_PartyLeader_ServerRpc(array, server);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060001F5 RID: 501 RVA: 0x0000F2B4 File Offset: 0x0000D4B4
		private static void __rpc_handler_380088622(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((PlayBarManager)target).UnplayHit_ServerRpc(server);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060001F6 RID: 502 RVA: 0x0000F314 File Offset: 0x0000D514
		private static void __rpc_handler_2524213796(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			bool flag;
			reader.ReadValueSafe<bool>(out flag, default(FastBufferWriter.ForPrimitives));
			int[] array = null;
			if (flag)
			{
				reader.ReadValueSafe<int>(out array, default(FastBufferWriter.ForPrimitives));
			}
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((PlayBarManager)target).UnplayHit_PartyLeader_ServerRpc(array, server);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060001F7 RID: 503 RVA: 0x0000F3BC File Offset: 0x0000D5BC
		private static void __rpc_handler_3062243066(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			bool flag;
			reader.ReadValueSafe<bool>(out flag, default(FastBufferWriter.ForPrimitives));
			int[] array = null;
			if (flag)
			{
				reader.ReadValueSafe<int>(out array, default(FastBufferWriter.ForPrimitives));
			}
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((PlayBarManager)target).UnreadyPlayers_ClientRpc(array);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060001F8 RID: 504 RVA: 0x0000F458 File Offset: 0x0000D658
		private static void __rpc_handler_28542295(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			int num;
			ByteUnpacker.ReadValueBitPacked(reader, out num);
			PlayBarManager.PlayerReadyStatus playerReadyStatus;
			reader.ReadValueSafe<PlayBarManager.PlayerReadyStatus>(out playerReadyStatus, default(FastBufferWriter.ForEnums));
			PlayBarManager.PlayerReadyStatus playerReadyStatus2;
			reader.ReadValueSafe<PlayBarManager.PlayerReadyStatus>(out playerReadyStatus2, default(FastBufferWriter.ForEnums));
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((PlayBarManager)target).HandlePlayerReadyStatusChanged_ClientRpc(num, playerReadyStatus, playerReadyStatus2);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060001F9 RID: 505 RVA: 0x0000F4F8 File Offset: 0x0000D6F8
		protected internal override string __getTypeName()
		{
			return "PlayBarManager";
		}

		// Token: 0x04000246 RID: 582
		[HideInInspector]
		public UnityEvent<int, PlayBarManager.PlayerReadyStatus, PlayBarManager.PlayerReadyStatus> OnPlayerReadyChanged = new UnityEvent<int, PlayBarManager.PlayerReadyStatus, PlayBarManager.PlayerReadyStatus>();

		// Token: 0x04000247 RID: 583
		[HideInInspector]
		public UnityEvent OnMatchTimerUpdated = new UnityEvent();

		// Token: 0x04000248 RID: 584
		[SerializeField]
		private float matchTransitionDuration = 30f;

		// Token: 0x04000249 RID: 585
		[SerializeField]
		private float allReadyDuration = 5f;

		// Token: 0x0400024A RID: 586
		[SerializeField]
		private float soloDuration = 1f;

		// Token: 0x0400024B RID: 587
		private HashSet<int> readyUserIds = new HashSet<int>();

		// Token: 0x0400024C RID: 588
		private Coroutine timerCoroutine;

		// Token: 0x02000084 RID: 132
		public enum PlayerReadyStatus
		{
			// Token: 0x0400024F RID: 591
			None,
			// Token: 0x04000250 RID: 592
			NotReady,
			// Token: 0x04000251 RID: 593
			Ready
		}

		// Token: 0x02000085 RID: 133
		public enum PlayOption
		{
			// Token: 0x04000253 RID: 595
			None,
			// Token: 0x04000254 RID: 596
			PlayWithParty,
			// Token: 0x04000255 RID: 597
			PlaySolo,
			// Token: 0x04000256 RID: 598
			StopSolo,
			// Token: 0x04000257 RID: 599
			StopWithParty
		}
	}
}
