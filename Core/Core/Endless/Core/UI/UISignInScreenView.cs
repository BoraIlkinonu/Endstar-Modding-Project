using System;
using System.Collections.Generic;
using Endless.Matchmaking;
using Endless.Shared;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI
{
	// Token: 0x0200009B RID: 155
	public class UISignInScreenView : UIBaseScreenView
	{
		// Token: 0x17000055 RID: 85
		// (get) Token: 0x06000341 RID: 833 RVA: 0x000116AA File Offset: 0x0000F8AA
		// (set) Token: 0x06000342 RID: 834 RVA: 0x000116B2 File Offset: 0x0000F8B2
		public UnityEvent InitializedUnityEvent { get; private set; } = new UnityEvent();

		// Token: 0x06000343 RID: 835 RVA: 0x000116BB File Offset: 0x0000F8BB
		public static UISignInScreenView Display(UIScreenManager.DisplayStackActions displayStackAction)
		{
			if (ExitManager.IsQuitting)
			{
				return null;
			}
			return (UISignInScreenView)MonoBehaviourSingleton<UIScreenManager>.Instance.Display<UISignInScreenView>(displayStackAction, null);
		}

		// Token: 0x06000344 RID: 836 RVA: 0x000116D7 File Offset: 0x0000F8D7
		public override void OnDespawn()
		{
			base.OnDespawn();
			this.usernameInputField.Clear(true);
			this.passwordInputField.Clear(true);
			this.rememberMeToggle.SetIsOn(false, true, false);
		}

		// Token: 0x06000345 RID: 837 RVA: 0x00011705 File Offset: 0x0000F905
		public override void Initialize(Dictionary<string, object> supplementalData)
		{
			base.Initialize(supplementalData);
			this.usernameInputField.text = EndlessCloudService.GetCachedUserName();
			this.rememberMeToggle.SetIsOn(!this.usernameInputField.text.IsNullOrEmptyOrWhiteSpace(), true, false);
		}

		// Token: 0x04000262 RID: 610
		[Header("UISignInScreenView")]
		[SerializeField]
		private UIInputField usernameInputField;

		// Token: 0x04000263 RID: 611
		[SerializeField]
		private UIInputField passwordInputField;

		// Token: 0x04000264 RID: 612
		[SerializeField]
		private UIToggle rememberMeToggle;
	}
}
