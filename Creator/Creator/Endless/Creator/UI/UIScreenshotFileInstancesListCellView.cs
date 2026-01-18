using System;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x0200016B RID: 363
	public class UIScreenshotFileInstancesListCellView : UIBaseListCellView<ScreenshotFileInstances>
	{
		// Token: 0x06000565 RID: 1381 RVA: 0x0001CFCA File Offset: 0x0001B1CA
		protected override void Start()
		{
			base.Start();
			this.dragInstanceHandler.OnInstantiateUnityEvent.AddListener(new UnityAction<GameObject>(this.OnInstantiate));
		}

		// Token: 0x06000566 RID: 1382 RVA: 0x0001CFEE File Offset: 0x0001B1EE
		public override void OnDespawn()
		{
			base.OnDespawn();
			this.screenshot.Clear();
		}

		// Token: 0x06000567 RID: 1383 RVA: 0x0001D004 File Offset: 0x0001B204
		public override void View(UIBaseListView<ScreenshotFileInstances> listView, int dataIndex)
		{
			base.View(listView, dataIndex);
			UIScreenshotFileInstancesListModel uiscreenshotFileInstancesListModel = base.ListModel as UIScreenshotFileInstancesListModel;
			this.screenshot.SetScreenshotType(uiscreenshotFileInstancesListModel.ScreenshotType);
			this.screenshot.Display(base.Model);
		}

		// Token: 0x06000568 RID: 1384 RVA: 0x0001D048 File Offset: 0x0001B248
		private void OnInstantiate(GameObject instantiation)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnInstantiate", new object[] { instantiation.DebugSafeName(true) });
			}
			UIScreenshotFileInstancesListCellView uiscreenshotFileInstancesListCellView;
			if (!instantiation.TryGetComponent<UIScreenshotFileInstancesListCellView>(out uiscreenshotFileInstancesListCellView))
			{
				return;
			}
			uiscreenshotFileInstancesListCellView.View(base.ListView, base.DataIndex);
		}

		// Token: 0x040004D7 RID: 1239
		[Header("UIScreenshotFileInstancesListCellView")]
		[SerializeField]
		private UIScreenshotView screenshot;

		// Token: 0x040004D8 RID: 1240
		[SerializeField]
		private UIDragInstanceHandler dragInstanceHandler;
	}
}
