using System;
using Endless.Creator.DynamicPropCreation;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIPropCreationDataListCellController : UIBaseListCellController<PropCreationData>
{
	[Header("UIPropCreationDataListCellController")]
	[SerializeField]
	private UIButton selectButton;

	[SerializeField]
	private UIPropCreationPromptDataModalView propCreationPromptDataModalSource;

	[SerializeField]
	private UIPropCreationMenuModalView propCreationMenuModalSource;

	[SerializeField]
	private UIAbstractPropCreationModalView abstractPropCreationModalSource;

	[SerializeField]
	private UIGenericPropCreationModalView genericPropCreationModalSource;

	protected override void Start()
	{
		base.Start();
		selectButton.onClick.AddListener(SelectPropCreation);
	}

	protected override void OnAddButton()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnAddButton");
		}
		throw new NotImplementedException();
	}

	private void SelectPropCreation()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SelectPropCreation");
		}
		if (base.Model.IsSubMenu)
		{
			PropCreationMenuData propCreationMenuData = (PropCreationMenuData)base.Model;
			MonoBehaviourSingleton<UIModalManager>.Instance.Display(propCreationMenuModalSource, UIModalManagerStackActions.MaintainStack, propCreationMenuData);
		}
		else if (base.Model is PropCreationPromptData propCreationPromptData)
		{
			MonoBehaviourSingleton<UIModalManager>.Instance.Display(propCreationPromptDataModalSource, UIModalManagerStackActions.MaintainStack, propCreationPromptData);
		}
		else if (base.Model is AbstractPropCreationScreenData abstractPropCreationScreenData)
		{
			MonoBehaviourSingleton<UIModalManager>.Instance.Display(abstractPropCreationModalSource, UIModalManagerStackActions.MaintainStack, abstractPropCreationScreenData);
		}
		else if (base.Model is GenericPropCreationScreenData genericPropCreationScreenData)
		{
			MonoBehaviourSingleton<UIModalManager>.Instance.Display(genericPropCreationModalSource, UIModalManagerStackActions.MaintainStack, genericPropCreationScreenData);
		}
		else
		{
			Debug.LogError(base.Model.GetType());
		}
	}
}
