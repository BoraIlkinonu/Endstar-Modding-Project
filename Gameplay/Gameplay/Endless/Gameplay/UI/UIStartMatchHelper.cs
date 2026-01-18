using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Endless.Assets;
using Endless.Data;
using Endless.Data.UI;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.RightsManagement;
using Endless.GraphQl;
using Endless.Matchmaking;
using Endless.Networking;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Newtonsoft.Json;
using Runtime.Shared;
using Runtime.Shared.Matchmaking;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003D4 RID: 980
	public class UIStartMatchHelper : UIMonoBehaviourSingleton<UIStartMatchHelper>
	{
		// Token: 0x17000511 RID: 1297
		// (get) Token: 0x060018C2 RID: 6338 RVA: 0x000729B8 File Offset: 0x00070BB8
		public UnityEvent StartMatchRequestedUnityEvent { get; } = new UnityEvent();

		// Token: 0x17000512 RID: 1298
		// (get) Token: 0x060018C3 RID: 6339 RVA: 0x000729C0 File Offset: 0x00070BC0
		public UnityEvent<bool> StartMatchCompleteUnityEvent { get; } = new UnityEvent<bool>();

		// Token: 0x17000513 RID: 1299
		// (get) Token: 0x060018C4 RID: 6340 RVA: 0x000729C8 File Offset: 0x00070BC8
		public UnityEvent EndMatchAndStartNewMatchWithCachedDataUnityEvent { get; } = new UnityEvent();

		// Token: 0x17000514 RID: 1300
		// (get) Token: 0x060018C5 RID: 6341 RVA: 0x000729D0 File Offset: 0x00070BD0
		private static bool IsInMatch
		{
			get
			{
				return !MatchmakingClientController.Instance.ActiveGameId.IsEmpty;
			}
		}

		// Token: 0x17000515 RID: 1301
		// (get) Token: 0x060018C6 RID: 6342 RVA: 0x000729F2 File Offset: 0x00070BF2
		private static GroupInfo GroupInfo
		{
			get
			{
				return MatchmakingClientController.Instance.LocalGroup;
			}
		}

		// Token: 0x17000516 RID: 1302
		// (get) Token: 0x060018C7 RID: 6343 RVA: 0x000729FE File Offset: 0x00070BFE
		private static bool IsInUserGroupWithMoreThanOneMember
		{
			get
			{
				return UIStartMatchHelper.GroupInfo != null && UIStartMatchHelper.GroupInfo.Members.Count > 1;
			}
		}

		// Token: 0x060018C8 RID: 6344 RVA: 0x00072A1C File Offset: 0x00070C1C
		public async void TryToStartMatch(string gameId, string gameVersion, string levelId, MainMenuGameContext mainMenuGameContext)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "TryToStartMatch", new object[] { gameId, gameVersion, levelId, mainMenuGameContext });
			}
			MonoBehaviourSingleton<LoadTimeTester>.Instance.StartTracking("MatchLoad");
			this.cachedGameId = gameId;
			if (mainMenuGameContext != MainMenuGameContext.Edit && string.IsNullOrEmpty(gameVersion))
			{
				GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetAssetAsync(gameId, "", null, true, 10);
				if (graphQlResult.HasErrors)
				{
					Debug.LogException(graphQlResult.GetErrorMessage(0));
					return;
				}
				Asset asset = JsonConvert.DeserializeObject<Game>(graphQlResult.GetDataMember().ToString());
				this.cachedGameVersion = asset.AssetVersion;
			}
			else
			{
				this.cachedGameVersion = gameVersion;
			}
			this.cachedLevelId = levelId;
			this.cachedMainMenuGameContext = mainMenuGameContext;
			bool localClientIsGroupHost = this.GetLocalClientIsGroupHost();
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0}: {1}, {2}: {3}, {4}: {5}, {6}: {7}", new object[]
				{
					"IsInMatch",
					UIStartMatchHelper.IsInMatch,
					"IsInUserGroupWithMoreThanOneMember",
					UIStartMatchHelper.IsInUserGroupWithMoreThanOneMember,
					"localClientIsGroupHost",
					localClientIsGroupHost,
					"UserCount",
					NetworkBehaviourSingleton<UserIdManager>.Instance.UserCount
				}), this);
			}
			TaskAwaiter<bool> taskAwaiter2;
			if (mainMenuGameContext == MainMenuGameContext.Edit && UIStartMatchHelper.IsInUserGroupWithMoreThanOneMember && localClientIsGroupHost)
			{
				try
				{
					TaskAwaiter<bool> taskAwaiter = this.EnsureAllUserGroupMembersHaveRoles((await MonoBehaviourSingleton<RightsManager>.Instance.GetAllUserRolesForAssetAsync(levelId, gameId, null, true)).Roles).GetAwaiter();
					if (!taskAwaiter.IsCompleted)
					{
						await taskAwaiter;
						taskAwaiter = taskAwaiter2;
						taskAwaiter2 = default(TaskAwaiter<bool>);
					}
					if (!taskAwaiter.GetResult())
					{
						return;
					}
				}
				catch (Exception ex)
				{
					ErrorHandler.HandleError(ErrorCodes.UIStartMatchHelper_RetrievingUsersWithRolesForAsset, ex, true, false);
					return;
				}
			}
			if (UIStartMatchHelper.IsInMatch)
			{
				if (NetworkBehaviourSingleton<UserIdManager>.Instance.UserCount <= 1)
				{
					this.EndMatchAndStartNewMatchWithCachedData();
				}
				else
				{
					bool flag = this.LocalClientIsMatchHost();
					if (flag)
					{
						TaskAwaiter<bool> taskAwaiter = MonoBehaviourSingleton<UIModalManager>.Instance.Confirm(this.hostMigrationConfirmationText, UIModalManagerStackActions.MaintainStack).GetAwaiter();
						if (!taskAwaiter.IsCompleted)
						{
							await taskAwaiter;
							taskAwaiter = taskAwaiter2;
							taskAwaiter2 = default(TaskAwaiter<bool>);
						}
						flag = !taskAwaiter.GetResult();
					}
					if (flag)
					{
						MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
					}
					else
					{
						MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
						if (!UIStartMatchHelper.IsInUserGroupWithMoreThanOneMember)
						{
							this.EndMatchAndStartNewMatchWithCachedData();
						}
						else
						{
							string text;
							UIModalGenericViewAction[] array;
							if (localClientIsGroupHost)
							{
								text = this.userGroupLeaderLeaveOrBringConfirmationText;
								array = new UIModalGenericViewAction[]
								{
									new UIModalGenericViewAction(UIColors.AzureRadiance, "Bring Them", new Action(this.EndMatchAndStartNewMatchWithCachedData)),
									new UIModalGenericViewAction(UIColors.AzureRadiance, "Leave Them", new Action(this.LeaveGroupAndStartMatchFromCachedData)),
									new UIModalGenericViewAction(UIColors.VampireRed, "Cancel", new Action(MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack))
								};
							}
							else
							{
								text = this.userGroupMemberLeaveConfirmationText;
								array = new UIModalGenericViewAction[]
								{
									new UIModalGenericViewAction(UIColors.AzureRadiance, "Leave Party", new Action(this.LeaveGroupAndStartMatchFromCachedData)),
									new UIModalGenericViewAction(UIColors.VampireRed, "Cancel", new Action(MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack))
								};
							}
							await MonoBehaviourSingleton<UIModalManager>.Instance.DisplayGenericModalAsync("Confirm", null, text, UIModalManagerStackActions.ClearStack, array);
						}
					}
				}
			}
			else if (UIStartMatchHelper.IsInUserGroupWithMoreThanOneMember && !localClientIsGroupHost)
			{
				string text2 = this.userGroupMemberLeaveConfirmationText;
				UIModalGenericViewAction[] array2 = new UIModalGenericViewAction[]
				{
					new UIModalGenericViewAction(UIColors.AzureRadiance, "Leave Party", new Action(this.LeaveGroupAndStartMatchFromCachedData)),
					new UIModalGenericViewAction(UIColors.VampireRed, "Cancel", new Action(MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack))
				};
				await MonoBehaviourSingleton<UIModalManager>.Instance.DisplayGenericModalAsync("Confirm", null, text2, UIModalManagerStackActions.ClearStack, array2);
			}
			else
			{
				this.StartMatchFromCachedData();
			}
		}

		// Token: 0x060018C9 RID: 6345 RVA: 0x00072A74 File Offset: 0x00070C74
		private void EndMatchAndStartNewMatchWithCachedData()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "EndMatchAndStartNewMatchWithCachedData", Array.Empty<object>());
			}
			MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
			MatchmakingClientController.MatchLeft += this.UnhookFromMatchClosedAndStartNewMatch;
			MonoBehaviourSingleton<ConnectionActions>.Instance.EndMatch(null);
			this.EndMatchAndStartNewMatchWithCachedDataUnityEvent.Invoke();
		}

		// Token: 0x060018CA RID: 6346 RVA: 0x00072ACA File Offset: 0x00070CCA
		private void UnhookFromMatchClosedAndStartNewMatch()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "UnhookFromMatchClosedAndStartNewMatch", Array.Empty<object>());
			}
			MatchmakingClientController.MatchLeft -= this.UnhookFromMatchClosedAndStartNewMatch;
			this.TryToStartMatchFromCachedData();
		}

		// Token: 0x060018CB RID: 6347 RVA: 0x00072AFB File Offset: 0x00070CFB
		private void TryToStartMatchFromCachedData()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "TryToStartMatchFromCachedData", Array.Empty<object>());
			}
			this.TryToStartMatch(this.cachedGameId, this.cachedGameVersion, this.cachedLevelId, this.cachedMainMenuGameContext);
		}

		// Token: 0x060018CC RID: 6348 RVA: 0x00072B33 File Offset: 0x00070D33
		private void CloseModalWithoutClearingStack()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "CloseModalWithoutClearingStack", Array.Empty<object>());
			}
			MonoBehaviourSingleton<UIModalManager>.Instance.CloseWithoutClearingStack();
		}

		// Token: 0x060018CD RID: 6349 RVA: 0x00072B58 File Offset: 0x00070D58
		private void StartMatchFromCachedData()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethodWithAppension(this, "StartMatchFromCachedData", string.Format("{0}: {1}, {2}: {3}, {4}: {5}, {6}: {7}", new object[] { "cachedGameId", this.cachedGameId, "cachedLevelId", this.cachedLevelId, "cachedMainMenuGameContext", this.cachedMainMenuGameContext, "cachedGameVersion", this.cachedGameVersion }), Array.Empty<object>());
			}
			MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
			this.StartMatchRequestedUnityEvent.Invoke();
			MatchmakingClientController.MatchStart += this.OnUserRequestedMatchStartSuccess;
			bool flag = this.cachedMainMenuGameContext == MainMenuGameContext.Edit;
			string text = null;
			if (this.cachedMainMenuGameContext == MainMenuGameContext.Admin)
			{
				text = MatchDataExtensions.BuildAdminSessionData();
			}
			MatchmakingClientController.Instance.StartMatch(this.cachedGameId, this.cachedLevelId, flag, this.cachedGameVersion, null, text, new Action<int, string>(this.OnUserRequestedMatchStartFailed));
		}

		// Token: 0x060018CE RID: 6350 RVA: 0x00072C44 File Offset: 0x00070E44
		private async Task<bool> EnsureAllUserGroupMembersHaveRoles(IReadOnlyList<UserRole> userRoles)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "EnsureAllUserGroupMembersHaveRoles", new object[] { userRoles });
			}
			HashSet<string> usersWhoHaveRoles = new HashSet<string>();
			List<string> memberDisplayNamesWithNoAccess = new List<string>();
			foreach (UserRole userRole in userRoles)
			{
				usersWhoHaveRoles.Add(userRole.UserId.ToString());
			}
			foreach (CoreClientData coreClientData in UIStartMatchHelper.GroupInfo.Members)
			{
				if (!usersWhoHaveRoles.Contains(coreClientData.PlatformId))
				{
					string text = await MonoBehaviourSingleton<RuntimeDatabase>.Instance.GetUserName(coreClientData.PlatformIdToEndlessUserId());
					memberDisplayNamesWithNoAccess.Add(text);
				}
			}
			List<CoreClientData>.Enumerator enumerator2 = default(List<CoreClientData>.Enumerator);
			bool flag;
			if (memberDisplayNamesWithNoAccess.Count == 0)
			{
				flag = true;
			}
			else
			{
				string text2 = StringUtility.CommaSeparate(memberDisplayNamesWithNoAccess) + "\n" + ((memberDisplayNamesWithNoAccess.Count == 1) ? "is" : "are") + " in your Party but does not have a Role for this level. Everyone in the Party must have access!";
				UIModalGenericViewAction uimodalGenericViewAction = new UIModalGenericViewAction(MonoBehaviourSingleton<UIModalManager>.Instance.DefaultGenericModalAction)
				{
					OnClick = new Action(this.CloseModalWithoutClearingStack)
				};
				MonoBehaviourSingleton<UIModalManager>.Instance.DisplayGenericModal("Party Access", null, text2, UIModalManagerStackActions.MaintainStack, new UIModalGenericViewAction[] { uimodalGenericViewAction });
				flag = false;
			}
			return flag;
		}

		// Token: 0x060018CF RID: 6351 RVA: 0x00072C90 File Offset: 0x00070E90
		private bool LocalClientIsMatchHost()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "LocalClientIsMatchHost", Array.Empty<object>());
			}
			if (MatchmakingClientController.Instance.LocalClientData == null)
			{
				Debug.LogException(new NullReferenceException("LocalClientData is null!"));
				return false;
			}
			int userId = NetworkBehaviourSingleton<UserIdManager>.Instance.GetUserId(0UL);
			ClientData value = MatchmakingClientController.Instance.LocalClientData.Value;
			return userId.ToString() == value.CoreData.PlatformId;
		}

		// Token: 0x060018D0 RID: 6352 RVA: 0x00072D14 File Offset: 0x00070F14
		private bool GetLocalClientIsGroupHost()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "GetLocalClientIsGroupHost", Array.Empty<object>());
			}
			if (MatchmakingClientController.Instance.LocalClientData == null)
			{
				Debug.LogException(new NullReferenceException("LocalClientData is null!"));
				return false;
			}
			if (!UIStartMatchHelper.IsInUserGroupWithMoreThanOneMember)
			{
				return false;
			}
			ClientData value = MatchmakingClientController.Instance.LocalClientData.Value;
			return UIStartMatchHelper.GroupInfo.Host.PlatformId == value.CoreData.PlatformId;
		}

		// Token: 0x060018D1 RID: 6353 RVA: 0x00072D99 File Offset: 0x00070F99
		private void OnUserRequestedMatchStartSuccess()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnUserRequestedMatchStartSuccess", Array.Empty<object>());
			}
			MatchmakingClientController.MatchStart -= this.OnUserRequestedMatchStartSuccess;
			this.StartMatchCompleteUnityEvent.Invoke(true);
		}

		// Token: 0x060018D2 RID: 6354 RVA: 0x00072DD0 File Offset: 0x00070FD0
		private void OnUserRequestedMatchStartFailed(int statusCode, string errorMessage)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnUserRequestedMatchStartFailed", new object[] { statusCode, errorMessage });
			}
			ErrorHandler.HandleError(ErrorCodes.UIStartMatchHelper_UserRequestedMatchStart, new Exception(errorMessage), true, true);
			this.StartMatchCompleteUnityEvent.Invoke(false);
		}

		// Token: 0x060018D3 RID: 6355 RVA: 0x00072E21 File Offset: 0x00071021
		private void LeaveGroupAndStartMatchFromCachedData()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "LeaveGroupAndStartMatchFromCachedData", Array.Empty<object>());
			}
			MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
			MatchmakingClientController.Instance.LeaveGroup(false, null);
			this.StartMatchFromCachedData();
		}

		// Token: 0x040013DD RID: 5085
		[SerializeField]
		[TextArea]
		private string hostMigrationConfirmationText = "You are the host. Loading this level will trigger a host migration where all other players will need to reload.";

		// Token: 0x040013DE RID: 5086
		[SerializeField]
		[TextArea]
		private string userGroupLeaderLeaveOrBringConfirmationText = "You are in a Party and its leader. To load this level, all members will be brought with you.";

		// Token: 0x040013DF RID: 5087
		[SerializeField]
		[TextArea]
		private string userGroupMemberLeaveConfirmationText = "You are in a Party and not its leader. To load this level, you will have to drop from your active Party.";

		// Token: 0x040013E0 RID: 5088
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x040013E1 RID: 5089
		private MainMenuGameContext cachedMainMenuGameContext;

		// Token: 0x040013E2 RID: 5090
		private string cachedGameId;

		// Token: 0x040013E3 RID: 5091
		private string cachedGameVersion;

		// Token: 0x040013E4 RID: 5092
		private string cachedLevelId;
	}
}
