using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Endless.Core.UI.Settings;
using Endless.Matchmaking;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Endless.Shared.Validation;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI.MainMenu
{
	// Token: 0x020000A4 RID: 164
	public class UIMainMenuTabGroup : UIBookendSpriteAndEnumTabGroup, IValidatable
	{
		// Token: 0x1700005C RID: 92
		// (get) Token: 0x06000382 RID: 898 RVA: 0x00012690 File Offset: 0x00010890
		public UIMainMenuTabGroupEnum EnumValue
		{
			get
			{
				return (UIMainMenuTabGroupEnum)base.Value.Enum;
			}
		}

		// Token: 0x1700005D RID: 93
		// (get) Token: 0x06000383 RID: 899 RVA: 0x000126A2 File Offset: 0x000108A2
		public UIMainMenuTabGroupEnum PreviousValueEnum
		{
			get
			{
				return (UIMainMenuTabGroupEnum)base.PreviousValue.Enum;
			}
		}

		// Token: 0x06000384 RID: 900 RVA: 0x000126B4 File Offset: 0x000108B4
		public void Validate()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Validate", Array.Empty<object>());
			}
			if (this.sprites.Length != Enum.GetValues(typeof(UIMainMenuTabGroupEnum)).Length)
			{
				Debug.LogError("sprites must have an item for each value in UIMainMenuTabGroupEnum", this);
			}
		}

		// Token: 0x06000385 RID: 901 RVA: 0x00012704 File Offset: 0x00010904
		protected override void Start()
		{
			base.Start();
			base.PopulateFromEnumWithSprites(UIMainMenuTabGroupEnum.CreatePlay, this.sprites, true);
			this.UpdateHiddenTabs();
			MatchmakingClientController.MatchStart += this.UpdateHiddenTabs;
			MatchmakingClientController.MatchLeft += this.UpdateHiddenTabs;
			base.OnValueChangedWithIndex.AddListener(new UnityAction<int>(this.OnTabChanged));
		}

		// Token: 0x06000386 RID: 902 RVA: 0x0001276C File Offset: 0x0001096C
		private void OnDestroy()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDestroy", Array.Empty<object>());
			}
			CancellationTokenSourceUtility.CancelAndCleanup(ref this.adminRequestCancellationTokenSource);
			MatchmakingClientController.MatchStart -= this.UpdateHiddenTabs;
			MatchmakingClientController.MatchLeft -= this.UpdateHiddenTabs;
			base.OnValueChangedWithIndex.RemoveListener(new UnityAction<int>(this.OnTabChanged));
		}

		// Token: 0x06000387 RID: 903 RVA: 0x000127D8 File Offset: 0x000109D8
		private void UpdateHiddenTabs()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "UpdateHiddenTabs", Array.Empty<object>());
			}
			List<SpriteAndEnum> hiddenOptions = this.GetHiddenOptions();
			base.SetHiddenOptions(hiddenOptions);
		}

		// Token: 0x06000388 RID: 904 RVA: 0x0001280C File Offset: 0x00010A0C
		private List<SpriteAndEnum> GetHiddenOptions()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "GetHiddenOptions", Array.Empty<object>());
			}
			List<SpriteAndEnum> list = new List<SpriteAndEnum>();
			if (MatchmakingClientController.Instance.ActiveGameId.IsEmpty)
			{
				int num = this.IndexOf(UIMainMenuTabGroupEnum.Match);
				if (num != -1)
				{
					SpriteAndEnum option = base.GetOption(num);
					list.Add(option);
				}
			}
			if (!EndlessCloudService.IsAdmin && !EndlessCloudService.IsModerator)
			{
				int num2 = this.IndexOf(UIMainMenuTabGroupEnum.Admin);
				if (num2 != -1)
				{
					SpriteAndEnum option2 = base.GetOption(num2);
					list.Add(option2);
				}
			}
			return list;
		}

		// Token: 0x06000389 RID: 905 RVA: 0x00012898 File Offset: 0x00010A98
		private int IndexOf(UIMainMenuTabGroupEnum enumValue)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "IndexOf", new object[] { enumValue });
			}
			for (int i = 0; i < base.OptionsLength; i++)
			{
				if ((UIMainMenuTabGroupEnum)base.GetOption(i).Enum == enumValue)
				{
					return i;
				}
			}
			return -1;
		}

		// Token: 0x0600038A RID: 906 RVA: 0x000128F0 File Offset: 0x00010AF0
		public override void SetValue(int valueIndex, bool triggerOnValueChanged)
		{
			UIMainMenuTabGroup.<>c__DisplayClass18_0 CS$<>8__locals1 = new UIMainMenuTabGroup.<>c__DisplayClass18_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.valueIndex = valueIndex;
			CS$<>8__locals1.triggerOnValueChanged = triggerOnValueChanged;
			UIMainMenuTabGroupEnum valueIndex2 = (UIMainMenuTabGroupEnum)CS$<>8__locals1.valueIndex;
			if (valueIndex2 == UIMainMenuTabGroupEnum.ConfirmLeave)
			{
				this.ConfirmLeave();
				return;
			}
			if (valueIndex2 != UIMainMenuTabGroupEnum.Options && this.EnumValue == UIMainMenuTabGroupEnum.Options && this.settingsVideoView.HasChanges)
			{
				MonoBehaviourSingleton<UIModalManager>.Instance.Display(this.settingsConfirmationModalSource, UIModalManagerStackActions.ClearStack, new object[]
				{
					this.settingsVideoController,
					new Action(CS$<>8__locals1.<SetValue>g__ReapplyValue|0)
				});
				base.SetValue(UIMainMenuTabGroupEnum.Options, true);
				return;
			}
			base.SetValue(CS$<>8__locals1.valueIndex, CS$<>8__locals1.triggerOnValueChanged);
		}

		// Token: 0x0600038B RID: 907 RVA: 0x00012994 File Offset: 0x00010B94
		private void OnTabChanged(int index)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnTabChanged", new object[] { index });
			}
			switch (this.EnumValue)
			{
			case UIMainMenuTabGroupEnum.CreatePlay:
			case UIMainMenuTabGroupEnum.Match:
			case UIMainMenuTabGroupEnum.Admin:
			case UIMainMenuTabGroupEnum.Options:
				return;
			case UIMainMenuTabGroupEnum.Party:
				UIUserGroupScreenView.Display(UIScreenManager.DisplayStackActions.Push);
				return;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		// Token: 0x0600038C RID: 908 RVA: 0x000129F4 File Offset: 0x00010BF4
		private void ConfirmLeave()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ConfirmLeave", Array.Empty<object>());
			}
			UIModalGenericViewAction uimodalGenericViewAction = new UIModalGenericViewAction(this.quitButtonColor, "QUIT", new Action(Application.Quit));
			UIModalGenericViewAction uimodalGenericViewAction2 = new UIModalGenericViewAction(this.logOutButtonColor, "LOG-OUT", delegate
			{
				UIModalManager.OnModalClosedByUser = (Action<UIBaseModalView>)Delegate.Remove(UIModalManager.OnModalClosedByUser, new Action<UIBaseModalView>(this.<ConfirmLeave>g__HandleModalClosed|20_2));
				this.Logout();
			});
			UIModalGenericViewAction uimodalGenericViewAction3 = new UIModalGenericViewAction(this.cancelButtonColor, "DONT QUIT", delegate
			{
				UIModalManager.OnModalClosedByUser = (Action<UIBaseModalView>)Delegate.Remove(UIModalManager.OnModalClosedByUser, new Action<UIBaseModalView>(this.<ConfirmLeave>g__HandleModalClosed|20_2));
				MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
				base.SetValue((this.PreviousValueEnum == UIMainMenuTabGroupEnum.ConfirmLeave) ? UIMainMenuTabGroupEnum.CreatePlay : this.PreviousValueEnum, true);
			});
			UIModalGenericViewAction[] array = new UIModalGenericViewAction[] { uimodalGenericViewAction, uimodalGenericViewAction2, uimodalGenericViewAction3 };
			UIModalManager.OnModalClosedByUser = (Action<UIBaseModalView>)Delegate.Combine(UIModalManager.OnModalClosedByUser, new Action<UIBaseModalView>(this.<ConfirmLeave>g__HandleModalClosed|20_2));
			MonoBehaviourSingleton<UIModalManager>.Instance.DisplayGenericModal("LEAVING?", null, string.Empty, UIModalManagerStackActions.ClearStack, array);
		}

		// Token: 0x0600038D RID: 909 RVA: 0x00012AC9 File Offset: 0x00010CC9
		private void Logout()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Logout", Array.Empty<object>());
			}
			MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
			EndlessCloudService.ClearCachedToken();
			MatchmakingClientController.Instance.Disconnect();
			this.UpdateHiddenTabs();
		}

		// Token: 0x06000391 RID: 913 RVA: 0x00012BB5 File Offset: 0x00010DB5
		[CompilerGenerated]
		private void <ConfirmLeave>g__HandleModalClosed|20_2(UIBaseModalView obj)
		{
			base.SetValue((this.PreviousValueEnum == UIMainMenuTabGroupEnum.ConfirmLeave) ? UIMainMenuTabGroupEnum.CreatePlay : this.PreviousValueEnum, true);
		}

		// Token: 0x0400028D RID: 653
		[Header("UIMainMenuTabGroup")]
		[SerializeField]
		private Sprite[] sprites = Array.Empty<Sprite>();

		// Token: 0x0400028E RID: 654
		[Header("Settings")]
		[SerializeField]
		private UISettingsVideoView settingsVideoView;

		// Token: 0x0400028F RID: 655
		[SerializeField]
		private UISettingsVideoController settingsVideoController;

		// Token: 0x04000290 RID: 656
		[SerializeField]
		private UISettingsConfirmationModalView settingsConfirmationModalSource;

		// Token: 0x04000291 RID: 657
		[Header("Quitting")]
		[SerializeField]
		private Color quitButtonColor = Color.red;

		// Token: 0x04000292 RID: 658
		[SerializeField]
		private Color logOutButtonColor = Color.yellow;

		// Token: 0x04000293 RID: 659
		[SerializeField]
		private Color cancelButtonColor = Color.white;

		// Token: 0x04000294 RID: 660
		private CancellationTokenSource adminRequestCancellationTokenSource;
	}
}
