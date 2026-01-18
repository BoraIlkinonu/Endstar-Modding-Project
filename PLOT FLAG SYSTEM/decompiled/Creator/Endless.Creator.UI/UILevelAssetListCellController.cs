using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.UI;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UILevelAssetListCellController : UIBaseListCellController<LevelAsset>
{
	[Header("UILevelAssetListCellController")]
	[SerializeField]
	private UIButton selectButton;

	[SerializeField]
	private UILevelLoaderModalView levelLoaderModalSource;

	[SerializeField]
	private UINewLevelStateModalView newLevelStateModalSource;

	private UILevelAssetListModel TypedListModel => (UILevelAssetListModel)base.ListModel;

	protected override void Start()
	{
		base.Start();
		selectButton.onClick.AddListener(SelectLevelAsset);
	}

	protected override void OnAddButton()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnAddButton");
		}
		MonoBehaviourSingleton<UIModalManager>.Instance.Display(newLevelStateModalSource, UIModalManagerStackActions.ClearStack, UINewLevelStateModalView.Contexts.Match);
	}

	private void SelectLevelAsset()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SelectLevelAsset");
		}
		UILevelAssetListView uILevelAssetListView = (UILevelAssetListView)base.ListView;
		switch (uILevelAssetListView.SelectAction)
		{
		case UILevelAssetListView.SelectActions.OpenLevelLoaderModal:
			MonoBehaviourSingleton<UIModalManager>.Instance.Display(levelLoaderModalSource, UIModalManagerStackActions.ClearStack, base.Model);
			break;
		case UILevelAssetListView.SelectActions.StartEditMatch:
		{
			MainMenuGameContext mainMenuGameContext = (TypedListModel.OpenLevelInAdminMode ? MainMenuGameContext.Admin : MainMenuGameContext.Edit);
			MonoBehaviourSingleton<UIStartMatchHelper>.Instance.TryToStartMatch(TypedListModel.Game.AssetID, null, base.Model.AssetID, mainMenuGameContext);
			break;
		}
		default:
			DebugUtility.LogNoEnumSupportError(this, uILevelAssetListView.SelectAction);
			break;
		}
	}
}
