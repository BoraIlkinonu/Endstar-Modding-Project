using System;
using System.Collections.Generic;
using Endless.Data;
using Endless.Data.UI;
using Endless.Matchmaking;
using Endless.Networking;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI
{
	// Token: 0x0200008E RID: 142
	public class UIAuthenticationScreenHandler : MonoBehaviour
	{
		// Token: 0x060002D0 RID: 720 RVA: 0x0000F4CB File Offset: 0x0000D6CB
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.SubscribeToEvents();
		}

		// Token: 0x060002D1 RID: 721 RVA: 0x0000F4EC File Offset: 0x0000D6EC
		public void VerifyLauncherEndlessToken(string token)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "VerifyLauncherEndlessToken", new object[] { token });
			}
			MonoBehaviourSingleton<UIScreenCoverTokenHandler>.Instance.Display(UIScreenCoverTokens.WaitingForEndlessCloudServiceVerifyToken, null, true);
			EndlessCloudService.VerifyToken(token, new Action<string>(this.OnLauncherEndlessTokenSuccess), new Action<Exception>(this.OnLauncherEndlessFailed));
		}

		// Token: 0x060002D2 RID: 722 RVA: 0x0000F541 File Offset: 0x0000D741
		public void VerifyMatchmakingToken(string token)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "VerifyMatchmakingToken", new object[] { token });
			}
			if (!MatchmakingClientController.Instance.TrySetUserToken(token, TargetPlatforms.Endless))
			{
				this.DisplayUserSignIn();
			}
		}

		// Token: 0x060002D3 RID: 723 RVA: 0x0000F574 File Offset: 0x0000D774
		private void SubscribeToEvents()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SubscribeToEvents", Array.Empty<object>());
			}
			UnityServicesManager.OnServicesInitializedFailure = (Action)Delegate.Combine(UnityServicesManager.OnServicesInitializedFailure, new Action(this.OnUnityServicesManagerAuthenticationFailed));
			if (MatchmakingClientController.Instance.IsInitialized)
			{
				this.HandleCommandLineArgs();
			}
			else
			{
				MatchmakingClientController.OnInitialized += this.HandleCommandLineArgs;
			}
			MatchmakingClientController.OnAuthenticationProcessSuccessful = (Action<ClientData>)Delegate.Combine(MatchmakingClientController.OnAuthenticationProcessSuccessful, new Action<ClientData>(this.OnAuthenticationProcessSuccessful));
			MatchmakingClientController.OnAuthenticationProcessFailed = (Action<string>)Delegate.Combine(MatchmakingClientController.OnAuthenticationProcessFailed, new Action<string>(this.OnAuthenticationProcessFailed));
			MatchmakingClientController.OnDisconnectedFromServer += this.OnDisconnectedFromServer;
			MonoBehaviourSingleton<InstallationErrorHandler>.Instance.OnErrorDetected.AddListener(new UnityAction(this.UnsubscribeToEvents));
		}

		// Token: 0x060002D4 RID: 724 RVA: 0x0000F64C File Offset: 0x0000D84C
		private void UnsubscribeToEvents()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "UnsubscribeToEvents", Array.Empty<object>());
			}
			UnityServicesManager.OnServicesInitializedFailure = (Action)Delegate.Remove(UnityServicesManager.OnServicesInitializedFailure, new Action(this.OnUnityServicesManagerAuthenticationFailed));
			MatchmakingClientController.OnInitialized -= this.HandleCommandLineArgs;
			MatchmakingClientController.OnAuthenticationProcessSuccessful = (Action<ClientData>)Delegate.Remove(MatchmakingClientController.OnAuthenticationProcessSuccessful, new Action<ClientData>(this.OnAuthenticationProcessSuccessful));
			MatchmakingClientController.OnAuthenticationProcessFailed = (Action<string>)Delegate.Remove(MatchmakingClientController.OnAuthenticationProcessFailed, new Action<string>(this.OnAuthenticationProcessFailed));
			MatchmakingClientController.OnDisconnectedFromServer -= this.OnDisconnectedFromServer;
		}

		// Token: 0x060002D5 RID: 725 RVA: 0x0000F6F3 File Offset: 0x0000D8F3
		private void OnDisconnectedFromServer(string reason)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDisconnectedFromServer", new object[] { reason });
			}
			this.endlessCloudServiceAuthenticated = false;
			this.endlessMatchmakingAuthenticated = false;
		}

		// Token: 0x060002D6 RID: 726 RVA: 0x0000F720 File Offset: 0x0000D920
		private void OnUnityServicesManagerAuthenticationFailed()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnUnityServicesManagerAuthenticationFailed", Array.Empty<object>());
			}
			Exception ex = new Exception("UnityServicesManager failed to authenticate!");
			ErrorHandler.HandleError(ErrorCodes.UIAuthenticationScreenHandler_OnServicesInitializedFailure, ex, true, false);
		}

		// Token: 0x060002D7 RID: 727 RVA: 0x0000F760 File Offset: 0x0000D960
		private void HandleCommandLineArgs()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "HandleCommandLineArgs", Array.Empty<object>());
			}
			MatchmakingClientController.OnInitialized -= this.HandleCommandLineArgs;
			string[] commandLineArgs = Environment.GetCommandLineArgs();
			List<string> list = new List<string>();
			foreach (string text in commandLineArgs)
			{
				list.AddRange(text.Split(" ", StringSplitOptions.None));
			}
			if (this.superVerboseLogging)
			{
				Debug.Log(string.Format("There are {0} {1}", list.Count, "commandLineArguments"));
			}
			string text2 = null;
			for (int j = 0; j < list.Count; j++)
			{
				string text3 = list[j];
				if (this.superVerboseLogging)
				{
					Debug.Log(string.Format("{0} [{1}]: {2}", "commandLineArguments", j, text3));
				}
				if (text3.StartsWith(this.endlessAccessTokenKey))
				{
					text2 = text3.Replace(this.endlessAccessTokenKey, string.Empty);
					if (this.verboseLogging)
					{
						Debug.Log("endlessAccessTokenValue: " + text2);
					}
					this.VerifyLauncherEndlessToken(text2);
				}
			}
			if (text2.IsNullOrEmptyOrWhiteSpace())
			{
				this.VerifyPlayerPrefsEndlessToken();
			}
		}

		// Token: 0x060002D8 RID: 728 RVA: 0x0000F884 File Offset: 0x0000DA84
		private void OnLauncherEndlessTokenSuccess(string endlessAccessTokenValue)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnLauncherEndlessTokenSuccess", new object[] { endlessAccessTokenValue });
			}
			MonoBehaviourSingleton<UIScreenCoverTokenHandler>.Instance.ReleaseToken(UIScreenCoverTokens.WaitingForEndlessCloudServiceVerifyToken);
			this.endlessCloudServiceAuthenticated = true;
			if (this.endlessMatchmakingAuthenticated)
			{
				this.DisplayMainMenu();
				return;
			}
			this.VerifyMatchmakingToken(endlessAccessTokenValue);
		}

		// Token: 0x060002D9 RID: 729 RVA: 0x0000F8D6 File Offset: 0x0000DAD6
		private void OnLauncherEndlessFailed(Exception exception)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnLauncherEndlessFailed", new object[] { exception.Message });
			}
			Debug.LogException(exception);
			MonoBehaviourSingleton<UIScreenCoverTokenHandler>.Instance.ReleaseToken(UIScreenCoverTokens.WaitingForEndlessCloudServiceVerifyToken);
			this.VerifyPlayerPrefsEndlessToken();
		}

		// Token: 0x060002DA RID: 730 RVA: 0x0000F914 File Offset: 0x0000DB14
		private void VerifyPlayerPrefsEndlessToken()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "VerifyPlayerPrefsEndlessToken", Array.Empty<object>());
			}
			MonoBehaviourSingleton<UIScreenCoverTokenHandler>.Instance.Display(UIScreenCoverTokens.WaitingForEndlessCloudServiceVerifyPlayerPrefsToken, null, true);
			EndlessCloudService.VerifyPlayerPrefsToken(new Action<string>(this.OnVerifyPlayerPrefsTokenSuccess), new Action<Exception>(this.OnVerifyPlayerPrefsTokenFailed));
		}

		// Token: 0x060002DB RID: 731 RVA: 0x0000F964 File Offset: 0x0000DB64
		private void OnVerifyPlayerPrefsTokenSuccess(string authToken)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnVerifyPlayerPrefsTokenSuccess", new object[] { authToken });
			}
			MonoBehaviourSingleton<UIScreenCoverTokenHandler>.Instance.ReleaseToken(UIScreenCoverTokens.WaitingForEndlessCloudServiceVerifyPlayerPrefsToken);
			this.endlessCloudServiceAuthenticated = true;
			if (this.endlessMatchmakingAuthenticated)
			{
				this.DisplayMainMenu();
				return;
			}
			this.VerifyMatchmakingToken(authToken);
		}

		// Token: 0x060002DC RID: 732 RVA: 0x0000F9B6 File Offset: 0x0000DBB6
		private void OnVerifyPlayerPrefsTokenFailed(Exception exception)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnVerifyPlayerPrefsTokenFailed", new object[] { exception.Message });
			}
			MonoBehaviourSingleton<UIScreenCoverTokenHandler>.Instance.ReleaseToken(UIScreenCoverTokens.WaitingForEndlessCloudServiceVerifyPlayerPrefsToken);
			this.DisplayUserSignIn();
		}

		// Token: 0x060002DD RID: 733 RVA: 0x0000F9EB File Offset: 0x0000DBEB
		private void OnAuthenticationProcessSuccessful(ClientData clientData)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnAuthenticationProcessSuccessful", new object[] { clientData.ToPrettyString() });
			}
			this.endlessMatchmakingAuthenticated = true;
			this.DisplayMainMenu();
		}

		// Token: 0x060002DE RID: 734 RVA: 0x0000FA1C File Offset: 0x0000DC1C
		private void OnAuthenticationProcessFailed(string error)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnAuthenticationProcessFailed", new object[] { error });
			}
			EndlessCloudService.ClearCachedToken();
			this.DisplayUserSignIn();
		}

		// Token: 0x060002DF RID: 735 RVA: 0x0000FA46 File Offset: 0x0000DC46
		private void DisplayUserSignIn()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "DisplayUserSignIn", Array.Empty<object>());
			}
			UISignInScreenView.Display(UIScreenManager.DisplayStackActions.ClearAndPush);
		}

		// Token: 0x060002E0 RID: 736 RVA: 0x0000FA68 File Offset: 0x0000DC68
		private void DisplayMainMenu()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "DisplayMainMenu", Array.Empty<object>());
			}
			if (!this.endlessCloudServiceAuthenticated)
			{
				DebugUtility.LogMethodWithAppension(this, "DisplayMainMenu", "endlessCloudServiceAuthenticated is false!", Array.Empty<object>());
				return;
			}
			if (!this.endlessMatchmakingAuthenticated)
			{
				DebugUtility.LogMethodWithAppension(this, "DisplayMainMenu", "endlessCloudServiceAuthenticated is false!", Array.Empty<object>());
				return;
			}
			UIMainMenuScreenView.Display(UIScreenManager.DisplayStackActions.ClearAndPush);
		}

		// Token: 0x04000223 RID: 547
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000224 RID: 548
		[SerializeField]
		private bool superVerboseLogging;

		// Token: 0x04000225 RID: 549
		private bool endlessCloudServiceAuthenticated;

		// Token: 0x04000226 RID: 550
		private bool endlessMatchmakingAuthenticated;

		// Token: 0x04000227 RID: 551
		private readonly string endlessAccessTokenKey = "endlessAccessToken=";
	}
}
