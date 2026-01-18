using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Creator.UI;

public class UILevelStateTemplateListCellView : UIBaseListCellView<LevelStateTemplateSourceBase>
{
	[Header("UILevelStateTemplateListCellView")]
	[SerializeField]
	private TextMeshProUGUI nameText;

	[SerializeField]
	private Image spriteImage;

	public override void View(UIBaseListView<LevelStateTemplateSourceBase> listView, int dataIndex)
	{
		base.View(listView, dataIndex);
		Setup();
	}

	private async void Setup()
	{
		TextMeshProUGUI textMeshProUGUI = nameText;
		textMeshProUGUI.text = await base.Model.GetDisplayName();
		Image image = spriteImage;
		image.sprite = await base.Model.GetDisplaySprite();
	}
}
