using System;
using System.Linq;
using Endless.Gameplay;
using Endless.Gameplay.UI;
using Endless.Matchmaking;
using Endless.Networking;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI
{
	// Token: 0x02000096 RID: 150
	[RequireComponent(typeof(UIUserGroupScreenView))]
	public class UIUserGroupScreenController : UIGameObject
	{
		// Token: 0x0600031C RID: 796 RVA: 0x00010A58 File Offset: 0x0000EC58
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.backButton.onClick.AddListener(new UnityAction(this.GoToMainMenu));
			this.openCharacterVisualSelectorWindow.onClick.AddListener(new UnityAction(this.OpenCharacterVisualSelectorWindow));
		}

		// Token: 0x0600031D RID: 797 RVA: 0x00010AB8 File Offset: 0x0000ECB8
		private void InviteToGroup(User user)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "InviteToGroup", new object[] { user });
			}
			MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
			if (MatchmakingClientController.Instance.LocalGroup != null && MatchmakingClientController.Instance.LocalGroup.Members.Any((CoreClientData member) => member.PlatformId == user.Id.ToString()))
			{
				MonoBehaviourSingleton<UIModalManager>.Instance.DisplayGenericModal("Already In Group", null, "It looks like " + user.UserName + " is already in your active Group.", UIModalManagerStackActions.ClearStack, Array.Empty<UIModalGenericViewAction>());
				return;
			}
			MatchmakingClientController.Instance.InviteToGroup(user.Id.ToString(), null);
			MonoBehaviourSingleton<UIModalManager>.Instance.DisplayGenericModal("Invite Sent", null, user.UserName + " has been invited to your group!", UIModalManagerStackActions.ClearStack, Array.Empty<UIModalGenericViewAction>());
		}

		// Token: 0x0600031E RID: 798 RVA: 0x00010BA6 File Offset: 0x0000EDA6
		private void GoToMainMenu()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "GoToMainMenu", Array.Empty<object>());
			}
			UIMainMenuScreenView.Display(UIScreenManager.DisplayStackActions.ClearAndPush);
		}

		// Token: 0x0600031F RID: 799 RVA: 0x00010BC8 File Offset: 0x0000EDC8
		private void OpenCharacterVisualSelectorWindow()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OpenCharacterVisualSelectorWindow", Array.Empty<object>());
			}
			UICharacterCosmeticsDefinitionSelectorWindowView.Display(CharacterCosmeticsDefinitionUtility.GetClientCharacterVisualId(), new Action<SerializableGuid>(CharacterCosmeticsDefinitionUtility.SetClientCharacterVisualId), null).transform.SetParent(this.characterVisualSelectorWindowParent, true);
		}

		// Token: 0x04000249 RID: 585
		[SerializeField]
		private UIButton backButton;

		// Token: 0x0400024A RID: 586
		[SerializeField]
		private UIButton openCharacterVisualSelectorWindow;

		// Token: 0x0400024B RID: 587
		[SerializeField]
		private Transform characterVisualSelectorWindowParent;

		// Token: 0x0400024C RID: 588
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
