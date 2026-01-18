using System;
using Endless.Gameplay;
using Endless.Matchmaking;
using Endless.Shared;
using UnityEngine;

namespace Endless.Core.GameStates
{
	// Token: 0x020000C8 RID: 200
	public class DefaultGameState : GameStateBase
	{
		// Token: 0x17000086 RID: 134
		// (get) Token: 0x0600048F RID: 1167 RVA: 0x00003CF2 File Offset: 0x00001EF2
		public override GameState StateType
		{
			get
			{
				return GameState.Default;
			}
		}

		// Token: 0x06000490 RID: 1168 RVA: 0x00016708 File Offset: 0x00014908
		public override void StateEntered(GameState oldState)
		{
			Screen.sleepTimeout = -2;
			MonoBehaviourSingleton<RuntimeDatabase>.Instance.ClearActiveGame();
			NetworkBehaviourSingleton<UserIdManager>.Instance.ClearUserIds();
			MatchSession.OnMatchSessionStart += this.HandleMatchSessionStarted;
		}

		// Token: 0x06000491 RID: 1169 RVA: 0x00016736 File Offset: 0x00014936
		private void HandleMatchSessionStarted(MatchSession matchSession)
		{
			MonoBehaviourSingleton<LoadTimeTester>.Instance.StartTracking("MatchLoad");
			NetworkBehaviourSingleton<GameStateManager>.Instance.EnterValidateGameLibraryState(matchSession.MatchData, matchSession.MatchData.IsEditSession ? GameState.LoadingCreator : GameState.LoadingGameplay);
		}

		// Token: 0x06000492 RID: 1170 RVA: 0x00016768 File Offset: 0x00014968
		public override void HandleExitingState(GameState newState)
		{
			Screen.sleepTimeout = -1;
			Debug.Log("Exiting default game state");
			MatchSession.OnMatchSessionStart -= this.HandleMatchSessionStarted;
		}
	}
}
