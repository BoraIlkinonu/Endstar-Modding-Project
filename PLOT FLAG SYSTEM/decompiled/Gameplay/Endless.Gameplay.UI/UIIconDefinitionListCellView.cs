using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Gameplay.UI;

public class UIIconDefinitionListCellView : UIBaseListCellView<IconDefinition>
{
	[Header("UIIconDefinitionListCellView")]
	[SerializeField]
	private UIIconDefinitionView icon;

	public override void View(UIBaseListView<IconDefinition> listView, int dataIndex)
	{
		base.View(listView, dataIndex);
		icon.View(base.Model);
	}
}
