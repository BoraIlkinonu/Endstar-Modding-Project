using System.Collections.Generic;
using Endless.Creator.DynamicPropCreation;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIPropCreationMenuModalView : UIBaseModalView
{
	[Header("UIPropCreationMenuModalView")]
	[SerializeField]
	private TextMeshProUGUI displayNameText;

	[SerializeField]
	private UIButton backButton;

	[SerializeField]
	private UIPropCreationDataListModel propCreationDataListModel;

	public override void OnBack()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log("OnBack", this);
		}
		MonoBehaviourSingleton<UIModalManager>.Instance.CloseWithoutClearingStack();
	}

	public override void OnDisplay(params object[] modalData)
	{
		base.OnDisplay(modalData);
		PropCreationMenuData propCreationMenuData = (PropCreationMenuData)modalData[0];
		displayNameText.text = propCreationMenuData.DisplayName;
		backButton.gameObject.SetActive(propCreationMenuData.IsSubMenu);
		List<PropCreationData> list = new List<PropCreationData>(propCreationMenuData.Options);
		propCreationDataListModel.Set(list, triggerEvents: true);
	}
}
