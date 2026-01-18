using System;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x02000172 RID: 370
	public class UIScreenshotFileInstancesGameAdditionListCellView : UIBaseListCellView<ScreenshotFileInstances>
	{
		// Token: 0x0600057F RID: 1407 RVA: 0x0001D4A0 File Offset: 0x0001B6A0
		public override void OnDespawn()
		{
			base.OnDespawn();
			if (!this.initialized)
			{
				return;
			}
			this.screenshot.Clear();
			UIAddScreenshotsToGameModalModel.ScreenshotsToAddChangedAction = (Action)Delegate.Remove(UIAddScreenshotsToGameModalModel.ScreenshotsToAddChangedAction, new Action(this.DisplaySelected));
			this.initialized = false;
		}

		// Token: 0x06000580 RID: 1408 RVA: 0x0001D4F0 File Offset: 0x0001B6F0
		public override void View(UIBaseListView<ScreenshotFileInstances> listView, int dataIndex)
		{
			base.View(listView, dataIndex);
			if (!this.initialized)
			{
				UIAddScreenshotsToGameModalModel.ScreenshotsToAddChangedAction = (Action)Delegate.Combine(UIAddScreenshotsToGameModalModel.ScreenshotsToAddChangedAction, new Action(this.DisplaySelected));
				this.initialized = true;
			}
			this.screenshot.Display(base.Model);
			this.DisplaySelected();
		}

		// Token: 0x06000581 RID: 1409 RVA: 0x0001D54C File Offset: 0x0001B74C
		private void DisplaySelected()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "DisplaySelected", Array.Empty<object>());
			}
			UIScreenshotFileInstancesListModel uiscreenshotFileInstancesListModel = (UIScreenshotFileInstancesListModel)base.ListModel;
			ScreenshotFileInstances screenshotFileInstances = uiscreenshotFileInstancesListModel[base.DataIndex];
			bool flag = false;
			UIAddScreenshotsToGameModalModel activeAddLevelScreenshotsToGameModalModel = this.GetActiveAddLevelScreenshotsToGameModalModel();
			if (activeAddLevelScreenshotsToGameModalModel)
			{
				flag = activeAddLevelScreenshotsToGameModalModel.ScreenshotsToAdd.Contains(screenshotFileInstances);
			}
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0}: {1}", "isSelected", flag), this);
			}
			if (flag)
			{
				int num = activeAddLevelScreenshotsToGameModalModel.ScreenshotsToAdd.IndexOf(screenshotFileInstances) + uiscreenshotFileInstancesListModel.ExteriorSelectedCount;
				base.ViewSelected(num);
			}
			bool flag2 = uiscreenshotFileInstancesListModel.GetExteriorSelectedValue(screenshotFileInstances) > -1;
			this.button.interactable = !flag2;
		}

		// Token: 0x06000582 RID: 1410 RVA: 0x0001D608 File Offset: 0x0001B808
		private UIAddScreenshotsToGameModalModel GetActiveAddLevelScreenshotsToGameModalModel()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "GetActiveAddLevelScreenshotsToGameModalModel", Array.Empty<object>());
			}
			if (!MonoBehaviourSingleton<UIModalManager>.Instance.ModalIsDisplaying)
			{
				return null;
			}
			if (MonoBehaviourSingleton<UIModalManager>.Instance.SpawnedModal.GetType() != typeof(UIAddScreenshotsToGameModalView))
			{
				return null;
			}
			return ((UIAddScreenshotsToGameModalView)MonoBehaviourSingleton<UIModalManager>.Instance.SpawnedModal).Model;
		}

		// Token: 0x040004E2 RID: 1250
		[Header("UIScreenshotFileInstancesGameAdditionListCellView")]
		[SerializeField]
		private UIScreenshotView screenshot;

		// Token: 0x040004E3 RID: 1251
		[SerializeField]
		private UIButton button;

		// Token: 0x040004E4 RID: 1252
		private bool initialized;
	}
}
