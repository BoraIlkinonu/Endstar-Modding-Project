using System;
using Endless.Gameplay.Screenshotting;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x02000119 RID: 281
	public class UIInMemoryScreenshotListCellView : UIBaseListCellView<ScreenshotAPI.InMemoryScreenShot>
	{
		// Token: 0x1700006B RID: 107
		// (get) Token: 0x06000475 RID: 1141 RVA: 0x0001A55A File Offset: 0x0001875A
		// (set) Token: 0x06000476 RID: 1142 RVA: 0x0001A562 File Offset: 0x00018762
		public UIScreenshotView Screenshot { get; private set; }

		// Token: 0x06000477 RID: 1143 RVA: 0x0001A56B File Offset: 0x0001876B
		public override void OnDespawn()
		{
			base.OnDespawn();
			this.Screenshot.Clear();
		}

		// Token: 0x06000478 RID: 1144 RVA: 0x0001A580 File Offset: 0x00018780
		public override void View(UIBaseListView<ScreenshotAPI.InMemoryScreenShot> listView, int dataIndex)
		{
			base.View(listView, dataIndex);
			this.Screenshot.Display(base.Model.Original);
			UIInMemoryScreenshotListView uiinMemoryScreenshotListView = (UIInMemoryScreenshotListView)base.ListView;
			this.selectButton.interactable = uiinMemoryScreenshotListView.Selectable;
		}

		// Token: 0x04000448 RID: 1096
		[SerializeField]
		private UIButton selectButton;
	}
}
