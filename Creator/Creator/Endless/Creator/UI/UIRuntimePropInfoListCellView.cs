using System;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Gameplay.LevelEditing;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x02000164 RID: 356
	public class UIRuntimePropInfoListCellView : UIBaseListCellView<PropLibrary.RuntimePropInfo>
	{
		// Token: 0x0600054F RID: 1359 RVA: 0x0001CB2C File Offset: 0x0001AD2C
		public override void OnDespawn()
		{
			base.OnDespawn();
			this.runtimePropInfoPresenter.Clear();
			this.selectedTweens.Cancel();
			this.unselectedTweens.Cancel();
			this.unselectedTweens.SetToEnd();
			base.HideSelectedVisuals();
			if (!this.propTool)
			{
				return;
			}
			this.propTool.OnSelectedAssetChanged.RemoveListener(new UnityAction<SerializableGuid>(this.OnSelectedAssetChanged));
			this.propTool = null;
		}

		// Token: 0x06000550 RID: 1360 RVA: 0x0001CBA4 File Offset: 0x0001ADA4
		public override void View(UIBaseListView<PropLibrary.RuntimePropInfo> listView, int dataIndex)
		{
			base.View(listView, dataIndex);
			this.runtimePropInfoPresenter.gameObject.SetActive(!this.IsAddButton);
			this.removeButton.gameObject.SetActive(!this.IsAddButton && base.ListModel.UserCanRemove);
			if (this.IsAddButton)
			{
				return;
			}
			if (!this.propTool)
			{
				this.propTool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<PropTool>();
				this.propTool.OnSelectedAssetChanged.AddListener(new UnityAction<SerializableGuid>(this.OnSelectedAssetChanged));
			}
			this.runtimePropInfoPresenter.SetModel(base.Model, true);
			this.VisualizeSelectedState();
		}

		// Token: 0x06000551 RID: 1361 RVA: 0x0001CC52 File Offset: 0x0001AE52
		private void OnSelectedAssetChanged(SerializableGuid assetId)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnSelectedAssetChanged", new object[] { assetId });
			}
			this.VisualizeSelectedState();
		}

		// Token: 0x06000552 RID: 1362 RVA: 0x0001CC7C File Offset: 0x0001AE7C
		private void VisualizeSelectedState()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "VisualizeSelectedState", Array.Empty<object>());
			}
			if (this.IsAddButton)
			{
				return;
			}
			UIRuntimePropInfoListModel.Contexts context = ((UIRuntimePropInfoListModel)base.ListModel).Context;
			bool flag;
			if (context != UIRuntimePropInfoListModel.Contexts.PropTool)
			{
				if (context != UIRuntimePropInfoListModel.Contexts.InventoryLibraryReference)
				{
					throw new ArgumentOutOfRangeException();
				}
				flag = base.ListModel.IsSelected(base.DataIndex);
			}
			else
			{
				flag = base.Model.PropData.AssetID == this.propTool.SelectedAssetId;
			}
			if (flag)
			{
				this.selectedTweens.Tween();
				return;
			}
			this.unselectedTweens.Tween();
		}

		// Token: 0x040004CA RID: 1226
		[Header("UIRuntimePropInfoListCellView")]
		[SerializeField]
		private UIRuntimePropInfoPresenter runtimePropInfoPresenter;

		// Token: 0x040004CB RID: 1227
		[SerializeField]
		private UIButton removeButton;

		// Token: 0x040004CC RID: 1228
		[SerializeField]
		private TweenCollection selectedTweens;

		// Token: 0x040004CD RID: 1229
		[SerializeField]
		private TweenCollection unselectedTweens;

		// Token: 0x040004CE RID: 1230
		private PropTool propTool;
	}
}
