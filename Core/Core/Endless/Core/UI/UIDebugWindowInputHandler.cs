using System;
using Endless.Matchmaking;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Endless.Core.UI
{
	// Token: 0x020000A2 RID: 162
	public class UIDebugWindowInputHandler : MonoBehaviour
	{
		// Token: 0x06000374 RID: 884 RVA: 0x000122F4 File Offset: 0x000104F4
		private void Awake()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Awake", Array.Empty<object>());
			}
			this.canOpenDebugWindow = Application.isEditor || Debug.isDebugBuild || MatchmakingClientController.Instance.NetworkEnv == NetworkEnvironment.STAGING;
			if (this.canOpenDebugWindow)
			{
				this.button.onClick.AddListener(new UnityAction(this.ToggleDebugWindow));
				this.endlessSharedInputActions = new EndlessSharedInputActions();
				return;
			}
			base.gameObject.SetActive(false);
			this.button.gameObject.SetActive(false);
		}

		// Token: 0x06000375 RID: 885 RVA: 0x0001238C File Offset: 0x0001058C
		private void OnEnable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnable", Array.Empty<object>());
			}
			if (!this.canOpenDebugWindow)
			{
				return;
			}
			this.endlessSharedInputActions.UI.ToggleDebugWindow.Bind(new Action<InputAction.CallbackContext>(this.ToggleDebugWindow));
		}

		// Token: 0x06000376 RID: 886 RVA: 0x000123E0 File Offset: 0x000105E0
		private void OnDisable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDisable", Array.Empty<object>());
			}
			if (!this.canOpenDebugWindow)
			{
				return;
			}
			this.endlessSharedInputActions.UI.ToggleDebugWindow.Unbind(new Action<InputAction.CallbackContext>(this.ToggleDebugWindow));
		}

		// Token: 0x06000377 RID: 887 RVA: 0x00012432 File Offset: 0x00010632
		private void ToggleDebugWindow(InputAction.CallbackContext callbackContext)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log("ToggleDebugWindow ( callbackContext: " + callbackContext.action.name + " )", this);
			}
			if (!this.canOpenDebugWindow)
			{
				return;
			}
			this.ToggleDebugWindow();
		}

		// Token: 0x06000378 RID: 888 RVA: 0x0001246C File Offset: 0x0001066C
		private void ToggleDebugWindow()
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log("ToggleDebugWindow", this);
			}
			if (!this.canOpenDebugWindow)
			{
				return;
			}
			if (this.displayedDebugWindow)
			{
				UIBaseWindowView.CloseAction = (Action<UIBaseWindowView>)Delegate.Remove(UIBaseWindowView.CloseAction, new Action<UIBaseWindowView>(this.OnWindowClosed));
				this.displayedDebugWindow.Close();
				this.displayedDebugWindow = null;
				return;
			}
			this.displayedDebugWindow = UIDebugWindowView.Display(null);
			UIBaseWindowView.CloseAction = (Action<UIBaseWindowView>)Delegate.Combine(UIBaseWindowView.CloseAction, new Action<UIBaseWindowView>(this.OnWindowClosed));
		}

		// Token: 0x06000379 RID: 889 RVA: 0x00012504 File Offset: 0x00010704
		private void OnWindowClosed(UIBaseWindowView closedWindow)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "OnWindowClosed", "closedWindow", closedWindow.GetType()), this);
			}
			if (this.displayedDebugWindow == closedWindow)
			{
				UIBaseWindowView.CloseAction = (Action<UIBaseWindowView>)Delegate.Remove(UIBaseWindowView.CloseAction, new Action<UIBaseWindowView>(this.OnWindowClosed));
				this.displayedDebugWindow = null;
			}
		}

		// Token: 0x04000285 RID: 645
		[SerializeField]
		private UIButton button;

		// Token: 0x04000286 RID: 646
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000287 RID: 647
		private EndlessSharedInputActions endlessSharedInputActions;

		// Token: 0x04000288 RID: 648
		private UIDebugWindowView displayedDebugWindow;

		// Token: 0x04000289 RID: 649
		private bool canOpenDebugWindow;
	}
}
