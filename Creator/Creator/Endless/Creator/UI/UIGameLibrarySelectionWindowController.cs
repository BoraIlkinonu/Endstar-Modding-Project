using System;
using System.Collections.Generic;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020002D4 RID: 724
	public class UIGameLibrarySelectionWindowController : UIWindowController
	{
		// Token: 0x06000C40 RID: 3136 RVA: 0x0003A8A2 File Offset: 0x00038AA2
		protected override void Start()
		{
			base.Start();
			this.confirmButton.onClick.AddListener(new UnityAction(this.OnConfirm));
			this.view = this.BaseWindowView as UIGameLibrarySelectionWindowView;
		}

		// Token: 0x06000C41 RID: 3137 RVA: 0x0003A8D8 File Offset: 0x00038AD8
		private void OnConfirm()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnConfirm", Array.Empty<object>());
			}
			IReadOnlyList<UIGameAsset> selectedTypedList = this.gameLibraryListModel.SelectedTypedList;
			if (selectedTypedList.Count > 0)
			{
				this.view.OnSelected(selectedTypedList[0].AssetID);
			}
			this.Close();
		}

		// Token: 0x04000A94 RID: 2708
		[Header("UIGameLibrarySelectionWindowController")]
		[SerializeField]
		private UIGameLibraryListModel gameLibraryListModel;

		// Token: 0x04000A95 RID: 2709
		[SerializeField]
		private UIButton confirmButton;

		// Token: 0x04000A96 RID: 2710
		private UIGameLibrarySelectionWindowView view;
	}
}
