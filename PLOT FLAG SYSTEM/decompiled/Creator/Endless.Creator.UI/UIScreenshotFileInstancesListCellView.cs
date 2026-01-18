using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIScreenshotFileInstancesListCellView : UIBaseListCellView<ScreenshotFileInstances>
{
	[Header("UIScreenshotFileInstancesListCellView")]
	[SerializeField]
	private UIScreenshotView screenshot;

	[SerializeField]
	private UIDragInstanceHandler dragInstanceHandler;

	protected override void Start()
	{
		base.Start();
		dragInstanceHandler.OnInstantiateUnityEvent.AddListener(OnInstantiate);
	}

	public override void OnDespawn()
	{
		base.OnDespawn();
		screenshot.Clear();
	}

	public override void View(UIBaseListView<ScreenshotFileInstances> listView, int dataIndex)
	{
		base.View(listView, dataIndex);
		UIScreenshotFileInstancesListModel uIScreenshotFileInstancesListModel = base.ListModel as UIScreenshotFileInstancesListModel;
		screenshot.SetScreenshotType(uIScreenshotFileInstancesListModel.ScreenshotType);
		screenshot.Display(base.Model);
	}

	private void OnInstantiate(GameObject instantiation)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnInstantiate", instantiation.DebugSafeName());
		}
		if (instantiation.TryGetComponent<UIScreenshotFileInstancesListCellView>(out var component))
		{
			component.View(base.ListView, base.DataIndex);
		}
	}
}
