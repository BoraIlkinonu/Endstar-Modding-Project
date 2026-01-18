using System;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIScreenshotFileInstancesGameAdditionListCellView : UIBaseListCellView<ScreenshotFileInstances>
{
	[Header("UIScreenshotFileInstancesGameAdditionListCellView")]
	[SerializeField]
	private UIScreenshotView screenshot;

	[SerializeField]
	private UIButton button;

	private bool initialized;

	public override void OnDespawn()
	{
		base.OnDespawn();
		if (initialized)
		{
			screenshot.Clear();
			UIAddScreenshotsToGameModalModel.ScreenshotsToAddChangedAction = (Action)Delegate.Remove(UIAddScreenshotsToGameModalModel.ScreenshotsToAddChangedAction, new Action(DisplaySelected));
			initialized = false;
		}
	}

	public override void View(UIBaseListView<ScreenshotFileInstances> listView, int dataIndex)
	{
		base.View(listView, dataIndex);
		if (!initialized)
		{
			UIAddScreenshotsToGameModalModel.ScreenshotsToAddChangedAction = (Action)Delegate.Combine(UIAddScreenshotsToGameModalModel.ScreenshotsToAddChangedAction, new Action(DisplaySelected));
			initialized = true;
		}
		screenshot.Display(base.Model);
		DisplaySelected();
	}

	private void DisplaySelected()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "DisplaySelected");
		}
		UIScreenshotFileInstancesListModel uIScreenshotFileInstancesListModel = (UIScreenshotFileInstancesListModel)base.ListModel;
		ScreenshotFileInstances screenshotFileInstances = uIScreenshotFileInstancesListModel[base.DataIndex];
		bool flag = false;
		UIAddScreenshotsToGameModalModel activeAddLevelScreenshotsToGameModalModel = GetActiveAddLevelScreenshotsToGameModalModel();
		if ((bool)activeAddLevelScreenshotsToGameModalModel)
		{
			flag = activeAddLevelScreenshotsToGameModalModel.ScreenshotsToAdd.Contains(screenshotFileInstances);
		}
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0}: {1}", "isSelected", flag), this);
		}
		if (flag)
		{
			int selectedOrder = activeAddLevelScreenshotsToGameModalModel.ScreenshotsToAdd.IndexOf(screenshotFileInstances) + uIScreenshotFileInstancesListModel.ExteriorSelectedCount;
			ViewSelected(selectedOrder);
		}
		bool flag2 = uIScreenshotFileInstancesListModel.GetExteriorSelectedValue(screenshotFileInstances) > -1;
		button.interactable = !flag2;
	}

	private UIAddScreenshotsToGameModalModel GetActiveAddLevelScreenshotsToGameModalModel()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "GetActiveAddLevelScreenshotsToGameModalModel");
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
}
