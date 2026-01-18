using Endless.Creator.DynamicPropCreation;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIAddPropGameAssetToGameLibraryModalController : UIGameObject
{
	[SerializeField]
	private UIButton searchButton;

	[SerializeField]
	private UIButton createButton;

	[SerializeField]
	private UIGameLibraryAssetAdditionModalView gameAssetSearchModalSource;

	[SerializeField]
	private UIPropCreationMenuModalView propCreationMenuModalSource;

	[SerializeField]
	private PropCreationMenuData dynamicPropCreationMenuData;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		searchButton.onClick.AddListener(Search);
		createButton.onClick.AddListener(Create);
	}

	private void Search()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Search");
		}
		ListCellSizeTypes listCellSizeTypes = ListCellSizeTypes.Cozy;
		UIGameAssetTypes uIGameAssetTypes = UIGameAssetTypes.Prop;
		MonoBehaviourSingleton<UIModalManager>.Instance.Display(gameAssetSearchModalSource, UIModalManagerStackActions.MaintainStack, listCellSizeTypes, uIGameAssetTypes);
	}

	private void Create()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Create");
		}
		MonoBehaviourSingleton<UIModalManager>.Instance.Display(propCreationMenuModalSource, UIModalManagerStackActions.MaintainStack, dynamicPropCreationMenuData);
	}
}
