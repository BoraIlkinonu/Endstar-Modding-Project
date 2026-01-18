using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIPropEntryListCellView : UIBaseListCellView<PropEntry>
{
	[Header("UIPropEntryListCellView")]
	[SerializeField]
	private UIPropEntryPresenter propEntryPresenter;

	public override void OnDespawn()
	{
		base.OnDespawn();
		propEntryPresenter.Clear();
	}

	public override void View(UIBaseListView<PropEntry> listView, int dataIndex)
	{
		base.View(listView, dataIndex);
		propEntryPresenter.SetModel(base.Model, triggerOnModelChanged: true);
	}
}
