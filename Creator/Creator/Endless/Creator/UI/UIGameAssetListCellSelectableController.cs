using System;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020000FE RID: 254
	public class UIGameAssetListCellSelectableController : UIGameAssetListCellController
	{
		// Token: 0x06000414 RID: 1044 RVA: 0x000194ED File Offset: 0x000176ED
		protected override void Start()
		{
			base.Start();
			this.select.onClick.AddListener(new UnityAction(this.Select));
		}

		// Token: 0x06000415 RID: 1045 RVA: 0x00019514 File Offset: 0x00017714
		protected override void Select()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} | {1}: {2}, {3}: {4}", new object[] { "Select", "DataIndex", base.DataIndex, "Model", base.Model }), this);
			}
			base.ListModel.Select(base.DataIndex, true);
		}

		// Token: 0x0400041F RID: 1055
		[Header("UIGameAssetListCellSelectableController")]
		[SerializeField]
		private UIButton select;
	}
}
