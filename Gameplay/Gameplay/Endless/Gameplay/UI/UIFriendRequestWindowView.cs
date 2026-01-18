using System;
using System.Collections.Generic;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.Social;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Gameplay.UI
{
	// Token: 0x02000415 RID: 1045
	public class UIFriendRequestWindowView : UIBaseWindowView
	{
		// Token: 0x1700053F RID: 1343
		// (get) Token: 0x060019F9 RID: 6649 RVA: 0x00077280 File Offset: 0x00075480
		// (set) Token: 0x060019FA RID: 6650 RVA: 0x00077288 File Offset: 0x00075488
		public UIFriendRequestWindowModel Model { get; set; }

		// Token: 0x060019FB RID: 6651 RVA: 0x00077294 File Offset: 0x00075494
		public static UIFriendRequestWindowView Display(UIFriendRequestWindowModel model, Transform parent = null)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object> { { "model", model } };
			return (UIFriendRequestWindowView)MonoBehaviourSingleton<UIWindowManager>.Instance.Display<UIFriendRequestWindowView>(parent, dictionary);
		}

		// Token: 0x060019FC RID: 6652 RVA: 0x000772C4 File Offset: 0x000754C4
		public override void Initialize(Dictionary<string, object> supplementalData)
		{
			base.Initialize(supplementalData);
			this.Model = (UIFriendRequestWindowModel)supplementalData["Model".ToLower()];
			if (!this.tabs.PopulatedFromEnum)
			{
				this.tabs.PopulateFromEnum(UIFriendRequestType.Received, true);
			}
			this.View(this.Model.ReceivedFriendRequests, this.Model.SentFriendRequests);
		}

		// Token: 0x060019FD RID: 6653 RVA: 0x0007732E File Offset: 0x0007552E
		public override void Close()
		{
			base.Close();
			this.sentFriendRequests.Clear();
			this.receivedFriendRequests.Clear();
		}

		// Token: 0x060019FE RID: 6654 RVA: 0x0007734C File Offset: 0x0007554C
		public void View(IReadOnlyList<FriendRequest> receivedFriendRequestsList, IReadOnlyList<FriendRequest> sentFriendRequestsList)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "View", new object[] { receivedFriendRequestsList.Count, sentFriendRequestsList.Count });
			}
			this.receivedFriendRequests.SetModel(receivedFriendRequestsList, false);
			this.sentFriendRequests.SetModel(sentFriendRequestsList, false);
			this.tabs.SetTabBadge(0, this.receivedFriendRequests.Count.ToString());
			this.tabs.SetTabBadge(1, sentFriendRequestsList.Count.ToString());
		}

		// Token: 0x040014A4 RID: 5284
		[Header("UIFriendRequestWindowView")]
		[SerializeField]
		private UISpriteAndEnumTabGroup tabs;

		// Token: 0x040014A5 RID: 5285
		[SerializeField]
		private UIIEnumerablePresenter receivedFriendRequests;

		// Token: 0x040014A6 RID: 5286
		[SerializeField]
		private UIIEnumerablePresenter sentFriendRequests;
	}
}
