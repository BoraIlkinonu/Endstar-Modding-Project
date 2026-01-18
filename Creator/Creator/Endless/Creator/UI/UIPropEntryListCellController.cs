using System;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x0200014D RID: 333
	public class UIPropEntryListCellController : UIBaseListCellController<PropEntry>
	{
		// Token: 0x06000515 RID: 1301 RVA: 0x0001C23A File Offset: 0x0001A43A
		protected override void Start()
		{
			base.Start();
			this.selectButton.onClick.AddListener(new UnityAction(this.OnSelect));
		}

		// Token: 0x06000516 RID: 1302 RVA: 0x0001C25E File Offset: 0x0001A45E
		protected override void OnAddButton()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnAddButton", Array.Empty<object>());
			}
			throw new NotImplementedException();
		}

		// Token: 0x06000517 RID: 1303 RVA: 0x0001C27D File Offset: 0x0001A47D
		private void OnSelect()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Select", Array.Empty<object>());
			}
			base.ListModel.ClearSelected(false);
			base.ListModel.Select(base.DataIndex, true);
		}

		// Token: 0x040004A6 RID: 1190
		[Header("UIPropEntryListCellView")]
		[SerializeField]
		private UIButton selectButton;
	}
}
