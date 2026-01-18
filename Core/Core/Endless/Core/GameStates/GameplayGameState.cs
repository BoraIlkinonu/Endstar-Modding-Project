using System;
using System.Collections;
using Endless.Core.UI;
using Endless.Data.UI;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Matchmaking;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.GameStates
{
	// Token: 0x020000C9 RID: 201
	public class GameplayGameState : GameStateBase
	{
		// Token: 0x17000087 RID: 135
		// (get) Token: 0x06000494 RID: 1172 RVA: 0x0001678B File Offset: 0x0001498B
		public override GameState StateType
		{
			get
			{
				return GameState.Gameplay;
			}
		}

		// Token: 0x06000495 RID: 1173 RVA: 0x00016790 File Offset: 0x00014990
		public override void StateEntered(GameState oldState)
		{
			MonoBehaviourSingleton<GameplayManager>.Instance.StartGameplay();
			if (base.IsServer)
			{
				NetworkBehaviourSingleton<GameStateManager>.Instance.UpdateClientStates(GameState.Gameplay);
				MonoBehaviourSingleton<GameplayManager>.Instance.RequestLevelChange = new Action<SerializableGuid>(this.HandleLevelChangeRequest);
				NetworkBehaviourSingleton<GameStateManager>.Instance.OnGameplayFlipRequested.AddListener(new UnityAction(this.HandleFlipRequested));
			}
			this.networkSyncingStartFrameCoroutine = NetworkBehaviourSingleton<GameStateManager>.Instance.StartCoroutine(this.NetworkSyncingStartFrameProcess());
			MonoBehaviourSingleton<LoadTimeTester>.Instance.LogTimeDelta("GameplayGameState", "MatchLoad");
			MonoBehaviourSingleton<LoadTimeTester>.Instance.StopTracking("MatchLoad");
		}

		// Token: 0x06000496 RID: 1174 RVA: 0x00016824 File Offset: 0x00014A24
		private IEnumerator NetworkSyncingStartFrameProcess()
		{
			MonoBehaviourSingleton<UIScreenCoverTokenHandler>.Instance.Display(UIScreenCoverTokens.WaitingForNetworkToStartGameplay, null, true);
			if (base.IsServer)
			{
				yield return new NetworkTickUtility.WaitForNetworkTicks(1);
				NetworkBehaviourSingleton<NetClock>.Instance.GameplayReadyFrame.Value = NetClock.CurrentFrame + 20U;
			}
			while ((NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer) && NetClock.CurrentFrame < NetworkBehaviourSingleton<NetClock>.Instance.GameplayReadyFrame.Value)
			{
				yield return null;
			}
			float num = (float)(base.IsServer ? (NetworkManager.Singleton.ServerTime.Time - NetClock.ServerAppearanceTime) : (NetClock.ClientExtrapolatedTime - NetClock.ClientExtrapolatedAppearanceTime));
			yield return new WaitForSecondsRealtime(num);
			MonoBehaviourSingleton<UIScreenCoverTokenHandler>.Instance.ReleaseToken(UIScreenCoverTokens.WaitingForNetworkToStartGameplay);
			this.networkSyncingStartFrameCoroutine = null;
			yield break;
		}

		// Token: 0x06000497 RID: 1175 RVA: 0x00016834 File Offset: 0x00014A34
		private void HandleLevelChangeRequest(SerializableGuid newLevelId)
		{
			Debug.Log(string.Format("Receiving Level Change Request: Current Level - {0}, Requested Level - {1}", NetworkBehaviourSingleton<GameStateManager>.Instance.LevelId, newLevelId));
			MonoBehaviourSingleton<GameplayManager>.Instance.RequestLevelChange = null;
			NetworkBehaviourSingleton<GameStateManager>.Instance.SetNewLevelId(newLevelId);
			base.ChangeState(new LoadingGameplayGameState(newLevelId, MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.GetLevelReferenceById(newLevelId).AssetVersion));
		}

		// Token: 0x06000498 RID: 1176 RVA: 0x0001689C File Offset: 0x00014A9C
		private void HandleFlipRequested()
		{
			base.ChangeState(new LoadingCreatorGameState(MatchmakingClientController.Instance.LocalMatch.GetLevelId()));
		}

		// Token: 0x06000499 RID: 1177 RVA: 0x000168C0 File Offset: 0x00014AC0
		private void RestartGame()
		{
			MonoBehaviourSingleton<GameplayManager>.Instance.CleanupGameplay();
			LevelReference levelReference = MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.levels[0];
			base.ChangeState(new LoadingGameplayGameState(levelReference.AssetID, levelReference.AssetVersion));
		}

		// Token: 0x0600049A RID: 1178 RVA: 0x0001690C File Offset: 0x00014B0C
		public override void HandleExitingState(GameState newState)
		{
			if (this.networkSyncingStartFrameCoroutine != null)
			{
				MonoBehaviourSingleton<UIScreenCoverTokenHandler>.Instance.ReleaseToken(UIScreenCoverTokens.WaitingForNetworkToStartGameplay);
				this.networkSyncingStartFrameCoroutine = null;
			}
			bool flag = newState == GameState.Default || newState == GameState.LoadingCreator;
			if (!flag && NetworkManager.Singleton.IsServer)
			{
				MonoBehaviourSingleton<GameplayManager>.Instance.SavePropStates();
			}
			MonoBehaviourSingleton<GameplayManager>.Instance.StopGameplay();
			if (!base.IsServer && NetworkBehaviourSingleton<GameStateManager>.Instance.LevelId == SerializableGuid.Empty)
			{
				NetworkBehaviourSingleton<GameplayMessagingManager>.Instance.LocalRestartLevel();
			}
			MonoBehaviourSingleton<StageManager>.Instance.FlushLoadedAndSpawnedStages(base.IsServer);
			if (flag)
			{
				MonoBehaviourSingleton<GameplayManager>.Instance.CleanupGameplay();
			}
			if (base.IsServer)
			{
				MonoBehaviourSingleton<GameplayManager>.Instance.RequestLevelChange = null;
				NetworkBehaviourSingleton<GameStateManager>.Instance.OnGameplayFlipRequested.RemoveListener(new UnityAction(this.HandleFlipRequested));
				NetworkBehaviourSingleton<NetClock>.Instance.GameplayReadyFrame.Value = uint.MaxValue;
			}
		}

		// Token: 0x04000312 RID: 786
		private const uint CLIENT_SYNC_TIMEFRAME = 20U;

		// Token: 0x04000313 RID: 787
		private bool isRestarting;

		// Token: 0x04000314 RID: 788
		private Coroutine networkSyncingStartFrameCoroutine;
	}
}
