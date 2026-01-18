using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIInventorySpawnOptionsModalView : UIEscapableModalView
{
	[SerializeField]
	private UIInventorySpawnOptionsListModel inventorySpawnOptionsListModel;

	private void OnDisable()
	{
		if (base.VerboseLogging)
		{
			Debug.Log("OnDisable", this);
		}
		inventorySpawnOptionsListModel.Clear(triggerEvents: true);
	}

	public override void OnDisplay(params object[] modalData)
	{
		base.OnDisplay(modalData);
		UIInventorySpawnOptionsPresenter inventorySpawnOptionsPresenter = modalData[0] as UIInventorySpawnOptionsPresenter;
		inventorySpawnOptionsListModel.Initialize(inventorySpawnOptionsPresenter);
	}
}
