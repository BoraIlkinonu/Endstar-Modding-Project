using Endless.Creator.LevelEditing.Runtime;
using Endless.Gameplay.LevelEditing.Tilesets;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UITilesetListCellController : UIBaseListCellController<Tileset>
{
	[Header("UITilesetListCellController")]
	[SerializeField]
	private UIButton selectButton;

	[SerializeField]
	private UIGameLibraryAssetAdditionModalView gameLibraryAssetAdditionModalSource;

	private PaintingTool paintingTool;

	protected override void Start()
	{
		base.Start();
		selectButton.onClick.AddListener(HandleSelect);
		paintingTool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<PaintingTool>();
	}

	protected override void OnAddButton()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnAddButton");
		}
		ListCellSizeTypes listCellSizeTypes = ListCellSizeTypes.Cozy;
		UIGameAssetTypes uIGameAssetTypes = UIGameAssetTypes.Terrain;
		MonoBehaviourSingleton<UIModalManager>.Instance.Display(gameLibraryAssetAdditionModalSource, UIModalManagerStackActions.ClearStack, listCellSizeTypes, uIGameAssetTypes);
	}

	private void HandleSelect()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethodWithAppension(this, "HandleSelect", string.Format("{0}: {1}", "Index", base.Model.Index));
		}
		if (((UITilesetListModel)base.ListModel).IsPaintTool)
		{
			paintingTool.SetActiveTilesetIndex(base.Model.Index);
		}
		else
		{
			Select();
		}
	}
}
