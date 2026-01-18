using Endless.Gameplay;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Creator.UI;

public class UIInventorySpawnOptionsListCellView : UIBaseListCellView<InventorySpawnOption>
{
	[Header("UIInventorySpawnOptionsListCellView")]
	[SerializeField]
	private Image displayIcon;

	[SerializeField]
	private TextMeshProUGUI displayName;

	[SerializeField]
	private CanvasGroup lockCanvasGroup;

	[SerializeField]
	private UIToggle lockToggle;

	[SerializeField]
	private UIToggle quantityToggle;

	[SerializeField]
	private UIStepper quantityStepper;

	[SerializeField]
	private TweenCollection lockDisplayTweens;

	[SerializeField]
	private TweenCollection lockHideTweens;

	private UIInventorySpawnOptionsListModel TypedListModel => (UIInventorySpawnOptionsListModel)base.ListModel;

	public override void OnDespawn()
	{
		base.OnDespawn();
		if (lockDisplayTweens.IsAnyTweening())
		{
			lockDisplayTweens.Cancel();
		}
		if (lockHideTweens.IsAnyTweening())
		{
			lockHideTweens.Cancel();
		}
	}

	public override void View(UIBaseListView<InventorySpawnOption> listView, int dataIndex)
	{
		base.View(listView, dataIndex);
		InventoryUsableDefinition inventoryUsableDefinition = TypedListModel.InventoryDefinitionLookUp[base.Model.AssetId];
		displayIcon.sprite = inventoryUsableDefinition.Sprite;
		displayName.text = inventoryUsableDefinition.DisplayName;
		lockToggle.SetIsOn(base.Model.LockItem, suppressOnChange: true, tweenVisuals: false);
		quantityToggle.gameObject.SetActive(!inventoryUsableDefinition.IsStackable);
		quantityStepper.gameObject.SetActive(inventoryUsableDefinition.IsStackable);
		quantityToggle.SetIsOn(base.Model.Quantity > 0, suppressOnChange: true, tweenVisuals: false);
		quantityStepper.SetValue(base.Model.Quantity, suppressOnChange: true);
		lockCanvasGroup.alpha = ((base.Model.Quantity != 0) ? 1 : 0);
	}
}
