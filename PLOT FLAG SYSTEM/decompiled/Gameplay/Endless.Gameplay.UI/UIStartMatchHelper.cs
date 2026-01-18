using System;
using System.Collections.Generic;
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

namespace Endless.Gameplay.UI;

public class UIStartMatchHelper : UIMonoBehaviourSingleton<UIStartMatchHelper>
{
	[SerializeField]
	[TextArea]
	private string hostMigrationConfirmationText = "You are the host. Loading this level will trigger a host migration where all other players will need to reload.";

	[SerializeField]
	[TextArea]
	private string userGroupLeaderLeaveOrBringConfirmationText = "You are in a Party and its leader. To load this level, all members will be brought with you.";

	[SerializeField]
	[TextArea]
	private string userGroupMemberLeaveConfirmationText = "You are in a Party and not its leader. To load this level, you will have to drop from your active Party.";

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private MainMenuGameContext cachedMainMenuGameContext;

	private string cachedGameId;

	private string cachedGameVersion;

	private string cachedLevelId;

	public UnityEvent StartMatchRequestedUnityEvent { get; } = new UnityEvent();

	public UnityEvent<bool> StartMatchCompleteUnityEvent { get; } = new UnityEvent<bool>();

	public UnityEvent EndMatchAndStartNewMatchWithCachedDataUnityEvent { get; } = new UnityEvent();

	private static bool IsInMatch => !MatchmakingClientController.Instance.ActiveGameId.IsEmpty;

	private static GroupInfo GroupInfo => MatchmakingClientController.Instance.LocalGroup;

	private static bool IsInUserGroupWithMoreThanOneMember
	{
		get
		{
			if (GroupInfo == null)
			{
				return false;
			}
			return GroupInfo.Members.Count > 1;
		}
	}

