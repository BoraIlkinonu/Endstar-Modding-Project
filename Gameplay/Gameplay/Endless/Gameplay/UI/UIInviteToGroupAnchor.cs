using System;
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
using UnityEngine.Events;

namespace Endless.Gameplay.UI
{
	// Token: 0x0200038B RID: 907
	public class UIInviteToGroupAnchor : UIBaseAnchor
	{
		// Token: 0x0600171F RID: 5919 RVA: 0x0006C0A3 File Offset: 0x0006A2A3
		public static UIInviteToGroupAnchor CreateInstance(UIInviteToGroupAnchor prefab, Transform target, RectTransform container, Vector3? offset = null)
		{
			return UIBaseAnchor.CreateAndInitialize<UIInviteToGroupAnchor>(prefab, target, container, offset);
		}

		// Token: 0x06001720 RID: 5920 RVA: 0x0006C0AE File Offset: 0x0006A2AE
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.displayFriendsModalButton.onClick.AddListener(new UnityAction(this.OpenUserSearchWindow));
		}

		// Token: 0x06001721 RID: 5921 RVA: 0x0006C0E4 File Offset: 0x0006A2E4
		private void OpenUserSearchWindow()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OpenUserSearchWindow", Array.Empty<object>());
			}
			User user = new User(EndlessServices.Instance.CloudService.ActiveUserId, null, EndlessServices.Instance.CloudService.ActiveUserName);
			List<User> list = new List<User>
			{
				this.endlessStudiosUserId.User,
				user
			};
			if (MatchmakingClientController.Instance && MatchmakingClientController.Instance.LocalGroup != null && MatchmakingClientController.Instance.LocalGroup.Members != null)
			{
				foreach (CoreClientData coreClientData in MatchmakingClientController.Instance.LocalGroup.Members)
				{
					int num;
					if (int.TryParse(coreClientData.PlatformId, out num))
					{
						User user2 = new User(num, null, null);
						list.Add(user2);
					}
					else
					{
						DebugUtility.LogError("Could not parse user id from " + coreClientData.ToPrettyString(), this);
					}
				}
			}
			UIUserSearchWindowView.Display(new UIUserSearchWindowModel("Invite To Group", list, SelectionType.Select0OrMore, new Action<List<object>>(this.InviteToUserGroup)), null);
		}

		// Token: 0x06001722 RID: 5922 RVA: 0x0006C21C File Offset: 0x0006A41C
		private void InviteToUserGroup(List<object> items)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "InviteToUserGroup", new object[] { items.Count });
			}
			foreach (object obj in items)
			{
				User user = obj as User;
				if (user != null)
				{
					this.InviteToGroupAsync(user);
				}
			}
		}

		// Token: 0x06001723 RID: 5923 RVA: 0x0006C29C File Offset: 0x0006A49C
		private async void InviteToGroupAsync(User user)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "InviteToGroupAsync", new object[] { user });
			}
			if (MatchmakingClientController.Instance.LocalGroup != null)
			{
				using (List<CoreClientData>.Enumerator enumerator = MatchmakingClientController.Instance.LocalGroup.Members.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current.PlatformId == user.Id.ToString())
						{
							MonoBehaviourSingleton<UIModalManager>.Instance.DisplayGenericModal("Already In Group", null, user.UserName + " is already in your active Group.", UIModalManagerStackActions.ClearStack, Array.Empty<UIModalGenericViewAction>());
							return;
						}
					}
				}
			}
			if (!MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying && MatchmakingClientController.Instance.ActiveGameId != SerializableGuid.Empty)
			{
				ref GetAllRolesResult ptr = await MonoBehaviourSingleton<RightsManager>.Instance.GetAllUserRolesForAssetAsync(MatchmakingClientController.Instance.ActiveGameId, null, false);
				Roles roles = Roles.None;
				foreach (UserRole userRole in ptr.Roles)
				{
					if (userRole.UserId == user.Id)
					{
						roles = userRole.Role;
					}
				}
				if (roles == Roles.None)
				{
					MonoBehaviourSingleton<UIModalManager>.Instance.DisplayGenericModal("No Role Access", null, user.UserName + " does not have access to the Game you are in!", UIModalManagerStackActions.ClearStack, Array.Empty<UIModalGenericViewAction>());
					return;
				}
			}
			ClientData clientData = new ClientData(user.Id.ToString(), TargetPlatforms.Endless, user.UserName);
			MatchmakingClientController.Instance.InviteToGroup(clientData.CoreData.PlatformId, null);
		}

		// Token: 0x0400128D RID: 4749
		[Header("UIInviteToGroupAnchor")]
		[SerializeField]
		private UIButton displayFriendsModalButton;

		// Token: 0x0400128E RID: 4750
		[SerializeField]
		private EndlessStudiosUserId endlessStudiosUserId;

		// Token: 0x0400128F RID: 4751
		[SerializeField]
		private int defaultIEnumerableWindowCanvasOverrideSorting = 4;
	}
}
