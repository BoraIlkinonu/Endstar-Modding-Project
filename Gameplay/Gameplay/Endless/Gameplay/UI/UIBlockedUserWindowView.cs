using System;
using System.Collections.Generic;
using Endless.Shared;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Gameplay.UI
{
	// Token: 0x02000410 RID: 1040
	public class UIBlockedUserWindowView : UIBaseWindowView
	{
		// Token: 0x1700053D RID: 1341
		// (get) Token: 0x060019E9 RID: 6633 RVA: 0x000770D4 File Offset: 0x000752D4
		// (set) Token: 0x060019EA RID: 6634 RVA: 0x000770DC File Offset: 0x000752DC
		public UIBlockedUserWindowModel Model { get; set; }

		// Token: 0x060019EB RID: 6635 RVA: 0x000770E8 File Offset: 0x000752E8
		public static UIBlockedUserWindowView Display(UIBlockedUserWindowModel model, Transform parent = null)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object> { { "model", model } };
			return (UIBlockedUserWindowView)MonoBehaviourSingleton<UIWindowManager>.Instance.Display<UIBlockedUserWindowView>(parent, dictionary);
		}

		// Token: 0x060019EC RID: 6636 RVA: 0x00077118 File Offset: 0x00075318
		public override void Initialize(Dictionary<string, object> supplementalData)
		{
			base.Initialize(supplementalData);
			this.Model = (UIBlockedUserWindowModel)supplementalData["Model".ToLower()];
			this.blockedUsers.SetModel(this.Model.BlockedUsers, true);
		}

		// Token: 0x060019ED RID: 6637 RVA: 0x00077153 File Offset: 0x00075353
		public override void Close()
		{
			base.Close();
			this.blockedUsers.Clear();
		}

		// Token: 0x0400149C RID: 5276
		[Header("UIBlockedUserWindowView")]
		[SerializeField]
		private UIIEnumerablePresenter blockedUsers;
	}
}
