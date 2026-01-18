using System;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x02000112 RID: 274
	public class UIGameLibraryFilterListCellController : UIBaseListCellController<UIGameAssetTypes>
	{
		// Token: 0x0600045D RID: 1117 RVA: 0x00019FDB File Offset: 0x000181DB
		protected override void Start()
		{
			base.Start();
			this.button.onClick.AddListener(new UnityAction(this.ToggleSelected));
		}

		// Token: 0x0600045E RID: 1118 RVA: 0x0001A000 File Offset: 0x00018200
		protected override void OnAddButton()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnAddButton", Array.Empty<object>());
			}
			throw new NotImplementedException();
		}

		// Token: 0x04000439 RID: 1081
		[Header("UIGameLibraryFilterListCellController")]
		[SerializeField]
		private UIButton button;
	}
}
