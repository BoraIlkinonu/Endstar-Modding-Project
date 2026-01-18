using System;
using Endless.Data;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Endless.Shared.UI.Windows;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Endless.Core.UI
{
	// Token: 0x02000046 RID: 70
	[DefaultExecutionOrder(2147483647)]
	public class UICursorVisibilityHandler : MonoBehaviour
	{
		// Token: 0x06000159 RID: 345 RVA: 0x00008C89 File Offset: 0x00006E89
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			if (!this.initialized)
			{
				this.Initialize();
			}
		}

		// Token: 0x0600015A RID: 346 RVA: 0x00008CB1 File Offset: 0x00006EB1
		private void Initialize()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Initialize", Array.Empty<object>());
			}
			if (this.initialized)
			{
				return;
			}
			this.playerInputActions = new PlayerInputActions();
			this.RegisterListeners();
			this.initialized = true;
		}

		// Token: 0x0600015B RID: 347 RVA: 0x00008CEC File Offset: 0x00006EEC
		private void OnDestroy()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDestroy", Array.Empty<object>());
			}
			this.UnregisterListeners();
		}

		// Token: 0x0600015C RID: 348 RVA: 0x00008D0C File Offset: 0x00006F0C
		private void OnApplicationFocus(bool hasFocus)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnApplicationFocus", new object[] { hasFocus });
			}
			if (!hasFocus)
			{
				return;
			}
			this.UpdateCursor();
		}

		// Token: 0x0600015D RID: 349 RVA: 0x00008D3A File Offset: 0x00006F3A
		private void OnApplicationPause(bool pauseStatus)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnApplicationPause", new object[] { pauseStatus });
			}
			if (!pauseStatus)
			{
				return;
			}
			this.UpdateCursor();
		}

		// Token: 0x0600015E RID: 350 RVA: 0x00008D68 File Offset: 0x00006F68
		private void RegisterListeners()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "RegisterListeners", Array.Empty<object>());
			}
			NetworkBehaviourSingleton<GameStateManager>.Instance.OnGameStateChanged.AddListener(new UnityAction<GameState, GameState>(this.OnGameStateChanged));
			UIScreenManager.OnScreenSystemOpen = (Action)Delegate.Combine(UIScreenManager.OnScreenSystemOpen, new Action(this.UpdateCursor));
			UIScreenManager.OnScreenSystemClose = (Action)Delegate.Combine(UIScreenManager.OnScreenSystemClose, new Action(this.UpdateCursor));
			MonoBehaviourSingleton<UIWindowManager>.Instance.DisplayUnityEvent.AddListener(new UnityAction<UIBaseWindowView>(this.OnWindowDisplayed));
			UIBaseWindowView.CloseAction = (Action<UIBaseWindowView>)Delegate.Combine(UIBaseWindowView.CloseAction, new Action<UIBaseWindowView>(this.OnWindowClosed));
			MonoBehaviourSingleton<UINewWindowManager>.Instance.OnWindowOpened += this.UpdateCursor;
			MonoBehaviourSingleton<UINewWindowManager>.Instance.OnAllWindowsClosed += this.UpdateCursor;
			UIUserReportWindowView.OnDisplay = (Action)Delegate.Combine(UIUserReportWindowView.OnDisplay, new Action(this.UpdateCursor));
			UIUserReportWindowView.OnHide = (Action)Delegate.Combine(UIUserReportWindowView.OnHide, new Action(this.UpdateCursor));
			this.playerInputActions.Player.EnableCursor.started += this.OnEnableCursorButtonStateChange;
			this.playerInputActions.Player.EnableCursor.canceled += this.OnEnableCursorButtonStateChange;
			this.playerInputActions.Player.EnableCursor.Enable();
		}

		// Token: 0x0600015F RID: 351 RVA: 0x00008EF0 File Offset: 0x000070F0
		private void UnregisterListeners()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "UnregisterListeners", Array.Empty<object>());
			}
			NetworkBehaviourSingleton<GameStateManager>.Instance.OnGameStateChanged.RemoveListener(new UnityAction<GameState, GameState>(this.OnGameStateChanged));
			UIScreenManager.OnScreenSystemOpen = (Action)Delegate.Remove(UIScreenManager.OnScreenSystemOpen, new Action(this.UpdateCursor));
			UIScreenManager.OnScreenSystemClose = (Action)Delegate.Remove(UIScreenManager.OnScreenSystemClose, new Action(this.UpdateCursor));
			MonoBehaviourSingleton<UIWindowManager>.Instance.DisplayUnityEvent.RemoveListener(new UnityAction<UIBaseWindowView>(this.OnWindowDisplayed));
			UIBaseWindowView.CloseAction = (Action<UIBaseWindowView>)Delegate.Remove(UIBaseWindowView.CloseAction, new Action<UIBaseWindowView>(this.OnWindowClosed));
			MonoBehaviourSingleton<UINewWindowManager>.Instance.OnWindowOpened -= this.UpdateCursor;
			MonoBehaviourSingleton<UINewWindowManager>.Instance.OnAllWindowsClosed -= this.UpdateCursor;
			UIUserReportWindowView.OnDisplay = (Action)Delegate.Remove(UIUserReportWindowView.OnDisplay, new Action(this.UpdateCursor));
			UIUserReportWindowView.OnHide = (Action)Delegate.Remove(UIUserReportWindowView.OnHide, new Action(this.UpdateCursor));
			this.playerInputActions.Player.EnableCursor.started -= this.OnEnableCursorButtonStateChange;
			this.playerInputActions.Player.EnableCursor.canceled -= this.OnEnableCursorButtonStateChange;
			this.playerInputActions.Player.EnableCursor.Disable();
		}

		// Token: 0x06000160 RID: 352 RVA: 0x00009077 File Offset: 0x00007277
		private void OnWindowDisplayed(UIBaseWindowView displayedWindow)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnWindowDisplayed", new object[] { displayedWindow });
			}
			this.UpdateCursor();
		}

		// Token: 0x06000161 RID: 353 RVA: 0x0000909C File Offset: 0x0000729C
		private void OnWindowClosed(UIBaseWindowView closedWindow)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnWindowClosed", new object[] { closedWindow });
			}
			this.UpdateCursor();
		}

		// Token: 0x06000162 RID: 354 RVA: 0x000090C1 File Offset: 0x000072C1
		private void OnGameStateChanged(GameState oldGameState, GameState newGameState)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnGameStateChanged", new object[] { oldGameState, newGameState });
			}
			this.UpdateCursor();
		}

		// Token: 0x06000163 RID: 355 RVA: 0x000090F4 File Offset: 0x000072F4
		private void OnEnableCursorButtonStateChange(InputAction.CallbackContext callbackContext)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnableCursorButtonStateChange", new object[] { callbackContext });
			}
			this.UpdateCursor();
		}

		// Token: 0x06000164 RID: 356 RVA: 0x00009120 File Offset: 0x00007320
		private void UpdateCursor()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "UpdateCursor", Array.Empty<object>());
			}
			if (!this.initialized)
			{
				this.Initialize();
			}
			bool flag = true;
			if (NetworkBehaviourSingleton<GameStateManager>.Instance)
			{
				flag = NetworkBehaviourSingleton<GameStateManager>.Instance.CurrentState != GameState.Gameplay;
			}
			Cursor.lockState = ((Cursor.visible = flag || MonoBehaviourSingleton<UIScreenManager>.Instance.IsDisplaying || MonoBehaviourSingleton<UIWindowManager>.Instance.IsDisplaying || MonoBehaviourSingleton<UINewWindowManager>.Instance.IsDisplayingAnyWindows || MonoBehaviourSingleton<UIUserReportWindowView>.Instance.IsDisplaying || this.playerInputActions.Player.EnableCursor.IsPressed()) ? CursorLockMode.None : CursorLockMode.Locked);
		}

		// Token: 0x040000E4 RID: 228
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x040000E5 RID: 229
		private PlayerInputActions playerInputActions;

		// Token: 0x040000E6 RID: 230
		private bool initialized;
	}
}
