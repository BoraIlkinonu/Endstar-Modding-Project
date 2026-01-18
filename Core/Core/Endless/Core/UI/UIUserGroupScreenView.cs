using System;
using System.Collections.Generic;
using Endless.Gameplay.UI;
using Endless.Matchmaking;
using Endless.Networking;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.SoVariables;
using Endless.Shared.UI;
using Runtime.Shared.Matchmaking;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI
{
	// Token: 0x02000098 RID: 152
	public class UIUserGroupScreenView : UIBaseScreenView
	{
		// Token: 0x1700004F RID: 79
		// (get) Token: 0x06000323 RID: 803 RVA: 0x000082D0 File Offset: 0x000064D0
		private GroupInfo UserGroup
		{
			get
			{
				return MatchmakingClientController.Instance.LocalGroup;
			}
		}

		// Token: 0x17000050 RID: 80
		// (get) Token: 0x06000324 RID: 804 RVA: 0x00010C43 File Offset: 0x0000EE43
		private bool IsInUserGroup
		{
			get
			{
				return this.UserGroup != null;
			}
		}

		// Token: 0x17000051 RID: 81
		// (get) Token: 0x06000325 RID: 805 RVA: 0x00010C50 File Offset: 0x0000EE50
		private ClientData LocalClientData
		{
			get
			{
				return MatchmakingClientController.Instance.LocalClientData.Value;
			}
		}

		// Token: 0x17000052 RID: 82
		// (get) Token: 0x06000326 RID: 806 RVA: 0x00010C6F File Offset: 0x0000EE6F
		private bool LocalClientIsGroupHost
		{
			get
			{
				return this.IsInUserGroup && this.UserGroup.Host == this.LocalClientData.CoreData;
			}
		}

		// Token: 0x06000327 RID: 807 RVA: 0x00010C96 File Offset: 0x0000EE96
		public static UIUserGroupScreenView Display(UIScreenManager.DisplayStackActions displayStackAction)
		{
			return (UIUserGroupScreenView)MonoBehaviourSingleton<UIScreenManager>.Instance.Display<UIUserGroupScreenView>(displayStackAction, null);
		}

		// Token: 0x06000328 RID: 808 RVA: 0x00010CAC File Offset: 0x0000EEAC
		public override void Initialize(Dictionary<string, object> supplementalData)
		{
			base.Initialize(supplementalData);
			if (!this.userGroupScreenBackground)
			{
				this.userGroupScreenBackground = MonoBehaviourSingleton<UIScreenBackgroundManager>.Instance.GetScreenBackground<UIUserGroupScreenBackground>();
			}
			this.userGroupScreenBackground.OnPlayerAdded.AddListener(new UnityAction<ClientData, UIUserGroupScreenCharacter>(this.OnPlayerAdded));
			this.userGroupScreenBackground.OnPlayerRemoved.AddListener(new UnityAction<ClientData, UIUserGroupScreenCharacter>(this.OnPlayerRemoved));
			for (int i = 0; i < this.userGroupMaxSize.Value; i++)
			{
				UIInviteToGroupAnchor uiinviteToGroupAnchor = UIInviteToGroupAnchor.CreateInstance(this.inviteToGroupAnchorSource, this.userGroupScreenBackground.CharacterPositions[i].transform, base.RectTransform, null);
				this.inviteButtons.Add(uiinviteToGroupAnchor);
			}
			if (this.IsInUserGroup)
			{
				using (IEnumerator<KeyValuePair<ClientData, UIUserGroupScreenCharacter>> enumerator = this.userGroupScreenBackground.CharacterDictionary.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						KeyValuePair<ClientData, UIUserGroupScreenCharacter> keyValuePair = enumerator.Current;
						this.OnPlayerAdded(keyValuePair.Key, keyValuePair.Value);
					}
					goto IL_0100;
				}
			}
			this.HandleInviteButtonVisibility();
			IL_0100:
			this.UpdateUserGroupTitleText();
		}

		// Token: 0x06000329 RID: 809 RVA: 0x0000D92B File Offset: 0x0000BB2B
		public override void OnBack()
		{
			base.OnBack();
			UIMainMenuScreenView.Display(UIScreenManager.DisplayStackActions.ClearAndPush);
		}

		// Token: 0x0600032A RID: 810 RVA: 0x00010DD0 File Offset: 0x0000EFD0
		public override void Close(Action callOnComplete)
		{
			base.Close(callOnComplete);
			this.userGroupScreenBackground.OnPlayerAdded.RemoveListener(new UnityAction<ClientData, UIUserGroupScreenCharacter>(this.OnPlayerAdded));
			this.userGroupScreenBackground.OnPlayerRemoved.RemoveListener(new UnityAction<ClientData, UIUserGroupScreenCharacter>(this.OnPlayerRemoved));
			foreach (UIInviteToGroupAnchor uiinviteToGroupAnchor in this.inviteButtons)
			{
				MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn<UIInviteToGroupAnchor>(uiinviteToGroupAnchor);
			}
			this.inviteButtons.Clear();
		}

		// Token: 0x0600032B RID: 811 RVA: 0x00010E74 File Offset: 0x0000F074
		private void HandleInviteButtonVisibility()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "HandleInviteButtonVisibility", Array.Empty<object>());
			}
			int num = (this.IsInUserGroup ? (this.userGroupMaxSize.Value - this.UserGroup.Members.Count) : (this.userGroupMaxSize.Value - 1));
			int num2 = this.userGroupMaxSize.Value - num;
			for (int i = 0; i < this.inviteButtons.Count; i++)
			{
				this.inviteButtons[i].gameObject.SetActive(num2 <= i);
			}
		}

		// Token: 0x0600032C RID: 812 RVA: 0x00010F10 File Offset: 0x0000F110
		private void OnPlayerAdded(ClientData target, UIUserGroupScreenCharacter character)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnPlayerAdded", new object[]
				{
					target.ToPrettyString(),
					character.DebugSafeName(true)
				});
			}
			UICorePlayerAnchor uicorePlayerAnchor = UICorePlayerAnchor.CreateInstance(this.corePlayerAnchorSource, character.transform, base.RectTransform, target, null);
			this.characterNameDisplayDictionary.Add(character, uicorePlayerAnchor);
			this.HandleInviteButtonVisibility();
			this.UpdateUserGroupTitleText();
		}

		// Token: 0x0600032D RID: 813 RVA: 0x00010F84 File Offset: 0x0000F184
		private void OnPlayerRemoved(ClientData target, UIUserGroupScreenCharacter character)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnPlayerRemoved", new object[]
				{
					target.ToPrettyString(),
					character.DebugSafeName(true)
				});
			}
			if (!this.characterNameDisplayDictionary.ContainsKey(character))
			{
				return;
			}
			this.characterNameDisplayDictionary[character].Close();
			this.characterNameDisplayDictionary.Remove(character);
			this.HandleInviteButtonVisibility();
			this.UpdateUserGroupTitleText();
		}

		// Token: 0x0600032E RID: 814 RVA: 0x00010FF8 File Offset: 0x0000F1F8
		private void UpdateUserGroupTitleText()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "UpdateUserGroupTitleText", Array.Empty<object>());
			}
			int num = (this.IsInUserGroup ? this.UserGroup.Members.Count : 1);
			this.userGroupTitleText.text = string.Format("Party {0}/{1}", num, this.userGroupMaxSize.Value);
		}

		// Token: 0x0400024E RID: 590
		[Header("UIUserGroupScreenView")]
		[SerializeField]
		private IntVariable userGroupMaxSize;

		// Token: 0x0400024F RID: 591
		[SerializeField]
		private TextMeshProUGUI userGroupTitleText;

		// Token: 0x04000250 RID: 592
		[SerializeField]
		private UIInviteToGroupAnchor inviteToGroupAnchorSource;

		// Token: 0x04000251 RID: 593
		[SerializeField]
		private UICorePlayerAnchor corePlayerAnchorSource;

		// Token: 0x04000252 RID: 594
		private readonly Dictionary<IPoolableT, UICorePlayerAnchor> characterNameDisplayDictionary = new Dictionary<IPoolableT, UICorePlayerAnchor>();

		// Token: 0x04000253 RID: 595
		private readonly List<UIInviteToGroupAnchor> inviteButtons = new List<UIInviteToGroupAnchor>();

		// Token: 0x04000254 RID: 596
		private UIUserGroupScreenBackground userGroupScreenBackground;
	}
}
