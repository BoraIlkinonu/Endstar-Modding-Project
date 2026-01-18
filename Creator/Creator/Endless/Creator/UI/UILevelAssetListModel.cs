using System;
using System.Collections.Generic;
using Endless.Assets;
using Endless.Data;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x0200012C RID: 300
	public class UILevelAssetListModel : UIBaseRearrangeableListModel<LevelAsset>, IUILoadingSpinnerViewCompatible
	{
		// Token: 0x17000074 RID: 116
		// (get) Token: 0x060004B2 RID: 1202 RVA: 0x0001B01F File Offset: 0x0001921F
		// (set) Token: 0x060004B3 RID: 1203 RVA: 0x0001B027 File Offset: 0x00019227
		public bool OpenLevelInAdminMode { get; private set; }

		// Token: 0x17000075 RID: 117
		// (get) Token: 0x060004B4 RID: 1204 RVA: 0x0001B030 File Offset: 0x00019230
		public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

		// Token: 0x17000076 RID: 118
		// (get) Token: 0x060004B5 RID: 1205 RVA: 0x0001B038 File Offset: 0x00019238
		public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

		// Token: 0x17000077 RID: 119
		// (get) Token: 0x060004B6 RID: 1206 RVA: 0x0001B040 File Offset: 0x00019240
		// (set) Token: 0x060004B7 RID: 1207 RVA: 0x0001B048 File Offset: 0x00019248
		public Asset Game { get; private set; }

		// Token: 0x060004B8 RID: 1208 RVA: 0x0001B051 File Offset: 0x00019251
		private void Start()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			base.OnItemMovedUnityEvent.AddListener(new UnityAction<int, int>(this.OnOrderChanged));
		}

		// Token: 0x060004B9 RID: 1209 RVA: 0x0001B082 File Offset: 0x00019282
		private void OnEnable()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnable", Array.Empty<object>());
			}
			UIDragInstanceHandler.InstanceEndDragAction = (Action<RectTransform>)Delegate.Combine(UIDragInstanceHandler.InstanceEndDragAction, new Action<RectTransform>(this.HandleDrop));
		}

		// Token: 0x060004BA RID: 1210 RVA: 0x0001B0BC File Offset: 0x000192BC
		private void OnDisable()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDisable", Array.Empty<object>());
			}
			UIDragInstanceHandler.InstanceEndDragAction = (Action<RectTransform>)Delegate.Remove(UIDragInstanceHandler.InstanceEndDragAction, new Action<RectTransform>(this.HandleDrop));
			this.orderChanges.Clear();
		}

		// Token: 0x060004BB RID: 1211 RVA: 0x0001B10C File Offset: 0x0001930C
		public override void Clear(bool triggerEvents)
		{
			base.Clear(triggerEvents);
			this.OpenLevelInAdminMode = false;
		}

		// Token: 0x060004BC RID: 1212 RVA: 0x0001B11C File Offset: 0x0001931C
		public void SetGame(Asset game)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetGame", new object[] { game.Name });
			}
			this.Game = game;
		}

		// Token: 0x060004BD RID: 1213 RVA: 0x0001B147 File Offset: 0x00019347
		public void SetOpenLevelInAdminMode(bool openLevelInAdminMode)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetOpenLevelInAdminMode", new object[] { openLevelInAdminMode });
			}
			this.OpenLevelInAdminMode = openLevelInAdminMode;
		}

		// Token: 0x060004BE RID: 1214 RVA: 0x0001B174 File Offset: 0x00019374
		private void OnOrderChanged(int oldIndex, int newIndex)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnOrderChanged", new object[] { oldIndex, newIndex });
			}
			Vector2Int vector2Int = new Vector2Int(oldIndex, newIndex);
			Vector2Int vector2Int2 = new Vector2Int(newIndex, oldIndex);
			if (this.orderChanges.Contains(vector2Int2))
			{
				this.orderChanges.Remove(vector2Int2);
				return;
			}
			this.orderChanges.Add(vector2Int);
		}

		// Token: 0x060004BF RID: 1215 RVA: 0x0001B1E8 File Offset: 0x000193E8
		private void HandleDrop(RectTransform rectTransform)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "HandleDrop", new object[] { rectTransform.name });
			}
			UILevelAssetListCellView uilevelAssetListCellView;
			if (!rectTransform.TryGetComponent<UILevelAssetListCellView>(out uilevelAssetListCellView))
			{
				return;
			}
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0}.{1}: {2}", "orderChanges", "Count", this.orderChanges.Count), this);
			}
			if (this.orderChanges.Count == 0)
			{
				return;
			}
			this.orderChanges.Clear();
			this.ApplyLevelOrderChange();
		}

		// Token: 0x060004C0 RID: 1216 RVA: 0x0001B274 File Offset: 0x00019474
		private async void ApplyLevelOrderChange()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ApplyLevelOrderChange", Array.Empty<object>());
			}
			List<LevelReference> list = new List<LevelReference>();
			foreach (LevelAsset levelAsset in base.ReadOnlyList)
			{
				LevelReference levelReference = new LevelReference
				{
					AssetID = levelAsset.AssetID,
					AssetVersion = levelAsset.AssetVersion,
					AssetType = levelAsset.AssetType
				};
				list.Add(levelReference);
			}
			this.OnLoadingStarted.Invoke();
			try
			{
				await MonoBehaviourSingleton<GameEditor>.Instance.ReorderGameLevels(list);
			}
			catch (Exception ex)
			{
				ErrorHandler.HandleError(ErrorCodes.UILevelAssetListModel_ApplyLevelOrderChange, ex, true, false);
			}
			this.OnLoadingEnded.Invoke();
		}

		// Token: 0x04000468 RID: 1128
		private readonly HashSet<Vector2Int> orderChanges = new HashSet<Vector2Int>();
	}
}
