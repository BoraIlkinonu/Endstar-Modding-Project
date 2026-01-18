using System;
using Endless.Data;
using Endless.Matchmaking;
using Endless.Shared;
using Endless.Shared.UI;
using Runtime.Shared.Matchmaking;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Endless.Creator.UI
{
	// Token: 0x020000B3 RID: 179
	public class UIAdminWindowDisplayHandler : UIGameObject
	{
		// Token: 0x060002C7 RID: 711 RVA: 0x0001275C File Offset: 0x0001095C
		private void OnEnable()
		{
			if (this.verboseLogging)
			{
				Debug.Log("OnEnable", this);
			}
			if (this.playerInputActions == null)
			{
				this.playerInputActions = new PlayerInputActions();
			}
			if (MatchmakingClientController.Instance.LocalMatch == null)
			{
				return;
			}
			MatchData matchData = MatchmakingClientController.Instance.LocalMatch.GetMatchData();
			this.isAdmin = matchData.IsAdminSession();
			this.playerInputActions.Player.DisplayGameLibraryModerationWindow.performed += this.DisplayGameLibraryModerationWindow;
			this.playerInputActions.Player.DisplayGameLibraryModerationWindow.Enable();
		}

		// Token: 0x060002C8 RID: 712 RVA: 0x000127F4 File Offset: 0x000109F4
		private void OnDisable()
		{
			if (this.verboseLogging)
			{
				Debug.Log("OnDisable", this);
			}
			this.isAdmin = false;
			this.playerInputActions.Player.DisplayGameLibraryModerationWindow.performed -= this.DisplayGameLibraryModerationWindow;
			this.playerInputActions.Player.DisplayGameLibraryModerationWindow.Disable();
		}

		// Token: 0x060002C9 RID: 713 RVA: 0x00012857 File Offset: 0x00010A57
		private void DisplayGameLibraryModerationWindow(InputAction.CallbackContext callbackContext)
		{
			if (this.verboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1} : {2} )", "DisplayGameLibraryModerationWindow", "callbackContext", callbackContext), this);
			}
			if (this.isAdmin)
			{
				this.DisplayGameLibraryModerationWindow();
			}
		}

		// Token: 0x060002CA RID: 714 RVA: 0x00012890 File Offset: 0x00010A90
		private void DisplayGameLibraryModerationWindow()
		{
			if (this.verboseLogging)
			{
				Debug.Log("DisplayGameLibraryModerationWindow", this);
			}
			if (MonoBehaviourSingleton<UIWindowManager>.Instance.IsDisplaying && MonoBehaviourSingleton<UIWindowManager>.Instance.Displayed.GetType() == typeof(UIGameLibraryModerationWindowView))
			{
				return;
			}
			UIGameLibraryModerationWindowView.Display(null);
		}

		// Token: 0x040002FE RID: 766
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x040002FF RID: 767
		private PlayerInputActions playerInputActions;

		// Token: 0x04000300 RID: 768
		private bool isAdmin;
	}
}
