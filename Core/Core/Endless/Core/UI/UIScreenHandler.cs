using System;
using Endless.Matchmaking;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI
{
	// Token: 0x0200008F RID: 143
	public class UIScreenHandler : MonoBehaviour
	{
		// Token: 0x060002E2 RID: 738 RVA: 0x0000FAE3 File Offset: 0x0000DCE3
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.SubscribeToEvents();
			if (MatchmakingClientController.Instance.IsMissingIdentity)
			{
				this.DisplayUserSignInScreen();
			}
		}

		// Token: 0x060002E3 RID: 739 RVA: 0x0000FB18 File Offset: 0x0000DD18
		private void SubscribeToEvents()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SubscribeToEvents", Array.Empty<object>());
			}
			MatchmakingClientController.OnConnectionToServerFailed += this.DisplayUserSignInScreen;
			MatchmakingClientController.MatchStart += this.CloseActiveScreen;
			MatchmakingClientController.MatchLeft += this.DisplayMainMenu;
			MatchmakingClientController.OnDisconnectedFromServer += this.DisplayUserSignInScreen;
			MatchmakingClientController.OnMissingIdentity += this.DisplayUserSignInScreen;
			MatchSession.OnMatchSessionStop += this.DisplayMainMenu;
			UnityServicesManager.OnServicesInitializedFailure = (Action)Delegate.Combine(UnityServicesManager.OnServicesInitializedFailure, new Action(this.DisplayConnectionFailedScreen));
			MonoBehaviourSingleton<InstallationErrorHandler>.Instance.OnErrorDetected.AddListener(new UnityAction(this.UnsubscribeToEvents));
		}

		// Token: 0x060002E4 RID: 740 RVA: 0x0000FBE0 File Offset: 0x0000DDE0
		private void UnsubscribeToEvents()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "UnsubscribeToEvents", Array.Empty<object>());
			}
			MatchmakingClientController.OnConnectionToServerFailed -= this.DisplayUserSignInScreen;
			MatchmakingClientController.MatchStart -= this.CloseActiveScreen;
			MatchmakingClientController.MatchLeft -= this.DisplayMainMenu;
			MatchmakingClientController.OnDisconnectedFromServer -= this.DisplayUserSignInScreen;
			MatchmakingClientController.OnMissingIdentity -= this.DisplayUserSignInScreen;
			MatchSession.OnMatchSessionStop -= this.DisplayMainMenu;
			UnityServicesManager.OnServicesInitializedFailure = (Action)Delegate.Remove(UnityServicesManager.OnServicesInitializedFailure, new Action(this.DisplayConnectionFailedScreen));
		}

		// Token: 0x060002E5 RID: 741 RVA: 0x0000FC8B File Offset: 0x0000DE8B
		private void CloseActiveScreen()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "CloseActiveScreen", Array.Empty<object>());
			}
			if (!MonoBehaviourSingleton<UIScreenManager>.Instance.IsDisplaying)
			{
				return;
			}
			MonoBehaviourSingleton<UIScreenManager>.Instance.Close(UIScreenManager.CloseStackActions.Clear, null, true, false);
		}

		// Token: 0x060002E6 RID: 742 RVA: 0x0000FCC0 File Offset: 0x0000DEC0
		private void DisplayConnectionFailedScreen()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "DisplayConnectionFailedScreen", Array.Empty<object>());
			}
			MatchmakingClientController.Instance.Disconnect();
			UIConnectionFailedScreenView.Display(UIScreenManager.DisplayStackActions.ClearAndPush);
		}

		// Token: 0x060002E7 RID: 743 RVA: 0x0000FCEB File Offset: 0x0000DEEB
		private void DisplayUserSignInScreen(string error)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log("DisplayUserSignInScreen ( error: " + error + " )", this);
			}
			this.DisplayUserSignInScreen();
		}

		// Token: 0x060002E8 RID: 744 RVA: 0x0000FD11 File Offset: 0x0000DF11
		private void DisplayUserSignInScreen()
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log("DisplayUserSignInScreen", this);
			}
			UISignInScreenView.Display(UIScreenManager.DisplayStackActions.ClearAndPush);
		}

		// Token: 0x060002E9 RID: 745 RVA: 0x0000FD2D File Offset: 0x0000DF2D
		private void DisplayMainMenu(MatchSession matchSession)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "DisplayMainMenu", "matchSession", matchSession), this);
			}
			this.DisplayMainMenu();
		}

		// Token: 0x060002EA RID: 746 RVA: 0x0000FD58 File Offset: 0x0000DF58
		private void DisplayMainMenu()
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log("DisplayMainMenu", this);
			}
			UIMainMenuScreenView.Display(UIScreenManager.DisplayStackActions.ClearAndPush);
		}

		// Token: 0x04000228 RID: 552
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