	public async void TryToStartMatch(string gameId, string gameVersion, string levelId, MainMenuGameContext mainMenuGameContext)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "TryToStartMatch", gameId, gameVersion, levelId, mainMenuGameContext);
		}
		MonoBehaviourSingleton<LoadTimeTester>.Instance.StartTracking("MatchLoad");
		cachedGameId = gameId;
		if (mainMenuGameContext != MainMenuGameContext.Edit && string.IsNullOrEmpty(gameVersion))
		{
			GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetAssetAsync(gameId, "", null, debugQuery: true);
			if (graphQlResult.HasErrors)
			{
				Debug.LogException(graphQlResult.GetErrorMessage());
				return;
			}
			Asset asset = JsonConvert.DeserializeObject<Game>(graphQlResult.GetDataMember().ToString());
			cachedGameVersion = asset.AssetVersion;
		}
		else
		{
			cachedGameVersion = gameVersion;
		}
		cachedLevelId = levelId;
		cachedMainMenuGameContext = mainMenuGameContext;
		bool localClientIsGroupHost = GetLocalClientIsGroupHost();
		if (verboseLogging)
		{
			DebugUtility.Log(string.Format("{0}: {1}, {2}: {3}, {4}: {5}, {6}: {7}", "IsInMatch", IsInMatch, "IsInUserGroupWithMoreThanOneMember", IsInUserGroupWithMoreThanOneMember, "localClientIsGroupHost", localClientIsGroupHost, "UserCount", NetworkBehaviourSingleton<UserIdManager>.Instance.UserCount), this);
		}
		if (mainMenuGameContext == MainMenuGameContext.Edit && IsInUserGroupWithMoreThanOneMember && localClientIsGroupHost)
		{
			try
			{
				if (!(await EnsureAllUserGroupMembersHaveRoles((await MonoBehaviourSingleton<RightsManager>.Instance.GetAllUserRolesForAssetAsync(levelId, gameId, null, forceRefresh: true)).Roles)))
				{
					return;
				}
			}
			catch (Exception exception)
			{
				ErrorHandler.HandleError(ErrorCodes.UIStartMatchHelper_RetrievingUsersWithRolesForAsset, exception);
				return;
			}
		}
		if (IsInMatch)
		{
			if (NetworkBehaviourSingleton<UserIdManager>.Instance.UserCount <= 1)
			{
				EndMatchAndStartNewMatchWithCachedData();
				return;
			}
			bool flag = LocalClientIsMatchHost();
			if (flag)
			{
				flag = !(await MonoBehaviourSingleton<UIModalManager>.Instance.Confirm(hostMigrationConfirmationText));
			}
			if (flag)
			{
				MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
				return;
			}
			MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
			if (!IsInUserGroupWithMoreThanOneMember)
			{
				EndMatchAndStartNewMatchWithCachedData();
				return;
			}
			string body;
			UIModalGenericViewAction[] buttons;
			if (localClientIsGroupHost)
			{
				body = userGroupLeaderLeaveOrBringConfirmationText;
				buttons = new UIModalGenericViewAction[3]
				{
					new UIModalGenericViewAction(UIColors.AzureRadiance, "Bring Them", EndMatchAndStartNewMatchWithCachedData),
					new UIModalGenericViewAction(UIColors.AzureRadiance, "Leave Them", LeaveGroupAndStartMatchFromCachedData),
					new UIModalGenericViewAction(UIColors.VampireRed, "Cancel", MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack)
				};
			}
			else
			{
				body = userGroupMemberLeaveConfirmationText;
				buttons = new UIModalGenericViewAction[2]
				{
					new UIModalGenericViewAction(UIColors.AzureRadiance, "Leave Party", LeaveGroupAndStartMatchFromCachedData),
					new UIModalGenericViewAction(UIColors.VampireRed, "Cancel", MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack)
				};
			}
			await MonoBehaviourSingleton<UIModalManager>.Instance.DisplayGenericModalAsync("Confirm", null, body, UIModalManagerStackActions.ClearStack, buttons);
		}
		else if (IsInUserGroupWithMoreThanOneMember && !localClientIsGroupHost)
		{
			string body2 = userGroupMemberLeaveConfirmationText;
			UIModalGenericViewAction[] buttons2 = new UIModalGenericViewAction[2]
			{
				new UIModalGenericViewAction(UIColors.AzureRadiance, "Leave Party", LeaveGroupAndStartMatchFromCachedData),
				new UIModalGenericViewAction(UIColors.VampireRed, "Cancel", MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack)
			};
			await MonoBehaviourSingleton<UIModalManager>.Instance.DisplayGenericModalAsync("Confirm", null, body2, UIModalManagerStackActions.ClearStack, buttons2);
		}
		else
		{
			StartMatchFromCachedData();
		}
	}

	private void EndMatchAndStartNewMatchWithCachedData()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "EndMatchAndStartNewMatchWithCachedData");
		}
		MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
		MatchmakingClientController.MatchLeft += UnhookFromMatchClosedAndStartNewMatch;
		MonoBehaviourSingleton<ConnectionActions>.Instance.EndMatch();
		EndMatchAndStartNewMatchWithCachedDataUnityEvent.Invoke();
	}

	private void UnhookFromMatchClosedAndStartNewMatch()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "UnhookFromMatchClosedAndStartNewMatch");
		}
		MatchmakingClientController.MatchLeft -= UnhookFromMatchClosedAndStartNewMatch;
		TryToStartMatchFromCachedData();
	}

	private void TryToStartMatchFromCachedData()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "TryToStartMatchFromCachedData");
		}
		TryToStartMatch(cachedGameId, cachedGameVersion, cachedLevelId, cachedMainMenuGameContext);
	}

	private void CloseModalWithoutClearingStack()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "CloseModalWithoutClearingStack");
		}
		MonoBehaviourSingleton<UIModalManager>.Instance.CloseWithoutClearingStack();
	}

	private void StartMatchFromCachedData()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethodWithAppension(this, "StartMatchFromCachedData", string.Format("{0}: {1}, {2}: {3}, {4}: {5}, {6}: {7}", "cachedGameId", cachedGameId, "cachedLevelId", cachedLevelId, "cachedMainMenuGameContext", cachedMainMenuGameContext, "cachedGameVersion", cachedGameVersion));
		}
		MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
		StartMatchRequestedUnityEvent.Invoke();
		MatchmakingClientController.MatchStart += OnUserRequestedMatchStartSuccess;
		bool isEditSession = cachedMainMenuGameContext == MainMenuGameContext.Edit;
		string customData = null;
		if (cachedMainMenuGameContext == MainMenuGameContext.Admin)
		{
			customData = MatchDataExtensions.BuildAdminSessionData();
		}
		MatchmakingClientController.Instance.StartMatch(cachedGameId, cachedLevelId, isEditSession, cachedGameVersion, null, customData, OnUserRequestedMatchStartFailed);
	}

	private async Task<bool> EnsureAllUserGroupMembersHaveRoles(IReadOnlyList<UserRole> userRoles)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "EnsureAllUserGroupMembersHaveRoles", userRoles);
		}
		HashSet<string> usersWhoHaveRoles = new HashSet<string>();
		List<string> memberDisplayNamesWithNoAccess = new List<string>();
		foreach (UserRole userRole in userRoles)
		{
			usersWhoHaveRoles.Add(userRole.UserId.ToString());
		}
		foreach (CoreClientData member in GroupInfo.Members)
		{
			if (!usersWhoHaveRoles.Contains(member.PlatformId))
			{
				memberDisplayNamesWithNoAccess.Add(await MonoBehaviourSingleton<RuntimeDatabase>.Instance.GetUserName(member.PlatformIdToEndlessUserId()));
			}
		}
		if (memberDisplayNamesWithNoAccess.Count == 0)
		{
			return true;
		}
		string body = StringUtility.CommaSeparate(memberDisplayNamesWithNoAccess) + "\n" + ((memberDisplayNamesWithNoAccess.Count == 1) ? "is" : "are") + " in your Party but does not have a Role for this level. Everyone in the Party must have access!";
		UIModalGenericViewAction uIModalGenericViewAction = new UIModalGenericViewAction(MonoBehaviourSingleton<UIModalManager>.Instance.DefaultGenericModalAction);
		uIModalGenericViewAction.OnClick = CloseModalWithoutClearingStack;
		UIModalGenericViewAction uIModalGenericViewAction2 = uIModalGenericViewAction;
		MonoBehaviourSingleton<UIModalManager>.Instance.DisplayGenericModal("Party Access", null, body, UIModalManagerStackActions.MaintainStack, uIModalGenericViewAction2);
		return false;
	}

	private bool LocalClientIsMatchHost()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "LocalClientIsMatchHost");
		}
		if (!MatchmakingClientController.Instance.LocalClientData.HasValue)
		{
			Debug.LogException(new NullReferenceException("LocalClientData is null!"));
			return false;
		}
		int userId = NetworkBehaviourSingleton<UserIdManager>.Instance.GetUserId(0uL);
		ClientData value = MatchmakingClientController.Instance.LocalClientData.Value;
		return userId.ToString() == value.CoreData.PlatformId;
	}

	private bool GetLocalClientIsGroupHost()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "GetLocalClientIsGroupHost");
		}
		if (!MatchmakingClientController.Instance.LocalClientData.HasValue)
		{
			Debug.LogException(new NullReferenceException("LocalClientData is null!"));
			return false;
		}
		if (!IsInUserGroupWithMoreThanOneMember)
		{
			return false;
		}
		ClientData value = MatchmakingClientController.Instance.LocalClientData.Value;
		return GroupInfo.Host.PlatformId == value.CoreData.PlatformId;
	}

	private void OnUserRequestedMatchStartSuccess()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnUserRequestedMatchStartSuccess");
		}
		MatchmakingClientController.MatchStart -= OnUserRequestedMatchStartSuccess;
		StartMatchCompleteUnityEvent.Invoke(arg0: true);
	}

	private void OnUserRequestedMatchStartFailed(int statusCode, string errorMessage)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnUserRequestedMatchStartFailed", statusCode, errorMessage);
		}
		ErrorHandler.HandleError(ErrorCodes.UIStartMatchHelper_UserRequestedMatchStart, new Exception(errorMessage), displayModal: true, leaveMatch: true);
		StartMatchCompleteUnityEvent.Invoke(arg0: false);
	}

	private void LeaveGroupAndStartMatchFromCachedData()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "LeaveGroupAndStartMatchFromCachedData");
		}
		MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
		MatchmakingClientController.Instance.LeaveGroup(stayInMatch: false);
		StartMatchFromCachedData();
	}
}
