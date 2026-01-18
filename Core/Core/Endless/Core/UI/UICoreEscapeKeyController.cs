using System;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI
{
	// Token: 0x02000045 RID: 69
	public class UICoreEscapeKeyController : UIGameObject
	{
		// Token: 0x06000155 RID: 341 RVA: 0x00008B6C File Offset: 0x00006D6C
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			MonoBehaviourSingleton<BackManager>.Instance.OnEscapeWithNoContext.AddListener(new UnityAction(this.OnEscapeWithNoContext));
			UIBaseScreenView.CloseBegunAction = (Action<UIBaseScreenView>)Delegate.Combine(UIBaseScreenView.CloseBegunAction, new Action<UIBaseScreenView>(this.OnScreenClose));
		}

		// Token: 0x06000156 RID: 342 RVA: 0x00008BCC File Offset: 0x00006DCC
		private void OnEscapeWithNoContext()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethodWithAppension(this, "OnEscapeWithNoContext", NetworkBehaviourSingleton<GameStateManager>.Instance.SharedGameState.ToString(), Array.Empty<object>());
			}
			if (NetworkBehaviourSingleton<GameStateManager>.Instance.SharedGameState == GameState.None || NetworkBehaviourSingleton<GameStateManager>.Instance.SharedGameState == GameState.Default)
			{
				return;
			}
			if (this.mainMenuScreen)
			{
				MonoBehaviourSingleton<UIScreenManager>.Instance.Close(UIScreenManager.CloseStackActions.Clear, null, false, false);
				return;
			}
			this.mainMenuScreen = UIMainMenuScreenView.Display(UIScreenManager.DisplayStackActions.ClearAndPush);
		}

		// Token: 0x06000157 RID: 343 RVA: 0x00008C4B File Offset: 0x00006E4B
		private void OnScreenClose(UIBaseScreenView closedScreen)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnScreenClose", new object[] { closedScreen.GetType().Name });
			}
			if (closedScreen == this.mainMenuScreen)
			{
				this.mainMenuScreen = null;
			}
		}

		// Token: 0x040000E2 RID: 226
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x040000E3 RID: 227
		private UIMainMenuScreenView mainMenuScreen;
	}
}
