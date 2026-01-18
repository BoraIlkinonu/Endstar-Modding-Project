using System;
using Endless.Data;
using Endless.Data.UI;
using Endless.GraphQl;
using Endless.Matchmaking;
using Endless.Networking;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Endless.Shared.Validation;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI
{
	// Token: 0x02000099 RID: 153
	public class UISignInScreenController : UIGameObject, IUILoadingSpinnerViewCompatible, IValidatable
	{
		// Token: 0x17000053 RID: 83
		// (get) Token: 0x06000330 RID: 816 RVA: 0x00011082 File Offset: 0x0000F282
		public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

		// Token: 0x17000054 RID: 84
		// (get) Token: 0x06000331 RID: 817 RVA: 0x0001108A File Offset: 0x0000F28A
		public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

		// Token: 0x06000332 RID: 818 RVA: 0x00011092 File Offset: 0x0000F292
		private void OnEnable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnable", Array.Empty<object>());
			}
			MatchmakingClientController.OnConnectionToServerFailed += this.OnConnectionToMatchmakingServerFailed;
		}

		// Token: 0x06000333 RID: 819 RVA: 0x000110C0 File Offset: 0x0000F2C0
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.passwordInputField.onSubmit.AddListener(new UnityAction<string>(this.SignIn));
			this.signInButton.onClick.AddListener(new UnityAction(this.SignIn));
			this.quitButton.onClick.AddListener(new UnityAction(this.ConfirmApplicationQuit));
			this.view.InitializedUnityEvent.AddListener(new UnityAction(this.OnLoadingEnded.Invoke));
		}

		// Token: 0x06000334 RID: 820 RVA: 0x0001115A File Offset: 0x0000F35A
		private void OnDisable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDisable", Array.Empty<object>());
			}
			MatchmakingClientController.OnConnectionToServerFailed -= this.OnConnectionToMatchmakingServerFailed;
		}

		// Token: 0x06000335 RID: 821 RVA: 0x00011185 File Offset: 0x0000F385
		[ContextMenu("Validate")]
		public void Validate()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Validate", Array.Empty<object>());
			}
			DebugUtility.DebugIsNull("view", this.view, this);
		}

		// Token: 0x06000336 RID: 822 RVA: 0x000111B1 File Offset: 0x0000F3B1
		private void SignIn(string password)
		{
			if (this.verboseLogging)
			{
				Debug.Log("SignIn");
			}
			this.SignIn();
		}

		// Token: 0x06000337 RID: 823 RVA: 0x000111CC File Offset: 0x0000F3CC
		private async void SignIn()
		{
			if (this.verboseLogging)
			{
				Debug.Log("SignIn");
			}
			if (!this.usernameInputField.IsNullOrEmptyOrWhiteSpace(true))
			{
				if (!this.passwordInputField.IsNullOrEmptyOrWhiteSpace(true))
				{
					string text = this.usernameInputField.text.Replace(" ", string.Empty);
					string text2 = this.passwordInputField.text.Replace(" ", string.Empty);
					this.OnLoadingStarted.Invoke();
					MonoBehaviourSingleton<UIScreenCoverTokenHandler>.Instance.Display(UIScreenCoverTokens.WaitingForEndlessCloudServiceSignIn, null, true);
					await EndlessCloudService.SignIn(text, text2, this.rememberMeToggle.IsOn, new Action<string>(this.OnSignInToEndlessCloudServiceSuccess), new Action<GraphQlResult>(this.OnSignInToEndlessCloudServiceFailed));
				}
			}
		}

		// Token: 0x06000338 RID: 824 RVA: 0x00011204 File Offset: 0x0000F404
		private void OnSignInToEndlessCloudServiceSuccess(string authToken)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnSignInToEndlessCloudServiceSuccess", new object[] { authToken });
			}
			MonoBehaviourSingleton<UIScreenCoverTokenHandler>.Instance.ReleaseToken(UIScreenCoverTokens.WaitingForEndlessCloudServiceSignIn);
			if (MatchmakingClientController.Instance.TrySetUserToken(authToken, TargetPlatforms.Endless))
			{
				MatchmakingClientController.OnAuthenticationProcessSuccessful = (Action<ClientData>)Delegate.Combine(MatchmakingClientController.OnAuthenticationProcessSuccessful, new Action<ClientData>(this.OnMatchmakingAuthenticationSuccess));
				MatchmakingClientController.OnAuthenticationProcessFailed = (Action<string>)Delegate.Combine(MatchmakingClientController.OnAuthenticationProcessFailed, new Action<string>(this.OnMatchmakingAuthenticationFailed));
				return;
			}
			this.OnLoadingEnded.Invoke();
			ErrorHandler.HandleError(ErrorCodes.UISignInScreenController_SetUserToken, new Exception("Error during TrySetUserToken!"), true, false);
		}

		// Token: 0x06000339 RID: 825 RVA: 0x000112A9 File Offset: 0x0000F4A9
		private void OnConnectionToMatchmakingServerFailed(string error)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnConnectionToMatchmakingServerFailed", new object[] { error });
			}
			this.OnLoadingEnded.Invoke();
		}

		// Token: 0x0600033A RID: 826 RVA: 0x000112D4 File Offset: 0x0000F4D4
		private void OnMatchmakingAuthenticationSuccess(ClientData clientData)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnMatchmakingAuthenticationSuccess", new object[] { clientData.ToPrettyString() });
			}
			this.OnLoadingEnded.Invoke();
			MatchmakingClientController.OnAuthenticationProcessSuccessful = (Action<ClientData>)Delegate.Remove(MatchmakingClientController.OnAuthenticationProcessSuccessful, new Action<ClientData>(this.OnMatchmakingAuthenticationSuccess));
			MatchmakingClientController.OnAuthenticationProcessFailed = (Action<string>)Delegate.Remove(MatchmakingClientController.OnAuthenticationProcessFailed, new Action<string>(this.OnMatchmakingAuthenticationFailed));
			UIMainMenuScreenView.Display(UIScreenManager.DisplayStackActions.ClearAndPush);
		}

		// Token: 0x0600033B RID: 827 RVA: 0x00011358 File Offset: 0x0000F558
		private void OnMatchmakingAuthenticationFailed(string error)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnMatchmakingAuthenticationFailed", new object[] { error });
			}
			this.OnLoadingEnded.Invoke();
			MatchmakingClientController.OnAuthenticationProcessSuccessful = (Action<ClientData>)Delegate.Remove(MatchmakingClientController.OnAuthenticationProcessSuccessful, new Action<ClientData>(this.OnMatchmakingAuthenticationSuccess));
			MatchmakingClientController.OnAuthenticationProcessFailed = (Action<string>)Delegate.Remove(MatchmakingClientController.OnAuthenticationProcessFailed, new Action<string>(this.OnMatchmakingAuthenticationFailed));
			ErrorHandler.HandleError(ErrorCodes.UISignInScreenController_Authentication, new Exception(error), true, false);
		}

		// Token: 0x0600033C RID: 828 RVA: 0x000113E0 File Offset: 0x0000F5E0
		private void OnSignInToEndlessCloudServiceFailed(GraphQlResult result)
		{
			Exception errorMessage = result.GetErrorMessage(0);
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnSignInToEndlessCloudServiceFailed", new object[] { errorMessage.Message });
			}
			this.OnLoadingEnded.Invoke();
			MonoBehaviourSingleton<UIScreenCoverTokenHandler>.Instance.ReleaseToken(UIScreenCoverTokens.WaitingForEndlessCloudServiceSignIn);
			string errorCode = result.GetErrorCode(0);
			if (errorCode == "USER_NOT_FOUND" || errorCode == "INVALID_CREDENTIALS")
			{
				this.usernameInputField.PlayInvalidInputTweens();
				this.passwordInputField.PlayInvalidInputTweens();
				ErrorHandler.HandleError(ErrorCodes.UISignInScreenController_SignIn_InvalidUsernameOrPassword, errorMessage, true, false);
				return;
			}
			if (errorCode == "PASSWORD_NOT_SET")
			{
				this.passwordInputField.PlayInvalidInputTweens();
				ErrorHandler.HandleError(ErrorCodes.UISignInScreenController_SignIn_PasswordNeedsSet, errorMessage, true, false);
				return;
			}
			if (errorMessage is TimeoutException)
			{
				ErrorHandler.HandleError(ErrorCodes.UISignInScreenController_Timeout, errorMessage, true, false);
				return;
			}
			ErrorHandler.HandleError(ErrorCodes.UISignInScreenController_SignIn_Unknown_ErrorCode, errorMessage, true, false);
		}

		// Token: 0x0600033D RID: 829 RVA: 0x000114C0 File Offset: 0x0000F6C0
		private void ConfirmApplicationQuit()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ConfirmApplicationQuit", Array.Empty<object>());
			}
			MonoBehaviourSingleton<UIModalManager>.Instance.Confirm("Are you sure you want to quit?", new Action(Application.Quit), new Action(MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack), UIModalManagerStackActions.ClearStack);
		}

		// Token: 0x04000255 RID: 597
		[SerializeField]
		private UISignInScreenView view;

		// Token: 0x04000256 RID: 598
		[SerializeField]
		private UIInputField usernameInputField;

		// Token: 0x04000257 RID: 599
		[SerializeField]
		private UIInputField passwordInputField;

		// Token: 0x04000258 RID: 600
		[SerializeField]
		private UIToggle rememberMeToggle;

		// Token: 0x04000259 RID: 601
		[SerializeField]
		private UIButton signInButton;

		// Token: 0x0400025A RID: 602
		[SerializeField]
		private UIButton quitButton;

		// Token: 0x0400025B RID: 603
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
