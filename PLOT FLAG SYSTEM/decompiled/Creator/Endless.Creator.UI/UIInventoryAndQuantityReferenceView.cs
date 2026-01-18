using System;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.UI;
using Runtime.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Creator.UI;

public class UIInventoryAndQuantityReferenceView : UIBaseView<TradeInfo.InventoryAndQuantityReference, UIInventoryAndQuantityReferenceView.Styles>, IUIInteractable
{
	public enum Styles
	{
		Default
	}

	[Header("UIInventoryAndQuantityReferenceView")]
	[SerializeField]
	private Image iconImage;

	[SerializeField]
	private Sprite missingIconSprite;

	[SerializeField]
	private GameObject stackableQuantityContainer;

	[SerializeField]
	private UIText[] stackableQuantity = Array.Empty<UIText>();

	[SerializeField]
	private UIButton openWindowButton;

	[field: SerializeField]
	public override Styles Style { get; protected set; }

	public event Action OpenWindow;

	private void Start()
	{
		openWindowButton.onClick.AddListener(InvokeOpenWindow);
	}

	public override void View(TradeInfo.InventoryAndQuantityReference model)
	{
		if ((object)model == null)
		{
			model = new TradeInfo.InventoryAndQuantityReference();
		}
		iconImage.enabled = false;
		if (model.IsReferenceEmpty())
		{
			stackableQuantityContainer.SetActive(value: false);
			return;
		}
		SerializableGuid id = InspectorReferenceUtility.GetId(model);
		if (MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(id, out var metadata))
		{
			iconImage.sprite = (metadata.IsMissingObject ? missingIconSprite : metadata.Icon);
			iconImage.enabled = true;
			stackableQuantityContainer.SetActive(model.Quantity > 1);
			string value = "x" + StringUtility.AbbreviateQuantity(model.Quantity);
			UIText[] array = stackableQuantity;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Value = value;
			}
		}
	}

	public override void Clear()
	{
		iconImage.sprite = null;
		stackableQuantityContainer.SetActive(value: false);
	}

	public void SetInteractable(bool interactable)
	{
		openWindowButton.interactable = interactable;
	}

	private void InvokeOpenWindow()
	{
		this.OpenWindow?.Invoke();
	}
}
