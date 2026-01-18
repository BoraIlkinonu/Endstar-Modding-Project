using System;
using System.Collections;
using Endless.Core.UI;
using Endless.Data.UI;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using UnityEngine;

namespace Endless.Core.GameStates
{
	// Token: 0x020000CC RID: 204
	public class LoadedGameplayGameState : GameStateBase
	{
		// Token: 0x1700008D RID: 141
		// (get) Token: 0x060004AE RID: 1198 RVA: 0x00016BED File Offset: 0x00014DED
		public override GameState StateType
		{
			get
			{
				return GameState.LoadedGameplay;
			}
		}

		// Token: 0x060004AF RID: 1199 RVA: 0x00016BF0 File Offset: 0x00014DF0
		public override void HandleExitingState(GameState newState)
		{
			if (newState != GameState.Gameplay)
			{
				MonoBehaviourSingleton<StageManager>.Instance.FlushLoadedAndSpawnedStages(base.IsServer);
			}
			NetworkBehaviourSingleton<GameStateManager>.Instance.OnClientStateCollectionUpdated -= this.HandleClientStateUpdated;
			MonoBehaviourSingleton<UIScreenCoverTokenHandler>.Instance.ReleaseToken(UIScreenCoverTokens.LoadedGameplayGameState);
		}

		// Token: 0x060004B0 RID: 1200 RVA: 0x00016C28 File Offset: 0x00014E28
		public override void StateEntered(GameState oldState)
		{
			MonoBehaviourSingleton<UIScreenCoverTokenHandler>.Instance.Display(UIScreenCoverTokens.LoadedGameplayGameState, null, true);
			if (base.IsServer)
			{
				int num;
				int num2;
				if (NetworkBehaviourSingleton<GameStateManager>.Instance.AreAllClientsInState(GameState.LoadedGameplay, out num, out num2))
				{
					this.EnterGameplay();
					return;
				}
				MonoBehaviourSingleton<LoadTimeTester>.Instance.Pause("MatchLoad");
				MonoBehaviourSingleton<UIScreenCoverTokenHandler>.Instance.UpdateText(UIScreenCoverTokens.LoadedGameplayGameState, string.Format("Waiting on other players to load: {0:N0}/{1:N0}", num, num2));
				NetworkBehaviourSingleton<GameStateManager>.Instance.OnClientStateCollectionUpdated += this.HandleClientStateUpdated;
				this.timeoutCoroutine = NetworkBehaviourSingleton<GameStateManager>.Instance.StartCoroutine(this.TimeoutSlowClients());
			}
		}

		// Token: 0x060004B1 RID: 1201 RVA: 0x00016CC0 File Offset: 0x00014EC0
		private void EnterGameplay()
		{
			base.ChangeState(new GameplayGameState());
		}

		// Token: 0x060004B2 RID: 1202 RVA: 0x00016CD0 File Offset: 0x00014ED0
		private void HandleClientStateUpdated()
		{
			int num;
			int num2;
			if (NetworkBehaviourSingleton<GameStateManager>.Instance.AreAllClientsInState(GameState.LoadedGameplay, out num, out num2))
			{
				if (this.timeoutCoroutine != null)
				{
					NetworkBehaviourSingleton<GameStateManager>.Instance.StopCoroutine(this.timeoutCoroutine);
				}
				MonoBehaviourSingleton<LoadTimeTester>.Instance.Resume("MatchLoad");
				this.EnterGameplay();
				return;
			}
			MonoBehaviourSingleton<UIScreenCoverTokenHandler>.Instance.UpdateText(UIScreenCoverTokens.LoadedGameplayGameState, string.Format("Waiting on other players to load: {0:N0}/{1:N0}", num, num2));
		}

		// Token: 0x060004B3 RID: 1203 RVA: 0x00016D3E File Offset: 0x00014F3E
		private IEnumerator TimeoutSlowClients()
		{
			yield return new WaitForSecondsRealtime(60f);
			NetworkBehaviourSingleton<GameStateManager>.Instance.DisconnectClientsNotInState(this.StateType, "Loading gameplay timed out.");
			yield break;
		}

		// Token: 0x04000319 RID: 793
		private const float postLoadClientTimeoutDuration = 60f;

		// Token: 0x0400031A RID: 794
		private Coroutine timeoutCoroutine;
	}
}
