using Endless.Creator.DynamicPropCreation;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Creator;

public class UIPropCreationDataView : UIGameObject
{
	[Header("UIPropCreationDataView")]
	[SerializeField]
	private TextMeshProUGUI displayNameText;

	[SerializeField]
	private Image iconImage;

	[SerializeField]
	private RawImage iconRawImage;

	[SerializeField]
	private Image subMenuIconImage;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	public void View(PropCreationData model)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "View", model);
		}
		displayNameText.text = model.DisplayName;
		if (model is GenericPropCreationScreenData genericPropCreationScreenData)
		{
			iconRawImage.texture = genericPropCreationScreenData.PropIcon;
			iconRawImage.enabled = true;
			iconImage.enabled = false;
		}
		else
		{
			iconImage.sprite = model.Icon;
			iconImage.enabled = true;
			iconRawImage.enabled = false;
		}
		subMenuIconImage.enabled = model.IsSubMenu;
	}
}
