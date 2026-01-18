using System;
using Endless.Data;
using Endless.Data.UI;
using Endless.Gameplay.UI;
using Endless.Matchmaking;
using Endless.Networking;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI
{
	// Token: 0x02000076 RID: 118
	[DefaultExecutionOrder(2147483647)]
	public class UIScreenCoverHandler : UIMonoBehaviourSingleton<UIScreenCoverHandler>
	{
		// Token: 0x06000228 RID: 552 RVA: 0x0000C304 File Offset: 0x0000A504
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.DisplayMatchmakingClientControllerInitializationScreenCover();
			this.SubscribeSafely(delegate
			{
				MatchmakingClientController.OnInitialized += this.CloseMatchmakingClientControllerInitializationScreenCover;
			}, "OnInitialized");
			if (MatchmakingClientController.Instance.IsInitialized)
			{
				this.CloseMatchmakingClientControllerInitializationScreenCover();
			}
			this.SubscribeSafely(delegate
			{
				BuildUtilities.OnFailure += this.CloseMatchmakingClientControllerInitializationScreenCover;
			}, "OnFailure");
			this.SubscribeSafely(delegate
			{
				MatchmakingClientController.OnStartedConnectionToServer += this.DisplayWaitingForMatchmakingConnectionScreenCover;
			}, "OnStartedConnectionToServer");
			this.SubscribeSafely(delegate
			{
				MatchmakingClientController.OnConnectedToServer += this.CloseWaitingForMatchmakingConnectionScreenCover;
			}, "OnConnectedToServer");
			this.SubscribeSafely(delegate
			{
				MatchmakingClientController.OnConnectionToServerFailed += this.CloseWaitingForMatchmakingConnectionScreenCoverAndHandleError;
			}, "OnConnectionToServerFailed");
			this.SubscribeSafely(delegate
			{
				MatchmakingClientController.OnDisconnectedFromServer += this.OnDisconnectedFromServer;
			}, "OnDisconnectedFromServer");
			this.SubscribeSafely(delegate
			{
				MatchmakingClientController.OnAuthenticationProcessStarted += this.DisplayWaitingForMatchmakingAuthenticationScreenCover;
			}, "OnAuthenticationProcessStarted");
			this.SubscribeSafely(delegate
			{
				MatchmakingClientController.OnAuthenticationProcessSuccessful = (Action<ClientData>)Delegate.Combine(MatchmakingClientController.OnAuthenticationProcessSuccessful, new Action<ClientData>(this.CloseWaitingForMatchmakingAuthenticationScreenCover));
			}, "OnAuthenticationProcessSuccessful");
			this.SubscribeSafely(delegate
			{
				MatchmakingClientController.OnAuthenticationProcessFailed = (Action<string>)Delegate.Combine(MatchmakingClientController.OnAuthenticationProcessFailed, new Action<string>(this.CloseWaitingForMatchmakingAuthenticationScreenCoverAndHandleError));
			}, "OnAuthenticationProcessFailed");
			this.SubscribeSafely(delegate
			{
				MatchmakingClientController.MatchStart += this.DisplayWaitingForMatchAllocationScreenCover;
			}, "MatchStart");
			this.SubscribeSafely(delegate
			{
				MatchmakingClientController.MatchAllocated += this.OnMatchAllocated;
			}, "MatchAllocated");
			this.SubscribeSafely(delegate
			{
				MatchmakingClientController.MatchAllocationError += this.OnMatchAllocationError;
			}, "MatchAllocationError");
			this.SubscribeSafely(delegate
			{
				MatchmakingClientController.MatchHostChanged += this.DisplayMatchHostMigrationScreenCover;
			}, "MatchHostChanged");
			this.SubscribeSafely(delegate
			{
				MatchmakingClientController.MatchLeft += this.CloseAllMatchRelatedScreenCovers;
			}, "MatchLeft");
			bool flag = false;
			if (NetworkBehaviourSingleton<GameStateManager>.Instance)
			{
				this.SubscribeSafely(delegate
				{
					NetworkBehaviourSingleton<GameStateManager>.Instance.OnGameStateChanged.AddListener(new UnityAction<GameState, GameState>(this.OnGameStateChanged));
				}, "OnGameStateChanged");
			}
			else
			{
				NullReferenceException ex = new NullReferenceException("GameStateManager.Instance is null!");
				ErrorHandler.HandleError(ErrorCodes.UIScreenCoverHandler_Start_NullGameStateManager, ex, true, false);
				flag = true;
			}
			if (MonoBehaviourSingleton<UIStartMatchHelper>.Instance)
			{
				this.SubscribeSafely(delegate
				{
					MonoBehaviourSingleton<UIStartMatchHelper>.Instance.StartMatchRequestedUnityEvent.AddListener(new UnityAction(this.DisplayWaitingForMatchStartedScreenCover));
				}, "StartMatchRequestedUnityEvent");
				this.SubscribeSafely(delegate
				{
					MonoBehaviourSingleton<UIStartMatchHelper>.Instance.StartMatchCompleteUnityEvent.AddListener(new UnityAction<bool>(this.CloseWaitingForMatchStartedScreenCover));
				}, "StartMatchCompleteUnityEvent");
				return;
			}
			NullReferenceException ex2 = new NullReferenceException("UIStartMatchHelper.Instance is null!");
			ErrorHandler.HandleError(ErrorCodes.UIScreenCoverHandler_Start_NullStartMatchHelper, ex2, !flag, false);
		}

		// Token: 0x06000229 RID: 553 RVA: 0x0000C514 File Offset: 0x0000A714
		public void OnEndMatchError(int errorCode, string errorMessage)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEndMatchError", new object[] { errorCode, errorMessage });
			}
			ErrorHandler.HandleError(ErrorCodes.UIMatchSectionController_EndMatch, new Exception(errorMessage), true, true);
		}

		// Token: 0x0600022A RID: 554 RVA: 0x0000C54E File Offset: 0x0000A74E
		private void DisplayMatchmakingClientControllerInitializationScreenCover()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "DisplayMatchmakingClientControllerInitializationScreenCover", Array.Empty<object>());
			}
			MonoBehaviourSingleton<UIScreenCoverTokenHandler>.Instance.Display(UIScreenCoverTokens.MatchmakingClientControllerInitialization, null, false);
		}

		// Token: 0x0600022B RID: 555 RVA: 0x0000C575 File Offset: 0x0000A775
		private void CloseMatchmakingClientControllerInitializationScreenCover()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "CloseMatchmakingClientControllerInitializationScreenCover", Array.Empty<object>());
			}
			MonoBehaviourSingleton<UIScreenCoverTokenHandler>.Instance.ReleaseToken(UIScreenCoverTokens.MatchmakingClientControllerInitialization);
		}

		// Token: 0x0600022C RID: 556 RVA: 0x0000C59A File Offset: 0x0000A79A
		private void DisplayWaitingForMatchmakingConnectionScreenCover()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "DisplayWaitingForMatchmakingConnectionScreenCover", Array.Empty<object>());
			}
			MonoBehaviourSingleton<UIScreenCoverTokenHandler>.Instance.Display(UIScreenCoverTokens.WaitingForMatchmakingConnection, null, true);
		}

		// Token: 0x0600022D RID: 557 RVA: 0x0000C5C1 File Offset: 0x0000A7C1
		private void CloseWaitingForMatchmakingConnectionScreenCover()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "CloseWaitingForMatchmakingConnectionScreenCover", Array.Empty<object>());
			}
			MonoBehaviourSingleton<UIScreenCoverTokenHandler>.Instance.ReleaseToken(UIScreenCoverTokens.WaitingForMatchmakingConnection);
		}

		// Token: 0x0600022E RID: 558 RVA: 0x0000C5E6 File Offset: 0x0000A7E6
		private void CloseWaitingForMatchmakingConnectionScreenCoverAndHandleError(string error)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "CloseWaitingForMatchmakingConnectionScreenCoverAndHandleError", new object[] { error });
			}
			ErrorHandler.HandleError(ErrorCodes.UIScreenCoverHandler_ConnectionToServerFailed, new Exception(error), true, false);
			MonoBehaviourSingleton<UIScreenCoverTokenHandler>.Instance.ReleaseToken(UIScreenCoverTokens.WaitingForMatchmakingConnection);
		}

		// Token: 0x0600022F RID: 559 RVA: 0x0000C622 File Offset: 0x0000A822
		private void OnDisconnectedFromServer(string reason)
		{
			if (ExitManager.IsQuitting)
			{
				return;
			}
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDisconnectedFromServer", new object[] { reason });
			}
			MonoBehaviourSingleton<UIScreenCoverTokenHandler>.Instance.ForceClose();
			this.DisplayMatchmakingClientControllerInitializationScreenCover();
		}

		// Token: 0x06000230 RID: 560 RVA: 0x0000C659 File Offset: 0x0000A859
		private void DisplayWaitingForMatchmakingAuthenticationScreenCover()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "DisplayWaitingForMatchmakingAuthenticationScreenCover", Array.Empty<object>());
			}
			MonoBehaviourSingleton<UIScreenCoverTokenHandler>.Instance.Display(UIScreenCoverTokens.WaitingForMatchmakingAuthentication, null, true);
		}

		// Token: 0x06000231 RID: 561 RVA: 0x0000C680 File Offset: 0x0000A880
		private void CloseWaitingForMatchmakingAuthenticationScreenCover(ClientData clientData)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "CloseWaitingForMatchmakingAuthenticationScreenCover", new object[] { clientData.ToPrettyString() });
			}
			MonoBehaviourSingleton<UIScreenCoverTokenHandler>.Instance.ReleaseToken(UIScreenCoverTokens.WaitingForMatchmakingAuthentication);
		}

		// Token: 0x06000232 RID: 562 RVA: 0x0000C6AF File Offset: 0x0000A8AF
		private void CloseWaitingForMatchmakingAuthenticationScreenCoverAndHandleError(string error)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log("CloseWaitingForMatchmakingAuthenticationScreenCoverAndHandleError, error: " + error, this);
			}
			MonoBehaviourSingleton<UIScreenCoverTokenHandler>.Instance.ReleaseToken(UIScreenCoverTokens.WaitingForMatchmakingAuthentication);
			ErrorHandler.HandleError(ErrorCodes.UIScreenCoverHandler_AuthenticationProcessFailed, new Exception(error), true, false);
		}

		// Token: 0x06000233 RID: 563 RVA: 0x0000C6E7 File Offset: 0x0000A8E7
		private void DisplayWaitingForMatchAllocationScreenCover()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "DisplayWaitingForMatchAllocationScreenCover", Array.Empty<object>());
			}
			MonoBehaviourSingleton<UIScreenCoverTokenHandler>.Instance.Display(UIScreenCoverTokens.WaitingForMatchAllocation, null, false);
		}

		// Token: 0x06000234 RID: 564 RVA: 0x0000C710 File Offset: 0x0000A910
		private void CloseWaitingForMatchAllocationScreenCover()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethodWithAppension(this, "CloseWaitingForMatchAllocationScreenCover", string.Format("{0}.{1}: {2}", "GameStateManager", "CurrentState", NetworkBehaviourSingleton<GameStateManager>.Instance.CurrentState), Array.Empty<object>());
			}
			MonoBehaviourSingleton<UIScreenCoverTokenHandler>.Instance.ReleaseToken(UIScreenCoverTokens.WaitingForMatchAllocation);
		}

		// Token: 0x06000235 RID: 565 RVA: 0x0000C764 File Offset: 0x0000A964
		private void OnMatchAllocated()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnMatchAllocated", Array.Empty<object>());
			}
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			if (MonoBehaviourSingleton<UIScreenCoverTokenHandler>.Instance)
			{
				flag2 = MonoBehaviourSingleton<UIScreenCoverTokenHandler>.Instance.Contains(UIScreenCoverTokens.MatchHostMigration);
				flag3 = MonoBehaviourSingleton<UIScreenCoverTokenHandler>.Instance.Contains(UIScreenCoverTokens.WaitingForGameStateOtherThanNoneOrDefault);
			}
			else
			{
				NullReferenceException ex = new NullReferenceException("UIScreenCoverTokenHandler.Instance is null!");
				ErrorHandler.HandleError(ErrorCodes.UIScreenCoverHandler_OnMatchAllocated_NullScreenCoverTokenHandler, ex, false, false);
				flag = true;
			}
			bool flag4 = true;
			if (NetworkBehaviourSingleton<GameStateManager>.Instance)
			{
				GameState currentState = NetworkBehaviourSingleton<GameStateManager>.Instance.CurrentState;
				flag4 = currentState == GameState.None || currentState <= GameState.Default;
			}
			else
			{
				NullReferenceException ex2 = new NullReferenceException("GameStateManager.Instance is null!");
				ErrorHandler.HandleError(ErrorCodes.UIScreenCoverHandler_OnMatchAllocated_NullGameStateManager, ex2, false, false);
				flag = true;
			}
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0}: {1}", "matchHostMigrationInProgress", flag2), this);
				DebugUtility.Log(string.Format("{0}: {1}", "currentGameStateIsNoneOrDefault", flag4), this);
				DebugUtility.Log(string.Format("{0}: {1}", "isWaitingForGameStateOtherThanNoneOrDefault", flag3), this);
				DebugUtility.Log(string.Format("{0}: {1}", "errorOccured", flag), this);
			}
			if (flag)
			{
				this.CloseMatchHostMigrationScreenCover();
				this.CloseWaitingForMatchAllocationScreenCover();
				return;
			}
			if (flag2)
			{
				this.CloseMatchHostMigrationScreenCover();
				return;
			}
			this.CloseWaitingForMatchAllocationScreenCover();
			if (flag4 && !flag3)
			{
				this.DisplayWaitingForGameStateOtherThanNoneOrDefaultScreenCover();
			}
		}

		// Token: 0x06000236 RID: 566 RVA: 0x0000C8B8 File Offset: 0x0000AAB8
		private void OnMatchAllocationError(int errorCode, string error)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnMatchAllocationError", new object[] { errorCode, error });
			}
			MonoBehaviourSingleton<UIScreenCoverTokenHandler>.Instance.ReleaseToken(UIScreenCoverTokens.WaitingForMatchAllocation);
			ErrorHandler.HandleError(ErrorCodes.UIScreenCoverHandler_MatchAllocationError, new Exception(error), true, false);
		}

		// Token: 0x06000237 RID: 567 RVA: 0x0000C908 File Offset: 0x0000AB08
		private void DisplayMatchHostMigrationScreenCover(string groupId)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "DisplayMatchHostMigrationScreenCover", new object[] { groupId });
			}
			MonoBehaviourSingleton<UIScreenCoverTokenHandler>.Instance.Display(UIScreenCoverTokens.MatchHostMigration, null, false);
		}

		// Token: 0x06000238 RID: 568 RVA: 0x0000C935 File Offset: 0x0000AB35
		private void CloseMatchHostMigrationScreenCover()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "CloseMatchHostMigrationScreenCover", Array.Empty<object>());
			}
			MonoBehaviourSingleton<UIScreenCoverTokenHandler>.Instance.ReleaseToken(UIScreenCoverTokens.MatchHostMigration);
		}

		// Token: 0x06000239 RID: 569 RVA: 0x0000C95C File Offset: 0x0000AB5C
		private void DisplayWaitingForGameStateOtherThanNoneOrDefaultScreenCover()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethodWithAppension(this, "DisplayWaitingForGameStateOtherThanNoneOrDefaultScreenCover", string.Format("{0}.{1}: {2}", "GameStateManager", "CurrentState", NetworkBehaviourSingleton<GameStateManager>.Instance.CurrentState), Array.Empty<object>());
			}
			MonoBehaviourSingleton<UIScreenCoverTokenHandler>.Instance.Display(UIScreenCoverTokens.WaitingForGameStateOtherThanNoneOrDefault, null, true);
		}

		// Token: 0x0600023A RID: 570 RVA: 0x0000C9B1 File Offset: 0x0000ABB1
		private void CloseWaitingForGameStateOtherThanNoneOrDefaultScreenCover()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "CloseWaitingForGameStateOtherThanNoneOrDefaultScreenCover", Array.Empty<object>());
			}
			MonoBehaviourSingleton<UIScreenCoverTokenHandler>.Instance.ReleaseToken(UIScreenCoverTokens.WaitingForGameStateOtherThanNoneOrDefault);
		}

		// Token: 0x0600023B RID: 571 RVA: 0x0000C9D8 File Offset: 0x0000ABD8
		private void CloseAllMatchRelatedScreenCovers()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "CloseAllMatchRelatedScreenCovers", Array.Empty<object>());
			}
			foreach (UIScreenCoverTokens uiscreenCoverTokens in this.screenCoverTokensToCancelIfMatchCloses)
			{
				if (MonoBehaviourSingleton<UIScreenCoverTokenHandler>.Instance.Contains(uiscreenCoverTokens))
				{
					MonoBehaviourSingleton<UIScreenCoverTokenHandler>.Instance.ReleaseToken(uiscreenCoverTokens);
				}
			}
		}

		// Token: 0x0600023C RID: 572 RVA: 0x0000CA30 File Offset: 0x0000AC30
		private void OnGameStateChanged(GameState previousGameState, GameState newGameState)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnGameStateChanged", new object[] { previousGameState, newGameState });
			}
			bool flag = previousGameState == GameState.None || previousGameState == GameState.Default;
			bool flag2 = newGameState == GameState.None || newGameState == GameState.Default;
			bool flag3 = MonoBehaviourSingleton<UIScreenCoverTokenHandler>.Instance.Contains(UIScreenCoverTokens.MatchHostMigration);
			bool flag4 = MonoBehaviourSingleton<UIScreenCoverTokenHandler>.Instance.Contains(UIScreenCoverTokens.WaitingForGameStateOtherThanNoneOrDefault);
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0}: {1}", "previousGameStateIsNoneOrDefault", flag), this);
				DebugUtility.Log(string.Format("{0}: {1}", "newGameStateIsNoneOrDefault", flag2), this);
				DebugUtility.Log(string.Format("{0}: {1}", "matchHostMigrationInProgress", flag3), this);
				DebugUtility.Log(string.Format("{0}: {1}", "isWaitingForGameStateOtherThanNoneOrDefault", flag4), this);
			}
			if (!flag2 && flag4)
			{
				this.CloseWaitingForGameStateOtherThanNoneOrDefaultScreenCover();
			}
			if (flag2 && flag3)
			{
				this.DisplayWaitingForGameStateOtherThanNoneOrDefaultScreenCover();
			}
		}

		// Token: 0x0600023D RID: 573 RVA: 0x0000CB26 File Offset: 0x0000AD26
		private void DisplayWaitingForMatchStartedScreenCover()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "DisplayWaitingForMatchStartedScreenCover", Array.Empty<object>());
			}
			MonoBehaviourSingleton<UIScreenCoverTokenHandler>.Instance.Display(UIScreenCoverTokens.WaitingForMatchStarted, null, true);
		}

		// Token: 0x0600023E RID: 574 RVA: 0x0000CB50 File Offset: 0x0000AD50
		private void CloseWaitingForMatchStartedScreenCover(bool matchStartWasSuccessful)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethodWithAppension(this, "CloseWaitingForMatchStartedScreenCover", string.Format("{0}.{1}: {2}", "GameStateManager", "CurrentState", NetworkBehaviourSingleton<GameStateManager>.Instance.CurrentState), new object[] { matchStartWasSuccessful });
			}
			MonoBehaviourSingleton<UIScreenCoverTokenHandler>.Instance.ReleaseToken(UIScreenCoverTokens.WaitingForMatchStarted);
			if (matchStartWasSuccessful)
			{
				this.DisplayWaitingForGameStateOtherThanNoneOrDefaultScreenCover();
			}
		}

		// Token: 0x0600023F RID: 575 RVA: 0x0000CBB8 File Offset: 0x0000ADB8
		private void SubscribeSafely(Action subscription, string eventName)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SubscribeSafely", new object[]
				{
					subscription.DebugIsNull(),
					eventName
				});
			}
			try
			{
				if (subscription != null)
				{
					subscription();
				}
			}
			catch (Exception ex)
			{
				ErrorHandler.HandleError(ErrorCodes.UIScreenCoverHandler_SubscribeSafely, ex, true, false);
			}
		}

		// Token: 0x040001A0 RID: 416
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x040001A1 RID: 417
		private readonly UIScreenCoverTokens[] screenCoverTokensToCancelIfMatchCloses = new UIScreenCoverTokens[]
		{
			UIScreenCoverTokens.WaitingForMatchAllocation,
			UIScreenCoverTokens.WaitingForGameStateOtherThanNoneOrDefault,
			UIScreenCoverTokens.ValidatingGameLibrary,
			UIScreenCoverTokens.LoadingCreator,
			UIScreenCoverTokens.LoadingGameplay,
			UIScreenCoverTokens.LoadedGameplayGameState,
			UIScreenCoverTokens.EndingMatch,
			UIScreenCoverTokens.MatchHostMigration
		};
	}
}
