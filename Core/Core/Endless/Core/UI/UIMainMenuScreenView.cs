using System;
using System.Collections.Generic;
using Endless.Core.UI.MainMenu;
using Endless.Core.UI.Settings;
using Endless.Matchmaking;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Core.UI
{
	// Token: 0x0200008C RID: 140
	public class UIMainMenuScreenView : UIBaseScreenView
	{
		// Token: 0x060002C3 RID: 707 RVA: 0x0000F1D4 File Offset: 0x0000D3D4
		protected override void Start()
		{
			base.Start();
			this.scrollRect.verticalNormalizedPosition = 1f;
			SpriteAndEnum[] array = new SpriteAndEnum[]
			{
				new SpriteAndEnum(this.createSprite, UIMainMenuScreenView.CreateAndPlayTabsEnum.Create),
				new SpriteAndEnum(this.playSprite, UIMainMenuScreenView.CreateAndPlayTabsEnum.Play)
			};
			this.createAndPlayTabs.SetOptionsAndValue(array, 0, false);
			MatchmakingClientController.MatchStart += this.SetOpenMatchTabTrue;
			MatchmakingClientController.MatchLeft += this.SetOpenMatchTabFalse;
		}

		// Token: 0x060002C4 RID: 708 RVA: 0x0000F25E File Offset: 0x0000D45E
		private void OnDestroy()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDestroy", Array.Empty<object>());
			}
			MatchmakingClientController.MatchStart -= this.SetOpenMatchTabTrue;
			MatchmakingClientController.MatchLeft -= this.SetOpenMatchTabFalse;
		}

		// Token: 0x060002C5 RID: 709 RVA: 0x0000F29A File Offset: 0x0000D49A
		public static UIMainMenuScreenView Display(UIScreenManager.DisplayStackActions displayStackAction)
		{
			return (UIMainMenuScreenView)MonoBehaviourSingleton<UIScreenManager>.Instance.Display<UIMainMenuScreenView>(displayStackAction, null);
		}

		// Token: 0x060002C6 RID: 710 RVA: 0x0000F2AD File Offset: 0x0000D4AD
		public override void OnBack()
		{
			base.OnBack();
			if (NetworkBehaviourSingleton<GameStateManager>.Instance.SharedGameState > GameState.Default)
			{
				MonoBehaviourSingleton<UIScreenManager>.Instance.Close(UIScreenManager.CloseStackActions.Pop, null, true, false);
			}
		}

		// Token: 0x060002C7 RID: 711 RVA: 0x0000F2D0 File Offset: 0x0000D4D0
		public override void Initialize(Dictionary<string, object> supplementalData)
		{
			base.Initialize(supplementalData);
			MatchmakingClientController.MatchStart += this.OnMatchStarted;
			MatchmakingClientController.MatchLeft += this.HandleReturnToGameButtonVisibility;
			if (this.openMatchTab)
			{
				this.mainMenuTabGroup.SetValue(UIMainMenuTabGroupEnum.Match, true);
				this.openMatchTab = false;
			}
			else if (this.mainMenuTabGroup.PopulatedFromEnum && this.mainMenuTabGroup.EnumValue == UIMainMenuTabGroupEnum.Party)
			{
				bool flag = !MatchmakingClientController.Instance.ActiveGameId.IsEmpty;
				this.mainMenuTabGroup.SetValue(flag ? UIMainMenuTabGroupEnum.Match : UIMainMenuTabGroupEnum.CreatePlay, true);
			}
			this.HandleReturnToGameButtonVisibility();
		}

		// Token: 0x060002C8 RID: 712 RVA: 0x0000F378 File Offset: 0x0000D578
		public override void Close(Action onCloseTweenComplete)
		{
			base.Close(onCloseTweenComplete);
			MatchmakingClientController.MatchStart -= this.OnMatchStarted;
			MatchmakingClientController.MatchLeft -= this.HandleReturnToGameButtonVisibility;
		}

		// Token: 0x060002C9 RID: 713 RVA: 0x0000F3A3 File Offset: 0x0000D5A3
		public void SetPlaySearchField(string gameName)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetPlaySearchField", new object[] { gameName });
			}
			this.playerMainMenuGameModelListController.SetGameNameStringFilter(gameName);
		}

		// Token: 0x060002CA RID: 714 RVA: 0x0000F3CE File Offset: 0x0000D5CE
		private void OnMatchStarted()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnMatchStarted", Array.Empty<object>());
			}
			if (base.IsTweeningClose)
			{
				return;
			}
			this.HandleReturnToGameButtonVisibility();
		}

		// Token: 0x060002CB RID: 715 RVA: 0x0000F3F8 File Offset: 0x0000D5F8
		private void HandleReturnToGameButtonVisibility()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "HandleReturnToGameButtonVisibility", Array.Empty<object>());
			}
			bool flag = false;
			if (MatchmakingClientController.Instance && MatchmakingClientController.Instance.LocalMatch != null)
			{
				flag = true;
			}
			this.returnToGameButton.gameObject.SetActive(flag);
		}

		// Token: 0x060002CC RID: 716 RVA: 0x0000F44A File Offset: 0x0000D64A
		public void ViewPlaySection()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ViewPlaySection", Array.Empty<object>());
			}
			this.createAndPlayTabs.SetValue(UIMainMenuScreenView.CreateAndPlayTabsEnum.Play, true);
		}

		// Token: 0x060002CD RID: 717 RVA: 0x0000F476 File Offset: 0x0000D676
		private void SetOpenMatchTabTrue()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetOpenMatchTabTrue", Array.Empty<object>());
			}
			this.openMatchTab = true;
		}

		// Token: 0x060002CE RID: 718 RVA: 0x0000F497 File Offset: 0x0000D697
		private void SetOpenMatchTabFalse()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetOpenMatchTabFalse", Array.Empty<object>());
			}
			this.openMatchTab = false;
		}

		// Token: 0x04000215 RID: 533
		[SerializeField]
		private UIMainMenuTabGroup mainMenuTabGroup;

		// Token: 0x04000216 RID: 534
		[SerializeField]
		private UISpriteAndEnumTabGroup createAndPlayTabs;

		// Token: 0x04000217 RID: 535
		[SerializeField]
		private Sprite createSprite;

		// Token: 0x04000218 RID: 536
		[SerializeField]
		private Sprite playSprite;

		// Token: 0x04000219 RID: 537
		[SerializeField]
		private UIButton returnToGameButton;

		// Token: 0x0400021A RID: 538
		[SerializeField]
		private UIScrollRect scrollRect;

		// Token: 0x0400021B RID: 539
		[Header("User Group")]
		[SerializeField]
		private string userGroupTabKey = "Party";

		// Token: 0x0400021C RID: 540
		[SerializeField]
		private UISettingsVideoView settingsVideoView;

		// Token: 0x0400021D RID: 541
		[SerializeField]
		private UISettingsVideoController settingsVideoController;

		// Token: 0x0400021E RID: 542
		[SerializeField]
		private UIMainMenuGameModelCloudPaginatedListController playerMainMenuGameModelListController;

		// Token: 0x0400021F RID: 543
		private bool openMatchTab;

		// Token: 0x0200008D RID: 141
		private enum CreateAndPlayTabsEnum
		{
			// Token: 0x04000221 RID: 545
			Create,
			// Token: 0x04000222 RID: 546
			Play
		}
	}
}
