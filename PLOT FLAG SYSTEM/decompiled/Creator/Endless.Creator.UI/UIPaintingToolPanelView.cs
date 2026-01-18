using System.Collections.Generic;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.LevelEditing.Tilesets;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIPaintingToolPanelView : UIItemSelectionToolPanelView<PaintingTool, Tileset>
{
	[Header("UIPaintingToolPanelView")]
	[SerializeField]
	private UITilesetListModel tilesetListModel;

	[SerializeField]
	private TweenAnchoredPosition displayTween;

	[SerializeField]
	private TweenAnchoredPosition hideTween;

	[SerializeField]
	private Vector2 standaloneDisplayTo = new Vector2(-10f, -90f);

	[SerializeField]
	private Vector2 standaloneHideTo = new Vector2(510f, -90f);

	protected override bool HasSelectedItem => Tool.ActiveTilesetIndex != PaintingTool.NoSelection;

	protected override bool CanViewDetail => IsMobile;

	protected override float ListSize
	{
		get
		{
			if (!IsMobile)
			{
				return base.ListView.CompleteHeight;
			}
			return -1f;
		}
	}

	private void OnEnable()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnEnable");
		}
		Tool.OnActiveTilesetIndexChanged.AddListener(OnActiveTilesetIndexChanged);
		NetworkBehaviourSingleton<CreatorManager>.Instance.OnTerrainRepopulated += SetTilesetListModelToDistinctTilesets;
	}

	protected override void Start()
	{
		base.Start();
		if (!MobileUtility.IsMobile)
		{
			displayTween.SetTo(standaloneDisplayTo);
			hideTween.SetTo(standaloneHideTo);
		}
	}

	private void OnDisable()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDisable");
		}
		Tool.OnActiveTilesetIndexChanged.RemoveListener(OnActiveTilesetIndexChanged);
		NetworkBehaviourSingleton<CreatorManager>.Instance.OnTerrainRepopulated -= SetTilesetListModelToDistinctTilesets;
	}

	public override void Display()
	{
		SetTilesetListModelToDistinctTilesets();
		OnActiveTilesetIndexChanged(Tool.ActiveTilesetIndex);
		base.Display();
	}

	private void SetTilesetListModelToDistinctTilesets()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetTilesetListModelToDistinctTilesets");
		}
		List<Tileset> distinctTilesets = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.RuntimePalette.DistinctTilesets;
		tilesetListModel.Set(distinctTilesets, triggerEvents: true);
		TweenToMaxPanelHeight();
	}

	private void OnActiveTilesetIndexChanged(int activeTilesetIndex)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnActiveTilesetIndexChanged", activeTilesetIndex);
		}
		tilesetListModel.TriggerModelChanged();
		if (activeTilesetIndex == PaintingTool.NoSelection)
		{
			OnItemSelectionEmpty();
			return;
		}
		Tileset itemType = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.TilesetAtIndex(activeTilesetIndex);
		ViewSelectedItem(itemType);
	}
}
