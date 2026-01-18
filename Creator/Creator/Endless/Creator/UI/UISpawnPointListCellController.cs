using System;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x0200017E RID: 382
	public class UISpawnPointListCellController : UIBaseListCellController<UISpawnPoint>
	{
		// Token: 0x1700008D RID: 141
		// (get) Token: 0x0600059D RID: 1437 RVA: 0x0001D8A3 File Offset: 0x0001BAA3
		private UISpawnPointListCellView TypedView
		{
			get
			{
				return (UISpawnPointListCellView)this.View;
			}
		}

		// Token: 0x1700008E RID: 142
		// (get) Token: 0x0600059E RID: 1438 RVA: 0x0001D8B0 File Offset: 0x0001BAB0
		private UISpawnPointListModel TypedListModel
		{
			get
			{
				return (UISpawnPointListModel)base.ListModel;
			}
		}

		// Token: 0x0600059F RID: 1439 RVA: 0x0001D8C0 File Offset: 0x0001BAC0
		protected override void Start()
		{
			base.Start();
			this.toggleSelectedButton.onClick.AddListener(new UnityAction(this.ToggleSelected));
			this.displayExtraEditsButton.onClick.AddListener(new UnityAction(this.TypedView.ToggleExtraEditButtons));
			this.hideExtraEditsButton.onClick.AddListener(new UnityAction(this.TypedView.ToggleExtraEditButtons));
			this.moveUpButton.onClick.AddListener(new UnityAction(this.MoveUp));
			this.moveDownButton.onClick.AddListener(new UnityAction(this.MoveDown));
			this.removeButton.onClick.AddListener(new UnityAction(this.Remove));
		}

		// Token: 0x060005A0 RID: 1440 RVA: 0x0001D987 File Offset: 0x0001BB87
		protected override void OnAddButton()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnAddButton", Array.Empty<object>());
			}
			throw new NotImplementedException();
		}

		// Token: 0x060005A1 RID: 1441 RVA: 0x0001D9A6 File Offset: 0x0001BBA6
		private void MoveUp()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "MoveUp", Array.Empty<object>());
			}
			this.TypedListModel.MoveUp(base.DataIndex, true);
		}

		// Token: 0x060005A2 RID: 1442 RVA: 0x0001D9D2 File Offset: 0x0001BBD2
		private void MoveDown()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "MoveDown", Array.Empty<object>());
			}
			this.TypedListModel.MoveDown(base.DataIndex, true);
		}

		// Token: 0x040004F3 RID: 1267
		[Header("UISpawnPointListCellController")]
		[SerializeField]
		private UIButton toggleSelectedButton;

		// Token: 0x040004F4 RID: 1268
		[SerializeField]
		private UIButton displayExtraEditsButton;

		// Token: 0x040004F5 RID: 1269
		[SerializeField]
		private UIButton hideExtraEditsButton;

		// Token: 0x040004F6 RID: 1270
		[SerializeField]
		private UIButton moveUpButton;

		// Token: 0x040004F7 RID: 1271
		[SerializeField]
		private UIButton moveDownButton;

		// Token: 0x040004F8 RID: 1272
		[SerializeField]
		private UIButton removeButton;
	}
}
