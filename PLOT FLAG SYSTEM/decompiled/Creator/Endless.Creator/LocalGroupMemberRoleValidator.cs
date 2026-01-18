using System.Collections.Generic;
using System.Threading.Tasks;
using Endless.Data;
using Endless.Gameplay;
using Endless.Gameplay.RightsManagement;
using Endless.Matchmaking;
using Endless.Networking;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Runtime.Shared.Matchmaking;
using UnityEngine;

namespace Endless.Creator;

[CreateAssetMenu(menuName = "ScriptableObject/Creator/Local Group Member Role Validator", fileName = "Local Group Member Role Validator")]
public class LocalGroupMemberRoleValidator : ScriptableObject
{
	[TextArea]
	[SerializeField]
	private string modelTextToDisplayOnUserGroupMemberNotHavingRole = "Every member of your Group must have a Role!\nThe members of your Group listed below do not have a role in this game.\n";

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	public async Task<bool> ValidateAllLocalGroupMembersHaveRole(SerializableGuid assetId, SerializableGuid ancestorId, ErrorCodes errorCode)
	{
		if (verboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4}, {5}: {6} )", "ValidateAllLocalGroupMembersHaveRole", "assetId", assetId, "ancestorId", ancestorId, "errorCode", errorCode), this);
		}
		GetAllRolesResult getAllRolesResult = await MonoBehaviourSingleton<RightsManager>.Instance.GetAllUserRolesForAssetAsync(assetId, ancestorId, null, forceRefresh: true);
		if (getAllRolesResult.Error != null)
		{
			ErrorHandler.HandleError(errorCode, getAllRolesResult.Error);
		}
		return ValidateAllLocalGroupMembersHaveRole(getAllRolesResult.Roles);
	}

	public bool ValidateAllLocalGroupMembersHaveRole(IReadOnlyCollection<UserRole> userRoles)
	{
		if (verboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "ValidateAllLocalGroupMembersHaveRole", "userRoles", userRoles.Count), this);
			DebugUtility.DebugEnumerable("userRoles", userRoles, this);
		}
		GroupInfo localGroup = MatchmakingClientController.Instance.LocalGroup;
		if (localGroup == null)
		{
			return true;
		}
		HashSet<int> hashSet = new HashSet<int>();
		foreach (UserRole userRole in userRoles)
		{
			int userId = userRole.UserId;
			if (!hashSet.Add(userId))
			{
				DebugUtility.LogWarning("Somehow a duplicate userIdWithRole was found in userRoles!", this);
			}
		}
		List<int> list = new List<int>();
		foreach (CoreClientData member in localGroup.Members)
		{
			int item = member.PlatformIdToEndlessUserId();
			if (!hashSet.Contains(item))
			{
				list.Add(item);
			}
		}
		if (list.Count <= 0)
		{
			return true;
		}
		DisplayUserGroupAccessModal(list);
		return false;
	}

	private async void DisplayUserGroupAccessModal(List<int> localGroupMemberUserIdsWithoutRole)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "DisplayUserGroupAccessModal", localGroupMemberUserIdsWithoutRole.Count);
			DebugUtility.DebugEnumerable("localGroupMemberUserIdsWithoutRole", localGroupMemberUserIdsWithoutRole, this);
		}
		List<string> localGroupMemberUserNamesWithNoRole = new List<string>();
		foreach (int item in localGroupMemberUserIdsWithoutRole)
		{
			localGroupMemberUserNamesWithNoRole.Add(await MonoBehaviourSingleton<RuntimeDatabase>.Instance.GetUserName(item));
		}
		string text = string.Join(", ", localGroupMemberUserNamesWithNoRole);
		UIModalGenericViewAction uIModalGenericViewAction = new UIModalGenericViewAction(MonoBehaviourSingleton<UIModalManager>.Instance.DefaultGenericModalAction);
		uIModalGenericViewAction.OnClick = CloseModalWithoutClearingStack;
		UIModalGenericViewAction uIModalGenericViewAction2 = uIModalGenericViewAction;
		string body = modelTextToDisplayOnUserGroupMemberNotHavingRole + "\n" + text;
		MonoBehaviourSingleton<UIModalManager>.Instance.DisplayGenericModal("Party Access", null, body, UIModalManagerStackActions.MaintainStack, uIModalGenericViewAction2);
	}

	private void CloseModalWithoutClearingStack()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "CloseModalWithoutClearingStack");
		}
		MonoBehaviourSingleton<UIModalManager>.Instance.CloseWithoutClearingStack();
	}
}
