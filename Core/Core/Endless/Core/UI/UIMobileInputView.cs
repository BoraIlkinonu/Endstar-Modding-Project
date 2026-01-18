using System;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI
{
	// Token: 0x02000064 RID: 100
	public class UIMobileInputView : UIGameObject
	{
		// Token: 0x17000032 RID: 50
		// (get) Token: 0x060001D4 RID: 468 RVA: 0x0000AE45 File Offset: 0x00009045
		private bool CurrentGameStateEqualsDisplayIfGameState
		{
			get
			{
				return NetworkBehaviourSingleton<GameStateManager>.Instance.CurrentState == this.displayIfGameState;
			}
		}

		// Token: 0x060001D5 RID: 469 RVA: 0x0000AE5C File Offset: 0x0000905C
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			if (MobileUtility.IsMobile)
			{
				NetworkBehaviourSingleton<GameStateManager>.Instance.OnGameStateChanged.AddListener(new UnityAction<GameState, GameState>(this.OnGameStateChanged));
				MonoBehaviourSingleton<UIWindowManager>.Instance.DisplayUnityEvent.AddListener(new UnityAction<UIBaseWindowView>(this.OnWindowDisplayed));
				UIBaseWindowView.CloseAction = (Action<UIBaseWindowView>)Delegate.Combine(UIBaseWindowView.CloseAction, new Action<UIBaseWindowView>(this.OnWindowClosed));
				this.displayAndHideHandler.SetToDisplayStart(true);
				this.displayAndHideHandler.Display();
				this.globallyVisible = true;
				return;
			}
			base.gameObject.SetActive(false);
		}

		// Token: 0x060001D6 RID: 470 RVA: 0x0000AF09 File Offset: 0x00009109
		private void OnDestroy()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDestroy", Array.Empty<object>());
			}
			UIBaseWindowView.CloseAction = (Action<UIBaseWindowView>)Delegate.Remove(UIBaseWindowView.CloseAction, new Action<UIBaseWindowView>(this.OnWindowClosed));
		}

		// Token: 0x060001D7 RID: 471 RVA: 0x0000AF43 File Offset: 0x00009143
		private void OnWindowDisplayed(UIBaseWindowView window)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnWindowDisplayed", new object[] { window.GetType().Name });
			}
			this.SetVisibility(false);
		}

		// Token: 0x060001D8 RID: 472 RVA: 0x0000AF73 File Offset: 0x00009173
		private void OnWindowClosed(UIBaseWindowView window)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnWindowClosed", new object[] { window.GetType().Name });
			}
			this.SetVisibility(this.CurrentGameStateEqualsDisplayIfGameState);
		}

		// Token: 0x060001D9 RID: 473 RVA: 0x0000AFA8 File Offset: 0x000091A8
		private void OnGameStateChanged(GameState previousState, GameState currentState)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnGameStateChanged", new object[] { previousState, currentState });
			}
			this.SetVisibility(this.CurrentGameStateEqualsDisplayIfGameState);
		}

		// Token: 0x060001DA RID: 474 RVA: 0x0000AFE4 File Offset: 0x000091E4
		private void SetVisibility(bool newValue)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetVisibility", new object[] { newValue });
			}
			if (this.globallyVisible == newValue)
			{
				return;
			}
			this.globallyVisible = newValue;
			if (this.globallyVisible)
			{
				this.displayAndHideHandler.Display();
				return;
			}
			this.displayAndHideHandler.Hide();
		}

		// Token: 0x0400014F RID: 335
		[SerializeField]
		private GameState displayIfGameState = GameState.Gameplay;

		// Token: 0x04000150 RID: 336
		[SerializeField]
		private UIDisplayAndHideHandler displayAndHideHandler;

		// Token: 0x04000151 RID: 337
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000152 RID: 338
		private bool globallyVisible;
	}
}
