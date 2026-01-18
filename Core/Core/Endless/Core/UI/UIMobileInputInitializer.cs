using System;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI
{
	// Token: 0x02000063 RID: 99
	public class UIMobileInputInitializer : UIGameObject
	{
		// Token: 0x060001D1 RID: 465 RVA: 0x0000AD64 File Offset: 0x00008F64
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			if (!MobileUtility.IsMobile)
			{
				return;
			}
			NetworkBehaviourSingleton<GameStateManager>.Instance.OnGameStateChanged.AddListener(new UnityAction<GameState, GameState>(this.OnGameStateChanged));
		}

		// Token: 0x060001D2 RID: 466 RVA: 0x0000ADA4 File Offset: 0x00008FA4
		private void OnGameStateChanged(GameState previousState, GameState currentState)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnGameStateChanged", new object[] { previousState, currentState });
			}
			if (currentState != this.intendedGameState)
			{
				return;
			}
			RectTransform rectTransform;
			global::UnityEngine.Object.Instantiate<GameObject>(this.mobileInputSource, base.transform).TryGetComponent<RectTransform>(out rectTransform);
			rectTransform.SetAnchor(AnchorPresets.StretchAll, 0f, 0f, 0f, 0f);
			NetworkBehaviourSingleton<GameStateManager>.Instance.OnGameStateChanged.RemoveListener(new UnityAction<GameState, GameState>(this.OnGameStateChanged));
		}

		// Token: 0x0400014C RID: 332
		[SerializeField]
		private GameObject mobileInputSource;

		// Token: 0x0400014D RID: 333
		[SerializeField]
		private GameState intendedGameState = GameState.Gameplay;

		// Token: 0x0400014E RID: 334
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
