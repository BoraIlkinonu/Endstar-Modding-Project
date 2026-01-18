using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Gameplay.UI;

public class UICharacterCosmeticsDefinitionListCellView : UIBaseListCellView<CharacterCosmeticsDefinition>
{
	[Header("UICharacterCosmeticsDefinitionListCellView")]
	[SerializeField]
	private UICharacterCosmeticsDefinitionPortraitView characterCosmeticsDefinitionPortrait;

	public override void View(UIBaseListView<CharacterCosmeticsDefinition> listView, int dataIndex)
	{
		base.View(listView, dataIndex);
		characterCosmeticsDefinitionPortrait.Display(base.Model.AssetId);
	}
}
