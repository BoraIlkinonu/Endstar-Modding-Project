using System;
using System.Collections.Generic;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.LevelEditing.Tilesets;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020002AC RID: 684
	public class UIPaintingToolPanelView : UIItemSelectionToolPanelView<PaintingTool, Tileset>
	{
		// Token: 0x17000175 RID: 373
		// (get) Token: 0x06000B7A RID: 2938 RVA: 0x00036005 File Offset: 0x00034205
		protected override bool HasSelectedItem
		{
			get
			{
				return this.Tool.ActiveTilesetIndex != PaintingTool.NoSelection;
			}
		}

		// Token: 0x17000176 RID: 374
		// (get) Token: 0x06000B7B RID: 2939 RVA: 0x0003601C File Offset: 0x0003421C
		protected override bool CanViewDetail
		{
			get
			{
				return this.IsMobile;
			}
		}

		// Token: 0x17000177 RID: 375
		// (get) Token: 0x06000B7C RID: 2940 RVA: 0x00036024 File Offset: 0x00034224
		protected override float ListSize
		{
			get
			{
				if (!this.IsMobile)
				{
					return base.ListView.CompleteHeight;
				}
				return -1f;
			}
		}

		// Token: 0x06000B7D RID: 2941 RVA: 0x00036040 File Offset: 0x00034240
		private void OnEnable()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnable", Array.Empty<object>());
			}
			this.Tool.OnActiveTilesetIndexChanged.AddListener(new UnityAction<int>(this.OnActiveTilesetIndexChanged));
			NetworkBehaviourSingleton<CreatorManager>.Instance.OnTerrainRepopulated += this.SetTilesetListModelToDistinctTilesets;
		}

		// Token: 0x06000B7E RID: 2942 RVA: 0x00036097 File Offset: 0x00034297
		protected override void Start()
		{
			base.Start();
			if (!MobileUtility.IsMobile)
			{
				this.displayTween.SetTo(this.standaloneDisplayTo);
				this.hideTween.SetTo(this.standaloneHideTo);
			}
		}

		// Token: 0x06000B7F RID: 2943 RVA: 0x000360C8 File Offset: 0x000342C8
		private void OnDisable()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDisable", Array.Empty<object>());
			}
			this.Tool.OnActiveTilesetIndexChanged.RemoveListener(new UnityAction<int>(this.OnActiveTilesetIndexChanged));
			NetworkBehaviourSingleton<CreatorManager>.Instance.OnTerrainRepopulated -= this.SetTilesetListModelToDistinctTilesets;
		}

		// Token: 0x06000B80 RID: 2944 RVA: 0x0003611F File Offset: 0x0003431F
		public override void Display()
		{
			this.SetTilesetListModelToDistinctTilesets();
			this.OnActiveTilesetIndexChanged(this.Tool.ActiveTilesetIndex);
			base.Display();
		}

		// Token: 0x06000B81 RID: 2945 RVA: 0x00036140 File Offset: 0x00034340
		private void SetTilesetListModelToDistinctTilesets()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetTilesetListModelToDistinctTilesets", Array.Empty<object>());
			}
			List<Tileset> distinctTilesets = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.RuntimePalette.DistinctTilesets;
			this.tilesetListModel.Set(distinctTilesets, true);
			base.TweenToMaxPanelHeight();
		}

		// Token: 0x06000B82 RID: 2946 RVA: 0x00036190 File Offset: 0x00034390
		private void OnActiveTilesetIndexChanged(int activeTilesetIndex)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnActiveTilesetIndexChanged", new object[] { activeTilesetIndex });
			}
			this.tilesetListModel.TriggerModelChanged();
			if (activeTilesetIndex == PaintingTool.NoSelection)
			{
				base.OnItemSelectionEmpty();
				return;
			}
			Tileset tileset = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.TilesetAtIndex(activeTilesetIndex);
			base.ViewSelectedItem(tileset);
		}

		// Token: 0x040009B5 RID: 2485
		[Header("UIPaintingToolPanelView")]
		[SerializeField]
		private UITilesetListModel tilesetListModel;

		// Token: 0x040009B6 RID: 2486
		[SerializeField]
		private TweenAnchoredPosition displayTween;

		// Token: 0x040009B7 RID: 2487
		[SerializeField]
		private TweenAnchoredPosition hideTween;

		// Token: 0x040009B8 RID: 2488
		[SerializeField]
		private Vector2 standaloneDisplayTo = new Vector2(-10f, -90f);

		// Token: 0x040009B9 RID: 2489
		[SerializeField]
		private Vector2 standaloneHideTo = new Vector2(510f, -90f);
	}
}
