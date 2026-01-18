using System.Collections.Generic;
using Endless.Data;
using Endless.Gameplay.RightsManagement;
using Endless.Matchmaking;
using Endless.Networking;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Runtime.Shared.Matchmaking;
using UnityEngine;

namespace Endless.Gameplay.UI;

public class UIInviteToGroupAnchor : UIBaseAnchor
{
	[Header("UIInviteToGroupAnchor")]
	[SerializeField]
	private UIButton displayFriendsModalButton;

	[SerializeField]
	private EndlessStudiosUserId endlessStudiosUserId;

	[SerializeField]
	private int defaultIEnumerableWindowCanvasOverrideSorting = 4;

	public static UIInviteToGroupAnchor CreateInstance(UIInviteToGroupAnchor prefab, Transform target, RectTransform container, Vector3? offset = null)
	{
		return UIBaseAnchor.CreateAndInitialize(prefab, target, container, offset);
	}

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		displayFriendsModalButton.onClick.AddListener(OpenUserSearchWindow);
	}

	private void OpenUserSearchWindow()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OpenUserSearchWindow");
		}
		User item = new User(EndlessServices.Instance.CloudService.ActiveUserId, null, EndlessServices.Instance.CloudService.ActiveUserName);
		List<User> list = new List<User> { endlessStudiosUserId.User, item };
		if ((bool)MatchmakingClientController.Instance && MatchmakingClientController.Instance.LocalGroup != null && MatchmakingClientController.Instance.LocalGroup.Members != null)
		{
			foreach (CoreClientData member in MatchmakingClientController.Instance.LocalGroup.Members)
			{
				if (int.TryParse(member.PlatformId, out var result))
				{
					User item2 = new User(result, null, null);
					list.Add(item2);
				}
				else
				{
					DebugUtility.LogError("Could not parse user id from " + member.ToPrettyString(), this);
				}
			}
		}
		UIUserSearchWindowView.Display(new UIUserSearchWindowModel("Invite To Group", list, SelectionType.Select0OrMore, InviteToUserGroup));
	}

	private void InviteToUserGroup(List<object> items)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "InviteToUserGroup", items.Count);
		}
		foreach (object item in items)
		{
			if (item is User user)
			{
				InviteToGroupAsync(user);
			}
		}
	}

	private async void InviteToGroupAsync(User user)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "InviteToGroupAsync", user);
		}
		if (MatchmakingClientController.Instance.LocalGroup != null)
		{
			foreach (CoreClientData member in MatchmakingClientController.Instance.LocalGroup.Members)
			{
				if (member.PlatformId == user.Id.ToString())
				{
					MonoBehaviourSingleton<UIModalManager>.Instance.DisplayGenericModal("Already In Group", null, user.UserName + " is already in your active Group.", UIModalManagerStackActions.ClearStack);
					return;
				}
			}
		}
		if (!MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying && MatchmakingClientController.Instance.ActiveGameId != SerializableGuid.Empty)
		{
			GetAllRolesResult obj = await MonoBehaviourSingleton<RightsManager>.Instance.GetAllUserRolesForAssetAsync(MatchmakingClientController.Instance.ActiveGameId);
			Roles roles = Roles.None;
			foreach (UserRole role in obj.Roles)
			{
				if (role.UserId == user.Id)
				{
					roles = role.Role;
				}
			}
			if (roles == Roles.None)
			{
				MonoBehaviourSingleton<UIModalManager>.Instance.DisplayGenericModal("No Role Access", null, user.UserName + " does not have access to the Game you are in!", UIModalManagerStackActions.ClearStack);
				return;
			}
		}
		ClientData clientData = new ClientData(user.Id.ToString(), TargetPlatforms.Endless, user.UserName);
		MatchmakingClientController.Instance.InviteToGroup(clientData.CoreData.PlatformId);
	}
}
