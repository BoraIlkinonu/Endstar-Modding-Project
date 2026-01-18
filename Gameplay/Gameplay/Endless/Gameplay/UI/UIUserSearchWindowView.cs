using System;
using System.Collections.Generic;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Gameplay.UI
{
	// Token: 0x02000420 RID: 1056
	public class UIUserSearchWindowView : UIBaseWindowView
	{
		// Token: 0x1700054E RID: 1358
		// (get) Token: 0x06001A3E RID: 6718 RVA: 0x00078AAE File Offset: 0x00076CAE
		// (set) Token: 0x06001A3F RID: 6719 RVA: 0x00078AB6 File Offset: 0x00076CB6
		public UIUserSearchWindowModel Model { get; set; }

		// Token: 0x06001A40 RID: 6720 RVA: 0x00078AC0 File Offset: 0x00076CC0
		public static UIUserSearchWindowView Display(UIUserSearchWindowModel model, Transform parent = null)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object> { { "model", model } };
			return (UIUserSearchWindowView)MonoBehaviourSingleton<UIWindowManager>.Instance.Display<UIUserSearchWindowView>(parent, dictionary);
		}

		// Token: 0x06001A41 RID: 6721 RVA: 0x00078AF0 File Offset: 0x00076CF0
		public override void Initialize(Dictionary<string, object> supplementalData)
		{
			base.Initialize(supplementalData);
			this.Model = (UIUserSearchWindowModel)supplementalData["Model".ToLower()];
			if (base.VerboseLogging)
			{
				DebugUtility.DebugEnumerable<User>("UsersToHide", this.Model.UsersToHide, this);
			}
			this.titleText.Value = (this.Model.WindowTitle.IsNullOrEmptyOrWhiteSpace() ? this.defaultTitle : this.Model.WindowTitle);
			this.userNameSearchInputField.Select();
			this.userPaginatedGraphQlIEnumerableHandler.SetSelectionType(this.Model.SelectionType);
			this.confirmSelectionButton.interactable = false;
		}

		// Token: 0x06001A42 RID: 6722 RVA: 0x00078B9A File Offset: 0x00076D9A
		public override void Close()
		{
			base.Close();
			this.userNameSearchInputField.Clear(true);
			this.userPaginatedGraphQlIEnumerableHandler.Clear();
		}

		// Token: 0x040014F3 RID: 5363
		[Header("UIUserSearchWindowView")]
		[SerializeField]
		private UIText titleText;

		// Token: 0x040014F4 RID: 5364
		[SerializeField]
		private string defaultTitle = "User Search";

		// Token: 0x040014F5 RID: 5365
		[SerializeField]
		private UIInputField userNameSearchInputField;

		// Token: 0x040014F6 RID: 5366
		[SerializeField]
		private UIUserPaginatedGraphQlIEnumerableHandler userPaginatedGraphQlIEnumerableHandler;

		// Token: 0x040014F7 RID: 5367
		[SerializeField]
		private UIButton confirmSelectionButton;
	}
}
