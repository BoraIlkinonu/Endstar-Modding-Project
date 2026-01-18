using System;
using System.Collections.Generic;
using Endless.Data;
using Endless.Gameplay.UI;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Runtime.Shared.Matchmaking;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x02000038 RID: 56
	public class UIBlockedUserWindowController : UIWindowController
	{
		// Token: 0x1700002E RID: 46
		// (get) Token: 0x060000F9 RID: 249 RVA: 0x000056E1 File Offset: 0x000038E1
		private UIBlockedUserWindowModel Model
		{
			get
			{
				return this.view.Model;
			}
		}

		// Token: 0x060000FA RID: 250 RVA: 0x000056EE File Offset: 0x000038EE
		protected override void Start()
		{
			base.Start();
			this.addButton.onClick.AddListener(new UnityAction(this.OpenAddUserSelectionWindow));
		}

		// Token: 0x060000FB RID: 251 RVA: 0x00005714 File Offset: 0x00003914
		private void OpenAddUserSelectionWindow()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OpenAddUserSelectionWindow", Array.Empty<object>());
			}
			UIIEnumerableWindowView.Display(new UIIEnumerableWindowModel(this.canvas.sortingOrder + 1, "Friends", UIBaseIEnumerableView.ArrangementStyle.StraightVerticalVirtualized, new SelectionType?(SelectionType.Select0OrMore), this.Model.Friends, null, new List<object>(), null, new Action<List<object>>(this.BlockUsers)), null);
		}

		// Token: 0x060000FC RID: 252 RVA: 0x0000577C File Offset: 0x0000397C
		private void BlockUsers(List<object> users)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "BlockUsers", new object[] { users.Count });
			}
			List<object> list = new List<object>(this.blockedUsers.ModelList);
			foreach (object obj in users)
			{
				User user = obj as User;
				list.Add(user);
				EndlessServices.Instance.CloudService.BlockUserAsync(user.Id, base.VerboseLogging);
				EndlessServices.Instance.CloudService.UnfriendAsync(user.Id, base.VerboseLogging);
			}
			this.blockedUsers.SetModel(list, true);
		}

		// Token: 0x040000A6 RID: 166
		[Header("UIBlockedUserWindowController")]
		[SerializeField]
		private UIBlockedUserWindowView view;

		// Token: 0x040000A7 RID: 167
		[SerializeField]
		private EndlessStudiosUserId endlessStudiosUserId;

		// Token: 0x040000A8 RID: 168
		[SerializeField]
		private Canvas canvas;

		// Token: 0x040000A9 RID: 169
		[SerializeField]
		private UIIEnumerablePresenter blockedUsers;

		// Token: 0x040000AA RID: 170
		[SerializeField]
		private UIButton addButton;
	}
}
