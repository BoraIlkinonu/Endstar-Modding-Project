using System;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI
{
	// Token: 0x0200006E RID: 110
	[DisallowMultipleComponent]
	public class UIModalGameStateCloseHandler : UIGameObject
	{
		// Token: 0x060001FF RID: 511 RVA: 0x0000B560 File Offset: 0x00009760
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			NetworkBehaviourSingleton<GameStateManager>.Instance.OnGameStateChanged.AddListener(new UnityAction<GameState, GameState>(this.OnGameStateChanged));
		}

		// Token: 0x06000200 RID: 512 RVA: 0x0000B598 File Offset: 0x00009798
		private void OnGameStateChanged(GameState oldGameState, GameState newGameState)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnGameStateChanged", new object[] { oldGameState, newGameState });
			}
			if (newGameState == GameState.LoadingGameplay || newGameState == GameState.LoadedGameplay || newGameState == GameState.StartingGameplay || newGameState == GameState.Gameplay || newGameState == GameState.GameplayOutro)
			{
				MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
			}
		}

		// Token: 0x0400016E RID: 366
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
