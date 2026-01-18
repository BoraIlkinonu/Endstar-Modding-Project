using System;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x02000129 RID: 297
	public class UILevelAssetListCellView : UIBaseListCellView<LevelAsset>
	{
		// Token: 0x060004A8 RID: 1192 RVA: 0x0001AE84 File Offset: 0x00019084
		protected override void Start()
		{
			base.Start();
			if (this.dragInstanceHandler)
			{
				this.dragInstanceHandler.OnInstantiateUnityEvent.AddListener(new UnityAction<GameObject>(this.OnInstantiate));
			}
		}

		// Token: 0x060004A9 RID: 1193 RVA: 0x0001AEB5 File Offset: 0x000190B5
		public override void View(UIBaseListView<LevelAsset> listView, int dataIndex)
		{
			base.View(listView, dataIndex);
			if (this.IsAddButton)
			{
				return;
			}
			if (base.Model == null)
			{
				return;
			}
			this.levelAssetView.View(base.Model);
		}

		// Token: 0x060004AA RID: 1194 RVA: 0x0001AEE2 File Offset: 0x000190E2
		public override void OnDespawn()
		{
			base.OnDespawn();
			this.levelAssetView.Clear();
		}

		// Token: 0x060004AB RID: 1195 RVA: 0x0001AEF8 File Offset: 0x000190F8
		private void OnInstantiate(GameObject instantiation)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnInstantiate", new object[] { instantiation.DebugSafeName(true) });
			}
			UILevelAssetListCellView uilevelAssetListCellView;
			if (!instantiation.TryGetComponent<UILevelAssetListCellView>(out uilevelAssetListCellView))
			{
				return;
			}
			uilevelAssetListCellView.View(base.ListView, base.DataIndex);
		}

		// Token: 0x04000464 RID: 1124
		[Header("UILevelAssetListCellView")]
		[SerializeField]
		private UILevelAssetView levelAssetView;

		// Token: 0x04000465 RID: 1125
		[SerializeField]
		private UIDragInstanceHandler dragInstanceHandler;
	}
}
