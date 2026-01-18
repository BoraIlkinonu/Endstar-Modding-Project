using System;
using Endless.Matchmaking;
using Endless.Shared;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.Test
{
	// Token: 0x020000DD RID: 221
	public class GameLoadFpsInfo : BaseFpsInfo
	{
		// Token: 0x1700009B RID: 155
		// (get) Token: 0x060004F8 RID: 1272 RVA: 0x00018408 File Offset: 0x00016608
		private AutomaticMatchStarterData LevelData
		{
			get
			{
				switch (MatchmakingClientController.Instance.NetworkEnv)
				{
				case NetworkEnvironment.DEV:
					return this.devLevelData;
				case NetworkEnvironment.STAGING:
					return this.stagingLevelData;
				case NetworkEnvironment.PROD:
					return this.prodLevelData;
				default:
					return null;
				}
			}
		}

		// Token: 0x1700009C RID: 156
		// (get) Token: 0x060004F9 RID: 1273 RVA: 0x0001844A File Offset: 0x0001664A
		public override bool IsDone
		{
			get
			{
				return this.isDone;
			}
		}

		// Token: 0x060004FA RID: 1274 RVA: 0x00018454 File Offset: 0x00016654
		public override void StartTest()
		{
			this.isDone = false;
			NetworkBehaviourSingleton<GameStateManager>.Instance.OnGameStateChanged.AddListener(new UnityAction<GameState, GameState>(this.HandleGameStateChanged));
			if (this.LevelData != null)
			{
				this.LevelData.TryToStartMatch();
				return;
			}
			this.HandleLoadingCompleted();
		}

		// Token: 0x060004FB RID: 1275 RVA: 0x000184A3 File Offset: 0x000166A3
		private void HandleGameStateChanged(GameState previousState, GameState newState)
		{
			if (newState == GameState.Creator || newState == GameState.Gameplay)
			{
				this.HandleLoadingCompleted();
			}
		}

		// Token: 0x060004FC RID: 1276 RVA: 0x000184B3 File Offset: 0x000166B3
		private void HandleLoadingCompleted()
		{
			this.isDone = true;
		}

		// Token: 0x060004FD RID: 1277 RVA: 0x000184BC File Offset: 0x000166BC
		public override void StopTest()
		{
			NetworkBehaviourSingleton<GameStateManager>.Instance.OnGameStateChanged.RemoveListener(new UnityAction<GameState, GameState>(this.HandleGameStateChanged));
		}

		// Token: 0x060004FE RID: 1278 RVA: 0x0000229D File Offset: 0x0000049D
		protected override void ProcessFrame_Internal()
		{
		}

		// Token: 0x04000366 RID: 870
		[SerializeField]
		private AutomaticMatchStarterData devLevelData;

		// Token: 0x04000367 RID: 871
		[SerializeField]
		private AutomaticMatchStarterData stagingLevelData;

		// Token: 0x04000368 RID: 872
		[SerializeField]
		private AutomaticMatchStarterData prodLevelData;

		// Token: 0x04000369 RID: 873
		private bool isDone;
	}
}
