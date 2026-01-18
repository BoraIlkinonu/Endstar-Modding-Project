using System;
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

namespace Endless.Creator
{
	// Token: 0x0200007A RID: 122
	[CreateAssetMenu(menuName = "ScriptableObject/Creator/Local Group Member Role Validator", fileName = "Local Group Member Role Validator")]
	public class LocalGroupMemberRoleValidator : ScriptableObject
	{
		// Token: 0x060001A7 RID: 423 RVA: 0x0000CD5C File Offset: 0x0000AF5C
		public async Task<bool> ValidateAllLocalGroupMembersHaveRole(SerializableGuid assetId, SerializableGuid ancestorId, ErrorCodes errorCode)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4}, {5}: {6} )", new object[] { "ValidateAllLocalGroupMembersHaveRole", "assetId", assetId, "ancestorId", ancestorId, "errorCode", errorCode }), this);
			}
			GetAllRolesResult getAllRolesResult = await MonoBehaviourSingleton<RightsManager>.Instance.GetAllUserRolesForAssetAsync(assetId, ancestorId, null, true);
			if (getAllRolesResult.Error != null)
			{
				ErrorHandler.HandleError(errorCode, getAllRolesResult.Error, true, false);
			}
			return this.ValidateAllLocalGroupMembersHaveRole(getAllRolesResult.Roles);
		}

		// Token: 0x060001A8 RID: 424 RVA: 0x0000CDB8 File Offset: 0x0000AFB8
		public bool ValidateAllLocalGroupMembersHaveRole(IReadOnlyCollection<UserRole> userRoles)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "ValidateAllLocalGroupMembersHaveRole", "userRoles", userRoles.Count), this);
				DebugUtility.DebugEnumerable<UserRole>("userRoles", userRoles, this);
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
			foreach (CoreClientData coreClientData in localGroup.Members)
			{
				int num = coreClientData.PlatformIdToEndlessUserId();
				if (!hashSet.Contains(num))
				{
					list.Add(num);
				}
			}
			if (list.Count <= 0)
			{
				return true;
			}
			this.DisplayUserGroupAccessModal(list);
			return false;
		}

		// Token: 0x060001A9 RID: 425 RVA: 0x0000CED0 File Offset: 0x0000B0D0
		private async void DisplayUserGroupAccessModal(List<int> localGroupMemberUserIdsWithoutRole)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "DisplayUserGroupAccessModal", new object[] { localGroupMemberUserIdsWithoutRole.Count });
				DebugUtility.DebugEnumerable<int>("localGroupMemberUserIdsWithoutRole", localGroupMemberUserIdsWithoutRole, this);
			}
			List<string> localGroupMemberUserNamesWithNoRole = new List<string>();
			foreach (int num in localGroupMemberUserIdsWithoutRole)
			{
				string text = await MonoBehaviourSingleton<RuntimeDatabase>.Instance.GetUserName(num);
				localGroupMemberUserNamesWithNoRole.Add(text);
			}
			List<int>.Enumerator enumerator = default(List<int>.Enumerator);
			string text2 = string.Join(", ", localGroupMemberUserNamesWithNoRole);
			UIModalGenericViewAction uimodalGenericViewAction = new UIModalGenericViewAction(MonoBehaviourSingleton<UIModalManager>.Instance.DefaultGenericModalAction)
			{
				OnClick = new Action(this.CloseModalWithoutClearingStack)
			};
			string text3 = this.modelTextToDisplayOnUserGroupMemberNotHavingRole + "\n" + text2;
			MonoBehaviourSingleton<UIModalManager>.Instance.DisplayGenericModal("Party Access", null, text3, UIModalManagerStackActions.MaintainStack, new UIModalGenericViewAction[] { uimodalGenericViewAction });
		}

		// Token: 0x060001AA RID: 426 RVA: 0x0000CF0F File Offset: 0x0000B10F
		private void CloseModalWithoutClearingStack()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "CloseModalWithoutClearingStack", Array.Empty<object>());
			}
			MonoBehaviourSingleton<UIModalManager>.Instance.CloseWithoutClearingStack();
		}

		// Token: 0x0400021E RID: 542
		[TextArea]
		[SerializeField]
		private string modelTextToDisplayOnUserGroupMemberNotHavingRole = "Every member of your Group must have a Role!\nThe members of your Group listed below do not have a role in this game.\n";

		// Token: 0x0400021F RID: 543
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
